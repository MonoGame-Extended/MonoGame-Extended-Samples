using System;
using System.Collections.Generic;
using DemoGame.Shared.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Tilemaps;
using MonoGame.Extended.Tilemaps.Rendering;
using MonoGame.Extended.ViewportAdapters;

namespace DemoGame.Shared;

public class PlayScreen : GameScreen
{
    // A position and animated sprite pair used for one-shot death and pickup animations.
    private record struct OneShot(Vector2 Position, AnimatedSprite Sprite);

    private readonly ScreenManager _screenManager;

    private BoxingViewportAdapter _viewportAdapter = null!;
    private SpriteBatch _spriteBatch = null!;
    private OrthographicCamera _camera = null!;

    private Tilemap _tilemap = null!;
    private TilemapRenderer _tilemapRenderer = null!;

    private Texture2DAtlas _atlas = null!;
    private SpriteSheet _spriteSheet = null!;
    private Dictionary<string, RectangleF> _hitboxes = null!;

    // Background regions for parallax layers.
    private Texture2DRegion _backRegion = null!;
    private Texture2DRegion _middleRegion = null!;

    // Sky color fallback: #68C2D3. Used when the tilemap has no BackgroundColor set.
    private Color _skyColor = new Color(0x68, 0xC2, 0xD3);

    private CollisionMap _collisionMap = null!;

    private Player _player = null!;
    private readonly List<EagleEnemy> _eagles = new List<EagleEnemy>();
    private readonly List<OpossumEnemy> _opossums = new List<OpossumEnemy>();
    private readonly List<FrogEnemy> _frogs = new List<FrogEnemy>();
    private readonly List<Collectible> _collectibles = new List<Collectible>();
    private readonly List<OneShot> _feedbacks = new List<OneShot>();
    private readonly List<OneShot> _deaths = new List<OneShot>();

    private int _levelWidth;
    private int _levelHeight;

    // All frogs jump simultaneously every 2 seconds.
    private float _frogJumpTimer = 2.0f;
    private bool _frogGoRight;
    private KeyboardState _previousKeys;

    public PlayScreen(Game game, ScreenManager screenManager) : base(game)
    {
        _screenManager = screenManager;
    }

    public override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _viewportAdapter = new BoxingViewportAdapter(Game.Window, GraphicsDevice, DemoGameBase.VirtualWidth, DemoGameBase.VirtualHeight);
        _viewportAdapter.Reset();
        _camera = new OrthographicCamera(_viewportAdapter);

        DemoGameBase game = (DemoGameBase)Game;
        _atlas = game.Atlas;
        _spriteSheet = game.SpriteSheet;
        _hitboxes = game.Hitboxes;

        _backRegion = _atlas["back.png"];
        _middleRegion = _atlas["middle.png"];

        _tilemap = game.Tilemap;
        _tilemapRenderer = game.TilemapRenderer;
        _collisionMap = game.CollisionMap;

        _levelWidth = _tilemap.Width * _tilemap.TileWidth;
        _levelHeight = _tilemap.Height * _tilemap.TileHeight;

        // Use the tilemap's background color when available (LDtk sets this per-level).
        if (_tilemap.BackgroundColor.HasValue)
        {
            _skyColor = _tilemap.BackgroundColor.Value;
        }

        SpawnEntities(game);

