namespace DemoGame.Shared;

public enum TileCollisionType : byte
{
    None = 0,
    Solid = 1,

    // Passable from below; blocks downward movement from above.
    OneWay = 2
}
