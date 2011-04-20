using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sand
{
    internal class LobbyList : Actor
    {
        public LobbyList(Game game) : base(game)
        {
            PositionGravity = Gravity.Center;
        }

        public override void Draw(GameTime gameTime)
        {
            var i = 1;

            foreach(var gamer in Storage.NetworkSession.AllGamers)
            {
                var nameString = string.Format("{0}", gamer.Gamertag);
                var textSize = Storage.Font("Calibri24").MeasureString(nameString);
                var textOrigin = new Vector2(textSize.X, textSize.Y) * Gravity.Offset(PositionGravity);

                _spriteBatch.DrawString(Storage.Font("Calibri24"), nameString,
                                        new Vector2(X, i++ * 30 + Y),
                                        Color.White, 0.0f, textOrigin, 1.0f, SpriteEffects.None, 0.0f);
            }
        }
    }
}