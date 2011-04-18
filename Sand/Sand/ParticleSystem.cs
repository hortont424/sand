using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Sand
{
    public class Particle
    {
        public string Id;
        public Vector2 Position, Velocity;
        public Int64 Lifetime, LifeRemaining;
        public Team Team;

        public Particle(string id = null)
        {
            if(id != null)
            {
                Id = id;
            }
            else
            {
                var uuid = Guid.NewGuid();
                Id = uuid.ToString("N");
            }
        }
    }

    public class ParticleSystem : Actor
    {
        public Player Player;
        public Dictionary<string,Particle> Particles;

        public bool IsSand;

        public delegate void EmitParticleDelegate(Particle particle);

        public ParticleSystem(Game game, Player player) : base(game)
        {
            DrawOrder = 50;
            Player = player;
            Particles = new Dictionary<string,Particle>(1000);
        }

        public override void Update(GameTime gameTime)
        {
            var size = IsSand ? 4 : 2;

            foreach(var particle in Particles.Values)
            {
                if(!IsSand && particle.LifeRemaining <= 0)
                {
                    continue;
                }

                var newPosition = particle.Position + (particle.Velocity * new Vector2((float)gameTime.ElapsedGameTime.TotalSeconds));

                if(IsSand)
                {
                    particle.Velocity *= new Vector2(0.95f, 0.95f);
                }
                else
                {
                    particle.LifeRemaining -= (Int64)gameTime.ElapsedGameTime.TotalMilliseconds;
                }

                if (!_sandGame.GameMap.CollisionTest(newPosition, size))
                {
                    particle.Position = newPosition;
                }
                else
                {
                    if (!_sandGame.GameMap.CollisionTest(new Vector2(newPosition.X, particle.Position.Y), size))
                    {
                        particle.Velocity.Y = -particle.Velocity.Y;
                        particle.Position.X = newPosition.X;
                    }
                    else if (!_sandGame.GameMap.CollisionTest(new Vector2(particle.Position.X, newPosition.Y), size))
                    {
                        particle.Velocity.X = -particle.Velocity.X;
                        particle.Position.Y = newPosition.Y;
                    }
                    else
                    {
                        particle.Velocity.X = -particle.Velocity.X;
                        particle.Velocity.Y = -particle.Velocity.Y;
                    }
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            var size = IsSand ? 4 : 2;
            var offset = size / 2;

            foreach(var particle in Particles.Values)
            {
                if(!IsSand && particle.LifeRemaining <= 0)
                {
                    continue;
                }

                var color = IsSand ? Teams.ColorForTeam(particle.Team) : Color.Orange;
                float gray = IsSand ? 1.0f : particle.LifeRemaining / (float)particle.Lifetime;
                _spriteBatch.Draw(Storage.Sprite("pixel"),
                                  new Rectangle((int)(particle.Position.X - offset), (int)(particle.Position.Y - offset),
                                                size, size), color * gray);
            }
        }

        public void Emit(Particle p, bool broadcast = true)
        {
            if(IsSand && broadcast)
            {
                Messages.SendCreateSandMessage(Player, p, Player.Gamer.Id, true);
            }

            Particles.Add(p.Id, p);
        }

        public void Emit(int number, EmitParticleDelegate emitDelegate)
        {
            foreach(var particle in Particles.Values)
            {
                if(!IsSand && particle.LifeRemaining > 0)
                {
                    continue;
                }

                number--;

                emitDelegate(particle);
            }

            for(int i = 0; i < number; i++)
            {
                var particle = new Particle();
                emitDelegate(particle);
                Emit(particle);
            }
        }
    }
}