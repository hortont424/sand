using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sand
{
    internal class Crosshair : Actor
    {
        private readonly Texture2D _sprite;

        public delegate void Action(object sender, object userInfo);

        public Crosshair(Game game) : base(game)
        {
            DrawOrder = 2000;
            PositionGravity = Gravity.Center;

            _sprite = Storage.Sprite("Crosshair");
        }

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Draw(_sprite, new Vector2(_sandGame.MouseLocation.X, _sandGame.MouseLocation.Y), null,
                              Color.Gray, 0.0f,
                              Gravity.Offset(PositionGravity) * new Vector2(_sprite.Width, _sprite.Height), 1.0f,
                              SpriteEffects.None, 1.0f);
        }
    }
}