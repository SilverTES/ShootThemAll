using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mugen.Core;
using Mugen.Event;
using Mugen.GFX;
using Mugen.Input;
using Mugen.Physics;
using System.Collections.Generic;

namespace ShootThemAll
{
    public struct Star
    {
        public Vector2 Position;
        public Color Color;
        public float Size;
        public float Speed;
        public float Alpha;
    }
    public class StarManager
    {
        List<Star> _stars = new List<Star>();

        public StarManager()
        {

        }
        public void AddStar(Vector2 position, Color color, float size, float speed)
        {
            Star star = new Star
            {
                Position = position,
                Color = color,
                Size = size,
                Speed = speed,
                Alpha = 1f
            };
            _stars.Add(star);
        }

        public void GenerateStar(int nbStars, Rectangle rect)
        {
            for (int i = 0; i < nbStars; i++)
            {
                float x = Misc.Rng.Next(0, rect.Width);
                float y = Misc.Rng.Next(0, rect.Height);

                Vector2 pos = new Vector2(x, y);

                Color color = new Color(Misc.Rng.Next(100, 255), Misc.Rng.Next(100, 255), Misc.Rng.Next(100, 255));

                AddStar(pos, color, Misc.Rng.Next(1, 5), Misc.Rng.Next(20, 40) / 10f);
            }
        }

        public void UpdateStars()
        {
            for (int i = 0; i < _stars.Count; i++)
            {
                Star star = _stars[i];
                star.Position.Y += star.Speed;
                //star.Alpha -= 0.01f;
                //if (star.Alpha <= 0)
                //{
                //    _stars.RemoveAt(i);
                //    i--;
                //}
                //else
                //{
                //    _stars[i] = star;
                //}

                if (star.Position.Y > Screen.Height)
                {
                    star.Position.Y = 0;
                }

                _stars[i] = star;
            }
        }
        public void DrawStars(SpriteBatch batch, Vector2 offset)
        {
            foreach (var star in _stars)
            {
                batch.FillRectangle(star.Position + offset, new Vector2(star.Size), star.Color * star.Alpha);
            }
        }

    }

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

        StarManager _starManager = new StarManager();

        readonly Hero _hero;

        Vector2 _gridPos = new Vector2(0, 0);
        float _cellSize = 40f;

        private RasterizerState scissorRasterizerState = new RasterizerState { ScissorTestEnable = true };

        public Area() 
        {
            SetSize(960, Screen.Height - 80);

            int cellSize = 80;
            int gridWidth = (int)_rect.Width / cellSize;
            int gridHeight = (int)_rect.Height / cellSize;

            _grid = new Collision2DGrid(gridWidth, gridHeight, cellSize);

            _timer.Set(Timers.SpawnEnemy, Timer.Time(0, 0, 3f), true);
            _timer.Set(Timers.SpawnBonus, Timer.Time(0, 0, 10f), true);

            _timer.Start(Timers.SpawnEnemy);
            _timer.Start(Timers.SpawnBonus);

            _hero = new Hero(PlayerIndex.One);
            _hero.SetPosition(_rect.Width / 2, _rect.Height - 200);
            _hero.AppendTo(this);

            new Bonus().SetPosition(200, 200).AppendTo(this);
            new Bonus().SetPosition(400, 400).AppendTo(this);


            _timer.On(Timers.SpawnEnemy, () =>
            {
                Enemy enemy = new Enemy(Misc.Rng.Next(1, 4), _hero, Enemy.RandomColor());

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


            _starManager.GenerateStar(400, new Rectangle(0, 0, (int)_rect.Width, (int)_rect.Height));
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

            _gridPos.Y += 4f;

            if (_gridPos.Y >= 0)
            {
                _gridPos.Y = -_cellSize;
            }

            _hero._x = MathHelper.Clamp(_hero._x, _hero._oX, _rect.Width - _hero._oX);
            _hero._y = MathHelper.Clamp(_hero._y, _hero._oY, _rect.Height - _hero._oY);

            _starManager.UpdateStars();

            _timer.Update();
            UpdateChilds(gameTime);
            _grid.UpdateGridSystemZone(this);

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {

            // Sauvegarder l'état actuel
            RasterizerState previousRasterizerState = batch.GraphicsDevice.RasterizerState;
            Rectangle previousScissorRectangle = batch.GraphicsDevice.ScissorRectangle;
            // Définir le rectangle de clipping
            //Rectangle scissorRect = new Rectangle(50, 50, 200, 150);
            // Dessin avec ScissorRectangle
            batch.End();
            
            var param = ScreenManager.GetLayerParameter(indexLayer);

            batch.Begin(rasterizerState: scissorRasterizerState, blendState: param.blendState);
            batch.GraphicsDevice.ScissorRectangle = AbsRect;


            if (indexLayer == (int)Layers.Back)
            {
                _starManager.DrawStars(batch, AbsXY);
            }

            if (indexLayer == (int)Layers.Main)
            {
                batch.Grid(_gridPos + AbsXY, AbsRectF.Width, AbsRectF.Height + _cellSize * 2, _cellSize, _cellSize, Color.WhiteSmoke * .25f, 1f); 

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



            batch.End();
            // Restaurer l'état précédent
            batch.GraphicsDevice.RasterizerState = previousRasterizerState;
            batch.GraphicsDevice.ScissorRectangle = previousScissorRectangle;
            // Dessin supplémentaire (retour à l'état normal)
            batch.Begin();



            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
