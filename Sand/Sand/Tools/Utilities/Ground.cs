using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Sand.Tools.Utilities
{
    public class Ground : Tool
    {
        private float DrawGroundLength;

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
            var cannonRay = Player.ForwardRay();
            Player closestIntersectionPlayer = null;
            float? closestIntersectionDistance = null;

            DrawGroundLength = 3000;

            foreach (var remoteGamer in Storage.NetworkSession.RemoteGamers)
            {
                var remotePlayer = remoteGamer.Tag as Player;

                if (remotePlayer == null || remotePlayer == Player)
                {
                    continue;
                }

                var intersectionPosition = remotePlayer.Intersects(cannonRay);

                if (intersectionPosition != null)
                {
                    if (closestIntersectionDistance == null || intersectionPosition < closestIntersectionDistance)
                    {
                        closestIntersectionDistance = intersectionPosition;
                        closestIntersectionPlayer = remotePlayer;
                    }
                }
            }

            if (closestIntersectionDistance != null)
            {
                DrawGroundLength = closestIntersectionDistance.Value;
            }

            var wallIntersection = (Player.Game as Sand).GameMap.Intersects(cannonRay);

            if (wallIntersection != null && (wallIntersection < closestIntersectionDistance || closestIntersectionDistance == null))
            {
                closestIntersectionDistance = null;
                DrawGroundLength = wallIntersection.Value;
            }

            if (closestIntersectionDistance != null)
            {
                Messages.SendStunMessage(Player, closestIntersectionPlayer, -1, Player.Gamer.Id, true);

                Storage.Sound("Ground").CreateInstance().Play();
                Messages.SendPlaySoundMessage(Player, "Ground", Player.Gamer.Id, true);
            }
            else
            {
                Energy += EnergyConsumptionRate;

                Storage.Sound("Fail").CreateInstance().Play();
                Messages.SendPlaySoundMessage(Player, "Fail", Player.Gamer.Id, true);
            }

            base.Activate();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (DrawGroundLength != 0)
            {
                if (Player.Game.GraphicsDevice.PresentationParameters.MultiSampleCount > 1)
                {
                    spriteBatch.Draw(Storage.Sprite("pixel"),
                                     new Rectangle((int)Player.X, (int)Player.Y, 6, (int)DrawGroundLength), null,
                                     Color.White, Player.Angle, new Vector2(0.5f, 1.0f), SpriteEffects.None, 0.0f);
                }
                else
                {
                    spriteBatch.Draw(Storage.Sprite("pixel"),
                                     new Rectangle((int)Player.X, (int)Player.Y, 7, (int)DrawGroundLength), null,
                                     Color.White, Player.Angle, new Vector2(0.5f, 1.0f), SpriteEffects.None, 0.0f);
                }

                DrawGroundLength = 0;
            }
        }

        public override void SendActivationMessage()
        {
            Messages.SendActivateToolMessage(Player, Slot, Type, Active, "DrawGroundLength", DrawGroundLength, Player.Gamer.Id, true);
        }
    }
}