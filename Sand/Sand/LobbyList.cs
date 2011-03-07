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

            foreach(var gamer in SignedInGamer.SignedInGamers)
            {
                _spriteBatch.DrawString(Storage.Font("Calibri24"), gamer.Gamertag, new Vector2(20, i++ * 30), Color.White);
            }

            _spriteBatch.End();
        }
    }
}