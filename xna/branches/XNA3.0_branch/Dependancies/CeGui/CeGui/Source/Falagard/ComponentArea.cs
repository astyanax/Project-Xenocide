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
using System.Xml;
using System.Drawing;

namespace CeGui {

/// <summary>
///	Class that represents a target area for a widget or imagery component.
///
///	This is essentially a Rect built out of Dimension objects.  Of note is that
///	what would normally be the 'right' and 'bottom' edges may alternatively
///	represent width and height depending upon what the assigned Dimension(s)
///	represent.
/// </summary>
public class ComponentArea {
  #region Fields
  protected Dimension left;
  protected Dimension top;
  protected Dimension width;
  protected Dimension height;
  private string areaProperty;
  #endregion

  #region Properties
  public Dimension Left {
    get { return left; }
    set { left = value; }
  }

  public Dimension Top {
    get { return top; }
    set { top = value; }
  }

  public Dimension Width {
    get { return width; }
    set { width = value; }
  }

  public Dimension Height {
    get { return height; }
    set { height = value; }
  }
  #endregion

  /// <summary>
  /// Return a Rect describing the absolute pixel area represented by this ComponentArea.
  /// </summary>
  /// <param name="wnd">Window object to be used when calculating final pixel area.</param>
  /// <returns>
  /// Rect object describing the pixels area represented by this ComponentArea when using \a wnd
  ///	as a reference for calculating the final pixel dimensions.
  ///	</returns>
  public Rect GetPixelRect(Window wnd) {
    Rect pixelRect = new Rect();

    if(IsAreaFetchedFromProperty) {
      throw new NotImplementedException();
    } else {
      pixelRect.Left = left.BaseDimension.GetValue(wnd);
      pixelRect.Top = top.BaseDimension.GetValue(wnd);

      if(width.DimensionType == DimensionType.Width) {
        pixelRect.Width = width.BaseDimension.GetValue(wnd);
      } else {
        pixelRect.Right = width.BaseDimension.GetValue(wnd);
      }

      if(height.DimensionType == DimensionType.Height) {
        pixelRect.Height = height.BaseDimension.GetValue(wnd);
      } else {
        pixelRect.Bottom = height.BaseDimension.GetValue(wnd);
      }
    }
    return pixelRect;
  }

  /// <summary>
  /// Return a Rect describing the absolute pixel area represented by this ComponentArea.
  /// </summary>
  /// <param name="wnd">Window object to be used when calculating final pixel area.</param>
  /// <returns>
  /// Rect object describing the pixels area represented by this ComponentArea when using \a wnd
  ///	as a reference for calculating the final pixel dimensions.
  ///	</returns>
  public Rect GetPixelRect(Window wnd, Rect baseRect) {
    Rect pixelRect = new Rect();

    if(IsAreaFetchedFromProperty) {
      throw new NotImplementedException();
    } else {
      pixelRect.Left = left.BaseDimension.GetValue(wnd, baseRect) + baseRect.Left;
      pixelRect.Top = top.BaseDimension.GetValue(wnd, baseRect) + baseRect.Top;

      if(width.DimensionType == DimensionType.Width) {
        pixelRect.Width = width.BaseDimension.GetValue(wnd, baseRect);
      } else {
        pixelRect.Right = width.BaseDimension.GetValue(wnd, baseRect) + baseRect.Left;
      }

      if(height.DimensionType == DimensionType.Height) {
        pixelRect.Height = height.BaseDimension.GetValue(wnd, baseRect);
      } else {
        pixelRect.Bottom = height.BaseDimension.GetValue(wnd, baseRect) + baseRect.Top;
      }
    }
    return pixelRect;
  }

  public void WriteToXml(XmlWriter writer) {
  }

  /// <summary>
  /// Return whether this ComponentArea fetches it's area via a property on the target window.
  /// </summary>
  public bool IsAreaFetchedFromProperty {
    get { return (areaProperty.Length > 0); }
  }

  /// <summary>
  /// Gets/Sets the name of the property that will be used to determine the pixel area for this ComponentArea.
  /// </summary>
  public string AreaPropertySource {
    get { return areaProperty; }
    set { areaProperty = value; }
  }
}

} // namespace CeGui
