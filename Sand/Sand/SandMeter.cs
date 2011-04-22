using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sand
{
    internal class SandMeter : Actor
    {
        public Team Team;
        private readonly Color _fillColor, _borderColor, _darkColor;

        public SandMeter(Game game, Team team) : base(game)
        {
            Team = team;

            _fillColor = Teams.ColorForTeam(Team);

            double hue, saturation, value;
            SandColor.ToHSV(_fillColor, out hue, out saturation, out value);

            _borderColor = SandColor.FromHSV(hue, saturation, Math.Max(value - 0.3, 0.0));
            _darkColor = SandColor.FromHSV(hue, Math.Max(saturation - 0.4, 0.0), 0.3);

            Width = 100;
            Height = 600;
        }

        public override void Draw(GameTime gameTime)
        {
            const int borderRadius = 5;

            _spriteBatch.Draw(Storage.Sprite("pixel"),
                              new Rectangle(Bounds.X, Bounds.Y,
                                            Bounds.Width + (2 * borderRadius), Bounds.Height + (2 * borderRadius)),
                              null, _borderColor, 0.0f, Gravity.Offset(Gravity.Center), SpriteEffects.None, 0.0f);
            _spriteBatch.Draw(Storage.Sprite("pixel"), Bounds,
                              null, _darkColor, 0.0f, Gravity.Offset(Gravity.Center), SpriteEffects.None, 0.0f);
            _spriteBatch.Draw(Storage.Sprite("pixel"), new Rectangle(Bounds.X, (int)(Bounds.Y + (Bounds.Height / 2.0f)), Bounds.Width, 50),
                              null, _fillColor, 0.0f, new Vector2(0.5f, 1.0f), SpriteEffects.None, 0.0f);
        }
    }
}