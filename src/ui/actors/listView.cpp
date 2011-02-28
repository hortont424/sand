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

#include "listView.h"

#include <iostream>
#include <glog/logging.h>
#include <glfw.h>

ListView::ListView() : Actor::Actor()
{

}

void ListView::Draw()
{
    glPushMatrix();

    for(std::list<Text *>::iterator it = labels.begin(); it != labels.end(); it++)
    {
        Text * label = *it;

        label->Draw();

        glTranslatef(0.0f, label->GetH(), 0.0f);
    }

    glPopMatrix();
}

void ListView::SetElements(std::list<std::string> elements)
{
    int32_t maxW = 0, totalH = 0;
    labels.clear();

    for(std::list<std::string>::iterator it = elements.begin(); it != elements.end(); it++)
    {
        std::string str = *it;
        Text * strText = new Text(str.c_str());

        labels.push_back(strText);

        maxW = std::max(strText->GetW(), maxW);
        totalH += strText->GetH();
    }

    SetW(maxW);
    SetH(totalH);
}