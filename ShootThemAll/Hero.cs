using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mugen.Core;
using Mugen.Event;
using Mugen.GFX;
using Mugen.Physics;
using System;

namespace ShootThemAll
{
    public class Hero : Node
    {
        public enum States
        {
            Idle,
            Move,
            Shoot,
        }
        public States CurState => _state.CurState;
        State<States> _state = new State<States>(States.Idle);
        public enum Timers
        {
            Shoot,
            Move,
        }
        Timer<Timers> _timer = new Timer<Timers>();

        PlayerIndex _playerIndex;
        Vector2 _stickLeft;
        Vector2 _stickRight;

        GamePadState _gamePadState;
        public Hero(PlayerIndex playerIndex)
        {
            _type = UID.Get<Hero>();

            _playerIndex = playerIndex;
            SetSize(64, 64);
            SetPivot(Position.CENTER);

            _timer.Set(Timers.Shoot, Timer.Time(0, 0, 0.1f), true);
            _timer.Start(Timers.Shoot);
        }
        private void HandleInput()
        {
            _gamePadState = GamePad.GetState(_playerIndex);
            _stickLeft = _gamePadState.ThumbSticks.Left;
            _stickRight = _gamePadState.ThumbSticks.Right;

            if (_playerIndex == PlayerIndex.One)
            {
                if (G.Key.IsKeyDown(Keys.Up)) _stickLeft.Y = 1;
                if (G.Key.IsKeyDown(Keys.Down)) _stickLeft.Y = -1;
                if (G.Key.IsKeyDown(Keys.Left)) _stickLeft.X = -1;
                if (G.Key.IsKeyDown(Keys.Right)) _stickLeft.X = 1;

                if (Math.Abs(_stickLeft.X) > 0 && Math.Abs(_stickLeft.Y) > 0)
                {
                    _stickLeft.Normalize();
                }

                if (G.Key.IsKeyDown(Keys.LeftControl)) Shoot();
            }

            if (_gamePadState.Buttons.A == ButtonState.Pressed)
            {
                //Console.WriteLine("Shoot");
                Shoot();
            }
        }
        public void Shoot()
        {
            if (_timer.On(Timers.Shoot))
            {
                float angle = ((float)Misc.Rng.NextDouble() - 0.5f) / 20f;
                angle += Geo.RAD_0;

                Bullet bullet = new Bullet(XY, angle, 24);
                bullet.AppendTo(_parent);
            }
        }
        public override Node Update(GameTime gameTime)
        {
            _timer.Update();
            UpdateRect();

            HandleInput();

            _x += _stickLeft.X * 10f;
            _y += -_stickLeft.Y * 10f;

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Layers.Main)
            {
                batch.FillRectangle(AbsRectF, Color.Red);
                batch.RectangleCentered(AbsXY, AbsRectF.GetSize(), Color.Gold, 5f);

                batch.CenterStringXY(G.FontMain, "Hero", AbsXY, Color.White);
                //batch.CenterStringXY(G.FontMain, $"{_stickLeft}", AbsRectF.TopCenter, Color.White);

            }

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
