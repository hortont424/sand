using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Sand.Tools.Primaries
{
    public class Jet : Tool
    {
        public Jet(LocalPlayer player) : base(player)
        {
            Modifier = 0.5;

            Energy = TotalEnergy = 100;
            EnergyConsumptionMode = EnergyConsumptionMode.Instant;
            EnergyConsumptionRate = 25;
            EnergyRechargeRate = 0.2;
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