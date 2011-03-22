using System;
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

    internal class Player : DrawableGameComponent
    {
        private SpriteBatch _spriteBatch;

        protected Color[] _texture;

        public Vector2 Position;
        public float Angle;
        public int Width, Height;

        public Team Team;

        public Player(Game game) : base(game)
        {
            DrawOrder = 100;
            Width = Storage.Sprite("player").Width;
            Height = Storage.Sprite("player").Height;

            Position.X = 60;
            Position.Y = 60;
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            var sandGame = Game as Sand;

            if(sandGame != null)
            {
                _spriteBatch = sandGame.SpriteBatch;
            }

            // TODO: _texture should be a proper cache property, updating when team/class changes
            _texture = new Color[Width * Height];
            Storage.Sprite("player").GetData(_texture);
        }

        public override void Draw(GameTime gameTime)
        {
            Color teamColor =
                Storage.Color(Team == Team.None ? "NeutralTeam" : ((Team == Team.Red) ? "RedTeam" : "BlueTeam"));
            _spriteBatch.Draw(Storage.Sprite("player"), new Rectangle((int)Position.X, (int)Position.Y, Width, Height),
                              null,
                              teamColor, Angle, new Vector2(Width / 2.0f, Height / 2.0f), SpriteEffects.None, 0.0f);
        }
    }

    internal class RemotePlayer : Player
    {
        private NetworkGamer _gamer;

        public RemotePlayer(Game game, NetworkGamer gamer) : base(game)
        {
            _gamer = gamer;
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

        public LocalPlayer(Game game) : base(game)
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
                // TODO: this is obviously wrong (should reflect, not just bounce back in the same direction!)
                _velocity.X = -_velocity.X;
                _velocity.Y = -_velocity.Y;
            }
        }
    }
}