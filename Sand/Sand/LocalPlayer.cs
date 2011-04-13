﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Sand.Tools;
using Sand.Tools.Mobilities;
using Sand.Tools.Utilities;
using Sand.Tools.Weapons;

namespace Sand
{
    public class LocalPlayer : Player
    {
        public Vector2 Acceleration;
        public Vector2 Drag;
        public Vector2 MovementAcceleration;
        public readonly Vector2 DefaultAcceleration;

        public Tool PrimaryA, PrimaryB;
        public Tool Mobility;
        public Tool Weapon;
        public Tool Utility;

        private MouseState _oldMouseState;
        private KeyboardState _oldKeyState;
        private Vector2 _velocity;

        private ParticleSystem _particles;
        private Vector2 _pureAcceleration;

        public LocalPlayer(Game game, NetworkGamer gamer) : base(game, gamer)
        {
            Drag = new Vector2(1.5f, 1.5f);
            DefaultAcceleration = new Vector2(450.0f, 450.0f);
            MovementAcceleration = DefaultAcceleration;

            _particles = new ParticleSystem(game, this);
            Children.Add(_particles);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            UpdateStun(gameTime);
            UpdateInput(gameTime);
            UpdatePosition(gameTime);
            UpdateAngle();

            // TODO: move into BoostDrive
            if (Mobility is BoostDrive && Mobility.Active)
            {
                _particles.Emit(100, (p) =>
                                     {
                                         var velocity = new Vector2(-_pureAcceleration.X * 0.2f + _random.Next(-70, 70),
                                                                    -_pureAcceleration.Y * 0.2f + _random.Next(-70, 70));

                                         p.LifeRemaining = p.Lifetime = _random.Next(150, 350);

                                         var angle = (float)_random.NextDouble()*(Math.PI * 2.0f);
                                         var length = (float)_random.Next(0, 6);

                                         p.Position = new Vector2((float)(X + (length * Math.Cos(angle))), (float)(Y + (length * Math.Sin(angle))));
                                         p.Velocity = velocity;
                                     });
            }
        }

        private void UpdateStun(GameTime gameTime)
        {
            StunTimeRemaining = new TimeSpan(_unstunTime.Ticks - gameTime.TotalGameTime.Ticks);

            if(Stunned && gameTime.TotalGameTime.Ticks > _unstunTime.Ticks)
            {
                Stunned = false;
            }
        }

        private void UpdateInput(GameTime gameTime)
        {
            var newKeyState = Keyboard.GetState();
            var newMouseState = Mouse.GetState();

            if(!Storage.AcceptInput)
            {
                _oldKeyState = newKeyState;
                _oldMouseState = newMouseState;

                return;
            }

            Acceleration.X = Acceleration.Y = 0.0f;

            if(newKeyState.IsKeyDown(Keys.A))
            {
                Acceleration.X = -MovementAcceleration.X;
            }
            else if(newKeyState.IsKeyDown(Keys.D))
            {
                Acceleration.X = MovementAcceleration.X;
            }

            if(newKeyState.IsKeyDown(Keys.W))
            {
                Acceleration.Y = -MovementAcceleration.Y;
            }
            else if(newKeyState.IsKeyDown(Keys.S))
            {
                Acceleration.Y = MovementAcceleration.Y;
            }

            if(newKeyState.IsKeyDown(Keys.D1))
            {
                Class = Class.Defense;
            }
            else if(newKeyState.IsKeyDown(Keys.D2))
            {
                Class = Class.Offense;
            }
            else if(newKeyState.IsKeyDown(Keys.D3))
            {
                Class = Class.Support;
            }

            if(!Stunned)
            {
                if (newKeyState.IsKeyDown(Mobility.Key) != _oldKeyState.IsKeyDown(Mobility.Key))
                {
                    Mobility.Active = newKeyState.IsKeyDown(Mobility.Key);
                }

                if (newMouseState.LeftButton != _oldMouseState.LeftButton)
                {
                    // TODO: make sure cursor is in our window!!
                    Weapon.Active = (newMouseState.LeftButton == ButtonState.Pressed);
                }

                if (newKeyState.IsKeyDown(Utility.Key) != _oldKeyState.IsKeyDown(Utility.Key))
                {
                    Utility.Active = newKeyState.IsKeyDown(Utility.Key);
                }

                if (newKeyState.IsKeyDown(Keys.Y) && !_oldKeyState.IsKeyDown(Keys.Y))
                {
                    Stun(25.0f);
                }

                if(PrimaryA != null && newKeyState.IsKeyDown(PrimaryA.Key) && !_oldKeyState.IsKeyDown(PrimaryA.Key))
                {
                    PrimaryA.Active = newKeyState.IsKeyDown(PrimaryA.Key);
                }
            }

            _oldKeyState = newKeyState;
            _oldMouseState = newMouseState;
        }

