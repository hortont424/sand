using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Sand
{
    internal class Button : DrawableGameComponent
    {
        private Rectangle _bounds;
        private SpriteBatch _spriteBatch;
        private bool _hovered;

        public Button(Game game, Rectangle bounds) : base(game)
        {
            _bounds = bounds;
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        public override void Update(GameTime gameTime)
        {
            UpdateInput();
        }

        private void UpdateInput()
        {
            MouseState mouse = Mouse.GetState();

            _hovered = false;

            if(_bounds.Intersects(new Rectangle(mouse.X, mouse.Y, 1, 1)))
            {
                _hovered = true;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            Color color = _hovered ? Color.Green : Color.Red;

            _spriteBatch.Begin();
            _spriteBatch.Draw(Storage.Sprite("pixel"), new Rectangle((int)_bounds.X, (int)_bounds.Y, (int)_bounds.Width, (int)_bounds.Height), color);
            _spriteBatch.End();
        }
    }
}
