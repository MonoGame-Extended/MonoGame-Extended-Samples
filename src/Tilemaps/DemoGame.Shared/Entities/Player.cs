using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Graphics;

namespace DemoGame.Shared.Entities;

public class Player
{
    private const float Gravity = 500f;
    private const float JumpVelocity = -170f;
    private const float MoveSpeed = 150f;
    private const float MaxFallSpeed = 600f;
    private const float HurtDuration = 0.5f;
    private const float HurtKnockbackY = -100f;
    private const float HurtKnockbackX = 100f;
    private const float StompBounceVelocity = -200f;

    // Seconds after leaving a ledge where a jump is still allowed.
    private const float CoyoteTime = 0.50f;

    // Seconds before landing where a pressed jump is remembered and fires on touch.
    private const float JumpBufferTime = 0.10f;

    // Seconds that one-way platforms are ignored when drop-through is triggered.
    private const float DropThroughTime = 0.20f;

    private static readonly RectangleF DefaultHitbox = new RectangleF(8f, 16f, 12f, 16f);

    public Vector2 Position;
    public Vector2 Velocity;

    public bool IsOnGround { get; private set; }

    // Sprite artwork in the atlas faces right; false here means the player starts facing left.
    public bool FacingRight { get; private set; } = false;
    public bool IsAlive { get; private set; } = true;
    public bool IsHurt { get; private set; }

    private float _hurtTimer;

    // Counts down after leaving ground without jumping, allowing a late jump.
    private float _coyoteTimer;

    // Counts down after jump is pressed in the air; fires the jump on the next grounded frame.
    private float _jumpBufferTimer;

    // Counts down while drop-through is active; keeps one-way tiles passable.
    private float _dropTimer;

    private bool _wasGrounded;

    // Prevents coyote time from re-triggering on the same takeoff.
    private bool _jumpConsumed;

    private enum PlayerState { Idle, Run, Jump, Fall, Crouch }
    private PlayerState _state = PlayerState.Idle;
    private PlayerState _previousState = PlayerState.Idle;

    private readonly AnimatedSprite _sprite;
    private readonly CollisionMap _collision;
    private readonly Dictionary<string, RectangleF> _hitboxes;

