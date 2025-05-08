using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Mugen.Core;
using Mugen.GFX;
using System;

namespace ShootThemAll
{
    public class ScreenPlay : Node
    {
        readonly Game _game;
        Area _area;

        float _ticWave = 0f;
        float _wave = 0f;

        
        public ScreenPlay(Game game)
        {
            _game = game;
            
            SetSize(Screen.Width, Screen.Height);

            _area = new Area(game);
            _area.SetPosition(480, 80);
            _area.AppendTo(this);

            // Play music at start !
            MediaPlayer.Play(G.MusicTest);
            MediaPlayer.Volume = 0.08f * G.Volume;
            MediaPlayer.IsRepeating = true;
        }
        public override Node Update(GameTime gameTime)
        {
            UpdateChilds(gameTime);

            _ticWave += 0.1f;
            _wave = (float)Math.Sin(_ticWave) * 4f;

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            batch.GraphicsDevice.Clear(Color.Transparent);

            if (indexLayer == (int)Layers.BackUI)
            {
                batch.Draw(G.TexBG00, new Rectangle(0, 0, Screen.Width, Screen.Height), Color.White);
            }

            if (indexLayer == (int)Layers.Main)
            {
                //batch.FillRectangle(new Rectangle(0, 0, Screen.Width, Screen.Height), Color.DarkSlateBlue * .5f);
                //batch.Grid(Vector2.Zero, Screen.Width, Screen.Height, 40, 40, Color.Gray * .1f, 3f);

                //batch.Draw(G.TexCG00, new Vector2(1100, _wave), Color.White * 1f);
                //batch.Rectangle(((RectangleF)G.TexCG00.Bounds).Translate(new Vector2(1200, 0)), Color.White, 3f);
            }

            if (indexLayer == (int)Layers.Front)
            {
                batch.Rectangle(_area.AbsRectF.Extend(4), Color.Black * .5f, 3f);
            }

            if (indexLayer == (int)Layers.Debug)
            {
                //_grid.Render(batch, G.FontMain, Color.Pink);
                //batch.RectangleTarget(new RectangleF(20, 20, 120, 80), Color.Yellow, 20, 20, 5f);
            }

            DrawChilds(batch, gameTime, indexLayer);

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
