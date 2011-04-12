using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Sand.Tools;
using Sand.Tools.Mobilities;
using Sand.Tools.Utilities;
using Sand.Tools.Weapons;

namespace Sand.GameState
{
    public class LoadoutState : GameState
    {
        private Billboard _sandLogo;
        private Button _readyButton;
        private ToolChooserButton _weaponsToolChooser;
        private ToolChooserButton _utilitiesToolChooser;
        private ToolChooserButton _mobilitiesToolChooser;

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

            var weapons = new List<Type> { typeof(Cannon) };
            var utilities = new List<Type> { typeof(Shield) };
            var mobilities = new List<Type> { typeof(BoostDrive), typeof(WinkDrive) };

            _weaponsToolChooser = new ToolChooserButton(Game, new Vector2(200, _sandLogo.Y + 300), "Weapons", weapons);
            _utilitiesToolChooser = new ToolChooserButton(Game, new Vector2(200, _sandLogo.Y + 500), "Utilities",
                                                          utilities);
            _mobilitiesToolChooser = new ToolChooserButton(Game, new Vector2(200, _sandLogo.Y + 700), "Mobilities",
                                                           mobilities);

            Game.Components.Add(_sandLogo);
            Game.Components.Add(_readyButton);
            Game.Components.Add(_weaponsToolChooser);
            Game.Components.Add(_utilitiesToolChooser);
            Game.Components.Add(_mobilitiesToolChooser);
        }

        public override void Update()
        {
        }

        public override Dictionary<string, object> Leave()
        {
            var player = Storage.NetworkSession.LocalGamers[0].Tag as LocalPlayer;

            if(player != null)
            {
                Type[] localPlayerTypeArray = { typeof(LocalPlayer) };
                object[] localPlayerArray = { player };

                player.Weapon =
                    _weaponsToolChooser.SelectedTool.GetConstructor(localPlayerTypeArray).Invoke(localPlayerArray) as
                    Tool;
                player.Utility =
                    _utilitiesToolChooser.SelectedTool.GetConstructor(localPlayerTypeArray).Invoke(localPlayerArray) as
                    Tool;
                player.Mobility =
                    _mobilitiesToolChooser.SelectedTool.GetConstructor(localPlayerTypeArray).Invoke(localPlayerArray) as
                    Tool;
            }

            Game.Components.Remove(_sandLogo);
            Game.Components.Remove( _readyButton);
            Game.Components.Remove(_weaponsToolChooser);
            Game.Components.Remove(_utilitiesToolChooser);
            Game.Components.Remove(_mobilitiesToolChooser);

            return null;
        }
    }
}