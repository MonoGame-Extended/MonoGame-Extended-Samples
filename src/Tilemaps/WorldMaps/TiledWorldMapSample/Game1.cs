using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Tilemaps;
using MonoGame.Extended.Tilemaps.Rendering;
using MonoGame.Extended.ViewportAdapters;

namespace TiledWorldMapSample;

/// <summary>
/// Demonstrates world-map rendering using rooms exported from LDtk to Tiled format.
/// World positions come from the .world JSON file. Pan with arrow keys or WASD, zoom
/// with the scroll wheel or +/-, and switch world depth layers with Page Up/Down.
///
/// Tiled's .world format has no depth field, so depth-1 rooms (Shop, Long_hallway,
/// Exit, Shortcut_passage) are assigned manually.
///
/// Controls:
///   Left / Right / A / D - pan
///   Up / Down / W / S    - pan
///   Scroll / +/-         - zoom in/out
///   R                    - reset zoom
///   Page Up / Page Down  - switch world depth layer
///   Escape               - exit
/// </summary>
public sealed class Game1 : Game
{
    // Tiled's .world format has no depth field. These entries are depth 1 in the
    // LDtk source and overlap depth-0 rooms in world space.
    private static readonly HashSet<string> s_depth1Levels = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "0009_Shop",
        "0017_Long_hallway",
        "0018_Exit",
        "0021_Shortcut_passage"
    };

    private const int WindowWidth = 1280;
    private const int WindowHeight = 720;
    private const float DefaultZoom = 2f;
    private const float MinZoom = 0.25f;
    private const float MaxZoom = 8f;
    private const float ZoomStep = 0.25f;

    // World-space pan speed at zoom 1; scaled by 1/zoom so screen-space speed stays constant.
    private const float PanSpeed = 500f;

    private readonly GraphicsDeviceManager _graphics;
    private BoxingViewportAdapter _viewportAdapter = null!;
    private OrthographicCamera _camera = null!;
    private TilemapWorld _tilemapWorld = null!;
    private TilemapWorldRenderer _renderer = null!;

    private int _worldDepth;
    private KeyboardState _previousKeys;
    private MouseState _previousMouse;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = WindowWidth,
            PreferredBackBufferHeight = WindowHeight,
            SynchronizeWithVerticalRetrace = false
        };

        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        IsFixedTimeStep = false;
        InactiveSleepTime = TimeSpan.Zero;
    }

    protected override void LoadContent()
    {
        _viewportAdapter = new BoxingViewportAdapter(Window, GraphicsDevice, WindowWidth, WindowHeight);
        _camera = new OrthographicCamera(_viewportAdapter);
        _camera.Zoom = DefaultZoom;

        _tilemapWorld = Content.Load<TilemapWorld>("WorldMap_GridVania_layout");

        // Tiled's .world format has no depth field; assign it manually.
        foreach (Tilemap tilemap in _tilemapWorld.Levels)
        {
            if (s_depth1Levels.Contains(tilemap.Name))
            {
                tilemap.WorldDepth = 1;
            }
        }

        // Some rooms have negative world positions in the .world file (e.g. Ossuary at -1024, 0).
        // Shift all WorldPositions so the minimum is at (0, 0) before loading into the
        // renderer, which requires non-negative coordinates.
        NormalizeWorldPositions();

        Rectangle worldBounds = ComputeWorldBounds();
        _camera.LookAt(new Vector2(worldBounds.Width / 2f, worldBounds.Height / 2f));
        _camera.EnableWorldBounds(worldBounds);

        _renderer = new TilemapWorldRenderer(GraphicsDevice);
        _renderer.BlendState = BlendState.AlphaBlend;
        _renderer.Load(_tilemapWorld.Levels);
    }

    private void NormalizeWorldPositions()
    {
        float minX = float.MaxValue, minY = float.MaxValue;
        foreach (Tilemap t in _tilemapWorld.Levels)
        {
            if (t.WorldPosition.X < minX)
            {
                minX = t.WorldPosition.X;
            }

            if (t.WorldPosition.Y < minY)
            {
                minY = t.WorldPosition.Y;
            }
        }

        float offX = minX < 0f ? -minX : 0f;
        float offY = minY < 0f ? -minY : 0f;

        if (offX == 0f && offY == 0f)
        {
            return;
        }

        Vector2 offset = new Vector2(offX, offY);
        foreach (Tilemap tilemap in _tilemapWorld.Levels)
        {
            tilemap.WorldPosition += offset;
        }
    }

    private Rectangle ComputeWorldBounds()
    {
        int maxX = 0, maxY = 0;
        foreach (Tilemap tilemap in _tilemapWorld.Levels)
        {
            int right = (int)tilemap.WorldPosition.X + tilemap.WorldBounds.Width;
            int bottom = (int)tilemap.WorldPosition.Y + tilemap.WorldBounds.Height;

            if (right > maxX)
            {
                maxX = right;
            }

            if (bottom > maxY)
            {
                maxY = bottom;
            }
        }

        return new Rectangle(0, 0, Math.Max(maxX, 1), Math.Max(maxY, 1));
    }

    protected override void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        KeyboardState keys = Keyboard.GetState();

        if (keys.IsKeyDown(Keys.Escape))
        {
            Exit();
        }

        if (keys.IsKeyDown(Keys.PageUp) && !_previousKeys.IsKeyDown(Keys.PageUp))
        {
            _worldDepth = Math.Max(0, _worldDepth - 1);
        }

        if (keys.IsKeyDown(Keys.PageDown) && !_previousKeys.IsKeyDown(Keys.PageDown))
        {
            _worldDepth = Math.Min(1, _worldDepth + 1);
        }

        // Camera pan: world-space velocity scaled by 1/zoom keeps screen-space speed constant.
        Vector2 panDir = Vector2.Zero;

        if (keys.IsKeyDown(Keys.Left) || keys.IsKeyDown(Keys.A))
        {
            panDir.X -= 1f;
        }

        if (keys.IsKeyDown(Keys.Right) || keys.IsKeyDown(Keys.D))
        {
            panDir.X += 1f;
        }

        if (keys.IsKeyDown(Keys.Up) || keys.IsKeyDown(Keys.W))
        {
            panDir.Y -= 1f;
        }

        if (keys.IsKeyDown(Keys.Down) || keys.IsKeyDown(Keys.S))
        {
            panDir.Y += 1f;
        }

        if (panDir != Vector2.Zero)
        {
            _camera.Move(panDir * (PanSpeed / _camera.Zoom) * dt);
        }

        MouseState mouse = Mouse.GetState();
        int scrollDelta = mouse.ScrollWheelValue - _previousMouse.ScrollWheelValue;

        if (scrollDelta > 0)
        {
            _camera.Zoom = MathHelper.Clamp(_camera.Zoom + ZoomStep, MinZoom, MaxZoom);
        }
        else if (scrollDelta < 0)
        {
            _camera.Zoom = MathHelper.Clamp(_camera.Zoom - ZoomStep, MinZoom, MaxZoom);
        }

        if (keys.IsKeyDown(Keys.OemPlus) && !_previousKeys.IsKeyDown(Keys.OemPlus))
        {
            _camera.Zoom = MathHelper.Clamp(_camera.Zoom + ZoomStep, MinZoom, MaxZoom);
        }

        if (keys.IsKeyDown(Keys.OemMinus) && !_previousKeys.IsKeyDown(Keys.OemMinus))
        {
            _camera.Zoom = MathHelper.Clamp(_camera.Zoom - ZoomStep, MinZoom, MaxZoom);
        }

        if (keys.IsKeyDown(Keys.R) && !_previousKeys.IsKeyDown(Keys.R))
        {
            _camera.Zoom = DefaultZoom;
        }

        _previousMouse = mouse;
        _previousKeys = keys;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(0x37, 0x49, 0x4E));

        _renderer.Draw(_camera, _worldDepth);

        base.Draw(gameTime);
    }

    protected override void UnloadContent()
    {
        _renderer?.Dispose();
        base.UnloadContent();
    }
}
