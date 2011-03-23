using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Net;

namespace Sand.GameState
{
    public class ReadyWaitState : GameState
    {
        public ReadyWaitState(Sand game) : base(game)
        {
        }

        public override void Enter(Dictionary<string, object> data)
        {
            Storage.networkSession.LocalGamers[0].IsReady = true;

            Storage.networkSession.GameStarted += GameStarted;
            Storage.networkSession.GameEnded += GameEnded;
        }

        private void GameStarted(object sender, GameStartedEventArgs e)
        {
            Console.WriteLine("start game!");

            Game.TransitionState(States.Play);
        }

        private void GameEnded(object sender, GameEndedEventArgs e)
        {
            Console.WriteLine("done game!");
        }

        public override void Update()
        {
            Messages.Update();

            if(Storage.networkSession.IsHost)
            {
                if(Storage.networkSession.IsEveryoneReady)
                {
                    Storage.networkSession.StartGame();

                    foreach(var signedInGamer in Gamer.SignedInGamers)
                    {
                        signedInGamer.Presence.PresenceMode = GamerPresenceMode.InCombat;
                    }
                }
            }
        }
    }
}