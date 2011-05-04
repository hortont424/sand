using System;
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

            HasMaxDistance = true;
            MaxDistance = 325;
            MaxDistanceColor = Color.Blue;
        }

        public static string _name()
        {
            return "Blink Drive";
        }

        public static string _description()
        {
            return "Teleport your tank to your crosshair.";
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

            int blinkDistance;

            int idealDistance = (int)Math.Sqrt(Math.Pow(Storage.Game.MouseLocation.X - Player.X, 2) +
                                               Math.Pow(Storage.Game.MouseLocation.Y - Player.Y, 2));

            if (idealDistance < MaxDistance)
            {
                blinkDistance = idealDistance;
            }
            else
            {
                blinkDistance = MaxDistance;
            }

            var blinkAngle = Player.Angle - ((float)Math.PI / 2.0f);
            var blinkPosition = new Vector2(Player.X, Player.Y) +
                                (new Vector2((float)Math.Cos(blinkAngle), (float)Math.Sin(blinkAngle)) *
                                 new Vector2((float)blinkDistance));

            if(!Storage.Game.GameMap.CollisionTest(Player.Texture,
                                                   new Rectangle(
                                                       (int)(Storage.Game.MouseLocation.X - (Player.Width / 2.0)),
                                                       (int)(Storage.Game.MouseLocation.Y - (Player.Height / 2.0)),
                                                       (int)Player.Width, (int)Player.Height)) &&
               (Storage.Game.MouseLocation.X > 0.0f && Storage.Game.MouseLocation.X < Storage.Game.GameMap.Width &&
                Storage.Game.MouseLocation.Y > 0.0f && Storage.Game.MouseLocation.Y < Storage.Game.GameMap.Height))
            {
                Player.X = blinkPosition.X;
                Player.Y = blinkPosition.Y;

                Sound.OneShot("BlinkDrive");
            }
            else
            {
                // TODO: make a noise
                Energy += EnergyConsumptionRate;

                Sound.OneShot("Fail", false);
            }
        }
    }
}