using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Collisions.Layers;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Screens;

namespace Tutorials.Screens;

public class CollisionScreen : GameScreen
{
    private const string WallLayerName = "walls";

    private readonly List<DemoActor> _actors = new();
    private CollisionWorld2D _collisionWorld = null!;
    private SpriteBatch _spriteBatch = null!;
    private Texture2D _blankTexture = null!;
    private DemoBall _movingBall = null!;
    private ControllableBall _controllableBall = null!;
    private BitmapFont _bitmapFont = null!;

    public new GameMain Game => (GameMain)base.Game;

    public CollisionScreen(GameMain game)
        : base(game)
    {
    }

    public override void LoadContent()
    {
        _collisionWorld = new CollisionWorld2D(new Layer(new SpatialHash(new SizeF(128f, 128f))));
        _collisionWorld.AddLayer(WallLayerName, new Layer(new SpatialHash(new SizeF(256f, 256f)))
        {
            IsDynamic = false
        });

        _bitmapFont = Content.Load<BitmapFont>("Fonts/montserrat-32");
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        Texture2D spikyBallTexture = Content.Load<Texture2D>("Textures/spike_ball");
        _blankTexture = new Texture2D(GraphicsDevice, 1, 1);
        _blankTexture.SetData(new[] { Color.WhiteSmoke });

        _movingBall = new DemoBall(1, new Sprite(spikyBallTexture), new Vector2(600, 240), 60f)
        {
            Velocity = new Vector2(0f, 120f)
        };

        _controllableBall = new ControllableBall(2, new Sprite(spikyBallTexture), new Vector2(400, 240), 60f);

        DemoWall topWall = new DemoWall(3, _blankTexture, new Vector2(0, 0), new Vector2(800, 20));
        DemoWall bottomWall = new DemoWall(4, _blankTexture, new Vector2(0, 460), new Vector2(800, 20));
        StationaryBall centerBall = new StationaryBall(5, new Sprite(spikyBallTexture), new Vector2(400, 240), 60f);

        _actors.Add(_movingBall);
        _actors.Add(_controllableBall);
        _actors.Add(topWall);
        _actors.Add(bottomWall);
        _actors.Add(centerBall);

        _collisionWorld.Insert(_movingBall);
        _collisionWorld.Insert(_controllableBall);
        _collisionWorld.Insert(centerBall);
        _collisionWorld.Insert(topWall, WallLayerName);
        _collisionWorld.Insert(bottomWall, WallLayerName);

        base.LoadContent();
    }

    public override void Update(GameTime gameTime)
    {
        KeyboardState keyboardState = Keyboard.GetState();

        if (keyboardState.IsKeyDown(Keys.Escape))
        {
            Game.LoadScreen(ScreenName.MainMenu);
        }

        UpdateControlledBall(gameTime, _controllableBall);
        _movingBall.Update(gameTime);

        _collisionWorld.RebuildDynamicLayers();
        ResolveControllableBallCollisions();

        _collisionWorld.RebuildDynamicLayers();
        ResolveMovingBallCollisions();
    }

    public override void Draw(GameTime gameTime)
    {
        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

        foreach (DemoActor actor in _actors)
        {
            actor.Draw(_spriteBatch);
        }

        _spriteBatch.End();

        _spriteBatch.Begin(blendState: BlendState.AlphaBlend);
        _spriteBatch.DrawString(
            _bitmapFont,
            "Use W,A,S,D to move, ESC to go back.\nPlayer movement uses QueryCandidates + TryGetCollision.\nThe moving ball uses CollisionWorld2D query results.",
            new Vector2(5, 5),
            Color.DarkBlue);
        _spriteBatch.End();
    }

    private void UpdateControlledBall(GameTime gameTime, DemoActor actor)
    {
        KeyboardState keyboardState = Keyboard.GetState();
        float speed = 150.0f;
        Vector2 position = actor.Position;
        float distance = speed * gameTime.GetElapsedSeconds();

        if (keyboardState.IsKeyDown(Keys.W))
        {
            position.Y -= distance;
        }

        if (keyboardState.IsKeyDown(Keys.S))
        {
            position.Y += distance;
        }

        if (keyboardState.IsKeyDown(Keys.A))
        {
            position.X -= distance;
        }

        if (keyboardState.IsKeyDown(Keys.D))
        {
            position.X += distance;
        }

        actor.Position = position;
    }

