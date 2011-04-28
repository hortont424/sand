using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sand.Tools.Weapons
{
    public class EMP : Tool
    {
        public byte DrawEMPRing { get; set; }

        public EMP(Player player) : base(player)
        {
            Modifier = 0.5;

            Energy = TotalEnergy = 100;
            EnergyConsumptionMode = EnergyConsumptionMode.Instant;
            EnergyConsumptionRate = 50;
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
            DrawEMPRing = 255;

            Sound.OneShot("EMP");

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

            base.Activate();
        }

        public override void SendActivationMessage()
        {
            Messages.SendActivateToolMessage(Player, Slot, Type, Active, "DrawEMPRing", DrawEMPRing,
                                             Player.Gamer.Id, true);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if(DrawEMPRing > 24)
            {
                var sprite = Storage.Sprite("WhiteCircle");
                var grayLevel = DrawEMPRing / 255.0f;

                spriteBatch.Draw(sprite, new Vector2((int)Player.X, (int)Player.Y), null,
                                 new Color(grayLevel, grayLevel, grayLevel), 0.0f,
                                 new Vector2(sprite.Width / 2.0f, sprite.Height / 2.0f),
                                 (((255 - DrawEMPRing) / 255.0f) * 3.125f) + 1.0f,
                                 SpriteEffects.None, 0.0f);

                DrawEMPRing -= 24;
            }
        }
    }
}