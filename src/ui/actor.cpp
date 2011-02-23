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

#include "actor.h"

#include <iostream>
#include <glog/logging.h>
#include <glfw.h>

Actor::Actor()
{

}

void Actor::SetX(int32_t x)
{
    this->x = x;
    this->dirty = true;
}

void Actor::SetY(int32_t y)
{
    this->y = y;
    this->dirty = true;
}

void Actor::SetW(int32_t w)
{
    this->w = w;
    this->dirty = true;
}

void Actor::SetH(int32_t h)
{
    this->h = h;
    this->dirty = true;
}

void Actor::SetPosition(int32_t x, int32_t y)
{
    this->x = x;
    this->y = y;
    this->dirty = true;
}

void Actor::SetSize(int32_t w, int32_t h)
{
    this->w = w;
    this->h = h;
    this->dirty = true;
}

int32_t Actor::GetX()
{
    return x;
}

int32_t Actor::GetY()
{
    return y;
}

int32_t Actor::GetW()
{
    return w;
}

int32_t Actor::GetH()
{
    return h;
}
