using System;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Net;

namespace Sand.GameState
{
    public class ReadyWaitState : GameState
    {
        public ReadyWaitState(Sand game) : base(game)
        {
        }

        public override void Enter()
        {
            Storage.networkSession.LocalGamers[0].IsReady = true;

            Storage.networkSession.GameStarted += GameStarted;
            Storage.networkSession.GameEnded += GameEnded;
        }

        private void GameStarted(object sender, GameStartedEventArgs e)
        {
            Console.WriteLine("start game!");

            foreach(var gamer in Storage.networkSession.AllGamers)
            {
                if(!gamer.IsLocal)
                {
                    gamer.Tag = new RemotePlayer(Game, gamer);
                }
            }

            Game.TransitionState(States.Play);
        }

        private void GameEnded(object sender, GameEndedEventArgs e)
        {
            Console.WriteLine("done game!");
        }

        public override void Update()
        {
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

        public override void Leave()
        {
        }
    }
}