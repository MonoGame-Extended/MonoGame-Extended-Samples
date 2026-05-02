// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Collision;

public class BallEntity : IEntity
{
    private BoundingCircle2D _bounds;
    private Vector2 _velocity;

    public BallEntity(int id, BoundingCircle2D bounds)
    {
        Id = id;
        _bounds = bounds;
        RandomizeVelocity();
    }

    public int Id { get; }

    public CollisionShape2D Shape => new(_bounds);

    public void Update(GameTime gameTime, BoundingBox2D worldBounds)
    {
        _bounds = _bounds.Translate(_velocity * gameTime.GetElapsedSeconds() * 80f);
        ConstrainToWorld(worldBounds);
    }

    public void Move(Vector2 translation)
    {
        _bounds = _bounds.Translate(translation);
    }

    public void Bounce()
    {
        _velocity *= -1f;

        if (_velocity == Vector2.Zero)
        {
            RandomizeVelocity();
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.DrawCircle(_bounds.Center, _bounds.Radius, 16, Color.Red, 3f);
    }

    private void ConstrainToWorld(BoundingBox2D worldBounds)
    {
        Vector2 position = _bounds.Center;
        bool bounced = false;

        if (position.X - _bounds.Radius < worldBounds.Min.X)
        {
            position.X = worldBounds.Min.X + _bounds.Radius;
            _velocity.X *= -1f;
            bounced = true;
        }
        else if (position.X + _bounds.Radius > worldBounds.Max.X)
        {
            position.X = worldBounds.Max.X - _bounds.Radius;
            _velocity.X *= -1f;
            bounced = true;
        }

        if (position.Y - _bounds.Radius < worldBounds.Min.Y)
        {
            position.Y = worldBounds.Min.Y + _bounds.Radius;
            _velocity.Y *= -1f;
            bounced = true;
        }
        else if (position.Y + _bounds.Radius > worldBounds.Max.Y)
        {
            position.Y = worldBounds.Max.Y - _bounds.Radius;
            _velocity.Y *= -1f;
            bounced = true;
        }

        _bounds.Center = position;

        if (bounced && _velocity == Vector2.Zero)
        {
            RandomizeVelocity();
        }
    }

    private void RandomizeVelocity()
    {
        do
        {
            _velocity = new Vector2(Random.Shared.Next(-1, 2), Random.Shared.Next(-1, 2));
        }
        while (_velocity == Vector2.Zero);
    }
}
