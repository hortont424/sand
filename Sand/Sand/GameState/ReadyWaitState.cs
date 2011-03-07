using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.GamerServices;

namespace Sand.GameState
{
    public class ReadyWaitState : GameState
    {
        //Dictionary<,> 

        public ReadyWaitState(Sand game) : base(game)
        {
        }

        public override void Enter()
        {
            if(!Storage.networkSession.IsHost)
            {
                Storage.networkSession.LocalGamers[0].IsReady = true;
                //Storage.packetWriter.Write("ready");
            }
        }

        public override void Update()
        {
            bool allReady = true;

            foreach(var player in Storage.networkSession.AllGamers)
            {
                if (!player.IsReady)
                    allReady = false;
            }

            if(allReady)
            {
                Game.TransitionState(States.Play);
            }
        }

        public override void Leave()
        {
        }
    }
}