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

    internal class Player : DrawableGameComponent
    {
        private SpriteBatch _spriteBatch;

        public Vector2 Position;
        public float Angle;
        
        public Team Team;

        public Player(Game game) : base(game)
        {
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
            Color teamColor = Storage.Color(Team == Team.None ? "NeutralTeam" : ((Team == Team.Red) ? "RedTeam" : "BlueTeam"));
            _spriteBatch.Draw(Storage.Sprite("pixel"), new Rectangle((int)Position.X, (int)Position.Y, 5, 20), null,
                              teamColor, Angle, new Vector2(0.5f, 0.5f), SpriteEffects.None, 0.0f);
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
            var transformedMouse = Vector2.Transform(new Vector2(mouse.X, mouse.Y),
                                                     Matrix.Invert(sandGame.GlobalTransformMatrix));

            Angle = (float)Math.Atan2(transformedMouse.Y - Position.Y, transformedMouse.X - Position.X) + ((float)Math.PI / 2.0f);
        }

        private void UpdatePosition(GameTime gameTime)
        {
            var timestep = (float)(gameTime.ElapsedGameTime.TotalMilliseconds / (1000 / 60));

            _acceleration.X -= _drag.X * _velocity.X;
            _acceleration.Y -= _drag.Y * _velocity.Y;

            _velocity.X += _acceleration.X;
            _velocity.Y += _acceleration.Y;

            Position.X += _velocity.X * timestep;
            Position.Y += _velocity.Y * timestep;

            _acceleration.X = _acceleration.Y = 0.0f;
        }
    }
}