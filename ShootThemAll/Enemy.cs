using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mugen.Animation;
using Mugen.Core;
using Mugen.Event;
using Mugen.Event.Message;
using Mugen.GFX;
using Mugen.Physics;
using System;
using System.Collections.Generic;

namespace ShootThemAll
{
    public class DamageMessage : IMessage
    {
        public int Damage { get; set; }
        public DamageMessage(int damage)
        {
            Damage = damage;
        }
    }
    public class  EnemyDestroyedMessage : IMessage
    {
        public Enemy Enemy { get; set; }
        public EnemyDestroyedMessage(Enemy enemy)
        {
            Enemy = enemy;
        }
    }
    public class EnemyMagnetMessage : IMessage
    {
        public Enemy Enemy { get; set; }
        public EnemyMagnetMessage(Enemy enemy)
        {
            Enemy = enemy;
        }
    }

    public class Enemy : Node
    {
        public static List<Color> Colors =
        [
            new Color(150, 10, 10),
            new Color(10, 150, 10),
            new Color(10, 10, 150),
            new Color(150, 150, 10),
            new Color(10, 150, 150),
            new Color(150, 10, 150),
        ];
        public static Color RandomColor()
        {
            return Colors[Misc.Rng.Next(0, Colors.Count)];
        }
        public enum States
        {
            Idle,
            GetDamage,
            Shoot,
            MagnetHero,
            FollowHero,
            MagnetEnemy,
            FollowEnemy,
        }
        public States CurState => _state.CurState;
        State<States> _state = new State<States>(States.Idle);

        public enum Timers
        {
            DamageTime,
            ShootDelay,
            HasShoot,
        }
        Timer<Timers> _timer = new Timer<Timers>();

        public const int ZoneBody = 0;

        public Shake Shake = new Shake();

        int _maxEnergy;
        int _energy;
        EasingValue _easeEnergy;

        float _speed;
        float _size;

        float _ticWave;
        float _wave;
        Node _target;
        Node _magnet;

        public Color Color => _color;
        Color _color;

