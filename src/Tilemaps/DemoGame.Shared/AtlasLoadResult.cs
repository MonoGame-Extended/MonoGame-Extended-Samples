using System.Collections.Generic;
using MonoGame.Extended;
using MonoGame.Extended.Graphics;

namespace DemoGame.Shared;

public sealed record AtlasLoadResult(Texture2DAtlas Atlas, SpriteSheet SpriteSheet, Dictionary<string, RectangleF> Hitboxes);
