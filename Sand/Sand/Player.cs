using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;

namespace Sand
{
    public enum Team
    {
        None,
        Red,
        Blue
    } ;

    public enum Class
    {
        None,
        Defense,
        Offense,
        Support
    } ;

    internal class Player : DrawableGameComponent
    {
        public NetworkGamer Gamer;
        private SpriteBatch _spriteBatch;

        protected Color[] _texture;

        public Vector2 Position;
        public float Angle;
        public int Width, Height;

        private Class _class;
        private Team _team;
        private Texture2D _sprite;

        public Team Team
        {
            get { return _team; }
            set
            {
                _team = value;

                if (this is LocalPlayer)
                {
                    Messages.SendUpdatePlayerTeamMessage(this, this.Gamer.Id, true);
                }
            }
        }

        public Class Class
        {
            get { return _class; }
            set
            {
                _class = value;
                _sprite = SpriteForClass(_class);
                _sprite.GetData(_texture);

                if(this is LocalPlayer)
                {
                    Messages.SendUpdatePlayerClassMessage(this, this.Gamer.Id, true);
                }
            }
        }

        public Player(Game game, NetworkGamer gamer) : base(game)
        {
            Gamer = gamer;
            DrawOrder = 100;
            Width = Storage.Sprite("DefenseClass").Width;
            Height = Storage.Sprite("DefenseClass").Height;

            Position.X = 60;
            Position.Y = 60;

            _texture = new Color[Width * Height];
            Class = Class.None;
        }

        private static Texture2D SpriteForClass(Class cls)
        {
            string spriteName;

            switch(cls) // TODO: I hear you like dictionaries?
            {
                case Class.None:
                    spriteName = "DefenseClass"; // TODO: questionmark class?
                    break;
                case Class.Defense:
                    spriteName = "DefenseClass";
                    break;
                case Class.Offense:
                    spriteName = "OffenseClass";
                    break;
                case Class.Support:
                    spriteName = "SupportClass";
                    break;
                default:
                    throw new ArgumentOutOfRangeException("cls");
            }

            return Storage.Sprite(spriteName);
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            var sandGame = Game as Sand;

            if(sandGame != null)
            {
                _spriteBatch = sandGame.SpriteBatch;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            Color teamColor =
                Storage.Color(Team == Team.None ? "NeutralTeam" : ((Team == Team.Red) ? "RedTeam" : "BlueTeam"));

            _spriteBatch.Draw(Storage.Sprite("pixel"), new Rectangle((int)Position.X, (int)Position.Y, 1, 3000), null,
                              teamColor, Angle, new Vector2(0.5f, 1.0f), SpriteEffects.None, 0.0f);

            _spriteBatch.Draw(_sprite, new Rectangle((int)Position.X, (int)Position.Y, Width, Height),
                              null,
                              teamColor, Angle, new Vector2(Width / 2.0f, Height / 2.0f), SpriteEffects.None, 0.0f);
        }
    }

    internal class RemotePlayer : Player
    {
        public RemotePlayer(Game game, NetworkGamer gamer) : base(game, gamer)
        {

        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
    }

    internal class LocalPlayer : Player
    {
        private Vector2 _acceleration;
        private Vector2 _drag;
        private Vector2 _movementAcceleration;
        private KeyboardState _oldKeyState;

        private Vector2 _velocity;

        public LocalPlayer(Game game, NetworkGamer gamer) : base(game, gamer)
        {
            _drag = new Vector2(0.1f, 0.1f);
            _movementAcceleration = new Vector2(1.0f, 1.0f);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            UpdateInput();
            UpdateAngle();
            UpdatePosition(gameTime);
        }

        private void UpdateInput()
        {
            var newKeyState = Keyboard.GetState();

            if(newKeyState.IsKeyDown(Keys.A))
            {
                _acceleration.X += -_movementAcceleration.X;
            }
            else if(newKeyState.IsKeyDown(Keys.D))
            {
                _acceleration.X += _movementAcceleration.X;
            }

            if(newKeyState.IsKeyDown(Keys.W))
            {
                _acceleration.Y += -_movementAcceleration.Y;
            }
            else if(newKeyState.IsKeyDown(Keys.S))
            {
                _acceleration.Y += _movementAcceleration.Y;
            }

            if(newKeyState.IsKeyDown(Keys.D1))
            {
                Class = Class.Defense;
            }
            else if(newKeyState.IsKeyDown(Keys.D2))
            {
                Class = Class.Offense;
            }
            else if(newKeyState.IsKeyDown(Keys.D3))
            {
                Class = Class.Support;
            }

            _oldKeyState = newKeyState;
        }

        private void UpdateAngle()
        {
            var mouse = Mouse.GetState();
            var sandGame = Game as Sand;

            Angle = (float)Math.Atan2(sandGame.MouseLocation.Y - Position.Y, sandGame.MouseLocation.X - Position.X) +
                    ((float)Math.PI / 2.0f);
        }

        private void UpdatePosition(GameTime gameTime)
        {
            Vector2 newPosition = new Vector2(Position.X, Position.Y);
            var sandGame = Game as Sand;
            var timestep = (float)(gameTime.ElapsedGameTime.TotalMilliseconds / (1000 / 60));

            _acceleration.X -= _drag.X * _velocity.X;
            _acceleration.Y -= _drag.Y * _velocity.Y;

            _velocity.X += _acceleration.X;
            _velocity.Y += _acceleration.Y;

            newPosition.X += _velocity.X * timestep;
            newPosition.Y += _velocity.Y * timestep;

            _acceleration.X = _acceleration.Y = 0.0f;

            if(!sandGame.GameMap.CollisionTest(_texture,
                                               new Rectangle((int)(newPosition.X - (Width / 2.0)),
                                                             (int)(newPosition.Y - (Height / 2.0)),
                                                             Width, Height)))
            {
                Position.X = newPosition.X;
                Position.Y = newPosition.Y;
            }
            else
            {
                if(!sandGame.GameMap.CollisionTest(_texture, new Rectangle((int)(newPosition.X - (Width / 2.0)),
                                                                           (int)(Position.Y - (Height / 2.0)), Width,
                                                                           Height)))
                {
                    _velocity.Y = -_velocity.Y;
                    Position.X = newPosition.X;
                }
                else if(!sandGame.GameMap.CollisionTest(_texture, new Rectangle((int)(Position.X - (Width / 2.0)),
                                                                                (int)(newPosition.Y - (Height / 2.0)),
                                                                                Width, Height)))
                {
                    _velocity.X = -_velocity.X;
                    Position.Y = newPosition.Y;
                }
                else
                {
                    _velocity.X = -_velocity.X;
                    _velocity.Y = -_velocity.Y;
                }
            }
        }
    }
}