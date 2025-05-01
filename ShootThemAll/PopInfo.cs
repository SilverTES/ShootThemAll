using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Mugen.Animation;
using Mugen.Core;
using Mugen.GFX;


namespace ShootThemAll
{
    public class PopInfo : Node
    {
        string _label;
        Color _color;
        Color _colorBG;

        Animate _animate = new Animate();

        public PopInfo(string label, Color color, Color colorBG, float start = 0, float end = 8, float duration = 24)
        {
            _label = label;
            _color = color;
            _colorBG = colorBG;

            _animate.Add("popup", Easing.BackEaseInOut, new Tweening(start, end, duration));
            _animate.Start("popup");

            _z = -10000; // Over all Node Childs

            _alpha = 1f;
        }

        public override Node Update(GameTime gameTime)
        {
            UpdateRect();

            if (_animate.Off("popup"))
            {
                KillMe();
            }

            _animate.NextFrame();

            _alpha -= .02f;
            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Layers.FrontFX)
            {
                batch.TopCenterBorderedString(G.FontMain, _label, AbsX, AbsY - _animate.Value(), _color * _alpha, _colorBG * _alpha);
            }

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
