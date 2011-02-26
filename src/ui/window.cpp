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

void GLFWCALL _setMousePosition(int x, int y)
{
    if(_window)
        _window->MouseMoved(x, y);
}

void GLFWCALL _setMouseClick(int button, int action)
{
    if(_window)
    {
        if(action == GLFW_PRESS)
            _window->MouseDown(button);
        else
            _window->MouseUp(button);
    }
}

void GLFWCALL _setKeyPress(int key, int action)
{
    if(_window)
    {
        if(action == GLFW_PRESS)
            _window->KeyDown(key);
        else
            _window->KeyUp(key);
    }
}

Window::Window(int width, int height) : Group::Group()
{
    if(_window)
    {
        LOG(FATAL) << "Can only create one window at a time!";
    }

    _window = this;

    window = this;
    parent = NULL;
    focusedActor = NULL;

    glfwOpenWindow(width, height, 8, 8, 8, 8, 0, 0, GLFW_WINDOW);
    glfwSetWindowSizeCallback(_setWindowSize);
    glfwSetMousePosCallback(_setMousePosition);
    glfwSetMouseButtonCallback(_setMouseClick);
    glfwSetKeyCallback(_setKeyPress);

    glShadeModel(GL_SMOOTH);
    glEnable(GL_BLEND);
    glDisable(GL_DEPTH_TEST);
    glDisable(GL_LIGHTING);

    glEnable(GL_LINE_SMOOTH);
    glEnable(GL_POINT_SMOOTH);
    glEnable(GL_POLYGON_SMOOTH);

    glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
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

std::list<Actor *> * Window::Pick(int x, int y)
{
    GLuint selectBuffer[64];
    GLint viewport[4];
    GLint selectedCount;
    std::list<Actor *> * selectedActors = new std::list<Actor *>();

    glSelectBuffer(64, selectBuffer);
    glRenderMode(GL_SELECT);

    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
    glMatrixMode(GL_PROJECTION);
    glPushMatrix();
    glLoadIdentity();
    glGetIntegerv(GL_VIEWPORT, viewport);
    gluPickMatrix(x, y, 1, 1, viewport);
    glOrtho(0, GetW(), GetH(), 0, -100.0, 100.0); // flipped coords
    glMatrixMode(GL_MODELVIEW);
    glLoadIdentity();
    glInitNames();

    Draw();

    glMatrixMode(GL_PROJECTION);
    glPopMatrix();
    glMatrixMode(GL_MODELVIEW);
    glFlush();

    selectedCount = glRenderMode(GL_RENDER);

    for(int i = 0; i < selectedCount; i++)
    {
        Actor * actor = Actor::GetActorForPick(selectBuffer[i * 4 + 3]);

        selectedActors->push_front(actor);
    }

    return selectedActors;
}

void Window::MouseMoved(int x, int y)
{
    if(x < 0 || x > GetW() || y < 0 || y > GetH())
        return;

    if(ActorCount() == 0)
        return;

    for(std::list<Actor *>::iterator it = hoveredActors.begin(); it != hoveredActors.end(); it++)
    {
        Actor * actor = *it;

        actor->SetHovering(false);

        if(actor->GetClicking())
        {
            actor->MouseCancelled();
        }
    }

    hoveredActors.clear();

    std::list<Actor *> * selectedActors = Pick(x, y);

    for(std::list<Actor *>::iterator it = selectedActors->begin(); it != selectedActors->end(); it++)
    {
        Actor * actor = *it;

        actor->SetHovering(true);
        hoveredActors.push_front(actor);
    }

    delete selectedActors;
}

void Window::MouseDown(int button)
{
    Actor * lastHovered = NULL;

    if(hoveredActors.size())
    {
        lastHovered = hoveredActors.back();
    }

    for(std::list<Actor *>::iterator it = hoveredActors.begin(); it != hoveredActors.end(); it++)
    {
        (*it)->MouseDown(button);
    }

    if(focusedActor && (focusedActor != lastHovered))
    {
        focusedActor->SetFocused(false);
        focusedActor = NULL;
    }

    if(lastHovered && lastHovered->AcceptsFocus())
    {
        focusedActor = hoveredActors.back();
        hoveredActors.back()->SetFocused(true);
    }
}

void Window::MouseUp(int button)
{
    for(std::list<Actor *>::iterator it = hoveredActors.begin(); it != hoveredActors.end(); it++)
    {
        (*it)->MouseUp(button);
    }
}

void Window::KeyDown(int key)
{
    if(focusedActor)
    {
        focusedActor->KeyDown(key);
    }
}

void Window::KeyUp(int key)
{
    if(focusedActor)
    {
        focusedActor->KeyUp(key);
    }
}

