using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;

namespace Sand.GameState
{
    public class LoadingState : GameState
    {
        private Billboard _sandLogo;
        private Label _readyLabel;

        public LoadingState(Sand game) : base(game)
        {
        }

        public override void Enter(Dictionary<string, object> data)
        {
            var logoSprite = Storage.Sprite("SandLogo");
            var sandLogoOrigin = new Vector2(Game.BaseScreenSize.X * 0.5f - (logoSprite.Width * 0.5f), 300);

            _sandLogo = new Billboard(Game, sandLogoOrigin, logoSprite);
            _readyLabel = new Label(Game, Game.BaseScreenSize.X * 0.5f, Game.BaseScreenSize.Y * 0.5f,
                                    "Loading...", "Calibri48Bold") { PositionGravity = Gravity.Center };

            Game.Components.Add(_sandLogo);
            Game.Components.Add(_readyLabel);

            DoSignIn();
        }

        private void DoSignIn()
        {
            if (Gamer.SignedInGamers.Count > 0)
            {
                Console.WriteLine("Player already signed in!");
                FindSession();
                return;
            }

            Console.WriteLine("Nobody's signed in, show the guide!");
            SignedInGamer.SignedIn += (o, args) => FindSession();

            Storage.AnimationController.Add(new Animation { CompletedDelegate = ShowGuide }, 500);
        }

        private void ShowGuide()
        {
            if (!Guide.IsVisible && Gamer.SignedInGamers.Count == 0)
            {
                Guide.ShowSignIn(1, false);
            }
        }

        private void FindSession()
        {
            ThreadStart networkThread = delegate
            {
                // Try to find a Sand server. If there isn't one, start one!

                var availableSessions = NetworkSession.Find(NetworkSessionType.SystemLink, 1, null);

                if (availableSessions.Count > 0)
                {
                    Console.WriteLine("Connecting to server from {0}", availableSessions[0].HostGamertag);
                    Storage.NetworkSession = NetworkSession.Join(availableSessions[0]);
                }
                else
                {
                    Console.WriteLine("Couldn't find a server! Starting one...");
                    Storage.NetworkSession = NetworkSession.Create(NetworkSessionType.SystemLink, 1, 6);
                }

                foreach (var gamer in Storage.NetworkSession.AllGamers)
                {
                    if (gamer.IsLocal)
                    {
                        gamer.Tag = new LocalPlayer(Storage.Game, gamer);
                    }
                }

                if (Storage.NetworkSession != null)
                {
                    Storage.Game.DoneAcquiringSession = true;
                }
                else
                {
                    Console.WriteLine("Failed to get a session!");
                }
            };

            new Thread(networkThread).Start();
        }

        public override void Update()
        {
            if(Storage.Game.DoneLoading == 3 && Storage.Game.DoneAcquiringSession)
            {
                Storage.Game.TransitionState(States.InitialReady);
            }
        }

        public override Dictionary<string, object> Leave()
        {
            Game.Components.Remove(_sandLogo);
            Game.Components.Remove(_readyLabel);

            return new Dictionary<string, object> { { "SandLogo", _sandLogo } };
        }
    }
}