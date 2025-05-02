using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mugen.Core;
using Mugen.Event;
using Mugen.GFX;
using Mugen.Input;
using Mugen.Physics;

namespace ShootThemAll
{
    public class Area : Node
    {
        public enum Timers
        {
            SpawnEnemy,
            SpawnBonus,
        }
        Timer<Timers> _timer = new Timer<Timers>();

        
        Collision2DGrid _grid;

        bool _isPaused = false;

        public Area() 
        {
            SetSize(800, Screen.Height);

            int cellSize = 80;
            int gridWidth = (int)_rect.Width / cellSize;
            int gridHeight = (int)_rect.Height / cellSize;

            _grid = new Collision2DGrid(gridWidth, gridHeight, cellSize);

            _timer.Set(Timers.SpawnEnemy, Timer.Time(0, 0, 3f), true);
            _timer.Set(Timers.SpawnBonus, Timer.Time(0, 0, 10f), true);

            _timer.Start(Timers.SpawnEnemy);
            _timer.Start(Timers.SpawnBonus);


            new Bonus().SetPosition(200, 200).AppendTo(this);
            new Bonus().SetPosition(400, 400).AppendTo(this);


            _timer.On(Timers.SpawnEnemy, () =>
            {
                Enemy enemy = new Enemy(Misc.Rng.Next(1, 4));

                int border = 80;

                enemy.SetPosition(Misc.Rng.Next(border, (int)_rect.Width - border), 0);
                enemy.AppendTo(this);

                float time = Misc.Rng.Next(10, 30) / 10f;
                _timer.Set(Timers.SpawnEnemy, Timer.Time(0, 0, time), true);
            });

            _timer.On(Timers.SpawnBonus, () =>
            {
                Bonus bonus = new();

                int border = 80;

                bonus.SetPosition(Misc.Rng.Next(border, (int)_rect.Width - border), 0);
                bonus.AppendTo(this);

                float time = Misc.Rng.Next(80, 120) / 10f;
                _timer.Set(Timers.SpawnBonus, Timer.Time(0, 0, time), true);
            });

        }
        public override Node Update(GameTime gameTime)
        {
            UpdateRect();

            if (ButtonControl.OnePress("Pause", G.Key.IsKeyDown(Keys.P) || GamePad.GetState(PlayerIndex.One).Buttons.Start == ButtonState.Pressed))
            {
                _isPaused = !_isPaused;
            }

            if (_isPaused)
            {
                return base.Update(gameTime);
            }

            _timer.Update();
            UpdateChilds(gameTime);
            _grid.UpdateGridSystemZone(this);

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Layers.Main)
            {
                batch.FillRectangle(AbsRectF, Color.Black * .5f);
            }

            if (indexLayer == (int)Layers.Front)
            {
                if (_isPaused)
                {
                    batch.FillRectangleCentered(AbsRectF.Center, new Vector2(300, 100), Color.Black * .5f, 0);
                    batch.CenterStringXY(G.FontMain, "P A U S E", AbsRectF.Center, Color.White);
                }
            }

            if (indexLayer == (int)Layers.Debug)
            {

            }

            DrawChilds(batch, gameTime, indexLayer);

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
