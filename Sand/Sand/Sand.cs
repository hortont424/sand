using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Sand.GameState;

namespace Sand
{
    public class Sand : Game
    {
        private GraphicsDeviceManager _graphics;
        public SpriteBatch SpriteBatch;

        private KeyboardState _oldKeyState;
        private States _gameState;
        private Dictionary<States, GameState.GameState> _gameStateInstances;

        public Vector2 BaseScreenSize;
        private Matrix GlobalTransformMatrix;
        public Vector2 MouseLocation;

        public Map GameMap;

        // E27 white rice wonton soup ("I'll go with the boned")
        // E16 chicken fried rice wonton soup

        public Sand()
        {
            BaseScreenSize = new Vector2(1920, 1200);

            _graphics = new GraphicsDeviceManager(this)
                        {
                            PreferredBackBufferWidth = 1440,
                            // TODO: these should be figured out dynamically
                            PreferredBackBufferHeight = 900,
                            GraphicsProfile = GraphicsProfile.Reach,
                            PreferMultiSampling = true
                        };

            // TODO: when going fullscreen, determine the size of the screen!
            //_graphics.ToggleFullScreen();

            Content.RootDirectory = "Content";

            IsMouseVisible = true;

            _gameState = States.Begin;
            _gameStateInstances = new Dictionary<States, GameState.GameState>();
            _gameStateInstances[States.Begin] = new BeginState(this);
            _gameStateInstances[States.Login] = new LoginState(this);
            _gameStateInstances[States.AcquireSession] = new AcquireSessionState(this);
            _gameStateInstances[States.InitialReady] = new InitialReadyState(this);
            _gameStateInstances[States.Lobby] = new LobbyState(this);
            _gameStateInstances[States.ReadyWait] = new ReadyWaitState(this);
            _gameStateInstances[States.Play] = new PlayState(this);

            Components.Add(new GamerServicesComponent(this));

            Storage.animationController = new AnimationController(this);
            Components.Add(Storage.animationController);
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            float horScaling = GraphicsDevice.PresentationParameters.BackBufferWidth / BaseScreenSize.X;
            float verScaling = GraphicsDevice.PresentationParameters.BackBufferHeight / BaseScreenSize.Y;
            var screenScalingFactor = new Vector3(horScaling, verScaling, 1);
            GlobalTransformMatrix = Matrix.CreateScale(screenScalingFactor);

            Storage.AddFont("Calibri24", Content.Load<SpriteFont>("Fonts/Calibri24"));
            Storage.AddFont("Calibri48Bold", Content.Load<SpriteFont>("Fonts/Calibri48Bold"));
            Storage.AddFont("Gotham24", Content.Load<SpriteFont>("Fonts/Gotham24"));

            Storage.AddColor("WidgetFill", new Color(0.1f, 0.5f, 0.1f));
            Storage.AddColor("RedTeam", new Color(0.65f, 0.1f, 0.1f));
            Storage.AddColor("BlueTeam", new Color(0.1f, 0.37f, 0.65f));
            Storage.AddColor("NeutralTeam", new Color(0.6f, 0.6f, 0.6f));

            Storage.AddSprite("SandLogo", Content.Load<Texture2D>("Textures/Menu/sand"));
            Storage.AddSprite("player", Content.Load<Texture2D>("Textures/player"));

            var rectTexture = new Texture2D(GraphicsDevice, 1, 1);
            rectTexture.SetData(new[] { Color.White });
            Storage.AddSprite("pixel", rectTexture);
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        public bool TransitionState(States newState)
        {
            Console.WriteLine("Moving from {0} to {1}", _gameState, newState);

            if(!_gameStateInstances[_gameState].CanLeave())
            {
                Console.WriteLine("Couldn't leave {0}", _gameState);
                return false;
            }

            if(!_gameStateInstances[newState].CanEnter())
            {
                Console.WriteLine("Couldn't enter {0}", newState);
                return false;
            }

            var data = _gameStateInstances[_gameState].Leave();

            _gameState = newState;

            _gameStateInstances[_gameState].Enter(data);

            return true;
        }

        protected override void Update(GameTime gameTime)
        {
            MouseState mouse = Mouse.GetState();
            MouseLocation = Vector2.Transform(new Vector2(mouse.X, mouse.Y),
                                              Matrix.Invert(GlobalTransformMatrix)); // TODO: cache inverse?

            UpdateInput();
            UpdateState();

            if(Storage.networkSession != null)
            {
                Storage.networkSession.Update();
            }

            base.Update(gameTime);
        }

        private void UpdateInput()
        {
            KeyboardState newKeyState = Keyboard.GetState();

            if(newKeyState.IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            _oldKeyState = newKeyState;
        }

        private void UpdateState()
        {
            if(_gameState == States.Begin)
            {
                TransitionState(States.Login);
            }

            _gameStateInstances[_gameState].Update();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, GlobalTransformMatrix);

            if(!Guide.IsVisible)
            {
                base.Draw(gameTime);
            }

            SpriteBatch.End();
        }
    }
}