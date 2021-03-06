﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Sand
{
    public class Button : Actor
    {
        public bool Hovered;
        private bool _clicked;
        private object _actionUserInfo;
        private Action _action;
        private object _hoverActionUserInfo;
        private Action _hoverAction;
        private object _unhoverActionUserInfo;
        private Action _unhoverAction;
        private string _text;
        public Color BaseColor;
        private Color _clickColor;
        private Color _highlightColor;
        private Color _borderColor;
        private Texture2D _sprite;
        public Color TeamColor;
        private bool _haveInitialized;
        public bool AcceptsClick = true;
        private int _padding;

        public int Padding
        {
            get
            {
                return _padding;
            }

            set
            {
                Width -= _padding * 2;
                Height -= _padding * 2;

                _padding = value;

                Width += _padding * 2;
                Height += _padding * 2;
            }
        }

        public delegate void Action(object sender, object userInfo);

        public Button(Game game, Rectangle bounds, string text) : base(game)
        {
            X = bounds.X;
            Y = bounds.Y;
            Width = bounds.Width;
            Height = bounds.Height;

            _text = text;
            _sprite = null;
            _action = null;

            BaseColor = Storage.Color("WidgetFill");
            TeamColor = Storage.Color("NeutralTeam");
        }

        public Button(Game game, Rectangle bounds, string text, Color baseColor) : this(game, bounds, text)
        {
            BaseColor = baseColor;
        }

        public Button(Game game, Vector2 origin, Texture2D sprite) : base(game)
        {
            X = origin.X;
            Y = origin.Y;
            Width = sprite.Width + (Padding * 2);
            Height = sprite.Height + (Padding * 2);

            _sprite = sprite;

            _text = null;
            _action = null;

            BaseColor = Storage.Color("WidgetFill");
        }

        public Button(Game game, Vector2 origin, Texture2D sprite, Color baseColor) : this(game, origin, sprite)
        {
            BaseColor = baseColor;
        }

        public Button(Game game, Vector2 origin, Texture2D sprite, Color teamColor, Color baseColor)
            : this(game, origin, sprite, baseColor)
        {
            TeamColor = teamColor;
        }

        public void SetAction(Action action, object userInfo)
        {
            _action = action;
            _actionUserInfo = userInfo;
        }

        public void UnsetAction()
        {
            _action = null;
        }

        public void SetHoverAction(Action action, object userInfo)
        {
            _hoverAction = action;
            _hoverActionUserInfo = userInfo;
        }

        public void SetUnhoverAction(Action action, object userInfo)
        {
            _unhoverAction = action;
            _unhoverActionUserInfo = userInfo;
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            double hue, saturation, value;
            SandColor.ToHSV(BaseColor, out hue, out saturation, out value);

            _borderColor = SandColor.FromHSV(hue, saturation, Math.Max(value - 0.2, 0.0));
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

            if(!Storage.AcceptInput)
            {
                return;
            }

            var oldHovered = Hovered;

            Hovered = false;

            if(_action != null)
            {
                if(Bounds.Intersects(new Rectangle((int)_sandGame.MouseLocation.X, (int)_sandGame.MouseLocation.Y, 1, 1)))
                {
                    Hovered = true;

                    if((_hoverAction != null) && Hovered && !oldHovered)
                    {
                        _hoverAction(this, _hoverActionUserInfo);
                    }
                }
                else
                {
                    Hovered = false;

                    if((_unhoverAction != null) && !Hovered && oldHovered)
                    {
                        _unhoverAction(this, _unhoverActionUserInfo);
                    }
                }

                if(AcceptsClick)
                {
                    var oldClicked = _clicked;
                    _clicked = Hovered && (mouse.LeftButton == ButtonState.Pressed);

                    if(!oldClicked && _clicked)
                    {
                        if(_haveInitialized)
                        {
                            _action(this, _actionUserInfo);
                        }
                        else
                        {
                            Hovered = false;
                        }
                    }
                }
                else
                {
                    _clicked = false;
                }
            }

            _haveInitialized = true;
        }

        public override void Draw(GameTime gameTime)
        {
            Color fillColor = _clicked ? _clickColor : (Hovered ? _highlightColor : BaseColor);
            Color borderColor = _borderColor;
            Color textColor = Color.White;
            const int borderRadius = 5;

            if (_action == null)
            {
                fillColor *= 0.2f;
                borderColor *= 0.2f;
                textColor *= 0.2f;
            }

            _spriteBatch.Draw(Storage.Sprite("pixel"),
                              new Rectangle(Bounds.X - borderRadius, Bounds.Y - borderRadius,
                                            Bounds.Width + (2 * borderRadius), Bounds.Height + (2 * borderRadius)),
                              borderColor);
            _spriteBatch.Draw(Storage.Sprite("pixel"),
                              new Rectangle(Bounds.X, Bounds.Y,
                                            Bounds.Width, Bounds.Height),
                              fillColor);

            if(_text != null)
            {
                Vector2 textSize = Storage.Font("Calibri24").MeasureString(_text);
                Vector2 textOrigin = textSize / 2;
                _spriteBatch.DrawString(Storage.Font("Calibri24"), _text,
                                        new Vector2(Bounds.X + (Bounds.Width / 2),
                                                    Bounds.Y + (Bounds.Height / 2) + 2),
                                        textColor, 0, textOrigin, 1.0f, SpriteEffects.None, 0.5f);
            }

            if(_sprite != null)
            {
                _spriteBatch.Draw(_sprite, new Rectangle(Bounds.X + Padding, Bounds.Y + Padding, Bounds.Width - (Padding * 2), Bounds.Height - (Padding * 2)), TeamColor);
            }
        }
    }
}