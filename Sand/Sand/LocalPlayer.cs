using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Sand.Tools.Utilities;

namespace Sand
{
    public class LocalPlayer : Player
    {
        private MouseState _oldMouseState;
        private KeyboardState _oldKeyState;

        public LocalPlayer(Game game, NetworkGamer gamer) : base(game, gamer)
        {
            MovementAcceleration = DefaultAcceleration;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            UpdateStun(gameTime);
            UpdateInput(gameTime);
            UpdatePosition(gameTime);
            UpdateAngle();
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

            if(newKeyState.IsKeyDown(Keys.R))
            {
                if(Mobility != null)
                {
                    Mobility.Reset();
                }

                if(Weapon != null)
                {
                    Weapon.Reset();
                }

                if(Utility != null)
                {
                    Utility.Reset();
                }

                if(PrimaryA != null)
                {
                    PrimaryA.Reset();
                }

                if(PrimaryB != null)
                {
                    PrimaryB.Reset();
                }
            }

            if(newKeyState.IsKeyDown(Keys.P))
            {
                var p = new Particle(null, Gamer.Id);

                p.LifeRemaining = p.Lifetime = 100;

                var angle = (float)(Storage.Random.NextDouble() * (Math.PI / 8.0)) - (Math.PI / 16.0f);
                var length = (float)Storage.Random.Next(200, 450);

                p.Team = Team;
                p.Position = new Vector2(X, Y);
                p.Velocity = Velocity +
                             new Vector2(
                                 (float)Math.Cos(Angle - (Math.PI / 2.0f) + angle) * length,
                                 (float)Math.Sin(Angle - (Math.PI / 2.0f) + angle) * length);

                Storage.SandParticles.Emit(p);
            }

            if(newKeyState.IsKeyDown(Keys.Y) && !_oldKeyState.IsKeyDown(Keys.Y))
            {
                Stun(25.0f);
            }

            if(!Stunned)
            {
                var tools = new[] { Mobility, Utility, Weapon, PrimaryA, PrimaryB };

                foreach(var tool in tools.Where(tool => tool != null))
                {
                    if(tool.ShouldDisable(newKeyState, newMouseState, _oldKeyState, _oldMouseState))
                    {
                        tool.Active = false;
                    }
                }

                bool activeTool = Mobility.Active || Weapon.Active || Utility.Active ||
                                  (PrimaryA != null && PrimaryA.Active) ||
                                  (PrimaryB != null && PrimaryB.Active);

                if(!activeTool)
                {
                    foreach(var tool in tools.Where(tool => tool != null))
                    {
                        if(tool.ShouldEnable(newKeyState, newMouseState, _oldKeyState, _oldMouseState))
                        {
                            tool.Active = true;
                            break;
                        }
                    }
                }
            }
            else
            {
                Mobility.Active = false;
                Weapon.Active = false;
                Utility.Active = false;

                if(PrimaryA != null)
                {
                    PrimaryA.Active = false;
                }

                if(PrimaryB != null)
                {
                    PrimaryB.Active = false;
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

            PureAcceleration = Acceleration;

            Acceleration.X -= Drag.X * Velocity.X;
            Acceleration.Y -= Drag.Y * Velocity.Y;

            Velocity.X += Acceleration.X * timestep;
            Velocity.Y += Acceleration.Y * timestep;

            newPosition.X += Velocity.X * timestep;
            newPosition.Y += Velocity.Y * timestep;

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
                        Velocity.Y = -Velocity.Y;
                        X = newPosition.X;
                    }
                    else if(!_sandGame.GameMap.CollisionTest(_texture, new Rectangle((int)(X - (Width / 2.0)),
                                                                                     (int)
                                                                                     (newPosition.Y - (Height / 2.0)),
                                                                                     (int)Width, (int)Height)))
                    {
                        Velocity.X = -Velocity.X;
                        Y = newPosition.Y;
                    }
                    else
                    {
                        Velocity.X = -Velocity.X;
                        Velocity.Y = -Velocity.Y;
                    }
                }
            }
            else
            {
                Velocity.X = Velocity.Y = 0;
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
                Messages.SendPlaySoundMessage(this, "Shock", Gamer.Id, true);
            }

            var newStunTimeRemaining = new TimeSpan(0, 0, (int)(energy / 5));

            if(!wasStunned || newStunTimeRemaining > StunTimeRemaining)
            {
                StunTimeRemaining = newStunTimeRemaining;
                _unstunTime = new TimeSpan(Storage.CurrentTime.TotalGameTime.Ticks).Add(StunTimeRemaining);
            }
        }
    }
}