        _camera.EnableWorldBounds(new Rectangle(0, 0, _levelWidth, _levelHeight));
    }

    private void SpawnEntities(DemoGameBase game)
    {
        TilemapObjectLayer? entityLayer = null;
        foreach (TilemapLayer layer in _tilemap.Layers)
        {
            if (layer is TilemapObjectLayer ol && ol.Name == game.EntityLayerName)
            {
                entityLayer = ol;
                break;
            }
        }

        if (entityLayer == null)
        {
            return;
        }

        Vector2 playerSpawn = new Vector2(64f, 80f);

        foreach (TilemapObject obj in entityLayer.Objects)
        {
            Vector2 pos = obj.Position;

            switch (obj.Name)
            {
                case var name when name == game.PlayerSpawnName:
                    // Sprite is 32px tall; spawn object is 16px - shift up 16px so feet align.
                    playerSpawn = new Vector2(pos.X, pos.Y - 16f);
                    break;

                case "EAGLE":
                    _eagles.Add(new EagleEnemy(pos, _spriteSheet));
                    break;

                case "OPOSSUM":
                    // Sprite is 28px tall; spawn object is 16px - shift up 12px so feet align.
                    _opossums.Add(new OpossumEnemy(new Vector2(pos.X, pos.Y - 12f), _spriteSheet, _collisionMap, _hitboxes));
                    break;

                case "FROG":
                    // Sprite is 32px tall; spawn object is 16px - shift up 16px so feet align.
                    _frogs.Add(new FrogEnemy(new Vector2(pos.X, pos.Y - 16f), _spriteSheet, _collisionMap, _hitboxes));
                    break;

                case "CHERRY":
                    _collectibles.Add(new Collectible(pos, CollectibleType.Cherry, _spriteSheet));
                    break;

                case "GEM":
                    _collectibles.Add(new Collectible(pos, CollectibleType.Gem, _spriteSheet));
                    break;
            }
        }

        _player = new Player(playerSpawn, _spriteSheet, _collisionMap, _hitboxes);
        Vector2 spawnLookAt = _player.Position + new Vector2(16f, 16f);
        _camera.LookAt(new Vector2(MathF.Floor(spawnLookAt.X), MathF.Floor(spawnLookAt.Y)));
    }

    public override void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        KeyboardState keys = Keyboard.GetState();

        if (keys.IsKeyDown(Keys.Escape) && !_previousKeys.IsKeyDown(Keys.Escape))
        {
            _screenManager.ReplaceScreen(new TitleScreen(Game, _screenManager));
            return;
        }

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
        {
            Game.Exit();
        }

        _previousKeys = keys;

        // All frogs jump simultaneously every 2 seconds.
        _frogJumpTimer -= dt;
        if (_frogJumpTimer <= 0f)
        {
            _frogGoRight = !_frogGoRight;
            _frogJumpTimer = 2.0f;
            foreach (FrogEnemy frog in _frogs)
            {
                frog.ReceiveJumpSignal(_frogGoRight);
            }
        }

        _player.Update(gameTime);
        foreach (EagleEnemy eagle in _eagles)
        {
            eagle.Update(gameTime);
        }

        foreach (OpossumEnemy opossum in _opossums)
        {
            opossum.Update(gameTime);
        }

        foreach (FrogEnemy frog in _frogs)
        {
            frog.Update(gameTime);
        }

        RectangleF playerHitbox = _player.WorldHitbox;
        foreach (Collectible c in _collectibles)
        {
            c.Update(gameTime);
            if (c.TryCollect(playerHitbox))
            {
                AnimatedSprite feedback = new AnimatedSprite(_spriteSheet, "item-feedback");
                _feedbacks.Add(new OneShot(c.Position, feedback));
            }
        }

        for (int i = _feedbacks.Count - 1; i >= 0; i--)
        {
            _feedbacks[i].Sprite.Update(gameTime);
            if (!_feedbacks[i].Sprite.Controller.IsAnimating)
            {
                _feedbacks.RemoveAt(i);
            }
        }

        CheckPlayerEnemyCollisions();

        _eagles.RemoveAll(e => !e.IsAlive);
        _opossums.RemoveAll(o => !o.IsAlive);
        _frogs.RemoveAll(f => !f.IsAlive);

        for (int i = _deaths.Count - 1; i >= 0; i--)
        {
            _deaths[i].Sprite.Update(gameTime);
            if (!_deaths[i].Sprite.Controller.IsAnimating)
            {
                _deaths.RemoveAt(i);
            }
        }

        // Snap to integer virtual pixels so tile vertices land on exact screen-pixel
        // boundaries at the 3x viewport scale, preventing seams between tiles.
        Vector2 lookAt = _player.Position + new Vector2(16f, 16f);
        _camera.LookAt(new Vector2(MathF.Floor(lookAt.X), MathF.Floor(lookAt.Y)));

        _tilemapRenderer.Update(gameTime);
    }

    private void CheckPlayerEnemyCollisions()
    {
        RectangleF playerBox = _player.WorldHitbox;

        foreach (EagleEnemy eagle in _eagles)
        {
            if (!eagle.IsAlive) continue;
            RectangleF enemyBox = eagle.WorldHitbox;
            if (!playerBox.Intersects(enemyBox)) continue;

            if (_player.Velocity.Y > 0f && playerBox.Y + playerBox.Height * 0.5f < enemyBox.Y)
            {
                eagle.Kill();
                SpawnDeathAnimation(eagle.Position);
                _player.StompBounce();
            }
            else if (!_player.IsHurt)
            {
                _player.Hurt(_player.FacingRight ? HorizontalDirection.Left : HorizontalDirection.Right);
            }
        }

        foreach (OpossumEnemy opossum in _opossums)
        {
            if (!opossum.IsAlive) continue;
            RectangleF enemyBox = opossum.WorldHitbox;
            if (!playerBox.Intersects(enemyBox)) continue;

            if (_player.Velocity.Y > 0f && playerBox.Y + playerBox.Height * 0.5f < enemyBox.Y)
            {
                opossum.Kill();
                SpawnDeathAnimation(opossum.Position);
                _player.StompBounce();
            }
            else if (!_player.IsHurt)
            {
                _player.Hurt(_player.FacingRight ? HorizontalDirection.Left : HorizontalDirection.Right);
            }
        }

        foreach (FrogEnemy frog in _frogs)
        {
            if (!frog.IsAlive) continue;
            RectangleF enemyBox = frog.WorldHitbox;
            if (!playerBox.Intersects(enemyBox)) continue;

            if (_player.Velocity.Y > 0f && playerBox.Y + playerBox.Height * 0.5f < enemyBox.Y)
            {
                frog.Kill();
                SpawnDeathAnimation(frog.Position);
                _player.StompBounce();
            }
            else if (!_player.IsHurt)
            {
                _player.Hurt(_player.FacingRight ? HorizontalDirection.Left : HorizontalDirection.Right);
            }
        }
    }

    private void SpawnDeathAnimation(Vector2 position)
    {
        AnimatedSprite sprite = new AnimatedSprite(_spriteSheet, "enemy-death");
        sprite.Controller.Play();
        _deaths.Add(new OneShot(position, sprite));
    }

    public override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(_skyColor);

        // 1. Parallax sky/clouds (back.png)
        DrawParallaxLayer(_backRegion, parallaxX: 0.3f, parallaxY: 0.5f, worldY: 0f);

        // 2. Parallax hills/trees (middle.png)
        DrawParallaxLayer(_middleRegion, parallaxX: 0.6f, parallaxY: 0.8f, worldY: 0f);

        // 3. Tilemap
        _tilemapRenderer.Draw(_camera);

        // 4. World-space sprites
        _spriteBatch.Begin(
            sortMode: SpriteSortMode.Deferred,
            blendState: BlendState.AlphaBlend,
            samplerState: SamplerState.PointClamp,
            transformMatrix: _camera.GetViewMatrix());

        DrawProp("tree.png", 31 * 16, 2 * 16 + 3);
        DrawProp("house.png", 48 * 16, 1 * 16 + 5);
        DrawProp("bush.png", 10 * 16, 6 * 16 + 4);
        DrawProp("sign.png", 11 * 16, 17 * 16 - 4);
        DrawProp("skulls.png", 15 * 16, 17 * 16 + 6);
        DrawProp("face-block.png", 23 * 16, 17 * 16);
        DrawProp("shrooms.png", 28 * 16, 18 * 16);

        foreach (Collectible collectible in _collectibles)
        {
            collectible.Draw(_spriteBatch);
        }

        foreach (OneShot death in _deaths)
        {
            _spriteBatch.Draw(
                death.Sprite.TextureRegion.Texture,
                death.Position,
                death.Sprite.TextureRegion.Bounds,
                Color.White,
                0f,
                Vector2.Zero,
                Vector2.One,
                SpriteEffects.None,
                0f
            );
        }

        foreach (OneShot feedback in _feedbacks)
        {
            _spriteBatch.Draw(
                feedback.Sprite.TextureRegion.Texture,
                feedback.Position,
                feedback.Sprite.TextureRegion.Bounds,
                Color.White,
                0f,
                Vector2.Zero,
                Vector2.One,
                SpriteEffects.None,
                0f
            );
        }

        foreach (FrogEnemy frog in _frogs)
        {
            frog.Draw(_spriteBatch);
        }

        foreach (OpossumEnemy opossum in _opossums)
        {
            opossum.Draw(_spriteBatch);
        }

        foreach (EagleEnemy eagle in _eagles)
        {
            eagle.Draw(_spriteBatch, _player.Position);
        }

        _player.Draw(_spriteBatch);

        _spriteBatch.End();
    }

    private void DrawProp(string regionName, int x, int y)
    {
        Texture2DRegion region = _atlas[regionName];
        _spriteBatch.Draw(
            region.Texture,
            new Vector2(x, y),
            region.Bounds,
            Color.White,
            0f,
            Vector2.Zero,
            Vector2.One,
            SpriteEffects.None,
            0f
        );
    }

    private void DrawParallaxLayer(Texture2DRegion region, float parallaxX, float parallaxY, float worldY)
    {
        Matrix parallaxMatrix = _camera.GetViewMatrix(new Vector2(parallaxX, parallaxY));

        _spriteBatch.Begin(
            sortMode: SpriteSortMode.Deferred,
            blendState: BlendState.AlphaBlend,
            samplerState: SamplerState.PointClamp,
            transformMatrix: parallaxMatrix);

        int tilesNeeded = (int)Math.Ceiling((float)_levelWidth / region.Width) + 2;
        for (int i = -1; i < tilesNeeded; i++)
        {
            _spriteBatch.Draw(
                region.Texture,
                new Vector2(i * region.Width, worldY),
                region.Bounds,
                Color.White, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
        }

        _spriteBatch.End();
    }

    public override void UnloadContent()
    {
        // _tilemapRenderer is owned by DemoGameBase, do not disposed here.
        _viewportAdapter?.Dispose();
        _spriteBatch?.Dispose();
    }
}