    public Player(Vector2 spawnPosition, SpriteSheet spriteSheet, CollisionMap collision, Dictionary<string, RectangleF> hitboxes)
    {
        Position = spawnPosition;
        _collision = collision;
        _hitboxes = hitboxes;

        _sprite = new AnimatedSprite(spriteSheet, "player-foxy-idle");
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

    public void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (IsHurt)
        {
            _hurtTimer -= dt;
            if (_hurtTimer <= 0f)
            {
                IsHurt = false;

                // Force animation refresh on the next state evaluation.
                _previousState = (PlayerState)(-1);
            }
        }

        bool left = false;
        bool right = false;
        bool jumpPressed = false;
        bool crouch = false;

        if (!IsHurt)
        {
            KeyboardState keys = Keyboard.GetState();
            left = keys.IsKeyDown(Keys.Left) || keys.IsKeyDown(Keys.A);
            right = keys.IsKeyDown(Keys.Right) || keys.IsKeyDown(Keys.D);
            jumpPressed = keys.IsKeyDown(Keys.Space) || keys.IsKeyDown(Keys.Up) || keys.IsKeyDown(Keys.W);
            crouch = keys.IsKeyDown(Keys.Down) || keys.IsKeyDown(Keys.S);
        }

        // Coyote time
        // Starts counting down when the player walks off a ledge without jumping.
        if (_wasGrounded && !IsOnGround && !_jumpConsumed)
        {
            _coyoteTimer = CoyoteTime;
        }

        if (_coyoteTimer > 0f)
        {
            _coyoteTimer -= dt;
        }

        // Jump buffer
        // Records a jump press so it fires on the next grounded frame if the player
        // presses jump just before landing.
        if (jumpPressed)
        {
            _jumpBufferTimer = JumpBufferTime;
        }
        else if (_jumpBufferTimer > 0f)
        {
            _jumpBufferTimer -= dt;
        }

        // Drop-through timer
        if (_dropTimer > 0f)
        {
            _dropTimer -= dt;
        }

        // Horizontal movement
        if (!IsHurt)
        {
            if (crouch && IsOnGround)
            {
                Velocity.X = 0f;
            }
            else if (left)
            {
                Velocity.X = -MoveSpeed;
                FacingRight = false;
            }
            else if (right)
            {
                Velocity.X = MoveSpeed;
                FacingRight = true;
            }
            else
            {
                Velocity.X = 0f;
            }
        }

        // Drop through one-way platform
        // Down + Jump while grounded activates drop-through.
        // _jumpConsumed is set so coyote time does not start on the following frame
        // (the player intentionally left the platform; coyote would cause an immediate jump).
        // _jumpBufferTimer is cleared so the held jump key does not fire a jump this frame.
        if (!IsHurt && crouch && jumpPressed && IsOnGround)
        {
            _dropTimer = DropThroughTime;
            _jumpBufferTimer = 0f;
            _jumpConsumed = true;
        }

        // Jump
        // Ground/coyote jump: consumed from the buffer, fires when grounded or within coyote time.
        // Blocked while a drop-through is in progress so a still-held jump key
        // does not immediately re-jump after the player leaves the platform.
        bool canGroundJump = !IsHurt && _jumpBufferTimer > 0f && _dropTimer <= 0f && (IsOnGround || _coyoteTimer > 0f);

        if (canGroundJump)
        {
            Velocity.Y = JumpVelocity;
            _jumpBufferTimer = 0f;
            _coyoteTimer = 0f;
            _jumpConsumed = true;
        }

        // Gravity
        if (!IsOnGround)
        {
            Velocity.Y = Math.Min(Velocity.Y + Gravity * dt, MaxFallSpeed);
        }

        // Kinematic move with tile collision
        KinematicMoveResult move = KinematicBody.Move(
            Position,
            Velocity,
            CurrentHitbox,
            _collision,
            dt,
            ignoreOneWay: _dropTimer > 0f);

        Position = move.Position;
        Velocity = move.Velocity;
        _wasGrounded = IsOnGround;
        IsOnGround = move.IsGrounded;

        if (IsOnGround)
        {
            _jumpConsumed = false;
        }

        // State machine
        if (!IsHurt)
        {
            if (IsOnGround)
            {
                if (crouch)
                {
                    _state = PlayerState.Crouch;
                }
                else if (Math.Abs(Velocity.X) > 1f)
                {
                    _state = PlayerState.Run;
                }
                else
                {
                    _state = PlayerState.Idle;
                }
            }
            else
            {
                _state = Velocity.Y < 0f ? PlayerState.Jump : PlayerState.Fall;
            }

            if (_state != _previousState)
            {
                _previousState = _state;
                string animName = _state switch
                {
                    PlayerState.Run => "player-foxy-run",
                    PlayerState.Jump => "player-foxy-jump",
                    PlayerState.Fall => "player-foxy-fall",
                    PlayerState.Crouch => "player-foxy-crouch",
                    _ => "player-foxy-idle"
                };
                _sprite.SetAnimation(animName).Play();
            }
        }

        _sprite.Update(gameTime);
    }

    public void Hurt(HorizontalDirection knockbackDirection)
    {
        if (IsHurt)
        {
            return;
        }

        IsHurt = true;
        _hurtTimer = HurtDuration;
        Velocity.Y = HurtKnockbackY;
        Velocity.X = (knockbackDirection == HorizontalDirection.Right ? 1f : -1f) * HurtKnockbackX;
        _sprite.SetAnimation("player-foxy-hurt").Play();
    }

    public void StompBounce() => Velocity.Y = StompBounceVelocity;

    public void Draw(SpriteBatch spriteBatch)
    {
        SpriteEffects effects = FacingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

        spriteBatch.Draw(
            _sprite.TextureRegion.Texture,
            Position,
            _sprite.TextureRegion.Bounds,
            Color.White, 0f, Vector2.Zero, Vector2.One, effects, 0f);
    }

    public RectangleF WorldHitbox
    {
        get
        {
            RectangleF hb = CurrentHitbox;
            return new RectangleF(Position.X + hb.X, Position.Y + hb.Y, hb.Width, hb.Height);
        }
    }
}
