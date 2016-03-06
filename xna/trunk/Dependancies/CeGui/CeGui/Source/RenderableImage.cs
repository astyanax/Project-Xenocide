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
///		A higher order GUI entity that represents a renderable image with formatting options.
/// </summary>
/// <remarks>
///		This class is intended to be used where a (usually top-level) GUI element needs to draw an image that requires some additional
///		formatting.  It is possible to specify the Image that is to be rendered, as well as the horizontal and vertical formatting
///		required.
/// </remarks>
public class RenderableImage : RenderableElement {
  #region Fields

  /// <summary>
  ///		Currently set horizontal formatting option.
  /// </summary>
  protected HorizontalImageFormat horizontalFormat;
  /// <summary>
  ///		Currently set vertical formatting option.
  /// </summary>
  protected VerticalImageFormat verticalFormat;
  /// <summary>
  ///		Reference to the actual Image to be displayed.
  /// </summary>
  protected Image image;

  #endregion Fields

  #region Constructor

  /// <summary>
  ///		Default constructor.
  /// </summary>
  public RenderableImage() {
    // setup default formats
    horizontalFormat = HorizontalImageFormat.LeftAligned;
    verticalFormat = VerticalImageFormat.TopAligned;
  }

  #endregion Constructor

  #region Properties

  /// <summary>
  ///		Set the required horizontal formatting.
  /// </summary>
  /// <value>One of the <see cref="HorizontalImageFormat"/> values specifying the formatting required.</value>
  public HorizontalImageFormat HorizontalFormat {
    get {
      return horizontalFormat;
    }
    set {
      horizontalFormat = value;
    }
  }

  /// <summary>
  ///		Get/Set the Image object to be drawn by this RenderableImage.
  /// </summary>
  /// <value>
  ///		Reference to the Image object to be rendered.  
  ///		Can be 'null' to specify no image is to be rendered.
  ///	</value>
  public Image Image {
    get {
      return image;
    }
    set {
      image = value;
    }
  }

  /// <summary>
  ///		Set the required vertical formatting.
  /// </summary>
  /// <value>One of the <see cref="VerticalImageFormat"/> values specifying the formatting required.</value>
  public VerticalImageFormat VerticalFormat {
    get {
      return verticalFormat;
    }
    set {
      verticalFormat = value;
    }
  }

  #endregion Properties

  #region RenderableElement Members

  /// <summary>
  ///		Renders the imagery for a RenderableImage element.
  /// </summary>
  /// <param name="position">The final rendering position for the object.</param>
  /// <param name="clipRect">The clipping area for the rendering.  No rendering will be performed outside this area.</param>
  protected override void DrawImpl(Vector3 position, Rect clipRect) {
    // do not draw anything if there is not image
    if(image == null) {
      return;
    }

    // // calculate final clipping Rect which is intersection of RenderableImage area and supplied clipping area
    Rect finalClipper = new Rect(position.x, position.y, 0, 0);
    finalClipper.Size = area.Size;
    finalClipper = clipRect.GetIntersection(finalClipper);

    SizeF imageSize = image.Size;

    // // calculate number of times to tile image based of formatting options
    int horzTiles = (horizontalFormat == HorizontalImageFormat.Tiled) ?
        (int)((area.Width + (imageSize.Width - 1)) / imageSize.Width) : 1;

    int vertTiles = (verticalFormat == VerticalImageFormat.Tiled) ?
        (int)((area.Height + (imageSize.Height - 1)) / imageSize.Height) : 1;

    // calculate 'base' X co-ordinate, depending upon formatting
    float baseX = 0, baseY = 0;

    // calc horizontal base position
    switch(horizontalFormat) {
      case HorizontalImageFormat.Stretched:
        imageSize.Width = area.Width;
        baseX = position.x;
        break;

      case HorizontalImageFormat.Tiled:
      case HorizontalImageFormat.LeftAligned:
        baseX = position.x;
        break;

      case HorizontalImageFormat.Centered:
        baseX = position.x + ((area.Width - imageSize.Width) * 0.5f);
        break;

      case HorizontalImageFormat.RightAligned:
        baseX = position.x + area.Width - imageSize.Width;
        break;

      default:
        throw new InvalidRequestException("An unknown horizontal format was specified for a RenderableImage.");
    }

    // calc vertical base position
    switch(verticalFormat) {
      case VerticalImageFormat.Stretched:
        imageSize.Height = area.Height;
        baseY = position.y;
        break;

      case VerticalImageFormat.Tiled:
      case VerticalImageFormat.TopAligned:
        baseY = position.y;
        break;

      case VerticalImageFormat.Centered:
        baseY = position.y + ((area.Height - imageSize.Height) * 0.5f);
        break;

      case VerticalImageFormat.BottomAligned:
        baseY = position.y + area.Height - imageSize.Height;
        break;

      default:
        throw new InvalidRequestException("An unknown vertical format was specified for a RenderableImage.");
    }

    Vector3 drawPos = new Vector3(0, baseY, position.z);

    // perform actual rendering
    for(int row = 0; row < vertTiles; row++) {
      drawPos.x = baseX;

      for(int col = 0; col < horzTiles; col++) {
        image.Draw(drawPos, imageSize, finalClipper, colors);
        drawPos.x += imageSize.Width;
      }

      drawPos.y += imageSize.Height;
    }
  }

  #endregion RenderableElement Members
}

} // namespace CeGui
