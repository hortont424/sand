using System;
using Microsoft.Xna.Framework.GamerServices;

namespace Sand.GameState
{
    public class PlayState : GameState
    {
        private LocalPlayer _player;

        public PlayState(Sand game) : base(game)
        {
        }

        public override void Enter()
        {
            foreach(var gamer in Storage.networkSession.AllGamers)
            {
                Game.Components.Add((Player)gamer.Tag);
            }
        }

        public override void Update()
        {
            Messages.Update();
        }

        public override void Leave()
        {
            foreach (var gamer in Storage.networkSession.AllGamers)
            {
                Game.Components.Remove((Player)gamer.Tag);
            }
        }
    }
}