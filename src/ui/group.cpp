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

#include "group.h"

#include <iostream>
#include <glog/logging.h>
#include <glfw.h>

Group::Group() : Actor::Actor()
{

}

void Group::Draw()
{
    // transform by group coords

    for(std::set<Actor *>::iterator it = actors.begin(); it != actors.end(); it++)
    {
        Actor * actor = *it;
        int32_t x, y, grav;

        x = actor->GetX();
        y = actor->GetY();
        grav = actor->GetGravity();

        if(grav & GRAVITY_RIGHT)
        {
            x -= actor->GetW();
        }
        else if(grav & GRAVITY_HCENTER)
        {
            x -= actor->GetW() / 2;
        }

        if(grav & GRAVITY_TOP)
        {
            y -= actor->GetH();
        }
        else if(grav & GRAVITY_VCENTER)
        {
            y -= actor->GetH() / 2;
        }

        glPushName(actor->GetPickingName());
        glPushMatrix();
        glTranslatef(x, y, 0.0);
        actor->Draw();
        glPopMatrix();
        glPopName();
    }
}

void Group::AddActor(Actor * actor)
{
    actors.insert(actor);
    actor->parent = this;
    actor->window = window; // TODO: do this with a setter so we can recurse through children
}

void Group::RemoveActor(Actor * actor)
{
    actors.erase(actor);

    actor->parent = actor->window = NULL;
}

void Group::ClearActors()
{
    actors.clear();

    // TODO: clear all parents and windows
}

uint32_t Group::ActorCount()
{
    return actors.size();
}
