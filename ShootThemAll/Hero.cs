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

namespace ShootThemAll
{
    public class Hero : Node
    {
        public enum Buttons
        {
            A,
            B,
        }
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
        public const int ZoneCast = 1;

        private int _maxEnergy = 40;
        private int _energy = 40;

        private float _fireSpeed = 0.4f;   

        PlayerIndex _playerIndex;
        Vector2 _accMove = new Vector2(); // Acceleration/Deceleration du mouvement du joueur si il utilise le clavier
        Vector2 _stickLeft;
        Vector2 _stickRight;

        GamePadState _gamePadState;

        Shake Shake = new Shake();

        Node _targetScan = null;
        Node _magnetEnemy = null;
        bool _isShowTarget => _targetScan != null ? _targetScan._isActive : false ;

        float _ticWave = 0f;
        float _wave = 0f;

        public int SlotSize = 5;
        public List<Color> ChainColors => _chainColors;
        List<Color> _chainColors = [];
        
        Control<Buttons> _control = new(); // TODO: Implement button control for hero

        public Hero(PlayerIndex playerIndex)
        {
            _type = UID.Get<Hero>();

            _playerIndex = playerIndex;
            SetSize(48, 48);
            SetPivot(Position.CENTER);

            SetCollideZone(ZoneBody, _rect);
            SetCollideZone(ZoneCast, _rect);

            _timer.Set(Timers.Shoot, Timer.Time(0, 0, _fireSpeed), true);
            _timer.Start(Timers.Shoot);

            MessageBus.Instance.Subscribe<FireSpeedMessage>((m) =>
            {
                SetFireSpeed(_fireSpeed * (1f - 1f / m.Speed)); // 10% de la vitesse
            });

            MessageBus.Instance.Subscribe<EnemyMagnetMessage>((m) =>
            {
                _magnetEnemy = m.Enemy;
            });

        }
        public void SetFireSpeed(float fireSpeed)
        {
            _fireSpeed = fireSpeed;

            _fireSpeed = Math.Clamp(_fireSpeed, 0.05f, 2f); // Clamp entre 0.1 et 2 secondes

            Console.WriteLine($"_fireSpeed = {_fireSpeed}");
            _timer.Set(Timers.Shoot, Timer.Time(0, 0, _fireSpeed), true);
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
            }

            if (_gamePadState.Buttons.A == ButtonState.Pressed || (_playerIndex == PlayerIndex.One ? G.Key.IsKeyDown(Keys.LeftControl) : false)) Shoot();
            if (_control.Once(Buttons.B, _gamePadState.Buttons.B == ButtonState.Pressed || (_playerIndex == PlayerIndex.One ? G.Key.IsKeyDown(Keys.LeftAlt) : false))) MagnetEnemy();

