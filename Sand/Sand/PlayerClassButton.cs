using Microsoft.Xna.Framework;

namespace Sand
{
    internal class PlayerClassButton : Actor
    {
        public readonly Button Button;
        public readonly Label Label;
        private readonly Class _class;
        private readonly Team _team;

        public PlayerClassButton(Game game, Vector2 origin, Class cls, Team team) : base(game)
        {
            _class = cls;
            _team = team;

            Button = new Button(game, origin, Teams.SpriteForClass(cls, true), Teams.ColorForTeam(team),
                                Storage.Color("NeutralTeam"));

            Label = new Label(game, origin.X + (Button.Bounds.Width / 2.0f), origin.Y + (Button.Bounds.Height / 2.0f), "", "Calibri24") { DrawOrder = 10000 };

            Children.Add(Button);
            Children.Add(Label);
        }

        public override void Update(GameTime gameTime)
        {
            bool taken = false;

            Label.Text = "";

            base.Update(gameTime);

            foreach(var gamer in Storage.NetworkSession.AllGamers)
            {
                var player = gamer.Tag as Player;

                if(player == null)
                {
                    continue;
                }

                if(player.Team == _team && player.Class == _class)
                {
                    taken = true;
                    Label.Text = gamer.Gamertag;
                    break;
                }
            }

            Button.TeamColor = taken ? Storage.Color("NeutralTeam") : Teams.ColorForTeam(_team);
        }
    }
}