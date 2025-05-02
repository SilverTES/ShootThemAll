using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mugen.Core;
using Mugen.GFX;
using Mugen.Physics;

namespace ShootThemAll
{
    public class Bullet : Node
    {
        public const int ZoneBody = 0;

        Vector2 _velocity = new Vector2();
        float _angle;
        float _acceleration = 0f;
        float _speed;

        int _lifeTime;
        public int Power => _power;
        int _power;

        public Node Owner;
        Color _color;
        public Bullet(Node owner, Vector2 position, float angle, float speed, Color color, int lifeTime = 100, int power = 3)
        {
            Owner = owner;
            _type = UID.Get<Bullet>();

            SetPosition(position.X, position.Y);

            _angle = angle;
            _speed = speed;
            _lifeTime = lifeTime;
            _power = power;
            _color = color;

            SetSize(10, 10);
            SetPivot(Position.CENTER);

            SetCollideZone(ZoneBody, _rect);
        }
        public override Node Update(GameTime gameTime)
        {

            UpdateCollideZone(ZoneBody, _rect);

            UpdateRect();

            _acceleration += 2f;
            if (_acceleration > _speed)
                _acceleration = _speed;

            _velocity = Geo.GetVector(_angle) * _acceleration;

            // Update position based on velocity
            _x += _velocity.X;
            _y += _velocity.Y;

            _lifeTime--;

            if (_lifeTime <= 0)
            {
                // Remove the bullet from the game
                KillMe();
            }

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Layers.Main)
            {
                batch.LineTexture(G.TexLine, AbsXY, AbsXY - _velocity * 2, 15, _color * .5f);
                batch.LineTexture(G.TexLine, AbsXY, AbsXY - _velocity * 2, 7,_color * .75f);
                batch.LineTexture(G.TexLine, AbsXY, AbsXY - _velocity * 2, 3, _color * 1f);
            }
            if (indexLayer == (int)Layers.FrontFX)
            {
            }

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
