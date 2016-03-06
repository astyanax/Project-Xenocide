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
using CeGui.Widgets;
using System.Drawing;

namespace CeGui {

/// <summary>
/// Summary description for ListboxTextItem.
/// </summary>
[ AlternateWidgetName("ListboxTextItem") ]
public class ListboxTextItem : ListboxItem {
  #region Fields
  /// <summary>
  /// Font to use for text rendering.
  /// </summary>
  protected Font font = null;

  /// <summary>
  /// Colors to use when rendering text.
  /// </summary>
  protected ColourRect textColors;
  #endregion

  #region Properties
  /// <summary>
  /// Get/Set the font used for text rendering.
  /// </summary>
  public Font Font {
    get {
      // prefer out own font
      if(font != null) {
        return font;
      }
        // try our owner window's font setting (may be null if owner uses no existant default font)
    else if(owner != null) {
        return owner.Font;
      }
        // no owner, just use the default (which may be null anyway)
    else {
        return GuiSystem.Instance.DefaultFont;
      }
    }

    set {
      font = value;
    }
  }

  /// <summary>
  /// Get/Set the text colors for this ListboxItem.
  /// </summary>
  public ColourRect TextColors {
    get {
      return textColors;
    }

    set {
      textColors = value;
    }
  }

  #endregion

  #region Constructors

  public ListboxTextItem()
    :
      base(string.Empty) {
    textColors = new ColourRect(
        new Colour(1, 1, 1),
        new Colour(1, 1, 1),
        new Colour(1, 1, 1),
        new Colour(1, 1, 1)
        );
  }

  public ListboxTextItem(string text)
    :
      base(text) {
    textColors = new ColourRect(
        new Colour(1, 1, 1),
        new Colour(1, 1, 1),
        new Colour(1, 1, 1),
        new Colour(1, 1, 1)
        );
  }

  public ListboxTextItem(string text, int itemID)
    :
      this(text, itemID, null) {
  }

  public ListboxTextItem(string text, int itemID, object itemData)
    :
      this(text, itemID, itemData, false) {
  }

  public ListboxTextItem(string text, int itemID, object itemData, bool itemDisabled)
    :
      base(text, itemID, itemData, itemDisabled) {
    textColors = new ColourRect(
        new Colour(1, 1, 1),
        new Colour(1, 1, 1),
        new Colour(1, 1, 1),
        new Colour(1, 1, 1)
        );
  }

  #endregion

  #region Methods
  /// <summary>
  /// Set the colors to use for the text.
  /// </summary>
  /// <param name="topLeft"></param>
  /// <param name="topRight"></param>
  /// <param name="bottomLeft"></param>
  /// <param name="bottomRight"></param>
  public void SetTextColors(Colour topLeft, Colour topRight, Colour bottomLeft, Colour bottomRight) {
    textColors = new ColourRect(topLeft, topRight, bottomLeft, bottomRight);
  }

  /// <summary>
  /// Set the text color to be used.
  /// </summary>
  /// <param name="color"></param>
  public void SetTextColors(Colour color) {
    textColors = new ColourRect(color, color, color, color);
  }

  #endregion

  #region Base Class Methods

  #region Properties

  /// <summary>
  /// Return the pixel SizeF of this ListboxItem.
  /// </summary>
  public override SizeF Size {
    get {
      SizeF tmp = new SizeF(0, 0);

      Font fnt = Font;

      if(fnt != null) {
        tmp.Height = fnt.LineSpacing;
        tmp.Width = fnt.GetTextExtent(itemText);
      }

      return tmp;
    }
  }

  #endregion

  #region Methods

  /// <summary>
  /// Perform rendering for this ListboxItem
  /// </summary>
  /// <param name="position">Vector3 object describing the upper-left corner of area that should be rendered in to for the draw operation.</param>
  /// <param name="alpha">Alpha value to be used when rendering the item (between 0.0f and 1.0f).</param>
  /// <param name="clipper">Rect object describing the clipping Rect for the draw operation.</param>
  public override void Draw(Vector3 position, float alpha, Rect clipper) {
    if(selected && (selectBrushImage != null)) {
      selectBrushImage.Draw(clipper, GuiSystem.Instance.Renderer.GetZLayer(2), clipper, GetModulateAlphaColourRect(selectColors, alpha));
    }

    Font fnt = Font;

    if(fnt != null) {
      position.z = GuiSystem.Instance.Renderer.GetZLayer(3);
      fnt.DrawText(itemText, position, clipper, GetModulateAlphaColourRect(textColors, alpha));
    }
  }

  #endregion

  #endregion
}

} // namespace CeGui
