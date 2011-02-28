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
#include <ui/actors/button.h>
#include <ui/actors/text.h>
#include <ui/actors/textField.h>
#include <ui/window.h>

void DoSomething(void * sender, void * info)
{
    printf("do something!!\n");
}

int main(int argc, char const * argv[])
{
    google::InitGoogleLogging(argv[0]);
    glfwInit();

    Peer * peer = new Peer("Unnamed");
    Window * win = new Window(1440, 900);

    Button * button = new Button("Ready");
    button->SetPosition(1440 - 20, 20);
    button->SetGravity(GRAVITY_RIGHT | GRAVITY_BOTTOM);
    button->SetAction(DoSomething, NULL);
    win->AddActor(button);

    TextField * textField = new TextField();
    textField->SetPosition(1440 / 2, 5 * 900 / 6);
    textField->SetW(300);
    textField->SetGravity(GRAVITY_VCENTER | GRAVITY_HCENTER);
    win->AddActor(textField);

    Text * nameLabel = new Text("Enter your name:");
    nameLabel->SetPosition(1440 / 2, (5 * 900 / 6) + textField->GetH() + 5);
    nameLabel->SetGravity(GRAVITY_VCENTER | GRAVITY_HCENTER);
    win->AddActor(nameLabel);

    /*button = new Button();
    button->SetPosition(400 - 50, 300 - 60);
    button->SetSize(100, 40);
    win->AddActor(button);
    Text * text = new Text("Hello, world!", 200, 200);
    text->SetPosition(400 - 50, 300 - 60);
    win->AddActor(text);*/

    win->MainLoop();

    return 0;
}