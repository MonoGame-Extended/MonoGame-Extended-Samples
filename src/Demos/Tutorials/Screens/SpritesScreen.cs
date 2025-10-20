using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Screens;

namespace Tutorials.Screens
{
    public class SpritesScreen : GameScreen
    {
        private Sprite _apple;
        private Sprite _axeSprite;
        private Texture2D _backgroundTexture;
        private float _particleOpacity;
        private Sprite _particleSprite0;
        private Sprite _particleSprite1;
        private Sprite _spikeyBallSprite;
        private SpriteBatch _spriteBatch;

        private BitmapFont _bitmapFont;

        private NinePatch _clippingTextureRegion;

        public new GameMain Game => (GameMain)base.Game;

        public SpritesScreen(GameMain game) : base(game) { }

        public override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _bitmapFont = Content.Load<BitmapFont>("Fonts/montserrat-32");

            _backgroundTexture = Content.Load<Texture2D>("Textures/bg_sharbi");

            var testRegion = new Texture2DRegion(Content.Load<Texture2D>("Textures/clipping-test"));
            _clippingTextureRegion = testRegion.CreateNinePatch(16);

            var appleTexture = Content.Load<Texture2D>("Sprites/apple");
            _apple = new Sprite(appleTexture);
            _apple.OriginNormalized = new Vector2(0.5f, 0.5f);

            var axeTexture = Content.Load<Texture2D>("Textures/axe");
            _axeSprite = new Sprite(axeTexture)
            {
                Origin = new Vector2(243, 679),
                //Position = new Vector2(400, 0),
                //Scale = Vector2.One * 0.5f
                OriginNormalized = new Vector2(0.5f, 0.5f)
            };

            var spikeyBallTexture = Content.Load<Texture2D>("Textures/spike_ball");
            _spikeyBallSprite = new Sprite(spikeyBallTexture)
            {
                //Position = new Vector2(400, 340)
                OriginNormalized = new Vector2(0.5f, 0.5f)
            };

            var particleTexture = Content.Load<Texture2D>("Textures/particle");
            _particleSprite0 = new Sprite(particleTexture)
            {
                //Position = new Vector2(600, 340)
                OriginNormalized = new Vector2(0.5f, 0.5f)
            };
            _particleSprite1 = new Sprite(particleTexture)
            {
                //Position = new Vector2(200, 340)
                OriginNormalized = new Vector2(0.5f, 0.5f)
            };
            _particleOpacity = 0.0f;
        }

        private MouseState _previousMouseState;

        public override void Update(GameTime gameTime)
        {
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            var keyboardState = Keyboard.GetState();
            var mouseState = Mouse.GetState();

            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                Game.LoadScreen(ScreenName.MainMenu);
            }

            //_axeSprite.Rotation = MathHelper.ToRadians(180) + MathHelper.PiOver2 * 0.8f * (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds);

            //_spikeyBallSprite.Rotation -= deltaTime * 2.5f;
            //_spikeyBallSprite.Position = new Vector2(mouseState.X, mouseState.Y);

            _particleOpacity = 0.5f + (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds);
            _particleSprite0.Color = Color.White * _particleOpacity;
            _particleSprite1.Color = Color.White * (1.0f - _particleOpacity);

            var dx = mouseState.X - _previousMouseState.X;
            var dy = mouseState.Y - _previousMouseState.Y;

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                _clippingRectangle.X += dx;
                _clippingRectangle.Y += dy;
            }

            if (mouseState.RightButton == ButtonState.Pressed)
            {
                _clippingRectangle.Width += dx;
                _clippingRectangle.Height += dy;
            }

            _previousMouseState = mouseState;
        }

        private Rectangle _clippingRectangle = new Rectangle(50 + 32, 250 - 32, 64, 128 + 64);

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            _spriteBatch.Draw(_backgroundTexture, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);
            //_spriteBatch.Draw(_axeSprite);
            //_spriteBatch.Draw(_spikeyBallSprite);
            //_spriteBatch.Draw(_particleSprite0);
            //_spriteBatch.Draw(_particleSprite1);

            // clipping test
            _spriteBatch.Draw(_clippingTextureRegion, new Rectangle(50, 50, 128, 128), Color.White, clippingRectangle: null);
            _spriteBatch.Draw(_clippingTextureRegion, new Rectangle(50, 250, 512, 512), Color.White, clippingRectangle: _clippingRectangle);
            _spriteBatch.DrawRectangle(_clippingRectangle.ToRectangleF(), Color.White);

            _spriteBatch.Draw(_apple, new Vector2(100, 100));

            _spriteBatch.DrawString(_bitmapFont, "Left-Click to Drag the white viewing rect", new Vector2(200, 16), Color.White);
            _spriteBatch.DrawString(_bitmapFont, "Right-click and Drag to Resize it", new Vector2(200, 40), Color.White);
            _spriteBatch.DrawString(_bitmapFont, "Press ESC to go back.", new Vector2(200, 64), Color.White);

            _spriteBatch.End();

        }
    }
}
