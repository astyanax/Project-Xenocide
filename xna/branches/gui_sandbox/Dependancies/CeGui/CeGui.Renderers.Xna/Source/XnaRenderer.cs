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
using System.Collections.Generic;
using System.Drawing;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using XnaColor = Microsoft.Xna.Framework.Graphics.Color;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;
using XnaVector3 = Microsoft.Xna.Framework.Vector3;

namespace CeGui.Renderers.Xna {

  /// <summary>Renders GUIs using XNA (DirectX 9.0c)</summary>
  /// <remarks>
  ///   The GraphicsDevice can theoretically be shut down and replaced in an XNA application,
  ///   so the XNA renderer must be able to give up its GraphicsDevice entirely and
  ///   migrate to another one at will. This can be achieved by simply reassigning the
  ///   GraphicsDevice property of the XNA renderer instance.
  /// </remarks>
  public class XnaRenderer : CeGui.Renderer, IDisposable {

    /// <summary>The size of a single vertex batch</summary>
    /// <remarks>
    ///   The optimal size is entirely dependent on the generation of video card used
    ///   and should not be hardcoded for ideal performance. However, there isn't
    ///   really that much to gain besides a huge increase in complexity, so we'll use
    ///   a well-working default size.
    /// </remarks>
    public const int VertexBatchSize = 4096;

    /// <summary>Initializes the XNA renderer</summary>
    /// <param name="maxQuads">
    ///   Maximum number of quads that the Renderer will be able to render per frame
    /// </param>
    public XnaRenderer(int maxQuads) : this(null, maxQuads) { }

    /// <summary>Initializes the XNA renderer</summary>
    /// <param name="graphicsDevice">GraphicsDevice to use for rendering</param>
    /// <param name="maxQuads">
    ///   Maximum number of quads that the Renderer will be able to render per frame
    /// </param>
    public XnaRenderer(GraphicsDevice graphicsDevice, int maxQuads) {
      this.graphicsDevice = graphicsDevice;

      this.maxVertices = maxQuads * 6; // vertices for two triangles in triangle list mode
      this.vertices = new VertexPositionColorTexture[this.maxVertices];
      this.textures = new List<XnaTexture>();

      // Bring the render list into a valid state
      ClearRenderList();

      // If we have a workable graphics device already, prepare the vertex buffer
      if(graphicsDevice != null)
        createGraphicsDeviceResources();
    }

    /// <summary>Releases the renderer's resources immediately</summary>
    public void Dispose() {
      destroyGraphicsDeviceResources();
    }

    /// <summary>Custom effect to render the GUI with</summary>
    /// <remarks>
    ///   <para>
    ///     The XNA renderer allows developers to set their own effect files in order to
    ///     achieve advanced GUI effects. This feature has not been natively foreseen by
    ///     CeGui, so it's only possible to apply one effect to the entire GUI. Still,
    ///     it allows you to do add pixel shader effects like a water mark, image distortion,
    ///     or a broken TV effect - great for virtual computers in your game!
    ///   </para>
    ///   <para>
    ///     If set to null, a default shader will be used instead.
    ///   </para>
    ///   <para>
    ///     When a custom effect is used, the application is still responsible for
    ///     maintaining the effect, this includes disposing it on shutdown and
    ///     recreating it when the device resets!
    ///   </para>
    /// </remarks>
    public Effect Effect {
      get { return this.effect; }
      set { this.effect = value; }
    }

    /// <summary>
    ///   Support method that must be called prior to a Reset() call on the graphics device.
    /// </summary>
    /// <remarks>
    ///   If you are using the XNA graphics device manager, this is the method to call
    ///   upon receiving a DeviceResetting event.
    /// </remarks>
    public void PreDeviceReset() {

      // Kill off the vertex buffer (the VB is in the default resource pool because dynamic
      // vertex buffers cannot be in the managed one).
      destroyGraphicsDeviceResources();

      // Let the textures know that the device is resetting
      foreach(XnaTexture texture in this.textures)
        texture.PreDeviceReset();

    }

