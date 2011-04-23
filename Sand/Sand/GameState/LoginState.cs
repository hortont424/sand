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
                return;
            }

            Console.WriteLine("Nobody's signed in, show the guide!");
            SignedInGamer.SignedIn += (o, args) => Game.TransitionState(States.AcquireSession);

            Storage.AnimationController.Add(new Animation { CompletedDelegate = ShowGuide }, 500);
        }

        private void ShowGuide()
        {
            if(!Guide.IsVisible && Gamer.SignedInGamers.Count == 0)
            {
                Guide.ShowSignIn(1, false);
            }
        }
    }
}