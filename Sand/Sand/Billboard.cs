using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sand
{
    internal class Billboard : Actor
    {
        public float X { get; set; }
        public float Y { get; set; }

        private SpriteBatch _spriteBatch;
        private Texture2D _texture;

        public delegate void Action(object sender, object userInfo);

        public Billboard(Game game, Vector2 origin, Texture2D texture) : this(game, origin.X, origin.Y, texture)
        {
        }

        public Billboard(Game game, float x, float y, Texture2D texture) : base(game)
        {
            X = x;
            Y = y;
            _texture = texture;
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
            _spriteBatch.Draw(_texture, new Vector2(X, Y), Color.White);
        }
    }
}