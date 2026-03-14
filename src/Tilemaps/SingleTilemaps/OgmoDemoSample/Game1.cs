using System.Text.Json;
using DemoGame.Shared;
using MonoGame.Extended.Tilemaps;

namespace OgmoDemoSample;

public class Game1 : DemoGameBase
{
    public override string FormatName => "Ogmo";
    public override string EntityLayerName => "Entities";
    public override string PlayerSpawnName => "PLAYER_START";

    protected override Tilemap LoadTilemap()
    {
        return Content.Load<Tilemap>("project");
    }

    protected override CollisionMap BuildCollisionMap(Tilemap tilemap)
    {
        TilemapTileLayer? collisionLayer = null;
        foreach (TilemapLayer layer in tilemap.Layers)
        {
            if (layer is TilemapTileLayer tl && layer.Name == "Collisions")
            {
                collisionLayer = tl;
                break;
            }
        }

        if (collisionLayer == null)
        {
            throw new System.InvalidOperationException("Collisions layer not found in level.");
        }

        // The Ogmo content pipeline importer stores grid values as a JSON string array
        // in a custom layer property named "Ogmo_GridValues": "1" = Solid, "2" = OneWay.
        CollisionMap map = new CollisionMap(collisionLayer.Width, collisionLayer.Height, collisionLayer.TileWidth, collisionLayer.TileHeight);
        string gridJson = collisionLayer.Properties.GetString("Ogmo_GridValues");
        if (!string.IsNullOrEmpty(gridJson))
        {
            string[]? values = JsonSerializer.Deserialize<string[]>(gridJson);
            if (values != null)
            {
                for (int i = 0; i < values.Length && i < collisionLayer.Width * collisionLayer.Height; i++)
                {
                    int col = i % collisionLayer.Width;
                    int row = i / collisionLayer.Width;
                    if (values[i] == "1") map.SetTile(col, row, TileCollisionType.Solid);
                    else if (values[i] == "2") map.SetTile(col, row, TileCollisionType.OneWay);
                }
            }
        }

        return map;
    }
}
