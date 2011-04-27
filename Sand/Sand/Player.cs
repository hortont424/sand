﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Net;
using Sand.Tools;

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
        public Vector2 Drag = new Vector2(1.5f, 1.5f);
        public Vector2 MovementAcceleration;
        public readonly Vector2 DefaultAcceleration = new Vector2(450.0f, 450.0f);

        public TimeSpan StunTimeRemaining, ProtectTimeRemaining;

        protected TimeSpan _unstunTime, _unprotectTime;
        private Class _class;
        private Team _team;
        private Texture2D _sprite;

        public Tool PrimaryA, PrimaryB;
        public Tool Mobility;
        public Tool Weapon;
        public Tool Utility;

        public Tool CurrentPrimary, LastTool, AlternatePrimary;

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
        public bool Protected { get; set; }

        public Player(Game game, NetworkGamer gamer) : base(game)
        {
            Gamer = gamer;
            DrawOrder = 100;
            Width = Storage.Sprite("DefenseClass").Width;
            Height = Storage.Sprite("DefenseClass").Height;

            Texture = new Color[(int)(Width * Height)];
            Class = Class.None;
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

            _spriteBatch.Draw(Storage.Sprite("pixel"),
                              new Rectangle((int)virtualX, (int)virtualY, lineWidth, wallIntersection),
                              null,
                              lineColor, Angle, new Vector2(0.5f, 1.0f), SpriteEffects.None, 0.0f);

            _spriteBatch.Draw(_sprite, new Rectangle((int)virtualX, (int)virtualY, (int)Width, (int)Height),
                              null,
                              teamColor, Angle, new Vector2(Width / 2.0f, Height / 2.0f), SpriteEffects.None, 0.0f);

            if(Protected)
            {
                var sprite = Storage.Sprite("WhiteCircle");

                _spriteBatch.Draw(sprite, new Vector2((int)virtualX, (int)virtualY), null,
                                  Color.DarkSlateBlue, 0.0f,
                                  new Vector2(sprite.Width / 2.0f, sprite.Height / 2.0f), 1.5f,
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

            if(Phase == GamePhases.Phase1)
            {
                drawBoundaryTools = new[] { Mobility, Utility, Weapon, CurrentPrimary };
            }
            else if(Phase == GamePhases.Phase2)
            {
                drawBoundaryTools = new[] { Mobility, Utility, Weapon };
            }

            var toolAngle = Angle - ((float)Math.PI / 2.0f);

            foreach(var tool in drawBoundaryTools)
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