using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Sand
{
    internal class Billboard : DrawableGameComponent
    {
        private Vector2 _origin;
        private SpriteBatch _spriteBatch;
        private Texture2D _texture;

        public delegate void Action(object sender, object userInfo);

        public Billboard(Game game, Vector2 origin, Texture2D texture)
            : base(game)
        {
            _origin = origin;
            _texture = texture;
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            var sandGame = Game as Sand;

            if (sandGame != null)
            {
                _spriteBatch = sandGame.SpriteBatch;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Draw(_texture, _origin, Color.White);
        }
    }
}