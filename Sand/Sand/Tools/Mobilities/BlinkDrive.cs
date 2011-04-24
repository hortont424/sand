using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Sand.Tools.Mobilities
{
    public class BlinkDrive : Tool
    {
        public BlinkDrive(Player player) : base(player)
        {
            Modifier = 0.5;

            Energy = TotalEnergy = 100;
            EnergyConsumptionMode = EnergyConsumptionMode.Instant;
            EnergyConsumptionRate = 50;
            EnergyRechargeRate = 0.1;
        }

        public static string _name()
        {
            return "Blink Drive";
        }

        public static string _description()
        {
            return "Teleport!";
        }

        public static Texture2D _icon()
        {
            return Storage.Sprite("BlinkDrive");
        }

        public static Keys _key()
        {
            return Keys.Space;
        }

        public static ToolType _type()
        {
            return ToolType.BlinkDrive;
        }

        public static ToolSlot _slot()
        {
            return ToolSlot.Mobility;
        }

        protected override void Activate()
        {
            base.Activate();

            

            if(!Storage.Game.GameMap.CollisionTest(Player.Texture,
                                                   new Rectangle(
                                                       (int)(Storage.Game.MouseLocation.X - (Player.Width / 2.0)),
                                                       (int)(Storage.Game.MouseLocation.Y - (Player.Height / 2.0)),
                                                       (int)Player.Width, (int)Player.Height)) &&
               (Storage.Game.MouseLocation.X > 0.0f && Storage.Game.MouseLocation.X < Storage.Game.GameMap.Width &&
                Storage.Game.MouseLocation.Y > 0.0f && Storage.Game.MouseLocation.Y < Storage.Game.GameMap.Height))
            {
                Player.X = Storage.Game.MouseLocation.X;
                Player.Y = Storage.Game.MouseLocation.Y;

                Storage.Sound("BlinkDrive").CreateInstance().Play();
            }
            else
            {
                // TODO: make a noise
                Energy += EnergyConsumptionRate;

                Storage.Sound("Fail").CreateInstance().Play();
            }
        }
    }
}