// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using System;
using Gum.Forms.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Screens;
using MonoGame.Extended.ViewportAdapters;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using RenderingLibrary;

namespace Tutorials.Screens;

public class MainMenuScreen : GameScreen
{
    private ContainerRuntime _menu;
    private ViewportAdapter _viewportAdapter;
    private new GameMain Game => (GameMain)base.Game;

    public MainMenuScreen(Game game) : base(game) { }

    public override void LoadContent()
    {
        base.LoadContent();

        Texture2D buttonTexture = Game.Content.Load<Texture2D>("Gui/button_rectangle_border");
        Texture2DRegion buttonRegion = new Texture2DRegion(buttonTexture);
        NinePatch ninePatch = buttonRegion.CreateNinePatch(6);
        _viewportAdapter = new BoxingViewportAdapter(Game.Window, GraphicsDevice, 800, 480);

        _menu = new ContainerRuntime()
        {
            Width = _viewportAdapter.VirtualWidth,
            Height = _viewportAdapter.VirtualHeight,
            ChildrenLayout = Gum.Managers.ChildrenLayout.AutoGridHorizontal,
            AutoGridHorizontalCells = 4,
            AutoGridVerticalCells = 3,
            WrapsChildren = true
        };
        _menu.AddToRoot();

        foreach (var screen in (ScreenName[])Enum.GetValues<ScreenName>())
        {
            if (screen == ScreenName.MainMenu)
            {
                continue;
            }

            DemoButton demoInstance = new DemoButton();
            demoInstance.SetTexture(buttonTexture);
            demoInstance.SetText(screen.ToString());
            Button demoButton = demoInstance.FormsControl;
            demoButton.Click += (_, _) =>
            {
                Game.LoadScreen(screen);
            };
            _menu.Children.Add(demoButton.Visual);
        };

        DemoButton closeInstance = new DemoButton();
        closeInstance.SetTexture(buttonTexture);
        closeInstance.SetText("Close");
        closeInstance.SetHighlightedTextColor(Color.Orange);
        Button closeButton = closeInstance.FormsControl;
        closeButton.Click += (_, _) => { Game.Exit(); };
        _menu.Children.Add(closeButton.Visual);
    }
    public override void Update(GameTime gameTime)
    {
        Game.GumUI.Update(gameTime);
        //FormsUtilities.Update(gameTime, _menu);
       // SystemManagers.Default.Activity(gameTime.TotalGameTime.TotalSeconds);
    }

    public override void Draw(GameTime gameTime)
    {
        float scaleFactor = _viewportAdapter.GetScaleMatrix().M11;
        Game.GumUI.Draw();
        //SystemManagers.Default.Renderer.Camera.Zoom = scaleFactor;
        //SystemManagers.Default.Draw();
    }
}
