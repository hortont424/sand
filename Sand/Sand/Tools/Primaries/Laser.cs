﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace Sand.Tools.Primaries
{
    internal class Laser : Tool
    {
        private readonly HashSet<Particle> _particleQueue;
        private readonly Animation _laserUpdateTimer;
        private readonly AnimationGroup _laserUpdateTimerGroup;
        private readonly SoundEffectInstance _laserSound;
        public int DrawLaserLength { get; set; }
        private Vector2 _laserPosition;
        private readonly ParticleSystem _laserParticles;
        private bool _lastActive;
        private int _lastDrawLaserLength;

        public Laser(Player player) : base(player)
        {
            Modifier = 0.5;

            Energy = TotalEnergy = 150;
            EnergyConsumptionMode = EnergyConsumptionMode.Drain;
            EnergyConsumptionRate = 0.5;
            EnergyRechargeRate = 0.2;

            _particleQueue = new HashSet<Particle>();
            _laserUpdateTimer = new Animation { CompletedDelegate = LaserUpdate };
            _laserUpdateTimerGroup = new AnimationGroup(_laserUpdateTimer, 50) { Loops = true };
            Storage.AnimationController.AddGroup(_laserUpdateTimerGroup);

            _laserSound = Storage.Sound("Laser").CreateInstance();
            _laserSound.IsLooped = true;

            _laserParticles = new ParticleSystem(Player.Game, Player);
            Player.Game.Components.Add(_laserParticles);

            HasMaxDistance = true;
            MaxDistance = 300;
            MaxDistanceColor = Color.Orange;
        }

        public static string _name()
        {
            return "Laser";
        }

        public static string _description()
        {
            return "Precision tool for destroying sand at a distance.";
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

            int ? laserDistance = null;

            int idealDistance = (int)Math.Sqrt(Math.Pow(sandGame.MouseLocation.X - Player.X, 2) +
                                               Math.Pow(sandGame.MouseLocation.Y - Player.Y, 2));

            if(idealDistance < MaxDistance)
            {
                laserDistance = idealDistance;
            }
            else
            {
                laserDistance = MaxDistance;
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
                    particle.OnFire = true;
                }

                _particleQueue.Add(particle);
            }

            if(Active != _lastActive || DrawLaserLength != _lastDrawLaserLength)
            {
                SendActivationMessage();
            }

            _lastActive = Active;
            _lastDrawLaserLength = DrawLaserLength;
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
        }

        protected override void Activate()
        {
            _laserSound.Play();

            base.Activate();
        }

        protected override void Deactivate()
        {
            _laserSound.Stop();

            base.Deactivate();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if(Active)
            {
                if(Player is LocalPlayer)
                {
                    LaserZap();
                }

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

                var laserAngle = Player.Angle - ((float)Math.PI / 2.0f);
                var laserPosition = new Vector2(Player.X, Player.Y) +
                                    (new Vector2((float)Math.Cos(laserAngle), (float)Math.Sin(laserAngle)) *
                                     new Vector2(DrawLaserLength));

                _laserParticles.Emit(20, (p) =>
                                         {
                                             var velocity =
                                                 new Vector2(Storage.Random.Next(-300, 300),
                                                             Storage.Random.Next(-300, 300));

                                             p.LifeRemaining = p.Lifetime = Storage.Random.Next(50, 150);

                                             p.Position = laserPosition;
                                             p.Velocity = velocity;
                                         });
            }
        }

        public override void SendActivationMessage()
        {
            Messages.SendActivateToolMessage(Player, Slot, Type, Active, "DrawLaserLength", DrawLaserLength,
                                             Player.Gamer.Id, true);
        }
    }
}