namespace Sand.GameState
{
    public class PlayState : GameState
    {
        public PlayState(Sand game) : base(game)
        {
        }

        public override void Enter()
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

        public override void Leave()
        {
            var sandGame = Game;

            foreach(var gamer in Storage.networkSession.AllGamers)
            {
                Game.Components.Remove((Player)gamer.Tag);
            }

            Game.Components.Remove(sandGame.GameMap);
        }
    }
}