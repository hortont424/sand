using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;

namespace Sand
{
    public class Sand : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private LocalPlayer _player;
        private NetworkSession _networkSession;
        private KeyboardState _oldKeyState;
        private GameState _gameState;
        private GameState _oldGameState;

        private enum GameState
        {
            Begin,
            Login,
            Lobby
        } ;

        // E27 white rice wonton soup ("I'll go with the boned")
        // E16 chicken fried rice wonton soup

        public Sand()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            _player = new LocalPlayer(this);
            Components.Add(_player);

            _networkSession = null;

            _gameState = GameState.Begin;

            Components.Add(new GamerServicesComponent(this));
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        private void TransitionState(GameState newState)
        {
            switch(_gameState)
            {
                case GameState.Begin:
                    break;
                case GameState.Login:
                    break;
                case GameState.Lobby:
                    break;
                default:
                    break;
            }

            _gameState = newState;

            switch(_gameState)
            {
                case GameState.Begin:
                    break;
                case GameState.Login:
                    DoLogin();
                    break;
                case GameState.Lobby:
                    break;
                default:
                    break;
            }
        }

        private void DoLogin()
        {
            if(!Guide.IsVisible)
            {
                SignedInGamer.SignedIn += new EventHandler<SignedInEventArgs>(UserReady);
                Guide.ShowSignIn(1, false);
            }
        }

        private void UserReady(Object sender, SignedInEventArgs eventArgs)
        {
            TransitionState(GameState.Lobby);
        }

        protected override void Update(GameTime gameTime)
        {
            UpdateInput();
            UpdateState();

            if(_gameState > GameState.Login)
            {
                if(_networkSession == null)
                {
                    // Try to find a Sand server. If there isn't one, start one!

                    var availableSessions = NetworkSession.Find(NetworkSessionType.SystemLink, 1, null);

                    if(availableSessions.Count > 0)
                    {
                        _networkSession = NetworkSession.Join(availableSessions[0]);
                    }
                    else
                    {
                        Console.WriteLine("Couldn't find a server!");
                    }
                }

                if(_networkSession != null)
                {
                    _networkSession.Update();
                }
            }

            // Allows the game to exit
            if(GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                Exit();
            }

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        private void UpdateInput()
        {
            KeyboardState newKeyState = Keyboard.GetState();

            _oldKeyState = newKeyState;
        }

        private void UpdateState()
        {
            if(_gameState == GameState.Begin)
            {
                TransitionState(GameState.Login);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}