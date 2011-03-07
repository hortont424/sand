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
            Color color = _clicked ? Color.PaleVioletRed : (_hovered ? Color.DarkRed : Color.Red);

            _spriteBatch.Begin();
            _spriteBatch.Draw(Storage.Sprite("pixel"),
                              new Rectangle(_bounds.X, _bounds.Y, _bounds.Width, _bounds.Height), color);
            _spriteBatch.End();
        }
    }
}