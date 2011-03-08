using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;

namespace Sand
{
    internal class Player : DrawableGameComponent
    {
        internal Vector2 _position;
        internal SpriteBatch _spriteBatch;
        internal float _angle;

        public Player(Game game) : base(game)
        {
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();
            _spriteBatch.Draw(Storage.Sprite("pixel"), new Rectangle((int) _position.X, (int) _position.Y, 5, 20), null,
                              Color.Red, _angle, new Vector2(0.5f, 0.5f), SpriteEffects.None, 0.0f);
            _spriteBatch.End();
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

            // see if any of the incoming messages from the server pertain to this player, update his position if so!
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
            var mouseState = Mouse.GetState();

            _angle = (float)Math.Atan2(mouseState.Y - _position.Y, mouseState.X - _position.X) + ((float)Math.PI / 2.0f);
        }

        private void UpdatePosition(GameTime gameTime)
        {
            var timestep = (float)(gameTime.ElapsedGameTime.TotalMilliseconds / (1000 / 60));

            _acceleration.X -= _drag.X * _velocity.X;
            _acceleration.Y -= _drag.Y * _velocity.Y;

            _velocity.X += _acceleration.X;
            _velocity.Y += _acceleration.Y;

            _position.X += _velocity.X * timestep;
            _position.Y += _velocity.Y * timestep;

            _acceleration.X = _acceleration.Y = 0.0f;
        }
    }
}