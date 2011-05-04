using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;

namespace Sand.GameState
{
    public class LobbyState : GameState
    {
        private Button _readyButton;
        private Billboard _sandLogo;
        private PlayerClassButton _redSupportButton, _redDefenseButton, _redOffenseButton;
        private PlayerClassButton _blueSupportButton, _blueDefenseButton, _blueOffenseButton;
        private ClassDescription[] _classDescriptions;

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

        private string AddRandomSuffix(string str)
        {
            return String.Format("{0}{1}", str, Storage.Random.Next(1, 4));
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

            _sandLogo = data.ContainsKey("SandLogo") ? data["SandLogo"] as Billboard : new Billboard(Game, sandLogoOrigin, logoSprite);

            Storage.AnimationController.Add(new Animation(_sandLogo, "Y", sandLogoOrigin.Y), 750);

            var readyButtonRect = new Rectangle(0, 0, 200, 50);
            readyButtonRect.X = (int)Game.BaseScreenSize.X - readyButtonRect.Width - 50;
            readyButtonRect.Y = (int)Game.BaseScreenSize.Y - readyButtonRect.Height - 50;
            _readyButton = new Button(Game, readyButtonRect, "Ready", new Color(0.1f, 0.7f, 0.1f));
            _readyButton.SetAction(
                (a, userInfo) => Game.TransitionState(Storage.NetworkSession.IsHost ? States.ChooseMap : States.Loadout),
                null);

            var playerClassOrigin = new Vector2((Game.BaseScreenSize.X / 2.0f) - 128,
                                                sandLogoOrigin.Y + logoSprite.Height + 32);

            // TODO: this code needs to be cleaned up a lot
            // TODO: make sure we aren't clicking on a taken combination

            _redDefenseButton = new PlayerClassButton(Game, new Vector2(playerClassOrigin.X - 256, playerClassOrigin.Y),
                                                      Class.Defense, Team.Red);
            _redDefenseButton.Button.SetAction((a, userInfo) =>
                                               {
                                                   player.Team = Team.Red;
                                                   player.Class = Class.Defense;
                                                   Sound.OneShot(AddRandomSuffix("DefenseClass"), false);
                                               }, null);
            Game.Components.Add(_redDefenseButton);
            _redOffenseButton = new PlayerClassButton(Game,
                                                      new Vector2(playerClassOrigin.X - 256,
                                                                  playerClassOrigin.Y + 256 + 32), Class.Offense,
                                                      Team.Red);
            _redOffenseButton.Button.SetAction((a, userInfo) =>
                                               {
                                                   player.Team = Team.Red;
                                                   player.Class = Class.Offense;
                                                   Sound.OneShot(AddRandomSuffix("OffenseClass"), false);
                                               }, null);
            Game.Components.Add(_redOffenseButton);
            _redSupportButton = new PlayerClassButton(Game,
                                                      new Vector2(playerClassOrigin.X - 256,
                                                                  playerClassOrigin.Y + 512 + 64), Class.Support,
                                                      Team.Red);
            _redSupportButton.Button.SetAction((a, userInfo) =>
                                               {
                                                   player.Team = Team.Red;
                                                   player.Class = Class.Support;
                                                   Sound.OneShot(AddRandomSuffix("SupportClass"), false);
                                               }, null);
            Game.Components.Add(_redSupportButton);

            _blueDefenseButton = new PlayerClassButton(Game, new Vector2(playerClassOrigin.X + 256, playerClassOrigin.Y),
                                                       Class.Defense, Team.Blue);
            _blueDefenseButton.Button.SetAction((a, userInfo) =>
                                                {
                                                    player.Team = Team.Blue;
                                                    player.Class = Class.Defense;
                                                    Sound.OneShot(AddRandomSuffix("DefenseClass"), false);
                                                }, null);
            Game.Components.Add(_blueDefenseButton);
            _blueOffenseButton = new PlayerClassButton(Game,
                                                       new Vector2(playerClassOrigin.X + 256,
                                                                   playerClassOrigin.Y + 256 + 32), Class.Offense,
                                                       Team.Blue);
            _blueOffenseButton.Button.SetAction((a, userInfo) =>
                                                {
                                                    player.Team = Team.Blue;
                                                    player.Class = Class.Offense;
                                                    Sound.OneShot(AddRandomSuffix("OffenseClass"), false);
                                                }, null);
            Game.Components.Add(_blueOffenseButton);
            _blueSupportButton = new PlayerClassButton(Game,
                                                       new Vector2(playerClassOrigin.X + 256,
                                                                   playerClassOrigin.Y + 512 + 64), Class.Support,
                                                       Team.Blue);
            _blueSupportButton.Button.SetAction((a, userInfo) =>
                                                {
                                                    player.Team = Team.Blue;
                                                    player.Class = Class.Support;
                                                    Sound.OneShot(AddRandomSuffix("SupportClass"), false);
                                                }, null);
            Game.Components.Add(_blueSupportButton);

            _classDescriptions = new ClassDescription[3];

            _classDescriptions[0] = new ClassDescription(Game, Class.Defense)
                                    {
                                        X =
                                            (_blueSupportButton.Button.X + _redSupportButton.Button.X +
                                             _redSupportButton.Button.Width) / 2,
                                        Y = _redDefenseButton.Button.Y
                                    };

            _classDescriptions[1] = new ClassDescription(Game, Class.Offense)
                                    {
                                        X = _classDescriptions[0].X,
                                        Y = _redOffenseButton.Button.Y
                                    };

            _classDescriptions[2] = new ClassDescription(Game, Class.Support)
                                    {
                                        X = _classDescriptions[0].X,
                                        Y = _redSupportButton.Button.Y
                                    };

            Game.Components.Add(_sandLogo);
            Game.Components.Add(_readyButton);
            Game.Components.Add(_classDescriptions[0]);
            Game.Components.Add(_classDescriptions[1]);
            Game.Components.Add(_classDescriptions[2]);
        }

        public override void Update()
        {
            Messages.Update();
        }

        public override Dictionary<string, object> Leave()
        {
            Storage.NetworkSession.GamerJoined -= GamerJoined;
            Storage.NetworkSession.GamerLeft -= GamerLeft;

            Game.Components.Remove(_sandLogo);
            Game.Components.Remove(_readyButton);
            Game.Components.Remove(_classDescriptions[0]);
            Game.Components.Remove(_classDescriptions[1]);
            Game.Components.Remove(_classDescriptions[2]);

            Game.Components.Remove(_redDefenseButton);
            Game.Components.Remove(_redOffenseButton);
            Game.Components.Remove(_redSupportButton);

            Game.Components.Remove(_blueDefenseButton);
            Game.Components.Remove(_blueOffenseButton);
            Game.Components.Remove(_blueSupportButton);

            var returns = new Dictionary<string, object>();
            returns["SandLogo"] = _sandLogo;

            Storage.SandParticles = new ParticleSystem(Game, Storage.NetworkSession.LocalGamers[0].Tag as Player, true);

            return returns;
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