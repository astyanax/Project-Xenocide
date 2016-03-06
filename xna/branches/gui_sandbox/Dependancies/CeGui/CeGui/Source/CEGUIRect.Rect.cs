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

namespace CeGui {

/// <summary>Two dimensional rectangle</summary>
public struct Rect {

  /// <summary>Initializes a new rectangle to specified coordinates</summary>
  /// <param name="left">Position of the left side of the rectangle</param>
  /// <param name="top">Position of the upper side of the rectangle</param>
  /// <param name="right">Position of the right side of the rectangle</param>
  /// <param name="bottom">Position of the lower side of the rectangle</param>
  public Rect(float left, float top, float right, float bottom) {
    this.Left = left;
    this.Top = top;
    this.Right = right;
    this.Bottom = bottom;
  }

  /// <summary>Implicit conversion of this class to System.Drawing.RectangleF</summary>
  /// <param name="rectangle">Rectangle to be converted</param>
  /// <returns>An equivalent rectangle in the RectangleF class</returns>
  public static implicit operator RectangleF(Rect rectangle) {
    return new RectangleF(rectangle.Position, rectangle.Size);
  }

  /// <summary>Implicit conversion of System.Drawing.RectangleF to this class</summary>
  /// <param name="rectangle">Rectangle to be converted</param>
  /// <returns>An equivalent rectangle in the Rect class</returns>
  public static implicit operator Rect(RectangleF rectangle) {
    return new Rect(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
  }

  /// <summary>Gets if this Rect has a size of 0</summary>
  public bool IsEmpty {
    get { return (this.Left == this.Right) && (this.Top == this.Bottom); }
  }

  /// <summary>An empty rectangle</summary>
  public static readonly Rect Empty = new Rect(0, 0, 0, 0);

  /// <summary>Get/Set the top-left corner of the Rect</summary>
  /// <value>Point that holds the x and y position</value>
  public PointF Position {
    get {
      return new PointF(this.Left, this.Top);
    }
    set {
      SizeF size = this.Size;

      this.Left = value.X;
      this.Top = value.Y;

      this.Size = size;
    }
  }

  /// <summary>Width of this Rect, from the left side</summary>
  public float Width {
    get {
      return this.Right - this.Left;
    }
    set {
      // HACK: test
      if(float.IsNaN(value))
        value = 0;

      this.Right = this.Left + value;
    }
  }

  /// <summary>Height of this Rect, from the top edge</summary>
  public float Height {
    get {
      return this.Bottom - this.Top;
    }
    set {
      // HACK: test
      if(float.IsNaN(value))
        value = 0;

      this.Bottom = this.Top + value;
    }
  }

  /// <summary>Get/Set the size of the rect area</summary>
  /// <value>The desired size of the Rect</value>
  public SizeF Size {
    get {
      return new SizeF(this.Width, this.Height);
    }
    set {
      this.Width = value.Width;
      this.Height = value.Height;
    }
  }

  /// <summary>
  ///   Check the size of this rect if it is bigger than <paramref name="size"/>,
  ///   resize it so it isn't.
  /// </summary>
  /// <param name="size">
  ///   Describes the maximum dimensions that this Rect should be limited to
  /// </param>
  public void ConstrainSizeMax(SizeF size) {

    if(this.Width > size.Width)
      this.Width = size.Width;

    if(this.Height > size.Height)
      this.Height = size.Height;

  }

  /// <summary>
  ///   Check the size of this rect if it is smaller than <paramref name="size"/>,
  ///   resize it so it isn't.
  /// </summary>
  /// <param name="size">
  ///   Describes the minimum dimensions that this Rect should be limited to
  /// </param>
  public void ConstrainSizeMin(SizeF size) {

    if(this.Width < size.Width)
      this.Width = size.Width;

    if(this.Height < size.Height)
      this.Height = size.Height;

  }

  /// <summary>
  ///   Check the size of this rect if it is bigger than <paramref name="max"/> or
  ///   smaller than <paramref name="min"/>, resize it so it isn't.
  /// </summary>
  /// <param name="max">
  ///   Describes the maximum dimensions that this Rect should be limited to
  /// </param>
  /// <param name="min">
  ///   Describes the minimum dimensions that this Rect should be limited to
  /// </param>
  public void ConstrainSize(SizeF max, SizeF min) {
    SizeF currentSize = this.Size;

    if(this.Width > max.Width)
      this.Width = max.Width;
    else if(this.Width < min.Width)
      this.Width = min.Width;

    if(this.Height > max.Height)
      this.Height = max.Height;
    else if(this.Height < min.Height)
      this.Height = min.Height;

  }

  /// <summary>
  ///   return a Rect that is the intersection of 'this' Rect with <paramref name="rect"/>
  /// </summary>
  /// <remarks>
  ///   It can be assumed that if left == right, or top == bottom,
  ///   or Width == 0, or Height == 0, then 'this' rect was totally
  ///   outside <paramref name="rect"/>
  /// </remarks>
  /// <param name="rect">Rect to test for intersection</param>
  /// <returns>Instersection rect</returns>
  public Rect GetIntersection(Rect rect) {
    // check for total exclusion
    if(
      (this.Right > rect.Left) &&
      (this.Left < rect.Right) &&
      (this.Bottom > rect.Top) &&
      (this.Top < rect.Bottom)
    ) {

      Rect temp = new Rect();

      // fill in temp with the intersection
      temp.Left = (this.Left > rect.Left) ? this.Left : rect.Left;
      temp.Right = (this.Right < rect.Right) ? this.Right : rect.Right;
      temp.Top = (this.Top > rect.Top) ? this.Top : rect.Top;
      temp.Bottom = (this.Bottom < rect.Bottom) ? this.Bottom : rect.Bottom;

      return temp;
    }

    return new Rect(0.0f, 0.0f, 0.0f, 0.0f);
  }

  /// <summary>Return true if the given Point falls within this Rect</summary>
  /// <param name="point">Point describing the position to test</param>
  /// <returns>True if the point is within this Rect, false otherwise</returns>
  public bool IsPointInRect(PointF point) {
    return (
      (point.X >= this.Left) &&
      (point.X < this.Right) &&
      (point.Y >= this.Top) &&
      (point.Y < this.Bottom)
    );
  }

  /// <summary>Applies an offset this Rect</summary>
  /// <param name="point">Point containing the offsets to be applied to the Rect</param>
  public void Offset(PointF point) {
    this.Left += point.X;
    this.Right += point.X;
    this.Top += point.Y;
    this.Bottom += point.Y;
  }

  /// <summary>
  ///  Returns a string representation of this Rect object in the form
  ///  "l:[left] t:[top] r:[right] b:[bottom]
  /// </summary>
  /// <returns>A string representation of this Rect object</returns>
  public override string ToString() {
    return string.Format(
      "l:{0} t:{1} r:{2} b:{3}", this.Left, this.Top, this.Right, this.Bottom
    );
  }

  /// <summary>
  ///   Parses the string representation of a Rect and returns the corresponding
  ///   Rect object
  /// </summary>
  /// <param name="data">String representation of a Rect</param>
  /// <returns>Rect corresponding to the passed in string</returns>
  public static Rect Parse(string data) {
    string[] parameters = data.Split(new char[] { ' ', ':' });
    Rect rect = new Rect();

    for(int i = 0; i < parameters.Length; i++) {
      if(0 == parameters[i].CompareTo("l")) {
        rect.Left = float.Parse(parameters[++i]);
      } else if(0 == parameters[i].CompareTo("t")) {
        rect.Top = float.Parse(parameters[++i]);
      } else if(0 == parameters[i].CompareTo("r")) {
        rect.Right = float.Parse(parameters[++i]);
      } else if(0 == parameters[i].CompareTo("b")) {
        rect.Bottom = float.Parse(parameters[++i]);
      }
    }
    return rect;
  }

  /// <summary>Coordinate of the rectangle's left side</summary>
  public float Left;
  /// <summary>Coordinate of the rectangle's right side</summary>
  public float Right;
  /// <summary>Coordinate of the rectangle's upper side</summary>
  public float Top;
  /// <summary>Coordinate of the rectangle's lower side</summary>
  public float Bottom;

}

} // namespace CeGui
