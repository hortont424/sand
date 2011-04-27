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
    public enum GamePhases
    {
        Phase1,
        WonPhase1,
        WaitForPhase2,
        Phase2,
        WonPhase2,
        Done
    }

    public class Sand : Game
    {
        public GraphicsDeviceManager Graphics;
        public SpriteBatch SpriteBatch;

        private KeyboardState _oldKeyState;
        private States _gameState;
        private readonly Dictionary<States, GameState.GameState> _gameStateInstances;

        public Vector2 BaseScreenSize;
        private Matrix _globalTransformMatrix, _invGlobalTransformMatrix;
        public Vector2 MouseLocation;

        public MapManager MapManager;
        public Map GameMap;

        // E27 white rice wonton soup ("I'll go with the boned")
        // E16 chicken fried rice wonton soup

        public Sand()
        {
            BaseScreenSize = new Vector2(1920, 1200);
            IsFixedTimeStep = false;

            Graphics = new GraphicsDeviceManager(this)
                       {
                           PreferredBackBufferWidth =
                               (int)(0.9 * GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width),
                           PreferredBackBufferHeight =
                               (int)(0.9 * GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height),
                           GraphicsProfile = GraphicsProfile.Reach,
                           PreferMultiSampling = true,
                           PreferredDepthStencilFormat = DepthFormat.None
                       };

            Graphics.PreparingDeviceSettings += GraphicsPreparingDeviceSettings;

            Content.RootDirectory = "Content";

            IsMouseVisible = true;

            _gameState = States.Begin;
            _gameStateInstances = new Dictionary<States, GameState.GameState>();
            _gameStateInstances[States.Begin] = new BeginState(this);
            _gameStateInstances[States.Login] = new LoginState(this);
            _gameStateInstances[States.AcquireSession] = new AcquireSessionState(this);
            _gameStateInstances[States.InitialReady] = new InitialReadyState(this);
            _gameStateInstances[States.Lobby] = new LobbyState(this);
            _gameStateInstances[States.Loadout] = new LoadoutState(this);
            _gameStateInstances[States.ReadyWait] = new ReadyWaitState(this);
            _gameStateInstances[States.Play] = new PlayState(this);
            _gameStateInstances[States.ChooseMap] = new ChooseMapState(this);

            Components.ComponentAdded += ComponentAdded;
            Components.ComponentRemoved += ComponentRemoved;

            Components.Add(new GamerServicesComponent(this));

            Storage.Game = this;
            Storage.AnimationController = new AnimationController(this);
            Components.Add(Storage.AnimationController);

            
        }

        private void GraphicsPreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
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

            ComputeTransform();

            Storage.AddFont("Calibri24", Content.Load<SpriteFont>("Fonts/Calibri24"));
            Storage.AddFont("Calibri24Bold", Content.Load<SpriteFont>("Fonts/Calibri24Bold"));
            Storage.AddFont("Calibri32", Content.Load<SpriteFont>("Fonts/Calibri32"));
            Storage.AddFont("Calibri32Bold", Content.Load<SpriteFont>("Fonts/Calibri32Bold"));
            Storage.AddFont("Calibri48Bold", Content.Load<SpriteFont>("Fonts/Calibri48Bold"));
            Storage.AddFont("Gotham24", Content.Load<SpriteFont>("Fonts/Gotham24"));

            Storage.AddColor("WidgetFill", new Color(0.1f, 0.5f, 0.1f));
            Storage.AddColor("RedTeam", new Color(0.760f, 0.207f, 1.0f));
            Storage.AddColor("BlueTeam", new Color(0.207f, 0.741f, 0.215f));
            Storage.AddColor("NeutralTeam", new Color(0.4f, 0.4f, 0.4f));

            Storage.AddSprite("Crosshair", Content.Load<Texture2D>("Textures/Crosshair"));

            Storage.AddSound("Fail", Content.Load<SoundEffect>("Sounds/fail"));

            Storage.AddSound("Drained", Content.Load<SoundEffect>("Sounds/Drained"));
            Storage.AddSound("SandBurning", Content.Load<SoundEffect>("Sounds/sand_burning_loop"));

            Storage.AddSprite("SandLogo", Content.Load<Texture2D>("Textures/Menu/sand"));
            Storage.AddSprite("DefenseClass", Content.Load<Texture2D>("Textures/Classes/defense"));
            Storage.AddSprite("OffenseClass", Content.Load<Texture2D>("Textures/Classes/offense"));
            Storage.AddSprite("SupportClass", Content.Load<Texture2D>("Textures/Classes/support"));

            Storage.AddSprite("DefenseClassLarge", Content.Load<Texture2D>("Textures/Classes/DefenseLarge"));
            Storage.AddSprite("OffenseClassLarge", Content.Load<Texture2D>("Textures/Classes/OffenseLarge"));
            Storage.AddSprite("SupportClassLarge", Content.Load<Texture2D>("Textures/Classes/SupportLarge"));

            Storage.AddSound("DefenseClass1", Content.Load<SoundEffect>("Sounds/Defense_1"));
            Storage.AddSound("OffenseClass1", Content.Load<SoundEffect>("Sounds/Offense_1"));
            Storage.AddSound("SupportClass1", Content.Load<SoundEffect>("Sounds/Support_1"));

            Storage.AddSound("DefenseClass2", Content.Load<SoundEffect>("Sounds/Defense_2"));
            Storage.AddSound("OffenseClass2", Content.Load<SoundEffect>("Sounds/Offense_2"));
            Storage.AddSound("SupportClass2", Content.Load<SoundEffect>("Sounds/Support_2"));

            Storage.AddSound("DefenseClass3", Content.Load<SoundEffect>("Sounds/Defense_3"));
            Storage.AddSound("OffenseClass3", Content.Load<SoundEffect>("Sounds/Offense_3"));
            Storage.AddSound("SupportClass3", Content.Load<SoundEffect>("Sounds/Support_3"));

            Storage.AddSprite("BlinkDrive", Content.Load<Texture2D>("Textures/Tools/Mobilities/BlinkDrive"));
            Storage.AddSound("BlinkDrive", Content.Load<SoundEffect>("Sounds/blinkdrive"));

            Storage.AddSprite("BoostDrive", Content.Load<Texture2D>("Textures/Tools/Mobilities/BoostDrive"));
            Storage.AddSound("BoostDrive_Start", Content.Load<SoundEffect>("Sounds/BoostDrive_Start"));
            Storage.AddSound("BoostDrive_Stop", Content.Load<SoundEffect>("Sounds/BoostDrive_Stop"));
            Storage.AddSound("BoostDrive_Engine", Content.Load<SoundEffect>("Sounds/BoostDrive_Engine"));

            Storage.AddSprite("WinkDrive", Content.Load<Texture2D>("Textures/Tools/Mobilities/WinkDrive"));
            Storage.AddSound("WinkDrive_Start", Content.Load<SoundEffect>("Sounds/WinkDrive_Start"));
            Storage.AddSound("WinkDrive_Stop", Content.Load<SoundEffect>("Sounds/WinkDrive_Stop"));

            Storage.AddSprite("Cannon", Content.Load<Texture2D>("Textures/Tools/Weapons/Cannon"));
            Storage.AddSound("Cannon", Content.Load<SoundEffect>("Sounds/Cannon"));
            Storage.AddSound("Shock", Content.Load<SoundEffect>("Sounds/Shock"));

            Storage.AddSprite("Shield", Content.Load<Texture2D>("Textures/Tools/Utilities/Shield"));
            Storage.AddSprite("ShieldCircle", Content.Load<Texture2D>("Textures/Tools/Utilities/ShieldCircle"));
            Storage.AddSprite("WhiteCircle", Content.Load<Texture2D>("Textures/Tools/Utilities/WhiteCircle"));
            Storage.AddSprite("ToolDot", Content.Load<Texture2D>("Textures/Tools/Utilities/ToolDot"));
            Storage.AddSound("Shield", Content.Load<SoundEffect>("Sounds/Shield"));

            Storage.AddSprite("Jet", Content.Load<Texture2D>("Textures/Tools/Primaries/Jet"));
            Storage.AddSound("Jet", Content.Load<SoundEffect>("Sounds/jet_drain"));

            Storage.AddSprite("Plow", Content.Load<Texture2D>("Textures/Tools/Primaries/Plow"));
            Storage.AddSound("Plow", Content.Load<SoundEffect>("Sounds/Plow"));

            Storage.AddSprite("Laser", Content.Load<Texture2D>("Textures/Tools/Primaries/Laser"));
            Storage.AddSound("Laser", Content.Load<SoundEffect>("Sounds/Laser"));

            Storage.AddSprite("SandCharge", Content.Load<Texture2D>("Textures/Tools/Primaries/SandCharge"));
            Storage.AddSound("SandCharge", Content.Load<SoundEffect>("Sounds/sandcharge"));
            Storage.AddSprite("FlameCharge", Content.Load<Texture2D>("Textures/Tools/Primaries/FlameCharge"));
            Storage.AddSound("FlameCharge", Content.Load<SoundEffect>("Sounds/FlameCharge"));
            Storage.AddSprite("PressureCharge", Content.Load<Texture2D>("Textures/Tools/Primaries/PressureCharge"));
            Storage.AddSound("PressureCharge", Content.Load<SoundEffect>("Sounds/PressureCharge"));

            Storage.AddSprite("EMP", Content.Load<Texture2D>("Textures/Tools/Weapons/EMP"));
            Storage.AddSound("EMP", Content.Load<SoundEffect>("Sounds/EMP"));

            Storage.AddSprite("Ground", Content.Load<Texture2D>("Textures/Tools/Utilities/Ground"));
            Storage.AddSound("Ground", Content.Load<SoundEffect>("Sounds/Ground"));

            var rectTexture = new Texture2D(GraphicsDevice, 1, 1);
            rectTexture.SetData(new[] { Color.White });
            Storage.AddSprite("pixel", rectTexture);

            MapManager = new MapManager(this);
        }

        private void ComputeTransform()
        {
            float horScaling = GraphicsDevice.PresentationParameters.BackBufferWidth / BaseScreenSize.X;
            float verScaling = GraphicsDevice.PresentationParameters.BackBufferHeight / BaseScreenSize.Y;
            var screenScalingFactor = new Vector3(horScaling, verScaling, 1);
            _globalTransformMatrix = Matrix.CreateScale(screenScalingFactor);
            _invGlobalTransformMatrix = Matrix.Invert(_globalTransformMatrix);
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

        public GameState.GameState CurrentState()
        {
            return _gameStateInstances[_gameState];
        }

        protected override void Update(GameTime gameTime)
        {
            Storage.CurrentTime = gameTime;
            Storage.AcceptInput = IsActive;

            MouseState mouse = Mouse.GetState();
            MouseLocation = Vector2.Transform(new Vector2(mouse.X, mouse.Y), _invGlobalTransformMatrix);

            UpdateInput();
            UpdateState();

            if(Storage.NetworkSession != null)
            {
                Storage.NetworkSession.Update();
            }

            Sounds.Update();

            base.Update(gameTime);
        }

        private void UpdateInput()
        {
            KeyboardState newKeyState = Keyboard.GetState();

            if(newKeyState.IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            if(newKeyState.IsKeyDown(Keys.F) && newKeyState.IsKeyDown(Keys.LeftControl) &&
               !_oldKeyState.IsKeyDown(Keys.F))
            {
                if(Graphics.IsFullScreen)
                {
                    // Going to non-fullscreen

                    Graphics.PreferredBackBufferWidth =
                        (int)(0.9f * GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width);
                    Graphics.PreferredBackBufferHeight =
                        (int)(0.9f * GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);
                }
                else
                {
                    // Going to fullscreen

                    Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                    Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                }

                Graphics.ToggleFullScreen();

                ComputeTransform();
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

            SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, _globalTransformMatrix);

            if(!Guide.IsVisible)
            {
                base.Draw(gameTime);
            }

            SpriteBatch.End();
        }
    }
}