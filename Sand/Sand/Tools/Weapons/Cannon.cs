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

            var cannonRay = Player.ForwardRay();
            Player.Invisible = false;
            
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
                    // TODO: find closest player intersection, instead of taking the first one we find!
                    // TODO: make sure closest player intersection is closer than closest wall intersection
                    //Storage.Sound("BoostDrive_Engine").Play();
                    //Console.WriteLine("whee");
                    Player.Invisible = true;
                }

                //if(remotePlayer.Position)
            }
        }
    }
}