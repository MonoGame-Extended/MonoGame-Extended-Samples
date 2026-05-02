using Autofac;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.Input;
using MonoGame.Extended.Tilemaps;
using MonoGame.Extended.Tilemaps.Rendering;
using Platformer.Systems;

namespace Platformer;

public class GameMain : GameBase
{
    private Tilemap _map = null!;
    private TilemapRenderer _renderer = null!;
    private EntityFactory _entityFactory = null!;
    private OrthographicCamera _camera = null!;
    private World _world = null!;

    protected override void RegisterDependencies(ContainerBuilder builder)
    {
        _camera = new OrthographicCamera(GraphicsDevice);

        builder.RegisterInstance(new SpriteBatch(GraphicsDevice));
        builder.RegisterInstance(_camera);
    }

    protected override void LoadContent()
    {
        _world = new WorldBuilder()
            .AddSystem(new WorldSystem())
            .AddSystem(new PlayerSystem())
            .AddSystem(new EnemySystem())
            .AddSystem(new RenderSystem(new SpriteBatch(GraphicsDevice), _camera))
            .Build();

        Components.Add(_world);

        _entityFactory = new EntityFactory(_world, Content);
        _map = Content.Load<Tilemap>("test-map");
        _renderer = new TilemapRenderer(GraphicsDevice);
        _renderer.LoadTilemap(_map);

        foreach (TilemapTileLayer tileLayer in _map.Layers.GetLayers<TilemapTileLayer>())
        {
            foreach (TilemapTileEntry tileEntry in tileLayer.GetTiles())
            {
                if (tileEntry.Tile.GlobalId == 1)
                {
                    _entityFactory.CreateTile(tileEntry.X, tileEntry.Y, _map.TileWidth, _map.TileHeight);
                }
            }
        }

        _entityFactory.CreateBlue(new Vector2(600, 240));
        _entityFactory.CreateBlue(new Vector2(700, 100));
        _entityFactory.CreatePlayer(new Vector2(100, 240));
    }

    protected override void Update(GameTime gameTime)
    {
        KeyboardExtended.Update();
        MouseExtended.Update();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        _renderer.Draw(_camera);

        base.Draw(gameTime);
    }
}
