using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.GamerServices;

namespace Sand.GameState
{
    public class LoginState : GameState
    {
        public LoginState(Sand game) : base(game)
        {
        }

        public override void Enter(Dictionary<string, object> data)
        {
            if(Gamer.SignedInGamers.Count > 0)
            {
                Console.WriteLine("Player already signed in!");
                Game.TransitionState(States.AcquireSession);
            }

            if(!Guide.IsVisible)
            {
                Console.WriteLine("Nobody's signed in, show the guide!");
                SignedInGamer.SignedIn += (o, args) => Game.TransitionState(States.AcquireSession);
                Guide.ShowSignIn(1, false);
            }
        }
    }
}