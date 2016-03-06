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
using System.Drawing;

namespace CeGui.Widgets {

/// <summary>
/// Base class for list box items.
/// </summary>
/// <remarks>
/// The ListboxItem is intended to be sub-classed and has the potential to allow virtually any
/// data to be put into a list box type widget.
/// </remarks>
[ AlternateWidgetName("ListboxItem") ]
public abstract class ListboxItem {
  #region Constants

  #endregion

  #region Fields
  /// <summary>
  /// The text for this item.
  /// </summary>
  protected string itemText;
  /// <summary>
  /// Application assigned ID code for this item
  /// </summary>
  protected int itemID;
  /// <summary>
  /// Application assigned data for this item.
  /// </summary>
  protected object itemData;
  /// <summary>
  /// true if the item is selected
  /// </summary>
  protected bool selected;
  /// <summary>
  /// true if the item is disabled.
  /// </summary>
  protected bool disabled;
  /// <summary>
  /// The Window object that owns this ListboxItem.
  /// </summary>
  protected Window owner;
  /// <summary>
  /// Colors to be used for the selection highlight.
  /// </summary>
  protected ColourRect selectColors;
  /// <summary>
  /// Image to use for rendering the selection highlight.
  /// </summary>
  protected Image selectBrushImage;

  #endregion

  #region Properties
  /// <summary>
  /// Get/Set the text for this ListboxItem.
  /// </summary>
  public string Text {
    get {
      return itemText;
    }

    set {
      itemText = value;
    }
  }

  /// <summary>
  /// Get/Set the ID for this ListboxItem.
  /// </summary>
  public int ID {
    get {
      return itemID;
    }

    set {
      itemID = value;
    }
  }

  /// <summary>
  /// Get/Set the user data object for this ListboxItem.
  /// </summary>
  public object UserData {
    get {
      return itemData;
    }

    set {
      itemData = value;
    }
  }

  /// <summary>
  /// Get/Set the selected state of this ListboxItem.
  /// </summary>
  public bool Selected {
    get {
      return selected;
    }

    set {
      selected = value;
    }
  }

  /// <summary>
  /// Get/Set the disabled state of this ListboxItem.
  /// </summary>
  public bool Disabled {
    get {
      return disabled;
    }

    set {
      disabled = value;
    }
  }

  /// <summary>
  /// Get/Set the owner window for this ListboxItem.
  /// </summary>
  public Window OwnerWindow {
    get {
      return owner;
    }

    set {
      owner = value;
    }
  }

  /// <summary>
  /// Get/Set the selection colors for this ListboxItem.
  /// </summary>
  public ColourRect SelectionColors {
    get {
      return selectColors;
    }

    set {
      selectColors = value;
    }
  }

  /// <summary>
  /// Get/Set the selection brush image for this ListboxItem.
  /// </summary>
  public Image SelectionBrushImage {
    get {
      return selectBrushImage;
    }

    set {
      selectBrushImage = value;
    }
  }

  #endregion

  #region Methods

  /// <summary>
  /// Set the individual colors to use for the selection area.
  /// </summary>
  /// <param name="topLeft"></param>
  /// <param name="topRight"></param>
  /// <param name="bottomLeft"></param>
  /// <param name="bottomRight"></param>
  public void SetSelectionColors(Colour topLeft, Colour topRight, Colour bottomLeft, Colour bottomRight) {
    selectColors = new ColourRect(topLeft, topRight, bottomLeft, bottomRight);
  }

  /// <summary>
  /// Set the selection color to be used for the entire selection area.
  /// </summary>
  /// <param name="color"></param>
  public void SetSelectionColors(Colour color) {
    selectColors = new ColourRect(color, color, color, color);
  }

  /// <summary>
  /// Set the selection brush image via Imageset and Image names.
  /// </summary>
  /// <param name="imagesetName"></param>
  /// <param name="imageName"></param>
  public void SetSelectionBrushImage(string imagesetName, string imageName) {
    selectBrushImage = ImagesetManager.Instance.GetImageset(imagesetName).GetImage(imageName);
  }

  #endregion

  #region Abstract Members

  #region Properties

