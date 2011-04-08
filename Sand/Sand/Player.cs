using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Sand.Tools;
using Sand.Tools.Mobilities;
using Sand.Tools.Utilities;
using Sand.Tools.Weapons;

namespace Sand
{
    public class Player : Actor
    {
        public NetworkGamer Gamer;

        protected Color[] _texture;

        public float Angle;

        private Class _class;
        private Team _team;
        private bool _invisible;
        private Texture2D _sprite;
        protected TimeSpan _unstunTime;
        private readonly Random _random;

        public Team Team
        {
            get
            {
                return _team;
            }
            set
            {
                _team = value;

                if(this is LocalPlayer)
                {
                    Messages.SendUpdatePlayerTeamMessage(this, Gamer.Id, true);
                }
            }
        }

        public Class Class
        {
            get
            {
                return _class;
            }
            set
            {
                _class = value;
                _sprite = Teams.SpriteForClass(_class);
                _sprite.GetData(_texture);

                if(this is LocalPlayer)
                {
                    Messages.SendUpdatePlayerClassMessage(this, Gamer.Id, true);
                }
            }
        }

        public bool Invisible
        {
            get
            {
                return _invisible;
            }
            set
            {
                _invisible = value;

                if(this is LocalPlayer)
                {
                    Messages.SendInvisiblePlayerMessage(this, Gamer.Id, true);
                }
            }
        }

        public bool Stunned { get; set; }

        public Player(Game game, NetworkGamer gamer) : base(game)
        {
            Gamer = gamer;
            DrawOrder = 100;
            Width = Storage.Sprite("DefenseClass").Width;
            Height = Storage.Sprite("DefenseClass").Height;

            X = 60;
            Y = 60;

            _texture = new Color[(int)(Width * Height)];
            Class = Class.None;
            _random = new Random();
        }

        public override void Draw(GameTime gameTime)
        {
            var teamColor =
                Storage.Color(Team == Team.None ? "NeutralTeam" : ((Team == Team.Red) ? "RedTeam" : "BlueTeam"));

            var shakeAmplitude = (_unstunTime.TotalMilliseconds - Storage.CurrentTime.TotalGameTime.TotalMilliseconds) / 1000;
            var virtualX = Stunned ? X + ((shakeAmplitude / 2.0f) - (_random.Next() % shakeAmplitude)) : X;
            var virtualY = Stunned ? Y + ((shakeAmplitude / 2.0f) - (_random.Next() % shakeAmplitude)) : Y;

            if(Invisible)
            {
                if(this is RemotePlayer)
                {
                    return;
                }

                double hue, saturation, value;
                SandColor.ToHSV(teamColor, out hue, out saturation, out value);

                teamColor = SandColor.FromHSV(hue, saturation, Math.Max(value - 0.5, 0.0));
            }

            if(this is LocalPlayer)
            {
                if(Game.GraphicsDevice.PresentationParameters.MultiSampleCount > 1)
                {
                    _spriteBatch.Draw(Storage.Sprite("pixel"), new Rectangle((int)virtualX, (int)virtualY, 1, 3000), null,
                                      teamColor, Angle, new Vector2(0.5f, 1.0f), SpriteEffects.None, 0.0f);
                }
                else
                {
                    _spriteBatch.Draw(Storage.Sprite("pixel"), new Rectangle((int)virtualX, (int)virtualY, 2, 3000), null,
                                      teamColor, Angle, new Vector2(0.5f, 1.0f), SpriteEffects.None, 0.0f);
                }
            }

            _spriteBatch.Draw(_sprite, new Rectangle((int)virtualX, (int)virtualY, (int)Width, (int)Height),
                              null,
                              teamColor, Angle, new Vector2(Width / 2.0f, Height / 2.0f), SpriteEffects.None, 0.0f);
        }

        public float ? Intersects(Ray ray)
        {
            return ray.Intersects(new BoundingBox(
                                      new Vector3(X - (Width / 2.0f), Y - (Height / 2.0f), -1.0f),
                                      new Vector3(X + (Width / 2.0f),
                                                  Y + (Height / 2.0f), 1.0f)));
        }

        public Ray ForwardRay()
        {
            var cannonDirection = new Vector3((float)Math.Cos(Angle - (Math.PI / 2.0f)),
                                              (float)Math.Sin(Angle - (Math.PI / 2.0f)), 0.0f);
            cannonDirection.Normalize();
            return new Ray(new Vector3(X, Y, 0.0f), cannonDirection);
        }

