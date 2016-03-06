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

/// <summary>
///   Two dimensional vector class built using unified dimensions. The UnifiedVector2 class
///   is used for representing both positions and sizes.
/// </summary>
public struct UVector2 {

  /// <summary>A unified vector that has been initialized to zero</summary>
  public static readonly UVector2 Zero = new UVector2(
    UDim.Zero, UDim.Zero
  );

  /// <summary>Initializes a new unified vector</summary>
  /// <param name="x">X coordinate to store in the vector</param>
  /// <param name="Y">Y coordinate to store in the vector</param>
  public UVector2(UDim x, UDim y) {
    this.X = x;
    this.Y = y;
  }

  /// <summary>Converts the unified vector into an absolute position</summary>
  /// <param name="bounds">Extents into which to project the unified vector</param>
  /// <returns>The absolute position described by the unified vector</returns>
  public PointF AsAbsolutePosition(SizeF bounds) {
    return new PointF(this.X.AsAbsolute(bounds.Width), this.Y.AsAbsolute(bounds.Height));
  }

  /// <summary>Converts the unified vector into an absolute size</summary>
  /// <param name="bounds">Extents into which to project the unified vector</param>
  /// <returns>The absolute size described by the unified vector</returns>
  public SizeF AsAbsoluteSize(SizeF bounds) {
    return new SizeF(this.X.AsAbsolute(bounds.Width), this.Y.AsAbsolute(bounds.Height));
  }

  /// <summary>Converts the unified vector into a relative position</summary>
  /// <param name="bounds">Extents into which to project the unified vector</param>
  /// <returns>The relative position described by the unified vector</returns>
  public PointF AsRelativePosition(SizeF bounds) {
    return new PointF(this.X.AsRelative(bounds.Width), this.Y.AsRelative(bounds.Height));
  }

  /// <summary>Converts the unified vector into a relative size</summary>
  /// <param name="bounds">Extents into which to project the unified vector</param>
  /// <returns>The relative size described by the unified vector</returns>
  public SizeF AsRelativeSize(SizeF bounds) {
    return new SizeF(this.X.AsRelative(bounds.Width), this.Y.AsRelative(bounds.Height));
  }

  /// <summary>Adds one unified vector to the other</summary>
  /// <param name="vector">Base unified vector to add to</param>
  /// <param name="summand">Vector to add to the base</param>
  /// <returns>The result of the addition operation</returns>
  public static UVector2 operator +(
    UVector2 vector, UVector2 summand
  ) {
    return new UVector2(vector.X + summand.X, vector.Y + summand.Y);
  }

  /// <summary>Subtracts one unified vector from the other</summary>
  /// <param name="vector">Base unified vector to subtract from</param>
  /// <param name="subtrahend">Vector to subtract from the base</param>
  /// <returns>The result of the subtraction operation</returns>
  public static UVector2 operator -(
    UVector2 vector, UVector2 subtrahend
  ) {
    return new UVector2(vector.X - subtrahend.X, vector.Y - subtrahend.Y);
  }

  /// <summary>Divides one unified vector by another one</summary>
  /// <param name="vector">Base unified vector to be divided</param>
  /// <param name="divisor">Divisor to divide by</param>
  /// <returns>The result of the division operation</returns>
  public static UVector2 operator /(
    UVector2 vector, UVector2 divisor
  ) {
    return new UVector2(vector.X / divisor.X, vector.Y / divisor.Y);
  }

  /// <summary>Multiplies one unified vector with another one</summary>
  /// <param name="vector">Base unified vector to be multiplied</param>
  /// <param name="factor">Factor to multiply by</param>
  /// <returns>The result of the multiplication operation</returns>
  public static UVector2 operator *(
    UVector2 vector, UVector2 factor
  ) {
    return new UVector2(vector.X * factor.X, vector.Y * factor.Y);
  }

  /// <summary>Checks two unified vectors for inequality</summary>
  /// <param name="first">First vector to be compared</param>
  /// <param name="second">Second vector to be compared</param>
  /// <returns>True if the instances differ or exactly one reference is set to null</returns>
  public static bool operator !=(UVector2 first, UVector2 second) {
    return !(first == second);
  }

  /// <summary>Checks two unified vectors for equality</summary>
  /// <param name="first">First vector to be compared</param>
  /// <param name="second">Second vector to be compared</param>
  /// <returns>True if both instances are equal or both references are null</returns>
  public static bool operator ==(UVector2 first, UVector2 second) {
    if(ReferenceEquals(first, null))
      return ReferenceEquals(second, null);

    return first.Equals(second);
  }

  /// <summary>Checks whether another instance is equal to this instance</summary>
  /// <param name="other">Other instance to compare to this instance</param>
  /// <returns>True if the other instance is equal to this instance</returns>
  public override bool Equals(object other) {
    if(!(other is UVector2))
      return false;

    return Equals((UVector2)other);
  }

  /// <summary>Checks whether another instance is equal to this instance</summary>
  /// <param name="other">Other instance to compare to this instance</param>
  /// <returns>True if the other instance is equal to this instance</returns>
  public bool Equals(UVector2 other) {
    if(ReferenceEquals(other, null))
      return false;

    return (this.X == other.X) && (this.Y == other.Y);
  }

  /// <summary>Obtains a hash code of this instance</summary>
  /// <returns>The hash code of the instance</returns>
  public override int GetHashCode() {
    unchecked { return this.X.GetHashCode() ^ this.Y.GetHashCode(); }
  }

  /// <summary>The X coordinate of the unified vector</summary>
  public UDim X;
  /// <summary>The Y coordinate of the unified vector</summary>
  public UDim Y;
}

} // namespace CeGui
