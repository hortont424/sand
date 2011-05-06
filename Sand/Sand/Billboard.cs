using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sand
{
    internal class Billboard : Actor
    {
        private Texture2D _texture;
        public Texture2D Texture
        {
            get
            {
                return _texture;
            }
            set
            {
                _texture = value;
                Width = _texture.Width;
                Height = _texture.Height;
            }
        }

        public Color Color { get; set; }

        public delegate void Action(object sender, object userInfo);

        public Billboard(Game game, Vector2 origin, Texture2D texture) : this(game, (int)origin.X, (int)origin.Y, texture)
        {
        }

        public Billboard(Game game, int x, int y, Texture2D texture) : this(game, x, y, texture.Width, texture.Height, texture)
        {
        }

        public Billboard(Game game, int x, int y, int w, int h, Texture2D texture) : base(game)
        {
            X = x;
            Y = y;
            Width = w;
            Height = h;

            Texture = texture;

            Color = Color.White;
        }

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Draw(Texture, Bounds, null, Color, 0.0f, Gravity.Offset(PositionGravity), SpriteEffects.None, 1.0f);
        }
    }
}