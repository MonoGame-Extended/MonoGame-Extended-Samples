using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Tilemaps;
using MonoGame.Extended.Tilemaps.Rendering;

namespace DemoGame.Shared;

/// <summary>
/// Abstract base class for the three tilemap format demo games.
/// Handles all common startup: atlas loading, screen management, music, and rendering.
/// Subclasses provide format-specific tilemap loading and collision map building.
/// </summary>
public abstract class DemoGameBase : Game
{
    // Virtual canvas: 18 x 12 tiles at 16 px each = 288 x 192.
    public const int VirtualWidth = 288;
    public const int VirtualHeight = 192;

    // Window is 3x (864 x 576) for a clean integer pixel presentation.
    public const int ViewWidth = VirtualWidth * 3;
    public const int ViewHeight = VirtualHeight * 3;

    protected readonly GraphicsDeviceManager Graphics;
    private readonly ScreenManager _screenManager;

    private Song? _song;

    public Texture2DAtlas Atlas { get; private set; } = null!;
    public SpriteSheet SpriteSheet { get; private set; } = null!;
    public Dictionary<string, RectangleF> Hitboxes { get; private set; } = null!;
    public Tilemap Tilemap { get; private set; } = null!;
    public TilemapRenderer TilemapRenderer { get; private set; } = null!;
    public CollisionMap CollisionMap { get; private set; } = null!;

    /// <summary>
    /// Display name for the tilemap format used by this demo.
    /// </summary>
    public virtual string FormatName => "Tilemap";

    /// <summary>
    /// Name of the object layer in the tilemap that contains entity spawn points.
    /// Override in subclasses when the layer has a different name.
    /// </summary>
    public virtual string EntityLayerName => "Objects";

    /// <summary>
    /// Name of the tilemap object that marks the player spawn position.
    /// Override in subclasses when the spawn object has a different name.
    /// </summary>
    public virtual string PlayerSpawnName => "PLAYER_SPAWN";

    protected DemoGameBase()
    {
        Graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = ViewWidth,
            PreferredBackBufferHeight = ViewHeight
        };

        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _screenManager = new ScreenManager();
        Graphics.SynchronizeWithVerticalRetrace = false;
        IsFixedTimeStep = false;
    }

    protected override void Initialize()
    {
        Components.Add(_screenManager);
        base.Initialize();
    }

    protected override void LoadContent()
    {
        string atlasJsonPath = Path.Combine(Content.RootDirectory, "atlas.json");
        AtlasLoadResult loaded = AtlasJsonLoader.Load(Content, atlasJsonPath, "atlas");
        Atlas = loaded.Atlas;
        SpriteSheet = loaded.SpriteSheet;
        Hitboxes = loaded.Hitboxes;

        Tilemap = LoadTilemap();
        TilemapRenderer = new TilemapRenderer(GraphicsDevice);
        TilemapRenderer.LoadTilemap(Tilemap);

        CollisionMap = BuildCollisionMap(Tilemap);

        try
        {
            _song = Content.Load<Song>("music");
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = 0.5f;
            MediaPlayer.Play(_song);
        }
        catch (Exception)
        {
            // Music optional
            // Not use in the world map demos as of right now
        }

        _screenManager.ShowScreen(new TitleScreen(this, _screenManager));
    }

    /// <summary>
    /// Parses and returns the tilemap for this demo's format.
    /// Called once during <see cref="LoadContent"/>.
    /// </summary>
    protected abstract Tilemap LoadTilemap();

    /// <summary>
    /// Builds and returns a collision map from the parsed tilemap.
    /// Called once during <see cref="LoadContent"/> after <see cref="LoadTilemap"/>.
    /// </summary>
    protected abstract CollisionMap BuildCollisionMap(Tilemap tilemap);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            MediaPlayer.Stop();
            _song?.Dispose();
            Atlas?.Texture?.Dispose();
            TilemapRenderer?.Dispose();
        }
        base.Dispose(disposing);
    }
}
