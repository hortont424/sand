using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Sand.GameState;

namespace Sand
{
    public class Sand : Game
    {
        public GraphicsDeviceManager Graphics;
        public SpriteBatch SpriteBatch;

        private KeyboardState _oldKeyState;
        private States _gameState;
        private readonly Dictionary<States, GameState.GameState> _gameStateInstances;

        public Vector2 BaseScreenSize;
        private Matrix GlobalTransformMatrix;
        public Vector2 MouseLocation;

        public Map GameMap;

        // E27 white rice wonton soup ("I'll go with the boned")
        // E16 chicken fried rice wonton soup

        public Sand()
        {
            BaseScreenSize = new Vector2(1920, 1200);
            IsFixedTimeStep = false;

            Graphics = new GraphicsDeviceManager(this)
                       {
                           PreferredBackBufferWidth = (int)(0.9 * GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width),
                           PreferredBackBufferHeight = (int)(0.9 * GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height),
                           GraphicsProfile = GraphicsProfile.Reach,
                           PreferMultiSampling = true,
                           PreferredDepthStencilFormat = DepthFormat.None
                       };

            Graphics.PreparingDeviceSettings += new EventHandler<PreparingDeviceSettingsEventArgs>(GraphicsPreparingDeviceSettings);
            
            //Graphics.ToggleFullScreen();

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

            Components.ComponentAdded += ComponentAdded;
            Components.ComponentRemoved += ComponentRemoved;

            Components.Add(new GamerServicesComponent(this));

            Storage.AnimationController = new AnimationController(this);
            Components.Add(Storage.AnimationController);
        }

        void GraphicsPreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            if(e.GraphicsDeviceInformation.Adapter.DeviceId == 1065)
            {
                e.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = 1;
            }
        }

        private void ComponentAdded(object sender, GameComponentCollectionEventArgs e)
        {
            var actor = e.GameComponent as Actor;

            if(actor != null)
            {
                foreach(var child in actor.Children)
                {
                    Components.Add(child);
                }
            }
        }

        private void ComponentRemoved(object sender, GameComponentCollectionEventArgs e)
        {
            var actor = e.GameComponent as Actor;

            if(actor != null)
            {
                foreach(var child in actor.Children)
                {
                    Components.Remove(child);
                }
            }
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
            Storage.AddFont("Calibri24Bold", Content.Load<SpriteFont>("Fonts/Calibri24Bold"));
            Storage.AddFont("Calibri48Bold", Content.Load<SpriteFont>("Fonts/Calibri48Bold"));
            Storage.AddFont("Gotham24", Content.Load<SpriteFont>("Fonts/Gotham24"));

            Storage.AddColor("WidgetFill", new Color(0.1f, 0.5f, 0.1f));
            Storage.AddColor("RedTeam", new Color(0.760f, 0.207f, 1.0f));
            Storage.AddColor("BlueTeam", new Color(0.207f, 0.741f, 0.215f));
            Storage.AddColor("NeutralTeam", new Color(0.3f, 0.3f, 0.3f));

            Storage.AddSprite("SandLogo", Content.Load<Texture2D>("Textures/Menu/sand"));
            Storage.AddSprite("DefenseClass", Content.Load<Texture2D>("Textures/Classes/defense"));
            Storage.AddSprite("OffenseClass", Content.Load<Texture2D>("Textures/Classes/offense"));
            Storage.AddSprite("SupportClass", Content.Load<Texture2D>("Textures/Classes/support"));

            Storage.AddSprite("DefenseClassLarge", Content.Load<Texture2D>("Textures/Classes/DefenseLarge"));
            Storage.AddSprite("OffenseClassLarge", Content.Load<Texture2D>("Textures/Classes/OffenseLarge"));
            Storage.AddSprite("SupportClassLarge", Content.Load<Texture2D>("Textures/Classes/SupportLarge"));

            Storage.AddSprite("BoostDrive", Content.Load<Texture2D>("Textures/Tools/Mobilities/BoostDrive"));
            Storage.AddSound("BoostDrive_Start", Content.Load<SoundEffect>("Sounds/BoostDrive_Start"));
            Storage.AddSound("BoostDrive_Stop", Content.Load<SoundEffect>("Sounds/BoostDrive_Stop"));
            Storage.AddSound("BoostDrive_Engine", Content.Load<SoundEffect>("Sounds/BoostDrive_Engine"));

            Storage.AddSprite("Cannon", Content.Load<Texture2D>("Textures/Tools/Weapons/Cannon"));
            Storage.AddSound("Cannon", Content.Load<SoundEffect>("Sounds/Cannon"));
            Storage.AddSound("Shock", Content.Load<SoundEffect>("Sounds/Shock"));

            Storage.AddSprite("Shield", Content.Load<Texture2D>("Textures/Tools/Utilities/Shield"));
            Storage.AddSprite("ShieldCircle", Content.Load<Texture2D>("Textures/Tools/Utilities/ShieldCircle"));
            Storage.AddSound("Shield", Content.Load<SoundEffect>("Sounds/Shield"));

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
            Storage.CurrentTime = gameTime;
            Storage.AcceptInput = IsActive;

            MouseState mouse = Mouse.GetState();
            MouseLocation = Vector2.Transform(new Vector2(mouse.X, mouse.Y),
                                              Matrix.Invert(GlobalTransformMatrix)); // TODO: cache inverse?

            UpdateInput();
            UpdateState();

            if(Storage.NetworkSession != null)
            {
                Storage.NetworkSession.Update();
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