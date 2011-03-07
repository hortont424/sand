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
            _player = new LocalPlayer(Game);
            Game.Components.Add(_player);
        }

        public override void Update()
        {
            
        }

        public override void Leave()
        {
            Game.Components.Remove(_player);
        }
    }
}