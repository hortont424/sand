using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sand
{
    public class Label : Actor
    {
        public string Text;
        private readonly string _fontName;

        public Label(Game game, float x, float y, string text) : base(game)
        {
            X = x;
            Y = y;

            Text = text;
            _fontName = "Calibri24";
        }

        public Label(Game game, float x, float y, string text, string font) : this(game, x, y, text)
        {
            _fontName = font;
        }

        public override void Update(GameTime gameTime)
        {
        }

        public override void Draw(GameTime gameTime)
        {
            Vector2 textSize = Storage.Font(_fontName).MeasureString(Text);
            Vector2 textOrigin = textSize * Gravity.Offset(PositionGravity);
            _spriteBatch.DrawString(Storage.Font(_fontName), Text,
                                    new Vector2(Bounds.X, Bounds.Y), 
                                    Color.White, 0, textOrigin, 1.0f, SpriteEffects.None, 0.5f);
        }
    }
}