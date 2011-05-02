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
    }

    public enum Class
    {
        None,
        Defense,
        Offense,
        Support
    }

    public enum ClassVariant
    {
        Normal,
        Large,
        Filled
    }

    public class Teams
    {
        private static readonly Dictionary<Team, String> TeamColorNames = new Dictionary<Team, String>()
            {
                  { Team.None, "NeutralTeam" },
                  { Team.Red, "RedTeam" },
                  { Team.Blue, "BlueTeam" }
            };

        private static readonly Dictionary<Team, String> TeamNames = new Dictionary<Team, String>()
            {
                  { Team.None, "Nobody" },
                  { Team.Red, "Purple" },
                  { Team.Blue, "Green" }
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

        public static string NameForTeam(Team team)
        {
            return TeamNames[team];
        }

        public static Texture2D SpriteForClass(Class cls, ClassVariant variant)
        {
            string spriteName = ClassSpriteNames[cls];

            switch(variant)
            {
                case ClassVariant.Normal:
                    break;
                case ClassVariant.Large:
                    spriteName += "Large";
                    break;
                case ClassVariant.Filled:
                    spriteName += "Filled";
                    break;
                default:
                    throw new ArgumentOutOfRangeException("variant");
            }

            return Storage.Sprite(spriteName);
        }

        public static Texture2D SpriteForClass(Class cls)
        {
            return SpriteForClass(cls, ClassVariant.Normal);
        }
    }
}