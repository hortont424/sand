using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Sand
{
    internal class Button : Actor
    {
        private Rectangle _bounds;
        private SpriteBatch _spriteBatch;
        private bool _hovered;
        private bool _clicked;
        private object _actionUserInfo;
        private Action _action;
        private string _text;
        public Color BaseColor;
        private Color _clickColor;
        private Color _highlightColor;
        private Color _borderColor;
        private Texture2D _sprite;
        public Color TeamColor;
        private bool _haveInitialized;

        public delegate void Action(object sender, object userInfo);

        public Button(Game game, Rectangle bounds, string text) : base(game)
        {
            _bounds = bounds;
            _text = text;
            _sprite = null;
            _action = null;
            BaseColor = Storage.Color("WidgetFill");
        }

        public Button(Game game, Rectangle bounds, string text, Color baseColor) : this(game, bounds, text)
        {
            BaseColor = baseColor;
        }

        public Button(Game game, Vector2 origin, Texture2D sprite) : base(game)
        {
            _bounds.X = (int)origin.X;
            _bounds.Y = (int)origin.Y;
            _bounds.Width = sprite.Width + 12;
            _bounds.Height = sprite.Height + 12;

            _sprite = sprite;

            _text = null;
            _action = null;

            BaseColor = Storage.Color("WidgetFill");
        }

        public Button(Game game, Vector2 origin, Texture2D sprite, Color baseColor) : this(game, origin, sprite)
        {
            BaseColor = baseColor;
        }

        public Button(Game game, Vector2 origin, Texture2D sprite, Color teamColor, Color baseColor) : this(game, origin, sprite, baseColor)
        {
            TeamColor = teamColor;
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
            SandColor.ToHSV(BaseColor, out hue, out saturation, out value);

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

            if(_action != null)
            {
                if(_bounds.Intersects(new Rectangle((int)sandGame.MouseLocation.X, (int)sandGame.MouseLocation.Y, 1, 1)))
                {
                    _hovered = true;
                }

                var oldClicked = _clicked;
                _clicked = _hovered && (mouse.LeftButton == ButtonState.Pressed);

                if(!oldClicked && _clicked)
                {
                    if (_haveInitialized)
                    {
                        _action(this, _actionUserInfo);
                    }
                    else
                    {
                        _hovered = false;
                    }
                    
                }
            }

            _haveInitialized = true;
        }

        public override void Draw(GameTime gameTime)
        {
            Color fillColor = _clicked ? _clickColor : (_hovered ? _highlightColor : BaseColor);
            Color borderColor = _borderColor;
            const int borderRadius = 3;

            _spriteBatch.Draw(Storage.Sprite("pixel"),
                              new Rectangle(_bounds.X, _bounds.Y, _bounds.Width, _bounds.Height), borderColor);
            _spriteBatch.Draw(Storage.Sprite("pixel"),
                              new Rectangle(_bounds.X + borderRadius, _bounds.Y + borderRadius,
                                            _bounds.Width - (2 * borderRadius), _bounds.Height - (2 * borderRadius)),
                              fillColor);

            if(_text != null)
            {
                Vector2 textSize = Storage.Font("Calibri24").MeasureString(_text);
                Vector2 textOrigin = textSize / 2;
                _spriteBatch.DrawString(Storage.Font("Calibri24"), _text,
                                        new Vector2(_bounds.X + (_bounds.Width / 2),
                                                    _bounds.Y + (_bounds.Height / 2) + 2),
                                        Color.White, 0, textOrigin, 1.0f, SpriteEffects.None, 0.5f);
            }
           
            if(_sprite != null)
            {
                _spriteBatch.Draw(_sprite, new Vector2(_bounds.X + 6, _bounds.Y + 6), TeamColor);
            }
        }
    }
}