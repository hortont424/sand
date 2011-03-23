using System.Collections.Generic;

namespace Sand.GameState
{
    public class PlayState : GameState
    {
        public PlayState(Sand game) : base(game)
        {
        }

        public override void Enter(Dictionary<string, object> data)
        {
            var sandGame = Game;

            foreach(var gamer in Storage.networkSession.AllGamers)
            {
                Game.Components.Add((Player)gamer.Tag);
            }

            sandGame.GameMap = new Map(Game, "01");

            Game.Components.Add(sandGame.GameMap);
        }

        public override void Update()
        {
            Messages.Update();
        }

        public override Dictionary<string, object> Leave()
        {
            var sandGame = Game;

            foreach(var gamer in Storage.networkSession.AllGamers)
            {
                Game.Components.Remove((Player)gamer.Tag);
            }

            Game.Components.Remove(sandGame.GameMap);

            return null;
        }
    }
}