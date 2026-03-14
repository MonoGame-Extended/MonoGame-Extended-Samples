using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Screens;
using MonoGame.Extended.ViewportAdapters;

namespace DemoGame.Shared;

public class TitleScreen : GameScreen
{
    private enum TitleStage { Title, Instructions }

    private static readonly Color SkyColor = new Color(0x68, 0xC2, 0xD3);

    private readonly ScreenManager _screenManager;

    private SpriteBatch _spriteBatch = null!;
    private BoxingViewportAdapter _viewportAdapter = null!;

    private Texture2DRegion _backRegion = null!;
    private Texture2DRegion _middleRegion = null!;
    private Texture2DRegion _titleRegion = null!;
    private Texture2DRegion _enterRegion = null!;
    private Texture2DRegion _creditsRegion = null!;
    private Texture2DRegion _portRegion = null!;
    private Texture2DRegion _instructionsRegion = null!;

    private float _backScrollX;
    private float _middleScrollX;

    private float _blinkTimer;
    private bool _blinkVisible = true;
    private TitleStage _stage = TitleStage.Title;
    private bool _enterWasDown;

    public TitleScreen(Game game, ScreenManager screenManager) : base(game)
    {
        _screenManager = screenManager;
    }

    public override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _viewportAdapter = new BoxingViewportAdapter(Game.Window, GraphicsDevice, DemoGameBase.VirtualWidth, DemoGameBase.VirtualHeight);
        _viewportAdapter.Reset();

        DemoGameBase game = (DemoGameBase)Game;
        _backRegion = game.Atlas["back.png"];
        _middleRegion = game.Atlas["middle.png"];
        _titleRegion = game.Atlas["title-screen.png"];
        _enterRegion = game.Atlas["press-enter-text.png"];
        _creditsRegion = game.Atlas["credits-text.png"];
        _portRegion = game.Atlas["port-text.png"];
        _instructionsRegion = game.Atlas["instructions.png"];
    }

    public override void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        _backScrollX -= 18f * dt;
        _middleScrollX -= 36f * dt;

        _blinkTimer += dt;
        if (_blinkTimer >= 0.7f)
        {
            _blinkTimer = 0f;
            _blinkVisible = !_blinkVisible;
        }

        bool enterDown = Keyboard.GetState().IsKeyDown(Keys.Enter);
        if (enterDown && !_enterWasDown)
        {
            if (_stage == TitleStage.Title)
            {
                _stage = TitleStage.Instructions;
            }
            else
            {
                _screenManager.ReplaceScreen(new PlayScreen(Game, _screenManager));
            }
        }
        _enterWasDown = enterDown;
    }

    public override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(SkyColor);

        _spriteBatch.Begin(
            sortMode: SpriteSortMode.Deferred,
            blendState: BlendState.AlphaBlend,
            samplerState: SamplerState.PointClamp,
            transformMatrix: _viewportAdapter.GetScaleMatrix());

        DrawTiled(_backRegion, _backScrollX, y: 0);
        DrawTiled(_middleRegion, _middleScrollX, y: 80);

        int vw = DemoGameBase.VirtualWidth;
        int vh = DemoGameBase.VirtualHeight;

        if (_stage == TitleStage.Title)
        {
            DrawCentered(_titleRegion, vw / 2, 35);
            DrawCentered(_creditsRegion, vw / 2, vh - 10, anchorBottom: true);
            DrawCentered(_portRegion, vw / 2, vh, anchorBottom: true);

            if (_blinkVisible)
            {
                DrawCentered(_enterRegion, vw / 2, vh - 35, anchorBottom: true);
            }
        }
        else
        {
            DrawCentered(_instructionsRegion, vw / 2, vh / 2, anchorCenter: true);
        }

        _spriteBatch.End();
    }

    private void DrawTiled(Texture2DRegion region, float scrollX, int y)
    {
        float tileW = region.Width;
        float offset = scrollX % tileW;
        if (offset > 0) offset -= tileW;

        int tilesNeeded = (int)Math.Ceiling((float)DemoGameBase.VirtualWidth / tileW) + 2;
        for (int i = 0; i < tilesNeeded; i++)
        {
            _spriteBatch.Draw(
                region.Texture,
                new Vector2(offset + i * tileW, y),
                region.Bounds,
                Color.White, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
        }
    }

    private void DrawCentered(Texture2DRegion region, int centerX, int y,
        bool anchorBottom = false, bool anchorCenter = false)
    {
        int drawX = centerX - region.Width / 2;
        int drawY = anchorBottom ? y - region.Height :
                    anchorCenter ? y - region.Height / 2 :
                    y;

        _spriteBatch.Draw(
            region.Texture,
            new Vector2(drawX, drawY),
            region.Bounds,
            Color.White,
            0f,
            Vector2.Zero,
            Vector2.One,
            SpriteEffects.None,
            0f
        );
    }

    public override void UnloadContent()
    {
        _viewportAdapter?.Dispose();
        _spriteBatch?.Dispose();
    }
}
