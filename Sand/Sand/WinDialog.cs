using Microsoft.Xna.Framework;

namespace Sand
{
    internal class WinDialog : Actor
    {
        private readonly Label _label;

        public string Text
        {
            get
            {
                return _label.Text;
            }
            set
            {
                _label.Text = value;
            }
        }

        public WinDialog(Game game, string message, Button.Action action, object userInfo) : base(game)
        {
            DrawOrder = 10000;

            var sandGame = Game as Sand;

            const int margin = 5;

            Width = 725;
            Height = 200;

            var halfway = sandGame.BaseScreenSize / 2.0f;

            var background = new Billboard(Game, (int)halfway.X, (int)halfway.Y, (int)(Width + (margin * 2)),
                                           (int)(Height + (margin * 2)), Storage.Sprite("pixel"))
                             {
                                 Color = Color.DarkGray,
                                 PositionGravity = Gravity.Center,
                                 DrawOrder = 9999
                             };
            var foreground = new Billboard(Game, (int)halfway.X, (int)halfway.Y, (int)Width,
                                           (int)Height, Storage.Sprite("pixel"))
                             {
                                 Color = new Color(0.2f, 0.2f, 0.2f),
                                 PositionGravity = Gravity.Center,
                                 DrawOrder = 10000
                             };
            _label = new Label(Game, halfway.X, halfway.Y - 35, message, "Calibri48Bold")
                        {
                            PositionGravity = Gravity.Center,
                            DrawOrder = 10001
                        };

            var ready = new Button(Game, new Rectangle((int)halfway.X - 100, (int)halfway.Y + 15, 200, 50), "Ready",
                                   new Color(0.1f, 0.7f, 0.1f))
                        {
                            PositionGravity = Gravity.Center,
                            DrawOrder = 10001
                        };

            ready.SetAction(action, userInfo);

            Children.Add(background);
            Children.Add(foreground);
            Children.Add(_label);
            Children.Add(ready);
        }
    }
}