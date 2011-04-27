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

        public Vector2 Position
        {
            get
            {
                return new Vector2((int)X, (int)Y);
            }
        }

        public Vector2 Size
        {
            get
            {
                return new Vector2((int)Width, (int)Height);
            }
        }

        public Actor(Game game) : base(game)
        {
            _sandGame = Game as Sand;
            Children = new List<Actor>();
            PositionGravity = new Tuple<Gravity.Vertical, Gravity.Horizontal>(Gravity.Vertical.Top,
                                                                              Gravity.Horizontal.Left);
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            if(_sandGame != null)
            {
                _spriteBatch = _sandGame.SpriteBatch;
            }
        }
    }
}