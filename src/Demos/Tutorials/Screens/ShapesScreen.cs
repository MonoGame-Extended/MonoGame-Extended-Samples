using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Shapes;
using MonoGame.Extended.VectorDraw;

namespace Tutorials.Screens
{
    public class ShapesScreen : GameScreen
    {
        private PrimitiveDrawing _primitiveDrawing;
        PrimitiveBatch _primitiveBatch;
        private Matrix _localProjection;
        private Matrix _localView;

        private SpriteBatch _spriteBatch;
        private BitmapFont _bitmapFont;

        public new GameMain Game => (GameMain)base.Game;

        public ShapesScreen(GameMain game) : base(game) { }

        private readonly Polygon _polygon = new Polygon(new[]
        {
            new Vector2(0, 0),
            new Vector2(40, 0),
            new Vector2(40, 40),
            new Vector2(60, 40),
            new Vector2(60, 60),
            new Vector2(0, 60),
        });

        public override void LoadContent()
        {
            _primitiveBatch = new PrimitiveBatch(GraphicsDevice);
            _primitiveDrawing = new PrimitiveDrawing(_primitiveBatch);
            _localProjection = Matrix.CreateOrthographicOffCenter(0f, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0f, 0f, 1f);
            _localView = Matrix.Identity;

            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _bitmapFont = Content.Load<BitmapFont>("Fonts/montserrat-32");
        }

        public override void Update(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                Game.LoadScreen(ScreenName.MainMenu);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            _localProjection = Matrix.CreateOrthographicOffCenter(0f, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0f, 0f, 1f);
            _localView = Matrix.Identity;

            _primitiveBatch.Begin(ref _localProjection, ref _localView);

            _primitiveDrawing.DrawPoint(new Vector2(10, 10), Color.Brown);

            _primitiveDrawing.DrawRectangle(new Vector2(20, 20), 50, 50, Color.Yellow);
            _primitiveDrawing.DrawSolidRectangle(new Vector2(20, 120), 50, 50, Color.Yellow);

            _primitiveDrawing.DrawCircle(new Vector2(120, 45), 25, Color.Green);
            _primitiveDrawing.DrawSolidCircle(new Vector2(120, 145), 25, Color.Green);

            _primitiveDrawing.DrawEllipse(new Vector2(220, 45), new Vector2(50, 25), 32, Color.Orange);
            _primitiveDrawing.DrawSolidEllipse(new Vector2(220, 145), new Vector2(50, 25), 32, Color.Orange);

            _primitiveDrawing.DrawSegment(new Vector2(320, 20), new Vector2(370, 170), Color.White);

            _primitiveDrawing.DrawPolygon(new Vector2(420, 20), _polygon.Vertices, Color.Aqua);
            _primitiveDrawing.DrawSolidPolygon(new Vector2(420, 120), _polygon.Vertices, Color.Aqua);

            _primitiveBatch.End();

            _spriteBatch.Begin();
            _spriteBatch.DrawString(_bitmapFont, " Press ESC to go back.", new Vector2(5, GraphicsDevice.Viewport.Height - 40), Color.DarkBlue);
            _spriteBatch.End();
        }
    }
}
