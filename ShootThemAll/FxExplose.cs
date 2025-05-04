using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Mugen.Core;
using Mugen.Physics;
using Mugen.GFX;

namespace ShootThemAll
{
    struct Particles
    {
        public enum Shapes
        {
            Point,
            Circle,
            Line,
            Square,
            Texture,
        }

        Vector2 _position;
        Vector2 _velocity;
        float _angle;
        float _speed;
        float _acceleration;
        Color _color;
        float _size;
        float _alpha = 1f;
        public Shapes Shape = Shapes.Circle;
        public Particles(Shapes shape, Vector2 position, float angle, float speed, Color color, float size = 3, float acceleration = .90f)
        {
            Shape = shape;
            _position = position;
            _angle = angle;
            _speed = speed;
            _color = color;
            _size = size;
            _acceleration = acceleration;
        }

        public void Update(GameTime gameTime)
        {
            _speed *= _acceleration;
            _alpha *= _acceleration;

            _velocity = Geo.GetVector(_angle) * _speed;
            _position += _velocity;

        }
        public void Draw(SpriteBatch batch)
        {

            switch (Shape)
            {
                case Shapes.Point:

                    batch.Point(_position, _size, HSV.Adjust(_color, valueMultiplier: 1.5f) * _alpha);
                    batch.Point(_position, _size / 2, HSV.Adjust(_color, valueMultiplier: 2f) * _alpha * .75f);
                    batch.Point(_position, _size / 4, HSV.Adjust(_color, valueMultiplier: 4f) * _alpha * 1f);

                    break;

                case Shapes.Circle:

                    batch.FilledCircle(G.TexCircle, _position, _size, HSV.Adjust(_color, valueMultiplier: 1.5f) * _alpha * .5f);
                    batch.FilledCircle(G.TexCircle, _position, _size / 2, HSV.Adjust(_color, valueMultiplier: 2f) * _alpha * .75f);
                    batch.FilledCircle(G.TexCircle, _position, _size / 4, HSV.Adjust(_color, valueMultiplier: 4f) * _alpha * 1f);

                    break;

                case Shapes.Line:

                    batch.LineTexture(G.TexLine, _position, _position - _velocity * 4, _size, HSV.Adjust(_color, valueMultiplier: 1.5f) * _alpha * .5f);
                    batch.LineTexture(G.TexLine, _position, _position - _velocity * 4, _size / 2, HSV.Adjust(_color, valueMultiplier: 2f) * _alpha * .75f);
                    batch.LineTexture(G.TexLine, _position, _position - _velocity * 4, _size / 4, HSV.Adjust(_color, valueMultiplier: 4f) * _alpha * 1f);

                    break;

                case Shapes.Square:

                    batch.FillRectangleCentered(_position, Vector2.One * _size, HSV.Adjust(_color, valueMultiplier: 1.5f) * _alpha * .5f, 0);
                    batch.FillRectangleCentered(_position, Vector2.One * _size / 2, HSV.Adjust(_color, valueMultiplier: 2f) * _alpha * .75f, 0);
                    batch.FillRectangleCentered(_position, Vector2.One * _size / 4, HSV.Adjust(_color, valueMultiplier: 4f) * _alpha * 1f, 0);

                    break;

                case Shapes.Texture:

                    GFX.Draw(batch, G.TexGlow1, HSV.Adjust(_color, valueMultiplier: 1.5f) * _alpha, 0, _position, Position.CENTER, Vector2.One * _size * .001f);
                    GFX.Draw(batch, G.TexGlow1, HSV.Adjust(_color, valueMultiplier: 2f) * _alpha * .75f, 0, _position, Position.CENTER, Vector2.One * _size * .001f);
                    GFX.Draw(batch, G.TexGlow1, HSV.Adjust(_color, valueMultiplier: 4f) * _alpha * 1f, 0, _position, Position.CENTER, Vector2.One * _size * .001f);

                    break;

                default:
                    break;
            }
        }

    }


    internal class FxExplose : Node
    {
        int _numParticles = 10;
        Particles[] _particles;

        int _lifeTime = 40;
        float _size = 0f;
        Color _color;
        public FxExplose(Particles.Shapes shape, Vector2 position, Color color, float size = 3, int numParticles = 10, int lifeTime = 40, float maxSpeed = 10, float acceleration = .90f)
        {
            _numParticles = numParticles;
            _particles = new Particles[_numParticles];

            _color = color;
            _lifeTime = lifeTime;

            _alpha = 1f;

            _x = position.X;
            _y = position.Y;

            for (int i = 0; i < _numParticles; i++)
            {
                float angle = (float)Misc.Rng.NextDouble() * Geo.RAD_360;
                float speed = (float)Misc.Rng.NextDouble() * maxSpeed;
                float rsize = (float)Misc.Rng.NextDouble() * size + 1f;

                _particles[i] = new Particles(shape, position, angle, speed, color, rsize, acceleration);
            }
        }
        public override Node Update(GameTime gameTime)
        {
            UpdateRect();

            _size += 2f;
            _alpha -= 1/_lifeTime;

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
            if (indexLayer == (int)Layers.Front)
            {
                //batch.FilledCircle(G.TexCircle, AbsXY, _size, _color * _alpha * .25f);
                //batch.FilledCircle(G.TexCircle, AbsXY, _size / 2, _color * _alpha * .5f);
                //batch.FilledCircle(G.TexCircle, AbsXY, _size / 4, _color * _alpha);

                for (int i = 0; i < _numParticles; i++)
                {
                    _particles[i].Draw(batch);
                }
            }

            if (indexLayer == (int)Layers.FrontFX)
                for (int i = 0; i < _numParticles; i++)
                {
                    _particles[i].Draw(batch);
                }

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
