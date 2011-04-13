using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Net;

namespace Sand
{
    public static class Storage
    {
        private static readonly Dictionary<string, Texture2D> _sprites = new Dictionary<string, Texture2D>();
        private static readonly Dictionary<string, SoundEffect> _sounds = new Dictionary<string, SoundEffect>();
        private static readonly Dictionary<string, SpriteFont> _fonts = new Dictionary<string, SpriteFont>();
        private static readonly Dictionary<string, Color> _colors = new Dictionary<string, Color>();

        public static AnimationController AnimationController;
        public static NetworkSession NetworkSession;
        public static PacketReader PacketReader = new PacketReader();
        public static PacketWriter PacketWriter = new PacketWriter();

        public static ParticleSystem SandParticles;
        public static Random Random = new Random();

        public static bool AcceptInput = true;
        public static GameTime CurrentTime;

        public static void AddSprite(string name, Texture2D texture)
        {
            _sprites.Add(name, texture);
        }

        public static Texture2D Sprite(string name)
        {
            return _sprites[name];
        }

        public static void AddSound(string name, SoundEffect sound)
        {
            _sounds.Add(name, sound);
        }

        public static SoundEffect Sound(string name)
        {
            return _sounds[name];
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