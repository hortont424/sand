using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Sand.Tools.Mobilities
{
    public class WinkDrive : Tool
    {
        private SoundEffectInstance _startSound, _stopSound;

        public WinkDrive(LocalPlayer player) : base(player)
        {
            Modifier = 0.5;

            Energy = TotalEnergy = 100;
            EnergyConsumptionMode = EnergyConsumptionMode.Drain;
            EnergyConsumptionRate = 1;
            EnergyRechargeRate = 0.2;

            _startSound = Storage.Sound("WinkDrive_Start").CreateInstance();
            _stopSound = Storage.Sound("WinkDrive_Stop").CreateInstance();
        }

        public static string _name()
        {
            return "Wink Drive";
        }

        public static string _description()
        {
            return "Disappear!";
        }

        public static Texture2D _icon()
        {
            return Storage.Sprite("WinkDrive");
        }

        public static Keys _key()
        {
            return Keys.Space;
        }

        protected override void Activate()
        {
            base.Activate();

            Player.Invisible = true;

            _startSound.Play();
        }

        protected override void Deactivate()
        {
            base.Deactivate();

            Player.Invisible = false;

            _startSound.Stop();

            if(!_inCooldown)
            {
                _stopSound.Play();
            }
        }
    }
}