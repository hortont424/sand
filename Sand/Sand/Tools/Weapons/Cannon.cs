using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Sand.Tools.Weapons
{
    public class Cannon : Tool
    {
        private bool _drawCannonNextFrame;

        public Cannon(LocalPlayer player) : base(player)
        {
            Name = "Cannon";
            Description = "Shoot Stuff!";
            Icon = Storage.Sprite("Cannon");
            Modifier = 0.5;
            Key = Keys.Space;

            Energy = TotalEnergy = 100;
            EnergyConsumptionMode = EnergyConsumptionMode.Instant;
            EnergyConsumptionRate = 25;
            EnergyRechargeRate = 0.2;
        }

        protected override void Activate()
        {
            base.Activate();

            Storage.Sound("Cannon").CreateInstance().Play();
            Messages.SendPlaySoundMessage(Player, "Cannon", Player.Gamer.Id, true);

            var cannonRay = Player.ForwardRay();
            Player closestIntersectionPlayer = null;
            float ? closestIntersectionDistance = null;

            _drawCannonNextFrame = true;

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

            var wallIntersection = (Player.Game as Sand).GameMap.Intersects(cannonRay);

            if(wallIntersection != null && wallIntersection < closestIntersectionDistance)
            {
                closestIntersectionDistance = null;
            }

            if(closestIntersectionDistance != null)
            {
                Messages.SendStunMessage(Player, closestIntersectionPlayer, (int)EnergyConsumptionRate, Player.Gamer.Id,
                                         true);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if(_drawCannonNextFrame)
            {
                if(Player.Game.GraphicsDevice.PresentationParameters.MultiSampleCount > 1)
                {
                    spriteBatch.Draw(Storage.Sprite("pixel"), new Rectangle((int)Player.X, (int)Player.Y, 3, 3000), null,
                                     Color.White, Player.Angle, new Vector2(0.5f, 1.0f), SpriteEffects.None, 0.0f);
                }
                else
                {
                    spriteBatch.Draw(Storage.Sprite("pixel"), new Rectangle((int)Player.X, (int)Player.Y, 4, 3000), null,
                                     Color.White, Player.Angle, new Vector2(0.5f, 1.0f), SpriteEffects.None, 0.0f);
                }

                _drawCannonNextFrame = false;
            }
        }
    }
}