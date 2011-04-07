using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sand
{
    public class Label : Actor
    {
        public float X { get; set; }
        public float Y { get; set; }

        private SpriteBatch _spriteBatch;
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

        protected override void LoadContent()
        {
            base.LoadContent();

            var sandGame = Game as Sand;

            if(sandGame != null)
            {
                _spriteBatch = sandGame.SpriteBatch;
            }
        }

        public override void Update(GameTime gameTime)
        {
        }

        public override void Draw(GameTime gameTime)
        {
            Vector2 textSize = Storage.Font(_fontName).MeasureString(Text);
            Vector2 textOrigin = textSize / 2;
            _spriteBatch.DrawString(Storage.Font(_fontName), Text,
                                    new Vector2(X, Y),
                                    Color.White, 0, textOrigin, 1.0f, SpriteEffects.None, 0.5f);
        }
    }
}