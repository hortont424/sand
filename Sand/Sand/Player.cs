using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Net;

namespace Sand
{
    public class Player : Actor
    {
        public NetworkGamer Gamer;

        protected Color[] _texture;

        public float Angle;

        public TimeSpan StunTimeRemaining;
        
        protected TimeSpan _unstunTime;
        private Class _class;
        private Team _team;
        private bool _invisible;
        private Texture2D _sprite;
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

            var shakeAmplitude = StunTimeRemaining.TotalMilliseconds / 1000;
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
}