using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;

namespace Sand.GameState
{
    public class LobbyState : GameState
    {
        private LobbyList _lobbyListRed, _lobbyListBlue, _lobbyListNone;
        private Button _readyButton;
        private Button _noTeamButton;
        private Button _redTeamButton;
        private Button _blueTeamButton;
        private Billboard _sandLogo;

        public LobbyState(Sand game) : base(game)
        {
        }

        private void GamerJoined(object sender, GamerJoinedEventArgs e)
        {
            var gamer = e.Gamer;

            if(gamer.IsLocal)
            {
                return;
            }

            var player = new RemotePlayer(Game, gamer);
            gamer.Tag = player;
        }

        private void GamerLeft(object sender, GamerLeftEventArgs e)
        {
        }

        public override void Enter(Dictionary<string, object> data)
        {
            Storage.networkSession.GamerJoined += GamerJoined;
            Storage.networkSession.GamerLeft += GamerLeft;

            var netPlayer = Storage.networkSession.LocalGamers[0];
            var player = netPlayer.Tag as Player;

            if(player == null)
            {
                throw new Exception("Player not inited!?");
            }

            var logoSprite = Storage.Sprite("SandLogo");
            var sandLogoOrigin = new Vector2(Game.BaseScreenSize.X * 0.5f - (logoSprite.Width * 0.5f), 30);

            _sandLogo = data["SandLogo"] as Billboard ?? new Billboard(Game, sandLogoOrigin, logoSprite);

            Storage.animationController.Add(new Animation(_sandLogo, "Y", sandLogoOrigin.Y), 750);

            var readyButtonRect = new Rectangle(0, 0, 200, 50);
            readyButtonRect.X = (int)Game.BaseScreenSize.X - readyButtonRect.Width - 50;
            readyButtonRect.Y = (int)Game.BaseScreenSize.Y - readyButtonRect.Height - 50;
            _readyButton = new Button(Game, readyButtonRect, "Ready", new Color(0.1f, 0.7f, 0.1f));
            _readyButton.SetAction((a, userInfo) => Game.TransitionState(States.ReadyWait), null);

            var noTeamButtonRect = new Rectangle(0, 0, 200, 50);
            noTeamButtonRect.X = 20;
            noTeamButtonRect.Y = (int)(logoSprite.Height + sandLogoOrigin.Y + 30);
            _noTeamButton = new Button(Game, noTeamButtonRect, "No Team", Storage.Color("NeutralTeam"));

            var redTeamButtonRect = new Rectangle(0, 0, 200, 50);
            redTeamButtonRect.X = (int)(Game.BaseScreenSize.X / 3.0f) - (redTeamButtonRect.Width / 2);
            redTeamButtonRect.Y = noTeamButtonRect.Y;
            _redTeamButton = new Button(Game, redTeamButtonRect, "Choose Team", Storage.Color("RedTeam"));
            _redTeamButton.SetAction((a, userInfo) => player.Team = Team.Red, null);

            var blueTeamButtonRect = new Rectangle(0, 0, 200, 50);
            blueTeamButtonRect.X = (int)(2.0f * Game.BaseScreenSize.X / 3.0f) - (blueTeamButtonRect.Width / 2);
            blueTeamButtonRect.Y = noTeamButtonRect.Y;
            _blueTeamButton = new Button(Game, blueTeamButtonRect, "Choose Team", Storage.Color("BlueTeam"));
            _blueTeamButton.SetAction((a, userInfo) => player.Team = Team.Blue, null);

            _lobbyListNone = new LobbyList(Game, Team.None) { X = 20, Y = redTeamButtonRect.Y + 40 };
            _lobbyListRed = new LobbyList(Game, Team.Red) { X = redTeamButtonRect.X, Y = redTeamButtonRect.Y + 40 };
            _lobbyListBlue = new LobbyList(Game, Team.Blue) { X = blueTeamButtonRect.X, Y = blueTeamButtonRect.Y + 40 };

            Game.Components.Add(_sandLogo);
            Game.Components.Add(_lobbyListNone);
            Game.Components.Add(_lobbyListRed);
            Game.Components.Add(_lobbyListBlue);
            Game.Components.Add(_readyButton);
            Game.Components.Add(_noTeamButton);
            Game.Components.Add(_redTeamButton);
            Game.Components.Add(_blueTeamButton);
        }

        public override void Update()
        {
            Messages.Update();
        }

        public override Dictionary<string, object> Leave()
        {
            Game.Components.Remove(_sandLogo);
            Game.Components.Remove(_lobbyListNone);
            Game.Components.Remove(_lobbyListRed);
            Game.Components.Remove(_lobbyListBlue);
            Game.Components.Remove(_readyButton);
            Game.Components.Remove(_noTeamButton);
            Game.Components.Remove(_redTeamButton);
            Game.Components.Remove(_blueTeamButton);

            return null;
        }

        public override bool CanLeave()
        {
            var netPlayer = Storage.networkSession.LocalGamers[0];
            var player = netPlayer.Tag as Player;

            if(player == null)
            {
                throw new Exception("Player not inited!?");
            }

            if(player.Team == Team.None)
            {
                return false;
            }

            return true;
        }
    }
}