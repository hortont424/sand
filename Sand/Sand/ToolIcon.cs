using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sand.Tools;
using Color = System.Drawing.Color;
using Rectangle = System.Drawing.Rectangle;

namespace Sand
{
    internal class ToolIcon : Actor
    {
        private readonly Tool _tool;
        private SpriteBatch _spriteBatch;
        private Texture2D _drainTexture;
        private double _drainValue;
        private Bitmap _bitmap;
        private Graphics _graphics;
        private byte[] _bitmapBytes;

        public Vector2 Position { get; set; }

        public ToolIcon(Game game, Tool tool) : base(game)
        {
            DrawOrder = 100;

            _tool = tool;
            _drainValue = 0.0f;

            _bitmap = new Bitmap(128 + 20 + 6, 128 + 20 + 6);
            _graphics = Graphics.FromImage(_bitmap);
            _graphics.SmoothingMode = SmoothingMode.HighQuality;
            _drainTexture = null;
            _bitmapBytes = null;
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            var sandGame = Game as Sand;

            if(sandGame != null)
            {
                _spriteBatch = sandGame.SpriteBatch;
            }

            _drainTexture = new Texture2D(GraphicsDevice, _bitmap.Width, _bitmap.Height, false, SurfaceFormat.Color);
        }

        private void UpdateDrainMeter()
        {
            _graphics.Clear(Color.Transparent);
            _graphics.FillPie(Brushes.White, new Rectangle(3, 3, _bitmap.Width - 6, _bitmap.Height - 6), 270.0f,
                              (float)(360.0f * (_tool.Energy / _tool.TotalEnergy)));
            _graphics.Flush();

            var data = _bitmap.LockBits(new Rectangle(0, 0, _bitmap.Width, _bitmap.Height),
                                        ImageLockMode.ReadOnly, _bitmap.PixelFormat);

            if(_bitmapBytes == null)
            {
                _bitmapBytes = new byte[data.Height * data.Stride];
            }

            Marshal.Copy(data.Scan0, _bitmapBytes, 0, _bitmapBytes.Length);
            _drainTexture.SetData(_bitmapBytes);
            _bitmap.UnlockBits(data);
        }

        public override void Draw(GameTime gameTime)
        {
            if(_tool.Energy != _drainValue)
            {
                UpdateDrainMeter();
            }

            if(_drainTexture != null && _tool.Energy > 0.0f)
            {
                _spriteBatch.Draw(_drainTexture,
                                  Position,
                                  null, Microsoft.Xna.Framework.Color.White, 0.0f,
                                  new Vector2(_drainTexture.Width / 2.0f, _drainTexture.Height / 2.0f),
                                  1.0f,
                                  SpriteEffects.None, 0);
            }

            _spriteBatch.Draw(_tool.Icon, Position, null, Microsoft.Xna.Framework.Color.White, 0.0f,
                              new Vector2(_tool.Icon.Width / 2.0f, _tool.Icon.Height / 2.0f), 1.0f, SpriteEffects.None,
                              0);
        }
    }
}