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
            X,
            Y,
            Start,
            Back,
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

        private float _fireRate = 0.4f;   

        PlayerIndex _playerIndex;
        Vector2 _accMove = new Vector2(); // Acceleration/Deceleration du mouvement du joueur si il utilise le clavier
        Vector2 _stickLeft;
        Vector2 _stickRight;

        GamePadState _gamePadState;

        Shake Shake = new Shake();

        Node _targetScan = null;
        Node _magnetEnemy = null;
        public bool IsHasTarget => _targetScan != null ? _targetScan._isActive : false ;

        float _ticWave = 0f;
        float _wave = 0f;

        public int SlotSize = 5;
        public List<Color> ChainColors => _chainColors;
        List<Color> _chainColors = [];
        
        Control<Buttons> _control = new(); // TODO: Implement button control for hero
        bool _isReleasedA = false;

        public Hero(PlayerIndex playerIndex)
        {
            _type = UID.Get<Hero>();

            _playerIndex = playerIndex;
            SetSize(48, 48);
            SetPivot(Position.CENTER);

            SetCollideZone(ZoneBody, _rect);
            SetCollideZone(ZoneCast, _rect);

            _timer.Set(Timers.Shoot, Timer.Time(0, 0, _fireRate));
            _timer.Start(Timers.Shoot);

            MessageBus.Instance.Subscribe<FireRateMessage>((m) =>
            {
                SetFireRate(_fireRate * (1f - 1f / m.Speed)); // 10% de la vitesse
            });

            MessageBus.Instance.Subscribe<EnemyMagnetMessage>((m) =>
            {
                _magnetEnemy = m.Enemy;
            });

        }
        public void SetFireRate(float fireSpeed)
        {
            _fireRate = fireSpeed;

            _fireRate = Math.Clamp(_fireRate, 0.05f, 2f); // Clamp entre 0.1 et 2 secondes

            Console.WriteLine($"_fireRate = {_fireRate}");
            _timer.Set(Timers.Shoot, Timer.Time(0, 0, _fireRate));
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


            if (_gamePadState.Buttons.A == ButtonState.Pressed || (_playerIndex == PlayerIndex.One ? G.Key.IsKeyDown(Keys.LeftControl) : false))
            {
                if (_timer.On(Timers.Shoot) || _isReleasedA)
                {
                    Shoot();
                }
            }

            if (_control.Once(Buttons.X, _gamePadState.Buttons.X == ButtonState.Pressed || (_playerIndex == PlayerIndex.One ? G.Key.IsKeyDown(Keys.LeftShift) : false)))
            {
                MessageBus.Instance.SendMessage(new DestroyAllEnemyMessage());
            }

            if (_control.Once(Buttons.B, _gamePadState.Buttons.B == ButtonState.Pressed || (_playerIndex == PlayerIndex.One ? G.Key.IsKeyDown(Keys.LeftAlt) : false)))
            {
                MagnetEnemy();
            }

            _isReleasedA = _gamePadState.Buttons.A == ButtonState.Released && (_playerIndex == PlayerIndex.One ? G.Key.IsKeyUp(Keys.LeftControl) : false);


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
        private void MagnetEnemy()
        {
            //Misc.Log($"Try Magnet Enemy: {IsHasTarget}");

            if (IsHasTarget)
            {
                Misc.Log($"Magnet Enemy: {_targetScan._index}");

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
            float angle = ((float)Misc.Rng.NextDouble() - 0.5f) / 20f;
            angle += -Geo.RAD_90;

            var pos = XY - Vector2.UnitY * _oY;
            // Utilisation du pool
            G.PoolBullet.Get().Set(this, pos, angle, 24, Color.Gold, 100, 10).AppendTo(_parent);
            
            new FxExplose(Particles.Shapes.Line, pos + _parent.XY, Color.Gold, 10, 10, 40, 5).AppendTo(_parent);
            new FxGlow(pos, Color.Gold, .025f, 40).AppendTo(_parent);
            
            //G.SoundBim.Play(0.025f * G.Volume, 1f, 0f);
            G.SoundEffectManager.Play(G.SoundBim, 0.025f * G.Volume, 1f, 0f);
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

            collider = Collision2D.OnCollideZoneByNodeType(GetCollideZone(ZoneBody), UID.Get<Bonus<FireRateMessage>>(), Bonus<FireRateMessage>.ZoneBody);
            if (collider != null)
            {
                var bonus = collider._node as Bonus<FireRateMessage>;
                if (bonus != null)
                {
                    bonus.DestroyMe("Fire Rate +10%", new FireRateMessage(10));

                    //SetFireSpeed(_fireSpeed * (1f - 1f/10)); // 10% de la vitesse
                }
            }


            UpdateCollideZone(ZoneCast, new RectangleF(_x, 0, 1, _y - _oY));

            var colliders = Collision2D.ListCollideZoneByNodeType(GetCollideZone(ZoneCast), [UID.Get<Enemy>(), UID.Get<Bonus<FireRateMessage>>()], [Enemy.ZoneBody, Bonus<FireRateMessage>.ZoneBody]);

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
                        //MessageBus.Instance.SendMessage(new TogglePauseMessage());
                        continue;
                    }


                    if (node._type == UID.Get<Bonus<FireRateMessage>>())
                    {
                        Bonus<FireRateMessage> bonus = colliders[i]._node as Bonus<FireRateMessage>;
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

                        if (enemy._y > maxY && enemy.CurState != Enemy.States.FollowHero)
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


        }
        public override Node Update(GameTime gameTime)
        {
            _timer.Update();
            UpdateRect();

            HandleInput();

            _targetScan = null;

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
                batch.FillRectangleCentered(AbsXY + Vector2.UnitY * 40, AbsRectF.GetSize() * .90f, Color.Black *.5f, 0);

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
                if (IsHasTarget)
                {
                    batch.LineTexture(G.TexLine, AbsXY, new Vector2(AbsX, _targetScan.AbsY + _targetScan._oY), 5f, Color.Red * .75f);
                    //batch.RectangleTargetCentered(_targetScan.AbsXY, _targetScan.AbsRectF.GetSize() * (1.2f + _wave), Color.Red * .75f, 16, 16, 5f);
                    batch.RectangleTargetCentered(_targetScan.AbsXY, _targetScan.AbsRectF.GetSize() * (1.2f + _wave), HSV.Adjust(Color.Red, valueMultiplier: .5f + _wave * 2f), 16, 16, 5f);
                    batch.RectangleTargetCentered(_targetScan.AbsXY, _targetScan.AbsRectF.GetSize() * (1.2f + _wave), HSV.Adjust(Color.Yellow, valueMultiplier: .5f + _wave * 2f), 16, 16, 3f);
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
