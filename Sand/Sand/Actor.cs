using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Sand
{
    public class Actor : DrawableGameComponent
    {
        public List<Actor> Children;
        public Tuple<Gravity.Vertical,Gravity.Horizontal> PositionGravity;
        public Rectangle Bounds;

        public Actor(Game game) : base(game)
        {
            Children = new List<Actor>();
            PositionGravity = new Tuple<Gravity.Vertical, Gravity.Horizontal>(Gravity.Vertical.Top, Gravity.Horizontal.Left);
        }
    }
}
