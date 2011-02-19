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

Peer::Peer(const char * name)
{
    this->name = name;
    listenThread = new tthread::thread(Listen, this);
    broadcastLock.lock();
    broadcastThread = new tthread::thread(Broadcast, this);
    updateThread = new tthread::thread(UpdatePeers, this);
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
        connectionSock = accept(sock, (struct sockaddr*)&client, &clientlen);

        LOG(INFO) << "Accepted connection from " << inet_ntoa(client.sin_addr);

        read(connectionSock, buffer, sizeof(buffer) - 1);
        buffer[sizeof(buffer)] = 0;

        // TODO: bad strcmp
        if(strncmp(buffer, self->name, sizeof(buffer)) == 0)
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
            self->outgoingPeers.push_front(connectionSock);
            self->updatePeers = true;
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
            LOG(INFO) << "Outgoing peers: ";

            for(std::list<int>::iterator it = self->outgoingPeers.begin(); it != self->outgoingPeers.end(); it++)
            {
                LOG(INFO) << *it;
            }

            LOG(INFO) << "Incoming peers: ";

            for(std::list<int>::iterator it = self->incomingPeers.begin(); it != self->incomingPeers.end(); it++)
            {
                LOG(INFO) << *it;
            }

            self->updatePeers = false;
            self->updateLock.unlock();
        }
        else
        {
            usleep(10000);
        }
    }
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

    LOG(INFO) << "Peer Joined: " << inet_ntoa(peerAddress.sin_addr);

    sock = socket(AF_INET, SOCK_STREAM, 0);
    peerAddress.sin_family = AF_INET;

    LOG(INFO) << "Connecting to " << inet_ntoa(peerAddress.sin_addr) << " on " << ntohs(peerAddress.sin_port);

    if(connect(sock, (struct sockaddr *)&peerAddress, sizeof(peerAddress)) < 0)
    {
        LOG(ERROR) << "Failed to connect to " << inet_ntoa(peerAddress.sin_addr);
        return;
    }

    write(sock, self->name, strlen(self->name));
    read(sock, &buffer, 1);

    if(buffer == '1')
    {
        self->updateLock.lock();
        self->incomingPeers.push_front(sock);
        self->updatePeers = true;
        self->updateLock.unlock();
    }
    else
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
