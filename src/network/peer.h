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

#ifndef _SAND_NETWORK_PEER_H_
#define _SAND_NETWORK_PEER_H_

#include "remotePeer.h"

#include <glog/logging.h>
#include <tinythread.h>
#include <fast_mutex.h>
#include <network/discovery/mdns.h>
#include <list>
#include <queue>

class Peer
{
    public:
        Peer(const char * name);

        void UpdateName(const char * name);
        const char * GetName();

    private:
        tthread::thread * listenThread, * broadcastThread, * updateThread, * listenPeersThread;
        tthread::fast_mutex broadcastLock, updateLock;

        const char * name;
        const char * uuid;

        uint16_t port;
        bool updatePeers;
        std::list<RemotePeer *> peers;
        std::queue<std::string> globalUpdates;

        Peer();
        static void Listen(void * arg);
        static void ListenPeers(void * arg);
        static void Broadcast(void * arg);
        static void UpdatePeers(void * arg);

        static void PeerJoined(MDNSService * service, void * info);
        static void PeerLeft(MDNSService * service, void * info);
};

#endif
