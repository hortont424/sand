using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Sand.Tools.Mobilities;
using Sand.Tools.Primaries;
using Sand.Tools.Utilities;
using Sand.Tools.Weapons;

namespace Sand.GameState
{
    internal class ChooseMapState : GameState
    {
        private Button _readyButton;
        private Billboard _sandLogo;

        private MapChooserButton _mapChooser;

        public ChooseMapState(Sand game) : base(game)
        {
        }

        public override void Enter(Dictionary<string, object> data)
        {
            _sandLogo = data["SandLogo"] as Billboard;

            var readyButtonRect = new Rectangle(0, 0, 200, 50);
            readyButtonRect.X = (int)Game.BaseScreenSize.X - readyButtonRect.Width - 50;
            readyButtonRect.Y = (int)Game.BaseScreenSize.Y - readyButtonRect.Height - 50;
            _readyButton = new Button(Game, readyButtonRect, "Ready", new Color(0.1f, 0.7f, 0.1f));
            _readyButton.SetAction((a, userInfo) => Game.TransitionState(States.Loadout), null);

            _mapChooser = new MapChooserButton(Game, new Vector2(50, 450));

            

            Game.Components.Add(_sandLogo);
            Game.Components.Add(_readyButton);
            Game.Components.Add(_mapChooser);
        }

        public override void Update()
        {
            Messages.Update();
        }

        public override Dictionary<string, object> Leave()
        {
            Game.Components.Remove(_sandLogo);
            Game.Components.Remove(_readyButton);
            Game.Components.Remove(_mapChooser);

            var returns = new Dictionary<string, object>();
            returns["SandLogo"] = _sandLogo;

            return returns;
        }
    }
}