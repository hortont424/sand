using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Sand.GameState
{
    public class PlayState : GameState
    {
        private Label _countdownLabel;

        public PlayState(Sand game) : base(game)
        {
        }

        public override void Enter(Dictionary<string, object> data)
        {
            var sandGame = Game;

            foreach(var gamer in Storage.NetworkSession.AllGamers)
            {
                Game.Components.Add((Player)gamer.Tag);
            }

            sandGame.GameMap = new Map(Game, "01");

            Game.Components.Add(sandGame.GameMap);

            var localPlayer = Storage.NetworkSession.LocalGamers[0].Tag as LocalPlayer;

            if(localPlayer != null)
            {
                var mobilityIcon = new ToolIcon(Game, localPlayer.Mobility)
                                   {
                                       Position = new Vector2(sandGame.GameMap.Width + 10.0f + 148.0f, 10.0f + 148.0f)
                                   };

                Game.Components.Add(mobilityIcon);

                var weaponIcon = new ToolIcon(Game, localPlayer.Weapon)
                                 {
                                     Position = new Vector2(mobilityIcon.Position.X + 60.0f + 148.0f, 10.0f + 148.0f)
                                 };

                Game.Components.Add(weaponIcon);

                var utilityIcon = new ToolIcon(Game, localPlayer.Utility)
                                  {
                                      Position = new Vector2(weaponIcon.Position.X + 60.0f + 148.0f, 10.0f + 148.0f)
                                  };

                Game.Components.Add(utilityIcon);
            }

            var acceptInputAnimation = new Animation { CompletedDelegate = BeginAcceptingInput };
            Storage.AnimationController.Add(acceptInputAnimation, 1500);

            _countdownLabel = new Label(Game, Game.BaseScreenSize.X / 2.0f, Game.BaseScreenSize.Y / 2.0f, "Ready...",
                                           "Calibri48Bold") { DrawOrder = 1000 };
            Game.Components.Add(_countdownLabel);
        }

        private void BeginAcceptingInput()
        {
            Storage.AcceptInput = true;
            Game.Components.Remove(_countdownLabel);
        }

        public override void Update()
        {
            Messages.Update();
        }

        public override Dictionary<string, object> Leave()
        {
            var sandGame = Game;

            foreach(var gamer in Storage.NetworkSession.AllGamers)
            {
                Game.Components.Remove((Player)gamer.Tag);
            }

            Game.Components.Remove(sandGame.GameMap);

            return null;
        }
    }
}