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
using System.Text;
using System.Drawing;

namespace CeGui {

  /// <summary>
  ///   Enumerated type that contains the valid flags that can be to use when
  ///   rendering image
  /// </summary>
  public enum OrientationFlags {

    /// <summary>Horizontally flip the image</summary>
    FlipHorizontal = 1,
    /// <summary>Vertically flip the image</summary>
    FlipVertical = 2,
    /// <summary>Rotate the image counter-clockwise 90 degree</summary>
    RotateRightAngle = 4

  };

  /// <summary>
  ///   Enumerated type that contains the valid diagonal-mode that specifies how
  ///   a quad is split into triangles when rendered with fx. a 3D API
  /// </summary>
  public enum QuadSplitMode {

    /// <summary>Diagonal goes from top-left to bottom-right</summary>
    TopLeftToBottomRight,

    /// <summary>Diagonal goes from bottom-left to top-right</summary>
    BottomLeftToTopRight
  };

  /// <summary>The interface for custom GUI renderers</summary>
  /// <remarks>
  ///   Objects derived from Renderer are the means by which the GUI system interfaces
  ///   with specific rendering technologies. To use a rendering system or API to draw
  ///   CEGUI imagery requires that an appropriate Renderer object be available.
  /// </remarks>
  public abstract class Renderer {

    /// <summary>Initial value to use for 'z' each frame</summary>
    const float GuiZInitialValue = 1.0f;

    /// <summary>Value to step 'z' for each GUI element</summary>
    /// <remarks>Enough for 1000 windows</remarks>
    const float GuiZElementStep = 0.001f;

    /// <summary>Value to step 'z' for each GUI layer</summary>
    /// <remarks>Enough for 10 layers per window</remarks>
    const float GuiZLayerStep = 0.0001f;

    /// <summary>Add a quad to the rendering queue</summary>
    /// <remarks>
    ///   All clipping and other adjustments should have been made prior to calling this
    /// </remarks>
    /// <summary>Add a quad to the rendering queue (or render immediately)</summary>
    /// <param name="destRect">Coordinates at which to draw the quad, in pixels</param>
    /// <param name="z">Z coordinate at which to draw the quad</param>
    /// <param name="texture">Texture containing the bitmap to draw onto the quad</param>
    /// <param name="textureRect">
    ///   Region within the texture to be drawn onto the quad, in texture coordinates
    /// </param>
    /// <param name="colors">Vertex colors for each of the 4 corners</param>
    /// <param name="quadSplitMode">Where to split the quad into 2 triangles</param>
    public abstract void AddQuad(
      Rect destRect, float z, Texture texture, Rect textureRect,
      ColourRect colors, QuadSplitMode quadSplitMode
    );

    /// <summary>Perform final rendering for all quads that have been queued</summary>
    /// <remarks>
    ///   The contents of the rendering queue is retained and can be rendered again as required.
    ///   If the contents is not required call <see cref="ClearRenderList"/>
    /// </remarks>
    public abstract void DoRender();

    /// <summary>Clears all queued quads from the render queue</summary>
    public abstract void ClearRenderList();

    /// <summary>Creates a 'null' Texture object</summary>
    /// <returns>
    ///   A newly created Texture object. The returned Texture object has no size
    ///   or imagery associated with it, and is generally of little or no use.
    /// </returns>
    public abstract Texture CreateTexture();

    /// <summary>
    ///   Create a <see cref="Texture"/> object using the given image file name.
    /// </summary>
    /// <remarks>
    ///   Textures are always created with a SizeF that is a power of 2. If the file
    ///   you specify is of a SizeF that is not a power of two, the final size will be
    ///   rounded up. Additionally, textures are always square, so the ultimate sizef
    ///   is governed by the larger of the width and height of the specified file. You
    ///   can check the ultimate sizes by querying the texture after creation.
    /// </remarks>
    /// <param name="fileName">
    ///   The path and filename of the image file to use when creating the texture
    /// </param>
    /// <param name="resourceGroup">
    ///   Resource group identifier to be passed to the resource provider when loading
    ///   the texture file
    /// </param>
    /// <returns>
    ///   A newly created Texture object. The initial contents of the texture memory is
    ///   the requested image file
    /// </returns>
    public abstract Texture CreateTexture(string fileName, string resourceGroup);

    /// <summary>
    ///   Create a Texture object with the given pixel dimensions as specified by
    ///   <paramref name="size"/>
    /// </summary>
    /// <remarks>
    ///   Textures are always created with a size that is a power of 2. If you specify a
    ///   sizef that is not a power of two, the final	sizef will be rounded up. So if you
    ///   specify a sizef of 1024, the texture will be (1024 x 1024), however, if you
    ///   specify a Sizef of 1025, the texture will be (2048 x 2048). You can check the
    ///   ultimate sizef by querying the texture after creation.
    /// </remarks>
    /// <param name="size">
    ///   Float value that specifies the size to use for the width and height when creating
    ///   the new texture
    /// </param>
    /// <returns>
    ///   A newly created Texture object. The initial contents of the texture memory are
    ///   undefined / random
    /// </returns>
    public abstract Texture CreateTexture(float size);

