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
#include <network/discovery/mdns.h>
#include <network/util.h>

void addService(MDNSService * service)
{
    char addrString[255];

    LOG(INFO) << "add: " << sockaddrToString(&service->address);
}

void removeService(MDNSService * service)
{
    char addrString[255];

    LOG(INFO) << "remove: " << sockaddrToString(&service->address);
}

Peer::Peer(const char * name)
{
    this->name = name;
    listenThread = new tthread::thread(Listen, this);
    broadcastLock.lock();
    broadcastThread = new tthread::thread(Broadcast, this);
}

void Peer::Listen(void * arg)
{
    Peer * self = (Peer *)arg;
    struct sockaddr_in server;
    int sock;
    uint16_t port;

    LOG(INFO) << "Peer::Listen()";

    sock = socket(AF_INET, SOCK_STREAM, 0);

    server.sin_family = AF_INET;
    server.sin_addr.s_addr = INADDR_ANY;

    // Try to find a port that we can use

    for(port = 1824; port < (1824 + 10); port++)
    {
        server.sin_port = htons(port);

        if(bind(sock, (struct sockaddr *)&server, sizeof(server)) >= 0)
        {
            break;
        }
        else
        {
            LOG(WARNING) << "Skipping port " << ntohs(server.sin_port) << "; already in use!";
        }
    }

    self->port = ntohs(server.sin_port);
    LOG(INFO) << "Going to listen on port " << self->port;

    self->broadcastLock.unlock();

    while(1)
    {
        sleep(1);
    }
}

void Peer::Broadcast(void * arg)
{
    Peer * self = (Peer *) arg;

    self->broadcastLock.lock();

    MDNSRegister(self->port);
    MDNSBrowse(addService, removeService);

    while(1)
    {
        MDNSResponderTick();
        usleep(10000);
    }
}
