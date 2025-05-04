using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Mugen.Audio;
using Mugen.Core;
using Mugen.Event.Message;
using Mugen.GFX;
using Mugen.Input;

namespace ShootThemAll
{
    public enum Layers
    {
        Back,
        Main,
        Front,
        FrontFX,
        Glow,
        UI,
        Debug,
    }

    public class Screen
    {
        public static int Width = 1920;
        public static int Height = 1080;
    }

    public class G
    {
        public static SoundEffectManager SoundEffectManager;

        public static ObjectPool<Bullet> PoolBullet;
        public static ObjectPool<Enemy> PoolEnemy;

        public static KeyboardState Key;
        public static MouseState Mouse;
        public static Vector2 MousePos;

        public static SpriteFont FontMain;

        public static Texture2D TexLine;
        public static Texture2D TexCircle;

        public static Texture2D TexBG00;
        public static Texture2D TexCG00;
        public static Texture2D TexGlow1;
        public static Texture2D TexCircleGlow;

        public static float Volume = 0.5f;

        public static SoundEffect SoundBim;
        public static SoundEffect SoundHit;
        public static SoundEffect SoundExplose;
        public static SoundEffect SoundBonus;

        public static Song MusicTest;

        public static void DrawEnergyBar(SpriteBatch batch, Vector2 pos, float energy, float maxEnergy, float alpha,  float scale = 1f, float warningEnergy = 10f)
        {
            Color fg = Color.GreenYellow;
            Color bg = Color.Green;

            if (energy <= warningEnergy)
            {
                fg = Color.Yellow;
                bg = Color.Red;
            }

            GFX.Bar(batch, pos, maxEnergy * scale, 8, Color.Red * alpha);
            GFX.Bar(batch, pos, energy * scale, 8, fg * alpha);
            GFX.BarLines(batch, pos, maxEnergy * scale, 8, Color.Black * alpha, 2);
            GFX.Bar(batch, pos - Vector2.UnitY * 2f, maxEnergy * scale, 2, Color.White * .5f * alpha);
        }

    }

    public class Game1 : Game
    {
        private ScreenPlay _screenPlay;
        private SamplerState _samplerState = SamplerState.LinearWrap;
        public Game1()
        {
            WindowManager.Init(this, Screen.Width, Screen.Height);

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
        }

        protected override void Initialize()
        {
            base.Initialize();

            _screenPlay = new ScreenPlay(this);

            ScreenManager.Init(_screenPlay, Enums.GetList<Layers>());
            ScreenManager.SetLayerParameter((int)Layers.Main, blendState: BlendState.AlphaBlend, samplerState : _samplerState);
            ScreenManager.SetLayerParameter((int)Layers.FrontFX, blendState: BlendState.Additive, samplerState: _samplerState);
            ScreenManager.SetLayerParameter((int)Layers.Glow, blendState: BlendState.Additive, samplerState: _samplerState);

            G.TexLine = GFX.CreateLineTextureAA(GraphicsDevice, 100, 10, 5);
            G.TexCircle = GFX.CreateCircleTextureAA(GraphicsDevice, 100, 5);

        }

        protected override void LoadContent()
        {
            G.FontMain = Content.Load<SpriteFont>("Fonts/FontMain");


            G.TexBG00 = Content.Load<Texture2D>("Images/bg00");
            G.TexCG00 = Content.Load<Texture2D>("Images/CG00");
            G.TexGlow1 = Content.Load<Texture2D>("Images/glow1");
            G.TexCircleGlow = Content.Load<Texture2D>("Images/circleGlow1");

            G.SoundBim = Content.Load<SoundEffect>("Sounds/laser-pistol-gun");
            G.SoundHit = Content.Load<SoundEffect>("Sounds/ingame_door_close");
            G.SoundExplose = Content.Load<SoundEffect>("Sounds/Explosion");
            G.SoundBonus = Content.Load<SoundEffect>("Sounds/success_1");

            //G.MusicTest = Content.Load<Song>("Musics/destinazione_altrove_-_Kalte_Ohren_(_Remix_)");
            G.MusicTest = Content.Load<Song>("Musics/music");


            G.SoundEffectManager = new SoundEffectManager();
            G.SoundEffectManager.AddSoundEffect(G.SoundBim, 4);
            G.SoundEffectManager.AddSoundEffect(G.SoundHit, 4);
            G.SoundEffectManager.AddSoundEffect(G.SoundExplose, 4);
            G.SoundEffectManager.AddSoundEffect(G.SoundBonus, 4);

        }
        protected override void UnloadContent()
        {
            // Libérer les ressources
            G.SoundEffectManager.Dispose();

            base.UnloadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            // Traiter les messages
            MessageBus.Instance.ProcessMessages(gameTime);

            G.MousePos = WindowManager.GetMousePosition();
            G.Mouse = Mouse.GetState();
            G.Key = Keyboard.GetState();

            WindowManager.Update(gameTime);
            ScreenManager.Update(gameTime);

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (ButtonControl.OnPress("ToggleFullscreen", G.Key.IsKeyDown(Keys.F11)))
                WindowManager.ToggleFullscreen();

            G.SoundEffectManager.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            ScreenManager.DrawScreen(gameTime);
            ScreenManager.ShowScreen(gameTime, sortMode: SpriteSortMode.Deferred, blendState: BlendState.AlphaBlend, samplerState: _samplerState);

            base.Draw(gameTime);
        }
    }
}
