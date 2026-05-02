// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Collisions.Layers;

namespace Collision;

public class Game1 : Game
{
    private const int MapWidth = 500;
    private const int MapHeight = 500;
    private static readonly BoundingBox2D WorldBounds = BoundingBox2D.CreateFromPositionAndSize(Vector2.Zero, new Vector2(MapWidth, MapHeight));

    private readonly GraphicsDeviceManager _graphics;
    private readonly List<IEntity> _entities = new();
    private readonly CollisionWorld2D _collisionWorld;
    private SpriteBatch _spriteBatch = null!;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = MapWidth;
        _graphics.PreferredBackBufferHeight = MapHeight;

        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _collisionWorld = new CollisionWorld2D(new Layer(new SpatialHash(new SizeF(64f, 64f))));
    }

    protected override void Initialize()
    {
        base.Initialize();

        for (int i = 0; i < 50; i++)
        {
            float size = Random.Shared.Next(20, 40);
            Vector2 position = new Vector2(
                Random.Shared.Next(0, MapWidth),
                Random.Shared.Next(0, MapHeight));

            IEntity entity = i % 2 == 0
                ? new BallEntity(i + 1, new BoundingCircle2D(position, size * 0.5f))
                : new CubeEntity(i + 1, BoundingBox2D.CreateFromCenterAndExtents(position, new Vector2(size * 0.5f)));

            _entities.Add(entity);
            _collisionWorld.Insert(entity);
        }
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
    }

    protected override void Update(GameTime gameTime)
    {
        KeyboardState keyboardState = Keyboard.GetState();
        GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);

        if (gamePadState.Buttons.Back == ButtonState.Pressed || keyboardState.IsKeyDown(Keys.Escape))
        {
            Exit();
        }

        foreach (IEntity entity in _entities)
        {
            entity.Update(gameTime, WorldBounds);
        }

        _collisionWorld.RebuildDynamicLayers();

        foreach (CollisionPair2D collisionPair in _collisionWorld.QueryCollisionPairs(null, null))
        {
            IEntity first = (IEntity)collisionPair.First;
            IEntity second = (IEntity)collisionPair.Second;
            Vector2 separation = collisionPair.FirstResult.MinimumTranslationVector * 0.5f;

            first.Move(separation);
            second.Move(-separation);
            first.Bounce();
            second.Bounce();
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();
        _spriteBatch.DrawRectangle(0, 0, MapWidth, MapHeight, Color.White, 2f);

        foreach (IEntity entity in _entities)
        {
            entity.Draw(_spriteBatch);
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
