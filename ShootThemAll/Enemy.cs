using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mugen.Core;
using Mugen.Event;
using Mugen.GFX;
using Mugen.Physics;
using System;
using System.Collections.Generic;

namespace ShootThemAll
{
    public class Enemy : Node
    {
        public static List<Color> Colors =
        [
            new Color(128, 0, 0),
            new Color(0, 128, 0),
            new Color(0, 0, 128),
            new Color(128, 128, 0),
            new Color(0, 128, 128),
            new Color(128, 0, 128),
        ];
        public static Color RandomColor()
        {
            return Colors[Misc.Rng.Next(0, Colors.Count)];
        }
        public enum States
        {
            Idle,
            Hit,
            Shoot,
            Dead,
        }
        public States CurState => _state.CurState;
        State<States> _state = new State<States>(States.Idle);

        public enum Timers
        {
            Hit,
            Shoot,
            HasShoot,
        }
        Timer<Timers> _timer = new Timer<Timers>();

        public const int ZoneBody = 0;

        public Shake Shake = new Shake();

        int _maxEnergy = 30;
        int _energy = 30;
        EasingValue _easeEnergy;

        float _speed = 2f;
        float _size = 1f;

        float _ticWave = 0f;
        float _wave = 0f;
        Node _target;

        Color _color;

        public Enemy(float speed, Node target, Color color)
        {
            _type = UID.Get<Enemy>();
            _speed = speed;
            _target = target;
            _color = color;

            _easeEnergy = new EasingValue(_energy);

            SetSize(64, 64);
            SetPivot(Position.CENTER);

            SetCollideZone(ZoneBody, _rect);

            _timer.Set(Timers.Hit, Timer.Time(0, 0, 0.1f));
            _timer.Set(Timers.Shoot, Timer.Time(0, 0, 1.5f));
            _timer.Set(Timers.HasShoot, Timer.Time(0, 0, .25f));

            //_timer.Start(Timers.Shoot);

            _timer.On(Timers.Shoot, () => 
            {
                //Console.WriteLine("Shoooot");
                Shoot(_target.XY);
                _state.Change(States.Shoot);

                float time = Misc.Rng.Next(10, 30) / 10f;
                _timer.Set(Timers.Shoot, Timer.Time(0, 0, time), true);
            });

            _timer.On(Timers.HasShoot, () =>
            {
                _state.Change(States.Idle);
                _timer.Stop(Timers.HasShoot);
            });

            _timer.On(Timers.Hit, () =>
            {
                _state.Change(States.Idle);
                _timer.Stop(Timers.Hit);
            });


            _state.On(States.Shoot, () =>
            {
                _timer.Start(Timers.HasShoot);
                //Shake.SetIntensity(8f, 1f, false);
            });

        }
        public void AddEnergy(int energy)
        {
            _energy += energy;
            _easeEnergy.SetValue(_energy);

            _energy = Math.Clamp(_energy, 0, 100);
        }
        private void HandleCollision()
        {
            UpdateCollideZone(ZoneBody, _rect);

            var collider = Collision2D.OnCollideZoneByNodeType(GetCollideZone(ZoneBody), UID.Get<Bullet>(), Bullet.ZoneBody);

            if (collider != null)
            {
                Bullet bullet = (Bullet)collider._node;
                if (bullet != null)
                {
                    if (bullet.Owner._type == UID.Get<Hero>())
                    {
                        AddEnergy(-bullet.Power);

                        Vector2 impact = new Vector2(bullet._x, _y + _oY);

                        new PopInfo(bullet.Power.ToString(), Color.Yellow, Color.Red).AppendTo(_parent).SetPosition(impact);
                        new FxExplose(impact + _parent.XY, Color.LightCyan, 10, 20, 40).AppendTo(_parent);
                        bullet.KillMe();

                        Shake.SetIntensity(4f, .5f);

                        _state.Set(States.Hit);
                        _timer.Start(Timers.Hit);

                        G.SoundHit.Play(0.1f * G.Volume, .5f, 0f);
                    }
                }
            }
        }
        public void Shoot(Vector2 target)
        {
            float angle = ((float)Misc.Rng.NextDouble() - 0.5f) / 20f;

            //angle += Geo.RAD_90;
            angle += Geo.GetRadian(XY, target);

            Bullet bullet = new Bullet(this, XY, angle, 6, Color.OrangeRed, 240);
            bullet.AppendTo(_parent);
        }
        private void Move(float speed)
        {
            // Move logic here
            _ticWave += 0.1f;
            _wave = (float)Math.Sin(_ticWave) * .5f;

            _x += _wave;
            _y += speed;
            if (_y > Screen.Height)
            {
                _y = 0;
            }
        }
        private void RunState()
        {
            switch (_state.CurState)
            {
                case States.Idle:
                    HandleCollision();
                    Move(_speed);
                    break;

                case States.Hit:
                    //HandleCollision();
                    Move(_speed / 2);

                    break;

                case States.Shoot:
                    //HandleCollision();
                    //Move(_speed);
                    break;

                case States.Dead:

                    _size -= 0.05f;
                    if (_size <= 0)
                    {
                        _size = 0;

                        G.SoundExplose.Play(0.1f * G.Volume, 1f, 0f);
                        new FxExplose(AbsXY, Color.GreenYellow, 20, 100, 50, 10, .92f).AppendTo(_parent);
                        new FxGlow(XY, Color.White, .1f).AppendTo(_parent);
                        KillMe();
                    }
                    break;
            }
        }
        public void DestroyMe()
        {
            _state.Change(States.Dead);
        }
        public override Node Update(GameTime gameTime)
        {
            UpdateRect();
            _timer.Update();
            _easeEnergy.Update(gameTime);
            RunState();

            if (_energy <= 0 && !_state.Is(States.Dead))
            {
                //Console.WriteLine("DestroyMe");
                _state.Change(States.Dead);
            }

            return base.Update(gameTime);
        }

        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Layers.Main)
            {
                var pos = AbsXY + Shake.GetVector2();

                if (_state.Is(States.Idle))
                    batch.FillRectangleCentered(pos, AbsRectF.GetSize() * _size, _color, 0);
                else if (_state.Is(States.Hit))
                    batch.FillRectangleCentered(pos, AbsRectF.GetSize() * _size, HSV.Adjust(_color, valueMultiplier : 2.0f), 0);
                else if (_state.Is(States.Shoot))
                    batch.FillRectangleCentered(pos, AbsRectF.GetSize() * _size, HSV.Adjust(_color, valueMultiplier: 1.5f), 0);
                else if (_state.Is(States.Dead))
                    batch.FillRectangleCentered(pos, AbsRectF.GetSize() * _size, HSV.Adjust(_color, valueMultiplier: 1.2f), 0);

                batch.RectangleCentered(pos, AbsRectF.GetSize() * _size, _state.Is(States.Hit)? Color.White:Color.Gray, 3f);

                //batch.CenterStringXY(G.FontMain, "Enemy", AbsXY, Color.White);
                batch.CenterStringXY(G.FontMain, $"{_state.CurState}", AbsRectF.TopCenter, Color.Cyan);
                batch.CenterStringXY(G.FontMain, $"{_easeEnergy.Value}", AbsRectF.BottomCenter, Color.Yellow);

                pos = AbsRectF.TopCenter - Vector2.UnitY * 10 - Vector2.UnitX * (_maxEnergy / 2) + Shake.GetVector2() * .5f;
                G.DrawEnergyBar(batch, pos, _easeEnergy.Value, _maxEnergy, _alpha, 1f, 10f);
            }

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
