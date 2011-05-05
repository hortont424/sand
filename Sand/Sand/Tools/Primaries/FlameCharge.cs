using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sand.Tools.Primaries
{
    public class FlameCharge : Tool
    {
        public byte DrawFlameRing { get; set; }

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
            return "Destroy a large area of sand all around your tank.";
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
            DrawFlameRing = 255;

            var particleQueue = new HashSet<Particle>();

            Sound.OneShot("FlameCharge");

            const int flameChargeRadius = 75;

            foreach(var pair in Storage.SandParticles.Particles)
            {
                var id = pair.Key;
                var particle = pair.Value;

                var distanceToParticle =
                    Math.Sqrt(Math.Pow(Player.X - particle.Position.X, 2) +
                    Math.Pow(Player.Y - particle.Position.Y, 2));

                if(distanceToParticle > flameChargeRadius)
                {
                    continue;
                }

                var direction = new Vector3(particle.Position - Player.Position, 0.0f);
                direction.Normalize();
                var rayBetween = new Ray(new Vector3(Player.Position, 0.0f),
                                         direction);

                if(Storage.Game.GameMap.Intersects(rayBetween) > distanceToParticle)
                {
                    particleQueue.Add(particle);
                }
            }

            foreach(var particle in particleQueue)
            {
                if(!particle.OnFire)
                {
                    particle.OnFire = true;
                }

                Messages.SendUpdateSandMessage(Player, particle, Player.Gamer.Id, false);
            }

            if(particleQueue.Count > 0)
            {
                Messages.SendOneOffMessage(Player);
            }

            base.Activate();
        }

        public override void SendActivationMessage()
        {
            Messages.SendActivateToolMessage(Player, Slot, Type, Active, "DrawFlameRing", DrawFlameRing,
                                             Player.Gamer.Id, true);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if(DrawFlameRing > 24)
            {
                var sprite = Storage.Sprite("ShieldCircle");
                var grayLevel = DrawFlameRing / 255.0f;

                spriteBatch.Draw(sprite, new Vector2((int)Player.X, (int)Player.Y), null,
                                 new Color(grayLevel, grayLevel, grayLevel), 0.0f,
                                 new Vector2(sprite.Width / 2.0f, sprite.Height / 2.0f),
                                 (((255 - DrawFlameRing) / 255.0f) * 1.56f) + 1.0f,
                                 SpriteEffects.None, 0.0f);

                DrawFlameRing -= 24;
            }
        }
    }
}