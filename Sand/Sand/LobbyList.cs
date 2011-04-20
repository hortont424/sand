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
            var location = 2;

            DrawName("Players", "Calibri24Bold", 1);

            foreach(var gamer in Storage.NetworkSession.AllGamers)
            {
                var nameString = string.Format("{0}", gamer.Gamertag);
                DrawName(nameString, "Calibri24", location);
                location++;
            }
        }

        public void DrawName(string name, string font, int location)
        {
            var textSize = Storage.Font(font).MeasureString(name);
            var textOrigin = new Vector2(textSize.X, textSize.Y) * Gravity.Offset(PositionGravity);

            _spriteBatch.DrawString(Storage.Font(font), name,
                                    new Vector2(X, location * 40 + Y),
                                    Color.White, 0.0f, textOrigin, 1.0f, SpriteEffects.None, 0.0f);
        }
    }
}