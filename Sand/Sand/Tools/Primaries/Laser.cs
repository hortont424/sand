﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace Sand.Tools.Primaries
{
    internal class Laser : Tool
    {
        private readonly Animation _laserTimer;
        private readonly AnimationGroup _laserTimerGroup;
        private readonly HashSet<Particle> _particleQueue;
        private readonly Animation _laserUpdateTimer;
        private readonly AnimationGroup _laserUpdateTimerGroup;
        private readonly SoundEffectInstance _laserSound;
        public int DrawLaserLength { get; set; }
        private Vector2 _laserPosition;
        private readonly ParticleSystem _laserParticles;

        public Laser(Player player) : base(player)
        {
            Modifier = 0.5;

            Energy = TotalEnergy = 150;
            EnergyConsumptionMode = EnergyConsumptionMode.Drain;
            EnergyConsumptionRate = 1;
            EnergyRechargeRate = 0.2;

            _laserTimer = new Animation { CompletedDelegate = LaserZap };
            _laserTimerGroup = new AnimationGroup(_laserTimer, 10) { Loops = true };

            _particleQueue = new HashSet<Particle>();
            _laserUpdateTimer = new Animation { CompletedDelegate = LaserUpdate };
            _laserUpdateTimerGroup = new AnimationGroup(_laserUpdateTimer, 50) { Loops = true };
            Storage.AnimationController.AddGroup(_laserUpdateTimerGroup);

            _laserSound = Storage.Sound("Laser").CreateInstance();
            _laserSound.IsLooped = true;

            _laserParticles = new ParticleSystem(Player.Game, Player);
            Player.Game.Components.Add(_laserParticles);
        }

        public static string _name()
        {
            return "Laser";
        }

        public static string _description()
        {
            return "Light Sand on Fire!";
        }

        public static Texture2D _icon()
        {
            return Storage.Sprite("Laser");
        }

        public static MouseButton _button()
        {
            return MouseButton.LeftButton;
        }

        public static ToolType _type()
        {
            return ToolType.Laser;
        }

        public static ToolSlot _slot()
        {
            return ToolSlot.Primary;
        }

        private void LaserZap()
        {
            var sandGame = Player.Game as Sand;

            if(sandGame == null)
            {
                return;
            }

            var wallIntersection = (Player.Game as Sand).GameMap.Intersects(Player.ForwardRay());
            const int maxLaserDistance = 300;

            int ? laserDistance = null;

            int idealDistance = (int)Math.Sqrt(Math.Pow(sandGame.MouseLocation.X - Player.X, 2) +
                                               Math.Pow(sandGame.MouseLocation.Y - Player.Y, 2));

            if(idealDistance < maxLaserDistance)
            {
                laserDistance = idealDistance;
            }
            else
            {
                laserDistance = maxLaserDistance;
            }

            if(wallIntersection != null && wallIntersection < laserDistance)
            {
                laserDistance = (int)wallIntersection.Value;
            }

            DrawLaserLength = (int)laserDistance;

            const int laserRadius = 20 * 20;

            var laserAngle = Player.Angle - ((float)Math.PI / 2.0f);
            var laserPosition = new Vector2(Player.X, Player.Y) +
                                (new Vector2((float)Math.Cos(laserAngle), (float)Math.Sin(laserAngle)) *
                                 new Vector2((float)laserDistance));

            _laserPosition = laserPosition;

            _laserParticles.Emit(10, (p) =>
                                     {
                                         var velocity =
                                            new Vector2(Storage.Random.Next(-300, 300),
                                                        Storage.Random.Next(-300, 300));

                                         p.LifeRemaining = p.Lifetime = Storage.Random.Next(50, 150);

                                         p.Position = _laserPosition;
                                         p.Velocity = velocity;
                                     });

            foreach(var pair in Storage.SandParticles.Particles)
            {
                var id = pair.Key;
                var particle = pair.Value;

                var distanceToParticle =
                    Math.Pow(laserPosition.X - particle.Position.X, 2) +
                    Math.Pow(laserPosition.Y - particle.Position.Y, 2);

                if(distanceToParticle > laserRadius)
                {
                    continue;
                }

                if(!particle.OnFire)
                {
                    particle.Fire = (byte)Math.Min(particle.Fire + (EnergyConsumptionRate * 10), 255);
                }

                _particleQueue.Add(particle);
            }
        }

        private void LaserUpdate()
        {
            foreach(var particle in _particleQueue)
            {
                Messages.SendUpdateSandMessage(Player, particle, Player.Gamer.Id, false);
            }

            if(_particleQueue.Count > 0)
            {
                Messages.SendOneOffMessage(Player);
            }

            _particleQueue.Clear();

            SendActivationMessage();
        }

        protected override void Activate()
        {
            _laserSound.Play();
            Storage.AnimationController.AddGroup(_laserTimerGroup);

            base.Activate();
        }

        protected override void Deactivate()
        {
            _laserSound.Stop();
            Storage.AnimationController.RemoveGroup(_laserTimerGroup);

            base.Deactivate();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if(DrawLaserLength != 0)
            {
                if(Player.Game.GraphicsDevice.PresentationParameters.MultiSampleCount > 1)
                {
                    spriteBatch.Draw(Storage.Sprite("pixel"),
                                     new Rectangle((int)Player.X, (int)Player.Y, 6, DrawLaserLength), null,
                                     Color.Orange, Player.Angle, new Vector2(0.5f, 1.0f), SpriteEffects.None, 0.0f);
                }
                else
                {
                    spriteBatch.Draw(Storage.Sprite("pixel"),
                                     new Rectangle((int)Player.X, (int)Player.Y, 7, DrawLaserLength), null,
                                     Color.Orange, Player.Angle, new Vector2(0.5f, 1.0f), SpriteEffects.None, 0.0f);
                }

                DrawLaserLength = 0;
            }
        }

        public override void SendActivationMessage()
        {
            Messages.SendActivateToolMessage(Player, Slot, Active, "DrawLaserLength", DrawLaserLength, Player.Gamer.Id, true);
        }
    }
}