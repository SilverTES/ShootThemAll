using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mugen.Core;
using Mugen.Event;
using Mugen.GFX;
using Mugen.Physics;
using System;

namespace ShootThemAll
{
    public class Enemy : Node
    {
        public enum States
        {
            Idle,
            Hit,
            Shoot,
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

        int _energy = 6;
        float _speed = 2f;


        float _ticWave = 0f;
        float _wave = 0f;
        public Enemy(float speed)
        {
            _type = UID.Get<Enemy>();
            _speed = speed;
            SetSize(64, 64);
            SetPivot(Position.CENTER);

            SetCollideZone(ZoneBody, _rect);

            _timer.Set(Timers.Hit, Timer.Time(0, 0, 0.1f));
            _timer.Set(Timers.Shoot, Timer.Time(0, 0, 1.5f));
            _timer.Set(Timers.HasShoot, Timer.Time(0, 0, .25f));

            _timer.Start(Timers.Shoot);

            _timer.On(Timers.Shoot, () => 
            {
                //Console.WriteLine("Shoooot");
                Shoot();
                _state.Change(States.Shoot);

                float time = Misc.Rng.Next(10, 30) / 10f;
                _timer.Set(Timers.Shoot, Timer.Time(0, 0, time), true);
            });

            _timer.On(Timers.HasShoot, () =>
            {
                _state.Change(States.Idle);
                _timer.Stop(Timers.HasShoot);
            });


            _state.On(States.Shoot, () =>
            {
                _timer.Start(Timers.HasShoot);
                Shake.SetIntensity(8f, 1f, false);
            });

        }
        public void AddEnergy(int energy)
        {
            _energy += energy;
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
                    if (bullet.Owner != this)
                    {
                        AddEnergy(-bullet.Power);

                        Vector2 impact = new Vector2(bullet._x, _y + _oY);

                        new PopInfo(bullet.Power.ToString(), Color.Yellow, Color.Red).AppendTo(_parent).SetPosition(impact);
                        new FxExplose(impact + _parent.XY, Color.Blue, 10, 20, 40).AppendTo(_parent);
                        bullet.KillMe();

                        Shake.SetIntensity(8f, 1f);

                        _state.Set(States.Hit);
                        _timer.Start(Timers.Hit);

                        G.SoundHit.Play(0.1f * G.Volume, .5f, 0f);
                    }
                }
            }
        }
        public void Shoot()
        {
            float angle = ((float)Misc.Rng.NextDouble() - 0.5f) / 20f;
            angle += Geo.RAD_90;

            Bullet bullet = new Bullet(this, XY, angle, 6, Color.OrangeRed, 240);
            bullet.AppendTo(_parent);
        }
        private void Move(float speed)
        {
            // Move logic here
            _ticWave += 0.1f;
            _wave = (float)Math.Sin(_ticWave) * 2f;

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
                    // Handle idle state
                    HandleCollision();
                    Move(_speed);
                    break;
                case States.Hit:
                    // Handle hit state
                    Move(_speed/2);
                    if (_energy <= 0)
                    {
                        DestroyMe();
                    }

                    if (_timer.On(Timers.Hit))
                    {
                        _state.Set(States.Idle);
                        _timer.Stop(Timers.Hit);
                    }

                    break;
                case States.Shoot:
                    // Handle shoot state
                    break;
            }
        }
        public void DestroyMe()
        {
            G.SoundExplose.Play(0.1f * G.Volume, 1f, 0f);

            new FxExplose(AbsXY, Color.Red, 20, 100, 50).AppendTo(_parent);
            new FxExplose(AbsXY, Color.Gold, 10, 100, 50).AppendTo(_parent);

            new FxGlow(XY, Color.White, .1f).AppendTo(_parent);
            KillMe();
        }
        public override Node Update(GameTime gameTime)
        {
            _timer.Update();
            UpdateRect();

            RunState();


            return base.Update(gameTime);
        }

        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Layers.Main)
            {
                var pos = AbsXY + Shake.GetVector2();

                if (_state.Is(States.Idle))
                    batch.FillRectangleCentered(pos, AbsRectF.GetSize(), Color.Green, 0);
                else if (_state.Is(States.Hit))
                    batch.FillRectangleCentered(pos, AbsRectF.GetSize(), Color.DarkGreen, 0);
                else if (_state.Is(States.Shoot))
                    batch.FillRectangleCentered(pos, AbsRectF.GetSize(), Color.ForestGreen, 0);
                
                batch.RectangleCentered(pos, AbsRectF.GetSize(), _state.Is(States.Hit)? Color.White:Color.Gray, 3f);

                //batch.CenterStringXY(G.FontMain, "Enemy", AbsXY, Color.White);
                batch.CenterStringXY(G.FontMain, $"{_state.CurState}", AbsRectF.TopCenter, Color.Cyan);
                batch.CenterStringXY(G.FontMain, $"{_energy}", AbsRectF.BottomCenter, Color.Yellow);
            }

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
