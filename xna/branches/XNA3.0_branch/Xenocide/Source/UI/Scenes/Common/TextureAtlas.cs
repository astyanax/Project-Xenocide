#region Copyright
/*
--------------------------------------------------------------------------------
This source file is part of Xenocide
  by  Project Xenocide Team

For the latest info on Xenocide, see http://www.projectxenocide.com/

This work is licensed under the Creative Commons
Attribution-NonCommercial-ShareAlike 2.5 License.

To view a copy of this license, visit
http://creativecommons.org/licenses/by-nc-sa/2.5/
or send a letter to Creative Commons, 543 Howard Street, 5th Floor,
San Francisco, California, 94105, USA.
--------------------------------------------------------------------------------
*/

/*
* @file TextureAtlas.cs
* @date Created: 2008/01/12
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Xenocide.Resources;




#endregion

namespace ProjectXenocide.UI.Scenes
{
    /// <summary>
    /// A bitmap that contains a number of textures
    /// </summary>
    public class TextureAtlas
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="width">Width of the bitmap (in pixels)</param>
        /// <param name="height">Height of the bitmap (in pixels)</param>
        public TextureAtlas(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        /// <summary>
        /// Load/create the graphic resources needed by the terrain
        /// </summary>
        /// <param name="device">the display</param>
        /// <param name="filename">name of file holding the bitmap</param>
        public void LoadContent(GraphicsDevice device, string filename)
        {
            bitmap = Texture2D.FromFile(device, filename);
            if ((bitmap.Height != height) || (bitmap.Width != width))
            {
                throw new ArgumentException(Strings.EXCEPTION_BITMAP_WRONG_SIZE);
            }
        }

        /// <summary>
        /// Define the position of a texture in the bitmap
        /// </summary>
        /// <param name="x">leftmost pixel of texture (0 = left edge)</param>
        /// <param name="y">topmost pixel of texture (0 = top edge) </param>
        /// <param name="w">width of texture (in pixels)</param>
        /// <param name="h">height of texture (in pixels)</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "x+1")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "y+1")]
        public void DefineTexture(int x, int y, int w, int h)
        {
            // due to "bleeding" from pixel sampler's linear filtering,
            // need to leave a one texel "gutter" around the textures
            float texWidth = (x + w - 2) / width;
            float texHeight = (y + h - 2) / height;
            textureCoords.Add(new Coord(new Vector4((x + 1) / width, (y + 1) / height, texWidth, texHeight)));
        }

        /// <summary>
        /// Holds UV co-ordinates of texture in atlas
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible",
            Justification = "We can handle nested types")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes",
            Justification = "Should never compare two cells")]
        public struct Coord
        {
            /// <summary>
            /// Ctor
            /// </summary>
            /// <param name="data">layout is left, top, right, bottom</param>
            public Coord(Vector4 data)
            {
                this.data = data;
            }

            /// <summary>
            /// Flip the horizontal co-ordinates
            /// </summary>
            public void FlipHorizontal()
            {
                float t = data.X;
                data.X = data.Z;
                data.Z = t;
            }

            #region Fields

            /// <summary>Return the UV co-ordinates of texture's Top Left corner</summary>
            public Vector2 LeftTop { get { return new Vector2(data.X, data.Y); } }

            /// <summary>Return the UV co-ordinates of texture's Bottom Left corner</summary>
            public Vector2 LeftBottom { get { return new Vector2(data.X, data.W); } }

            /// <summary>Return the UV co-ordinates of texture's Top Right corner</summary>
            public Vector2 RightTop { get { return new Vector2(data.Z, data.Y); } }

            /// <summary>Return the UV co-ordinates of texture's Bottom Right corner</summary>
            public Vector2 RightBottom { get { return new Vector2(data.Z, data.W); } }

            /// <summary>The UV co-ordinates of texture</summary>
            /// <remarks>layout is left, top, right, bottom</remarks>
            private Vector4 data;

            #endregion Fields
        }

        /// <summary>
        /// Construct and return the default texture atlas
        /// </summary>
        /// <param name="device">the display</param>
        /// <returns>Default texture atlas</returns>
        public static TextureAtlas DefaultAtlas(GraphicsDevice device)
        {
            TextureAtlas atlas = new TextureAtlas(512, 512);
            atlas.LoadContent(device, @"Content\Textures\Battlescape\textureAtlas.png");
            atlas.DefineTexture(0, 0, 0, 0); // dummy entry, "there is no texture"
            atlas.DefineTexture(384, 0, 128, 128); // floor, 4 grey tiles
            atlas.DefineTexture(0, 0, 1, 1); // ToDo: water
            atlas.DefineTexture(0, 0, 1, 1); // ToDo: X-Corp start/exit
            atlas.DefineTexture(0, 0, 1, 1); // ToDo: Grav Lift
            atlas.DefineTexture(127, 257, 128, 255); // house wall with wallpaper
            atlas.DefineTexture(0, 0, 1, 1); // ToDo: Door
            atlas.DefineTexture(0, 0, 1, 1); // ToDo: Window
            atlas.DefineTexture(319, 65, 64, 64); // floor, brick
            atlas.DefineTexture(384, 128, 128, 128); // floor, grass
            atlas.DefineTexture(128, 0, 128, 128); // floor, 16 blue tiles
            atlas.DefineTexture(0, 0, 128, 128); // shiny metal wall
            atlas.DefineTexture(0, 128, 128, 128); // shiny yellow wall
            return atlas;
        }

        #region Fields

        /// <summary>The co-ordinates of each texture in the bitmap</summary>
        public IList<Coord> TextureCoords { get { return textureCoords; } }

        /// <summary>The bitmap holding the textures</summary>
        public Texture2D Bitmap { get { return bitmap; } }

        /// <summary>The co-ordinates of each texture in the bitmap</summary>
        private List<Coord> textureCoords = new List<Coord>();

        /// <summary>Width of the bitmap (in pixels)</summary>
        private float width;

        /// <summary>Height of the bitmap (in pixels)</summary>
        private float height;

        /// <summary>The bitmap holding the textures</summary>
        private Texture2D bitmap;

        #endregion Fields
    }
}
