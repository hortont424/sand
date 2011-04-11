using System.Collections.Generic;

namespace Sand.GameState
{
    public enum States
    {
        Begin = 0,
        Login,
        AcquireSession,
        InitialReady,
        Lobby,
        Loadout,
        ReadyWait,
        Play
    }

    public abstract class GameState
    {
        public Sand Game;

        protected GameState(Sand game)
        {
            Game = game;
        }

        public virtual void Enter(Dictionary<string, object> data)
        {
            
        }

        public virtual void Update()
        {
            
        }

        public virtual Dictionary<string,object> Leave()
        {
            return null;
        }

        public virtual bool CanLeave()
        {
            return true;
        }

        public virtual bool CanEnter()
        {
            return true;
        }
    }
}