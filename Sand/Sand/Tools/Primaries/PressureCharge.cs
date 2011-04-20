using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sand.Tools.Primaries
{
    public class PressureCharge : Tool
    {
        public PressureCharge(Player player) : base(player)
        {
            Modifier = 0.5;

            Energy = TotalEnergy = 100;
            EnergyConsumptionMode = EnergyConsumptionMode.Instant;
            EnergyConsumptionRate = 50;
            EnergyRechargeRate = 0.2;
        }

        public static string _name()
        {
            return "Pressure Charge";
        }

        public static string _description()
        {
            return "Push Sand!";
        }

        public static Texture2D _icon()
        {
            return Storage.Sprite("PressureCharge");
        }

        public static MouseButton _button()
        {
            return MouseButton.LeftButton;
        }

        public static ToolType _type()
        {
            return ToolType.PressureCharge;
        }

        public static ToolSlot _slot()
        {
            return ToolSlot.Primary;
        }

        protected override void Activate()
        {
            base.Activate();

            var particleQueue = new HashSet<Particle>();
            const int maxDistance = 75 * 75;

            foreach(var pair in Storage.SandParticles.Particles)
            {
                var id = pair.Key;
                var particle = pair.Value;

                var angleToParticle = (float)Math.Atan2(particle.Position.Y - Player.Y, particle.Position.X - Player.X);
                var distanceToParticle =
                    Math.Pow(Player.X - particle.Position.X, 2) + Math.Pow(Player.Y - particle.Position.Y, 2);
                var velocity = Storage.Random.Next(0, 450);

                if(distanceToParticle > maxDistance)
                {
                    continue;
                }

                particle.Velocity +=
                    new Vector2((float)(velocity * Math.Cos(angleToParticle)), (float)(velocity * Math.Sin(angleToParticle))) *
                    new Vector2((float)((maxDistance - distanceToParticle) / maxDistance));

                particleQueue.Add(particle);
            }

            foreach(var particle in particleQueue)
            {
                Messages.SendUpdateSandMessage(Player, particle, Player.Gamer.Id, false);
            }

            if(particleQueue.Count > 0)
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