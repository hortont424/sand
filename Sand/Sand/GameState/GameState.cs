namespace Sand.GameState
{
    public enum States
    {
        Begin = 0,
        Login,
        AcquireSession,
        Lobby,
        Play
    }

    public abstract class GameState
    {
        public Sand Game;

        protected GameState(Sand game)
        {
            Game = game;
        }

        public abstract void Enter();

        public abstract void Leave();
    }
}