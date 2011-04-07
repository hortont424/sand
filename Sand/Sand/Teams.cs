using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sand
{
    public enum Team
    {
        None,
        Red,
        Blue
    } ;

    public enum Class
    {
        None,
        Defense,
        Offense,
        Support
    } ;

    public class Teams
    {
        public static Color ColorForTeam(Team team)
        {
            string colorName;

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

            return Storage.Color(colorName);
        }

        public static Texture2D SpriteForClass(Class cls, bool large)
        {
            string spriteName;

            switch(cls) // TODO: I hear you like dictionaries?
            {
                case Class.None:
                    spriteName = "DefenseClass"; // TODO: questionmark class?
                    break;
                case Class.Defense:
                    spriteName = "DefenseClass";
                    break;
                case Class.Offense:
                    spriteName = "OffenseClass";
                    break;
                case Class.Support:
                    spriteName = "SupportClass";
                    break;
                default:
                    throw new ArgumentOutOfRangeException("cls");
            }

            return Storage.Sprite(large ? spriteName + "Large" : spriteName);
        }

        public static Texture2D SpriteForClass(Class cls)
        {
            return SpriteForClass(cls, false);
        }
    }
}