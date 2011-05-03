using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Sand.Tools;
using Sand.Tools.Utilities;

namespace Sand
{
    public class LocalPlayer : Player
    {
        private MouseState _oldMouseState;
        private KeyboardState _oldKeyState;
        private GamePhases _oldPhase;

        private bool _wasStunned;

        public float Blurriness
        {
            get
            {
                return Storage.Game.EffectBlurriness.GetValueSingle();
            }
            set
            {
                Storage.Game.EffectBlurriness.SetValue(value);
            }
        }

        public LocalPlayer(Game game, NetworkGamer gamer) : base(game, gamer)
        {
            MovementAcceleration = DefaultAcceleration;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if(_oldPhase != Phase)
            {
                ResetPlayerState();
                DisableAllTools();
                _oldPhase = Phase;
            }

            UpdateStun(gameTime);
            UpdateInput(gameTime);
            UpdatePosition(gameTime);
            UpdateAngle();

            if(Stunned && !_wasStunned)
            {
                Storage.Game.Effect.CurrentTechnique = Storage.Game.Effect.Techniques["Blur"];
                Storage.AnimationController.Add(new Animation(this, "Blurriness", 0.004f), 1000);
            }
            else if(!Stunned && _wasStunned)
            {
                Storage.AnimationController.Add(
                    new Animation(this, "Blurriness", 0.000001f)
                    {
                        CompletedDelegate =
                            () => Storage.Game.Effect.CurrentTechnique = Storage.Game.Effect.Techniques["None"]
                    }, 750);
            }

            _wasStunned = Stunned;
        }

        private void UpdateStun(GameTime gameTime)
        {
            StunTimeRemaining = new TimeSpan(_unstunTime.Ticks - gameTime.TotalGameTime.Ticks);
            ProtectTicks = ((Storage.CurrentTime.TotalGameTime.Ticks - LastShockTime.Ticks) / 2);

            if(Stunned && gameTime.TotalGameTime.Ticks > _unstunTime.Ticks)
            {
                Stunned = false;
            }
        }

        private void UpdateInput(GameTime gameTime)
        {
            var newKeyState = Keyboard.GetState();
            var newMouseState = Mouse.GetState();

            var modifiedAcceleration = MovementAcceleration;

            Acceleration.X = Acceleration.Y = 0.0f;

            if(CurrentPrimary == null)
            {
                CurrentPrimary = PrimaryA;
                AlternatePrimary = PrimaryB;
            }

            AlternatePrimary.Active = false;

            if(!Storage.AcceptInput || Phase == GamePhases.WonPhase1 ||
               Phase == GamePhases.WonPhase2)
            {
                _oldKeyState = newKeyState;
                _oldMouseState = newMouseState;

                DisableAllTools();

                return;
            }

            var ourSandQuantity = Storage.SandParticles.TeamParticlesWithinRadius(new Vector2(X, Y), 10, Team);
            var theirSandQuantity = Storage.SandParticles.TeamParticlesWithinRadius(new Vector2(X, Y), 10,
                                                                                    Team == Team.Blue
                                                                                        ? Team.Red
                                                                                        : Team.Blue);
            if(!(ourSandQuantity != 0 && theirSandQuantity != 0))
            {
                if(ourSandQuantity != 0)
                {
                    modifiedAcceleration.X *= 1.4f;
                    modifiedAcceleration.Y *= 1.4f;
                }

                if(theirSandQuantity != 0)
                {
                    modifiedAcceleration.X *= 0.5f;
                    modifiedAcceleration.Y *= 0.5f;
                }
            }

            if(newKeyState.IsKeyDown(Keys.A) || newKeyState.IsKeyDown(Keys.Left))
            {
                Acceleration.X = -modifiedAcceleration.X;
            }
            else if(newKeyState.IsKeyDown(Keys.D) || newKeyState.IsKeyDown(Keys.Right))
            {
                Acceleration.X = modifiedAcceleration.X;
            }

            if(newKeyState.IsKeyDown(Keys.W) || newKeyState.IsKeyDown(Keys.Up))
            {
                Acceleration.Y = -modifiedAcceleration.Y;
            }
            else if(newKeyState.IsKeyDown(Keys.S) || newKeyState.IsKeyDown(Keys.Down))
            {
                Acceleration.Y = modifiedAcceleration.Y;
            }

            if(!Storage.ReadyToPlay)
            {
                _oldKeyState = newKeyState;
                _oldMouseState = newMouseState;

                DisableAllTools();

                return;
            }

            if(newKeyState.IsKeyDown(Keys.Q) && _oldKeyState.IsKeyUp(Keys.Q))
            {
                CurrentPrimary = CurrentPrimary == PrimaryA ? PrimaryB : PrimaryA;
                AlternatePrimary = CurrentPrimary == PrimaryB ? PrimaryA : PrimaryB;
            }

            if(Storage.DebugMode)
            {
                UpdateDebugModeInput(newKeyState);
            }

            if((!Stunned || StunType != StunType.ToolStun) && (Phase == GamePhases.Phase1 || Phase == GamePhases.Phase2))
            {
                Tool[] tools = { };

                if(Phase == GamePhases.Phase1)
                {
                    tools = new[] { Mobility, Utility, Weapon, CurrentPrimary };
                }
                else if(Phase == GamePhases.Phase2)
                {
                    tools = new[] { Mobility, Utility, Weapon };
                }

                foreach(var tool in tools.Where(tool => tool != null))
                {
                    if(tool.ShouldDisable(newKeyState, newMouseState, _oldKeyState, _oldMouseState))
                    {
                        tool.Active = false;
                    }
                }

                bool activeTool = Mobility.Active || Weapon.Active || Utility.Active ||
                                  (CurrentPrimary != null && CurrentPrimary.Active);

                if(!activeTool)
                {
                    foreach(var tool in tools.Where(tool => tool != null))
                    {
                        if(tool.ShouldEnable(newKeyState, newMouseState, _oldKeyState, _oldMouseState))
                        {
                            tool.Active = true;
                            LastTool = tool;
                            break;
                        }
                    }
                }
            }
            else
            {
                DisableAllTools();
            }

            _oldKeyState = newKeyState;
            _oldMouseState = newMouseState;
        }

