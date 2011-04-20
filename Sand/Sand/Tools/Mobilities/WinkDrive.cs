using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Sand.Tools.Mobilities
{
    public class WinkDrive : Tool
    {
        private SoundEffectInstance _startSound, _stopSound;
        private Animation _animation;
        private AnimationGroup _animationGroup;

        public WinkDrive(Player player) : base(player)
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

        public static ToolType _type()
        {
            return ToolType.WinkDrive;
        }

        public static ToolSlot _slot()
        {
            return ToolSlot.Mobility;
        }

        protected override void Activate()
        {
            base.Activate();

            _animation = new Animation(Player, "Invisible", 1.0f);
            _animationGroup = new AnimationGroup(_animation, 110.0f);
            Storage.AnimationController.AddGroup(_animationGroup);

            _startSound.Play();
        }

        protected override void Deactivate()
        {
            base.Deactivate();

            Storage.AnimationController.RemoveGroup(_animationGroup);

            _animation = new Animation(Player, "Invisible", 0.0f);
            _animationGroup = new AnimationGroup(_animation, 110.0f);
            Storage.AnimationController.AddGroup(_animationGroup);

            _startSound.Stop();

            if(!_inCooldown)
            {
                _stopSound.Play();
            }
        }
    }
}