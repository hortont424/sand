using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;

namespace Sand
{
    internal class RemotePlayer : Player
    {
        public RemotePlayer(Game game, NetworkGamer gamer) : base(game, gamer)
        {
        }
    }
}