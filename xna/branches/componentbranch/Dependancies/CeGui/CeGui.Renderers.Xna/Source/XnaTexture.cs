#region LGPL License
/*************************************************************************
    Crazy Eddie's GUI System (http://crayzedsgui.sourceforge.net)
    Copyright (C)2004 Paul D Turner (crayzed@users.sourceforge.net)

    C# Port developed by Chris McGuirk (leedgitar@latenitegames.com)
    Compatible with the Axiom 3D Engine (http://axiomengine.sf.net)

    This library is free software; you can redistribute it and/or
    modify it under the terms of the GNU Lesser General Public
    License as published by the Free Software Foundation; either
    version 2.1 of the License, or (at your option) any later version.

    This library is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
    Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public
    License along with this library; if not, write to the Free Software
    Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*************************************************************************/
#endregion LGPL License

using System;
using System.Collections;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CeGui.Renderers.Xna {

  /// <summary>A texture in XNA (DirectX 9.0c)</summary>
  public class XnaTexture : CeGui.Texture, IDisposable {

    /// <summary>Initializes the texture</summary>
    /// <param name="owner">Renderer who created this texture.</param>
    public XnaTexture(Renderer owner) : base(owner) { }

    /// <summary>
    ///   Support method that must be called prior to a Reset() call on the graphics device
    /// </summary>
    public void PreDeviceReset() {

      // Textures not based on files are in the managed pool, so we do
      // not worry about those.
      if(this.filename != string.Empty)
        Dispose();

    }

    /// <summary>
    ///   Support method that must be called after a Reset() call on the graphics device
    /// </summary>
    public void PostDeviceReset() {

      // Textures not based on files are in the managed pool, so we do
      // not worry about those.
      if(this.filename != string.Empty)
        LoadFromFile(this.filename);

    }

    /// <summary>Loads a texture from the specified file</summary>
    /// <param name="fileName">Name of the image file to load</param>
    public override void LoadFromFile(string fileName) {
      Dispose();

      this.texture = Texture2D.FromFile(
        (this.Renderer as XnaRenderer).GraphicsDevice, fileName
      );
      this.filename = fileName;

      // grab the inferred dimensions of the texture
      this.width = this.texture.Width;
      this.height = this.texture.Height;
    }

    /// <summary>Loads a texture file from a stream (could be in memory)</summary>
    /// <param name="buffer">Stream holding the image data to load into this texture.</param>
    /// <param name="bufferWidth">Width of the image data (in pixels).</param>
    /// <param name="bufferHeight">Height of the image data (in pixels).</param>
    public override void LoadFromMemory(
      System.IO.Stream buffer, int bufferWidth, int bufferHeight
    ) {
      Dispose();

      TextureCreationParameters parameters = new TextureCreationParameters(
        bufferWidth, bufferHeight, 1, 1,
        SurfaceFormat.Color, ResourceUsage.None, ResourceManagementMode.Automatic,
        Color.TransparentBlack, FilterOptions.Point, FilterOptions.Point
      );

      this.texture = Texture2D.FromFile(
        (this.Renderer as XnaRenderer).GraphicsDevice, buffer, parameters
      ) as Texture2D;
      this.filename = string.Empty;

      // grab the inferred dimensions of the texture
      this.width = this.texture.Width;
      this.height = this.texture.Height;
    }

    /// <summary>Loads a texture from the specified file</summary>
    /// <param name="fileName">Existing texture to hold</param>
    public void LoadFromTexture(Texture2D texture) {
      Dispose();

      this.texture = texture;
      this.filename = string.Empty;

      // grab the inferred dimensions of the texture
      this.width = this.texture.Width;
      this.height = this.texture.Height;
    }

    /// <summary>Explicitely releases all resources belonging to the instance</summary>
    public void Dispose() {
      if(texture != null) {
        this.texture.Dispose();
        this.texture = null;
      }
    }

    /// <summary>The XNA texture object</summary>
    public Texture2D InternalTexture {
      get { return this.texture; }
    }

    /// <summary>Reference to our underlying XNA texture</summary>
    protected Texture2D texture;
    /// <summary>Filename of the image the texture was loaded from, if any</summary>
    protected string filename;

  }

} // namespace CeGui.Renderers.Xna
