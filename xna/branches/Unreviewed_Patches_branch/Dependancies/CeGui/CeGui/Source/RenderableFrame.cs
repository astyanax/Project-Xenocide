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
///		A higher order GUI entity that represents a renderable frame.
/// </summary>
/// <remarks>
///		This class is intended to be used where a (usually top-level) GUI element needs to draw a frame that is constructed from
///		a collection of Images.  It is possible to specify the image to use for each of the four corners, which are placed appropriately
///		at their natural SizeF, and the images for the four edges, which are stretched to cover the area between any corner images.  Any
///		of the Images may be omitted, in which case that part of the frame is not drawn.  If the GUI element uses only one image for its
///		frame (usually stretched over the entire area of the element) then a better choice would be to use a RenderableImage, or perform the
///		rendering directly instead.
/// </remarks>
public class RenderableFrame : RenderableElement {
  #region Fields

  /// <summary>
  ///		Image to draw for the top-left corner.
  /// </summary>
  protected Image topLeft;
  /// <summary>
  ///		Image to draw for the top-right corner.
  /// </summary>
  protected Image topRight;
  /// <summary>
  ///		Image to draw for the bottom-left corner.
  /// </summary>
  protected Image bottomLeft;
  /// <summary>
  ///		Image to draw for the bottom-right corner.
  /// </summary>
  protected Image bottomRight;
  /// <summary>
  ///		Image to draw for the left edge.
  /// </summary>
  protected Image left;
  /// <summary>
  ///		Image to draw for the right edge.
  /// </summary>
  protected Image top;
  /// <summary>
  ///		Image to draw for the top edge.
  /// </summary>
  protected Image right;
  /// <summary>
  ///		Image to draw for the bottom edge.
  /// </summary>
  protected Image bottom;

  #endregion Fields

  #region Properties

  public Image TopLeft {
    get {
      return topLeft;
    }
    set {
      topLeft = value;
    }
  }

  public Image Top {
    get {
      return top;
    }
    set {
      top = value;
    }
  }

  public Image TopRight {
    get {
      return topRight;
    }
    set {
      topRight = value;
    }
  }

  public Image Left {
    get {
      return left;
    }
    set {
      left = value;
    }
  }

  public Image Right {
    get {
      return right;
    }
    set {
      right = value;
    }
  }

  public Image BottomLeft {
    get {
      return bottomLeft;
    }
    set {
      bottomLeft = value;
    }
  }

  public Image Bottom {
    get {
      return bottom;
    }
    set {
      bottom = value;
    }
  }

  public Image BottomRight {
    get {
      return bottomRight;
    }
    set {
      bottomRight = value;
    }
  }

  #endregion

  #region Constructor

  /// <summary>
  ///		Default constructor.
  /// </summary>
  public RenderableFrame() {
  }

  #endregion Constructor

  #region Methods

  /// <summary>
  ///		Specify the Image objects to use for each part of the frame.
  /// </summary>
  /// <remarks>
  ///		'null' may be used for any parameter to omit any part of the frame.
  /// </remarks>
  /// <param name="topLeft">Reference to an Image object to render as the top-left corner of the frame.</param>
  /// <param name="topRight">Reference to an Image object to render as the top-right corner of the frame.</param>
  /// <param name="bottomLeft">Reference to an Image object to render as the bottom-left corner of the frame.</param>
  /// <param name="bottomRight">Reference to an Image object to render as the bottom-right corner of the frame.</param>
  /// <param name="left">Reference to an Image object to render as the left corner of the frame.</param>
  /// <param name="top">Reference to an Image object to render as the top corner of the frame.</param>
  /// <param name="right">Reference to an Image object to render as the right corner of the frame.</param>
  /// <param name="bottom">Reference to an Image object to render as the bottom corner of the frame.</param>
  public void SetImages(Image topLeft, Image topRight, Image bottomLeft, Image bottomRight,
      Image left, Image top, Image right, Image bottom) {

    this.topLeft = topLeft;
    this.topRight = topRight;
    this.bottomLeft = bottomLeft;
    this.bottomRight = bottomRight;
    this.Left = left;
    this.Right = right;
    this.Top = top;
    this.Bottom = bottom;
  }

  #endregion Methods

  #region RenderableElement Members

