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

#include "window.h"

#include <iostream>
#include <glog/logging.h>
#include <glfw.h>

static Window * _window = NULL;

void GLFWCALL _setWindowSize(int width, int height)
{
    if(_window)
        _window->SetWindowSize(width, height);
}

Window::Window(int width, int height)
{
    if(_window)
    {
        LOG(FATAL) << "Can only create one window at a time!";
    }

    _window = this;

    glfwInit();
    glfwOpenWindow(width, height, 8, 8, 8, 8, 0, 0, GLFW_WINDOW);
    glfwSetWindowSizeCallback(_setWindowSize);
}

void Window::MainLoop()
{
    bool running = true;

    while(running)
    {
        glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

        glLoadIdentity();
        Draw();

        glfwSwapBuffers();

        running = glfwGetWindowParam(GLFW_OPENED);

        usleep(1000);
    }

    glfwTerminate();
}

void Window::SetWindowSize(int width, int height)
{
    SetSize(width, height);

    glViewport(0.0, 0.0, width, height);

    glMatrixMode(GL_PROJECTION);
    glLoadIdentity();
    glOrtho(0.0, width, 0.0, height, -100.0, 100.0);
    glMatrixMode(GL_MODELVIEW);
}