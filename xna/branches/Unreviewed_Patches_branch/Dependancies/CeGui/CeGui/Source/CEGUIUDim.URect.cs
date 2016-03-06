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

/// <summary>Area rectangle class built using unified dimensions (UDims)</summary>
public struct URect {

  /// <summary>A unified rectangle that has been initialized to zero</summary>
  public static readonly URect Zero = new URect(
    UVector2.Zero, UVector2.Zero
  );

  /// <summary>Initializes a new unified rectangle</summary>
  /// <param name="min">Minimum coordinates to store in the rectangle</param>
  /// <param name="max">Maximum coordinates to store in the rectangle</param>
  public URect(UVector2 min, UVector2 max) {
    this.Min = min;
    this.Max = max;
  }

  /// <summary>Initializes a new unified rectangle</summary>
  /// <param name="left">Left coordinate of the rectangle</param>
  /// <param name="top">Top coordinate of the rectangle</param>
  /// <param name="right">Right coordinate of the rectangle</param>
  /// <param name="bottom">Bottom coordinate of the rectangle</param>
  public URect(
    UDim left, UDim top,
    UDim right, UDim bottom
  ) {
    this.Min = new UVector2(left, top);
    this.Max = new UVector2(right, bottom);
  }

  /// <summary>Converts the unified rectangle into an absolute rectangle</summary>
  /// <param name="bounds">Extents into which to project the unified rectangle</param>
  /// <returns>The absolute rectangle described by the unified rectangle</returns>
  public Rect AsAbsolute(SizeF bounds) {
    return new Rect(
      this.Min.X.AsAbsolute(bounds.Width),
      this.Min.Y.AsAbsolute(bounds.Height),
      this.Max.X.AsAbsolute(bounds.Width),
      this.Max.Y.AsAbsolute(bounds.Height)
    );
  }

  /// <summary>Converts the unified rectangle into a relative rectangle</summary>
  /// <param name="bounds">Extents into which to project the unified rectangle</param>
  /// <returns>The relative rectangle described by the unified rectangle</returns>
  public Rect AsRelative(SizeF bounds) {
    return new Rect(
      this.Min.X.AsRelative(bounds.Width),
      this.Min.Y.AsRelative(bounds.Height),
      this.Max.X.AsRelative(bounds.Width),
      this.Max.Y.AsRelative(bounds.Height)
    );
  }

  /// <summary>Location of the rectangle</summary>
  public UVector2 Position {
    get { return this.Min; }
    set {
      UVector2 size = this.Max - this.Min;
      this.Min = value;
      this.Max = value + size;
    }
  }

  /// <summary>Size of the rectangle</summary>
  public UVector2 Size {
    get { return this.Max - this.Min; }
    set { this.Max = this.Min + value; }
  }

  /// <summary>Width of the rectangle</summary>
  public UDim Width {
    get { return this.Max.X - this.Min.X; }
    set { this.Max.X = this.Min.X + value; }
  }

  /// <summary>Height of the rectangle</summary>
  public UDim Height {
    get { return this.Max.Y - this.Min.Y; }
    set { this.Max.Y = this.Min.Y + value; }
  }

  /// <summary>Moves the unified rectangle by the given offset</summary>
  /// <param name="offset">Offset to move by</param>
  public void Offset(UVector2 offset) {
    this.Min += offset;
    this.Max += offset;
  }

  /// <summary>Moves the unified rectangle against the given offset</summary>
  /// <param name="offset">Offset to move against</param>
  public void Unoffset(UVector2 offset) {
    this.Min -= offset;
    this.Max -= offset;
  }

  /// <summary>Moves the unified rectangle by the given offset</summary>
  /// <param name="rectangle">Rectangle to move</param>
  /// <param name="offset">Offset to move by</param>
  /// <returns>The unified rectangle moved by the given offset</returns>
  public static URect operator +(
    URect rectangle, UVector2 offset
  ) {
    URect newRectangle = rectangle;
    newRectangle.Offset(offset);
    return newRectangle;
  }

  /// <summary>Moves the unified rectangle against the given offset</summary>
  /// <param name="rectangle">Rectangle to move</param>
  /// <param name="offset">Offset to move against</param>
  /// <returns>The unified rectangle moved against the given offset</returns>
  public static URect operator -(
    URect rectangle, UVector2 offset
  ) {
    URect newRectangle = rectangle;
    newRectangle.Unoffset(offset);
    return newRectangle;
  }

  /// <summary>Checks two unified rectangles for inequality</summary>
  /// <param name="first">First rectangle to be compared</param>
  /// <param name="second">Second rectangle to be compared</param>
  /// <returns>True if the instances differ or exactly one reference is set to null</returns>
  public static bool operator !=(URect first, URect second) {
    return !(first == second);
  }

  /// <summary>Checks two unified rectangles for equality</summary>
  /// <param name="first">First rectangle to be compared</param>
  /// <param name="second">Second rectangle to be compared</param>
  /// <returns>True if both instances are equal or both references are null</returns>
  public static bool operator ==(URect first, URect second) {
    if(ReferenceEquals(first, null))
      return ReferenceEquals(second, null);

    return first.Equals(second);
  }

  /// <summary>Checks whether another instance is equal to this instance</summary>
  /// <param name="other">Other instance to compare to this instance</param>
  /// <returns>True if the other instance is equal to this instance</returns>
  public override bool Equals(object other) {
    if(!(other is URect))
      return false;

    return Equals((URect)other);
  }

  /// <summary>Checks whether another instance is equal to this instance</summary>
  /// <param name="other">Other instance to compare to this instance</param>
  /// <returns>True if the other instance is equal to this instance</returns>
  public bool Equals(URect other) {
    if(ReferenceEquals(other, null))
      return false;

    return (this.Min == other.Min) && (this.Max == other.Max);
  }

  /// <summary>Obtains a hash code of this instance</summary>
  /// <returns>The hash code of the instance</returns>
  public override int GetHashCode() {
    unchecked { return this.Min.GetHashCode() ^ this.Max.GetHashCode(); }
  }

  /// <summary>The minimum coordinates of the rectangle</summary>
  public UVector2 Min;
  /// <summary>The maximum coordinates of the rectangle</summary>
  public UVector2 Max;

}

} // namespace CeGui