        private void UpdateDebugModeInput(KeyboardState newKeyState)
        {
            if(newKeyState.IsKeyDown(Keys.R))
            {
                ResetPlayerState();
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
        }

        private void ResetPlayerState()
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

        private void DisableAllTools()
        {
            if(Mobility != null)
            {
                Mobility.Active = false;
            }

            if(Weapon != null)
            {
                Weapon.Active = false;
            }

            if(Utility != null)
            {
                Utility.Active = false;
            }

            if(PrimaryA != null)
            {
                PrimaryA.Active = false;
            }

            if(PrimaryB != null)
            {
                PrimaryB.Active = false;
            }
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

            if(!Stunned || StunType != StunType.MotionStun)
            {
                if(!_sandGame.GameMap.CollisionTest(Texture,
                                                    new Rectangle((int)(newPosition.X - (Width / 2.0)),
                                                                  (int)(newPosition.Y - (Height / 2.0)),
                                                                  (int)Width, (int)Height)))
                {
                    X = newPosition.X;
                    Y = newPosition.Y;
                }
                else
                {
                    if(!_sandGame.GameMap.CollisionTest(Texture, new Rectangle((int)(newPosition.X - (Width / 2.0)),
                                                                               (int)(Y - (Height / 2.0)), (int)Width,
                                                                               (int)Height)))
                    {
                        Velocity.Y = -Velocity.Y * 0.6f;
                        X = newPosition.X;
                    }
                    else if(!_sandGame.GameMap.CollisionTest(Texture, new Rectangle((int)(X - (Width / 2.0)),
                                                                                    (int)
                                                                                    (newPosition.Y - (Height / 2.0)),
                                                                                    (int)Width, (int)Height)))
                    {
                        Velocity.X = -Velocity.X * 0.6f;
                        Y = newPosition.Y;
                    }
                    else
                    {
                        Velocity.X = -Velocity.X * 0.6f;
                        Velocity.Y = -Velocity.Y * 0.6f;
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
            var wasStunned = Stunned;
            var doStun = false;

            if(energy > 0.0f)
            {
                if(!(Utility is Shield && Utility.Active))
                {
                    doStun = true;
                    StunType = StunType.ToolStun;
                }
            }
            else
            {
                Stunned = false;
            }

            if(doStun)
            {
                Stunned = true;
                Sound.OneShot("Shock");

                if(!wasStunned)
                {
                    StunTimeRemaining = new TimeSpan(0);
                }

                StunTimeRemaining +=
                    new TimeSpan(Math.Min((Storage.CurrentTime.TotalGameTime.Ticks - LastShockTime.Ticks) / 2,
                                          new TimeSpan(0, 0, 5).Ticks));
                _unstunTime = new TimeSpan(Storage.CurrentTime.TotalGameTime.Ticks).Add(StunTimeRemaining);
                LastShockTime = Storage.CurrentTime.TotalGameTime;
            }
        }
    }
}