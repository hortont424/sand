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

        public InitialReadyState(Sand game) : base(game)
        {
        }

        public override void Enter(Dictionary<string, object> data)
        {
            var logoSprite = Storage.Sprite("SandLogo");
            var sandLogoOrigin = new Vector2(Game.BaseScreenSize.X * 0.5f - (logoSprite.Width * 0.5f), 300);

            _sandLogo = new Billboard(Game, sandLogoOrigin, logoSprite);
            _readyLabel = new Label(Game, Game.BaseScreenSize.X * 0.5f, Game.BaseScreenSize.Y * 0.5f,
                                    "Click to Begin", "Calibri48Bold");

            Game.Components.Add(_sandLogo);
            Game.Components.Add(_readyLabel);
        }

        public override void Update()
        {
            MouseState mouse = Mouse.GetState();

            Messages.Update();

            if(mouse.LeftButton == ButtonState.Pressed && !Guide.IsVisible)
            {
                Game.TransitionState(States.Lobby);
            }
        }

        public override Dictionary<string, object> Leave()
        {
            Game.Components.Remove(_sandLogo);
            Game.Components.Remove(_readyLabel);

            return new Dictionary<string, object> { { "SandLogo", _sandLogo } };
        }
    }
}