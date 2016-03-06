using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Xenocide.GUISystem
{
    public class XenoGUIBuilder : GameComponent, IGUIBuilder
    {
        public XenoGUIBuilder(Game game)
            : base(game)
        {
            guiManager = (IGUIManager)Game.Services.GetService(typeof(IGUIManager));
            guiManager.RegisterTexture(".\\Content\\Textures\\skybox");
            guiManager.RegisterTexture(".\\Content\\Textures\\ui\\xenolook_window");
            guiManager.RegisterFont(".\\Content\\Fonts\\XenoFont");
        }

        public override void Initialize()
        {
            
        }

        #region IGUIBuilder Member

        public StaticImage CreateStaticImage(Rectangle position, Quad skin)
        {
            StaticImage image = new StaticImage(guiManager);
            image.Quad = skin;
            image.Position = position;
            
            return image;
        }

        public StaticText CreateStaticText(Point position, string text)
        {
            StaticText staticText = new StaticText(guiManager);
            staticText.Position = new Rectangle(position.X, position.Y, 0, 0);
            staticText.Color = Color.Green;
            staticText.FontName = ".\\Content\\Fonts\\XenoFont";
            staticText.Text = text;

            return staticText;
        }

        public Frame CreateWindow(Rectangle position, string title)
        {
            const string TEXTURENAME = ".\\Content\\Textures\\ui\\xenolook_window";

            Frame window = new Frame(guiManager);
            
            window.Position = position;
            
            NineQuad skin = new NineQuad();
            skin.Top = new Quad(TEXTURENAME, new Rectangle(126, 28, 2, 2));
            skin.Bottom = skin.Top;
            skin.Left = skin.Top;
            skin.Right = skin.Top;
            skin.TopLeft = skin.Top;
            skin.TopRight = skin.Top;
            skin.BottomLeft = skin.Top;
            skin.BottomRight = skin.Top;
            skin.Center = new Quad(TEXTURENAME, new Rectangle(126, 33, 100, 100));

            window.Skin = skin;

            FrameTitleBar titleBar = new FrameTitleBar(guiManager);
            titleBar.Height = 25;
            titleBar.Text = title;
            titleBar.Font = ".\\Content\\Fonts\\XenoFont";
            titleBar.Skin = skin;

            window.TitleBar = titleBar;

            return window;
        }

        #endregion

        #region Fields
        
        IGUIManager guiManager;

        #endregion
    }
}
