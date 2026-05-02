using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace DemoGame.Shared;

/// <summary>
/// Result of a <see cref="KinematicBody.Move"/> call.
/// </summary>
public struct KinematicMoveResult
{
    public Vector2 Position;
    public Vector2 Velocity;
    public bool IsGrounded;
    public bool HitCeiling;
    public bool HitWall;
}

/// <summary>
/// Stateless kinematic movement resolver for tile-based platformers.
///
/// Algorithm:
///   1. Move X; resolve solid tile overlaps on the X axis.
///   2. Move Y; resolve solid and one-way tile overlaps on the Y axis.
///   3. Ground probe: 1 px strip below feet to detect resting contact.
///
/// Direction of velocity alone determines which face to resolve against.
/// When multiple tiles overlap, the shallowest penetration wins (min/max selection).
///
/// One-way (pass-through) platform rules:
///   - Ignored entirely when moving upward (velocity.Y &lt;= 0).
///   - Ignored when ignoreOneWay is true (drop-through input).
///   - Resolved when moving downward AND the actor's previous feet were
///     at or above the tile top (entered from above, not tunnelled from below).
/// </summary>
public static class KinematicBody
{
    // Reused per call to avoid allocations. Not thread-safe, but these demos are single-threaded.
    private static readonly List<TileHit> s_hits = new List<TileHit>();

    // When the player barely misses the top of a one-way tile from below, their feet can
    // peak a few pixels inside the tile before falling begins. This bias extends the
    // "was above tile top" snap condition so near-misses still land cleanly.
    // Applied only to the main Y snap, not the ground probe, to avoid false positives.
    private const float OneWaySnapBias = 4f;