        Animate2D _animate2D;
        public Enemy Set(Node target, Color color, float speed)
        {
            _target = target;
            _color = color;
            _speed = speed;

            return this;
        }
        public override Node Init()
        {
            _maxEnergy = 30;
            _energy = 30;
            _speed = 2f;
            _size = 1f;
            _ticWave = 0f;
            _wave = 0f;
            _easeEnergy.SetValue(_energy);
            _target = null;
            _magnet = null;

            _state.Set(States.Idle);

            return base.Init();
        }
        public Enemy(Node target, Color color, float speed)
        {
            _type = UID.Get<Enemy>();
            _easeEnergy = new EasingValue(_energy);

            Init();
            Set(target, color, speed);

            

            SetSize(48, 48);
            SetPivot(Position.CENTER);
            SetCollideZone(ZoneBody, _rect);

            _timer.Set(Timers.DamageTime, Timer.Time(0, 0, 0.1f));
            _timer.Set(Timers.ShootDelay, Timer.Time(0, 0, 5f));
            _timer.Set(Timers.HasShoot, Timer.Time(0, 0, .25f));

            _timer.Start(Timers.ShootDelay);

            _timer.On(Timers.ShootDelay, () =>
            {
                //Console.WriteLine("Shoooot");
                // Debug
                //if (_target == null)
                //{
                //    _state.Change(States.Idle);
                    
                //    MessageBus.Instance.SendMessage(new TogglePauseMessage());
                //    return;
                //}

                Shoot(_target.XY);
                _state.Change(States.Shoot);

                float time = Misc.Rng.Next(30, 50) / 10f;
                _timer.Set(Timers.ShootDelay, Timer.Time(0, 0, time), true);
                _timer.Start(Timers.ShootDelay);

            });

            _timer.On(Timers.HasShoot, () =>
            {
                _state.Change(States.Idle);
                _timer.Stop(Timers.HasShoot);
            });

            _timer.On(Timers.DamageTime, () =>
            {
                _state.Change(States.Idle);
                _timer.Stop(Timers.DamageTime);
            });


            _state.On(States.Idle, () =>
            {
                // Active le tir
                _timer.Start(Timers.ShootDelay);
            });
            _state.On(States.GetDamage, () =>
            {
                // Active le tir
                _timer.Start(Timers.DamageTime);
            });
            _state.Off(States.GetDamage, () =>
            {
                // Active le tir
                _timer.Start(Timers.ShootDelay);
            });

            _state.On(States.FollowHero, () =>
            {
                // Désactive le tir
                _timer.Stop(Timers.ShootDelay);
            });
            _state.On(States.FollowEnemy, () =>
            {
                // Désactive le tir
                _timer.Stop(Timers.ShootDelay);
            });

            _state.On(States.Shoot, () =>
            {
                _timer.Start(Timers.HasShoot);
            });

            _animate2D = new Animate2D();
            _animate2D.Add("Magnet");
            //_animate2D.Add("MagnetEnemy");

        }
        public void AddEnergy(int energy)
        {
            _energy += energy;
            _easeEnergy.SetValue(_energy);

            _energy = Math.Clamp(_energy, 0, 100);
        }
        private void ChainAddEnergy(int energy)
        {
            AddEnergy(energy);
            if (_magnet == null) return;
            if (_magnet._type == UID.Get<Enemy>())
            {
                Enemy enemy = (Enemy)_magnet;
                enemy.ChainAddEnergy(energy);
            }
            
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
                        // FX
                        Vector2 impact = new Vector2(bullet._x, _y + _oY);
                        new PopInfo(bullet.Power.ToString(), Color.Yellow, Color.Red).AppendTo(_parent).SetPosition(impact);

                        new FxExplose(Particles.Shapes.Line, impact + _parent.XY,HSV.Adjust(_color, valueMultiplier: 1.5f), 10, 20, 40).AppendTo(_parent);
                        
                        bullet.DestroyMe();
                        Shake.SetIntensity(4f, .5f);

                        if (!_state.Is(States.FollowEnemy))
                        {
                            _state.Set(States.GetDamage);
                            AddEnergy(-bullet.Power);
                        }
                        else
                        {
                            ChainAddEnergy(-bullet.Power);
                        }

                        //G.SoundHit.Play(0.1f * G.Volume, .5f, 0f);
                        G.SoundEffectManager.Play(G.SoundHit, 0.1f * G.Volume, .5f, 0f);
                    }
                }
            }
        }
        public void Shoot(Vector2 target)
        {
            float angle = ((float)Misc.Rng.NextDouble() - 0.5f) / 20f;

            //angle += Geo.RAD_90;
            angle += Geo.GetRadian(XY, target);

            G.PoolBullet.Get().Set(this, XY + Vector2.UnitY * _oY, angle, 3, Color.OrangeRed, 600).AppendTo(_parent);
        }
        private void FallMove(float speed, GameTime gameTime)
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
        public void MagnetHero(Hero hero)
        {
            _magnet = hero;
            _animate2D.SetMotion("Magnet", Easing.QuadraticEaseOut, XY, hero._rect.Center - Vector2.UnitY * (_oY + _magnet._oY), 16);
            _animate2D.Start("Magnet");
            _state.Change(States.MagnetHero);
        }
        public void MagnetEnemy(Enemy enemy)
        {
            _magnet = enemy;
            _animate2D.SetMotion("Magnet", Easing.QuadraticEaseOut, XY, enemy._rect.Center + Vector2.UnitY * (_oY + _magnet._oY), 16);
            _animate2D.Start("Magnet");
            _state.Change(States.MagnetEnemy);
        }
        private void RunState(GameTime gameTime)
        {
            switch (_state.CurState)
            {
                case States.Idle:
                    HandleCollision();
                    FallMove(_speed, gameTime);
                    break;

                case States.GetDamage:
                    HandleCollision();
                    FallMove(_speed / 2, gameTime);

                    break;

                case States.Shoot:
                    HandleCollision();
                    //Move(_speed);
                    break;

                case States.MagnetHero:

                    _x = _animate2D.Value("Magnet").X;
                    _y = _animate2D.Value("Magnet").Y;

                    if (_animate2D.OnFinish("Magnet"))
                    {
                        MessageBus.Instance.SendMessage(new EnemyMagnetMessage(this));
                        _state.Change(States.FollowHero);
                    }

                    break;

                case States.FollowHero:

                    var pos = _magnet._rect.Center - Vector2.UnitY * (_oY + _magnet._oY);

                    _x = pos.X;
                    _y = pos.Y;

                    break;
                case States.MagnetEnemy:

                    _x = _animate2D.Value("Magnet").X;
                    _y = _animate2D.Value("Magnet").Y;

                    if (_animate2D.OnFinish("Magnet"))
                    {
                        _state.Change(States.FollowEnemy);
                    }
                    break;
                case States.FollowEnemy:

                    pos = _magnet._rect.Center + Vector2.UnitY * (_oY + _magnet._oY);

                    _x = pos.X;
                    _y = pos.Y;

                    HandleCollision();

                    break;
            }
        }
        public void DestroyMe()
        {
            //G.SoundExplose.Play(0.1f * G.Volume, 1f, 0f);
            G.SoundEffectManager.Play(G.SoundExplose, 0.1f * G.Volume, 1f, 0f);

            Color color = HSV.Adjust(_color, valueMultiplier: 1.5f);

            new FxExplose(Particles.Shapes.Square, AbsXY, color, 20, 100, 50, 10, .92f).AppendTo(_parent);
            new FxGlow(XY, color, .1f).AppendTo(_parent);

            MessageBus.Instance.SendMessage(new EnemyDestroyedMessage(this));

            //KillMe();
            G.PoolEnemy.Return(this, _parent);

        }
        public override Node Update(GameTime gameTime)
        {
            _timer.Update();
            _easeEnergy.Update(gameTime);
            _animate2D.Update();

            RunState(gameTime);

            UpdateRect();

            if (_energy <= 0)
            {
                //Console.WriteLine("DestroyMe");
                DestroyMe();
            }

            return base.Update(gameTime);
        }

        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            var pos = AbsXY + Shake.GetVector2();
            
            if (indexLayer == (int)Layers.Main)
            {

                batch.FillRectangleCentered(pos, AbsRectF.GetSize() * _size, _state.Is(States.GetDamage) ? HSV.Adjust(_color, valueMultiplier: 1.5f) : _color, 0);
                batch.RectangleCentered(pos, AbsRectF.GetSize() * _size, _state.Is(States.GetDamage)? Color.White:Color.Gray, 3f);

            }

            if (indexLayer == (int)Layers.Front)
            {
                //batch.CenterStringXY(G.FontMain, "Enemy", AbsXY, Color.White);
                batch.CenterStringXY(G.FontMain, $"{_state.CurState}", AbsRectF.TopCenter, Color.Cyan);
                batch.CenterStringXY(G.FontMain, $"{_easeEnergy.Value}", AbsRectF.BottomCenter, Color.Yellow);

                pos = AbsRectF.TopCenter - Vector2.UnitY * 10 - Vector2.UnitX * (_maxEnergy / 2) + Shake.GetVector2() * .5f;
                G.DrawEnergyBar(batch, pos, _easeEnergy.Value, _maxEnergy, _alpha, 1f, 10f);

                //batch.Rectangle(_rect, Color.Red, 2f);
                //batch.Rectangle(_collideZone[ZoneBody].GetRect(), Color.Red, 2f);
            }

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
