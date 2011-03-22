using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sand
{
    internal class LobbyList : DrawableGameComponent
    {
        private SpriteBatch _spriteBatch;
        private Team _team;

        public float X, Y;

        public LobbyList(Game game, Team team) : base(game)
        {
            _team = team;
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

        public override void Update(GameTime gameTime)
        {
        }

        public override void Draw(GameTime gameTime)
        {
            int i = 1;
            var sandGame = Game as Sand;

            if(sandGame == null)
            {
                return;
            }

            foreach(var gamer in Storage.networkSession.AllGamers)
            {
                if((gamer.Tag as Player).Team != _team)
                {
                    continue;
                }

                var nameString = gamer.IsLocal
                                     ? string.Format("{0}", gamer.Gamertag)
                                     : string.Format("{0} ({1} ms)", gamer.Gamertag,
                                                     gamer.RoundtripTime.TotalMilliseconds / 2);

                _spriteBatch.DrawString(Storage.Font("Calibri24"), nameString,
                                        new Vector2(X, i++ * 30 + Y),
                                        Color.White);
            }
        }
    }
}