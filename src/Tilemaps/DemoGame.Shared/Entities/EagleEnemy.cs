using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Graphics;

namespace DemoGame.Shared.Entities;

public class EagleEnemy
{
    private enum VerticalDirection { Down, Up }

    private static readonly RectangleF DefaultHitbox = new RectangleF(8f, 20f, 16f, 13f);

    private const float Speed = 70f;

    // Pixels traveled each direction from the spawn point.
    private const float TravelHalf = 56f;

    public Vector2 Position;
    public bool IsAlive { get; private set; } = true;

    private readonly float _originY;
    private VerticalDirection _direction = VerticalDirection.Down;

    private readonly AnimatedSprite _sprite;

    public RectangleF WorldHitbox
    {
        get
        {
            RectangleF hb = DefaultHitbox;
            return new RectangleF(Position.X + hb.X, Position.Y + hb.Y, hb.Width, hb.Height);
        }
    }

    public void Kill() => IsAlive = false;

    public EagleEnemy(Vector2 spawnPosition, SpriteSheet spriteSheet)
    {
        Position = spawnPosition;
        _originY = spawnPosition.Y;

        _sprite = new AnimatedSprite(spriteSheet, "eagle-attack");
        _sprite.Controller.Play();
    }

    public void Update(GameTime gameTime)
    {
        if (!IsAlive)
        {
            return;
        }

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        float dirSign = _direction == VerticalDirection.Down ? 1f : -1f;

        Position.Y += Speed * dirSign * dt;

        if (Position.Y > _originY + TravelHalf)
        {
            Position.Y = _originY + TravelHalf;
            _direction = VerticalDirection.Up;
        }
        else if (Position.Y < _originY - TravelHalf)
        {
            Position.Y = _originY - TravelHalf;
            _direction = VerticalDirection.Down;
        }

        _sprite.Update(gameTime);
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 playerPosition)
    {
        if (!IsAlive)
        {
            return;
        }

        // Sprite faces left; flip when the player is to the right so the eagle always faces the player.
        SpriteEffects effects = playerPosition.X > Position.X
            ? SpriteEffects.FlipHorizontally
            : SpriteEffects.None;

        spriteBatch.Draw(
            _sprite.TextureRegion.Texture,
            Position,
            _sprite.TextureRegion.Bounds,
            Color.White, 0f, Vector2.Zero, Vector2.One, effects, 0f);
    }
}
