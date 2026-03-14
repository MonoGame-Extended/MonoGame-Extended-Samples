using DemoGame.Shared;
using MonoGame.Extended.Tilemaps;

namespace LDtkDemoSample;

public class Game1 : DemoGameBase
{
    public override string FormatName => "LDtk";
    public override string EntityLayerName => "GAMEPLAY_OBJECTS";

    protected override Tilemap LoadTilemap()
    {
        return Content.Load<Tilemap>("level");
    }

    protected override CollisionMap BuildCollisionMap(Tilemap tilemap)
    {
        TilemapDataLayer? collisionLayer = null;
        foreach (TilemapLayer layer in tilemap.Layers)
        {
            if (layer is TilemapDataLayer dl && layer.Name == "Collisions")
            {
                collisionLayer = dl;
                break;
            }
        }

        if (collisionLayer == null)
        {
            throw new System.InvalidOperationException("Collisions layer not found in level.");
        }

        // The LDtk content pipeline importer stores the IntGrid values as a CSV string
        // in a custom layer property named "LDtk_IntGridCsv": 1 = Solid, 2 = OneWay.
        CollisionMap map = new CollisionMap(collisionLayer.Width, collisionLayer.Height, collisionLayer.TileWidth, collisionLayer.TileHeight);

        if (collisionLayer.Properties.TryGetValue("LDtk_IntGridCsv", out TilemapPropertyValue csvValue))
        {
            string csv = csvValue.AsString();
            if (!string.IsNullOrEmpty(csv))
            {
                string[] parts = csv.Split(',');
                for (int i = 0; i < parts.Length; i++)
                {
                    if (!int.TryParse(parts[i].Trim(), out int val)) continue;
                    int col = i % collisionLayer.Width;
                    int row = i / collisionLayer.Width;
                    if (val == 1) map.SetTile(col, row, TileCollisionType.Solid);
                    else if (val == 2) map.SetTile(col, row, TileCollisionType.OneWay);
                }
            }
        }

        return map;
    }
}
