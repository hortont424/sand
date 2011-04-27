using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Audio;

namespace Sand
{
    public static class Sounds
    {
        private static readonly List<SoundEffectInstance> _soundEffectInstances = new List<SoundEffectInstance>(500);
        private static int _updateCount;

        public static SoundEffectInstance Add(SoundEffectInstance soundEffectInstance)
        {
            _soundEffectInstances.Add(soundEffectInstance);

            return soundEffectInstance;
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