﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Sand.Tools.Utilities
{
    public class Shield : Tool
    {
        private readonly SoundEffectInstance _shieldSound;
        private Animation _animation;
        private AnimationGroup _animationGroup;

        public float GrayLevel { get; set; }

        public Shield(Player player) : base(player)
        {
            Modifier = 0.5;

            Energy = TotalEnergy = 200;
            EnergyConsumptionMode = EnergyConsumptionMode.Drain;
            EnergyConsumptionRate = 1;
            EnergyRechargeRate = 0.4;

            _shieldSound = Storage.Sound("Shield").CreateInstance();
            _shieldSound.IsLooped = true;

            _animation = new Animation(this, "GrayLevel", 0.5f, 1.0f);
            _animationGroup = new AnimationGroup(_animation, 200) { Loops = true };
            Storage.AnimationController.AddGroup(_animationGroup);
        }

        public static string _name()
        {
            return "Shield";
        }

        public static string _description()
        {
            return "Protect Yourself!";
        }

        public static Texture2D _icon()
        {
            return Storage.Sprite("Shield");
        }

        public static Keys _key()
        {
            return Keys.LeftShift;
        }

        public static ToolType _type()
        {
            return ToolType.Shield;
        }

        public static ToolSlot _slot()
        {
            return ToolSlot.Utility;
        }

        protected override void Activate()
        {
            _shieldSound.Play();
            _shieldSound.Volume = 0.0f;

            Storage.AnimationController.Add(new Animation(_shieldSound, "Volume", 0.0f, 1.0f), 600);

            base.Activate();
        }

        protected override void Deactivate()
        {
            _shieldSound.Stop();

            base.Deactivate();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if(Active)
            {
                var sprite = Storage.Sprite("ShieldCircle");

                spriteBatch.Draw(sprite, new Vector2((int)Player.X, (int)Player.Y), null,
                                 new Color(GrayLevel, GrayLevel, GrayLevel), 0.0f,
                                 new Vector2(sprite.Width / 2.0f, sprite.Height / 2.0f), 1.0f,
                                 SpriteEffects.None, 0.0f);
            }
        }

        public float DeflectShock(float strength)
        {
            if(Active)
            {
                return 0.0f;
            }
            else
            {
                Player.Stunned = true;
                return strength;
            }
        }

        public override void SendActivationMessage()
        {
            Messages.SendActivateToolMessage(Player, Slot, Active, null, 0.0f, Player.Gamer.Id, true);
        }
    }
}