  /// <summary>
  /// Return the pixel SizeF of this ListboxItem.
  /// </summary>
  public abstract SizeF Size {
    get;
  }

  #endregion

  #region Methods

  /// <summary>
  /// Perform rendering for this ListboxItem
  /// </summary>
  /// <param name="position">Vector3 object describing the upper-left corner of area that should be rendered in to for the draw operation.</param>
  /// <param name="alpha">Alpha value to be used when rendering the item (between 0.0f and 1.0f).</param>
  /// <param name="clipper">Rect object describing the clipping Rect for the draw operation.</param>
  public abstract void Draw(Vector3 position, float alpha, Rect clipper);

  #endregion

  #endregion

  #region Constructors

  public ListboxItem(string text) {
    itemText = text;
    itemID = 0;
    itemData = null;
    selected = false;
    disabled = false;
    owner = null;
    SetSelectionColors(new Colour(0.2666f, 0.2666f, 0.6666f, 1.0f));
    selectBrushImage = null;
  }

  public ListboxItem(string text, int itemID, object itemData, bool itemDisabled) {
    itemText = text;
    this.itemID = itemID;
    this.itemData = itemData;
    selected = false;
    disabled = itemDisabled;
    owner = null;
    SetSelectionColors(new Colour(0.2666f, 0.2666f, 0.6666f, 1.0f));
    selectBrushImage = null;
  }


  #endregion

  #region Implementation Methods
  /// <summary>
  /// Return a ColorRect object describing the colors in ColorRect 'colors' after having
  /// their alpha component modulated by 'alpha'.
  /// </summary>
  /// <param name="colors">Input ColourRect object</param>
  /// <param name="alpha">value to modulate with</param>
  /// <returns>ColorRect containing the modulated version of 'colors'</returns>
  protected ColourRect GetModulateAlphaColourRect(ColourRect colors, float alpha) {
    return new ColourRect
        (
        CalculateModulatedAlphaColor(colors.topLeft, alpha),
        CalculateModulatedAlphaColor(colors.topRight, alpha),
        CalculateModulatedAlphaColor(colors.bottomLeft, alpha),
        CalculateModulatedAlphaColor(colors.bottomRight, alpha)
        );
  }

  /// <summary>
  /// Return a Color value describing the color specified by 'colour' after having its
  /// alpha component modulated by the value 'alpha'.
  /// </summary>
  /// <param name="color">Color whos alpha value is to be modulated</param>
  /// <param name="alpha">value by which to modulate the given Color</param>
  /// <returns>Modulated Color value.</returns>
  protected Colour CalculateModulatedAlphaColor(Colour color, float alpha) {
    color.Alpha *= alpha;
    return color;
  }

  /// <summary>
  ///		Method called from static operator overload to virtualise the compare operation
  /// </summary>
  /// <param name="other"></param>
  /// <returns></returns>
  protected virtual bool lessthan_operator(ListboxItem other) {
    return (this.Text.CompareTo(other.Text) < 0);
  }

  /// <summary>
  ///		Method called from static operator overload to virtualise the compare operation
  /// </summary>
  /// <param name="other"></param>
  /// <returns></returns>
  protected virtual bool greaterthan_operator(ListboxItem other) {
    return (this.Text.CompareTo(other.Text) > 0);
  }

  #endregion

  #region Operator Overloads

  /// <summary>
  ///		Less-than operator for ListboxItem objects.
  /// </summary>
  /// <param name="a"></param>
  /// <param name="b"></param>
  /// <returns></returns>
  public static bool operator <(ListboxItem a, ListboxItem b) {
    // This is done via a virtual method, which allows a derived class to provide
    // custom sorting.
    return a.lessthan_operator(b);
  }

  /// <summary>
  ///		Greater-than operator for ListboxItem objects.
  /// </summary>
  /// <param name="a"></param>
  /// <param name="b"></param>
  /// <returns></returns>
  public static bool operator >(ListboxItem a, ListboxItem b) {
    // This is done via a virtual method, which allows a derived class to provide
    // custom sorting.
    return a.greaterthan_operator(b);
  }

  #endregion
}

} // namespace CeGui.Widgets
