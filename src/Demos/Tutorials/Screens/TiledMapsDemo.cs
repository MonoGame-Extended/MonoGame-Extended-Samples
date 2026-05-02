using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Tilemaps;
using MonoGame.Extended.Tilemaps.Rendering;
using MonoGame.Extended.ViewportAdapters;

namespace Tutorials.Screens;

public class TiledMapsScreen : GameScreen
{
    private readonly Queue<string> _availableMaps = new();

    private BitmapFont _bitmapFont = null!;
    private OrthographicCamera _camera = null!;
    private SpriteBatch _spriteBatch = null!;
    private TilemapRenderer _mapRenderer = null!;
    private ViewportAdapter _viewportAdapter = null!;
    private KeyboardState _previousKeyboardState;
    private bool _showHelp;
    private Tilemap _map = null!;

    public new GameMain Game => (GameMain)base.Game;

    public TiledMapsScreen(GameMain game)
        : base(game)
    {
    }

    public override void Dispose()
    {
        _mapRenderer?.Dispose();
        base.Dispose();
    }

    public override void Initialize()
    {
        _viewportAdapter = new BoxingViewportAdapter(Game.Window, GraphicsDevice, 1024, 768);
        _camera = new OrthographicCamera(_viewportAdapter);

        Game.Window.AllowUserResizing = true;

        base.Initialize();
    }

    public override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _bitmapFont = Content.Load<BitmapFont>("Fonts/montserrat-32");
        _mapRenderer = new TilemapRenderer(GraphicsDevice);

        foreach (string mapName in new[] { "level01", "level02", "level03", "level04", "level05", "level06", "level07", "level08" })
        {
            _availableMaps.Enqueue(mapName);
        }

        LoadNextMap();
        _camera.Position = new Vector2(-104, -92);
    }

    public override void Update(GameTime gameTime)
    {
        float deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
        KeyboardState keyboardState = Keyboard.GetState();

        if (keyboardState.IsKeyDown(Keys.Escape))
        {
            Game.LoadScreen(ScreenName.MainMenu);
        }

        const float cameraSpeed = 500f;
        const float zoomSpeed = 0.3f;

        Vector2 moveDirection = Vector2.Zero;

        if (keyboardState.IsKeyDown(Keys.W) || keyboardState.IsKeyDown(Keys.Up))
        {
            moveDirection -= Vector2.UnitY;
        }

        if (keyboardState.IsKeyDown(Keys.A) || keyboardState.IsKeyDown(Keys.Left))
        {
            moveDirection -= Vector2.UnitX;
        }

        if (keyboardState.IsKeyDown(Keys.S) || keyboardState.IsKeyDown(Keys.Down))
        {
            moveDirection += Vector2.UnitY;
        }

        if (keyboardState.IsKeyDown(Keys.D) || keyboardState.IsKeyDown(Keys.Right))
        {
            moveDirection += Vector2.UnitX;
        }

        if (moveDirection != Vector2.Zero)
        {
            moveDirection.Normalize();
            _camera.Move(moveDirection * cameraSpeed * deltaSeconds);
        }

        if (keyboardState.IsKeyDown(Keys.R))
        {
            _camera.ZoomIn(zoomSpeed * deltaSeconds);
        }

        if (keyboardState.IsKeyDown(Keys.F))
        {
            _camera.ZoomOut(zoomSpeed * deltaSeconds);
        }

        if (_previousKeyboardState.IsKeyDown(Keys.Tab) && keyboardState.IsKeyUp(Keys.Tab))
        {
            LoadNextMap();
        }

        if (_previousKeyboardState.IsKeyDown(Keys.H) && keyboardState.IsKeyUp(Keys.H))
        {
            _showHelp = !_showHelp;
        }

        if (keyboardState.IsKeyDown(Keys.Z))
        {
            _camera.Position = Vector2.Zero;
        }

        if (keyboardState.IsKeyDown(Keys.X))
        {
            _camera.LookAt(Vector2.Zero);
        }

        if (keyboardState.IsKeyDown(Keys.C))
        {
            LookAtMapCenter();
        }

        _previousKeyboardState = keyboardState;
    }

    public override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _mapRenderer.Draw(_camera);
        DrawText();
    }

    private void LoadNextMap()
    {
        string name = _availableMaps.Dequeue();
        _map = Content.Load<Tilemap>($"TiledMaps/{name}");
        _availableMaps.Enqueue(name);
        _mapRenderer.LoadTilemap(_map);
        LookAtMapCenter();
    }

    private void LookAtMapCenter()
    {
        Rectangle worldBounds = _map.WorldBounds;
        Vector2 center = new Vector2(worldBounds.Center.X, worldBounds.Center.Y);

        if (_map.Orientation == TilemapOrientation.Isometric)
        {
            center = new Vector2(0f, worldBounds.Height * 0.5f);
        }

        _camera.LookAt(center);
    }

    private void DrawText()
    {
        int tileLayerCount = 0;
        int imageLayerCount = 0;

        foreach (TilemapLayer layer in _map.Layers)
        {
            if (layer is TilemapTileLayer)
            {
                tileLayerCount++;
            }
            else if (layer is TilemapImageLayer)
            {
                imageLayerCount++;
            }
        }

        Color textColor = Color.Black;
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);

        Vector2 baseTextPosition = new Vector2(5, 0);
        Vector2 textPosition = baseTextPosition;
        _spriteBatch.DrawString(
            _bitmapFont,
            $"Map: {_map.Name}; {tileLayerCount} tile layer(s) @ {_map.Width}x{_map.Height} tiles, {imageLayerCount} image layer(s)",
            textPosition,
            textColor);

        textPosition = baseTextPosition + new Vector2(0, _bitmapFont.LineHeight * 1);
        _spriteBatch.DrawString(
            _bitmapFont,
            $"Camera Position: (x={_camera.Position.X}, y={_camera.Position.Y})",
            textPosition,
            textColor);

        if (!_showHelp)
        {
            _spriteBatch.DrawString(_bitmapFont, "H: Show help", new Vector2(5, _bitmapFont.LineHeight * 2), textColor);
        }
        else
        {
            textPosition = baseTextPosition + new Vector2(0, _bitmapFont.LineHeight * 2);
            _spriteBatch.DrawString(_bitmapFont, "H: Hide help", textPosition, textColor);
            textPosition = baseTextPosition + new Vector2(0, _bitmapFont.LineHeight * 3);
            _spriteBatch.DrawString(_bitmapFont, "WASD/Arrows: Pan camera", textPosition, textColor);
            textPosition = baseTextPosition + new Vector2(0, _bitmapFont.LineHeight * 4);
            _spriteBatch.DrawString(_bitmapFont, "RF: Zoom camera in / out", textPosition, textColor);
            textPosition = baseTextPosition + new Vector2(0, _bitmapFont.LineHeight * 5);
            _spriteBatch.DrawString(_bitmapFont, "Z: Move camera to the origin", textPosition, textColor);
            textPosition = baseTextPosition + new Vector2(0, _bitmapFont.LineHeight * 6);
            _spriteBatch.DrawString(_bitmapFont, "X: Move camera to look at the origin", textPosition, textColor);
            textPosition = baseTextPosition + new Vector2(0, _bitmapFont.LineHeight * 7);
            _spriteBatch.DrawString(_bitmapFont, "C: Move camera to look at center of the map", textPosition, textColor);
            textPosition = baseTextPosition + new Vector2(0, _bitmapFont.LineHeight * 8);
            _spriteBatch.DrawString(_bitmapFont, "Tab: Cycle through maps", textPosition, textColor);
        }

        _spriteBatch.End();
    }
}