    /// <summary>
    ///   Support method that must be called after a Reset() call on the graphics device
    /// </summary>
    /// <remarks>
    ///   If you are using the XNA graphics device manager, this is the method to call
    ///   upon receiving a DeviceReset event.
    /// </remarks>
    public void PostDeviceReset() {

      // Recreate the vertex buffer on the reset or newly created graphics device
      createGraphicsDeviceResources();

      // Let the textures know that the grapics device has been reset
      foreach(XnaTexture texture in this.textures)
        texture.PostDeviceReset();

      // Now that we've come back, we MUST ensure a full redraw is done since the
      // textures in the stored quads will have been invalidated.
      GuiSystem.Instance.SignalRedraw();

    }

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
    public override void AddQuad(
      Rect destRect, float z, Texture texture, Rect textureRect,
      ColourRect colors, QuadSplitMode quadSplitMode
    ) {
      Texture2D xnaTexture = (texture as XnaTexture).InternalTexture;

      // Is this a quad we should render directly?
      if(!IsQueueingEnabled) {
        Effect effect = (this.effect == null) ? this.defaultEffect : this.effect;

        // Generate the required vertices
        VertexPositionColorTexture[] tempVertices = new VertexPositionColorTexture[6];
        generateQuadVertices(
          tempVertices, 0, destRect, z, textureRect, colors, quadSplitMode
        );

        effect.Begin();
        foreach(EffectPass pass in effect.CurrentTechnique.Passes) {
          pass.Begin();
          this.graphicsDevice.Textures[0] = xnaTexture;
          this.graphicsDevice.DrawUserPrimitives<VertexPositionColorTexture>(
            PrimitiveType.TriangleList, tempVertices, 0, 2
          );
          pass.End();
        }
        effect.End();

      } else { // Persistent vertices are appended to the vertex queue

        // Make sure the vertices for this quad will fit into the vertex buffer
        int remainingSpace = this.maxVertices - this.currentOperation.EndVertex;
        if(remainingSpace < 6)
          throw new ApplicationException("Too many quads. Try increasing maxQuads.");

        // If the texture changed since the last call, begin a new rendering operation
        if(this.currentOperation.Texture != xnaTexture) {
          this.currentOperation = new RenderOperation(
            this.currentOperation.EndVertex, xnaTexture
          );
          this.operations.Add(this.currentOperation);
        }

        // Initialize the quad and append the vertices to our vertex list
        generateQuadVertices(
          this.vertices, this.currentOperation.EndVertex,
          destRect, z, textureRect, colors, quadSplitMode
        );
        this.currentOperation.EndVertex += 6;

        // Remember that the vertex buffer needs to be updated
        this.vertexBufferUpToDate = false;
      }

    }

    /// <summary>Perform final rendering for all quads that have been queued</summary>
    /// <remarks>
    ///   The contents of the rendering queue is retained and can be rendered again as required.
    ///   If the contents is not required call <see cref="ClearRenderList"/>
    /// </remarks>
    public override void DoRender() {
      updateVertexBuffer();

      Effect effect = (this.effect == null) ? this.defaultEffect : this.effect;

      // Select our vertex buffer into the device
      this.graphicsDevice.Vertices[0].SetSource(
        this.vertexBuffer, 0, VertexPositionColorTexture.SizeInBytes
      );
      this.graphicsDevice.VertexDeclaration = this.vertexDeclaration;

      int passFirstOp      = 0;
      int passStartVertex  = 0;
      int currentOperation = passFirstOp;
      int drawStart        = passStartVertex;
      int batchLeft        = VertexBatchSize; 
      effect.Begin();
      while (passFirstOp < this.operations.Count) {
        foreach(EffectPass pass in effect.CurrentTechnique.Passes) {
          pass.Begin();

          currentOperation = passFirstOp;
          drawStart        = passStartVertex;
          batchLeft        = VertexBatchSize;

          while((currentOperation < this.operations.Count) && (3 < batchLeft)) {
            RenderOperation operation = this.operations[currentOperation];

            // now figure out the maximum number of vertices we can render in one call
            int drawCount = operation.EndVertex - drawStart;
            if (batchLeft < drawCount) {
              // we don't have enough space left in the "batch" to finish the op
              drawCount = batchLeft - batchLeft % 3;
            } else {
              // we're going to finish the op
              ++currentOperation;
            }

            this.graphicsDevice.Textures[0] = operation.Texture;
            this.graphicsDevice.DrawPrimitives(
              PrimitiveType.TriangleList, drawStart, drawCount / 3
            );

            drawStart += drawCount;
            batchLeft -= drawCount;
          };
          pass.End();
        }
        passFirstOp     = currentOperation;
        passStartVertex = drawStart;
      }
      effect.End();
    }

    /// <summary>Clears all queued quads from the render queue</summary>
    public override void ClearRenderList() {
      this.operations = new System.Collections.Generic.List<RenderOperation>();

      // Ensure there always is a currentOperation available. This saves us from
      // checking whether the current operation is null each time vertices are added.
      this.currentOperation = new RenderOperation(0, null);
    }

