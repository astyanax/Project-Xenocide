#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Xenocide.GUISystem
{
    public enum QuadRenderMode
    {
        Tiled, Scaled
    }

    public interface IGUIManager
    {
        void RegisterTexture(string texture);
        void RegisterFont(string texture);

        void DrawQuad(SpriteBatch spriteBatch, Quad quad, Rectangle destination, Color color, QuadRenderMode horizontalRenderMode, QuadRenderMode verticalRenderMode);
        void DrawNineQuad(SpriteBatch spriteBatch, NineQuad quad, Rectangle destination, Color color);

        void DrawString(SpriteBatch spriteBatch, string fontName, string text, Point position, Color color);
    }

    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class GUIManager : Microsoft.Xna.Framework.DrawableGameComponent, IGUIManager
    {
        private SpriteBatch spriteBatch;
        private IGraphicsDeviceService graphicsDeviceService;
        private IInputManagerService inputManager;
        private ContentManager contentManager;
        private Dictionary<string, Texture2D> textures = new Dictionary<string,Texture2D>();
        private List<string> texturesToLoad = new List<string>();
        private Dictionary<string, SpriteFont> fonts = new Dictionary<string,SpriteFont>();
        private List<string> fontsToLoad = new List<string>();


        public GUIManager(Game game)
            : base(game)
        {
            Game.Services.AddService(typeof(IGUIManager), this);
            contentManager = new ContentManager(Game.Services);
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            graphicsDeviceService = (IGraphicsDeviceService)Game.Services.GetService(typeof(IGraphicsDeviceService));
            inputManager = (IInputManagerService)Game.Services.GetService(typeof(IInputManagerService));
            inputManager.LeftClick += OnMouseClick;
            inputManager.MouseMove += OnMouseMove;
            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            if (rootComponent != null)
                rootComponent.Update(new Rectangle(0, 0, graphicsDeviceService.GraphicsDevice.Viewport.Width, graphicsDeviceService.GraphicsDevice.Viewport.Height));
            base.Update(gameTime);
        }

        protected override void LoadGraphicsContent(bool loadAllContent)
        {
            if (loadAllContent)
            {
                foreach (string textureString in texturesToLoad)
                {
                    Texture2D texture = contentManager.Load<Texture2D>(textureString);
                    textures[textureString] = texture;
                }

                foreach(string fontString in fontsToLoad)
                {
                    SpriteFont font = contentManager.Load<SpriteFont>(fontString);
                    fonts[fontString] = font;
                }

                spriteBatch = new SpriteBatch(graphicsDeviceService.GraphicsDevice);
            }
            base.LoadGraphicsContent(loadAllContent);
        }

        public override void Draw(GameTime gameTime)
        {
            if (rootComponent != null)
            {
                spriteBatch.Begin();
                rootComponent.Draw(spriteBatch);

                spriteBatch.End();
            }
            base.Draw(gameTime);
        }


        public void DrawQuad(SpriteBatch spriteBatch, Quad quad, Rectangle destination, Color color, QuadRenderMode horizontalRenderMode, QuadRenderMode verticalRenderMode)
        {
            Texture2D texture = GetTexture(quad.TextureName);
            if (texture == null)
                return;

            if (horizontalRenderMode == QuadRenderMode.Scaled)
            {
                if (verticalRenderMode == QuadRenderMode.Scaled)
                    spriteBatch.Draw(texture, destination, quad.Source, color);
                else
                {
                    DrawVerticalQuadTiles(spriteBatch, quad.Source, destination, color, texture);
                }
            }
            else
            {
                int horizontalPos = 0;
                while (horizontalPos < destination.Width - quad.Source.Width)
                {
                    Rectangle newDestination = new Rectangle(destination.X + horizontalPos, destination.Y, quad.Source.Width, destination.Height);

                    if (verticalRenderMode == QuadRenderMode.Scaled)
                    {
                        spriteBatch.Draw(texture, newDestination, quad.Source, color);
                    }
                    else
                    {
                        DrawVerticalQuadTiles(spriteBatch, quad.Source, newDestination, color, texture);
                    }

                    horizontalPos += quad.Source.Width;
                }

                if (horizontalPos < destination.Width)
                {
                    Rectangle newSource = quad.Source;
                    newSource.Width = destination.Width - horizontalPos;

                    Rectangle newDestination = new Rectangle(destination.X + horizontalPos, destination.Y, newSource.Width, destination.Height);
                    
                    if (verticalRenderMode == QuadRenderMode.Scaled)
                    {
                        spriteBatch.Draw(texture, newDestination, newSource, color);
                    }
                    else
                    {
                        DrawVerticalQuadTiles(spriteBatch, newSource, newDestination, color, texture);
                    }
                }
            }
        }

        public void DrawNineQuad(SpriteBatch spriteBatch, NineQuad quad, Rectangle destination, Color color)
        {
            int clientHeight = destination.Height - quad.Top.Source.Height - quad.Bottom.Source.Height;
            int clientWidth = destination.Width - quad.Left.Source.Width - quad.Right.Source.Width;

            DrawQuad(spriteBatch, quad.Center, new Rectangle(
                                                            destination.X + quad.Left.Source.Width,
                                                            destination.Y + quad.Top.Source.Width,
                                                            clientWidth,
                                                            clientHeight),
                                                    color,
                                                    QuadRenderMode.Tiled,
                                                    QuadRenderMode.Tiled);

            
            DrawQuad(spriteBatch, quad.Top, new Rectangle(
                                                       destination.Left + quad.TopLeft.Source.Width,
                                                       destination.Top,
                                                       destination.Width - quad.TopLeft.Source.Width - quad.TopRight.Source.Width,
                                                       quad.Top.Source.Height),
                                               color,
                                               QuadRenderMode.Tiled,
                                               QuadRenderMode.Scaled);

            DrawQuad(spriteBatch, quad.Bottom, new Rectangle(
                                                        destination.Left + quad.BottomLeft.Source.Width,
                                                        destination.Top + destination.Height - quad.Bottom.Source.Height,
                                                        destination.Width - quad.BottomLeft.Source.Width - quad.BottomRight.Source.Width,
                                                        quad.Bottom.Source.Height),
                                                color,
                                                QuadRenderMode.Tiled,
                                                QuadRenderMode.Scaled);
            
            DrawQuad(spriteBatch, quad.Left, new Rectangle(
                                                        destination.Left,
                                                        destination.Top + quad.TopLeft.Source.Height,
                                                        quad.Left.Source.Width,
                                                        destination.Height - quad.TopLeft.Source.Height - quad.BottomLeft.Source.Height),
                                                        color,
                                                        QuadRenderMode.Scaled,
                                                        QuadRenderMode.Tiled);

            DrawQuad(spriteBatch, quad.Right, new Rectangle(
                                                        destination.Left + destination.Width - quad.Right.Source.Width,
                                                        destination.Top + quad.TopRight.Source.Height,
                                                        quad.Right.Source.Width,
                                                        destination.Height - quad.TopRight.Source.Height - quad.BottomRight.Source.Height),
                                                        color,
                                                        QuadRenderMode.Scaled,
                                                        QuadRenderMode.Tiled);
            
            DrawQuad(spriteBatch, quad.TopLeft, new Rectangle(
                                                        destination.Left,
                                                        destination.Top,
                                                        quad.TopLeft.Source.Width,
                                                        quad.TopRight.Source.Height),
                                                        color,
                                                        QuadRenderMode.Scaled,
                                                        QuadRenderMode.Scaled);

            DrawQuad(spriteBatch, quad.TopRight, new Rectangle(
                                                        destination.Left + destination.Width - quad.TopRight.Source.Width,
                                                        destination.Top,
                                                        quad.TopRight.Source.Width,
                                                        quad.TopRight.Source.Height),
                                                        color,
                                                        QuadRenderMode.Scaled,
                                                        QuadRenderMode.Scaled);

            DrawQuad(spriteBatch, quad.BottomLeft, new Rectangle(
                                                        destination.Left,
                                                        destination.Top + destination.Height - quad.BottomLeft.Source.Height,
                                                        quad.BottomLeft.Source.Width,
                                                        quad.BottomLeft.Source.Height),
                                                        color,
                                                        QuadRenderMode.Scaled,
                                                        QuadRenderMode.Scaled);

            DrawQuad(spriteBatch, quad.BottomRight, new Rectangle(
                                                        destination.Left + destination.Width - quad.BottomRight.Source.Width,
                                                        destination.Top + destination.Height - quad.BottomRight.Source.Height,
                                                        quad.BottomRight.Source.Width,
                                                        quad.BottomRight.Source.Height),
                                                        color,
                                                        QuadRenderMode.Scaled,
                                                        QuadRenderMode.Scaled);

            /*
            spriteBatch.Draw(bottomTexture, new Rectangle(
                                    BottomLeft.Source.Width,
                                    destination.Top + clientHeight,
                                    destination.Width - BottomLeft.Source.Width - BottomRight.Source.Width,
                                    Bottom.Source.Height),
                                    Bottom.Source, Color);

            spriteBatch.Draw(textureLeft, new Rectangle(
                                    destination.Left,
                                    TopLeft.Source.Height,
                                    Left.Source.Width,
                                    destination.Height - TopLeft.Source.Height - BottomLeft.Source.Height),
                                    Left.Source, Color);

            spriteBatch.Draw(textureRight, new Rectangle(
                                    destination.Left + clientWidth,
                                    TopRight.Source.Height,
                                    Right.Source.Width,
                                    destination.Height - TopRight.Source.Height - BottomRight.Source.Height),
                                    Left.Source, Color);
             */
        }

        public void DrawString(SpriteBatch spriteBatch, string fontName, string text, Point position, Color color)
        {
            SpriteFont font = GetFont(fontName);
            if (font == null)
                return;

            spriteBatch.DrawString(font, text, new Vector2(position.X, position.Y), color);
        }

        private static void DrawVerticalQuadTiles(SpriteBatch spriteBatch, Rectangle source, Rectangle destination, Color color, Texture2D texture)
        {
            int verticalPos = 0;
            while (verticalPos < destination.Height - source.Height)
            {
                spriteBatch.Draw(texture, new Rectangle(
                                                destination.X,
                                                destination.Y + verticalPos,
                                                destination.Width,
                                                source.Height),
                                        source, color);

                verticalPos += source.Height;
            }

            if (verticalPos < destination.Height)
            {
                source.Height = destination.Height - verticalPos;
                spriteBatch.Draw(texture, new Rectangle(
                                                destination.X,
                                                destination.Y + verticalPos,
                                                destination.Width,
                                                source.Height),
                                        source, color);
            }
        }

        private Texture2D GetTexture(string texture)
        {
            if (String.IsNullOrEmpty(texture) || !textures.ContainsKey(texture))
                return null;
            else
                return textures[texture];
        }

        public void RegisterTexture(string texture)
        {
            texturesToLoad.Add(texture);
        }

        private SpriteFont GetFont(string font)
        {
            if (String.IsNullOrEmpty(font) || !fonts.ContainsKey(font))
                return null;
            else
                return fonts[font];
        }

        public void RegisterFont(string font)
        {
            fontsToLoad.Add(font);
        }

        public GUIComponent RootComponent
        {
            get
            {
                return rootComponent;
            }
            set
            {
                rootComponent = value;
            }
        }

        public void OnMouseClick(object sender, MouseEvent e)
        {
            if (rootComponent != null)
            {
                IList<GUIComponent> hits = new List<GUIComponent>();
                rootComponent.CheckCollision(e, true);
                if (hits.Count > 0)
                    hits[hits.Count - 1].OnClick(e);
            }
        }

        public void OnMouseMove(object sender, MouseEvent e)
        {
            if (rootComponent != null)
            {
                IList<GUIComponent> hits = new List<GUIComponent>();
                rootComponent.CheckCollision(e, false);
            }
        }

        private GUIComponent rootComponent;
    }
}


