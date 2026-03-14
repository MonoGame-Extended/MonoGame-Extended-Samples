using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Graphics;

namespace DemoGame.Shared;

/// <summary>
/// Loads a custom Zephyr atlas JSON file and creates a <see cref="Texture2DAtlas"/>,
/// <see cref="SpriteSheet"/>, and per-frame hitbox dictionary.
/// </summary>
public static class AtlasJsonLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public static AtlasLoadResult Load(ContentManager content, string atlasJsonPath, string? atlasTexturePath = null)
    {
        string json = File.ReadAllText(atlasJsonPath);
        AtlasData data = JsonSerializer.Deserialize<AtlasData>(json, JsonOptions)
            ?? throw new InvalidOperationException("Failed to parse atlas JSON.");

        string directory = Path.GetDirectoryName(atlasJsonPath) ?? ".";
        string texturePath = atlasTexturePath
            ?? Path.Combine(directory, data.Meta.Image);

        Texture2D texture = content.Load<Texture2D>(texturePath);
        texture.Name = Path.GetFileNameWithoutExtension(texturePath);

        Texture2DAtlas atlas = new Texture2DAtlas("atlas", texture);
        foreach (AtlasFrame frame in data.Frames)
        {
            atlas.CreateRegion(
                frame.Frame.X,
                frame.Frame.Y,
                frame.Frame.W,
                frame.Frame.H,
                frame.Filename);
        }

        SpriteSheet spriteSheet = new SpriteSheet("atlas", atlas);
        foreach (AtlasAnimation anim in data.Animations)
        {
            string animName = anim.Name;
            bool looping = anim.LoopEnabled;
            List<AtlasAnimationFrame> frames = anim.Frames;

            spriteSheet.DefineAnimation(animName, builder =>
            {
                builder.IsLooping(looping);
                foreach (AtlasAnimationFrame f in frames)
                {
                    builder.AddFrame(f.SpriteName, TimeSpan.FromMilliseconds(f.DelayMs));
                }
            });
        }

        Dictionary<string, RectangleF> hitboxes = new Dictionary<string, RectangleF>(StringComparer.Ordinal);
        foreach (AtlasFrame frame in data.Frames)
        {
            if (frame.HitboxEnabled && frame.Hitbox?.Rectangle is { } r)
            {
                hitboxes[frame.Filename] = new RectangleF(r.X, r.Y, r.W, r.H);
            }
        }

        return new AtlasLoadResult(atlas, spriteSheet, hitboxes);
    }

    #region JSON model

    private sealed class AtlasData
    {
        public AtlasMeta Meta { get; set; } = new AtlasMeta();
        public List<AtlasFrame> Frames { get; set; } = new List<AtlasFrame>();
        public List<AtlasAnimation> Animations { get; set; } = new List<AtlasAnimation>();
    }

    private sealed class AtlasMeta
    {
        public string Image { get; set; } = "";
    }

    private sealed class AtlasFrame
    {
        public string Filename { get; set; } = "";
        public AtlasRect Frame { get; set; } = new AtlasRect();
        public bool HitboxEnabled { get; set; }
        public AtlasHitboxData? Hitbox { get; set; }
    }

    private sealed class AtlasRect
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int W { get; set; }
        public int H { get; set; }
    }

    private sealed class AtlasHitboxData
    {
        public AtlasHitboxRect? Rectangle { get; set; }
    }

    private sealed class AtlasHitboxRect
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float W { get; set; }
        public float H { get; set; }
    }

    private sealed class AtlasAnimation
    {
        public string Name { get; set; } = "";
        public List<AtlasAnimationFrame> Frames { get; set; } = new List<AtlasAnimationFrame>();
        public bool LoopEnabled { get; set; }
    }

    private sealed class AtlasAnimationFrame
    {
        public string SpriteName { get; set; } = "";
        public int DelayMs { get; set; } = 100;
    }

    #endregion
}
