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
///		Base class for all renderable elements.
/// </summary>
public abstract class RenderableElement {
  #region Fields

  /// <summary>
  ///		Link to another RenderableElement.
  /// </summary>
  protected RenderableElement next;
  /// <summary>
  ///		Colors to be used for this element.
  /// </summary>
  protected ColourRect colors;
  /// <summary>
  ///		Currently defined area for this element.
  /// </summary>
  protected Rect area;

  #endregion Fields

  #region Constructors

  /// <summary>
  ///		Constructor.
  /// </summary>
  public RenderableElement() {
    colors = new ColourRect();
    area = new Rect(0, 0, 0, 0);
  }

  #endregion Constructors

  #region Base Members

  #region Methods

  /// <summary>
  ///		Draw the element chain starting with this element.
  /// </summary>
  /// <param name="position">
  ///		Vector3 object describing the base position to be used when rendering the element chain.  Each element
  ///		in the chain will be offset from this position by it's own internal position setting.
  /// </param>
  /// <param name="clipRect">Rect describing the clipping area.  No rendering will appear outside this area.</param>
  public void Draw(Vector3 position, Rect clipRect) {
    Vector3 finalPosition = position;
    finalPosition.x += area.Left;
    finalPosition.y += area.Top;

    // call implementation method for perform actual rendering
    DrawImpl(finalPosition, clipRect);

    // render the next element in the chain (if any)
    if(next != null) {
      next.Draw(position, clipRect);
    }
  }

  /// <summary>
  ///		Sets the colors to be applied when rendering the element.
  /// </summary>
  /// <param name="colors">ColorRect object describing the colors to be used.</param>
  public void SetColors(ColourRect colors) {
    this.colors = colors;
  }

  /// <summary>
  ///		Sets the colors to be applied when rendering the element.
  /// </summary>
  /// <param name="topLeft">Color to be applied to the top-left corner of each Image used in the element.</param>
  /// <param name="topRight">Color to be applied to the top-right corner of each Image used in the element.</param>
  /// <param name="bottomLeft">Color to be applied to the bottom-left corner of each Image used in the element.</param>
  /// <param name="bottomRight">Color to be applied to the bottom-right corner of each Image used in the element.</param>
  public void SetColors(Colour topLeft, Colour topRight, Colour bottomLeft, Colour bottomRight) {
    colors.topLeft = topLeft;
    colors.topRight = topRight;
    colors.bottomLeft = bottomLeft;
    colors.bottomRight = bottomRight;
  }

  #endregion Methods

  #region Properties

  /// <summary>
  ///		Get the rendering colors set for this RenderableElement.
  /// </summary>
  /// <value>ColourRect describing the colors to be used when rendering this RenderableElement.</value>
  public ColourRect Colors {
    get {
      return colors;
    }
  }

  /// <summary>
  ///		Get/Set another REnderableElement to this one.
  /// </summary>
  /// <remarks>
  ///		The linked element will be drawn whenever this element is drawn using the same base position and clipping area
  ///		as provided when the <see cref="RenderableElement.Draw"/> method is called.  Whole chains of Renderable Elements can be
  ///		created using this system.
  /// </remarks>
  /// <value>Reference of a RenderableElement object that will be linked to this element.</value>
  public RenderableElement NextElement {
    get {
      return next;
    }
    set {
      next = value;
    }
  }

  /// <summary>
  ///		Get/Set the offset position of this RenderableElement.
  /// </summary>
  /// <value>
  ///		Point describing the offset position that this RenderableElement is to be rendered at.  This
  ///		value is added to whatever position is specified when the <see cref="RenderableElement.Draw"/> method 
  ///		is called to obtain the final rendering position.
  /// </value>
  public PointF Position {
    get {
      return new PointF(area.Left, area.Top);
    }
    set {
      area.Position = value;
    }
  }

  /// <summary>
  ///		Get/Set the area for the element.
  /// </summary>
  /// <value>Rect describing the pixel area (offset from some unknown location) of the frame.</value>
  public Rect Rect {
    get {
      return area;
    }
    set {
      area = value;
    }
  }

  /// <summary>
  ///		Get/Set the SizeF of this element.
  /// </summary>
  /// <value>SizeF object describing the current SizeF of the RenderableElement.</value>
  public SizeF Size {
    get {
      return area.Size;
    }
    set {
      area.Size = value;
    }
  }

  #endregion Properties

  #endregion Base Members

  #region Abstract Members

  #region Methods

  /// <summary>
  ///		This function performs the required rendering for this element.
  /// </summary>
  /// <param name="position">Vector3 describing the final rendering position for the object.</param>
  /// <param name="clipRect">Rect describing the clipping area for the rendering.  No rendering will be performed outside this area.</param>
  protected abstract void DrawImpl(Vector3 position, Rect clipRect);

  #endregion Methods

  #endregion Abstract Members
}

} // namespace CeGui
