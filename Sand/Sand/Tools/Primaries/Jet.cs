using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Sand.Tools.Primaries
{
    public class Jet : Tool
    {
        private Animation _jetTimer;
        private AnimationGroup _jetTimerGroup;

        public Jet(LocalPlayer player) : base(player)
        {
            Modifier = 0.5;

            Energy = TotalEnergy = 100;
            EnergyConsumptionMode = EnergyConsumptionMode.Drain;
            EnergyConsumptionRate = 1;
            EnergyRechargeRate = 0.2;

            _jetTimer = new Animation { CompletedDelegate = JetEmit };
            _jetTimerGroup = new AnimationGroup(_jetTimer, 10) { Loops = true };
        }

        private void JetEmit()
        {
            Storage.SandParticles.Emit(1, (p) =>
            {
                p.LifeRemaining = p.Lifetime = 100;

                var angle = (float)(Storage.Random.NextDouble() * (Math.PI / 8.0)) - (Math.PI / 16.0f);
                var length = (float)Storage.Random.Next(100, 250);

                p.Position = new Vector2(Player.X, Player.Y);
                p.Velocity = (Player as LocalPlayer).Velocity +
                    new Vector2(
                        (float)Math.Cos(Player.Angle - (Math.PI / 2.0f) + angle) * length,
                        (float)Math.Sin(Player.Angle - (Math.PI / 2.0f) + angle) * length);
            });
        }

        protected override void Activate()
        {
            base.Activate();

            Storage.AnimationController.AddGroup(_jetTimerGroup);
        }

        protected override void Deactivate()
        {
            base.Deactivate();

            Storage.AnimationController.RemoveGroup(_jetTimerGroup);
        }

        public static string _name()
        {
            return "Jet";
        }

        public static string _description()
        {
            return "Make Sand!";
        }

        public static Texture2D _icon()
        {
            return Storage.Sprite("Jet");
        }

        public static Keys _key()
        {
            return Keys.Q;
        }
    }
}