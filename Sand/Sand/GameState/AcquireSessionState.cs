﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Net;

namespace Sand.GameState
{
    public class AcquireSessionState : GameState
    {
        public AcquireSessionState(Sand game) : base(game)
        {
        }

        public override void Enter(Dictionary<string, object> data)
        {
            // Try to find a Sand server. If there isn't one, start one!

            var availableSessions = NetworkSession.Find(NetworkSessionType.SystemLink, 1, null);

            if(availableSessions.Count > 0)
            {
                Console.WriteLine("Connecting to server from {0}", availableSessions[0].HostGamertag);
                Storage.networkSession = NetworkSession.Join(availableSessions[0]);
            }
            else
            {
                Console.WriteLine("Couldn't find a server! Starting one...");
                Storage.networkSession = NetworkSession.Create(NetworkSessionType.SystemLink, 1, 6);
            }

            foreach(var gamer in Storage.networkSession.AllGamers)
            {
                if(gamer.IsLocal)
                {
                    gamer.Tag = new LocalPlayer(Game, gamer);
                }
            }

            if(Storage.networkSession != null)
            {
                Game.TransitionState(States.InitialReady);
            }
            else
            {
                Console.WriteLine("Failed to get a session!");
            }
        }
    }
}