// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using Microsoft.Xna.Framework;

namespace Entities.Components
{
    public class Player
    {
        public int speed = 100;
        public Vector2 Position;

        public Player(int speed, Vector2 position)
        {
            this.speed = speed;
            this.Position = position;
        }
    }
}
