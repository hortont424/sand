using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sand
{
    class Countdown : Actor
    {
        private int _maxValue, _frameTime;
        private Animation _countdownAnimation;
        private AnimationGroup _countdownAnimationGroup;
        private string _currentString;
        private CountdownComplete _completeDelegate;

        public delegate void CountdownComplete();

        private int _currentValue;
        private int CurrentValue
        {
            get
            {
                return _currentValue;
            }
            set
            {
                _currentValue = value;
                _currentString = string.Format("{0}", _currentValue);

                var textSize = Storage.Font("Calibri400Bold").MeasureString(_currentString);
                Width = textSize.X;
                Height = textSize.Y;
            }
        }

        public Countdown(Game game) : base(game)
        {
            DrawOrder = 10000;
        }

        public void Start(int num, int ms, CountdownComplete completeDelegate)
        {
            _maxValue = CurrentValue = num;
            _frameTime = ms;
            _completeDelegate = completeDelegate;

            if(_countdownAnimationGroup != null)
            {
                Storage.AnimationController.RemoveGroup(_countdownAnimationGroup);
                _countdownAnimation = null;
                _countdownAnimationGroup = null;
            }

            _countdownAnimation = new Animation { CompletedDelegate = DecrementCounter };
            _countdownAnimationGroup = new AnimationGroup(_countdownAnimation, ms) { Loops = true };

            Storage.AnimationController.AddGroup(_countdownAnimationGroup);
        }

        private void DecrementCounter()
        {
            CurrentValue--;

            if(CurrentValue == 0)
            {
                Storage.AnimationController.RemoveGroup(_countdownAnimationGroup);
                _countdownAnimation = null;
                _countdownAnimationGroup = null;

                if(_completeDelegate != null)
                {
                    _completeDelegate();
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            var textOrigin = new Vector2(Width, Height) * Gravity.Offset(Gravity.Center);

            
            _spriteBatch.DrawString(Storage.Font("Calibri400Bold"), _currentString,
                                    Position + new Vector2(4.0f),
                                    Color.Black, 0, textOrigin, 1.0f, SpriteEffects.None, 0.5f);
            _spriteBatch.DrawString(Storage.Font("Calibri400Bold"), _currentString,
                                    Position,
                                    Color.Orange, 0, textOrigin, 1.0f, SpriteEffects.None, 0.5f);
        }
    }
}
