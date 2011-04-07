using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sand
{
    internal class Billboard : Actor
    {
        private Texture2D _texture;

        public delegate void Action(object sender, object userInfo);

        public Billboard(Game game, Vector2 origin, Texture2D texture) : this(game, (int)origin.X, (int)origin.Y, texture)
        {
        }

        public Billboard(Game game, int x, int y, Texture2D texture) : base(game)
        {
            X = x;
            Y = y;
            Width = texture.Width;
            Height = texture.Height;

            _texture = texture;
        }

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Draw(_texture, Bounds, null, Color.White, 0.0f, Gravity.Offset(PositionGravity), SpriteEffects.None, 1.0f);
        }
    }
}