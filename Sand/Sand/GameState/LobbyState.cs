using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;

namespace Sand.GameState
{
    public class LobbyState : GameState
    {
        private LobbyList _lobbyListNone;
        private Button _readyButton;
        private Button _noTeamButton;
        private Billboard _sandLogo;
        private PlayerClassButton _redSupportButton, _redDefenseButton, _redOffenseButton;
        private PlayerClassButton _blueSupportButton, _blueDefenseButton, _blueOffenseButton;

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
            Storage.NetworkSession.GamerJoined += GamerJoined;
            Storage.NetworkSession.GamerLeft += GamerLeft;

            var netPlayer = Storage.NetworkSession.LocalGamers[0];
            var player = netPlayer.Tag as Player;

            if(player == null)
            {
                throw new Exception("Player not inited!?");
            }

            var logoSprite = Storage.Sprite("SandLogo");
            var sandLogoOrigin = new Vector2(Game.BaseScreenSize.X * 0.5f - (logoSprite.Width * 0.5f), 30);

            _sandLogo = data["SandLogo"] as Billboard ?? new Billboard(Game, sandLogoOrigin, logoSprite);

            Storage.AnimationController.Add(new Animation(_sandLogo, "Y", sandLogoOrigin.Y), 750);

            var readyButtonRect = new Rectangle(0, 0, 200, 50);
            readyButtonRect.X = (int)Game.BaseScreenSize.X - readyButtonRect.Width - 50;
            readyButtonRect.Y = (int)Game.BaseScreenSize.Y - readyButtonRect.Height - 50;
            _readyButton = new Button(Game, readyButtonRect, "Ready", new Color(0.1f, 0.7f, 0.1f));
            _readyButton.SetAction((a, userInfo) => Game.TransitionState(States.ReadyWait), null);

            var noTeamButtonRect = new Rectangle(0, 0, 200, 50);
            noTeamButtonRect.X = 20;
            noTeamButtonRect.Y = (int)(logoSprite.Height + sandLogoOrigin.Y + 30);
            _noTeamButton = new Button(Game, noTeamButtonRect, "No Team", Storage.Color("NeutralTeam"));

            _lobbyListNone = new LobbyList(Game, Team.None) { X = 20, Y = noTeamButtonRect.Y + 40 };

            var playerClassOrigin = new Vector2((Game.BaseScreenSize.X / 2.0f) - 128, sandLogoOrigin.Y + logoSprite.Height + 32);

            _redDefenseButton = new PlayerClassButton(Game, new Vector2(playerClassOrigin.X - 256, playerClassOrigin.Y), Class.Defense, Team.Red);
            Game.Components.Add(_redDefenseButton);
            _redOffenseButton = new PlayerClassButton(Game, new Vector2(playerClassOrigin.X - 256, playerClassOrigin.Y + 256 + 32), Class.Offense, Team.Red);
            Game.Components.Add(_redOffenseButton);
            _redSupportButton = new PlayerClassButton(Game, new Vector2(playerClassOrigin.X - 256, playerClassOrigin.Y + 512 + 64), Class.Support, Team.Red);
            Game.Components.Add(_redSupportButton);

            _blueDefenseButton = new PlayerClassButton(Game, new Vector2(playerClassOrigin.X + 256, playerClassOrigin.Y), Class.Defense, Team.Blue);
            Game.Components.Add(_blueDefenseButton);
            _blueOffenseButton = new PlayerClassButton(Game, new Vector2(playerClassOrigin.X + 256, playerClassOrigin.Y + 256 + 32), Class.Offense, Team.Blue);
            Game.Components.Add(_blueOffenseButton);
            _blueSupportButton = new PlayerClassButton(Game, new Vector2(playerClassOrigin.X + 256, playerClassOrigin.Y + 512 + 64), Class.Support, Team.Blue);
            Game.Components.Add(_blueSupportButton);

            Game.Components.Add(_sandLogo);
            Game.Components.Add(_readyButton);
            Game.Components.Add(_noTeamButton);
        }

        public override void Update()
        {
            Messages.Update();
        }

        public override Dictionary<string, object> Leave()
        {
            Game.Components.Remove(_sandLogo);
            Game.Components.Remove(_lobbyListNone);
            Game.Components.Remove(_readyButton);
            Game.Components.Remove(_noTeamButton);

            Game.Components.Remove(_redDefenseButton);
            Game.Components.Remove(_redOffenseButton);
            Game.Components.Remove(_redSupportButton);

            Game.Components.Remove(_blueDefenseButton);
            Game.Components.Remove(_blueOffenseButton);
            Game.Components.Remove(_blueSupportButton);

            return null;
        }

        public override bool CanLeave()
        {
            var netPlayer = Storage.NetworkSession.LocalGamers[0];
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