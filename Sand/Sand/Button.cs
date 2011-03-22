using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Sand
{
    internal class Button : DrawableGameComponent
    {
        private Rectangle _bounds;
        private SpriteBatch _spriteBatch;
        private bool _hovered;
        private bool _clicked;
        private object _actionUserInfo;
        private Action _action;
        private string _text;
        private Color _baseColor;
        private Color _clickColor;
        private Color _highlightColor;
        private Color _borderColor;

        public delegate void Action(object sender, object userInfo);

        public Button(Game game, Rectangle bounds, string text) : base(game)
        {
            _bounds = bounds;
            _text = text;
            _baseColor = Storage.Color("WidgetFill");
        }

        public Button(Game game, Rectangle bounds, string text, Color baseColor) : this(game, bounds, text)
        {
            _baseColor = baseColor;
        }

        public void SetAction(Action action, object userInfo)
        {
            _action = action;
            _actionUserInfo = userInfo;
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            var sandGame = Game as Sand;

            if(sandGame != null)
            {
                _spriteBatch = sandGame.SpriteBatch;
            }

            double hue, saturation, value;
            SandColor.ToHSV(_baseColor, out hue, out saturation, out value);

            _borderColor = SandColor.FromHSV(hue, saturation, Math.Max(value - 0.3, 0.0));
            _clickColor = SandColor.FromHSV(hue, Math.Max(saturation - 0.2, 0.0), Math.Min(value + 0.4, 1.0));
            _highlightColor = SandColor.FromHSV(hue, Math.Max(saturation - 0.2, 0.0), Math.Min(value + 0.2, 1.0));
        }

        public override void Update(GameTime gameTime)
        {
            UpdateInput();
        }

        private void UpdateInput()
        {
            MouseState mouse = Mouse.GetState();
            var sandGame = Game as Sand;

            _hovered = false;

            if(_bounds.Intersects(new Rectangle((int)sandGame.MouseLocation.X, (int)sandGame.MouseLocation.Y, 1, 1)))
            {
                _hovered = true;
            }

            var oldClicked = _clicked;
            _clicked = _hovered && (mouse.LeftButton == ButtonState.Pressed);

            if(!oldClicked && _clicked)
            {
                if(_action != null)
                {
                    _action(this, _actionUserInfo);
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            Color fillColor = _clicked ? _clickColor : (_hovered ? _highlightColor : _baseColor);
            Color borderColor = _borderColor;
            const int borderRadius = 3;

            Vector2 textSize = Storage.Font("Calibri24").MeasureString(_text);
            Vector2 textOrigin = textSize / 2;

            _spriteBatch.Draw(Storage.Sprite("pixel"),
                              new Rectangle(_bounds.X, _bounds.Y, _bounds.Width, _bounds.Height), borderColor);
            _spriteBatch.Draw(Storage.Sprite("pixel"),
                              new Rectangle(_bounds.X + borderRadius, _bounds.Y + borderRadius,
                                            _bounds.Width - (2 * borderRadius), _bounds.Height - (2 * borderRadius)),
                              fillColor);
            _spriteBatch.DrawString(Storage.Font("Calibri24"), _text,
                                    new Vector2(_bounds.X + (_bounds.Width / 2), _bounds.Y + (_bounds.Height / 2) + 2),
                                    Color.White, 0, textOrigin, 1.0f, SpriteEffects.None, 0.5f);
        }
    }
}