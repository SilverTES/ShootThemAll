using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Mugen.Core;
using Mugen.Physics;
using Mugen.GFX;

namespace ShootThemAll
{
    struct Particles
    {
        Vector2 _position;
        Vector2 _velocity;
        float _angle;
        float _speed;
        Color _color;
        float _size;
        float _alpha = 1f;

        public Particles(Vector2 position, float angle, float speed, Color color, float size = 3)
        {
            _position = position;
            _angle = angle;
            _speed = speed;
            _color = color;
            _size = size;
        }

        public void Update(GameTime gameTime)
        {
            _speed *= .90f;

            _velocity = Geo.GetVector(_angle) * _speed;

            _position += _velocity;

            _alpha *= .90f;
        }
        public void Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            //batch.Point(_position, _size, _color * _alpha);
            //batch.Point(_position, _size / 2, Color.LightYellow * _alpha);
            //batch.Point(_position, _size / 4, Color.White * _alpha);

            batch.FilledCircle(G.TexCircle, _position, _size, _color * _alpha);
            batch.FilledCircle(G.TexCircle, _position, _size / 2, Color.LightYellow * _alpha);
            batch.FilledCircle(G.TexCircle, _position, _size / 4, Color.White * _alpha);
        }

    }


    internal class FxExplose : Node
    {
        int _numParticles = 10;
        Particles[] _particles;

        int _lifeTime = 40;
        public FxExplose(Vector2 position, Color color, float size = 3, int numParticles = 10, int lifeTime = 40)
        {
            _numParticles = numParticles;
            _particles = new Particles[_numParticles];

            _lifeTime = lifeTime;

            _x = position.X;
            _y = position.Y;

            for (int i = 0; i < _numParticles; i++)
            {
                float angle = (float)Misc.Rng.NextDouble() * Geo.RAD_360;

                _particles[i] = new Particles(position, angle, 8, color, size);
            }
        }
        public override Node Update(GameTime gameTime)
        {
            _lifeTime--;
            if (_lifeTime <= 0)
                KillMe();

            for (int i = 0; i < _numParticles; i++)
            {
                _particles[i].Update(gameTime);
            }

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Layers.FrontFX)
                for (int i = 0; i < _numParticles; i++)
                {
                    _particles[i].Draw(batch, gameTime, indexLayer);
                }

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
