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
            //_engineSound.Play(); // TODO: start this after the start sound finishes
        }

        protected override void Deactivate()
        {
            base.Deactivate();

            Player.MovementAcceleration = Player.DefaultAcceleration;

            //_engineSound.Stop();
            _startSound.Stop();
            _stopSound.Play();
        }
    }
}
