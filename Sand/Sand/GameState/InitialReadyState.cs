using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Input;

namespace Sand.GameState
{
    public class InitialReadyState : GameState
    {
        private Billboard _sandLogo;
        private Label _readyLabel;
        private Label _serverLabel;
        private Label _versionLabel;
        private Label _debugLabel;
        private KeyboardState _oldKeyState;

        public InitialReadyState(Sand game) : base(game)
        {
        }

        public override void Enter(Dictionary<string, object> data)
        {
            if(Storage.NetworkSession.IsHost)
            {
                Storage.IntroMusic.Play();
                Storage.InMenuMusic = true;
                Storage.AnimationController.Add(new Animation
                                                {
                                                    CompletedDelegate = () =>
                                                                        {
                                                                            if(Storage.InMenuMusic &&
                                                                               Storage.IntroMusic.State ==
                                                                               SoundState.Stopped)
                                                                            {
                                                                                Storage.LoopMusic.Play();
                                                                            }
                                                                        }
                                                }, 1, true);
            }

            var logoSprite = Storage.Sprite("SandLogo");
            var sandLogoOrigin = new Vector2(Game.BaseScreenSize.X * 0.5f - (logoSprite.Width * 0.5f), 300);

            _sandLogo = new Billboard(Game, sandLogoOrigin, logoSprite);
            _readyLabel = new Label(Game, Game.BaseScreenSize.X * 0.5f, Game.BaseScreenSize.Y * 0.5f,
                                    "Click to Begin", "Calibri48Bold") { PositionGravity = Gravity.Center };
            _serverLabel = new Label(Game, Game.BaseScreenSize.X - 15.0f, 10.0f,
                                     "", "Calibri24Bold")
                           {
                               PositionGravity =
                                   new Tuple<Gravity.Vertical, Gravity.Horizontal>(Gravity.Vertical.Top,
                                                                                   Gravity.Horizontal.Right)
                           };
            _versionLabel = new Label(Game, Game.BaseScreenSize.X - 15.0f, Game.BaseScreenSize.Y - 10.0f,
                                      Assembly.GetExecutingAssembly().GetName().Version.ToString(), "Calibri24Bold")
                            {
                                PositionGravity =
                                    new Tuple<Gravity.Vertical, Gravity.Horizontal>(Gravity.Vertical.Bottom,
                                                                                    Gravity.Horizontal.Right)
                            };
            _debugLabel = new Label(Game, 15.0f, Game.BaseScreenSize.Y - 10.0f,
                                    "", "Calibri24Bold")
                          {
                              PositionGravity =
                                  new Tuple<Gravity.Vertical, Gravity.Horizontal>(Gravity.Vertical.Bottom,
                                                                                  Gravity.Horizontal.Left),
                              Color = Color.Red
                          };

            Game.Components.Add(_sandLogo);
            Game.Components.Add(_serverLabel);
            Game.Components.Add(_versionLabel);
            Game.Components.Add(_debugLabel);
            Game.Components.Add(_readyLabel);
        }

        public override void Update()
        {
            MouseState mouse = Mouse.GetState();
            KeyboardState keyState = Keyboard.GetState();

            if(!Storage.AcceptInput)
            {
                _oldKeyState = keyState;
                return;
            }

            if(mouse.LeftButton == ButtonState.Pressed && !Guide.IsVisible)
            {
                Game.TransitionState(States.Lobby);
            }

            if(keyState.IsKeyDown(Keys.OemTilde) && _oldKeyState.IsKeyUp(Keys.OemTilde))
            {
                Storage.DebugMode = !Storage.DebugMode;
            }

            _serverLabel.Text = Storage.NetworkSession.IsHost ? "Server" : "";
            _debugLabel.Text = Storage.DebugMode ? "Debug" : "";

            _oldKeyState = keyState;
        }

        public override Dictionary<string, object> Leave()
        {
            Game.Components.Remove(_sandLogo);
            Game.Components.Remove(_serverLabel);
            Game.Components.Remove(_versionLabel);
            Game.Components.Remove(_readyLabel);

            return new Dictionary<string, object> { { "SandLogo", _sandLogo } };
        }
    }
}