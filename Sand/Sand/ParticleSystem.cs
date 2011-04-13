using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Sand
{
    public class Particle
    {
        public Vector2 Position, Velocity;
        public Int64 Lifetime, LifeRemaining;
    }

    public class ParticleSystem : Actor
    {
        public Player Player;
        public List<Particle> Particles;

        public bool IsSand;

        public delegate void EmitParticleDelegate(Particle particle);

        public ParticleSystem(Game game, Player player) : base(game)
        {
            DrawOrder = 50;
            Player = player;
            Particles = new List<Particle>(1000);
        }

        public override void Update(GameTime gameTime)
        {
            foreach (var particle in Particles.Where(particle => particle.LifeRemaining > 0))
            {
                particle.Position += (particle.Velocity * new Vector2((float)gameTime.ElapsedGameTime.TotalSeconds));

                if(IsSand)
                {
                    particle.Velocity *= new Vector2(0.95f, 0.95f);
                }
                else
                {
                    particle.LifeRemaining -= (Int64)gameTime.ElapsedGameTime.TotalMilliseconds;
                }
                
            }
        }

        public override void Draw(GameTime gameTime)
        {
            var teamColor = Teams.ColorForTeam(Player.Team);
            var size = IsSand ? 4 : 2;
            var offset = size / 2;

            foreach (var particle in Particles.Where(particle => particle.LifeRemaining > 0))
            {
                float gray = (float)particle.LifeRemaining / (float)particle.Lifetime;
                _spriteBatch.Draw(Storage.Sprite("pixel"), new Rectangle((int)(particle.Position.X - offset), (int)(particle.Position.Y - offset), size, size), teamColor * gray);
            }
        }

        public void Emit(int number, EmitParticleDelegate emitDelegate)
        {
            foreach (var particle in Particles.Where(particle => particle.LifeRemaining <= 0))
            {
                number--;

                emitDelegate(particle);
            }

            for(int i = 0; i < number; i++)
            {
                var particle = new Particle();
                emitDelegate(particle);
                Particles.Add(particle);
            }
        }
    }
}