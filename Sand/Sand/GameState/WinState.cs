using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Sand.GameState
{
    internal class WinState : GameState
    {
        private Button _readyButton;
        private Label _label;

        public WinState(Sand game) : base(game)
        {
        }

        public override void Enter(Dictionary<string, object> data)
        {
            _label = new Label(Game, Game.BaseScreenSize.X / 2, Game.BaseScreenSize.Y / 2, "", "Calibri120Bold")
                     { PositionGravity = Gravity.Center };

            var readyButtonRect = new Rectangle(0, 0, 200, 50);
            readyButtonRect.X = (int)Game.BaseScreenSize.X - readyButtonRect.Width - 50;
            readyButtonRect.Y = (int)Game.BaseScreenSize.Y - readyButtonRect.Height - 50;
            _readyButton = new Button(Game, readyButtonRect, "Ready", new Color(0.1f, 0.7f, 0.1f));
            _readyButton.SetAction((a, userInfo) => Continue(), null);

            Game.Components.Add(_readyButton);
            Game.Components.Add(_label);

            foreach (var gamer in Storage.NetworkSession.AllGamers)
            {
                Game.Components.Remove(gamer.Tag as Player);

                if (gamer.IsLocal)
                {
                    gamer.Tag = new LocalPlayer(Game, gamer);
                }
                else
                {
                    gamer.Tag = new RemotePlayer(Game, gamer);
                }
            }

            Storage.SandParticles.Particles.Clear();
            Storage.AnimationController.Clear();

            Storage.Game.Effect.CurrentTechnique = Storage.Game.Effect.Techniques["None"];
        }

        private void Continue()
        {
            Storage.Scores[Team.Red] = Storage.Scores[Team.Blue] = 0;

            Game.TransitionState(States.Lobby);
        }

        public override void Update()
        {
            Messages.Update();

            Console.WriteLine("{0} {1}", Storage.Scores[Team.Red], Storage.Scores[Team.Blue]);

            _label.Text = (Storage.Scores[Team.Red] == 3 ? "Purple" : "Green") + " Wins!";
        }

        public override Dictionary<string, object> Leave()
        {
            Game.Components.Remove(_readyButton);
            Game.Components.Remove(_label);

            return null;
        }
    }
}