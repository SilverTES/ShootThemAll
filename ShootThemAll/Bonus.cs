using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mugen.Core;
using Mugen.GFX;
using Mugen.Physics;
using System;

namespace ShootThemAll
{
    public class Bonus : Node
    {
        public const int ZoneBody = 0;
        public const int Radius = 48;

        float _ticWave = 0f;
        float _wave = 0f;
        public Bonus() 
        {
            _type = UID.Get<Bonus>();

            SetSize(Radius, Radius);
            SetPivot(Position.CENTER);
            _z = -10000; // Over all Node Childs
            _alpha = 1f;

            SetCollideZone(ZoneBody, _rect);
        }
        public void DestroyMe()
        {
            new FxExplose(XY + _parent.XY, Color.YellowGreen, 20, 100, 50).AppendTo(_parent);
            G.SoundBonus.Play(.25f * G.Volume, 1f, 0f);
            KillMe();
        }
        public override Node Update(GameTime gameTime)
        {
            UpdateRect();
            UpdateCollideZone(ZoneBody, _rect);

            _ticWave += 0.25f;
            _wave = (float)Math.Sin(_ticWave) * 4f;

            _y += 1f;

            if (_y > Screen.Height)
            {
                _y = 0;
            }   

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Layers.Main)
            {
                batch.FilledCircle(G.TexCircle, AbsXY, Radius + _wave, Color.Gold * _alpha * .5f);
                batch.FilledCircle(G.TexCircle, AbsXY, Radius - 8 + _wave, Color.Gold * _alpha);
            }

            if (indexLayer == (int)Layers.Glow)
            {

                GFX.Draw(batch, G.TexGlow1, Color.White * _alpha * .5f, 0, AbsXY, Position.CENTER, Vector2.One * .25f);
                GFX.Draw(batch, G.TexCircleGlow, Color.White * _alpha * .5f, 0, AbsXY, Position.CENTER, Vector2.One * .075f);
            }
            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
