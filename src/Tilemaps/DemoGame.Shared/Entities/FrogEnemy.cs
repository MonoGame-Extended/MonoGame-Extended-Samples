using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Graphics;

namespace DemoGame.Shared.Entities;

/// <summary>
/// A frog that waits for a global jump signal from PlayScreen (every 2 seconds),
/// then jumps alternately right and left.
/// </summary>
public class FrogEnemy
{
    private static readonly RectangleF DefaultHitbox = new RectangleF(8f, 11f, 16f, 16f);

    private const float JumpVelocityX = 100f;
    private const float JumpVelocityY = -200f;
    private const float Gravity = 500f;
    private const float MaxFallSpeed = 600f;

    private enum FrogState { Idle, JumpingRight, JumpingLeft }
    private FrogState _state = FrogState.Idle;

    public Vector2 Position;
    public bool IsAlive { get; private set; } = true;
    private Vector2 _velocity;
    private bool _onGround = true;

    // Starts facing right; flips on each landing to anticipate the next jump direction.
    private bool _facingRight = true;

    private readonly AnimatedSprite _sprite;
    private readonly CollisionMap _collision;
    private readonly Dictionary<string, RectangleF> _hitboxes;

    public FrogEnemy(Vector2 spawnPosition, SpriteSheet spriteSheet, CollisionMap collision, Dictionary<string, RectangleF> hitboxes)
    {
        Position = spawnPosition;
        _collision = collision;
        _hitboxes = hitboxes;

        _sprite = new AnimatedSprite(spriteSheet, "frog-idle");
        _sprite.Controller.Play();
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

    /// <summary>
    /// Called by PlayScreen every 2 seconds to signal all frogs to jump.
    /// The frog only responds when idle on the ground.
    /// </summary>
    public void ReceiveJumpSignal(bool goRight)
    {
        if (!IsAlive || _state != FrogState.Idle || !_onGround)
        {
            return;
        }

        _state = goRight ? FrogState.JumpingRight : FrogState.JumpingLeft;
        _facingRight = goRight;
        _velocity.X = goRight ? JumpVelocityX : -JumpVelocityX;
        _velocity.Y = JumpVelocityY;
        _onGround = false;
        _sprite.SetAnimation("frog-jump").Play();
    }

    public void Update(GameTime gameTime)
    {
        if (!IsAlive)
        {
            return;
        }

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        switch (_state)
        {
            case FrogState.Idle:
                break;

            case FrogState.JumpingRight:
            case FrogState.JumpingLeft:
                _velocity.Y = Math.Min(_velocity.Y + Gravity * dt, MaxFallSpeed);

                if (_velocity.Y >= 0f && _sprite.CurrentAnimation == "frog-jump")
                {
                    _sprite.SetAnimation("frog-fall").Play();
                }

                KinematicMoveResult move = KinematicBody.Move(
                    Position,
                    _velocity,
                    CurrentHitbox,
                    _collision,
                    dt);

                Position = move.Position;
                _velocity = move.Velocity;
                _onGround = move.IsGrounded;

                if (_onGround && _velocity.Y >= 0f)
                {
                    _velocity = Vector2.Zero;
                    // Flip to anticipate the opposite direction for the next jump.
                    _facingRight = !_facingRight;
                    _state = FrogState.Idle;
                    _sprite.SetAnimation("frog-idle").Play();
                }
                break;
        }

        _sprite.Update(gameTime);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (!IsAlive)
        {
            return;
        }

        // Sprite faces left in the atlas; flip when facing right.
        SpriteEffects effects = _facingRight ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

        spriteBatch.Draw(
            _sprite.TextureRegion.Texture,
            Position,
            _sprite.TextureRegion.Bounds,
            Color.White, 0f, Vector2.Zero, Vector2.One, effects, 0f);
    }
}
