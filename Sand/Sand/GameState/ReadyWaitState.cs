using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Net;

namespace Sand.GameState
{
    public class ReadyWaitState : GameState
    {
        private Label _readyLabel;

        public ReadyWaitState(Sand game) : base(game)
        {
        }

        public override void Enter(Dictionary<string, object> data)
        {
            Storage.NetworkSession.LocalGamers[0].IsReady = true;

            Storage.NetworkSession.GameStarted += GameStarted;
            Storage.NetworkSession.GameEnded += GameEnded;

            _readyLabel = new Label(Game, Game.BaseScreenSize.X / 2.0f, Game.BaseScreenSize.Y / 2.0f,
                                    "Waiting for Players",
                                    "Calibri48Bold") { DrawOrder = 1000, PositionGravity = Gravity.Center };
            Game.Components.Add(_readyLabel);
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

            if(Storage.NetworkSession.IsHost)
            {
                if(Storage.NetworkSession.IsEveryoneReady)
                {
                    Storage.NetworkSession.StartGame();

                    foreach(var signedInGamer in Gamer.SignedInGamers)
                    {
                        signedInGamer.Presence.PresenceMode = GamerPresenceMode.InCombat;
                    }
                }
            }
        }

        public override Dictionary<string, object> Leave()
        {
            var player = Storage.NetworkSession.LocalGamers[0].Tag as LocalPlayer;

            if(player != null)
            {
                switch(player.Team)
                {
                    case Team.None:
                        break;
                    case Team.Red:
                        player.X = Storage.Game.GameMap.RedSpawn.X;
                        player.Y = Storage.Game.GameMap.RedSpawn.Y;
                        break;
                    case Team.Blue:
                        player.X = Storage.Game.GameMap.BlueSpawn.X;
                        player.Y = Storage.Game.GameMap.BlueSpawn.Y;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            Game.Components.Remove(_readyLabel);
            return null;
        }
    }
}