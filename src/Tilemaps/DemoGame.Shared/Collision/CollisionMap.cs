using System;
using System.Collections.Generic;
using MonoGame.Extended;

namespace DemoGame.Shared;

/// <summary>
/// A tile hit returned by <see cref="CollisionMap.GetOverlappingTiles"/>.
/// </summary>
public readonly struct TileHit
{
    public readonly int Col;
    public readonly int Row;
    public readonly TileCollisionType Type;
    public readonly RectangleF Bounds;

    public TileHit(int col, int row, TileCollisionType type, int tileWidth, int tileHeight)
    {
        Col = col;
        Row = row;
        Type = type;
        Bounds = new RectangleF(col * tileWidth, row * tileHeight, tileWidth, tileHeight);
    }
}

/// <summary>
/// A grid of collision tiles for a tilemap level.
/// Provides O(k) spatial queries where k is the number of tiles overlapping
/// the query rectangle, not the total tile count.
/// </summary>
public sealed class CollisionMap
{
    // Indexed as [col, row].
    private readonly TileCollisionType[,] _tiles;

    public int Columns { get; }
    public int Rows { get; }
    public int TileWidth { get; }
    public int TileHeight { get; }

    public CollisionMap(int columns, int rows, int tileWidth, int tileHeight)
    {
        Columns = columns;
        Rows = rows;
        TileWidth = tileWidth;
        TileHeight = tileHeight;
        _tiles = new TileCollisionType[columns, rows];
    }

    public void SetTile(int col, int row, TileCollisionType type)
    {
        if (col >= 0 && col < Columns && row >= 0 && row < Rows)
        {
            _tiles[col, row] = type;
        }
    }

    public TileCollisionType GetTile(int col, int row)
    {
        if (col < 0 || col >= Columns || row < 0 || row >= Rows)
        {
            return TileCollisionType.None;
        }
        return _tiles[col, row];
    }

    /// <summary>
    /// Fills <paramref name="results"/> with every non-empty tile whose world bounds
    /// overlap <paramref name="worldRect"/>. Only tiles in the overlapping column/row
    /// range are examined.
    /// </summary>
    public void GetOverlappingTiles(RectangleF worldRect, List<TileHit> results)
    {
        results.Clear();

        int colMin = Math.Max(0, (int)Math.Floor(worldRect.Left / TileWidth));
        int colMax = Math.Min(Columns - 1, (int)Math.Floor((worldRect.Right - 0.001f) / TileWidth));
        int rowMin = Math.Max(0, (int)Math.Floor(worldRect.Top / TileHeight));
        int rowMax = Math.Min(Rows - 1, (int)Math.Floor((worldRect.Bottom - 0.001f) / TileHeight));

        for (int r = rowMin; r <= rowMax; r++)
        {
            for (int c = colMin; c <= colMax; c++)
            {
                TileCollisionType type = _tiles[c, r];
                if (type != TileCollisionType.None)
                {
                    results.Add(new TileHit(c, r, type, TileWidth, TileHeight));
                }
            }
        }
    }

    /// <summary>
    /// Returns true if any Solid tile overlaps <paramref name="worldRect"/>.
    /// Used for wall and ledge checks where one-way tiles do not block.
    /// </summary>
    public bool OverlapsSolid(RectangleF worldRect)
    {
        int colMin = Math.Max(0, (int)Math.Floor(worldRect.Left / TileWidth));
        int colMax = Math.Min(Columns - 1, (int)Math.Floor((worldRect.Right - 0.001f) / TileWidth));
        int rowMin = Math.Max(0, (int)Math.Floor(worldRect.Top / TileHeight));
        int rowMax = Math.Min(Rows - 1, (int)Math.Floor((worldRect.Bottom - 0.001f) / TileHeight));

        for (int r = rowMin; r <= rowMax; r++)
        {
            for (int c = colMin; c <= colMax; c++)
            {
                if (_tiles[c, r] == TileCollisionType.Solid)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Returns true if any Solid or OneWay tile overlaps <paramref name="worldRect"/>.
    /// Used for ground-presence checks (e.g. ledge detection where one-way tiles count as ground).
    /// </summary>
    public bool OverlapsGround(RectangleF worldRect)
    {
        int colMin = Math.Max(0, (int)Math.Floor(worldRect.Left / TileWidth));
        int colMax = Math.Min(Columns - 1, (int)Math.Floor((worldRect.Right - 0.001f) / TileWidth));
        int rowMin = Math.Max(0, (int)Math.Floor(worldRect.Top / TileHeight));
        int rowMax = Math.Min(Rows - 1, (int)Math.Floor((worldRect.Bottom - 0.001f) / TileHeight));

        for (int r = rowMin; r <= rowMax; r++)
        {
            for (int c = colMin; c <= colMax; c++)
            {
                if (_tiles[c, r] != TileCollisionType.None)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
