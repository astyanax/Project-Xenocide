using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Xenocide.UI.Screens;

namespace Xenocide.UI.Scenes.Common
{
    public abstract class EmbeddedScene : DrawableGameComponent
    {
        public EmbeddedScene(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            contentManager = ((IScreenManager)Game.Services.GetService(typeof(IScreenManager))).ContentManager;
            graphicsDeviceService = (IGraphicsDeviceService)Game.Services.GetService(typeof(IGraphicsDeviceService));
            base.Initialize();
        }

        /// <summary>
        /// convert Window's co-ordinates to viewport co-ordinates
        /// </summary>
        /// <param name="windowCoords">Window co-ords to translate</param>
        /// <param name="viewport">The current viewport</param>
        /// <returns>Viewport co-ordinates</returns>
        protected Viewport CalcViewportForSceneWindow(CeGui.Rect windowCoords, Viewport viewport)
        {
            int fullHeight = viewport.Height;
            int fullWidth = viewport.Width;
            viewport.X = (int)(fullWidth * windowCoords.Left);
            viewport.Y = (int)(fullHeight * windowCoords.Top);
            viewport.Width = (int)(fullWidth * windowCoords.Width);
            viewport.Height = (int)(fullHeight * windowCoords.Height);

            // compute the aspect ratio while we're about it
            aspectRatio = (float)viewport.Width / (float)viewport.Height;

            return viewport;
        }

        /// <summary>
        /// CeGui widget that shows the 3D scene
        /// <remarks>Actually, at moment, indicates where to draw the 3D scene</remarks>
        /// </summary>
        public CeGui.Rect SceneWindow { get { return sceneWindow; } set { sceneWindow = value; } }

        /// <summary>
        /// The viewport's aspect ratio
        /// </summary>
        protected float AspectRatio { get { return aspectRatio; } }

        public ContentManager ContentManager { get { return contentManager; } }
        public IGraphicsDeviceService GraphicsDeviceService { get { return graphicsDeviceService; } }

        /// <summary>
        /// CeGui widget that shows the 3D scene
        /// <remarks>Actually, at moment, indicates where to draw the 3D scene</remarks>
        /// </summary>
        private CeGui.Rect sceneWindow;
        private ContentManager contentManager;
        private IGraphicsDeviceService graphicsDeviceService;

        /// <summary>
        /// The viewport's aspect ratio
        /// </summary>
        private float aspectRatio;
    }
}
