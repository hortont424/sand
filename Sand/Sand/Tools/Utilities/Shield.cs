using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Sand.Tools.Utilities
{
    public class Shield : Tool
    {
        private SoundEffectInstance _shieldSound;

        public Shield(LocalPlayer player)
            : base(player)
        {
            Name = "Shield";
            Description = "Protect Yourself!";
            Icon = Storage.Sprite("Shield");
            Modifier = 0.5;
            Key = Keys.LeftShift;

            Energy = TotalEnergy = 200;
            EnergyConsumptionMode = EnergyConsumptionMode.Drain;
            EnergyConsumptionRate = 1;
            EnergyRechargeRate = 0.4;

            _shieldSound = Storage.Sound("Shield").CreateInstance();
            _shieldSound.IsLooped = true;
        }

        protected override void Activate()
        {
            _shieldSound.Play();
        }

        protected override void Deactivate()
        {
            _shieldSound.Stop();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if(Active)
            {
                var sprite = Storage.Sprite("ShieldCircle");

                spriteBatch.Draw(sprite, new Vector2((int)Player.X, (int)Player.Y), null,
                                 Color.White, 0.0f, new Vector2(sprite.Width / 2.0f, sprite.Height / 2.0f), 1.0f,
                                 SpriteEffects.None, 0.0f);
            }
        }

        public float DeflectShock(float strength)
        {
            if(Active)
            {
                //Player.Stunned = false;
                return 0.0f;
            }
            else
            {
                Player.Stunned = true;
                return strength;
            }
        }
    }
}