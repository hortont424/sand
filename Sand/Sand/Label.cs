using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sand
{
    internal class Label : DrawableGameComponent
    {
        public float X { get; set; }
        public float Y { get; set; }

        private SpriteBatch _spriteBatch;
        private readonly string _text;
        private readonly string _fontName;

        public Label(Game game, float x, float y, string text) : base(game)
        {
            X = x;
            Y = y;

            _text = text;
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
            Vector2 textSize = Storage.Font(_fontName).MeasureString(_text);
            Vector2 textOrigin = textSize / 2;
            _spriteBatch.DrawString(Storage.Font(_fontName), _text,
                                    new Vector2(X, Y),
                                    Color.White, 0, textOrigin, 1.0f, SpriteEffects.None, 0.5f);
        }
    }
}