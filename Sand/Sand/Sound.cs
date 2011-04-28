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

        public static void OneShot(string soundName, bool broadcast = true)
        {
            var player = Storage.NetworkSession.LocalGamers[0].Tag as Player;

            Add(Storage.Sound(soundName).CreateInstance()).Play();

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