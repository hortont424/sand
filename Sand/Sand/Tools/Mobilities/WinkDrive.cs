using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;

namespace Sand.Tools.Mobilities
{
    public class WinkDrive : Tool
    {
        public WinkDrive(LocalPlayer player) : base(player)
        {
            Name = "Wink Drive";
            Description = "Disappear!";
            Icon = Storage.Sprite("BoostDrive");
            Modifier = 0.5;
            Key = Keys.Space;

            Energy = TotalEnergy = 100;
            EnergyConsumptionMode = EnergyConsumptionMode.Drain;
            EnergyConsumptionRate = 1;
            EnergyRechargeRate = 0.2;
        }

        protected override void Activate()
        {
            base.Activate();

            Player.Invisible = true;
        }

        protected override void Deactivate()
        {
            base.Deactivate();

            Player.Invisible = false;
        }
    }
}
