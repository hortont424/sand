using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Audio;
using Sand.Tools;

namespace Sand.GameState
{
    public class TutorialState : GameState
    {
        private Crosshair _crosshair;
        private ToolIcon _primaryAIcon, _primaryBIcon;
        private ToolIcon _weaponIcon;
        private Countdown _countdownTimer;
        private ToolIcon _mobilityIcon;
        private ToolIcon _utilityIcon;
        private Label _nameLabel;
        private SoundEffectInstance _tutorialSound;
        private long _tutorialStartTime;
        private Billboard _keyBindingIcon;

        public TutorialState(Sand game)
            : base(game)
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

            _tutorialSound = Storage.Sound("Tutorial").CreateInstance();
            Storage.InTutorial = true;
            Storage.TutorialLevel = 0;

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

                _keyBindingIcon = new Billboard(Game, (int)centerSidebar, 400, Storage.Sprite("blank"))
                                  { PositionGravity = Gravity.Center };
                Game.Components.Add(_keyBindingIcon);

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
            _countdownTimer.Start(3, 1000, () =>
                                           {
                                               Storage.ReadyToPlay = true;
                                               Game.Components.Remove(_countdownTimer);

                                               if(Storage.NetworkSession.IsHost)
                                               {
                                                   _tutorialSound.Play();
                                                   _tutorialStartTime = Storage.CurrentTime.TotalGameTime.Ticks;
                                               }

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

            var currentTime = new TimeSpan(Storage.CurrentTime.TotalGameTime.Ticks - _tutorialStartTime).TotalSeconds;

            if(Storage.NetworkSession.IsHost && ((Storage.TutorialLevel == 0 && currentTime >= 26) ||
                                                 (Storage.TutorialLevel == 1 && currentTime >= 33) ||
                                                 (Storage.TutorialLevel == 2 && currentTime >= 60 + 4) ||
                                                 (Storage.TutorialLevel == 3 && currentTime >= 60 + 14) ||
                                                 (Storage.TutorialLevel == 4 && currentTime >= 60 + 51) ||
                                                 (Storage.TutorialLevel == 5 && currentTime >= 60 + 60 + 11) ||
                                                 (Storage.TutorialLevel == 6 && currentTime >= 60 + 60 + 60 + 1) ||
                                                 (Storage.TutorialLevel == 7 && currentTime >= 60 + 60 + 60 + 11) ||
                                                 (Storage.TutorialLevel == 8 && currentTime >= 60 + 60 + 60 + 60) ||
                                                 (Storage.TutorialLevel == 9 && currentTime >= 60 + 60 + 60 + 60 + 10) ||
                                                 (Storage.TutorialLevel == 10 && currentTime >= 60 + 60 + 60 + 60 + 37) ||
                                                 (Storage.TutorialLevel == 11 && currentTime >= 60 + 60 + 60 + 60 + 47)))
            {
                Storage.TutorialLevel++;
                Messages.SendChangeTutorialLevelMessage(localPlayer, localPlayer.Gamer.Id, true);
            }

            _keyBindingIcon.Texture = Storage.Sprite("blank");

            switch(Storage.TutorialLevel)
            {
                case 2:
                    _keyBindingIcon.Texture = Storage.Sprite("wasd");
                    break;
                case 3:
                    _keyBindingIcon.Texture = Storage.Sprite("space");
                    break;
                case 5:
                    _keyBindingIcon.Texture = Storage.Sprite("shiftright");
                    break;
                case 7:
                case 9:
                case 11:
                    _keyBindingIcon.Texture = Storage.Sprite("lmouse"); // and q
                    break;
            }

            if(Storage.NetworkSession.IsHost && _tutorialSound.State == SoundState.Stopped && Storage.TutorialLevel == 12)
            {
                Storage.NetworkSession.EndGame();
            }
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

            Game.Components.Remove(_mobilityIcon);
            Game.Components.Remove(_utilityIcon);
            Game.Components.Remove(_weaponIcon);
            Game.Components.Remove(_primaryAIcon);
            Game.Components.Remove(_primaryBIcon);
            Game.Components.Remove(_keyBindingIcon);

            Game.Components.Remove(_nameLabel);

            return null;
        }
    }
}