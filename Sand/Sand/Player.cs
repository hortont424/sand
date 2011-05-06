using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Sand.Tools;
using Sand.Tools.Mobilities;

namespace Sand
{
    public enum StunType
    {
        ToolStun,
        MotionStun
    }

    public class Player : Actor
    {
        public NetworkGamer Gamer;

        public Color[] Texture;

        public float Angle;

        public Vector2 Velocity;
        public Vector2 Acceleration;
        public Vector2 PureAcceleration;
        public Vector2 Drag = new Vector2(3f, 3f);
        public Vector2 MovementAcceleration;
        public readonly Vector2 DefaultAcceleration = new Vector2(750.0f, 750.0f);

        public TimeSpan StunTimeRemaining;

        public TimeSpan LastShockTime;

        protected TimeSpan _unstunTime;
        private Class _class;
        private Team _team;
        private Texture2D _sprite, _spriteFilled;

        public Tool PrimaryA, PrimaryB;
        public Tool Mobility;
        public Tool Weapon;
        public Tool Utility;

        public Tool CurrentPrimary, LastTool, AlternatePrimary;

        protected Queue<Tuple<Vector2, float>> _previousPositions;
        protected const int MaxPreviousPositions = 50;

        public GamePhases Phase { get; set; }

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
                _spriteFilled = Teams.SpriteForClass(_class, ClassVariant.Filled);
                _sprite.GetData(Texture);

