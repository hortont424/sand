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
        private ToolChooserButton _primaryAToolChooser, _primaryBToolChooser;
        private Billboard _primaryButtonA;
        private Billboard _primaryButtonB;
        private Billboard _weaponButton;
        private Billboard _utilityButton;
        private Billboard _mobilityButton;

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

            var weapons = new List<Type> { typeof(Cannon), typeof(EMP) };
            var utilities = new List<Type> { typeof(Shield), typeof(Ground) };
            var mobilities = new List<Type> { typeof(BoostDrive), typeof(WinkDrive), typeof(BlinkDrive) };

            var primaries = new List<Type>();

            switch((Storage.NetworkSession.LocalGamers[0].Tag as Player).Class)
            {
                case Class.None:
                    break;
                case Class.Defense:
                    primaries.Add(typeof(Jet));
                    primaries.Add(typeof(SandCharge));
                    break;
                case Class.Offense:
                    primaries.Add(typeof(Laser));
                    primaries.Add(typeof(FlameCharge));
                    break;
                case Class.Support:
                    primaries.Add(typeof(Plow));
                    primaries.Add(typeof(PressureCharge));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _primaryAToolChooser = new ToolChooserButton(Game, new Vector2(300, _sandLogo.Y + 220), "Primary 1",
                                                          primaries);
            _primaryBToolChooser = new ToolChooserButton(Game, new Vector2(300, _sandLogo.Y + 420), "Primary 2",
                                                          primaries);
            _primaryBToolChooser.ChooseTool(null, primaries[1], false); // TODO: this is all a hack
            _weaponsToolChooser = new ToolChooserButton(Game, new Vector2(300, _sandLogo.Y + 620), "Weapon", weapons);
            _utilitiesToolChooser = new ToolChooserButton(Game, new Vector2(300, _sandLogo.Y + 820), "Utility",
                                                          utilities);
            _mobilitiesToolChooser = new ToolChooserButton(Game, new Vector2(300, _sandLogo.Y + 1020), "Mobility",
                                                           mobilities);

            var origin = 50;
            var offset = 17;

            _primaryButtonA = new Billboard(Game, new Vector2(origin + 30, _primaryAToolChooser.Y - offset), Storage.Sprite("lmouse"))
                              { PositionGravity = Gravity.Center };

            _primaryButtonB = new Billboard(Game, new Vector2(origin + 30, _primaryBToolChooser.Y - offset), Storage.Sprite("lmouse")) { PositionGravity = Gravity.Center };

            _weaponButton = new Billboard(Game, new Vector2(origin + 30, _weaponsToolChooser.Y - offset), Storage.Sprite("rmouse")) { PositionGravity = Gravity.Center };

            _utilityButton = new Billboard(Game, new Vector2(origin, _utilitiesToolChooser.Y - offset), Storage.Sprite("shift")) { PositionGravity = Gravity.Center };

            _mobilityButton = new Billboard(Game, new Vector2(origin, _mobilitiesToolChooser.Y - offset), Storage.Sprite("space")) { PositionGravity = Gravity.Center };


            Game.Components.Add(_sandLogo);
            Game.Components.Add(_readyButton);
            Game.Components.Add(_weaponsToolChooser);
            Game.Components.Add(_utilitiesToolChooser);
            Game.Components.Add(_mobilitiesToolChooser);
            Game.Components.Add(_primaryAToolChooser);
            Game.Components.Add(_primaryBToolChooser);

            Game.Components.Add(_primaryButtonA);
            Game.Components.Add(_primaryButtonB);
            Game.Components.Add(_weaponButton);
            Game.Components.Add(_utilityButton);
            Game.Components.Add(_mobilityButton);
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

                if(_primaryAToolChooser.SelectedTool != null)
                {
                    player.PrimaryA =
                        _primaryAToolChooser.SelectedTool.GetConstructor(localPlayerTypeArray).Invoke(localPlayerArray)
                        as
                        Tool;
                }

                if (_primaryBToolChooser.SelectedTool != null)
                {
                    player.PrimaryB =
                        _primaryBToolChooser.SelectedTool.GetConstructor(localPlayerTypeArray).Invoke(localPlayerArray)
                        as
                        Tool;
                }
            }

            Game.Components.Remove(_sandLogo);
            Game.Components.Remove(_readyButton);
            Game.Components.Remove(_weaponsToolChooser);
            Game.Components.Remove(_utilitiesToolChooser);
            Game.Components.Remove(_mobilitiesToolChooser);
            Game.Components.Remove(_primaryAToolChooser);
            Game.Components.Remove(_primaryBToolChooser);

            Game.Components.Remove(_primaryButtonA);
            Game.Components.Remove(_primaryButtonB);
            Game.Components.Remove(_weaponButton);
            Game.Components.Remove(_utilityButton);
            Game.Components.Remove(_mobilityButton);

            Messages.Update();

            return null;
        }
    }
}