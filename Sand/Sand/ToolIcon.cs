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
        private Texture2D _drainTexture;
        private double _drainValue;
        private readonly Bitmap _bitmap;
        private readonly Graphics _graphics;
        private byte[] _bitmapBytes;

        private bool _disabled;
        private bool _sawDisabled = false;
        public float Scale { get; set; }

        public bool Disabled
        {
            get
            {
                return _disabled;
            }
            set
            {
                var oldDisabled = _disabled;
                _disabled = value;

                var newScale = _disabled ? 0.75f : 1.0f;

                if(_sawDisabled == false)
                {
                    _sawDisabled = true;
                    Scale = newScale;
                    return;
                }

                if(_disabled != oldDisabled)
                {
                    Storage.AnimationController.Add(new Animation(this, "Scale", newScale), 150);
                }
            }
        }

        public ToolIcon(Game game, Tool tool) : base(game)
        {
            DrawOrder = 100;

            _tool = tool;
            _drainValue = 0.0f;

            _bitmap = new Bitmap(128 + 20 + 6, 128 + 20 + 6);
            _graphics = Graphics.FromImage(_bitmap);
            _graphics.SmoothingMode = SmoothingMode.HighQuality;
            _graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            _drainTexture = null;
            _bitmapBytes = null;

            Scale = 1.0f;
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            _drainTexture = new Texture2D(GraphicsDevice, _bitmap.Width, _bitmap.Height, false, SurfaceFormat.Color);
        }

        private void UpdateDrainMeter()
        {
            _drainValue = _tool.Energy;

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

            var mulColorComponent = (Scale - 0.75f) * 3 + 0.25f;
            var mulColor = new Microsoft.Xna.Framework.Color(mulColorComponent, mulColorComponent, mulColorComponent);

            if(_drainTexture != null && _tool.Energy > 0.0f)
            {
                _spriteBatch.Draw(_drainTexture,
                                  Position, 
                                  null, mulColor, 0.0f,
                                  new Vector2(_drainTexture.Width / 2.0f, _drainTexture.Height / 2.0f),
                                  Scale,
                                  SpriteEffects.None, 0);
            }

            _spriteBatch.Draw(_tool.Icon, Position, null, mulColor, 0.0f,
                new Vector2(_tool.Icon.Width / 2.0f, _tool.Icon.Height / 2.0f), Scale, SpriteEffects.None,
                              0);
        }
    }
}