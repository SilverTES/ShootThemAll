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
    public class ScreenPlay : Node
    {
        public enum Timers
        {
            SpawnEnemy,
        }
        Timer<Timers> _timer = new Timer<Timers>();

        readonly Game _game;
        readonly Hero _hero;

        Collision2DGrid _grid;

        bool _isPaused = false;

        public ScreenPlay(Game game)
        {
            _game = game;
            
            SetSize(Screen.Width, Screen.Height);

            _hero = new Hero(PlayerIndex.One);
            _hero.SetPosition(100, 100);
            _hero.AppendTo(this);

            int cellSize = 80;
            int gridWidth = Screen.Width / cellSize;
            int gridHeight = Screen.Height / cellSize;

            _grid = new Collision2DGrid(gridWidth, gridHeight, cellSize);

            _timer.Set(Timers.SpawnEnemy, Timer.Time(0, 0, 3f), true);
            _timer.Start(Timers.SpawnEnemy);
        }
        public override Node Update(GameTime gameTime)
        {

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

            if (_timer.On(Timers.SpawnEnemy))
            {
                Enemy enemy = new Enemy(Misc.Rng.Next(1,5));
                enemy.SetPosition(Screen.Width, Misc.Rng.Next(100, Screen.Height - 100));
                enemy.AppendTo(this);

                float time = Misc.Rng.Next(10, 30) / 10f;
                _timer.Set(Timers.SpawnEnemy, Timer.Time(0, 0, time), true);
            }

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            batch.GraphicsDevice.Clear(Color.Transparent);

            if (indexLayer == (int)Layers.Main)
            {
                batch.FillRectangle(new Rectangle(0, 0, Screen.Width, Screen.Height), Color.DarkSlateBlue * .5f);
                batch.Grid(Vector2.Zero, Screen.Width, Screen.Height, 40, 40, Color.Gray * .1f, 3f);
            }

            if (indexLayer == (int)Layers.Front)
            {
                if (_isPaused)
                {
                    batch.FillRectangle(new Rectangle(0, 0, Screen.Width, Screen.Height), Color.Black * .5f);
                    batch.FillRectangleCentered(AbsRectF.Center, new Vector2(300, 100), Color.Black * .5f, 0);
                    batch.CenterStringXY(G.FontMain, "P A U S E", AbsRectF.Center, Color.White);
                }
            }

            if (indexLayer == (int)Layers.Debug)
            {
                //_grid.Render(batch, G.FontMain, Color.Pink);
            }

            DrawChilds(batch, gameTime, indexLayer);

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
