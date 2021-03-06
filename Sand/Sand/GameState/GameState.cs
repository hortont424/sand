﻿using System.Collections.Generic;

namespace Sand.GameState
{
    public enum States
    {
        Begin = 0,
        InitialReady,
        Lobby,
        Loadout,
        ReadyWait,
        Play,
        ChooseMap,
        Loading,
        Win,
        Tutorial
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