using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Net;

namespace Sand
{
    public static class Storage
    {
        private static Dictionary<string, Texture2D> _sprites = new Dictionary<string, Texture2D>();
        private static Dictionary<string, SpriteFont> _fonts = new Dictionary<string, SpriteFont>();

        public static NetworkSession networkSession;

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
    }
}