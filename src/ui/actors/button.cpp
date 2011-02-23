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

#include "button.h"

#include <iostream>
#include <glog/logging.h>
#include <glfw.h>

#define BUTTON_FILL_COLOR 0.310f, 0.525f, 0.788f, 0.2f
#define BUTTON_HOVER_FILL_COLOR 0.310f, 0.525f, 0.788f, 0.4f
#define BUTTON_BORDER_COLOR 0.310f, 0.525f, 0.788f, 1.0f

Button::Button() : Actor::Actor()
{

}

void Button::Draw()
{
    if(GetHovering())
        glColor4f(BUTTON_HOVER_FILL_COLOR);
    else
        glColor4f(BUTTON_FILL_COLOR);

    glBegin(GL_QUADS);
        glVertex2f(0.0f, 0.0f);
        glVertex2f(0.0f, GetH());
        glVertex2f(GetW(), GetH());
        glVertex2f(GetW(), 0.0f);
    glEnd();

    glColor4f(BUTTON_BORDER_COLOR);

    glBegin(GL_LINE_STRIP);
        glVertex2f(0.0f, 0.0f);
        glVertex2f(0.0f, GetH());
        glVertex2f(GetW(), GetH());
        glVertex2f(GetW(), 0.0f);
        glVertex2f(0.0f, 0.0f);
    glEnd();
}