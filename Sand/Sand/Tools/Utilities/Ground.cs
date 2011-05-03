using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Sand.Tools.Utilities
{
    public class Ground : Tool
    {
        public byte DrawGroundLength { get; set; }
        public bool DrawGroundDirection { get; set; }
        public Vector2 DrawGroundLocation;

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
            return "End shock effects on nearby teammates.";
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
            DrawGroundLength = 255;
            DrawGroundDirection = false;
            DrawGroundLocation = (Storage.NetworkSession.LocalGamers[0].Tag as LocalPlayer).Position;

            var groundPlayers = new List<Player>();
            const int groundDistance = 150 * 150;

            foreach(var remoteGamer in Storage.NetworkSession.RemoteGamers)
            {
                var remotePlayer = remoteGamer.Tag as Player;

                if(remotePlayer == null || remotePlayer == Player)
                {
                    continue;
                }

                if(Math.Pow(remotePlayer.X - Player.X, 2) + Math.Pow(remotePlayer.Y - Player.Y, 2) > groundDistance)
                {
                    continue;
                }

                if(remotePlayer.Team == Player.Team)
                {
                    groundPlayers.Add(remotePlayer);
                }
            }

            if(groundPlayers.Count > 0)
            {
                foreach(var groundPlayer in groundPlayers)
                {
                    Messages.SendStunMessage(Player, groundPlayer, -1, Player.Gamer.Id, true);
                }

                Sound.OneShot("Ground");
            }
            else
            {
                Energy += EnergyConsumptionRate;

                Sound.OneShot("Fail", false);
            }

            base.Activate();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if(DrawGroundLength != 0)
            {
                if(DrawGroundLength <= 24)
                {
                    DrawGroundDirection = true;
                }

                var sprite = Storage.Sprite("GroundCircle");
                float grayLevel;
                
                if(DrawGroundLength > 24)
                {
                    grayLevel = (255.0f - DrawGroundLength) / 255.0f;
                }
                else
                {
                    grayLevel = DrawGroundLength / 24.0f;
                }

                spriteBatch.Draw(sprite, new Vector2((int)DrawGroundLocation.X, (int)DrawGroundLocation.Y), null,
                                 new Color(grayLevel, grayLevel, grayLevel, 0.0f), 0.0f,
                                 new Vector2(sprite.Width / 2.0f, sprite.Height / 2.0f),
                                 ((255 - DrawGroundLength) / 255.0f) + 1.0f,
                                 SpriteEffects.None, 0.0f);

                if(DrawGroundDirection)
                {
                    DrawGroundLength = (byte)Math.Max(DrawGroundLength - 1, 0);
                }
                else
                {
                    DrawGroundLength -= 24;
                }
            }
        }

        public override void SendActivationMessage()
        {
            Messages.SendActivateToolMessage(Player, Slot, Type, Active, "DrawGroundLength", DrawGroundLength,
                                             Player.Gamer.Id, true);
        }
    }
}