using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;

namespace Sand
{
    public static class Sound
    {
        private static readonly List<SoundEffectInstance> _soundEffectInstances = new List<SoundEffectInstance>(500);
        private static int _updateCount;

        public static SoundEffectInstance Add(SoundEffectInstance soundEffectInstance)
        {
            _soundEffectInstances.Add(soundEffectInstance);

            return soundEffectInstance;
        }

        public static void OneShot(string soundName, bool broadcast = true, Player remotePlayer = null)
        {
            var player = Storage.NetworkSession.LocalGamers[0].Tag as Player;
            float distance = 0.0f;

            if (remotePlayer != null)
            {
                distance = (float)Math.Sqrt(Math.Pow(remotePlayer.X - player.X, 2) + Math.Pow(remotePlayer.Y - player.Y, 2));
            }

            var sound = Storage.Sound(soundName).CreateInstance();
            sound.Volume = Math.Max(0.0f, (1700.0f - distance) / 1700.0f);
            Add(sound).Play();

            if (player == null || !broadcast)
                return;

            Messages.SendPlaySoundMessage(player, soundName, player.Gamer.Id, true);
        }

        public static void Update()
        {
            if(_updateCount == 1024)
            {
                _soundEffectInstances.RemoveAll((a) => a.State != SoundState.Playing);
                _updateCount = 0;
            }

            _updateCount++;
        }
    }
}