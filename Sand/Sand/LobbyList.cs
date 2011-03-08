using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sand
{
    internal class LobbyList : DrawableGameComponent
    {
        private SpriteBatch _spriteBatch;

        public LobbyList(Game game) : base(game)
        {
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        public override void Update(GameTime gameTime)
        {
        }

        public override void Draw(GameTime gameTime)
        {
            int i = 1;

            _spriteBatch.Begin();

            _spriteBatch.Draw(Storage.Sprite("SandLogo"), new Vector2(Game.Window.ClientBounds.Width * 0.2f, 20), Color.White);

            foreach(var gamer in Storage.networkSession.AllGamers)
            {
                _spriteBatch.DrawString(Storage.Font("Calibri24"), string.Format("{0} ({1} ms)", gamer.Gamertag, gamer.RoundtripTime.TotalMilliseconds),
                                        new Vector2(Game.Window.ClientBounds.Width * 0.22f, i++ * 30 + 120),
                                        Color.White);
            }

            _spriteBatch.End();
        }
    }
}