        public virtual void Stun(float energy)
        {
            throw new NotImplementedException();
        }
    }

    internal class RemotePlayer : Player
    {
        public RemotePlayer(Game game, NetworkGamer gamer) : base(game, gamer)
        {
        }
    }

    public class LocalPlayer : Player
    {
        public Vector2 Acceleration;
        public Vector2 Drag;
        public Vector2 MovementAcceleration;
        public readonly Vector2 DefaultAcceleration;

        public readonly List<Tool> Primaries;
        public readonly Tool Mobility;
        public readonly Tool Weapon;
        public readonly Tool Utility;

        private MouseState _oldMouseState;
        private KeyboardState _oldKeyState;
        private Vector2 _velocity;

        public LocalPlayer(Game game, NetworkGamer gamer) : base(game, gamer)
        {
            Drag = new Vector2(1.5f, 1.5f);
            DefaultAcceleration = new Vector2(450.0f, 450.0f);
            MovementAcceleration = DefaultAcceleration;

            Mobility = new BoostDrive(this);
            Weapon = new Cannon(this);
            Utility = new Shield(this);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            UpdateInput(gameTime);
            UpdateAngle();
            UpdatePosition(gameTime);
            UpdateStun(gameTime);
        }

        private void UpdateStun(GameTime gameTime)
        {
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

            if(newKeyState.IsKeyDown(Mobility.Key) != _oldKeyState.IsKeyDown(Mobility.Key))
            {
                Mobility.Active = newKeyState.IsKeyDown(Mobility.Key);
            }

            if(newMouseState.LeftButton != _oldMouseState.LeftButton)
            {
                // TODO: make sure cursor is in our window!!
                Weapon.Active = (newMouseState.LeftButton == ButtonState.Pressed);
            }

            if(newKeyState.IsKeyDown(Utility.Key) != _oldKeyState.IsKeyDown(Utility.Key))
            {
                Utility.Active = newKeyState.IsKeyDown(Utility.Key);
            }

            if(newKeyState.IsKeyDown(Keys.Y) && !_oldKeyState.IsKeyDown(Keys.Y))
            {
                Stun(25.0f);
            }

            _oldKeyState = newKeyState;
            _oldMouseState = newMouseState;
        }

        private void UpdateAngle()
        {
            var sandGame = Game as Sand;

            Angle = (float)Math.Atan2(sandGame.MouseLocation.Y - Y, sandGame.MouseLocation.X - X) +
                    ((float)Math.PI / 2.0f);
        }

        private void UpdatePosition(GameTime gameTime)
        {
            var newPosition = new Vector2(X, Y);
            var sandGame = Game as Sand;
            var timestep = (float)(gameTime.ElapsedGameTime.TotalSeconds);

            Acceleration.X -= Drag.X * _velocity.X;
            Acceleration.Y -= Drag.Y * _velocity.Y;

            _velocity.X += Acceleration.X * timestep;
            _velocity.Y += Acceleration.Y * timestep;

            newPosition.X += _velocity.X * timestep;
            newPosition.Y += _velocity.Y * timestep;

            if(!Stunned)
            {
                if(!sandGame.GameMap.CollisionTest(_texture,
                                                   new Rectangle((int)(newPosition.X - (Width / 2.0)),
                                                                 (int)(newPosition.Y - (Height / 2.0)),
                                                                 (int)Width, (int)Height)))
                {
                    X = newPosition.X;
                    Y = newPosition.Y;
                }
                else
                {
                    if(!sandGame.GameMap.CollisionTest(_texture, new Rectangle((int)(newPosition.X - (Width / 2.0)),
                                                                               (int)(Y - (Height / 2.0)), (int)Width,
                                                                               (int)Height)))
                    {
                        _velocity.Y = -_velocity.Y;
                        X = newPosition.X;
                    }
                    else if(!sandGame.GameMap.CollisionTest(_texture, new Rectangle((int)(X - (Width / 2.0)),
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
        }

        public override void Stun(float energy)
        {
            var shield = Utility as Shield;

            if(shield != null)
            {
                energy = shield.DeflectShock(energy);
            }
            else
            {
                Stunned = true;
            }

            _unstunTime =
                new TimeSpan(Storage.CurrentTime.TotalGameTime.Ticks).Add(new TimeSpan(0, 0, (int)(energy / 5)));
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

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

            // TODO: drawing for primaries
        }
    }
}