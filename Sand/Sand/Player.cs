using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Sand
{
    internal class LocalPlayer : DrawableGameComponent
    {
        private Vector2 _acceleration;
        private Vector2 _drag;
        private Vector2 _movementAcceleration;
        private KeyboardState _oldKeyState;
        private Vector2 _position;
        private SpriteBatch _spriteBatch;
        private Vector2 _velocity;

        public LocalPlayer(Game game) : base(game)
        {
            _drag = new Vector2(0.1f, 0.1f);
            _movementAcceleration = new Vector2(1.0f, 1.0f);
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        public override void Update(GameTime gameTime)
        {
            var timestep = (float)(gameTime.ElapsedGameTime.TotalMilliseconds / (1000 / 60));

            UpdateInput();

            _acceleration.X -= _drag.X * _velocity.X;
            _acceleration.Y -= _drag.Y * _velocity.Y;

            _velocity.X += _acceleration.X;
            _velocity.Y += _acceleration.Y;

            _position.X += _velocity.X * timestep;
            _position.Y += _velocity.Y * timestep;

            _acceleration.X = _acceleration.Y = 0.0f;
        }

        private void UpdateInput()
        {
            KeyboardState newKeyState = Keyboard.GetState();

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

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();
            _spriteBatch.Draw(Storage.Sprite("pixel"), new Rectangle((int)_position.X, (int)_position.Y, 5, 20), Color.Red);
            _spriteBatch.End();
        }
    }
}