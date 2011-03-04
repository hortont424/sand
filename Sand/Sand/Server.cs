using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;

namespace Sand
{
    class Server : Game
    {
        private NetworkSession _networkSession;

        public Server()
        {
            _networkSession = NetworkSession.Create(NetworkSessionType.SystemLink, 6, 6);
        }

        protected override void Update(GameTime gameTime)
        {
            _networkSession.Update();
            base.Update(gameTime);
        }
    }
}
