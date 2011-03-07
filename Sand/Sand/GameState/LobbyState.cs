using Microsoft.Xna.Framework;

namespace Sand.GameState
{
    public class LobbyState : GameState
    {
        private LobbyList _lobbyList;
        private Button _readyButton;

        public LobbyState(Sand game) : base(game)
        {
        }

        public override void Enter()
        {
            _lobbyList = new LobbyList(Game);
            _readyButton = new Button(Game, new Rectangle(20, 20, 200, 50));

            Game.Components.Add(_lobbyList);
            Game.Components.Add(_readyButton);
        }

        public override void Leave()
        {
            Game.Components.Remove(_lobbyList);
            Game.Components.Remove(_readyButton);
        }
    }
}