using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;

namespace Sand.Tools.Weapons
{
    public class Cannon : Tool
    {
        public Cannon(LocalPlayer player) : base(player)
        {
            Name = "Cannon";
            Description = "Shoot Stuff!";
            Icon = Storage.Sprite("Cannon");
            Modifier = 0.5;
            Key = Keys.Space;

            Energy = TotalEnergy = 100;
            EnergyConsumptionMode = EnergyConsumptionMode.Instant;
            EnergyConsumptionRate = 25;
            EnergyRechargeRate = 0.2;
        }

        protected override void Activate()
        {
            base.Activate();
            
            Storage.Sound("Cannon").CreateInstance().Play();
            Messages.SendPlaySoundMessage(Player, "Cannon", Player.Gamer.Id, true);
        }
    }
}
