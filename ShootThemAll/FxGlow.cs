using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mugen.Core;
using Mugen.GFX;
using Mugen.Physics;

namespace ShootThemAll
{
    public class FxGlow : Node  
    {
        float _size = 1f;
        int _lifeTime = 100;

        Color _color;

        public FxGlow(Vector2 position, Color color, float size = 1f)
        {
            _x = position.X;
            _y = position.Y;
            _size = size;
            _color = color;

            _alpha = 1f;
        }
        public override Node Update(GameTime gameTime)
        {
            UpdateRect();

            _alpha -= .025f;
            _size *= 1.05f;

            _lifeTime--;
            if (_lifeTime <= 0)
            {
                KillMe();
            }

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Layers.Glow)
            {
                GFX.Draw(batch, G.TexGlow1, _color * _alpha, 0, AbsXY, Position.CENTER, Vector2.One * _size);
                GFX.Draw(batch, G.TexCircleGlow, _color * _alpha, 0, AbsXY, Position.CENTER, Vector2.One * _size);
            }

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
