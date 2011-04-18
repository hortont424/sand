using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sand.Tools.Primaries
{
    internal class Plow : Tool
    {
        private readonly Animation _plowTimer;
        private readonly AnimationGroup _plowTimerGroup;
        private readonly Animation _plowUpdateTimer;
        private readonly HashSet<Particle> _particleQueue;
        private readonly AnimationGroup _plowUpdateTimerGroup;

        public Plow(LocalPlayer player) : base(player)
        {
            Modifier = 0.5;

            Energy = TotalEnergy = 150;
            EnergyConsumptionMode = EnergyConsumptionMode.Drain;
            EnergyConsumptionRate = 1;
            EnergyRechargeRate = 0.2;

            _plowTimer = new Animation { CompletedDelegate = PlowBlow };
            _plowTimerGroup = new AnimationGroup(_plowTimer, 10) { Loops = true };

            _particleQueue = new HashSet<Particle>();
            _plowUpdateTimer = new Animation { CompletedDelegate = PlowUpdate };
            _plowUpdateTimerGroup = new AnimationGroup(_plowUpdateTimer, 50) { Loops = true };
            Storage.AnimationController.AddGroup(_plowUpdateTimerGroup);
        }

        public static string _name()
        {
            return "Plow";
        }

        public static string _description()
        {
            return "Move Sand!";
        }

        public static Texture2D _icon()
        {
            return Storage.Sprite("Plow");
        }

        public static MouseButton _button()
        {
            return MouseButton.LeftButton;
        }

        private void PlowBlow()
        {
            foreach(var pair in Storage.SandParticles.Particles)
            {
                var id = pair.Key;
                var particle = pair.Value;

                var angleToParticle = (float)Math.Atan2(particle.Position.Y - Player.Y, particle.Position.X - Player.X);
                var playerAngle = Player.Angle - ((float)Math.PI / 2.0f);
                var distanceToParticle =
                    Math.Sqrt(Math.Pow(Player.X - particle.Position.X, 2) + Math.Pow(Player.Y - particle.Position.Y, 2));
                const int maxDistance = 100;

                if(Math.Abs(angleToParticle - playerAngle) > (Math.PI / 6))
                {
                    continue;
                }

                if(distanceToParticle > maxDistance)
                {
                    continue;
                }

                particle.Velocity +=
                    new Vector2((float)(100.0f * Math.Cos(playerAngle)), (float)(100.0f * Math.Sin(playerAngle))) *
                    new Vector2((float)((maxDistance - distanceToParticle) / maxDistance));

                _particleQueue.Add(particle);
            }
        }

        private void PlowUpdate()
        {
            foreach(var particle in _particleQueue)
            {
                Messages.SendCreateSandMessage(Player, particle, Player.Gamer.Id, false);
            }

            if(_particleQueue.Count > 0)
            {
                Messages.SendOneOffMessage(Player);
            }

            _particleQueue.Clear();
        }

        protected override void Activate()
        {
            base.Activate();

            Storage.AnimationController.AddGroup(_plowTimerGroup);
        }

        protected override void Deactivate()
        {
            base.Deactivate();

            Storage.AnimationController.RemoveGroup(_plowTimerGroup);
        }
    }
}