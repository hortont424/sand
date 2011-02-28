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

#ifndef _SAND_UI_ACTOR_H_
#define _SAND_UI_ACTOR_H_

#include <inttypes.h>

#define GRAVITY_TOP (1 << 0)
#define GRAVITY_BOTTOM (1 << 1)
#define GRAVITY_LEFT (1 << 2)
#define GRAVITY_RIGHT (1 << 3)

class Actor
{
    public:
        Actor * parent;
        Actor * window;

        Actor();

        virtual void Draw() = 0;

        virtual bool AcceptsFocus();

        void SetPosition(int32_t x, int32_t y);
        void SetSize(int32_t w, int32_t h);

        void SetGravity(int32_t grav);
        int32_t GetGravity();

        void SetX(int32_t x);
        void SetY(int32_t y);
        void SetW(int32_t w);
        void SetH(int32_t h);
        int32_t GetX();
        int32_t GetY();
        int32_t GetW();
        int32_t GetH();

        void SetHovering(bool hover);
        void SetClicking(bool click);
        void SetFocused(bool focused);
        bool GetHovering();
        bool GetClicking();
        bool GetFocused();

        int32_t GetPickingName();
        static Actor * GetActorForPick(int pick);

        virtual void MouseMoved(int x, int y);
        virtual void MouseDown(int button);
        virtual void MouseUp(int button);
        virtual void MouseCancelled();

        virtual void KeyUp(int key);
        virtual void KeyDown(int key);

    private:
        int32_t x, y, w, h, gravity;
        int32_t pickingName;
        bool dirty;
        bool hovering, clicking, focused;
};

#endif
