using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sand.Tools.Primaries
{
    public class FlameCharge : Tool
    {
        public FlameCharge(Player player) : base(player)
        {
            Modifier = 0.5;

            Energy = TotalEnergy = 100;
            EnergyConsumptionMode = EnergyConsumptionMode.Instant;
            EnergyConsumptionRate = 50;
            EnergyRechargeRate = 0.2;
        }

        public static string _name()
        {
            return "Flame Charge";
        }

        public static string _description()
        {
            return "Light Sand!";
        }

        public static Texture2D _icon()
        {
            return Storage.Sprite("FlameCharge");
        }

        public static MouseButton _button()
        {
            return MouseButton.LeftButton;
        }

        public static ToolType _type()
        {
            return ToolType.FlameCharge;
        }

        public static ToolSlot _slot()
        {
            return ToolSlot.Primary;
        }

        protected override void Activate()
        {
            var particleQueue = new HashSet<Particle>();
            base.Activate();

            const int flameChargeRadius = 50 * 50;

            foreach (var pair in Storage.SandParticles.Particles)
            {
                var id = pair.Key;
                var particle = pair.Value;

                var distanceToParticle =
                    Math.Pow(Player.X - particle.Position.X, 2) +
                    Math.Pow(Player.Y - particle.Position.Y, 2);

                if (distanceToParticle > flameChargeRadius)
                {
                    continue;
                }

                if(!particle.OnFire)
                {
                    particle.Fire = 255;
                }

                particleQueue.Add(particle);
            }

            foreach (var particle in particleQueue)
            {
                Messages.SendCreateSandMessage(Player, particle, Player.Gamer.Id, false);
            }

            if (particleQueue.Count > 0)
            {
                Messages.SendOneOffMessage(Player);
            }
        }

        protected override void Deactivate()
        {
            base.Deactivate();
        }
    }
}