using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sand
{
    public class Map : DrawableGameComponent
    {
        private Texture2D _map;
        private Texture2D _mapImage;
        private Color[] _mapTexture;
        private SpriteBatch _spriteBatch;

        public int Width, Height;

        public Map(Game game, string name) : base(game)
        {
            DrawOrder = 1;
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            var sandGame = Game as Sand;

            if(sandGame != null)
            {
                _spriteBatch = sandGame.SpriteBatch;
            }

            _map = sandGame.Content.Load<Texture2D>("Textures/Maps/01"); // TODO: use name
            _mapImage = _map;

            Width = _map.Width;
            Height = _map.Height;

            _mapTexture = new Color[Width * Height];
            _map.GetData(_mapTexture);
        }

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Draw(_mapImage, new Vector2(0.0f, 0.0f), Color.White);
        }

        // (C) Microsoft, Corp (see License.md)
        public bool CollisionTest(Color[] pTexture, Rectangle rectangleA)
        {
            var rectangleB = new Rectangle(0, 0, Width, Height);

            int top = Math.Max(rectangleA.Top, rectangleB.Top);
            int bottom = Math.Min(rectangleA.Bottom, rectangleB.Bottom);
            int left = Math.Max(rectangleA.Left, rectangleB.Left);
            int right = Math.Min(rectangleA.Right, rectangleB.Right);

            // Check every point within the intersection bounds
            for(int y = top; y < bottom; y++)
            {
                for(int x = left; x < right; x++)
                {
                    Color colorA = pTexture[(x - rectangleA.Left) +
                                            (y - rectangleA.Top) * rectangleA.Width];
                    Color colorB = _mapTexture[(x - rectangleB.Left) +
                                               (y - rectangleB.Top) * rectangleB.Width];

                    // If both pixels are not completely transparent,
                    if(colorA.A != 0 && colorB == Color.White)
                    {
                        // then an intersection has been found
                        return true;
                    }
                }
            }

            return false;
        }
    }
}