    /// <summary>Creates a 'null' Texture object</summary>
    /// <returns>
    ///   A newly created Texture object. The returned Texture object has no size
    ///   or imagery associated with it, and is generally of little or no use.
    /// </returns>
    public override Texture CreateTexture() {
      XnaTexture newTexture = new XnaTexture(this);
      this.textures.Add(newTexture);

      return newTexture;
    }

    /// <summary>
    ///   Create a Texture object with the given pixel dimensions as specified by
    ///   <paramref name="size"/>
    /// </summary>
    /// <remarks>
    ///   Textures are always created with a size that is a power of 2. If you specify a
    ///   size that is not a power of two, the final	sizef will be rounded up. So if you
    ///   specify a size of 1024, the texture will be (1024 x 1024), however, if you
    ///   specify a size of 1025, the texture will be (2048 x 2048). You can check the
    ///   ultimate size by querying the texture after creation.
    /// </remarks>
    /// <param name="size">
    ///   Float value that specifies the size to use for the width and height when creating
    ///   the new texture
    /// </param>
    /// <returns>
    ///   A newly created Texture object. The initial contents of the texture memory are
    ///   undefined / random
    /// </returns>
    public override Texture CreateTexture(float size) {
      throw new Exception("The method or operation is not implemented.");
    }

    /// <summary>
    ///   Create a <see cref="Texture"/> object using the given image file name.
    /// </summary>
    /// <remarks>
    ///   Textures are always created with a size that is a power of 2. If the file
    ///   you specify is of a size that is not a power of two, the final size will be
    ///   rounded up. Additionally, textures are always square, so the ultimate size
    ///   is governed by the larger of the width and height of the specified file. You
    ///   can check the ultimate sizes by querying the texture after creation.
    /// </remarks>
    /// <param name="fileName">
    ///   The path and filename of the image file to use when creating the texture
    /// </param>
    /// <returns>
    ///   A newly created Texture object. The initial contents of the texture memory is
    ///   the requested image file
    /// </returns>
    public override Texture CreateTexture(string filename, string resourceGroup) {
      XnaTexture newTexture = new XnaTexture(this);
      this.textures.Add(newTexture);

      newTexture.LoadFromFile(filename);

      return newTexture;
    }

    /// <summary>Destroy all texture objects</summary>
    public override void DestroyAllTextures() {
      foreach(XnaTexture texture in this.textures)
        texture.Dispose();

      this.textures.Clear();
    }

    /// <summary>Destroy the given Texture object</summary>
    /// <param name="texture">Reference to the texture to be destroyed</param>
    public override void DestroyTexture(Texture texture) {
      if(texture != null)
        ((IDisposable)texture).Dispose();

    }

    /// <summary>Return the current width of the display in pixels</summary>
    /// <value>Float value equal to the current width of the display in pixels.</value>
    public override float Width {
      get { return graphicsDevice.Viewport.Width; }
    }

    /// <summary>Return the current height of the display in pixels</summary>
    /// <value>Float value equal to the current height of the display in pixels</value>
    public override float Height {
      get { return graphicsDevice.Viewport.Height; }
    }

    /// <summary>Return the size of the display in pixels</summary>
    /// <value>A size object containing the dimensions of the current display</value>
    public override SizeF Size {
      get {
        return new SizeF(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height);
      }
    }

    /// <summary>Return a <see cref="Rect"/> describing the screen</summary>
    /// <value>
    ///   A Rect object that describes the screen area. Typically, the top-left
    ///   values are always 0, and the size of the area described is equal to
    ///   the screen resolution.
    /// </value>
    public override Rect Rect {
      get {
        return new Rect(
          0, 0,
          graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height
        );
      }
    }

    /// <summary>Return the maximum texture size available</summary>
    /// <value>
    ///   Size of the maximum supported texture in pixels (textures are always
    ///   assumed to be square)
    /// </value>
    public override int MaxTextureSize {
      get { throw new Exception("The method or operation is not implemented."); }
    }

    /// <summary>Return the horizontal display resolution dpi</summary>
    /// <value>Horizontal resolution of the display in dpi</value>
    public override int HorizontalScreenDPI {
      get { return 96; }
    }

    /// <summary>Return the vertical display resolution dpi</summary>
    /// <value>Vertical resolution of the display in dpi</value>
    public override int VerticalScreenDPI {
      get { return 96; }
    }

