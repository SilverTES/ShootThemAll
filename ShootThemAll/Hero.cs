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

        private int _maxEnergy = 40;
        private int _energy = 40;

        private float _fireSpeed = 0.4f;   

        PlayerIndex _playerIndex;
        Vector2 _accMove = new Vector2(); // Acceleration/Deceleration du mouvement du joueur si il utilise le clavier
        Vector2 _stickLeft;
        Vector2 _stickRight;

        GamePadState _gamePadState;

        Shake Shake = new Shake();

        public Hero(PlayerIndex playerIndex)
        {
            _type = UID.Get<Hero>();

            _playerIndex = playerIndex;
            SetSize(48, 48);
            SetPivot(Position.CENTER);

            SetCollideZone(ZoneBody, _rect);

            _timer.Set(Timers.Shoot, Timer.Time(0, 0, _fireSpeed), true);
            _timer.Start(Timers.Shoot);
        }
        public void SetFireSpeed(float fireSpeed)
        {
            _fireSpeed = fireSpeed;

            _fireSpeed = Math.Clamp(_fireSpeed, 0.05f, 2f); // Clamp entre 0.1 et 2 secondes

            Console.WriteLine($"_fireSpeed = {_fireSpeed}");
            _timer.Set(Timers.Shoot, Timer.Time(0, 0, _fireSpeed), true);
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

                if (G.Key.IsKeyDown(Keys.LeftControl)) 
                    Shoot();
            }

            if (_gamePadState.Buttons.A == ButtonState.Pressed)
            {
                Shoot();
            }

        }
        public void Shoot()
        {
            if (_timer.On(Timers.Shoot))
            {
                float angle = ((float)Misc.Rng.NextDouble() - 0.5f) / 20f;
                angle += -Geo.RAD_90;

                Bullet bullet = new Bullet(this, XY - Vector2.UnitY * _oY, angle, 24, Color.BlueViolet, 100, 10);
                bullet.AppendTo(_parent);

                new FxGlow(XY - Vector2.UnitY * _oY, Color.BlueViolet, .025f, 40).AppendTo(_parent);

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
                    
                }
            }

            collider = Collision2D.OnCollideZoneByNodeType(GetCollideZone(ZoneBody), UID.Get<Bonus>(), Bonus.ZoneBody);
            if (collider != null)
            {
                var bonus = collider._node as Bonus;
                if (bonus != null)
                {
                    bonus.DestroyMe();
                    SetFireSpeed(_fireSpeed * (1f - 1f/10)); // 10% de la vitesse
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
                //batch.CenterStringXY(G.FontMain, $"{_fireSpeed:F3}", AbsRectF.TopCenter, Color.White);
                batch.CenterStringXY(G.FontMain, $"{_energy}", AbsRectF.BottomCenter, Color.Orange);



                //Color fg = Color.GreenYellow;
                //Color bg = Color.Green;

                //if (_energy <= 10)
                //{
                //    fg = Color.Yellow;
                //    bg = Color.Red;
                //}

                //GFX.Bar(batch, pos, _maxEnergy, 8, Color.Red * _alpha);
                //GFX.Bar(batch, pos, _energy, 8, fg * _alpha);
                //GFX.BarLines(batch, pos, _maxEnergy, 8, Color.Black * _alpha, 2);
                //GFX.Bar(batch, pos - Vector2.UnitY * 2f, _maxEnergy, 2, Color.White * .5f * _alpha);

                Vector2 pos = AbsRectF.TopCenter - Vector2.UnitY * 10 - Vector2.UnitX * (_maxEnergy / 2) + Shake.GetVector2() * .5f;
                G.DrawEnergyBar(batch, pos, _energy, _maxEnergy, _alpha,  1f, 10f);

                //batch.CenterBorderedStringXY(G.FontMain, $"{_energy}", AbsRectF.TopLeft - Vector2.One * 10 + Shake.GetVector2() * .5f, fg * _alpha, bg * _alpha);

            }


            if (indexLayer == (int)Layers.FrontFX)
            {
                //batch.LineTexture(G.TexLine, AbsXY, new Vector2(AbsX, 0), 5f, Color.Red * .5f);

                batch.FilledCircle(G.TexCircle, AbsXY, 10, Color.Gold * _alpha);
            }

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
