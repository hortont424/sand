using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Xna.Framework;

namespace Sand.GameState
{
    public class PlayState : GameState
    {
        private Crosshair _crosshair;
        private ToolIcon _primaryAIcon, _primaryBIcon;
        private SandMeter _redSandMeter, _blueSandMeter;

        public PlayState(Sand game) : base(game)
        {
        }

        public override void Enter(Dictionary<string, object> data)
        {
            Game.Phase = GamePhases.Phase1;

            Cursor.Hide();
            _crosshair = new Crosshair(Game);
            Game.Components.Add(_crosshair);

            foreach(var gamer in Storage.NetworkSession.AllGamers)
            {
                Game.Components.Add((Player)gamer.Tag);
            }

            Game.GameMap = new Map(Game, "02");

            Game.Components.Add(Game.GameMap);

            var localPlayer = Storage.NetworkSession.LocalGamers[0].Tag as LocalPlayer;

            if(localPlayer != null)
            {
                var centerSidebar = (Game.GameMap.Width + Game.BaseScreenSize.X) / 2.0f;

                var mobilityIcon = new ToolIcon(Game, localPlayer.Mobility)
                                   {
                                       Position = new Vector2(centerSidebar - 20.0f - 148.0f, 10.0f + 148.0f)
                                   };
                Game.Components.Add(mobilityIcon);

                var weaponIcon = new ToolIcon(Game, localPlayer.Weapon)
                                 {
                                     Position = new Vector2(centerSidebar, 10.0f + 148.0f)
                                 };
                Game.Components.Add(weaponIcon);

                var utilityIcon = new ToolIcon(Game, localPlayer.Utility)
                                  {
                                      Position = new Vector2(centerSidebar + 20.0f + 148.0f, 10.0f + 148.0f)
                                  };
                Game.Components.Add(utilityIcon);

                if(localPlayer.PrimaryA != null)
                {
                    _primaryAIcon = new ToolIcon(Game, localPlayer.PrimaryA)
                                    {
                                        Position = new Vector2(centerSidebar - 10.0f - 74.0f, (10.0f + 148.0f) * 2.0f)
                                    };
                    Game.Components.Add(_primaryAIcon);
                }

                if(localPlayer.PrimaryB != null)
                {
                    _primaryBIcon = new ToolIcon(Game, localPlayer.PrimaryB)
                                    {
                                        Position = new Vector2(centerSidebar + 20.0f + 74.0f, (10.0f + 148.0f) * 2.0f)
                                    };
                    Game.Components.Add(_primaryBIcon);
                }

                var nameLabel = new Label(Game, centerSidebar, 2.0f,
                                          localPlayer.Gamer.Gamertag, "Calibri48Bold")
                                {
                                    PositionGravity =
                                        new Tuple<Gravity.Vertical, Gravity.Horizontal>(Gravity.Vertical.Top,
                                                                                        Gravity.Horizontal.Center)
                                };
                Game.Components.Add(nameLabel);
            }

            Game.Components.Add(Storage.SandParticles);

            _redSandMeter = new SandMeter(Game, Team.Red)
                            {
                                X = _primaryAIcon.Position.X,
                                Y = _primaryAIcon.Position.Y + 450
                            };
            _blueSandMeter = new SandMeter(Game, Team.Blue)
                             {
                                 X = _primaryBIcon.Position.X,
                                 Y = _primaryBIcon.Position.Y + 450
                             };

            Game.Components.Add(_redSandMeter);
            Game.Components.Add(_blueSandMeter);
        }

        public override void Update()
        {
            Messages.Update();

            var localPlayer = Storage.NetworkSession.LocalGamers[0].Tag as LocalPlayer;

            if(localPlayer != null)
            {
                _primaryAIcon.Disabled = !(localPlayer.CurrentPrimary == localPlayer.PrimaryA);
                _primaryBIcon.Disabled = !(localPlayer.CurrentPrimary == localPlayer.PrimaryB);
            }

            if(Game.Phase == GamePhases.Phase1)
            {
                int redCount = 0, blueCount = 0;

                foreach(var particle in Storage.SandParticles.Particles)
                {
                    if(particle.Value.Team == Team.Red)
                    {
                        redCount++;
                    }
                    else if(particle.Value.Team == Team.Blue)
                    {
                        blueCount++;
                    }
                }

                _redSandMeter.Progress = (redCount / 3000.0f);
                _blueSandMeter.Progress = (blueCount / 3000.0f);

                if(_redSandMeter.Progress == 1.0f)
                {
                    WinPhase1(Team.Red);
                }
                else if(_blueSandMeter.Progress == 1.0f)
                {
                    WinPhase1(Team.Blue);
                }
            }
        }

        private void WinPhase1(Team team)
        {
            Game.Phase = GamePhases.WonPhase1;
        }

        public override Dictionary<string, object> Leave()
        {
            var sandGame = Game;

            Cursor.Show();
            Game.Components.Remove(_crosshair);

            foreach(var gamer in Storage.NetworkSession.AllGamers)
            {
                Game.Components.Remove((Player)gamer.Tag);
            }

            Game.Components.Remove(sandGame.GameMap);
            Game.Components.Remove(Storage.SandParticles);

            Game.Components.Remove(_redSandMeter);
            Game.Components.Remove(_blueSandMeter);

            return null;
        }
    }
}