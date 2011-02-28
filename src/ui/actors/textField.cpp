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

#include "textField.h"

#include <iostream>
#include <glog/logging.h>
#include <glfw.h>

#define TEXT_FIELD_FILL_COLOR 0.310f, 0.525f, 0.788f, 0.2f
#define TEXT_FIELD_HOVER_FILL_COLOR 0.310f, 0.525f, 0.788f, 0.4f
#define TEXT_FIELD_CLICK_FILL_COLOR 0.310f, 0.525f, 0.788f, 0.6f
#define TEXT_FIELD_BORDER_COLOR 0.310f, 0.525f, 0.788f, 1.0f
#define TEXT_FIELD_FOCUSED_BORDER_COLOR 0.6f, 0.6, 0.6, 1.0f

TextField::TextField() : Actor::Actor()
{
    this->text = "TIM";

    this->label = new Text(this->text.c_str());

    SetH(this->label->GetH() + 6);

    action = (TextFieldAction)NULL;
    actionInfo = NULL;
}

bool TextField::AcceptsFocus()
{
    return true;
}

void TextField::Draw()
{
    if(GetHovering())
    {
        glColor4f(TEXT_FIELD_HOVER_FILL_COLOR);
    }
    else
    {
        glColor4f(TEXT_FIELD_FILL_COLOR);
    }

    // TODO: opengl1.1ify

    glBegin(GL_QUADS);
        glVertex2f(0.0f, 0.0f);
        glVertex2f(0.0f, GetH());
        glVertex2f(GetW(), GetH());
        glVertex2f(GetW(), 0.0f);
    glEnd();

    glColor4f(TEXT_FIELD_BORDER_COLOR);

    glBegin(GL_LINE_STRIP);
        glVertex2f(0.0f, 0.0f);
        glVertex2f(0.0f, GetH());
        glVertex2f(GetW(), GetH());
        glVertex2f(GetW(), 0.0f);
        glVertex2f(0.0f, 0.0f);
    glEnd();

    if(GetFocused())
    {
        glColor4f(TEXT_FIELD_FOCUSED_BORDER_COLOR);

        glBegin(GL_LINE_STRIP);
            glVertex2f(-4.0f, -4.0f);
            glVertex2f(-4.0f, GetH() + 4.0f);
            glVertex2f(GetW() + 4.0f, GetH() + 4.0f);
            glVertex2f(GetW() + 4.0f, -4.0f);
            glVertex2f(-4.0f, -4.0f);
        glEnd();
    }

    glPushMatrix();
    glTranslatef(8.0f, 3.0f, 0.0f);
    this->label->Draw();
    glPopMatrix();
}

void TextField::SetAction(TextFieldAction cb, void * info)
{
    action = cb;
    actionInfo = info;
}

void TextField::MouseDown(int button)
{
    SetClicking(true);
}

void TextField::MouseUp(int button)
{
    if(GetClicking())
    {
        SetClicking(false);

        if(action)
        {
            action(this, actionInfo);
        }
    }
}

void TextField::MouseCancelled()
{
    SetClicking(false);
}

void TextField::KeyUp(int key)
{
    if(key == 295)
    {
        if(this->text.length())
            this->text.erase(this->text.length() - 1);
    }
    else
    {
        if((key >= 'A' && key <= 'Z') || (key >= '0' && key <= '9'))
            this->text.push_back((char)key);
    }

    if(this->label)
    {
        delete this->label;
    }

    this->label = new Text(this->text.c_str());

    if(this->label->GetW() > (GetW() - 10))
    {
        KeyUp(295); // backspace
    }
}

void TextField::KeyDown(int key)
{
}
