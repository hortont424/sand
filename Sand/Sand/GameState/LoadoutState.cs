using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Input;

namespace Sand.GameState
{
    public class LoadoutState : GameState
    {
        private Billboard _sandLogo;
        private Button _readyButton;

        public LoadoutState(Sand game) : base(game)
        {
        }

        public override void Enter(Dictionary<string, object> data)
        {
            _sandLogo = data["SandLogo"] as Billboard;

            var readyButtonRect = new Rectangle(0, 0, 200, 50);
            readyButtonRect.X = (int)Game.BaseScreenSize.X - readyButtonRect.Width - 50;
            readyButtonRect.Y = (int)Game.BaseScreenSize.Y - readyButtonRect.Height - 50;
            _readyButton = new Button(Game, readyButtonRect, "Ready", new Color(0.1f, 0.7f, 0.1f));
            _readyButton.SetAction((a, userInfo) => Game.TransitionState(States.ReadyWait), null);

            Game.Components.Add(_sandLogo);
            Game.Components.Add(_readyButton);
        }

        public override void Update()
        {
        }

        public override Dictionary<string, object> Leave()
        {
            Game.Components.Remove(_sandLogo);
            Game.Components.Remove(_readyButton);

            return null;
        }
    }
}