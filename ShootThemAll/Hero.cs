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

        public const int ZoneBody = 0;

        private int _energy = 16;

        PlayerIndex _playerIndex;
        Vector2 _accMove = new Vector2(); // Acceleration/Deceleration du mouvement du joueur si il utilise le clavier
        Vector2 _stickLeft;
        Vector2 _stickRight;

        GamePadState _gamePadState;
        public Hero(PlayerIndex playerIndex)
        {
            _type = UID.Get<Hero>();

            _playerIndex = playerIndex;
            SetSize(48, 48);
            SetPivot(Position.CENTER);

            SetCollideZone(ZoneBody, _rect);

            _timer.Set(Timers.Shoot, Timer.Time(0, 0, 0.4f), true);
            _timer.Start(Timers.Shoot);
        }
        private void HandleInput()
        {
            _gamePadState = GamePad.GetState(_playerIndex);
            _stickLeft = _gamePadState.ThumbSticks.Left;
            _stickRight = _gamePadState.ThumbSticks.Right;

            if (_playerIndex == PlayerIndex.One && _stickLeft.Equals(Vector2.Zero))
            {
                if (!G.Key.IsKeyDown(Keys.Up) && _accMove.Y > 0) _accMove.Y = 0f;
                if (!G.Key.IsKeyDown(Keys.Down) && _accMove.Y < 0) _accMove.Y = 0f;

                if (!G.Key.IsKeyDown(Keys.Left) && _accMove.X < 0) _accMove.X = 0f;
                if (!G.Key.IsKeyDown(Keys.Right) && _accMove.X > 0) _accMove.X = 0f;

                if (G.Key.IsKeyDown(Keys.Up)) _accMove.Y += .1f;
                if (G.Key.IsKeyDown(Keys.Down)) _accMove.Y += -.1f;
                if (G.Key.IsKeyDown(Keys.Left)) _accMove.X += -.1f;
                if (G.Key.IsKeyDown(Keys.Right)) _accMove.X += .1f;

                _stickLeft.X = _accMove.X;
                _stickLeft.Y = _accMove.Y;

                _stickLeft.X = float.Clamp(_stickLeft.X, -1, 1);
                _stickLeft.Y = float.Clamp(_stickLeft.Y, -1, 1);

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
                angle += -Geo.RAD_90;

                Bullet bullet = new Bullet(this, XY - Vector2.UnitY * _oY, angle, 24, Color.BlueViolet);
                bullet.AppendTo(_parent);

                new FxGlow(XY - Vector2.UnitY * _oY, Color.White, .025f, 40).AppendTo(_parent);

                G.SoundBim.Play(0.025f * G.Volume, 1f, 0f);
            }
        }
        private void HandleCollision()
        {
            UpdateCollideZone(ZoneBody, _rect);

            var collider = Collision2D.OnCollideZoneByNodeType(GetCollideZone(ZoneBody), UID.Get<Enemy>(), Enemy.ZoneBody);


            if (collider != null)
            {
                var enemy = collider._node as Enemy;
                if (enemy != null)
                {
                    enemy.DestroyMe();
                    //Console.WriteLine($"Hit {bullet.Power}");
                    //AddEnergy(bullet.Power);
                    //bullet.KillMe();
                }
            }

        }
        public override Node Update(GameTime gameTime)
        {
            _timer.Update();
            UpdateRect();

            HandleInput();
            HandleCollision();

            _x += _stickLeft.X * 10f;
            _y += -_stickLeft.Y * 10f;

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Layers.Main)
            {
                batch.FillRectangle(AbsRectF, Color.Red);
                batch.RectangleCentered(AbsXY, AbsRectF.GetSize(), Color.Gray * .75f, 5f);

                //batch.CenterStringXY(G.FontMain, "Hero", AbsXY, Color.White);
                //batch.CenterStringXY(G.FontMain, $"{_stickLeft}", AbsRectF.TopCenter, Color.White);
                batch.CenterStringXY(G.FontMain, $"{_energy}", AbsRectF.BottomCenter, Color.Orange);

            }

            if (indexLayer == (int)Layers.FrontFX)
            {
                //batch.LineTexture(G.TexLine, AbsXY, new Vector2(AbsX, 0), 5f, Color.Red * .5f);
            }

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
