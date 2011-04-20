using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Sand.Tools.Weapons
{
    public class EMP : Tool
    {
        public EMP(Player player) : base(player)
        {
            Modifier = 0.5;

            Energy = TotalEnergy = 100;
            EnergyConsumptionMode = EnergyConsumptionMode.Instant;
            EnergyConsumptionRate = 100;
            EnergyRechargeRate = 0.2;
        }

        public static string _name()
        {
            return "EMP";
        }

        public static string _description()
        {
            return "Shoot Lots of Stuff!";
        }

        public static Texture2D _icon()
        {
            return Storage.Sprite("EMP");
        }

        public static MouseButton _button()
        {
            return MouseButton.RightButton;
        }

        public static ToolType _type()
        {
            return ToolType.EMP;
        }

        public static ToolSlot _slot()
        {
            return ToolSlot.Weapon;
        }

        protected override void Activate()
        {
            base.Activate();

            Storage.Sound("EMP").CreateInstance().Play();
            Messages.SendPlaySoundMessage(Player, "EMP", Player.Gamer.Id, true);

            var shockPlayers = new List<Player>();
            const int empDistance = 150 * 150;

            foreach(var remoteGamer in Storage.NetworkSession.RemoteGamers)
            {
                var remotePlayer = remoteGamer.Tag as Player;

                if(remotePlayer == null || remotePlayer == Player)
                {
                    continue;
                }

                if(Math.Pow(remotePlayer.X - Player.X, 2) + Math.Pow(remotePlayer.Y - Player.Y, 2) > empDistance)
                {
                    continue;
                }

                shockPlayers.Add(remotePlayer);
            }

            foreach(Player player in shockPlayers)
            {
                Messages.SendStunMessage(Player, player, 25, Player.Gamer.Id, true);
            }
        }
    }
}