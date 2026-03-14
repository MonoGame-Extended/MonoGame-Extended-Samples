using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Graphics;

namespace DemoGame.Shared.Entities;

public enum CollectibleType { Cherry, Gem }

public class Collectible
{
    private static readonly RectangleF PickupRadius = new RectangleF(-2f, -2f, 20f, 20f);

    public Vector2 Position;
    public CollectibleType Type;
    public bool IsCollected { get; private set; }

    private readonly AnimatedSprite _sprite;

    public Collectible(Vector2 position, CollectibleType type, SpriteSheet spriteSheet)
    {
        Position = position;
        Type = type;

        string animName = type == CollectibleType.Cherry ? "cherry" : "gem";
        _sprite = new AnimatedSprite(spriteSheet, animName);
        _sprite.Controller.Play();
    }

    public void Update(GameTime gameTime)
    {
        if (!IsCollected)
            _sprite.Update(gameTime);
    }

    /// <summary>
    /// Returns true the frame the item is first collected, false otherwise.
    /// </summary>
    public bool TryCollect(RectangleF playerHitbox)
    {
        if (IsCollected) return false;

        RectangleF bounds = new RectangleF(
            Position.X + PickupRadius.X,
            Position.Y + PickupRadius.Y,
            PickupRadius.Width,
            PickupRadius.Height);

        if (!bounds.Intersects(playerHitbox)) return false;

        IsCollected = true;
        return true;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (IsCollected) return;

        spriteBatch.Draw(
            _sprite.TextureRegion.Texture,
            Position,
            _sprite.TextureRegion.Bounds,
            Color.White, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
    }
}
