using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sand
{
    public class Map : Actor
    {
        public string Name { get; set; }

        private Texture2D _map;
        private Texture2D _mapImage;
        private Color[] _mapTexture;
        private SpriteBatch _spriteBatch;

        public int Width, Height;

        public Map(Game game, string name) : base(game)
        {
            Name = name;
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

            _map = sandGame.Content.Load<Texture2D>(string.Format("Textures/Maps/{0}", Name));
            _mapImage = sandGame.Content.Load<Texture2D>(string.Format("Textures/Maps/{0}-image", Name));

            Width = _map.Width;
            Height = _map.Height;

            _mapTexture = new Color[Width * Height];
            _map.GetData(_mapTexture);
        }

        public override void Draw(GameTime gameTime)
        {
            // If we change drawing location, we need to change ray intersection offset
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

        public float? Intersects(Ray ray)
        {
            float ? distance = null;

            var point = ray.Position;

            while(point.X < _mapImage.Width && point.Y < _mapImage.Height && point.X >= 0 && point.Y >= 0)
            {
                point.X += ray.Direction.X;
                point.Y += ray.Direction.Y;

                Color color = _mapTexture[(int)((int)Math.Floor(point.X) + (Math.Floor(point.Y) * _mapImage.Width))];

                if(color == Color.White)
                {
                    return (point - ray.Position).Length();
                }
            }

            return distance;
        }
    }
}