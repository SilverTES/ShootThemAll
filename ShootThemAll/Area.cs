using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mugen.Core;
using Mugen.Event;
using Mugen.Event.Message;
using Mugen.GFX;
using Mugen.Input;
using Mugen.Physics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShootThemAll
{
    public class TogglePauseMessage : IMessage
    {
        public string Name => "Toggle Pause";
    }
    public class DestroyAllEnemyMessage : IMessage
    {
        public string Name => "Destroy All Enemy";
    }
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

                star.Alpha = 1f - (1f / star.Speed) - .25f;

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
        float _cellSize = 80f;

        public Area() 
        {
            G.PoolBullet = new ObjectPool<Bullet>
            (
                () => new Bullet(null, Vector2.Zero, 0, 0, Color.Transparent),
                4
            );
            G.PoolEnemy = new ObjectPool<Enemy>
            (
                () => new Enemy(null, Color.Transparent, 0f),
                4
            );

            SetSize(640, 960);

            int cellSize = 80;
            int gridWidth = (int)_rect.Width / cellSize;
            int gridHeight = (int)_rect.Height / cellSize;

            _grid = new Collision2DGrid(gridWidth, gridHeight, cellSize);

            _timer.Set(Timers.SpawnEnemy, Timer.Time(0, 0, 3f));
            _timer.Set(Timers.SpawnBonus, Timer.Time(0, 0, 10f));

            _timer.Start(Timers.SpawnEnemy);
            _timer.Start(Timers.SpawnBonus);

            _hero = new Hero(PlayerIndex.One);
            _hero.SetPosition(_rect.Width / 2, _rect.Height - 200);
            _hero.AppendTo(this);

            //new Bonus<FireSpeedMessage>("FireSpeed +10%").SetPosition(200, 200).AppendTo(this);
            //new Bonus<FireSpeedMessage>("FireSpeed +10%").SetPosition(400, 400).AppendTo(this);


            _timer.On(Timers.SpawnEnemy, () =>
            {
                //Enemy enemy = new Enemy(_hero, Enemy.RandomColor(), Misc.Rng.Next(1, 4));

                Enemy enemy = G.PoolEnemy.Get().Set(_hero, Enemy.RandomColor(), Misc.Rng.Next(1, 2));

                int border = 80;

                enemy.SetPosition(Misc.Rng.Next(border, (int)_rect.Width - border), -100);
                enemy.AppendTo(this);

                float time = Misc.Rng.Next(10, 30) / 10f;
                _timer.Set(Timers.SpawnEnemy, Timer.Time(0, 0, time));
            });

            _timer.On(Timers.SpawnBonus, () =>
            {
                Bonus<FireSpeedMessage> bonus = new("FireSpeed +10%");

                int border = 80;

                bonus.SetPosition(Misc.Rng.Next(border, (int)_rect.Width - border), -100);
                bonus.AppendTo(this);

                float time = Misc.Rng.Next(80, 120) / 10f;
                _timer.Set(Timers.SpawnBonus, Timer.Time(0, 0, time));
            });


            _starManager.GenerateStar(100, new Rectangle(0, 0, (int)_rect.Width, (int)_rect.Height));

            MessageBus.Instance.Subscribe<EnemyDestroyedMessage>((m) =>
            {
                Misc.Log($"Enemy {m.Enemy._index} was destroyed !");

                _hero.AddChainColor(m.Enemy.Color);
            });

            MessageBus.Instance.Subscribe<TogglePauseMessage>((m) =>
            {
                TogglePause();
            });

            MessageBus.Instance.Subscribe<DestroyAllEnemyMessage>((m) =>
            {
                Misc.Log($"Destroy All Enemy !");
                foreach (var enemy in GroupOf<Enemy>())
                {
                    if (enemy._isActive)
                    {
                        enemy.DestroyMe();
                    }
                }
            });

        }
        public void TogglePause()
        {
            _isPaused = !_isPaused;
        }
        public override Node Update(GameTime gameTime)
        {
            UpdateRect();

            if (ButtonControl.OnePress("Pause", G.Key.IsKeyDown(Keys.P) || GamePad.GetState(PlayerIndex.One).Buttons.Start == ButtonState.Pressed))
            {
                TogglePause();
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
            ScreenManager.BeginScissor(batch, AbsRect, indexLayer);


            if (indexLayer == (int)Layers.Back)
            {
            }

            if (indexLayer == (int)Layers.Main)
            {
                batch.FillRectangle(AbsRectF, HSV.Adjust(Color.Gray, valueMultiplier: .25f) * 1f);

                _starManager.DrawStars(batch, AbsXY);
                batch.Grid(_gridPos + AbsXY, AbsRectF.Width, AbsRectF.Height + _cellSize * 2, _cellSize, _cellSize, Color.WhiteSmoke * .1f, 1f);

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


            ScreenManager.EndScissor(batch, indexLayer);


            if (indexLayer == (int)Layers.UI)
            {
                //batch.CenterStringXY(G.FontMain, $"Chain : {3}", AbsRectF.TopCenter - Vector2.UnitY * 10, Color.White);

                DrawChainColor(batch, AbsRectF.TopCenter - Vector2.UnitY * 32, _hero.SlotSize, 64, 48);

                //batch.Point(AbsRectF.TopCenter, 8, Color.White);

                // Debug Object Pool
                //for (int i = 0; i < G.PoolBullet.GetAllObjects().Count(); i++)
                //{
                //    var bullet = G.PoolBullet.GetAllObjects().ElementAt(i);
                //    batch.LeftMiddleString(G.FontMain, $"{i} : {bullet._index} : {bullet._isActive}", Vector2.One * 20 + Vector2.UnitY * i * 18, Color.White);
                //}
                //for (int i = 0; i < G.PoolEnemy.GetAllObjects().Count(); i++)
                //{
                //    var enemy = G.PoolEnemy.GetAllObjects().ElementAt(i);
                //    batch.LeftMiddleString(G.FontMain, $"{i} : {enemy._index} : {enemy._isActive} ", Vector2.One * 20 + Vector2.UnitY * i * 18 + Vector2.UnitX * 180, Color.White);
                //}
            }



            return base.Draw(batch, gameTime, indexLayer);
        }
        private void DrawChainColor(SpriteBatch batch, Vector2 position, int nbColor = 5, float space = 40, float size = 32)
        {
            Vector2 pos = position - new Vector2((space * (nbColor-1)) / 2, 0);
            for (int i = 0; i < nbColor; i++)
            {
                batch.FillRectangleCentered(pos + Vector2.UnitX * i * space, Vector2.One * size, Color.Black * .75f, 0f);
                batch.RectangleCentered(pos + Vector2.UnitX * i * space, Vector2.One * size, Color.Gray, 1f);
                batch.RectangleTargetCentered(pos + Vector2.UnitX * i * space, Vector2.One * size, Color.White, size/3, size/3, 3f);
            }

            for (int i = 0; i < _hero.ChainColors.Count; i++)
            {
                Color color = _hero.ChainColors[i];
                batch.FillRectangleCentered(pos + Vector2.UnitX * i * space, Vector2.One * (size - 16), color * 1f, 0f);
            }
        }
    }
}
