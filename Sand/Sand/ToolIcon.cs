using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sand.Tools;
using Color = System.Drawing.Color;
using Rectangle = System.Drawing.Rectangle;

namespace Sand
{
    internal class ToolIcon : DrawableGameComponent
    {
        private readonly Tool _tool;
        private SpriteBatch _spriteBatch;
        private Texture2D _drainTexture;
        private double _drainValue;

        public Vector2 Position { get; set; }

        public ToolIcon(Game game, Tool tool) : base(game)
        {
            _tool = tool;
            DrawOrder = 100;
            _drainValue = 0.0f;
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            var sandGame = Game as Sand;

            if(sandGame != null)
            {
                _spriteBatch = sandGame.SpriteBatch;
            }
        }

        private void UpdateDrainMeter()
        {
            var bitmap = new Bitmap(148, 148);

            var bitmapGraphics = Graphics.FromImage(bitmap);
            bitmapGraphics.Clear(Color.Transparent);
            bitmapGraphics.FillPie(Brushes.White, new Rectangle(0, 0, 148, 148), 270.0f,
                                   (float)(360.0f * (_tool.Energy / _tool.TotalEnergy)));
            bitmapGraphics.Dispose();

            _drainTexture = new Texture2D(GraphicsDevice, bitmap.Width, bitmap.Height, false, SurfaceFormat.Color);
            var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                                       ImageLockMode.ReadOnly, bitmap.PixelFormat);
            var bufferSize = data.Height * data.Stride;
            var bytes = new byte[bufferSize];
            Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
            _drainTexture.SetData(bytes);
            bitmap.UnlockBits(data);
        }

        public override void Draw(GameTime gameTime)
        {
            if(_tool.Energy != _drainValue)
            {
                UpdateDrainMeter();
            }

            _spriteBatch.Draw(_drainTexture, Position, null, Microsoft.Xna.Framework.Color.White, 0.0f,
                              new Vector2(_drainTexture.Width / 2.0f, _drainTexture.Height / 2.0f), 1.0f,
                              SpriteEffects.None, 0);
            _spriteBatch.Draw(_tool.Icon, Position, null, Microsoft.Xna.Framework.Color.White, 0.0f,
                              new Vector2(_tool.Icon.Width / 2.0f, _tool.Icon.Height / 2.0f), 1.0f, SpriteEffects.None,
                              0);
        }
    }
}