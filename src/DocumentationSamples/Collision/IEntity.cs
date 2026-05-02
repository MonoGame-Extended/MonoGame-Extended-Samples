// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;

namespace Collision;

public interface IEntity : ICollisionActor
{
    void Update(GameTime gameTime, BoundingBox2D worldBounds);
    void Move(Vector2 translation);
    void Bounce();
    void Draw(SpriteBatch spriteBatch);
}
