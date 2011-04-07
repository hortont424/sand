using System;
using Microsoft.Xna.Framework;

namespace Sand
{
    public class Gravity
    {
        public enum Vertical
        {
            Top,
            Center,
            Bottom
        }

        public enum Horizontal
        {
            Left,
            Center,
            Right
        }

        public static readonly Tuple<Vertical, Horizontal> Center = new Tuple<Vertical, Horizontal>(Vertical.Center, Horizontal.Center);
        public static readonly Tuple<Vertical, Horizontal> TopLeft = new Tuple<Vertical, Horizontal>(Vertical.Top, Horizontal.Left);

        public static Vector2 Offset(Tuple<Vertical,Horizontal> gravity)
        {
            float vOffset, hOffset;

            switch(gravity.Item1)
            {
                case Vertical.Top:
                    vOffset = 0.0f;
                    break;
                case Vertical.Center:
                    vOffset = 0.5f;
                    break;
                case Vertical.Bottom:
                    vOffset = 1.0f;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (gravity.Item2)
            {
                case Horizontal.Left:
                    hOffset = 0.0f;
                    break;
                case Horizontal.Center:
                    hOffset = 0.5f;
                    break;
                case Horizontal.Right:
                    hOffset = 1.0f;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new Vector2(hOffset, vOffset);
        }
    }
}