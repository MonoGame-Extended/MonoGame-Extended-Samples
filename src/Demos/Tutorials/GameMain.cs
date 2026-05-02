using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Screens;
using MonoGame.Extended.ViewportAdapters;
using MonoGameGum;
using Tutorials.Demos;
using Tutorials.Screens;

namespace Tutorials
{
    public class GameMain : Game
    {
        private readonly GraphicsDeviceManager _graphicsDeviceManager;
        private readonly FramesPerSecondCounter _fpsCounter = new FramesPerSecondCounter();
        private readonly ScreenManager _screenManager;
        private ScreenName _currentScreen;

        public ViewportAdapter ViewportAdapter { get; private set; }

        public GumService GumUI => GumService.Default;

        public GameMain(PlatformConfig config)
        {
            _graphicsDeviceManager = new GraphicsDeviceManager(this)
            {
                IsFullScreen = config.IsFullScreen,
                SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight
            };

            _graphicsDeviceManager.PreferredBackBufferWidth = 800;
            _graphicsDeviceManager.PreferredBackBufferHeight = 480;
            _graphicsDeviceManager.ApplyChanges();

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.AllowUserResizing = true;

            _screenManager = new ScreenManager();
            Components.Add(_screenManager);
        }

        protected override void Initialize()
        {
            GumUI.Initialize(this);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            ViewportAdapter = new BoxingViewportAdapter(Window, GraphicsDevice, 800, 480);
            LoadScreen(ScreenName.MainMenu);
        }

        public void LoadScreen(ScreenName screen)
        {
            IsMouseVisible = true;
            GameScreen nextScreen = CreateScreen(screen);

            if (_screenManager.ActiveScreen == null)
            {
                _screenManager.ShowScreen(nextScreen);
            }
            else
            {
                _screenManager.ReplaceScreen(nextScreen);
            }

            _currentScreen = screen;
        }

        protected override void Update(GameTime gameTime)
        {
            _fpsCounter.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _fpsCounter.Draw(gameTime);
            Window.Title = $"{_currentScreen} {_fpsCounter.FramesPerSecond}";
            base.Draw(gameTime);
        }

        private GameScreen CreateScreen(ScreenName screen)
        {
            switch (screen)
            {
                case ScreenName.Animation:
                    return new AnimationScreen(this);
                case ScreenName.Batching:
                    return new BatchingScreen(this);
                case ScreenName.BitmapFonts:
                    return new BitmapFontsScreen(this);
                case ScreenName.Camera:
                    return new CameraScreen(this);
                case ScreenName.Collision:
                    return new CollisionScreen(this);
                case ScreenName.InputListener:
                    return new InputListenersScreen(this);
                case ScreenName.MainMenu:
                    return new MainMenuScreen(this);
                case ScreenName.Particles:
                    return new ParticlesScreen(this);
                case ScreenName.Shapes:
                    return new ShapesScreen(this);
                case ScreenName.Sprites:
                    return new SpritesScreen(this);
                case ScreenName.TiledMaps:
                    return new TiledMapsScreen(this);
                case ScreenName.ViewportAdapter:
                    return new ViewportAdaptersScreen(this);
                default:
                    throw new System.ArgumentOutOfRangeException(nameof(screen), screen, null);
            }
        }
    }
}
