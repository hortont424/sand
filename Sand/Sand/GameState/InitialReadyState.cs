using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Input;

namespace Sand.GameState
{
    public class InitialReadyState : GameState
    {
        private Billboard _sandLogo;
        private Label _readyLabel;
        private Label _serverLabel;

        public InitialReadyState(Sand game) : base(game)
        {
        }

        public override void Enter(Dictionary<string, object> data)
        {
            var logoSprite = Storage.Sprite("SandLogo");
            var sandLogoOrigin = new Vector2(Game.BaseScreenSize.X * 0.5f - (logoSprite.Width * 0.5f), 300);

            _sandLogo = new Billboard(Game, sandLogoOrigin, logoSprite);
            _readyLabel = new Label(Game, Game.BaseScreenSize.X * 0.5f, Game.BaseScreenSize.Y * 0.5f,
                                    "Click to Begin", "Calibri48Bold") { PositionGravity = Gravity.Center };
            _serverLabel = new Label(Game, Game.BaseScreenSize.X - 15.0f, 10.0f,
                                    "", "Calibri24Bold") { PositionGravity = new Tuple<Gravity.Vertical, Gravity.Horizontal>(Gravity.Vertical.Top, Gravity.Horizontal.Right) };

            Game.Components.Add(_sandLogo);
            Game.Components.Add(_serverLabel);
            Game.Components.Add(_readyLabel);
        }

        public override void Update()
        {
            MouseState mouse = Mouse.GetState();

            if(mouse.LeftButton == ButtonState.Pressed && !Guide.IsVisible)
            {
                Game.TransitionState(States.Lobby);
            }

            _serverLabel.Text = Storage.NetworkSession.IsHost ? "Server" : "";
        }

        public override Dictionary<string, object> Leave()
        {
            Game.Components.Remove(_sandLogo);
            Game.Components.Remove(_serverLabel);
            Game.Components.Remove(_readyLabel);

            return new Dictionary<string, object> { { "SandLogo", _sandLogo } };
        }
    }
}