                if(this is LocalPlayer)
                {
                    Messages.SendUpdatePlayerClassMessage(this, Gamer.Id, true);
                }
            }
        }

        public float Invisible { get; set; }
        public bool Stunned { get; set; }
        public StunType StunType { get; set; }

        public long ProtectTicks;

        public Player(Game game, NetworkGamer gamer) : base(game)
        {
            Gamer = gamer;
            DrawOrder = 100;
            Width = Storage.Sprite("DefenseClass").Width;
            Height = Storage.Sprite("DefenseClass").Height;

            Texture = new Color[(int)(Width * Height)];
            Class = Class.None;

            _previousPositions = new Queue<Tuple<Vector2, float>>(MaxPreviousPositions);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // This is the best code ever, thanks Nate!

            if(Mobility is BoostDrive && Mobility.Active)
            {
                if(_previousPositions.Count > 0)
                {
                    _previousPositions.Enqueue(
                        new Tuple<Vector2, float>((_previousPositions.Last().Item1 + Position) / 2.0f, Angle));
                }

                _previousPositions.Enqueue(new Tuple<Vector2, float>(Position, Angle));
            }
            else if(_previousPositions.Count > 0)
            {
                _previousPositions.Dequeue();

                if(_previousPositions.Count > 0)
                {
                    _previousPositions.Dequeue();
                }
            }

            while(_previousPositions.Count >= MaxPreviousPositions)
            {
                _previousPositions.Dequeue();
            }
        }

        public override void Draw(GameTime gameTime)
        {
            var teamColor = Teams.ColorForTeam(Team);
            var originalTeamColor = teamColor;

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

            Storage.Game.EffectOffset.SetValue(new Vector2((virtualX - X) * 0.001f, (virtualY - Y) * 0.001f));

            if(Invisible != 0.0f)
            {
                double hue, saturation, value;
                SandColor.ToHSV(teamColor, out hue, out saturation, out value);

                teamColor = SandColor.FromHSV(hue, saturation,
                                              Math.Max(value - Invisible, this is LocalPlayer ? 0.2 : 0.0));
            }

            if(Invisible == 1.0f && this is RemotePlayer)
            {
                return;
            }

            var wallIntersection = (int)(Game as Sand).GameMap.Intersects(ForwardRay());
            var lineWidth = Game.GraphicsDevice.PresentationParameters.MultiSampleCount > 1 ? 1 : 2;
            var lineColor = teamColor;

            if(this is RemotePlayer)
            {
                double hue, saturation, value;
                SandColor.ToHSV(lineColor, out hue, out saturation, out value);

                lineColor = SandColor.FromHSV(hue, saturation, 0.2);
            }

            if(!Storage.InTutorial || Storage.TutorialLevel >= 1)
            _spriteBatch.Draw(Storage.Sprite("pixel"),
                              new Rectangle((int)virtualX, (int)virtualY, lineWidth, wallIntersection),
                              null,
                              lineColor, Angle, new Vector2(0.5f, 1.0f), SpriteEffects.None, 0.0f);

            var i = 0;
            foreach(var position in _previousPositions)
            {
                var scale = ((float)i) / MaxPreviousPositions;

                _spriteBatch.Draw(_sprite, new Vector2((int)position.Item1.X, (int)position.Item1.Y),
                                  null,
                                  teamColor * ((float)i / MaxPreviousPositions) * 0.3f, position.Item2,
                                  new Vector2(Width / 2.0f, Height / 2.0f), scale, SpriteEffects.None, 0.0f);
                i++;
            }

            _spriteBatch.Draw(_sprite, new Rectangle((int)virtualX, (int)virtualY, (int)Width, (int)Height),
                              null,
                              teamColor, Angle, new Vector2(Width / 2.0f, Height / 2.0f), SpriteEffects.None, 0.0f);

            
            var maxProtectTicks = new TimeSpan(0, 0, 0, 5).Ticks;

            if(ProtectTicks < maxProtectTicks)
            {
                var grayLevel = (float)(maxProtectTicks - ProtectTicks) / maxProtectTicks;
                grayLevel = 2.0f * grayLevel / 3.0f;

                _spriteBatch.Draw(_spriteFilled, new Rectangle((int)virtualX, (int)virtualY, (int)Width, (int)Height),
                                  null,
                                  teamColor * grayLevel, Angle, new Vector2(Width / 2.0f, Height / 2.0f),
                                  SpriteEffects.None, 0.0f);
            }

            var localPlayer = this as LocalPlayer;
            if(localPlayer != null)
            {
                var tool = localPlayer.LastTool;

                if(tool != null && (tool.Energy != tool.TotalEnergy))
                {
                    _spriteBatch.Draw(Storage.Sprite("pixel"),
                                      new Rectangle((int)(virtualX - (_sprite.Width / 2.0f)),
                                                    (int)(virtualY + (_sprite.Height / 2.0f) + 15),
                                                    _sprite.Width, 7),
                                      null,
                                      new Color(0.2f, 0.2f, 0.2f), 0.0f, new Vector2(0.0f, 0.5f), SpriteEffects.None,
                                      0.0f);

                    _spriteBatch.Draw(Storage.Sprite("pixel"),
                                      new Rectangle((int)(virtualX - (_sprite.Width / 2.0f)),
                                                    (int)(virtualY + (_sprite.Height / 2.0f) + 15),
                                                    (int)((tool.Energy / tool.TotalEnergy) * _sprite.Width), 7),
                                      null,
                                      originalTeamColor, 0.0f, new Vector2(0.0f, 0.5f), SpriteEffects.None, 0.0f);
                }
            }

            Tool[] drawBoundaryTools = { };

            switch(Phase)
            {
                case GamePhases.Phase1:
                    drawBoundaryTools = new[] { Mobility, Utility, Weapon, CurrentPrimary };
                    break;
                case GamePhases.Phase2:
                    drawBoundaryTools = new[] { Mobility, Utility, Weapon };
                    break;
            }

            if (this is LocalPlayer && (!Storage.InTutorial || Storage.TutorialLevel >= 1))
            {
                var toolAngle = Angle - ((float)Math.PI / 2.0f);

                foreach(var tool in drawBoundaryTools.Where(tool => tool != null))
                {
                    if(!tool.HasMaxDistance)
                    {
                        continue;
                    }

                    var actualDistance = wallIntersection < tool.MaxDistance ? wallIntersection : tool.MaxDistance;

                    var sprite = Storage.Sprite("ToolDot");

                    var toolCirclePosition = new Vector2(virtualX, virtualY) +
                                             (new Vector2((float)Math.Cos(toolAngle), (float)Math.Sin(toolAngle)) *
                                              new Vector2(actualDistance));

                    _spriteBatch.Draw(sprite, toolCirclePosition, null,
                                      tool.MaxDistanceColor, 0.0f,
                                      new Vector2(sprite.Width / 2.0f, sprite.Height / 2.0f), 1.0f,
                                      SpriteEffects.None, 0.0f);
                }
            }

            if(Storage.DebugMode)
            {
                var statsStr = string.Format("{0} particles, {1} alive", Storage.SandParticles.Particles.Count, Storage.SandParticles.Particles.Where(p => p.Value.Alive == true).Count());
                var netStr = string.Format("{0} KB/s in, {1} KB/s out", Storage.NetworkSession.BytesPerSecondReceived / 1024, Storage.NetworkSession.BytesPerSecondSent / 1024);
                _spriteBatch.DrawString(Storage.Font("Calibri24Bold"), statsStr, new Vector2(20, 60), Color.Blue);
                _spriteBatch.DrawString(Storage.Font("Calibri24Bold"), netStr, new Vector2(20, 90), Color.Blue);
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

        public virtual void Stun(float energy, bool miniature = false)
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