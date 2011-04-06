using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;

namespace Sand.Tools.Mobilities
{
    public class BoostDrive : Tool
    {
        private SoundEffectInstance _startSound, _stopSound, _engineSound;
        private Animation _startFinishedAnimation;
        private AnimationGroup _startFinishedAnimationGroup;

        public BoostDrive(LocalPlayer player) : base(player)
        {
            Name = "Boost Drive";
            Description = "Go faster!";
            Icon = Storage.Sprite("BoostDrive");
            Modifier = 0.5;
            Key = Keys.Space;

            Energy = TotalEnergy = 100;
            EnergyConsumptionMode = EnergyConsumptionMode.Drain;
            EnergyConsumptionRate = 1;
            EnergyRechargeRate = 0.2;

            _startSound = Storage.Sound("BoostDrive_Start").CreateInstance();
            _stopSound = Storage.Sound("BoostDrive_Stop").CreateInstance();
            _engineSound = Storage.Sound("BoostDrive_Engine").CreateInstance();
            _engineSound.IsLooped = true;
        }

        protected override void Activate()
        {
            base.Activate();

            Player.MovementAcceleration = new Vector2(1200.0f, 1200.0f);

            _startSound.Play();
            
            _startFinishedAnimation = new Animation { CompletedDelegate = CheckStartFinished };

            _startFinishedAnimationGroup = new AnimationGroup(_startFinishedAnimation, 1) { Loops = true };

            Storage.AnimationController.AddGroup(_startFinishedAnimationGroup);
        }

        private void CheckStartFinished()
        {
            if(_startSound.State == SoundState.Stopped)
            {
                Storage.AnimationController.RemoveGroup(_startFinishedAnimationGroup);

                if(Active)
                {
                    _engineSound.Play();
                }
                
            }
        }

        protected override void Deactivate()
        {
            base.Deactivate();

            Player.MovementAcceleration = Player.DefaultAcceleration;

            _engineSound.Stop();
            _startSound.Stop();
            _stopSound.Play();
        }
    }
}
