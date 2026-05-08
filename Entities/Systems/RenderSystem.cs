// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using Entities.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;

namespace Entities.Systems
{
    internal class RenderSystem : EntityDrawSystem
    {
        private SpriteBatch _spriteBatch;
        private ComponentMapper<Texture2D> _textureMapper;
        private ComponentMapper<Player> _playerMapper;

        public RenderSystem(SpriteBatch spriteBatch)
            : base(Aspect.All(typeof(Texture2D), typeof(Player)))
        {
            _spriteBatch = spriteBatch;
        }
        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();

            foreach (var entityId in ActiveEntities)
            {
                var texture = _textureMapper.Get(entityId);
                var player = _playerMapper.Get(entityId);
                _spriteBatch.Draw(texture, new Rectangle((int)player.Position.X, (int)player.Position.Y, 64, 64), Color.White);
            }

            _spriteBatch.End();
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _textureMapper = mapperService.GetMapper<Texture2D>();
            _playerMapper = mapperService.GetMapper<Player>();
        }
    }
}
