// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Collision;

public class CubeEntity : IEntity
{
    private BoundingBox2D _bounds;
    private Vector2 _velocity;

    public CubeEntity(int id, BoundingBox2D bounds)
    {
        Id = id;
        _bounds = bounds;
        RandomizeVelocity();
    }

    public int Id { get; }

    public CollisionShape2D Shape => new(_bounds);

    public void Update(GameTime gameTime, BoundingBox2D worldBounds)
    {
        _bounds = _bounds.Translate(_velocity * gameTime.GetElapsedSeconds() * 100f);
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
        spriteBatch.DrawRectangle(_bounds.Min, new SizeF(_bounds.Width, _bounds.Height), Color.Red, 3f);
    }

    private void ConstrainToWorld(BoundingBox2D worldBounds)
    {
        Vector2 translation = Vector2.Zero;
        bool bounced = false;

        if (_bounds.Min.X < worldBounds.Min.X)
        {
            translation.X = worldBounds.Min.X - _bounds.Min.X;
            _velocity.X *= -1f;
            bounced = true;
        }
        else if (_bounds.Max.X > worldBounds.Max.X)
        {
            translation.X = worldBounds.Max.X - _bounds.Max.X;
            _velocity.X *= -1f;
            bounced = true;
        }

        if (_bounds.Min.Y < worldBounds.Min.Y)
        {
            translation.Y = worldBounds.Min.Y - _bounds.Min.Y;
            _velocity.Y *= -1f;
            bounced = true;
        }
        else if (_bounds.Max.Y > worldBounds.Max.Y)
        {
            translation.Y = worldBounds.Max.Y - _bounds.Max.Y;
            _velocity.Y *= -1f;
            bounced = true;
        }

        if (translation != Vector2.Zero)
        {
            _bounds = _bounds.Translate(translation);
        }

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
