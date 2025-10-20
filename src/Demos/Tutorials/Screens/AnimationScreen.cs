using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Screens;
using MonoGame.Extended.ViewportAdapters;
using Tutorials.Screens;

namespace Tutorials.Demos
{
    public class AnimationScreen : GameScreen
    {
        private SpriteBatch _spriteBatch;

        private BitmapFont _bitmapFont;

        public new GameMain Game => (GameMain)base.Game;

        public AnimationScreen(GameMain game) : base(game){ }


        public override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            var viewportAdapter = new BoxingViewportAdapter(Game.Window, GraphicsDevice, 800, 480);

            _bitmapFont = Content.Load<BitmapFont>("Fonts/montserrat-32");
        }


        public override void UnloadContent()
        {

        }

        public override void Update(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();
            var mouseState = Mouse.GetState();

            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                Game.LoadScreen(ScreenName.MainMenu);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();
            _spriteBatch.DrawString(_bitmapFont, "Not implemented yet, Press ESC to go back", new Vector2(5, 5), Color.White);
            _spriteBatch.End();
        }
    }
}
