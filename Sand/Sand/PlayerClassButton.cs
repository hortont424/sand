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
        private readonly Button _classButton;
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
            _classButton = new Button(game, origin, Storage.Sprite(spriteName), Storage.Color(colorName), Storage.Color("NeutralTeam"));

            Children.Add(_classButton);
        }
    }
}
