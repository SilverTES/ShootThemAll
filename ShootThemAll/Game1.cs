using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mugen.Core;
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
        public static KeyboardState Key;
        public static MouseState Mouse;
        public static Vector2 MousePos;

        public static SpriteFont FontMain;

        public static Texture2D TexLine;
        public static Texture2D TexCircle;

        public static Texture2D TexCG00;

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
            _screenPlay = new ScreenPlay(this);

            ScreenManager.Init(_screenPlay, Enums.GetList<Layers>());
            ScreenManager.SetLayerParameter((int)Layers.Main, blendState: BlendState.AlphaBlend, samplerState : _samplerState);
            ScreenManager.SetLayerParameter((int)Layers.FrontFX, blendState: BlendState.Additive, samplerState: _samplerState);

            G.TexLine = GFX.CreateLineTextureAA(GraphicsDevice, 100, 10, 5);
            G.TexCircle = GFX.CreateCircleTextureAA(GraphicsDevice, 100, 5);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            G.FontMain = Content.Load<SpriteFont>("Fonts/FontMain");
            G.TexCG00 = Content.Load<Texture2D>("Images/CG00");
        }

        protected override void Update(GameTime gameTime)
        {
            G.MousePos = WindowManager.GetMousePosition();
            G.Mouse = Mouse.GetState();
            G.Key = Keyboard.GetState();

            WindowManager.Update(gameTime);
            ScreenManager.Update(gameTime);

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (ButtonControl.OnPress("ToggleFullscreen", G.Key.IsKeyDown(Keys.F11)))
                WindowManager.ToggleFullscreen();

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
