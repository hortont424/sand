using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Sand.Tools;
using Sand.Tools.Mobilities;
using Sand.Tools.Primaries;
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
        private Label _nameLabel, _descriptionLabel;
        private ToolChooserButton _primariesToolChooser;

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

            var primaries = new List<Type>();

            switch((Storage.NetworkSession.LocalGamers[0].Tag as Player).Class)
            {
                case Class.None:
                    break;
                case Class.Defense:
                    primaries.Add(typeof(Jet));
                    break;
                case Class.Offense:
                    primaries.Add(typeof(Laser));
                    break;
                case Class.Support:
                    primaries.Add(typeof(Plow));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _primariesToolChooser = new ToolChooserButton(Game, new Vector2(200, _sandLogo.Y + 300), "Primaries",
                                                          primaries);
            _weaponsToolChooser = new ToolChooserButton(Game, new Vector2(200, _sandLogo.Y + 500), "Weapons", weapons);
            _utilitiesToolChooser = new ToolChooserButton(Game, new Vector2(200, _sandLogo.Y + 700), "Utilities",
                                                          utilities);
            _mobilitiesToolChooser = new ToolChooserButton(Game, new Vector2(200, _sandLogo.Y + 900), "Mobilities",
                                                           mobilities);

            _weaponsToolChooser.SetHoverAction(HoverTool, null);
            _utilitiesToolChooser.SetHoverAction(HoverTool, null);
            _mobilitiesToolChooser.SetHoverAction(HoverTool, null);
            _primariesToolChooser.SetHoverAction(HoverTool, null);

            _nameLabel = new Label(Game, 1300, _sandLogo.Y + 300, "", "Calibri24Bold");
            _descriptionLabel = new Label(Game, 1300, _sandLogo.Y + 350, "", "Calibri24");

            Game.Components.Add(_sandLogo);
            Game.Components.Add(_readyButton);
            Game.Components.Add(_weaponsToolChooser);
            Game.Components.Add(_utilitiesToolChooser);
            Game.Components.Add(_mobilitiesToolChooser);
            Game.Components.Add(_primariesToolChooser);

            Game.Components.Add(_nameLabel);
            Game.Components.Add(_descriptionLabel);
        }

        private void HoverTool(Type toolclass, object userdata)
        {
            _nameLabel.Text = toolclass.GetMethod("_name").Invoke(null, null) as string;
            _descriptionLabel.Text = toolclass.GetMethod("_description").Invoke(null, null) as string;
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

                if(_primariesToolChooser.SelectedTool != null)
                {
                    player.PrimaryA =
                        _primariesToolChooser.SelectedTool.GetConstructor(localPlayerTypeArray).Invoke(localPlayerArray)
                        as
                        Tool;
                }
            }

            Game.Components.Remove(_sandLogo);
            Game.Components.Remove(_readyButton);
            Game.Components.Remove(_weaponsToolChooser);
            Game.Components.Remove(_utilitiesToolChooser);
            Game.Components.Remove(_mobilitiesToolChooser);
            Game.Components.Remove(_primariesToolChooser);

            Game.Components.Remove(_nameLabel);
            Game.Components.Remove(_descriptionLabel);

            Messages.Update();

            return null;
        }
    }
}