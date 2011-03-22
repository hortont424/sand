using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sand
{
    internal class Map : DrawableGameComponent
    {
        private Texture2D _map;
        private Texture2D _mapTexture;
        private SpriteBatch _spriteBatch;

        public Map(Game game, string name) : base(game)
        {
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            var sandGame = Game as Sand;

            if (sandGame != null)
            {
                _spriteBatch = sandGame.SpriteBatch;
            }

            _map = sandGame.Content.Load<Texture2D>("Textures/Maps/01"); // TODO: use name
            _mapTexture = _map;
        }

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Draw(_mapTexture, new Vector2(0.0f, 0.0f), Color.White);
        }
    }
}