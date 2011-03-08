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

        public delegate void Action(object sender, object userInfo);

        public Button(Game game, Rectangle bounds) : base(game)
        {
            _bounds = bounds;
        }

        public void SetAction(Action action, object userInfo)
        {
            _action = action;
            _actionUserInfo = userInfo;
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        public override void Update(GameTime gameTime)
        {
            UpdateInput();
        }

        private void UpdateInput()
        {
            MouseState mouse = Mouse.GetState();

            _hovered = false;

            if(_bounds.Intersects(new Rectangle(mouse.X, mouse.Y, 1, 1)))
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
            Color fillColor = _clicked
                                  ? Storage.Color("WidgetFillClick")
                                  : (_hovered ? Storage.Color("WidgetFillHighlight") : Storage.Color("WidgetFill"));
            Color borderColor = Storage.Color("WidgetBorder");
            const int borderRadius = 3;

            Vector2 textSize = Storage.Font("Calibri24").MeasureString("Button");
            Vector2 textOrigin = textSize / 2;

            _spriteBatch.Begin();
            _spriteBatch.Draw(Storage.Sprite("pixel"),
                              new Rectangle(_bounds.X, _bounds.Y, _bounds.Width, _bounds.Height), borderColor);
            _spriteBatch.Draw(Storage.Sprite("pixel"),
                              new Rectangle(_bounds.X + borderRadius, _bounds.Y + borderRadius,
                                            _bounds.Width - (2 * borderRadius), _bounds.Height - (2 * borderRadius)),
                              fillColor);
            _spriteBatch.DrawString(Storage.Font("Calibri24"), "Button",
                                    new Vector2(_bounds.X + (_bounds.Width / 2), _bounds.Y + (_bounds.Height / 2) + 2),
                                    Color.White, 0, textOrigin, 1.0f, SpriteEffects.None, 0.5f);
            _spriteBatch.End();
        }
    }
}