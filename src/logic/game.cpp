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

#include "game.h"

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

void UpdateNameString(void * sender, void * info)
{
    TextField * textField = (TextField *)sender;
    Peer * peer = (Peer *)info;

    peer->UpdateName(textField->GetText());
}

Game::Game()
{
    w = 1440;
    h = 900;

    state = GAME_STATE_STARTUP;

    myPeer = new Peer("Unnamed");
    window = new Window(w, h);
}

void Game::Run()
{
    window->MainLoop();
}

void Game::SetState(GameState newState)
{
    switch(state)
    {
        case GAME_STATE_STARTUP:
            break;
        case GAME_STATE_PLAYER_SETUP:
            LeavePlayerSetup();
            break;
        default:
            LOG(FATAL) << "Unknown from-state " << state;
            break;
    }

    state = newState;

    switch(state)
    {
        case GAME_STATE_PLAYER_SETUP:
            InitPlayerSetup();
            break;
        default:
            LOG(FATAL) << "Unknown to-state " << state;
            break;
    }
}

GameState Game::GetState()
{
    return state;
}

void Game::InitPlayerSetup()
{
    window->ClearActors();

    Button * button = new Button("Ready");
    button->SetPosition(w - 20, 20);
    button->SetGravity(GRAVITY_RIGHT | GRAVITY_BOTTOM);
    button->SetAction(DoSomething, NULL);
    window->AddActor(button);

    TextField * textField = new TextField();
    textField->SetPosition(w / 2, 5 * h / 6);
    textField->SetW(300);
    textField->SetGravity(GRAVITY_VCENTER | GRAVITY_HCENTER);
    textField->SetAction(UpdateNameString, myPeer);
    window->AddActor(textField);

    Text * nameLabel = new Text("Enter your name:");
    nameLabel->SetPosition(w / 2, (5 * h / 6) + textField->GetH() + 5);
    nameLabel->SetGravity(GRAVITY_VCENTER | GRAVITY_HCENTER);
    window->AddActor(nameLabel);
}

void Game::LeavePlayerSetup()
{

}