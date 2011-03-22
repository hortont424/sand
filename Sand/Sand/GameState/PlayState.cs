namespace Sand.GameState
{
    public class PlayState : GameState
    {
        private Map _gameMap;

        public PlayState(Sand game) : base(game)
        {
        }

        public override void Enter()
        {
            foreach(var gamer in Storage.networkSession.AllGamers)
            {
                Game.Components.Add((Player)gamer.Tag);
            }

            _gameMap = new Map(Game, "01");

            Game.Components.Add(_gameMap);
        }

        public override void Update()
        {
            Messages.Update();
        }

        public override void Leave()
        {
            foreach(var gamer in Storage.networkSession.AllGamers)
            {
                Game.Components.Remove((Player)gamer.Tag);
            }

            Game.Components.Remove(_gameMap);
        }
    }
}