    /// <summary>The graphics device used by the renderer</summary>
    /// <remarks>
    ///   This property can be reassigned at runtime to migrate the renderer to another
    ///   graphics device. It is also valid to assign 'null' to this property, which will
    ///   cause the renderer to give up all its graphics device dependent resources and
    ///   stop drawing until a new graphics device is assigned.
    /// </remarks>
    public GraphicsDevice GraphicsDevice {
      get { return this.graphicsDevice; }
      set {
        if(this.graphicsDevice != null)
          destroyGraphicsDeviceResources();

        this.graphicsDevice = value;

        if(this.graphicsDevice != null)
          createGraphicsDeviceResources();
      }
    }

    /// <summary>Queueable rendering operation</summary>
    private class RenderOperation {

      /// <summary>Constructs a textured RenderOperation</summary>
      /// <param name="startVertex">Starting vertex of the RenderOperation</param>
      /// <param name="texture">The texture to be selected when rendering</param>
      public RenderOperation(int startVertex, Texture2D texture) {
        this.StartVertex = startVertex;
        this.EndVertex = startVertex;
        this.Texture = texture;
      }

      /// <summary>First vertex to draw</summary>
      public int StartVertex;
      /// <summary>Vertex after the last vertex to draw</summary>
      public int EndVertex;
      /// <summary>Texture to use. Can be null</summary>
      public Texture2D Texture;

    }

    /// <summary>Creates the vertex buffer for the renderer</summary>
    private void createGraphicsDeviceResources() {
      // Make sure we always dispose the old buffer because gfx resources are precious
      destroyGraphicsDeviceResources();

      // Create a new vertex buffer
      this.vertexBuffer = new VertexBuffer(
        this.graphicsDevice,
        typeof(VertexPositionColorTexture),
        this.maxVertices,
        ResourceUsage.Dynamic | ResourceUsage.WriteOnly,
        ResourceManagementMode.Manual
      );

      // Also recreate the vertex declaration
      this.vertexDeclaration = new VertexDeclaration(
        this.graphicsDevice, VertexPositionColorTexture.VertexElements
      );

      this.defaultEffect = DefaultEffect.Compile(this.graphicsDevice);
      this.effect = DefaultEffect.Compile(this.graphicsDevice);
    }

    /// <summary>Destroys the vertex buffer of the renderer</summary>
    private void destroyGraphicsDeviceResources() {
      // Destroy the default effect, if created
      if(this.defaultEffect != null) {
        this.defaultEffect.Dispose();
        this.defaultEffect = null;
      }
      // Destroy the vertex declaration, if any
      if(this.vertexDeclaration != null) {
        this.vertexDeclaration.Dispose();
        this.vertexDeclaration = null;
      }
      // Destroy the vertex buffer, if any
      if(this.vertexBuffer != null) {
        this.vertexBuffer.Dispose();
        this.vertexBuffer = null;
      }
    }

    /// <summary>Generates the vertices for a single quad in the internal vertex array</summary>
    /// <param name="target">Array into which to write the quad</param>
    /// <param name="startIndex">Start index within the internal vertex array</param>
    /// <param name="destRect">Rectangle at which the quad will be drawn</param>
    /// <param name="z">Desired z coordinate of the vertices</param>
    /// <param name="textureRect">Texture coordinates of the texture region to use</param>
    /// <param name="colors">Vertex colors at the 4 corners of the quad</param>
    /// <param name="quadSplitMode">How to divide the quad into triangles</param>
    private void generateQuadVertices(
      VertexPositionColorTexture[] target, int startIndex,
      Rect destRect, float z, Rect textureRect, ColourRect colors, QuadSplitMode quadSplitMode
    ) {
      // Adjust the screen coordinates to hit the pixel centers. This is explained in detail in
      // the DirectX documentation. If we wouldn't do this, the image might appear blurred
      // (with texture filtering turned on) or a few pixels might become displaced because of
      // rounding errors (with texture filtering turned off).
      destRect.Top = (float)(int)destRect.Top - 0.5f;
      destRect.Left = (float)(int)destRect.Left - 0.5f;
      destRect.Right = (float)(int)destRect.Right - 0.5f;
      destRect.Bottom = (float)(int)destRect.Bottom - 0.5f;

      // Transform the coordinates into normalized viewport positions (ranging
      // from -1.0f to +1.0f on both axes). This is the way the vertex shader sees
      // the screen and thus we do not need to pass any arguments of our own to the effect.
      destRect.Top = 1.0f - (destRect.Top / this.Size.Height * 2.0f);
      destRect.Left = (destRect.Left / this.Size.Width * 2.0f) - 1.0f;
      destRect.Bottom = 1.0f - (destRect.Bottom / this.Size.Height * 2.0f);
      destRect.Right = (destRect.Right / this.Size.Width * 2.0f) - 1.0f;

      // We shamelessly ignore quadSplitMode. It has no effect in a properly implemented renderer.
      //if(quadSplitMode) ...

      // First triangle
      target[startIndex + 0].Position = new XnaVector3(destRect.Left, destRect.Top, z);
      target[startIndex + 0].Color = xnaColorFromColour(colors.topLeft);
      target[startIndex + 0].TextureCoordinate =
        new XnaVector2(textureRect.Left, textureRect.Top);

      target[startIndex + 1].Position = new XnaVector3(destRect.Left, destRect.Bottom, z);
      target[startIndex + 1].Color = xnaColorFromColour(colors.bottomLeft);
      target[startIndex + 1].TextureCoordinate =
        new XnaVector2(textureRect.Left, textureRect.Bottom);

      target[startIndex + 2].Position = new XnaVector3(destRect.Right, destRect.Bottom, z);
      target[startIndex + 2].Color = xnaColorFromColour(colors.bottomRight);
      target[startIndex + 2].TextureCoordinate =
        new XnaVector2(textureRect.Right, textureRect.Bottom);

      // Second triangle
      target[startIndex + 3] = target[startIndex + 0];

      target[startIndex + 4] = target[startIndex + 2];

      target[startIndex + 5].Position = new XnaVector3(destRect.Right, destRect.Top, z);
      target[startIndex + 5].Color = xnaColorFromColour(colors.topRight);
      target[startIndex + 5].TextureCoordinate =
        new XnaVector2(textureRect.Right, textureRect.Top);

    }