        private void UpdateAngle()
        {
            Angle = (float)Math.Atan2(_sandGame.MouseLocation.Y - Y, _sandGame.MouseLocation.X - X) +
                    ((float)Math.PI / 2.0f);
        }

        private void UpdatePosition(GameTime gameTime)
        {
            var newPosition = new Vector2(X, Y);
            var timestep = (float)(gameTime.ElapsedGameTime.TotalSeconds);

            _pureAcceleration = Acceleration;

            Acceleration.X -= Drag.X * _velocity.X;
            Acceleration.Y -= Drag.Y * _velocity.Y;

            _velocity.X += Acceleration.X * timestep;
            _velocity.Y += Acceleration.Y * timestep;

            newPosition.X += _velocity.X * timestep;
            newPosition.Y += _velocity.Y * timestep;

            if(!Stunned)
            {
                if(!_sandGame.GameMap.CollisionTest(_texture,
                                                   new Rectangle((int)(newPosition.X - (Width / 2.0)),
                                                                 (int)(newPosition.Y - (Height / 2.0)),
                                                                 (int)Width, (int)Height)))
                {
                    X = newPosition.X;
                    Y = newPosition.Y;
                }
                else
                {
                    if(!_sandGame.GameMap.CollisionTest(_texture, new Rectangle((int)(newPosition.X - (Width / 2.0)),
                                                                               (int)(Y - (Height / 2.0)), (int)Width,
                                                                               (int)Height)))
                    {
                        _velocity.Y = -_velocity.Y;
                        X = newPosition.X;
                    }
                    else if(!_sandGame.GameMap.CollisionTest(_texture, new Rectangle((int)(X - (Width / 2.0)),
                                                                                    (int)
                                                                                    (newPosition.Y - (Height / 2.0)),
                                                                                    (int)Width, (int)Height)))
                    {
                        _velocity.X = -_velocity.X;
                        Y = newPosition.Y;
                    }
                    else
                    {
                        _velocity.X = -_velocity.X;
                        _velocity.Y = -_velocity.Y;
                    }
                }
            }
            else
            {
                _velocity.X = _velocity.Y = 0;
            }
        }

        public override void Stun(float energy)
        {
            var shield = Utility as Shield;
            var wasStunned = Stunned;

            if(shield != null)
            {
                energy = shield.DeflectShock(energy);
            }
            else
            {
                Stunned = true;
            }

            if(Stunned)
            {
                Storage.Sound("Shock").CreateInstance().Play();
                Messages.SendPlaySoundMessage(this, "Shock", this.Gamer.Id, true);
            }

            var newStunTimeRemaining = new TimeSpan(0, 0, (int)(energy / 5));

            if(!wasStunned || newStunTimeRemaining > StunTimeRemaining)
            {
                StunTimeRemaining = newStunTimeRemaining;
                _unstunTime = new TimeSpan(Storage.CurrentTime.TotalGameTime.Ticks).Add(StunTimeRemaining);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            if(Utility != null)
            {
                Utility.Draw(_spriteBatch);
            }

            if(Mobility != null)
            {
                Mobility.Draw(_spriteBatch);
            }

            if(Weapon != null)
            {
                Weapon.Draw(_spriteBatch);
            }

            base.Draw(gameTime);

            // TODO: drawing for primaries
        }
    }
}