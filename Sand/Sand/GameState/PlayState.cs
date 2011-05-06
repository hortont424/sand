using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Input;
using Sand.Tools;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Sand.GameState
{
    public class PlayState : GameState
    {
        private Crosshair _crosshair;
        private ToolIcon _primaryAIcon, _primaryBIcon;
        private SandMeter _redSandMeter, _blueSandMeter;
        private WinDialog _winDialog;
        private Label _phase2Timer;
        private Team _teamWonPhase1;
        private ToolIcon _weaponIcon;
        private Countdown _countdownTimer;
        private KeyboardState _oldKeyState;
        private ToolIcon _mobilityIcon;
        private ToolIcon _utilityIcon;
        private Label _nameLabel;
        private ScoreMeter _redScoreMeter, _blueScoreMeter;

        public PlayState(Sand game) : base(game)
        {
        }

        private ToolIcon AddToolIconForTool(Tool tool, float x, float y)
        {
            if(tool == null)
            {
                return null;
            }

            var icon = new ToolIcon(Game, tool) { X = x, Y = y };

            Game.Components.Add(icon);

            return icon;
        }

        public override void Enter(Dictionary<string, object> data)
        {
            var localPlayer = Storage.NetworkSession.LocalGamers[0].Tag as LocalPlayer;

            if(localPlayer != null)
            {
                localPlayer.Phase = GamePhases.Phase1;
            }

            Cursor.Hide();
            _crosshair = new Crosshair(Game);
            Game.Components.Add(_crosshair);

            foreach(var gamer in Storage.NetworkSession.AllGamers)
            {
                Game.Components.Add((Player)gamer.Tag);
            }

            Game.Components.Add(Game.GameMap);

            if(localPlayer != null)
            {
                var centerSidebar = (Game.GameMap.Width + Game.BaseScreenSize.X) / 2.0f;

                _mobilityIcon = AddToolIconForTool(localPlayer.Mobility, centerSidebar - 20.0f - 148.0f, 10.0f + 148.0f);
                _weaponIcon = AddToolIconForTool(localPlayer.Weapon, centerSidebar, 10.0f + 148.0f);
                _utilityIcon = AddToolIconForTool(localPlayer.Utility, centerSidebar + 20.0f + 148.0f, 10.0f + 148.0f);
                _primaryAIcon = AddToolIconForTool(localPlayer.PrimaryA, centerSidebar - 10.0f - 74.0f,
                                                   (10.0f + 148.0f) * 2.0f);
                _primaryBIcon = AddToolIconForTool(localPlayer.PrimaryB, centerSidebar + 20.0f + 74.0f,
                                                   (10.0f + 148.0f) * 2.0f);

                if(_primaryBIcon != null)
                {
                    _primaryBIcon.Disabled = true;
                }

                _nameLabel = new Label(Game, centerSidebar, 2.0f,
                                       localPlayer.Gamer.Gamertag, "Calibri48Bold")
                             {
                                 PositionGravity =
                                     new Tuple<Gravity.Vertical, Gravity.Horizontal>(Gravity.Vertical.Top,
                                                                                     Gravity.Horizontal.Center)
                             };
                Game.Components.Add(_nameLabel);
            }

            Game.Components.Add(Storage.SandParticles);

            _redSandMeter = new SandMeter(Game, Team.Red)
                            {
                                X = _primaryAIcon.X,
                                Y = _primaryAIcon.Y + 430
                            };
            _blueSandMeter = new SandMeter(Game, Team.Blue)
                             {
                                 X = _primaryBIcon.X,
                                 Y = _primaryBIcon.Y + 430
                             };

            Game.Components.Add(_redSandMeter);
            Game.Components.Add(_blueSandMeter);

            _redScoreMeter = new ScoreMeter(Game, Team.Red)
                             {
                                 X = _redSandMeter.X,
                                 Y = _redSandMeter.Y + (_redSandMeter.Height / 2.0f) + 85
                             };
            _blueScoreMeter = new ScoreMeter(Game, Team.Blue)
                              {
                                  X = _blueSandMeter.X,
                                  Y = _blueSandMeter.Y + (_blueSandMeter.Height / 2.0f) + 85
                              };

            Game.Components.Add(_redScoreMeter);
            Game.Components.Add(_blueScoreMeter);

            StartTimer();
        }

        private void StartTimer()
        {
            Storage.ReadyToPlay = false;

            if(_countdownTimer != null)
            {
                Game.Components.Remove(_countdownTimer);
            }

            _countdownTimer = new Countdown(Game)
                              {
                                  X = Game.GameMap.Width / 2,
                                  Y = Storage.Game.BaseScreenSize.Y / 2
                              };
            Game.Components.Add(_countdownTimer);
            _countdownTimer.Start(5, 1000, () =>
                                           {
                                               Storage.ReadyToPlay = true;
                                               Game.Components.Remove(_countdownTimer);
                                               _countdownTimer = null;
                                           });
        }

        public override void Update()
        {
            Messages.Update();

            var localPlayer = Storage.NetworkSession.LocalGamers[0].Tag as LocalPlayer;

            if(localPlayer == null)
            {
                return;
            }

            _primaryAIcon.Disabled = localPlayer.CurrentPrimary != localPlayer.PrimaryA;
            _primaryBIcon.Disabled = localPlayer.CurrentPrimary != localPlayer.PrimaryB;

            if(localPlayer.Phase == GamePhases.Phase1 ||
               localPlayer.Phase == GamePhases.WonPhase1)
            {
                UpdateSandMeter();
            }

            if(Storage.NetworkSession.IsHost && localPlayer.Phase == GamePhases.Phase1)
            {
                CheckWinPhase1();
            }

            if(localPlayer.Phase == GamePhases.Phase2)
            {
                UpdatePhase2Timer();
            }

            if(Storage.NetworkSession.IsHost && localPlayer.Phase == GamePhases.Phase2)
            {
                CheckWinPhase2();
            }

            if(localPlayer.Phase == GamePhases.Done)
            {
                WaitForAllToFinish();
            }

            if(Storage.DebugMode)
            {
                _oldKeyState = Keyboard.GetState();
            }
        }

        private void WaitForAllToFinish()
        {
            var localPlayer = Storage.NetworkSession.LocalGamers[0].Tag as LocalPlayer;

            if(localPlayer == null)
            {
                return;
            }

            var anyNotWaiting = false;

            foreach(var gamer in Storage.NetworkSession.AllGamers)
            {
                var phase = (gamer.Tag as Player).Phase;

                if(phase != GamePhases.Done && phase != GamePhases.Phase1)
                {
                    anyNotWaiting = true;
                }
            }

            if(!anyNotWaiting)
            {
                if(_primaryAIcon != null)
                {
                    Game.Components.Add(_primaryAIcon);
                }

                if(_primaryBIcon != null)
                {
                    Game.Components.Add(_primaryBIcon);
                }

                Game.Components.Add(_redSandMeter);
                Game.Components.Add(_blueSandMeter);

                localPlayer.Phase = GamePhases.Phase1;
                Cursor.Hide();

                if(_winDialog != null)
                {
                    Game.Components.Remove(_winDialog);
                    _winDialog = null;
                }

                StartTimer();

                foreach(var particle in Storage.SandParticles.Particles)
                {
                    particle.Value.OnFire = true;
                    particle.Value.Fire = (byte)Storage.Random.Next(0, 255);
                }
            }
        }

        private void UpdatePhase2Timer()
        {
            var localPlayer = Storage.NetworkSession.LocalGamers[0].Tag as LocalPlayer;

            if(localPlayer == null)
            {
                return;
            }

            if(localPlayer.Gamer.IsHost)
            {
                Storage.RemainingTime =
                    new TimeSpan(Storage.Phase2EndTime.Ticks - Storage.CurrentTime.TotalGameTime.Ticks);
            }

            _phase2Timer.Text = string.Format("{0}", Storage.RemainingTime.Seconds);
        }

        private void UpdateSandMeter()
        {
            int redCount = 0, blueCount = 0;

            foreach(var particle in Storage.SandParticles.Particles)
            {
                if(particle.Value.Alive && !particle.Value.OnFire && particle.Value.Team == Team.Red)
                {
                    redCount++;
                }
                else if(particle.Value.Alive && !particle.Value.OnFire && particle.Value.Team == Team.Blue)
                {
                    blueCount++;
                }
            }

            _redSandMeter.Progress = (redCount / 1000.0f);
            _blueSandMeter.Progress = (blueCount / 1000.0f);
        }

        public void CheckWinPhase1()
        {
            var localPlayer = Storage.NetworkSession.LocalGamers[0].Tag as LocalPlayer;
            var keyState = Keyboard.GetState();

            if(localPlayer == null)
            {
                return;
            }

            if (_redSandMeter.Progress == 1.0f || (Storage.DebugMode && keyState.IsKeyDown(Keys.D1) && _oldKeyState.IsKeyUp(Keys.D1)))
            {
                WinPhase1(Team.Red);
                Messages.SendChangeWinStateMessage(localPlayer, GamePhases.WonPhase1, Team.Red,
                                                   localPlayer.Gamer.Id, true);
            }
            else if (_blueSandMeter.Progress == 1.0f || (Storage.DebugMode && keyState.IsKeyDown(Keys.D2) && _oldKeyState.IsKeyUp(Keys.D2)))
            {
                WinPhase1(Team.Blue);
                Messages.SendChangeWinStateMessage(localPlayer, GamePhases.WonPhase1, Team.Blue,
                                                   localPlayer.Gamer.Id, true);
            }
        }

        public void WinPhase1(Team team)
        {
            var localPlayer = Storage.NetworkSession.LocalGamers[0].Tag as LocalPlayer;

            _teamWonPhase1 = team;

            Cursor.Show();

            localPlayer.Phase = GamePhases.Phase2;
            Storage.Game.GameMap.WinPulse(team, null);

            localPlayer.Phase = GamePhases.Phase2;

            _phase2Timer = new Label(Game, _weaponIcon.X, _primaryAIcon.Y + 30, "", "Calibri120Bold")
                           { PositionGravity = Gravity.Center };
            Game.Components.Add(_phase2Timer);

            if(_primaryAIcon != null)
            {
                Game.Components.Remove(_primaryAIcon);
            }

            if(_primaryBIcon != null)
            {
                Game.Components.Remove(_primaryBIcon);
            }

            Game.Components.Remove(_redSandMeter);
            Game.Components.Remove(_blueSandMeter);

            if(localPlayer.Gamer.IsHost)
            {
                Storage.RemainingTime = new TimeSpan(0, 0, 1, 0);
                Storage.Phase2EndTime = Storage.CurrentTime.TotalGameTime + Storage.RemainingTime;

                Storage.Scores[team]++;

                if (localPlayer != null)
                {
                    Messages.SendUpdateScoreMessage(localPlayer, localPlayer.Gamer.Id, true);
                }

                if (Storage.Scores[team] == 6)
                {
                    Storage.NetworkSession.EndGame();
                    return;
                }
            }

            Cursor.Hide();
        }

        public void CheckWinPhase2()
        {
            var localPlayer = Storage.NetworkSession.LocalGamers[0].Tag as LocalPlayer;
            var keyState = Keyboard.GetState();

            var anyRedNotStunned = false;
            var anyBlueNotStunned = false;

            var foundRed = false;
            var foundBlue = false;

            foreach(var gamer in Storage.NetworkSession.AllGamers)
            {
                var player = (gamer.Tag as Player);

                switch(player.Team)
                {
                    case Team.None:
                        continue;
                    case Team.Red:
                        foundRed = true;
                        anyRedNotStunned = (!player.Stunned) ? true : anyRedNotStunned;
                        break;
                    case Team.Blue:
                        foundBlue = true;
                        anyBlueNotStunned = (!player.Stunned) ? true : anyBlueNotStunned;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (!anyRedNotStunned && foundRed || (Storage.DebugMode && keyState.IsKeyDown(Keys.D2) && _oldKeyState.IsKeyUp(Keys.D2)))
            {
                WinPhase2(Team.Blue);
                Messages.SendChangeWinStateMessage(localPlayer, GamePhases.WonPhase2, Team.Blue,
                                                   localPlayer.Gamer.Id, true);
            }
            else if (!anyBlueNotStunned && foundBlue || (Storage.DebugMode && keyState.IsKeyDown(Keys.D1) && _oldKeyState.IsKeyUp(Keys.D1)))
            {
                WinPhase2(Team.Red);
                Messages.SendChangeWinStateMessage(localPlayer, GamePhases.WonPhase2, Team.Red,
                                                   localPlayer.Gamer.Id, true);
            }
            else if(Storage.RemainingTime.Ticks <= 0)
            {
                WinPhase2(Team.None);
                Messages.SendChangeWinStateMessage(localPlayer, GamePhases.WonPhase2, _teamWonPhase1,
                                                   localPlayer.Gamer.Id, true);
            }
        }

        public void WinPhase2(Team team)
        {
            var localPlayer = Storage.NetworkSession.LocalGamers[0].Tag as LocalPlayer;

            Cursor.Show();
            Storage.Game.GameMap.EndWinPulse();

            if(_phase2Timer != null)
            {
                Game.Components.Remove(_phase2Timer);
            }

            if(team != Team.None && _teamWonPhase1 == team && Storage.NetworkSession.IsHost)
            {
                Storage.Scores[team]++;

                if(localPlayer != null)
                {
                    Messages.SendUpdateScoreMessage(localPlayer, localPlayer.Gamer.Id, true);
                }

                if(Storage.Scores[team] == 6)
                {
                    Storage.NetworkSession.EndGame();
                    return;
                }
            }

            localPlayer.Phase = GamePhases.Done;
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

            Game.Components.Remove(_redScoreMeter);
            Game.Components.Remove(_blueScoreMeter);

            Game.Components.Remove(_mobilityIcon);
            Game.Components.Remove(_utilityIcon);
            Game.Components.Remove(_weaponIcon);
            Game.Components.Remove(_primaryAIcon);
            Game.Components.Remove(_primaryBIcon);

            Game.Components.Remove(_nameLabel);

            if(_phase2Timer != null)
            {
                Game.Components.Remove(_phase2Timer);
            }

            if(_winDialog != null)
            {
                Game.Components.Remove(_winDialog);
            }

            return null;
        }
    }
}