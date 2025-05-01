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
            Move,
            Shoot,
        }
        public States CurState => _state.CurState;
        State<States> _state = new State<States>(States.Idle);

        public enum Timers
        {
            Hit,
            Move,
        }
        Timer<Timers> _timer = new Timer<Timers>();

        public const int ZoneBody = 0;

        public Shake Shake = new Shake();

        int _energy = 32;
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
            
        }
        public void AddEnergy(int energy)
        {
            _energy += energy;
            _energy = Math.Clamp(_energy, 0, 100);
        }
        private void HandleCollision()
        {
            UpdateCollideZone(ZoneBody, _rect);

            var colliders = Collision2D.ListCollideZoneByNodeType(GetCollideZone(ZoneBody), UID.Get<Bullet>(), Bullet.ZoneBody);

            if (colliders.Count > 0)
            {
                foreach (var collider in colliders)
                {
                    Bullet bullet = (Bullet)collider._node;
                    if (bullet != null)
                    {
                        // Handle collision with the bullet
                        //Console.WriteLine("Enemy hit by bullet!");

                        AddEnergy(-bullet.Power);

                        Vector2 impact = new Vector2(AbsXY.X - _oX, bullet.AbsXY.Y);

                        new PopInfo(bullet.Power.ToString(), Color.Yellow, Color.Red).AppendTo(_parent).SetPosition(impact);
                        new FxExplose(impact, Color.Yellow, 10, 20, 40).AppendTo(_parent);
                        bullet.KillMe();

                        Shake.SetIntensity(8f, 1f);

                        _state.Set(States.Hit);
                        _timer.Start(Timers.Hit);

                    }
                }
            }
        }
        private void Move(float speed)
        {
            // Move logic here
            _ticWave += 0.1f;
            _wave = (float)Math.Sin(_ticWave) * 2f;

            _y += _wave;
            _x -= speed;
            if (_x <= 0)
            {
                _x = Screen.Width;
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
                        new FxExplose(AbsXY, Color.Red, 20, 40, 50).AppendTo(_parent);
                        KillMe();
                    }

                    if (_timer.On(Timers.Hit))
                    {
                        _state.Set(States.Idle);
                        _timer.Stop(Timers.Hit);
                    }

                    break;
                case States.Move:
                    // Handle move state
                    break;
                case States.Shoot:
                    // Handle shoot state
                    break;
            }
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
                else if (_state.Is(States.Move))
                    batch.FillRectangleCentered(pos, AbsRectF.GetSize(), Color.Blue, 0);
                else if (_state.Is(States.Shoot))
                    batch.FillRectangleCentered(pos, AbsRectF.GetSize(), Color.Red, 0);
                
                batch.RectangleCentered(pos, AbsRectF.GetSize(), _state.Is(States.Hit)? Color.White:Color.Black, 5f);

                batch.CenterStringXY(G.FontMain, "Enemy", AbsXY, Color.White);
                batch.CenterStringXY(G.FontMain, $"{_state.CurState}", AbsRectF.TopCenter, Color.Cyan);
                batch.CenterStringXY(G.FontMain, $"{_energy}", AbsRectF.BottomCenter, Color.Yellow);
            }

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
