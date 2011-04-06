using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sand
{
    class PlayerClassButton : Actor
    {
        public readonly Button Button;
        private readonly Class _class;
        private readonly Team _team;

        public PlayerClassButton(Game game, Vector2 origin, Class cls, Team team) : base(game)
        {
            string spriteName, colorName;

            switch(cls) // TODO: I hear you like dictionaries?
            {
                case Class.None:
                    spriteName = "DefenseClassLarge"; // TODO: questionmark class?
                    break;
                case Class.Defense:
                    spriteName = "DefenseClassLarge";
                    break;
                case Class.Offense:
                    spriteName = "OffenseClassLarge";
                    break;
                case Class.Support:
                    spriteName = "SupportClassLarge";
                    break;
                default:
                    throw new ArgumentOutOfRangeException("cls");
            }

            switch(team)
            {
                case Team.None:
                    colorName = "NeutralTeam";
                    break;
                case Team.Red:
                    colorName = "RedTeam";
                    break;
                case Team.Blue:
                    colorName = "BlueTeam";
                    break;
                default:
                    throw new ArgumentOutOfRangeException("team");
            }

            _class = cls;
            _team = team;

            Button = new Button(game, origin, Storage.Sprite(spriteName), Storage.Color(colorName), Storage.Color("NeutralTeam"));

            Children.Add(Button);
        }

        public override void Update(GameTime gameTime)
        {
            bool taken = false;

            base.Update(gameTime);

            foreach (var gamer in Storage.NetworkSession.AllGamers)
            {
                var player = gamer.Tag as Player;

                if (player == null)
                    continue;

                if (player.Team == _team && player.Class == _class)
                {
                    taken = true;
                    break;
                }
            }

            if(taken)
            {
                Button.TeamColor = Storage.Color("NeutralTeam");
            }
            else
            {
                string colorName;

                // TODO: this code is everywhere, fix that.

                switch (_team)
                {
                    case Team.None:
                        colorName = "NeutralTeam";
                        break;
                    case Team.Red:
                        colorName = "RedTeam";
                        break;
                    case Team.Blue:
                        colorName = "BlueTeam";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("team");
                }

                Button.TeamColor = Storage.Color(colorName);
            }
        }
    }
}
