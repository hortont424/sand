/*
 * Copyright 2011 Tim Horton and Pete Schirmer. All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without modification,
 * are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice,
 * this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 * this list of conditions and the following disclaimer in the documentation
 * and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY TIM HORTON "AS IS" AND ANY EXPRESS OR IMPLIED
 * WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT
 * SHALL TIM HORTON OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
 * LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

#include "peer.h"

#include <unistd.h>
#include <network/util.h>
#include <sys/socket.h>
#include <google/protobuf/io/coded_stream.h>
#include <google/protobuf/io/zero_copy_stream_impl_lite.h>
#include <sys/select.h>
#include <unistd.h>
#include <fcntl.h>

#include "handshakes.pb.h"

#include "uuid.h"
#include "devrand.h"

using namespace google::protobuf::io;

Peer::Peer(const char * name)
{
    this->name = name;
    updatePeers = false;

    kashmir::system::DevRand devrandom;
    kashmir::system::DevRand & in = devrandom;
    std::ostringstream ss;
    std::ostream & out = ss;
    kashmir::uuid_t uuid_obj;

    in >> uuid_obj;
    out << uuid_obj;

    uuid = strdup(ss.str().c_str());

    LOG(INFO) << "Created Peer: " << uuid;

    broadcastLock.lock();
    listenThread = new tthread::thread(Listen, this);
    broadcastThread = new tthread::thread(Broadcast, this);
    updateThread = new tthread::thread(UpdatePeers, this);
    listenPeersThread = new tthread::thread(ListenPeers, this);
}

void Peer::UpdateName(const char * name)
{
    SandMessage nameUpdate;
    std::string data;

    LOG(INFO) << "Peer::UpdateName(" << name << ")";

    this->name = strdup(name);

    nameUpdate.set_type(SandMessage_MessageType_NAME_UPDATE);
    nameUpdate.mutable_nameupdate()->set_name(this->name);

    nameUpdate.SerializeToString(&data);

    updateLock.lock();
    globalUpdates.push(data);
    updateLock.unlock();

    updatePeers = true;
}

const char * Peer::GetName()
{
    return name;
}

void Peer::Listen(void * arg)
{
    Peer * self = (Peer *)arg;
    struct sockaddr_in server, client;
    int sock, connectionSock;
    socklen_t clientlen;
    uint16_t port;
    char buffer[256];

    LOG(INFO) << "Peer::Listen()";

    sock = socket(AF_INET, SOCK_STREAM, 0);

    server.sin_family = AF_INET;
    server.sin_addr.s_addr = INADDR_ANY;

    // Try to find a port that we can use

    for(port = 1824; port < 2000; port++)
    {
        server.sin_port = htons(port);

        if(bind(sock, (struct sockaddr *)&server, sizeof(server)) >= 0)
        {
            break;
        }
        else
        {
            LOG(WARNING) << "Skipping port " << ntohs(server.sin_port) << "; already in use!";
            server.sin_port = 0;
        }
    }

    // If we found a port, go ahead; if none were available, die

    if(server.sin_port)
    {
        self->port = ntohs(server.sin_port);
        LOG(INFO) << "Going to listen on port " << self->port;
    }
    else
    {
        LOG(FATAL) << "Couldn't find an available port!";
    }

    // Listen for connections and unlock the Bonjour thread

    listen(sock, 10);
    self->broadcastLock.unlock();

    while(1)
    {
        int nbytes = 0;

        connectionSock = accept(sock, (struct sockaddr*)&client, &clientlen);

        LOG(INFO) << "Accepted connection from " << inet_ntoa(client.sin_addr);

        nbytes = read(connectionSock, buffer, sizeof(buffer) - 1);
        buffer[nbytes] = 0;

        // TODO: bad strcmp
        if(strncmp(buffer, self->uuid, sizeof(buffer)) == 0)
        {
            LOG(INFO) << "Tried to connect to self";

            buffer[0] = '0';

            write(connectionSock, buffer, sizeof(buffer));
            close(connectionSock);
        }
        else
        {
            LOG(INFO) << "Connected to: " << buffer;

            buffer[0] = '1';

            write(connectionSock, buffer, sizeof(buffer));

            self->updateLock.lock();
            self->peers.push_front(new RemotePeer(connectionSock));
            //self->updatePeers = true;
            self->updateLock.unlock();
        }
    }
}

void Peer::UpdatePeers(void * arg)
{
    Peer * self = (Peer *)arg;

    while(1)
    {
        if(self->updatePeers)
        {
            self->updateLock.lock();
            LOG(INFO) << "Updating peers...";

            if(!self->globalUpdates.empty())
            {
                std::string data = self->globalUpdates.front();
                self->globalUpdates.pop();

                for(std::list<RemotePeer *>::iterator it = self->peers.begin(); it != self->peers.end(); it++)
                {
                    RemotePeer * rp = *it;

                    if(rp->GetName())
                    {
                        LOG(INFO) << "Updating " << rp->GetName();
                    }
                    else
                    {
                        LOG(INFO) << "Updating unnamed peer";
                    }

                    write(rp->GetSocket(), data.c_str(), data.length());

                    LOG(INFO) << "Wrote " << data.length() << " bytes to " << rp->GetSocket();
                }
            }

            if(self->globalUpdates.empty())
                self->updatePeers = false;

            self->updateLock.unlock();
        }
        else
        {
            usleep(10000);
        }
    }
}

void Peer::ListenPeers(void * arg)
{
    Peer * self = (Peer *)arg;
    int selected;
    fd_set fds;
    struct timeval timeout;
    timeout.tv_sec = 1;
    timeout.tv_usec = 0;
    bool readOne = false;
    int maxSock;

    while(1)
    {
        readOne = false;
        maxSock = 0;

        self->updateLock.lock();
        std::list<RemotePeer *> peersCopy = self->peers;
        self->updateLock.unlock();

        FD_ZERO(&fds);

        // TODO: keep this list around and update it when we get new peers, which is much less often!
        for(std::list<RemotePeer *>::iterator it = peersCopy.begin(); it != peersCopy.end(); it++)
        {
            int sock = (*it)->GetSocket();
            FD_SET(sock, &fds);

            if(sock > maxSock)
            {
                maxSock = sock;
            }
        }

        select(maxSock + 1, &fds, NULL, NULL, &timeout);

        // TODO: as above, use the faster, cached list
        for(std::list<RemotePeer *>::iterator it = peersCopy.begin(); it != peersCopy.end() && !readOne; it++)
        {
            RemotePeer * rp = *it;
            int sock = rp->GetSocket();

            if(FD_ISSET(sock, &fds))
            {
                int nbytes;
                int flags;
                char buffer[256];

                readOne = true;

                nbytes = read(sock, buffer, sizeof(buffer) - 1);

                LOG(INFO) << "Read " << nbytes << " bytes from " << sock;

                buffer[nbytes] = 0;
            }
        }

        if(!readOne)
        {
            LOG(INFO) << "Skipped select";
        }
    }
    //SandMessage nameParsed;
    //nameParsed.ParseFromString(data);

    //printf("nameout: %s\n", nameParsed.nameupdate().name().c_str());
}

void Peer::Broadcast(void * arg)
{
    Peer * self = (Peer *)arg;

    self->broadcastLock.lock();

    MDNSRegister(self->port);
    MDNSBrowse(PeerJoined, PeerLeft, self);

    while(1)
    {
        MDNSResponderTick();
        usleep(10000);
    }
}

void Peer::PeerJoined(MDNSService * service, void * info)
{
    Peer * self = (Peer *)info;
    int sock;
    char buffer;
    struct sockaddr_in peerAddress = *(sockaddr_in *)&service->address;

    sock = socket(AF_INET, SOCK_STREAM, 0);
    peerAddress.sin_family = AF_INET;

    LOG(INFO) << "Peer Joined: " << inet_ntoa(peerAddress.sin_addr) << " on " << ntohs(peerAddress.sin_port);

    if(connect(sock, (struct sockaddr *)&peerAddress, sizeof(peerAddress)) < 0)
    {
        LOG(ERROR) << "Failed to connect to " << inet_ntoa(peerAddress.sin_addr);
        return;
    }

    write(sock, self->uuid, strlen(self->uuid));
    read(sock, &buffer, 1);

    if(buffer == '0')
    {
        close(sock);
    }
}

void Peer::PeerLeft(MDNSService * service, void * info)
{
    Peer * self = (Peer *)info;
    struct sockaddr_in peerAddress = *(sockaddr_in *)&service->address;

    LOG(INFO) << "Peer Left: " << inet_ntoa(peerAddress.sin_addr);
}
