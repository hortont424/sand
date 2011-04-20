using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sand.Tools.Primaries
{
    public class SandCharge : Tool
    {
        public SandCharge(Player player) : base(player)
        {
            Modifier = 0.5;

            Energy = TotalEnergy = 100;
            EnergyConsumptionMode = EnergyConsumptionMode.Instant;
            EnergyConsumptionRate = 50;
            EnergyRechargeRate = 0.2;
        }

        public static string _name()
        {
            return "Sand Charge";
        }

        public static string _description()
        {
            return "Make Sand!";
        }

        public static Texture2D _icon()
        {
            return Storage.Sprite("SandCharge");
        }

        public static MouseButton _button()
        {
            return MouseButton.LeftButton;
        }

        public static ToolType _type()
        {
            return ToolType.SandCharge;
        }

        public static ToolSlot _slot()
        {
            return ToolSlot.Primary;
        }

        protected override void Activate()
        {
            base.Activate();

            Storage.SandParticles.Emit(100, (p) =>
                                            {
                                                p.LifeRemaining = p.Lifetime = 100;

                                                var angle = (float)(Storage.Random.NextDouble() * 2 * Math.PI);
                                                var length = (float)Storage.Random.Next(50, 200);

                                                p.Team = Player.Team;
                                                p.Position = new Vector2(Player.X, Player.Y);
                                                p.Velocity = (Player).Velocity +
                                                             new Vector2(
                                                                 (float)Math.Cos(angle) * length,
                                                                 (float)Math.Sin(angle) * length);
                                            });
        }

        protected override void Deactivate()
        {
            base.Deactivate();
        }
    }
}