using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
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

            _spriteBatch.DrawString(Storage.Font("Gotham24"), "SAND", new Vector2(200, 20), Color.White);

            foreach(var gamer in Storage.networkSession.AllGamers)
            {
                _spriteBatch.DrawString(Storage.Font("Gotham24"), gamer.Gamertag, new Vector2(20, i++ * 30 + 20), Color.White);
            }

            _spriteBatch.End();
        }
    }
}