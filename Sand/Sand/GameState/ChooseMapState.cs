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
        private Button _tutorialButton;

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

            _tutorialButton = new Button(Game, new Rectangle(50, readyButtonRect.Y, 200, 50), "Tutorial");
            _tutorialButton.SetAction((a, b) =>
                                      {
                                          Storage.Game.GameMap = Storage.Game.MapManager.TutorialMap;

                                          var player = Storage.NetworkSession.LocalGamers[0].Tag as LocalPlayer;

                                          if(player != null)
                                          {
                                              player.Weapon = new Cannon(player);
                                              player.Utility = new Shield(player);
                                              player.Mobility = new BoostDrive(player);

                                              switch(player.Class)
                                              {
                                                  case Class.None:
                                                      break;
                                                  case Class.Defense:
                                                      player.PrimaryA = new Jet(player);
                                                      player.PrimaryB = new SandCharge(player);

                                                      Storage.Game.GameMap.RedSpawn.X = (player.Team == Team.Red) ? 100 : 1100;
                                                      Storage.Game.GameMap.RedSpawn.Y = 400;
                                                      break;
                                                  case Class.Offense:
                                                      player.PrimaryA = new Laser(player);
                                                      player.PrimaryB = new FlameCharge(player);

                                                      Storage.Game.GameMap.RedSpawn.X = (player.Team == Team.Red) ? 100 : 1100;
                                                      Storage.Game.GameMap.RedSpawn.Y = 600;
                                                      break;
                                                  case Class.Support:
                                                      player.PrimaryA = new Plow(player);
                                                      player.PrimaryB = new PressureCharge(player);

                                                      Storage.Game.GameMap.RedSpawn.X = (player.Team == Team.Red) ? 100 : 1100;
                                                      Storage.Game.GameMap.RedSpawn.Y = 800;
                                                      break;
                                                  default:
                                                      throw new ArgumentOutOfRangeException();
                                              }

                                              Storage.Game.GameMap.BlueSpawn.X = Storage.Game.GameMap.RedSpawn.X;
                                              Storage.Game.GameMap.BlueSpawn.Y = Storage.Game.GameMap.RedSpawn.Y;
                                          }

                                          Game.TransitionState(States.ReadyWait);
                                      }, null);

            Game.Components.Add(_sandLogo);
            Game.Components.Add(_readyButton);
            Game.Components.Add(_mapChooser);
            Game.Components.Add(_tutorialButton);
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
            Game.Components.Remove(_tutorialButton);

            var returns = new Dictionary<string, object>();
            returns["SandLogo"] = _sandLogo;

            return returns;
        }
    }
}