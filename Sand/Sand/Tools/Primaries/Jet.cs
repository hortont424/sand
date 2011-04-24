using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace Sand.Tools.Primaries
{
    public class Jet : Tool
    {
        private readonly Animation _jetTimer;
        private readonly AnimationGroup _jetTimerGroup;
        private readonly SoundEffectInstance _jetSound;
        private Animation _jetSoundAnimation;

        public Jet(Player player) : base(player)
        {
            Modifier = 0.5;

            Energy = TotalEnergy = 100;
            EnergyConsumptionMode = EnergyConsumptionMode.Drain;
            EnergyConsumptionRate = 1;
            EnergyRechargeRate = 0.2;

            _jetTimer = new Animation { CompletedDelegate = JetEmit };
            _jetTimerGroup = new AnimationGroup(_jetTimer, 10) { Loops = true };

            _jetSound = Storage.Sound("Jet").CreateInstance();
            _jetSound.IsLooped = true;
            _jetSound.Volume = 0.0f;
        }

        public static string _name()
        {
            return "Jet";
        }

        public static string _description()
        {
            return "Make Sand!";
        }

        public static Texture2D _icon()
        {
            return Storage.Sprite("Jet");
        }

        public static MouseButton _button()
        {
            return MouseButton.LeftButton;
        }

        public static ToolType _type()
        {
            return ToolType.Jet;
        }

        public static ToolSlot _slot()
        {
            return ToolSlot.Primary;
        }

        private void JetEmit()
        {
            Storage.SandParticles.Emit(1, (p) =>
                                          {
                                              p.LifeRemaining = p.Lifetime = 100;

                                              var angle = (float)(Storage.Random.NextDouble() * (Math.PI / 8.0)) -
                                                          (Math.PI / 16.0f);
                                              var length = (float)Storage.Random.Next(200, 450);

                                              p.Team = Player.Team;
                                              p.Position = new Vector2(Player.X, Player.Y);
                                              p.Velocity = (Player).Velocity +
                                                           new Vector2(
                                                               (float)Math.Cos(Player.Angle - (Math.PI / 2.0f) + angle) *
                                                               length,
                                                               (float)Math.Sin(Player.Angle - (Math.PI / 2.0f) + angle) *
                                                               length);
                                          });
        }

        protected override void Activate()
        {
            Storage.AnimationController.AddGroup(_jetTimerGroup);

            _jetSound.Play();

            if (_jetSoundAnimation != null)
            {
                Storage.AnimationController.Remove(_jetSoundAnimation);
            }

            _jetSoundAnimation = new Animation(_jetSound, "Volume", 1.0f) { CompletedDelegate = FinishStartingSound };
            Storage.AnimationController.Add(_jetSoundAnimation, 300);
        }

        protected override void Deactivate()
        {
            Storage.AnimationController.RemoveGroup(_jetTimerGroup);

            if (_jetSoundAnimation != null)
            {
                Storage.AnimationController.Remove(_jetSoundAnimation);
            }

            _jetSoundAnimation = new Animation(_jetSound, "Volume", 0.0f) { CompletedDelegate = FinishStoppingSound };
            Storage.AnimationController.Add(_jetSoundAnimation, 300);

            base.Deactivate();
        }

        private void FinishStartingSound()
        {
            _jetSoundAnimation = null;
        }

        private void FinishStoppingSound()
        {
            _jetSound.Stop();
            _jetSoundAnimation = null;
        }

    }
}