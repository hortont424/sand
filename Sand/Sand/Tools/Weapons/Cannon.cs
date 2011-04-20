using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Sand.Tools.Weapons
{
    public class Cannon : Tool
    {
        public float DrawCannonLength { get; set; }

        public Cannon(Player player) : base(player)
        {
            Modifier = 0.5;

            Energy = TotalEnergy = 100;
            EnergyConsumptionMode = EnergyConsumptionMode.Instant;
            EnergyConsumptionRate = 25;
            EnergyRechargeRate = 0.2;
        }

        public static string _name()
        {
            return "Cannon";
        }

        public static string _description()
        {
            return "Shoot Stuff!";
        }

        public static Texture2D _icon()
        {
            return Storage.Sprite("Cannon");
        }

        public static MouseButton _button()
        {
            return MouseButton.RightButton;
        }

        public static ToolType _type()
        {
            return ToolType.Cannon;
        }

        public static ToolSlot _slot()
        {
            return ToolSlot.Weapon;
        }

        protected override void Activate()
        {
            Storage.Sound("Cannon").CreateInstance().Play();
            Messages.SendPlaySoundMessage(Player, "Cannon", Player.Gamer.Id, true);

            var cannonRay = Player.ForwardRay();
            Player closestIntersectionPlayer = null;
            float ? closestIntersectionDistance = null;

            DrawCannonLength = 3000;

            foreach(var remoteGamer in Storage.NetworkSession.RemoteGamers)
            {
                var remotePlayer = remoteGamer.Tag as Player;

                if(remotePlayer == null || remotePlayer == Player)
                {
                    continue;
                }

                var intersectionPosition = remotePlayer.Intersects(cannonRay);

                if(intersectionPosition != null)
                {
                    if(closestIntersectionDistance == null || intersectionPosition < closestIntersectionDistance)
                    {
                        closestIntersectionDistance = intersectionPosition;
                        closestIntersectionPlayer = remotePlayer;
                    }
                }
            }

            if(closestIntersectionDistance != null)
            {
                DrawCannonLength = closestIntersectionDistance.Value;
            }

            var wallIntersection = (Player.Game as Sand).GameMap.Intersects(cannonRay);

            if(wallIntersection != null && (wallIntersection < closestIntersectionDistance || closestIntersectionDistance == null))
            {
                closestIntersectionDistance = null;
                DrawCannonLength = wallIntersection.Value;
            }

            if(closestIntersectionDistance != null)
            {
                Messages.SendStunMessage(Player, closestIntersectionPlayer, (int)EnergyConsumptionRate, Player.Gamer.Id, true);
            }

            base.Activate();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if(DrawCannonLength != 0)
            {
                if(Player.Game.GraphicsDevice.PresentationParameters.MultiSampleCount > 1)
                {
                    spriteBatch.Draw(Storage.Sprite("pixel"),
                                     new Rectangle((int)Player.X, (int)Player.Y, 6, (int)DrawCannonLength), null,
                                     Color.White, Player.Angle, new Vector2(0.5f, 1.0f), SpriteEffects.None, 0.0f);
                }
                else
                {
                    spriteBatch.Draw(Storage.Sprite("pixel"),
                                     new Rectangle((int)Player.X, (int)Player.Y, 7, (int)DrawCannonLength), null,
                                     Color.White, Player.Angle, new Vector2(0.5f, 1.0f), SpriteEffects.None, 0.0f);
                }

                DrawCannonLength = 0;
            }
        }

        public override void SendActivationMessage()
        {
            Messages.SendActivateToolMessage(Player, Slot, Type, Active, "DrawCannonLength", DrawCannonLength, Player.Gamer.Id, true);
        }
    }
}