    private void ResolveControllableBallCollisions()
    {
        ResolveControllableBallAgainstLayer(null);
        _collisionWorld.RebuildDynamicLayers();
        ResolveControllableBallAgainstLayer(WallLayerName);
    }

    private void ResolveControllableBallAgainstLayer(string layerName)
    {
        foreach (ICollisionActor candidate in _collisionWorld.QueryCandidates(_controllableBall, layerName))
        {
            if (ReferenceEquals(candidate, _controllableBall))
            {
                continue;
            }

            if (!_controllableBall.Shape.TryGetCollision(candidate.Shape, out CollisionResult2D result))
            {
                continue;
            }

            _controllableBall.Move(result.MinimumTranslationVector);
            _collisionWorld.RebuildDynamicLayers();
        }
    }

    private void ResolveMovingBallCollisions()
    {
        ResolveMovingBallAgainstLayer(null);
        _collisionWorld.RebuildDynamicLayers();
        ResolveMovingBallAgainstLayer(WallLayerName);
    }

    private void ResolveMovingBallAgainstLayer(string layerName)
    {
        foreach (CollisionEvent2D collision in _collisionWorld.QueryCollisions(_movingBall, layerName))
        {
            _movingBall.Move(collision.Result.MinimumTranslationVector);
            _movingBall.Bounce();
            _collisionWorld.RebuildDynamicLayers();
        }
    }

    private abstract class DemoActor : ICollisionActor
    {
        protected DemoActor(int id, Vector2 position)
        {
            Id = id;
            Position = position;
        }

        public int Id { get; }

        public Vector2 Position { get; set; }

        public Vector2 Velocity { get; set; }

        public abstract CollisionShape2D Shape { get; }

        public virtual void Update(GameTime gameTime)
        {
            Position += gameTime.GetElapsedSeconds() * Velocity;
        }

        public virtual void Move(Vector2 translation)
        {
            Position += translation;
        }

        public virtual void Bounce()
        {
            Velocity *= -1f;
        }

        public abstract void Draw(SpriteBatch spriteBatch);
    }

    private sealed class DemoWall : DemoActor
    {
        private readonly Texture2D _texture;
        private readonly Vector2 _size;

        public DemoWall(int id, Texture2D texture, Vector2 position, Vector2 size)
            : base(id, position)
        {
            _texture = texture;
            _size = size;
        }

        public override CollisionShape2D Shape => new(BoundingBox2D.CreateFromPositionAndSize(Position, _size));

        public override void Update(GameTime gameTime)
        {
        }

        public override void Move(Vector2 translation)
        {
        }

        public override void Bounce()
        {
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                _texture,
                new Rectangle((int)Position.X, (int)Position.Y, (int)_size.X, (int)_size.Y),
                Color.WhiteSmoke);
        }
    }

    private class DemoBall : DemoActor
    {
        private readonly Sprite _sprite;
        private readonly float _radius;

        public DemoBall(int id, Sprite sprite, Vector2 position, float radius)
            : base(id, position)
        {
            _sprite = sprite;
            _sprite.OriginNormalized = new Vector2(0.5f, 0.5f);
            _radius = radius;
        }

        public override CollisionShape2D Shape => new(new BoundingCircle2D(Position, _radius));

        public override void Draw(SpriteBatch spriteBatch)
        {
            _sprite.Draw(spriteBatch, Position, 0f, Vector2.One);
        }
    }

    private sealed class StationaryBall : DemoBall
    {
        public StationaryBall(int id, Sprite sprite, Vector2 position, float radius)
            : base(id, sprite, position, radius)
        {
        }

        public override void Update(GameTime gameTime)
        {
        }

        public override void Move(Vector2 translation)
        {
        }

        public override void Bounce()
        {
        }
    }

    private sealed class ControllableBall : DemoBall
    {
        public ControllableBall(int id, Sprite sprite, Vector2 position, float radius)
            : base(id, sprite, position, radius)
        {
        }

        public override void Update(GameTime gameTime)
        {
        }

        public override void Bounce()
        {
        }
    }
}
