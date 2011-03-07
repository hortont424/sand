using Microsoft.Xna.Framework.GamerServices;

namespace Sand.GameState
{
    public class LoginState : GameState
    {
        public LoginState(Sand game) : base(game)
        {
        }

        public override void Enter()
        {
            if(!Guide.IsVisible)
            {
                SignedInGamer.SignedIn += (o, args) => Game.TransitionState(States.AcquireSession);
                Guide.ShowSignIn(1, false);
            }
        }

        public override void Leave()
        {
        }
    }
}