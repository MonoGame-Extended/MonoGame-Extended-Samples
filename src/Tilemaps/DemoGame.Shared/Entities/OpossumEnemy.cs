using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Graphics;

namespace DemoGame.Shared.Entities;

public class OpossumEnemy
{
    private static readonly RectangleF DefaultHitbox = new RectangleF(8f, 15f, 16f, 13f);

    private const float Speed = 60f;

    public Vector2 Position;
    public bool IsAlive { get; private set; } = true;
    private HorizontalDirection _direction = HorizontalDirection.Left;

    private readonly AnimatedSprite _sprite;
    private readonly CollisionMap _collision;
    private readonly Dictionary<string, RectangleF> _hitboxes;

    public OpossumEnemy(Vector2 spawnPosition, SpriteSheet spriteSheet, CollisionMap collision, Dictionary<string, RectangleF> hitboxes)
    {
        Position = spawnPosition;
        _collision = collision;
        _hitboxes = hitboxes;

        _sprite = new AnimatedSprite(spriteSheet, "opossum");
        _sprite.Controller.Play();

        // Validate initial direction before the first Draw call.
        // The ScreenManager may draw the new screen before its first Update runs,
        // so the wall/ledge check must also run here to ensure correct facing from frame zero.
        RectangleF hb = CurrentHitbox;
        float frontEdgeX = _direction == HorizontalDirection.Right
            ? Position.X + hb.X + hb.Width + 1f
            : Position.X + hb.X - 1f;
        RectangleF groundCheck = new RectangleF(frontEdgeX, Position.Y + hb.Y + hb.Height + 1f, 2f, 2f);
        if (!_collision.OverlapsGround(groundCheck))
        {
            _direction = _direction == HorizontalDirection.Right ? HorizontalDirection.Left : HorizontalDirection.Right;
        }
    }

    private RectangleF CurrentHitbox
    {
        get
        {
            if (_hitboxes.TryGetValue(_sprite.TextureRegion.Name, out RectangleF hb))
            {
                return hb;
            }
            return DefaultHitbox;
        }
    }

    public RectangleF WorldHitbox
    {
        get
        {
            RectangleF hb = CurrentHitbox;
            return new RectangleF(Position.X + hb.X, Position.Y + hb.Y, hb.Width, hb.Height);
        }
    }

    public void Kill() => IsAlive = false;

    public void Update(GameTime gameTime)
    {
        if (!IsAlive)
        {
            return;
        }

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        RectangleF hitbox = CurrentHitbox;
        float dirSign = _direction == HorizontalDirection.Right ? 1f : -1f;

        float newX = Position.X + Speed * dirSign * dt;

        RectangleF nextBox = new RectangleF(newX + hitbox.X, Position.Y + hitbox.Y, hitbox.Width, hitbox.Height);
        if (_collision.OverlapsSolid(nextBox))
        {
            _direction = _direction == HorizontalDirection.Right ? HorizontalDirection.Left : HorizontalDirection.Right;
            newX = Position.X;
        }
        else
        {
            float frontEdgeX = _direction == HorizontalDirection.Right
                ? newX + hitbox.X + hitbox.Width + 1f
                : newX + hitbox.X - 1f;

            RectangleF groundCheck = new RectangleF(frontEdgeX, Position.Y + hitbox.Y + hitbox.Height + 1f, 2f, 2f);
            if (!_collision.OverlapsGround(groundCheck))
            {
                _direction = _direction == HorizontalDirection.Right ? HorizontalDirection.Left : HorizontalDirection.Right;
                newX = Position.X;
            }
        }

        Position.X = newX;
        _sprite.Update(gameTime);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (!IsAlive)
        {
            return;
        }

        // Sprite faces left in the atlas; flip when moving right.
        SpriteEffects effects = _direction == HorizontalDirection.Right ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

        spriteBatch.Draw(
            _sprite.TextureRegion.Texture,
            Position,
            _sprite.TextureRegion.Bounds,
            Color.White, 0f, Vector2.Zero, Vector2.One, effects, 0f);
    }
}
