using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Sand.Tools.Mobilities
{
    public class BoostDrive : Tool
    {
        private readonly SoundEffectInstance _startSound, _stopSound, _engineSound;
        private Animation _startFinishedAnimation;
        private AnimationGroup _startFinishedAnimationGroup;
        private AnimationGroup _boostDriveTimerGroup;
        private Animation _boostDriveTimer;
        private ParticleSystem _particles;

        public BoostDrive(Player player) : base(player)
        {
            Modifier = 0.5;

            Energy = TotalEnergy = 200;
            EnergyConsumptionMode = EnergyConsumptionMode.Drain;
            EnergyConsumptionRate = 1;
            EnergyRechargeRate = 0.2;

            _startSound = Storage.Sound("BoostDrive_Start").CreateInstance();
            _stopSound = Storage.Sound("BoostDrive_Stop").CreateInstance();
            _engineSound = Storage.Sound("BoostDrive_Engine").CreateInstance();
            _engineSound.IsLooped = true;

            _boostDriveTimer = new Animation { CompletedDelegate = GenerateBoostDriveParticles };
            _boostDriveTimerGroup = new AnimationGroup(_boostDriveTimer, 10) { Loops = true };
            //Storage.AnimationController.AddGroup(_boostDriveTimerGroup);

            _particles = new ParticleSystem(Player.Game, Player);
            Player.Game.Components.Add(_particles);
        }

        private void GenerateBoostDriveParticles()
        {
            if(!Active)
            {
                return;
            }

            _particles.Emit(10, (p) =>
                                {
                                    var velocity =
                                        new Vector2(-Player.PureAcceleration.X * 0.2f + Storage.Random.Next(-70, 70),
                                                    -Player.PureAcceleration.Y * 0.2f + Storage.Random.Next(-70, 70));

                                    p.LifeRemaining = p.Lifetime = Storage.Random.Next(250, 450);

                                    var angle = (float)Storage.Random.NextDouble() * (Math.PI * 2.0f);
                                    var length = (float)Storage.Random.Next(0, 6);

                                    p.Team = Player.Team;
                                    p.Position = new Vector2((float)(Player.X + (length * Math.Cos(angle))),
                                                             (float)(Player.Y + (length * Math.Sin(angle))));
                                    p.Velocity = velocity;
                                });
        }

        public static string _name()
        {
            return "Boost Drive";
        }

        public static string _description()
        {
            return "Temporarily boost your speed.";
        }

        public static Texture2D _icon()
        {
            return Storage.Sprite("BoostDrive");
        }

        public static Keys _key()
        {
            return Keys.Space;
        }

        public static ToolType _type()
        {
            return ToolType.BoostDrive;
        }

        public static ToolSlot _slot()
        {
            return ToolSlot.Mobility;
        }

        protected override void Activate()
        {
            base.Activate();

            Player.MovementAcceleration = new Vector2(1500.0f, 1500.0f);

            _startSound.Play();

            _startFinishedAnimation = new Animation { CompletedDelegate = CheckStartFinished };

            _startFinishedAnimationGroup = new AnimationGroup(_startFinishedAnimation, 1) { Loops = true };

            Storage.AnimationController.AddGroup(_startFinishedAnimationGroup);
        }

        private void CheckStartFinished()
        {
            if(_startSound.State == SoundState.Stopped)
            {
                Storage.AnimationController.RemoveGroup(_startFinishedAnimationGroup);

                if(Active)
                {
                    _engineSound.Play();
                }
            }
        }

        protected override void Deactivate()
        {
            base.Deactivate();

            Player.MovementAcceleration = Player.DefaultAcceleration;

            _engineSound.Stop();
            _startSound.Stop();

            if(!_inCooldown)
            {
                _stopSound.Play();
            }
        }
    }
}