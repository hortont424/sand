using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Net;

namespace Sand
{
    public static class Storage
    {
        private static Dictionary<string, Texture2D> _sprites = new Dictionary<string, Texture2D>();
        private static Dictionary<string, SpriteFont> _fonts = new Dictionary<string, SpriteFont>();
        private static Dictionary<string, Color> _colors = new Dictionary<string, Color>();

        public static AnimationController animationController;
        public static NetworkSession networkSession;
        public static PacketReader packetReader = new PacketReader();
        public static PacketWriter packetWriter = new PacketWriter();

        public static void AddSprite(string name, Texture2D texture)
        {
            _sprites.Add(name, texture);
        }

        public static Texture2D Sprite(string name)
        {
            return _sprites[name];
        }

        public static void AddFont(string name, SpriteFont font)
        {
            _fonts.Add(name, font);
        }

        public static SpriteFont Font(string name)
        {
            return _fonts[name];
        }

        public static void AddColor(string name, Color color)
        {
            _colors.Add(name, color);
        }

        public static Color Color(string name)
        {
            return _colors[name];
        }
    }
}