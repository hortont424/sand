using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sand
{
    public class Label : Actor
    {
        private string _text;

        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;

                var textSize = Storage.Font(_fontName).MeasureString(Text);
                Width = textSize.X;
                Height = textSize.Y;
            }
        }

        private readonly string _fontName;
        public Color Color { get; set; }

        public Label(Game game, float x, float y, string text) : this(game, x, y, text, "Calibri24")
        {
        }

        public Label(Game game, float x, float y, string text, string font) : base(game)
        {
            X = x;
            Y = y;

            _fontName = font;

            Text = text;
            Color = Color.White;

            DrawOrder = 1000;
        }

        public override void Update(GameTime gameTime)
        {
        }

        public override void Draw(GameTime gameTime)
        {
            var textOrigin = new Vector2(Width, Height) * Gravity.Offset(PositionGravity);
            _spriteBatch.DrawString(Storage.Font(_fontName), Text,
                                    new Vector2(Bounds.X, Bounds.Y),
                                    Color, 0, textOrigin, 1.0f, SpriteEffects.None, 0.5f);
        }
    }
}