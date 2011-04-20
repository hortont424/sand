using System;
using System.Collections.Generic;
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
        private static readonly Dictionary<Team, String> TeamColorNames = new Dictionary<Team, String>()
            {
                  { Team.None, "NeutralTeam" },
                  { Team.Red, "RedTeam" },
                  { Team.Blue, "BlueTeam" }
            };

        private static readonly Dictionary<Class, String> ClassSpriteNames = new Dictionary<Class, String>()
            {
                  { Class.None, "DefenseClass" },
                  { Class.Defense, "DefenseClass" },
                  { Class.Offense, "OffenseClass" },
                  { Class.Support, "SupportClass"}
            };

        public static Color ColorForTeam(Team team)
        {
            return Storage.Color(TeamColorNames[team]);
        }

        public static Texture2D SpriteForClass(Class cls, bool large)
        {
            string spriteName = ClassSpriteNames[cls];

            return Storage.Sprite(large ? spriteName + "Large" : spriteName);
        }

        public static Texture2D SpriteForClass(Class cls)
        {
            return SpriteForClass(cls, false);
        }
    }
}