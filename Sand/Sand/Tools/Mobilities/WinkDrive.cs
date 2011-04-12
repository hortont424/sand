using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Sand.Tools.Mobilities
{
    public class WinkDrive : Tool
    {
        public WinkDrive(LocalPlayer player) : base(player)
        {
            Modifier = 0.5;

            Energy = TotalEnergy = 100;
            EnergyConsumptionMode = EnergyConsumptionMode.Drain;
            EnergyConsumptionRate = 1;
            EnergyRechargeRate = 0.2;
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
        }

        protected override void Deactivate()
        {
            base.Deactivate();

            Player.Invisible = false;
        }
    }
}
