using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Sand
{
    public class Particle
    {
        public string Id;
        public byte Owner;
        public Vector2 Position, Velocity;
        public Int64 Lifetime, LifeRemaining;
        public Team Team;
        public bool Alive;

        private byte _fire;

        public byte Fire
        {
            get
            {
                return _fire;
            }
            set
            {
                _fire = value;

                if(_fire == 255)
                {
                    OnFire = true;
                }
            }
        }

        public bool OnFire;

        public Particle(string id = null, byte owner = (byte)0)
        {
            if(id != null)
            {
                Id = id;
            }
            else
            {
                Id = Guid.NewGuid().ToString("N");
            }

            Alive = true;
            Owner = owner;
        }
    }

    public class ParticleSystem : Actor
    {
        public Player Player;
        public Dictionary<string, Particle> Particles;

        public bool IsSand;
        private HashSet<Particle> _particleQueue;

        public delegate void EmitParticleDelegate(Particle particle);

        public ParticleSystem(Game game, Player player) : base(game)
        {
            DrawOrder = 50;
            Player = player;
            Particles = new Dictionary<string, Particle>(1000);
            _particleQueue = new HashSet<Particle>();
        }

        public override void Update(GameTime gameTime)
        {
            var size = IsSand ? 4 : 2;

            double burningVolume = 0.0;

            foreach(var particle in Particles.Values.Where(particle => particle.Alive))
            {
                if(!IsSand && particle.LifeRemaining <= 0)
                {
                    particle.Alive = false;
                    continue;
                }

                var newPosition = particle.Position +
                                  (particle.Velocity * new Vector2((float)gameTime.ElapsedGameTime.TotalSeconds));

                if(IsSand)
                {
                    particle.Velocity *= new Vector2(0.95f, 0.95f);
                }
                else
                {
                    particle.LifeRemaining -= (Int64)gameTime.ElapsedGameTime.TotalMilliseconds;
                }

                if(!_sandGame.GameMap.CollisionTest(newPosition, size))
                {
                    particle.Position = newPosition;
                }
                else
                {
                    if(!_sandGame.GameMap.CollisionTest(new Vector2(newPosition.X, particle.Position.Y), size))
                    {
                        particle.Velocity.Y = -particle.Velocity.Y;
                        particle.Position.X = newPosition.X;
                    }
                    else if(!_sandGame.GameMap.CollisionTest(new Vector2(particle.Position.X, newPosition.Y), size))
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

                if(particle.OnFire)
                {
                    particle.Fire = (byte)Math.Max(particle.Fire - 10, 0);
                    burningVolume +=
                        Math.Sqrt(Math.Pow(particle.Position.X - Player.X, 2) +
                                  Math.Pow(particle.Position.Y - Player.Y, 2)); // TODO: play burning sound
                }

                if(IsSand && particle.Owner == Player.Gamer.Id)
                {
                    const int fireSpreadRadius = 10 * 10;

                    if(particle.OnFire)
                    {
                        if(particle.Fire == 0)
                        {
                            particle.Alive = false;

                            foreach(var pair in Storage.SandParticles.Particles)
                            {
                                var id = pair.Key;
                                var neighborParticle = pair.Value;

                                var distanceToParticle =
                                    Math.Pow(neighborParticle.Position.X - particle.Position.X, 2) +
                                    Math.Pow(neighborParticle.Position.Y - particle.Position.Y, 2);

                                if(distanceToParticle > fireSpreadRadius)
                                {
                                    continue;
                                }

                                neighborParticle.OnFire = true;
                                neighborParticle.Fire = 255;

                                _particleQueue.Add(neighborParticle);
                            }

                            _particleQueue.Add(particle);
                        }
                    }
                }
            }

            foreach(var sendParticle in _particleQueue)
            {
                Messages.SendCreateSandMessage(Player, sendParticle, Player.Gamer.Id, false);
            }

            if(_particleQueue.Count > 0)
            {
                Messages.SendOneOffMessage(Player);
            }

            _particleQueue.Clear();
        }

        public override void Draw(GameTime gameTime)
        {
            foreach(var particle in Particles.Values.Where(particle => particle.Alive))
            {
                if(!IsSand && particle.LifeRemaining <= 0)
                {
                    continue;
                }

                Color color;
                var size = IsSand ? (particle.OnFire ? ((255 - particle.Fire) / 30) + 4 : 4) : 2;
                var offset = size / 2;

                if(IsSand)
                {
                    if(particle.OnFire)
                    {
                        color = Color.Orange * (float)Storage.Random.NextDouble();
                    }
                    else
                    {
                        color = Teams.ColorForTeam(particle.Team);
                    }
                }
                else
                {
                    color = Color.Orange;
                }

                var gray = IsSand ? 1.0f : particle.LifeRemaining / (float)particle.Lifetime;
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
            foreach(var particle in Particles.Values.Where(particle => particle.Alive == false))
            {
                if(!IsSand && particle.LifeRemaining > 0)
                {
                    continue;
                }

                number--;

                particle.OnFire = false;
                particle.Fire = 0;
                particle.Alive = true;
                particle.Id = Guid.NewGuid().ToString("N");

                emitDelegate(particle);

                if(number == 0)
                {
                    break;
                }
            }

            for(int i = 0; i < number; i++)
            {
                var particle = new Particle(null, Player.Gamer.Id);
                emitDelegate(particle);
                Emit(particle);
            }
        }
    }
}