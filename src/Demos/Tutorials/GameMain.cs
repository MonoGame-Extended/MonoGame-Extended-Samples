using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Screens;
using MonoGame.Extended.ViewportAdapters;
using MonoGameGum;
using MonoGameGum.Forms;
using MonoGameGum.Forms.Controls;
using RenderingLibrary;
using Tutorials.Demos;
using Tutorials.Screens;

namespace Tutorials
{
    public class GameMain : Game
    {
        private readonly GraphicsDeviceManager _graphicsDeviceManager;
        private readonly FramesPerSecondCounter _fpsCounter = new FramesPerSecondCounter();
        private readonly Dictionary<ScreenName, GameScreen> _screens = new Dictionary<ScreenName, GameScreen>();

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
            //  Initialize GUM UI System
            GumUI.Initialize(this, DefaultVisualsVersion.V2);

            //  Initialize demos screens
            _screens.Add(ScreenName.Animation, new AnimationScreen(this));
            _screens.Add(ScreenName.Batching, new BatchingScreen(this));
            _screens.Add(ScreenName.BitmapFonts, new BitmapFontsScreen(this));
            _screens.Add(ScreenName.Camera, new CameraScreen(this));
            _screens.Add(ScreenName.Collision, new CollisionScreen(this));
            _screens.Add(ScreenName.InputListener, new InputListenersScreen(this));
            _screens.Add(ScreenName.MainMenu, new MainMenuScreen(this));
            _screens.Add(ScreenName.Particles, new ParticlesScreen(this));
            _screens.Add(ScreenName.Shapes, new ShapesScreen(this));
            _screens.Add(ScreenName.Sprites, new SpritesScreen(this));
            _screens.Add(ScreenName.TiledMaps, new TiledMapsScreen(this));
            _screens.Add(ScreenName.ViewportAdapter, new ViewportAdaptersScreen(this));

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
            _screenManager.LoadScreen(_screens[screen]);
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
    }
}