    /// <summary>Destroy the given Texture object</summary>
    /// <param name="texture">Reference to the texture to be destroyed</param>
    public abstract void DestroyTexture(Texture texture);

    /// <summary>Destroy all texture objects</summary>
    public abstract void DestroyAllTextures();

    /// <summary>Enables or disables render queueing</summary>
    /// <value>
    ///   If false, each call to <see cref="AddQuad"/> will be rendered immediately.
    ///   If true, calls will be queued and issued in a batch during a
    ///   <see cref="DoRender"/> call.
    /// </value>
    /// <remarks>
    ///   This only affects queueing. If queueing is turned off, any calls to addQuad will
    ///   cause the quad to be rendered directly. Note that disabling queueing will not cause
    ///   currently queued quads to be rendered, nor is the queue cleared - at any time the
    ///   queue can still be drawn by calling doRender, and the list can be cleared by
    ///   calling clearRenderList. Re-enabling the queue causes subsequent quads to be added
    ///   as if queueing had never been disabled.
    /// </remarks>
    public bool QueueingEnabled {
      get { return IsQueueingEnabled; }
      set { IsQueueingEnabled = value; }
    }

    /// <summary>Return the current width of the display in pixels</summary>
    /// <value>Float value equal to the current width of the display in pixels.</value>
    public abstract float Width { get; }

    /// <summary>Return the current height of the display in pixels</summary>
    /// <value>Float value equal to the current height of the display in pixels</value>
    public abstract float Height { get; }

    /// <summary>Return the size of the display in pixels</summary>
    /// <value>A size object containing the dimensions of the current display</value>
    public abstract SizeF Size { get; }

    /// <summary>Return a <see cref="Rect"/> describing the screen</summary>
    /// <value>
    ///   A Rect object that describes the screen area. Typically, the top-left
    ///   values are always 0, and the size of the area described is equal to
    ///   the screen resolution.
    /// </value>
    public abstract Rect Rect { get; }

    /// <summary>Return the maximum texture size available</summary>
    /// <value>
    ///   Size of the maximum supported texture in pixels (textures are always
    ///   assumed to be square)
    /// </value>
    public abstract int MaxTextureSize { get; }

    /// <summary>Return the horizontal display resolution dpi</summary>
    /// <value>Horizontal resolution of the display in dpi</value>
    public abstract int HorizontalScreenDPI { get; }

    /// <summary>Return the vertical display resolution dpi</summary>
    /// <value>Vertical resolution of the display in dpi</value>
    public abstract int VerticalScreenDPI { get; }

    /// <summary>The current z coordinate value</summary>
    protected float currentZ;

    /// <summary>
    ///   Fires when the underlying display mode had changed.
    /// </summary>
    /// <remarks>
    ///   It is important that all Renderer implementers fire this properly as the
    ///   system itself subscribes to this event.
    /// </remarks>
    public event GuiEventHandler DisplayModeChanged;

    /// <summary>
    ///   Internal method for firing the <see cref="DisplayModeChanged"/> event.
    /// </summary>
    /// <param name="e">Event arguments.</param>
    protected void OnDisplayModeChanged(GuiEventArgs e) {

      if(DisplayModeChanged != null)
        DisplayModeChanged(this, e);

    }

    /// <summary>Default constructor</summary>
    protected Renderer() {
      // Intialize Z to its starting value
      ResetZValue();
    }

    /// <summary>Reset the z co-ordinate for rendering</summary>
    public void ResetZValue() {
      currentZ = GuiZInitialValue;
    }

    /// <summary>Update the z co-ordinate for the next major UI element (window)</summary>
    public void AdvanceZValue() {
      currentZ -= GuiZElementStep;
    }

    /// <summary>Current Z value to use (equates to layer 0 for this UI element).</summary>
    /// <remarks>
    ///   A float value that specifies the z coordinate to be used for layer 0 on the
    ///   current GUI element
    /// </remarks>
    public float CurrentZ {
      get { return currentZ; }
    }

    /// <summary>
    ///   Returns the z co-ordinate to use for the requested layer on the current GUI element
    /// </summary>
    /// <param name="layer">
    ///   Specifies the layer to return the Z co-ordinate for. Each GUI element can use up
    ///   to 10 layers, so valid inputs are 0 to 9 inclusive. If you specify an invalid
    ///   value, results are undefined.
    /// </param>
    /// <returns></returns>
    public float GetZLayer(int layer) {

      // TODO: Throw exception for layer out of range?
      return currentZ - ((float)layer * GuiZLayerStep);

    }

    // <summary>
    //   Return identification string for the renderer module.  If the internal id string
    //   has not been set by the Renderer module creator, a generic string of
    //   "Unknown renderer" will be returned.
    // </summary>
    //public string IdentifierString {
    //  get { return this.identifierString; }
    //}

    /// <summary>
    ///   If false, each call to <see cref="AddQuad"/> will be rendered immediately.
    ///   If true, calls will be queued and issued in a batch during a
    ///   <see cref="DoRender"/> call
    /// </summary>
    protected bool IsQueueingEnabled;

  }

} // namespace CeGui
