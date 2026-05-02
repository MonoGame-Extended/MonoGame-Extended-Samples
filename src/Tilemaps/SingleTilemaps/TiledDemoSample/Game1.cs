using DemoGame.Shared;
using MonoGame.Extended.Tilemaps;

namespace TiledDemoSample;

public class Game1 : DemoGameBase
{
    public override string FormatName => "Tiled";

    protected override Tilemap LoadTilemap()
    {
        return Content.Load<Tilemap>("level");
    }

    protected override CollisionMap BuildCollisionMap(Tilemap tilemap)
    {
        TilemapTileLayer? tileLayer = null;
        foreach (TilemapLayer layer in tilemap.Layers)
        {
            if (layer is TilemapTileLayer tl && layer.Name == "Tiles")
            {
                tileLayer = tl;
                break;
            }
        }

        if (tileLayer == null)
        {
            throw new System.InvalidOperationException("Tiles layer not found in level.");
        }

        // Each tile's collision type is stored as a custom property "CollisionType" (int):
        // 1 = Solid, 2 = OneWay. The property may be on the tile data or on a collision object.
        CollisionMap map = new CollisionMap(tileLayer.Width, tileLayer.Height, tileLayer.TileWidth, tileLayer.TileHeight);
        foreach ((int x, int y, TilemapTile tile) in tileLayer.GetTiles())
        {
            TilemapTileData? tileData = tile.GetTileData(tilemap.Tilesets);
            if (tileData == null) continue;

            int ct = tileData.Properties.GetInt("CollisionType", 0);
            if (ct == 0)
            {
                foreach (TilemapObject obj in tileData.CollisionObjects)
                {
                    ct = obj.Properties.GetInt("CollisionType", 0);
                    if (ct != 0) break;
                }
            }

            if (ct == 1) map.SetTile(x, y, TileCollisionType.Solid);
            else if (ct == 2) map.SetTile(x, y, TileCollisionType.OneWay);
        }

        return map;
    }
}