  /// <summary>
  /// 
  /// </summary>
  /// <param name="position"></param>
  /// <param name="clipRect"></param>
  protected override void DrawImpl(Vector3 position, Rect clipRect) {
    Vector3 finalPos = position;
    float orgWidth = area.Width;
    float orgHeight = area.Height;
    SizeF finalSize = new SizeF();

    // calculate 'adjustments' required to accommodate corner pieces.
    float coordAdj = 0, sizeAdj = 0;

    // draw top-edge, if required
    if(top != null) {
      // calculate adjustments required if top-left corner will be rendered.
      if(topLeft != null) {
        sizeAdj = topLeft.Width - topLeft.OffsetX;
        coordAdj = topLeft.Width;
      } else {
        coordAdj = 0;
        sizeAdj = 0;
      }

      // calculate adjustments required if top-right corner will be rendered.
      if(topRight != null) {
        sizeAdj += (topRight.Width + topRight.OffsetX);
      }

      finalSize.Width = orgWidth - sizeAdj;
      finalSize.Height = top.Height;
      finalPos.x = position.x + coordAdj;

      top.Draw(finalPos, finalSize, clipRect, colors);
    }

    // draw bottom-edge, if required
    if(bottom != null) {
      // calculate adjustments required if bottom-left corner will be rendered.
      if(bottomLeft != null) {
        sizeAdj = (bottomLeft.Width - bottomLeft.OffsetX);
        coordAdj = bottomLeft.Width;
      } else {
        coordAdj = 0;
        sizeAdj = 0;
      }

      // calculate adjustments required if bottom-right corner will be rendered.
      if(bottomRight != null) {
        sizeAdj += (bottomRight.Width + bottomRight.OffsetX);
      }

      finalSize.Width = orgWidth - sizeAdj;
      finalSize.Height = bottom.Height;
      finalPos.x = position.x + coordAdj;
      finalPos.y = position.y + orgHeight - finalSize.Height;

      bottom.Draw(finalPos, finalSize, clipRect, colors);
    }

    // reset x co-ordinate to input value
    finalPos.x = position.x;

    // draw left-edge, if required
    if(left != null) {
      // calculate adjustments required if top-left corner will be rendered.
      if(topLeft != null) {
        sizeAdj = (topLeft.Height - topLeft.OffsetY);
        coordAdj = topLeft.Height;
      } else {
        coordAdj = 0;
        sizeAdj = 0;
      }

      // calculate adjustments required if bottom-left corner will be rendered.
      if(bottomLeft != null) {
        sizeAdj += (bottomLeft.Height + bottomLeft.OffsetY);
      }

      finalSize.Height = orgHeight - sizeAdj;
      finalSize.Width = left.Width;
      finalPos.y = position.y + coordAdj;

      left.Draw(finalPos, finalSize, clipRect, colors);
    }

    // draw right-edge, if required
    if(right != null) {
      // calculate adjustments required if top-left corner will be rendered.
      if(topRight != null) {
        sizeAdj = (topRight.Height - topRight.OffsetY);
        coordAdj = topRight.Height;
      } else {
        coordAdj = 0;
        sizeAdj = 0;
      }

      // calculate adjustments required if bottom-right corner will be rendered.
      if(bottomRight != null) {
        sizeAdj += (bottomRight.Height + bottomRight.OffsetY);
      }

      finalSize.Height = orgHeight - sizeAdj;
      finalSize.Width = left.Width;
      finalPos.y = position.y + coordAdj;
      finalPos.x = position.x + orgWidth - finalSize.Width;

      right.Draw(finalPos, finalSize, clipRect, colors);
    }

    // draw required corner pieces...
    if(topLeft != null) {
      topLeft.Draw(position, clipRect, colors);
    }

    if(topRight != null) {
      finalPos.x = position.x + orgWidth - topRight.Width;
      finalPos.y = position.y;
      topRight.Draw(finalPos, clipRect, colors);
    }

    if(bottomLeft != null) {
      finalPos.x = position.x;
      finalPos.y = position.y + orgHeight - bottomLeft.Height;
      bottomLeft.Draw(finalPos, clipRect, colors);
    }

    if(bottomRight != null) {
      finalPos.x = position.x + orgWidth - bottomRight.Width;
      finalPos.y = position.y + orgHeight - bottomRight.Height;
      bottomRight.Draw(finalPos, clipRect, colors);
    }
  }

  #endregion RenderableElement Members
}

} // namespace CeGui