            //Auto Shoot
            //if (_stickLeft.Equals(Vector2.Zero))
            //    Shoot();

        }
        public void AddChainColor(Color color)
        {
            _chainColors.Add(color);

            if (_chainColors.Count > SlotSize)
            {
                _chainColors.Clear();
                _chainColors.Add(color);
            }
        }
        public void MagnetEnemy()
        {
            if (_targetScan != null)
            {
                if (_targetScan._type == UID.Get<Enemy>())
                {
                    if (_magnetEnemy == null)
                    {
                        Enemy enemy = _targetScan as Enemy;
                        
                        enemy.MagnetHero(this);
                        Misc.Log("Magnet Hero");
                    }
                    else
                    {
                        if (_magnetEnemy._type == UID.Get<Enemy>())
                        {
                            Enemy magnetEnemy = _magnetEnemy as Enemy;
                            Enemy enemy = _targetScan as Enemy;
                            
                            magnetEnemy.MagnetEnemy(enemy);
                            Misc.Log("Magnet Enemy");

                            _magnetEnemy = null; // Reset magnet enemy
                        }
                    }
                }
            }
        }
        public void Shoot()
        {
            if (_timer.On(Timers.Shoot))
            {
                float angle = ((float)Misc.Rng.NextDouble() - 0.5f) / 20f;

                angle += -Geo.RAD_90;

                // Utilisation du pool
                G.PoolBullet.Get().Set(this, XY - Vector2.UnitY * _oY, angle, 24, Color.Gold, 100, 10).AppendTo(_parent);

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

            collider = Collision2D.OnCollideZoneByNodeType(GetCollideZone(ZoneBody), UID.Get<Bonus<FireSpeedMessage>>(), Bonus<FireSpeedMessage>.ZoneBody);
            if (collider != null)
            {
                var bonus = collider._node as Bonus<FireSpeedMessage>;
                if (bonus != null)
                {
                    bonus.DestroyMe("Fire Speed +10%", new FireSpeedMessage(10));

                    //SetFireSpeed(_fireSpeed * (1f - 1f/10)); // 10% de la vitesse
                }
            }


            UpdateCollideZone(ZoneCast, new RectangleF(_x, 0, 1, _y - _oY));

            var colliders = Collision2D.ListCollideZoneByNodeType(GetCollideZone(ZoneCast), [UID.Get<Enemy>(), UID.Get<Bonus<FireSpeedMessage>>()], [Enemy.ZoneBody, Bonus<FireSpeedMessage>.ZoneBody]);

            if (colliders.Count > 0)
            {
                float maxY = 0f;
                for (int i = 0; i < colliders.Count; i++)
                {
                    if (colliders[i]._node == null) continue;

                    var node = colliders[i]._node;

                    //if (node == null) continue;
                    if (!node._isActive)
                    {
                        Misc.Log($"Node {node._index} is not active");
                        MessageBus.Instance.SendMessage(new TogglePauseMessage());
                        continue;
                    }


                    if (node._type == UID.Get<Bonus<FireSpeedMessage>>())
                    {
                        Bonus<FireSpeedMessage> bonus = colliders[i]._node as Bonus<FireSpeedMessage>;
                        //if (bonus != null)
                        //{
                        //    //bonus.DestroyMe("Fire Speed +10%");
                        //    //SetFireSpeed(_fireSpeed * (1f - 1f / 10)); // 10% de la vitesse

                        //}
                        if (bonus._y > maxY)
                        {
                            maxY = bonus._y;
                            _targetScan = bonus;
                        }
                        else
                            continue;

                        continue;
                    }

                    if (node._type == UID.Get<Enemy>())
                    {
                        Enemy enemy = colliders[i]._node as Enemy;

                        if (enemy._y > maxY)// && enemy.CurState != Enemy.States.Dead)
                        {
                            maxY = enemy._y;
                            _targetScan = enemy;
                        }
                        else
                            continue;
                        //enemy.DestroyMe();
                        //Console.WriteLine("SCan found");
                    }

                }
            }

            //if (_targetScan != null)
            //    if (!_targetScan._isActive) _targetScan = null; // Reset target scan if not active anymore

        }
        public override Node Update(GameTime gameTime)
        {
            _timer.Update();
            UpdateRect();

            _targetScan = null;

            HandleInput();
            HandleCollision();

            _x += _stickLeft.X * 10f;
            _y += -_stickLeft.Y * 10f;

            _ticWave += 0.1f;
            _wave = (float)Math.Abs(Math.Sin(_ticWave) * .25f);

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


                Vector2 pos = AbsRectF.TopCenter - Vector2.UnitY * 10 - Vector2.UnitX * (_maxEnergy / 2) + Shake.GetVector2() * .5f;
                G.DrawEnergyBar(batch, pos, _energy, _maxEnergy, _alpha,  1f, 10f);

                //batch.CenterBorderedStringXY(G.FontMain, $"{_energy}", AbsRectF.TopLeft - Vector2.One * 10 + Shake.GetVector2() * .5f, fg * _alpha, bg * _alpha);

            }


            if (indexLayer == (int)Layers.FrontFX)
            {
                if (_isShowTarget)
                {
                    batch.LineTexture(G.TexLine, AbsXY, new Vector2(AbsX, _targetScan.AbsY + _targetScan._oY), 5f, Color.Red * .75f);
                    //batch.RectangleTargetCentered(_targetScan.AbsXY, _targetScan.AbsRectF.GetSize() * (1.2f + _wave), Color.Red * .75f, 16, 16, 5f);
                    batch.RectangleTargetCentered(_targetScan.AbsXY, _targetScan.AbsRectF.GetSize() * (1.2f + _wave), HSV.Adjust(Color.Gold, valueMultiplier: .5f + _wave * 2f), 16, 16, 3f);
                }
                else
                {
                    batch.LineTexture(G.TexLine, AbsXY, new Vector2(AbsX, 0), 5f, Color.Red * .5f);

                }

                batch.FilledCircle(G.TexCircle, AbsXY, 10, Color.Gold * _alpha);
            }

            if (indexLayer == (int)Layers.Debug)
            {
                //batch.Rectangle(GetCollideZone(ZoneCast)._rect, Color.Red * .5f);
            }

            return base.Draw(batch, gameTime, indexLayer);
        }

    }
}
