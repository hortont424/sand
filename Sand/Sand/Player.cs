using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Net;
using Sand.Tools;

namespace Sand
{
    public class Player : Actor
    {
        public NetworkGamer Gamer;

        protected Color[] _texture;

        public float Angle;

        public Vector2 Velocity;
        public Vector2 Acceleration;
        public Vector2 PureAcceleration;
        public Vector2 Drag = new Vector2(1.5f, 1.5f);
        public Vector2 MovementAcceleration;
        public readonly Vector2 DefaultAcceleration = new Vector2(450.0f, 450.0f);

        public TimeSpan StunTimeRemaining;

        protected TimeSpan _unstunTime;
        private Class _class;
        private Team _team;
        private Texture2D _sprite;

        public Tool PrimaryA, PrimaryB;
        public Tool Mobility;
        public Tool Weapon;
        public Tool Utility;

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

        public float Invisible { get; set; }

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
        }

        public override void Draw(GameTime gameTime)
        {
            var teamColor = Teams.ColorForTeam(Team);

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

            if(PrimaryA != null)
            {
                PrimaryA.Draw(_spriteBatch);
            }

            if(PrimaryB != null)
            {
                PrimaryB.Draw(_spriteBatch);
            }

            int shakeAmplitude = (int)(StunTimeRemaining.TotalMilliseconds / 1000) + 1;
            var virtualX = Stunned ? X + Storage.Random.Next(-shakeAmplitude, shakeAmplitude) : X;
            var virtualY = Stunned ? Y + Storage.Random.Next(-shakeAmplitude, shakeAmplitude) : Y;

            if(Invisible != 0.0f)
            {
                double hue, saturation, value;
                SandColor.ToHSV(teamColor, out hue, out saturation, out value);

                if(this is LocalPlayer)
                {
                    teamColor = SandColor.FromHSV(hue, saturation, Math.Max(value - Invisible, 0.2));
                }
                else
                {
                    Console.WriteLine(value);
                    teamColor = SandColor.FromHSV(hue, saturation, Math.Max(value - Invisible, 0.0));
                }
            }

            // TODO: hack
            if(Invisible == 1.0f && !(this is LocalPlayer))
            {
                return;
            }

            if(this is LocalPlayer)
            {
                if(Game.GraphicsDevice.PresentationParameters.MultiSampleCount > 1)
                {
                    _spriteBatch.Draw(Storage.Sprite("pixel"), new Rectangle((int)virtualX, (int)virtualY, 1, 3000),
                                      null,
                                      teamColor, Angle, new Vector2(0.5f, 1.0f), SpriteEffects.None, 0.0f);
                }
                else
                {
                    _spriteBatch.Draw(Storage.Sprite("pixel"), new Rectangle((int)virtualX, (int)virtualY, 2, 3000),
                                      null,
                                      teamColor, Angle, new Vector2(0.5f, 1.0f), SpriteEffects.None, 0.0f);
                }
            }

            _spriteBatch.Draw(_sprite, new Rectangle((int)virtualX, (int)virtualY, (int)Width, (int)Height),
                              null,
                              teamColor, Angle, new Vector2(Width / 2.0f, Height / 2.0f), SpriteEffects.None, 0.0f);

            var localPlayer = this as LocalPlayer;
            if(localPlayer != null)
            {
                var tool = localPlayer.LastTool;

                if(tool == null)
                {
                    return;
                }

                if(tool.Energy == tool.TotalEnergy)
                {
                    return;
                }

                _spriteBatch.Draw(Storage.Sprite("pixel"),
                                  new Rectangle((int)(virtualX - (_sprite.Width / 2.0f)),
                                                (int)(virtualY + (_sprite.Height / 2.0f) + 15),
                                                _sprite.Width, 7),
                                  null,
                                  new Color(0.2f, 0.2f, 0.2f), 0.0f, new Vector2(0.0f, 0.5f), SpriteEffects.None, 0.0f);

                _spriteBatch.Draw(Storage.Sprite("pixel"),
                                  new Rectangle((int)(virtualX - (_sprite.Width / 2.0f)),
                                                (int)(virtualY + (_sprite.Height / 2.0f) + 15),
                                                (int)((tool.Energy / tool.TotalEnergy) * _sprite.Width), 7),
                                  null,
                                  teamColor, 0.0f, new Vector2(0.0f, 0.5f), SpriteEffects.None, 0.0f);
            }
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

        public Tool ToolInSlot(ToolSlot slot, ToolType type)
        {
            switch(slot)
            {
                case ToolSlot.Primary:
                    return PrimaryA.Type == type ? PrimaryA : PrimaryB;
                case ToolSlot.Weapon:
                    return Weapon;
                case ToolSlot.Mobility:
                    return Mobility;
                case ToolSlot.Utility:
                    return Utility;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}