    /// <summary>
    /// Moves <paramref name="position"/> by <c>velocity * dt</c> with tile collision resolution.
    /// </summary>
    public static KinematicMoveResult Move(Vector2 position, Vector2 velocity, RectangleF hitbox, CollisionMap collision, float dt, bool ignoreOneWay = false, float skinWidth = 0.5f)
    {
        KinematicMoveResult result = new KinematicMoveResult
        {
            Velocity = velocity
        };

        float newX = position.X + velocity.X * dt;

        if (velocity.X != 0f)
        {
            RectangleF worldBoxX = new RectangleF(
                newX + hitbox.X,
                position.Y + hitbox.Y,
                hitbox.Width,
                hitbox.Height);

            collision.GetOverlappingTiles(worldBoxX, s_hits);

            if (velocity.X > 0f)
            {
                // Moving right: snap the entity's right edge to the leftmost solid tile face.
                float minLeft = float.MaxValue;
                foreach (TileHit hit in s_hits)
                {
                    if (hit.Type == TileCollisionType.Solid && hit.Bounds.Left < minLeft)
                    {
                        minLeft = hit.Bounds.Left;
                    }
                }

                if (minLeft < float.MaxValue)
                {
                    newX = minLeft - hitbox.X - hitbox.Width - skinWidth;
                    result.Velocity.X = 0f;
                    result.HitWall = true;
                }
            }
            else
            {
                // Moving left: snap the entity's left edge to the rightmost solid tile face.
                float maxRight = float.MinValue;
                foreach (TileHit hit in s_hits)
                {
                    if (hit.Type == TileCollisionType.Solid && hit.Bounds.Right > maxRight)
                    {
                        maxRight = hit.Bounds.Right;
                    }
                }

                if (maxRight > float.MinValue)
                {
                    newX = maxRight - hitbox.X + skinWidth;
                    result.Velocity.X = 0f;
                    result.HitWall = true;
                }
            }
        }

        float worldRight = collision.Columns * collision.TileWidth;
        if (newX + hitbox.X < 0f)
        {
            newX = -hitbox.X;
            result.Velocity.X = 0f;
        }
        else if (newX + hitbox.X + hitbox.Width > worldRight)
        {
            newX = worldRight - hitbox.X - hitbox.Width;
            result.Velocity.X = 0f;
        }

        // Record feet position before Y integration for the one-way snap guard.
        float prevFeetY = position.Y + hitbox.Y + hitbox.Height;
        float newY = position.Y + velocity.Y * dt;

        {
            RectangleF worldBoxY = new RectangleF(
                newX + hitbox.X,
                newY + hitbox.Y,
                hitbox.Width,
                hitbox.Height);

            collision.GetOverlappingTiles(worldBoxY, s_hits);

            if (result.Velocity.Y > 0f)
            {
                // Falling: find the highest tile top across solid and qualifying one-way tiles.
                float minTop = float.MaxValue;

                foreach (TileHit hit in s_hits)
                {
                    if (hit.Type == TileCollisionType.Solid)
                    {
                        if (hit.Bounds.Top < minTop)
                        {
                            minTop = hit.Bounds.Top;
                        }
                    }
                    else if (hit.Type == TileCollisionType.OneWay && !ignoreOneWay)
                    {
                        // Only land on this tile if feet were at or above its surface before this
                        // frame (entered from above, not tunnelled from below). OneWaySnapBias
                        // covers the near-miss case where the player's peak sits a few pixels
                        // inside the tile before they start falling.
                        if (prevFeetY <= hit.Bounds.Top + skinWidth + OneWaySnapBias && hit.Bounds.Top < minTop)
                        {
                            minTop = hit.Bounds.Top;
                        }
                    }
                }

                if (minTop < float.MaxValue)
                {
                    newY = minTop - hitbox.Y - hitbox.Height - skinWidth;
                    result.Velocity.Y = 0f;
                    result.IsGrounded = true;
                }
            }
            else if (result.Velocity.Y < 0f)
            {
                // Rising: find the lowest ceiling bottom among solid tiles.
                float maxBottom = float.MinValue;
                foreach (TileHit hit in s_hits)
                {
                    if (hit.Type == TileCollisionType.Solid && hit.Bounds.Bottom > maxBottom)
                    {
                        maxBottom = hit.Bounds.Bottom;
                    }
                }

                if (maxBottom > float.MinValue)
                {
                    newY = maxBottom - hitbox.Y + skinWidth;
                    result.Velocity.Y = 0f;
                    result.HitCeiling = true;
                }
            }
        }

        float worldBottom = collision.Rows * collision.TileHeight;
        if (newY + hitbox.Y < 0f)
        {
            newY = -hitbox.Y;
            result.Velocity.Y = 0f;
        }
        else if (newY + hitbox.Y + hitbox.Height > worldBottom)
        {
            newY = worldBottom - hitbox.Y - hitbox.Height;
            result.Velocity.Y = 0f;
            result.IsGrounded = true;
        }

        // A 1 px strip below the entity's feet detects resting ground contact when
        // the entity is stationary or moving downward and no snap already fired.
        // Skipped when moving upward so a one-way tile being jumped through does not
        // falsely register as ground.
        //
        // For one-way tiles the same "from above" guard applies:
        //   probe.Y <= tile.Top + skinWidth
        // This prevents two bugs:
        //   (a) probe fires while the entity is still inside the tile after jumping up
        //       through it (probe.Y would be well below tile.Top).
        //   (b) probe fires against the bottom edge of a one-way tile that sits one
        //       row above the entity, because the probe rectangle straddles the row
        //       boundary (probe.Y approximately equals tile.Bottom of the tile above).
        if (!result.IsGrounded && result.Velocity.Y >= 0f)
        {
            RectangleF probe = new RectangleF(
                newX + hitbox.X + 1f,
                newY + hitbox.Y + hitbox.Height,
                hitbox.Width - 2f,
                1f);

            collision.GetOverlappingTiles(probe, s_hits);
            foreach (TileHit hit in s_hits)
            {
                if (hit.Type == TileCollisionType.Solid)
                {
                    result.IsGrounded = true;
                    break;
                }

                if (hit.Type == TileCollisionType.OneWay && !ignoreOneWay
                    && probe.Y <= hit.Bounds.Top + skinWidth)
                {
                    result.IsGrounded = true;
                    break;
                }
            }
        }

        result.Position = new Vector2(newX, newY);
        return result;
    }
}
