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
 * THIS SOFTWARE IS PROVIDED BY THE CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED
 * WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT
 * SHALL ANY AUTHORS OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
 * LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

#include <iostream>

#include <network/peer.h>
#include <glog/logging.h>
#include <glfw.h>
#include <ui/actors/billboard.h>

int main(int argc, char const * argv[])
{
    bool running = true;

    google::InitGoogleLogging(argv[0]);

    glfwInit();
    glfwOpenWindow(200, 200, 8, 8, 8, 8, 0, 0, GLFW_WINDOW);

    Peer * peer = new Peer("Unnamed");
    Billboard * bill = new Billboard();

    while(running)
    {
        glClear(GL_COLOR_BUFFER_BIT);
        glfwSwapBuffers();

        bill->Draw();

        if(glfwGetKey(GLFW_KEY_ESC))
        {
            peer->UpdateName("Asdf");
            usleep(1000000);
        }

        running = glfwGetWindowParam(GLFW_OPENED);

        usleep(10000);
    }

    glfwTerminate();

    return 0;
}