    /// <summary>Converts a CeGui Colour value into an XNA Color value</summary>
    /// <param name="colour">CeGui Colour to be converted</param>
    /// <returns>The matching XNA Color value</returns>
    private static XnaColor xnaColorFromColour(Colour colour) {
      int argb = colour.ToARGB();

      return new XnaColor(
        (byte)(argb >> 16),
        (byte)(argb >> 8),
        (byte)(argb >> 0),
        (byte)(argb >> 24)
      );
    }

    /// <summary>Writes the current vertex list into the vertex buffer</summary>
    /// <remarks>
    ///   <para>
    ///     Normally, updating the vertex buffer can be a costly operation because AGP
    ///     memory needs to the locked and written into. Also, if you lock a vertex buffer,
    ///     XNA would have to wait until the graphics card is finished drawing the
    ///     current contents of the vertex buffer before granting access to modify it.
    ///   </para>
    ///   <para>
    ///     Locking with the 'Discard' flag actually tells XNA to create a new
    ///     vertex buffer that will be available for writing immediately. The old vertex
    ///     buffer is deleted as soon as the graphics card has finished drawing its vertices.
    ///   </para>
    /// </remarks>
    private void updateVertexBuffer() {
      if(this.vertexBufferUpToDate)
        return;

      // Put the vertex batch into the vertex buffer
      this.vertexBuffer.SetData<VertexPositionColorTexture>(
        this.vertices, 0, this.maxVertices, SetDataOptions.Discard
      );

      // The vertex buffer now is up-to-date once again
      this.vertexBufferUpToDate = true;
    }

    /// <summary>The XNA graphics device we're currently using for rendering</summary>
    private GraphicsDevice graphicsDevice;
    /// <summary>Primary vertex buffer used for caching primitives</summary>
    private VertexBuffer vertexBuffer;
    /// <summary>The vertex declaration for our GUI vertices</summary>
    private VertexDeclaration vertexDeclaration;
    /// <summary>List of all textures this renderer has created</summary>
    private List<XnaTexture> textures;

    /// <summary>All vertex batches enqueued for rendering so far</summary>
    private List<RenderOperation> operations;
    /// <summary>Cached reference to the current RenderOperation</summary>
    private RenderOperation currentOperation;
    /// <summary>Array used to cache geometry for the vertex buffer</summary>
    private VertexPositionColorTexture[] vertices;
    /// <summary>Maximum number of vertices allowed in the vertex array</summary>
    private int maxVertices;
    /// <summary>Whether the vertex buffer is up to date with the vertices array</summary>
    private bool vertexBufferUpToDate;

    /// <summary>The effect currently used to render the GUI (can be null)</summary>
    private Effect effect;
    /// <summary>Default effect for rendering, used when 'effect' field is null</summary>
    private Effect defaultEffect;

  }

} // namespace CeGui.Renderers.Xna
