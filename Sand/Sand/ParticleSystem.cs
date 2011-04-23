using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

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
        private readonly HashSet<Particle> _particleQueue;
        private readonly SoundEffectInstance _burningSound;
        private readonly Animation _updateRemoteSandTimer;
        private readonly AnimationGroup _updateRemoteSandTimerGroup;
        private readonly HashSet<Particle> _createParticleQueue;

        private readonly ParticleSystem _fireParticles;

        public delegate void EmitParticleDelegate(Particle particle);

        public ParticleSystem(Game game, Player player, bool isSand = false) : base(game)
        {
            DrawOrder = 50;
            Player = player;
            Particles = new Dictionary<string, Particle>(1000);
            IsSand = isSand;
            _particleQueue = new HashSet<Particle>();
            _createParticleQueue = new HashSet<Particle>();

            if(IsSand)
            {
                _fireParticles = new ParticleSystem(Game, Player);

                Children.Add(_fireParticles);

                _burningSound = Storage.Sound("SandBurning").CreateInstance();
                _burningSound.Volume = 0.0f;
                _burningSound.IsLooped = true;
                _burningSound.Play();

                _updateRemoteSandTimer = new Animation { CompletedDelegate = UpdateRemoteSand };
                _updateRemoteSandTimerGroup = new AnimationGroup(_updateRemoteSandTimer, 60) { Loops = true };

                Storage.AnimationController.AddGroup(_updateRemoteSandTimerGroup);
            }
        }

        private void UpdateRemoteSand()
        {
            int sentUpdates = 0, sentCreates = 0;

            while(_particleQueue.Count > 0 && sentUpdates <= 128)
            {
                var particle = _particleQueue.ElementAt(Storage.Random.Next(_particleQueue.Count));

                Messages.SendUpdateSandMessage(Player, particle, Player.Gamer.Id, false);
                sentUpdates++;
                _particleQueue.Remove(particle);
            }

            if(sentUpdates > 0)
            {
                Messages.SendOneOffMessage(Player, false);
            }

            while(_createParticleQueue.Count > 0 && sentCreates <= 128)
            {
                var particle = _createParticleQueue.ElementAt(Storage.Random.Next(_createParticleQueue.Count));

                Messages.SendCreateSandMessage(Player, particle, Player.Gamer.Id, false);
                sentCreates++;
                _createParticleQueue.Remove(particle);
            }

            if(sentCreates > 0)
            {
                Messages.SendOneOffMessage(Player);
            }
        }

        public override void Update(GameTime gameTime)
        {
            var size = IsSand ? 4 : 2;

            var burningVolume = 0.0;

            var drag = new Vector2(3.0f, 3.0f);

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
                    var accel = -drag * particle.Velocity;
                    particle.Velocity += accel * (float)gameTime.ElapsedGameTime.TotalSeconds;
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
                        1.0f / Math.Sqrt(Math.Pow(particle.Position.X - Player.X, 2) +
                                         Math.Pow(particle.Position.Y - Player.Y, 2));
                }

                if(IsSand && particle.OnFire && particle.Fire == 0 && particle.Alive)
                {
                    particle.Alive = false;

                    Particle particle1 = particle; // something about linq
                    _fireParticles.Emit(10, (p) =>
                                            {
                                                p.LifeRemaining = p.Lifetime = 300;

                                                var angle = (float)(Storage.Random.NextDouble() * 2 * Math.PI);
                                                var length = (float)Storage.Random.Next(20, 150);

                                                p.Position = new Vector2(particle1.Position.X, particle1.Position.Y);
                                                p.Velocity = new Vector2(
                                                    (float)Math.Cos(angle) * length,
                                                    (float)Math.Sin(angle) * length);
                                            });
                }

                if(IsSand && particle.Owner == Player.Gamer.Id)
                {
                    const int fireSpreadRadius = 10 * 10;

                    if(particle.OnFire)
                    {
                        if(particle.Fire == 0)
                        {
                            foreach(var pair in Storage.SandParticles.Particles)
                            {
                                var id = pair.Key;
                                var neighborParticle = pair.Value;

                                if(neighborParticle.OnFire)
                                {
                                    continue;
                                }

                                var distanceToParticle =
                                    Math.Pow(neighborParticle.Position.X - particle.Position.X, 2) +
                                    Math.Pow(neighborParticle.Position.Y - particle.Position.Y, 2);

                                if(distanceToParticle > fireSpreadRadius)
                                {
                                    continue;
                                }

                                neighborParticle.OnFire = true;
                                neighborParticle.Fire = (byte)Storage.Random.Next(128, 255);

                                _particleQueue.Add(neighborParticle);
                            }

                            _particleQueue.Add(particle);
                        }
                    }
                }
            }

            if(IsSand)
            {
                // This is all wrong
                burningVolume = Math.Min(burningVolume, 0.9f);
                _burningSound.Volume = (float)burningVolume;
            }
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
                _createParticleQueue.Add(p);
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