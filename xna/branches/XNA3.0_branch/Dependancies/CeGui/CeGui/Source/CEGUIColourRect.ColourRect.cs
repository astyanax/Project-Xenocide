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

/// <summary>Class that holds details of colors for the four corners of a Rect</summary>
public class ColourRect {

  /// <summary>Default constructor</summary>
  public ColourRect() : this(new Colour(1.0f, 1.0f, 1.0f, 1.0f)) {}

  /// <summary>
  ///   Constructor for ColourRect objects (via single colour).
  ///   Also handles default construction
  /// </summary>
  /// <param name="color">Color to use for the four corners</param>
  public ColourRect(Colour color) : this(color, color, color, color) {}

  /// <summary>Constructor for ColourRect objects</summary>
  /// <param name="topLeft">Color for the top left corner of the rectangle</param>
  /// <param name="topRight">Color for the top right corner of the rectangle</param>
  /// <param name="bottomLeft">Color for the bottom left corner of the rectangle</param>
  /// <param name="bottomRight">Color for the bottom right corner of the rectangle</param>
  public ColourRect(Colour topLeft, Colour topRight, Colour bottomLeft, Colour bottomRight) {
    this.topLeft = topLeft;
    this.topRight = topRight;
    this.bottomLeft = bottomLeft;
    this.bottomRight = bottomRight;
  }

  /// <summary>
  ///   Modulate all components of a colour rect with corresponding
  ///   components from another colour rect.
  /// </summary>
  /// <param name="first">Color rect whose components to modulate</param>
  /// <param name="second">Color rect by which the components are modulated</param>
  /// <returns>The modulated color rect</returns>
  public static ColourRect operator *(ColourRect first, ColourRect second) {
    return new ColourRect(
      first.topLeft * second.topLeft,
      first.topRight * second.topRight,
      first.bottomLeft * second.bottomLeft,
      first.bottomRight * second.bottomRight
    );
  }

  /// <summary>Sets the alpha value for all colors in the color Rect</summary>
  /// <param name="alpha">Alpha value to use</param>
  public void SetAlpha(float alpha) {
    this.topLeft.Alpha = alpha;
    this.topRight.Alpha = alpha;
    this.bottomLeft.Alpha = alpha;
    this.bottomRight.Alpha = alpha;
  }

  /// <summary>Set the alpha value to use for the top edge of the ColourRect</summary>
  /// <param name="alpha">Alpha value to use</param>
  public void SetTopAlpha(float alpha) {
    this.topLeft.Alpha = alpha;
    this.topRight.Alpha = alpha;
  }

  /// <summary>Set the alpha value to use for the bottom edge of the ColourRect</summary>
  /// <param name="alpha">Alpha value to use</param>
  public void SetBottomAlpha(float alpha) {
    this.bottomLeft.Alpha = alpha;
    this.bottomRight.Alpha = alpha;
  }

  /// <summary>Set the alpha value to use for the left edge of the ColourRect</summary>
  /// <param name="alpha">Alpha value to use</param>
  public void SetLeftAlpha(float alpha) {
    this.topLeft.Alpha = alpha;
    this.bottomLeft.Alpha = alpha;
  }

  /// <summary>Set the alpha value to use for the right edge of the ColourRect</summary>
  /// <param name="alpha">Alpha value to use</param>
  public void SetRightAlpha(float alpha) {
    this.topRight.Alpha = alpha;
    this.bottomRight.Alpha = alpha;
  }

  /// <summary>Determinate the ColourRect is monochromatic or variegated</summary>
  /// <returns>
  ///   True if all four corners of the ColourRect has same colour, false otherwise
  /// </returns>
  public bool IsMonochromatic() {
    return
      (this.topLeft == this.topRight) &&
      (this.topLeft == this.bottomLeft) &&
      (this.bottomLeft == this.bottomRight);
  }

  /// <summary>Gets a portion of this ColourRect as a subset ColourRect</summary>
  /// <param name="left">
  ///   The left side of this subrectangle (in the range of 0-1 float)
  /// </param>
  /// <param name="right">
  ///   The right side of this subrectangle (in the range of 0-1 float)
  /// </param>
  /// <param name="top">
  ///   The top side of this subrectangle (in the range of 0-1 float)
  /// </param>
  /// <param name="bottom">
  ///   The bottom side of this subrectangle (in the range of 0-1 float)
  /// </param>
  /// <returns>A ColourRect from the specified range</returns>
  public ColourRect GetSubRectangle(float left, float right, float top, float bottom) {
    return new ColourRect(
      GetColourAtPoint(left, top),
      GetColourAtPoint(right, top),
      GetColourAtPoint(left, bottom),
      GetColourAtPoint(right, bottom)
    );
  }

  /// <summary>Get the colour at a point in the rectangle</summary>
  /// <param name="x">The x coordinate of the point</param>
  /// <param name="y">The y coordinate of the point</param>
  /// <returns>The colour at the specified point</returns>
  public Colour GetColourAtPoint(float x, float y) {
    Colour h1 = (this.topRight - this.topLeft) * x + this.topLeft;
    Colour h2 = (this.bottomRight - this.bottomLeft) * x + this.bottomLeft;

    return (h2 - h1) * y + h1;
  }

  /// <summary>Set the colour of all four corners simultaneously</summary>
  /// <param name="col">colour that is to be set for all four corners of the ColourRect</param>
  public void SetColours(Colour col) {
    this.topLeft = col;
    this.topRight = col;
    this.bottomLeft = col;
    this.bottomRight = col;
  }

  /// <summary>Module the alpha components of each corner's colour by a constant</summary>
  /// <param name="alpha">The constant factor to modulate all alpha colour components by</param>
  public void ModulateAlpha(float alpha) {
    this.topLeft.Alpha *= alpha;
    this.topRight.Alpha *= alpha;
    this.bottomLeft.Alpha *= alpha;
    this.bottomRight.Alpha *= alpha;
  }

  /// <summary>
  ///   Returns a string representation of this ColorRect in the form
  ///   "tl:[color] tr:[color] bl:[color] br:[color]"
  /// </summary>
  /// <returns>A string representation of this ColorRect</returns>
  public override string ToString() {
    return string.Format(
      "tl:{0} tr:{1} bl:{2} br:{3}", topLeft, topRight, bottomLeft, bottomRight
    );
  }

  /// <summary>
  ///   Parses a string representation of the ColorRect and returns the corresponding
  ///   ColorRect object
  /// </summary>
  /// <param name="data">A string representation of a ColorRect</param>
  /// <returns>A ColorRect corresponding matching the string representation</returns>
  public static ColourRect Parse(string data) {
    string[] parameters = data.Split(new char[] { ' ', ':' });
    ColourRect colors = new ColourRect();

    for(int i = 0; i < parameters.Length; i++) {
      if(parameters[i].CompareTo("tl") == 0) {
        colors.topLeft = Colour.Parse(parameters[++i]);
      } else if(parameters[i].CompareTo("tr") == 0) {
        colors.topRight = Colour.Parse(parameters[++i]);
      } else if(parameters[i].CompareTo("bl") == 0) {
        colors.bottomLeft = Colour.Parse(parameters[++i]);
      } else if(parameters[i].CompareTo("br") == 0) {
        colors.bottomRight = Colour.Parse(parameters[++i]);
      }
    }

    return colors;
  }

  /// <summary>The colors at the four corners of the rectangle</summary>
  public Colour topLeft, topRight, bottomLeft, bottomRight;

}

} // namespace CeGui
