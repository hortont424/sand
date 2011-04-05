using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Sand.Tools.Weapons
{
    public class Cannon : Tool
    {
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

            var cannonDirection = new Vector3((float)Math.Cos(Player.Angle - (Math.PI / 2.0f)), (float)Math.Sin(Player.Angle - (Math.PI / 2.0f)), 0.0f);
            cannonDirection.Normalize();
            var cannonRay = new Ray(new Vector3(Player.Position, 0.0f), cannonDirection);
            Player.Invisible = false;
            
            foreach(var remoteGamer in Storage.NetworkSession.RemoteGamers)
            {
                var remotePlayer = remoteGamer.Tag as Player;

                if(remotePlayer == null || remotePlayer == Player)
                {
                    continue;
                }

                var intersectionPosition =
                    cannonRay.Intersects(new BoundingBox(
                            new Vector3(remotePlayer.Position.X - 16.0f, remotePlayer.Position.Y - 16.0f, -1.0f),
                            new Vector3(remotePlayer.Position.X + 16.0f,
                                        remotePlayer.Position.Y + 16.0f, 1.0f)));

                if(intersectionPosition != null)
                {
                    // TODO: find closest player intersection, instead of taking the first one we find!
                    //Storage.Sound("BoostDrive_Engine").Play();
                    //Console.WriteLine("whee");
                    Player.Invisible = true;
                }

                //if(remotePlayer.Position)
            }
        }
    }
}