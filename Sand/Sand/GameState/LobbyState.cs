using Microsoft.Xna.Framework;

namespace Sand.GameState
{
    public class LobbyState : GameState
    {
        private LobbyList _lobbyList;
        private Button _readyButton;
        private Button _redTeamButton;
        private Button _blueTeamButton;
        private Billboard _sandLogo;

        public LobbyState(Sand game) : base(game)
        {
        }

        public override void Enter()
        {
            var netPlayer = Storage.networkSession.LocalGamers[0];
            var player = netPlayer.Tag as Player;

            _lobbyList = new LobbyList(Game);

            var logoSprite = Storage.Sprite("SandLogo");
            var sandLogoOrigin = new Vector2(Game.BaseScreenSize.X * 0.5f - (logoSprite.Width * 0.5f), 30);

            _sandLogo = new Billboard(Game, sandLogoOrigin, logoSprite);

            var readyButtonRect = new Rectangle(0, 0, 200, 50);
            readyButtonRect.X = (int)Game.BaseScreenSize.X - readyButtonRect.Width - 50;
            readyButtonRect.Y = (int)Game.BaseScreenSize.Y - readyButtonRect.Height - 50;

            _readyButton = new Button(Game, readyButtonRect, "Ready", new Color(0.1f, 0.7f, 0.1f));
            _readyButton.SetAction((a, userInfo) => Game.TransitionState(States.ReadyWait), null);

            var redTeamButtonRect = new Rectangle(0, 0, 200, 50);
            redTeamButtonRect.X = (int)(Game.BaseScreenSize.X / 3.0f) - (redTeamButtonRect.Width / 2);
            redTeamButtonRect.Y = (int)(logoSprite.Height + sandLogoOrigin.Y + 30);

            _redTeamButton = new Button(Game, redTeamButtonRect, "Choose Team", Storage.Color("RedTeam"));
            _redTeamButton.SetAction((a, userInfo) => player.Team = Team.Red, null);

            var blueTeamButtonRect = new Rectangle(0, 0, 200, 50);
            blueTeamButtonRect.X = (int)(2.0f * Game.BaseScreenSize.X / 3.0f) - (blueTeamButtonRect.Width / 2);
            blueTeamButtonRect.Y = redTeamButtonRect.Y;

            _blueTeamButton = new Button(Game, blueTeamButtonRect, "Choose Team", Storage.Color("BlueTeam"));
            _blueTeamButton.SetAction((a, userInfo) => player.Team = Team.Blue, null);

            Game.Components.Add(_sandLogo);
            Game.Components.Add(_lobbyList);
            Game.Components.Add(_readyButton);
            Game.Components.Add(_redTeamButton);
            Game.Components.Add(_blueTeamButton);
        }

        public override void Update()
        {
        }

        public override void Leave()
        {
            Game.Components.Remove(_sandLogo);
            Game.Components.Remove(_lobbyList);
            Game.Components.Remove(_readyButton);
            Game.Components.Remove(_redTeamButton);
            Game.Components.Remove(_blueTeamButton);
        }
    }
}