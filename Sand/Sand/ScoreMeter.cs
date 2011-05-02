using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sand
{
    internal class ScoreMeter : Actor
    {
        private readonly Team _team;

        public ScoreMeter(Game game, Team team)
            : base(game)
        {
            _team = team;
            PositionGravity = Gravity.Center;
        }

        public override void Draw(GameTime gameTime)
        {
            DrawScore(string.Format("{0:0}", Storage.Scores[_team]), "Calibri120Bold");
        }

        public void DrawScore(string str, string font)
        {
            var textSize = Storage.Font(font).MeasureString(str);
            var textOrigin = new Vector2(textSize.X, textSize.Y) * Gravity.Offset(PositionGravity);

            _spriteBatch.DrawString(Storage.Font(font), str,
                                    new Vector2(X, Y),
                                    Teams.ColorForTeam(_team), 0.0f, textOrigin, 1.0f, SpriteEffects.None, 0.0f);
        }
    }
}