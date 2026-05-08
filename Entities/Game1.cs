// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Entities.Components;
using Entities.Systems;
using MonoGame.Extended.ECS;

namespace Entities
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private World _world;
        private Entity playerEntity;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _world = new WorldBuilder()
                .AddSystem(new PlayerSystem())
                .AddSystem(new RenderSystem(_spriteBatch))
                .Build();

            playerEntity = _world.CreateEntity();
            playerEntity.Attach(Content.Load<Texture2D>("logo-square-128"));
            playerEntity.Attach(new Player(100, new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2)));
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _world.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _world.Draw(gameTime);

            base.Draw(gameTime);
        }
    }
}
