using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sand
{
    public class Actor : DrawableGameComponent
    {
        public List<Actor> Children;
        public Tuple<Gravity.Vertical, Gravity.Horizontal> PositionGravity;

        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        protected SpriteBatch _spriteBatch;
        protected Sand _sandGame;

        public Rectangle Bounds
        {
            get
            {
                return new Rectangle((int)X, (int)Y, (int)Width, (int)Height);
            }
        }

        public Actor(Game game) : base(game)
        {
            Children = new List<Actor>();
            PositionGravity = new Tuple<Gravity.Vertical, Gravity.Horizontal>(Gravity.Vertical.Top,
                                                                              Gravity.Horizontal.Left);
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            _sandGame = Game as Sand;

            if(_sandGame != null)
            {
                _spriteBatch = _sandGame.SpriteBatch;
            }
        }
    }
}