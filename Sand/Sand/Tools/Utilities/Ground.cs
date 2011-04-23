using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Sand.Tools.Utilities
{
    public class Ground : Tool
    {
        public Ground(Player player) : base(player)
        {
            Modifier = 0.5;

            Energy = TotalEnergy = 100;
            EnergyConsumptionMode = EnergyConsumptionMode.Instant;
            EnergyConsumptionRate = 25;
            EnergyRechargeRate = 0.2;

        }

        public static string _name()
        {
            return "Ground";
        }

        public static string _description()
        {
            return "Heal Teammates!";
        }

        public static Texture2D _icon()
        {
            return Storage.Sprite("Ground");
        }

        public static Keys _key()
        {
            return Keys.LeftShift;
        }

        public static ToolType _type()
        {
            return ToolType.Ground;
        }

        public static ToolSlot _slot()
        {
            return ToolSlot.Utility;
        }

        protected override void Activate()
        {
            base.Activate();
        }

        protected override void Deactivate()
        {
            base.Deactivate();
        }
    }
}