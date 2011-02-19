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

#include <iostream>

#include <network/discovery/mdns.h>
#include <glog/logging.h>
#include <tinythread.h>
#include <glfw.h>

static bool runThread = true;

void TestThread(void * arg)
{
    while(runThread)
    {
        sleep(1);
    }
}

char * inet_sockaddr_toa(const struct sockaddr * sa, char * s, size_t maxlen)
{
    switch(sa->sa_family)
    {
        case AF_INET:
            inet_ntop(AF_INET, &(((struct sockaddr_in *)sa)->sin_addr), s, maxlen);
            break;

        case AF_INET6:
            inet_ntop(AF_INET6, &(((struct sockaddr_in6 *)sa)->sin6_addr), s, maxlen);
            break;

        default:
            strncpy(s, "Unknown", maxlen);
            return NULL;
    }

    return s;
}

void addService(MDNSService * service)
{
    char addrString[255];

    LOG(INFO) << "add: " << inet_sockaddr_toa(&service->address, addrString, sizeof(addrString));
}

void removeService(MDNSService * service)
{
    char addrString[255];

    LOG(INFO) << "remove: " << inet_sockaddr_toa(&service->address, addrString, sizeof(addrString));
}

int main(int argc, char const * argv[])
{
    bool running = true;

    google::InitGoogleLogging(argv[0]);

    glfwInit();
    glfwOpenWindow(800, 600, 8, 8, 8, 8, 0, 0, GLFW_WINDOW);

    MDNSRegister(atoi(argv[1]));
    MDNSBrowse(addService, removeService);

    tthread::thread t(TestThread, 0);

    while(running)
    {
        glClear(GL_COLOR_BUFFER_BIT);
        glfwSwapBuffers();

        MDNSResponderTick();

        running = !glfwGetKey(GLFW_KEY_ESC) && glfwGetWindowParam(GLFW_OPENED);
    }

    runThread = false;
    t.join();

    glfwTerminate();

    return 0;
}