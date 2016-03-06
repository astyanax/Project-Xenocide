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

namespace CeGui {

/// <summary>
///   A unified dimension is a dimension that has both a relative 'scale' portion and
///   an absolute 'offset' portion
/// </summary>
public struct UDim {

  /// <summary>A unified dimension that has been initialized to zero</summary>
  public static readonly UDim Zero = new UDim();

  /// <summary>Initializes a new unified dimension</summary>
  /// <param name="scale">Relative scale portion of the dimension</param>
  /// <param name="offset">Absolute offset portion of the dimension</param>
  public UDim(float scale, float offset) {
    this.Scale = scale;
    this.Offset = offset;
  }

  /// <summary>Creates a unified dimension describing an absolute position</summary>
  /// <param name="offset">Absolute position to place in the unified dimension</param>
  /// <returns>A new unified dimension containing the given absolute position</returns>
  public static UDim Absolute(float offset) {
    return new UDim(0, offset);
  }

  /// <summary>Creates a unified dimension describing a relative position</summary>
  /// <param name="scale">Relative position to place in the unified dimension</param>
  /// <returns>A new unified dimension containing the given relative position</returns>
  public static UDim Relative(float scale) {
    return new UDim(scale, 0);
  }

  /// <summary>Converts the unified dimension into an absolute value</summary>
  /// <param name="extent">Extents in which the absolute position is determined</param>
  /// <returns>The absolute position described by the unified dimension</returns>
  public float AsAbsolute(float extent) {
    return this.Scale * extent + this.Offset;
  }

  /// <summary>Converts the unified dimension into a relative value</summary>
  /// <param name="extent">Extents in which the relative position is determined</param>
  /// <returns>The relative position described by the unified dimension</returns>
  public float AsRelative(float extent) {
    if(extent == 0.0f)
      return this.Scale;
    else
      return this.Offset / extent + this.Scale;
  }

/*
  /// <summary>Implicit conversion from floats</summary>
  /// <param name="offset">Float that will serve as an offset value</param>
  /// <returns>A new unified dimension with the original float as offset</returns>
  public static implicit operator UnifiedDimension(float offset) {
    return UnifiedDimension.Absolute(offset);
  }
*/

  /// <summary>Adds one unified dimension to the other</summary>
  /// <param name="dimension">Base unified dimension to add to</param>
  /// <param name="summand">Dimension to add to the base</param>
  /// <returns>The result of the addition operation</returns>
  public static UDim operator +(
    UDim dimension, UDim summand
  ) {
    return new UDim(
      dimension.Scale + summand.Scale,
      dimension.Scale + summand.Offset
    );
  }

  /// <summary>Subtracts one unified dimension from the other</summary>
  /// <param name="dimension">Base unified dimension to subtract from</param>
  /// <param name="subtrahend">Dimension to subtract from the base</param>
  /// <returns>The result of the subtraction operation</returns>
  public static UDim operator -(
    UDim dimension, UDim subtrahend
  ) {
    return new UDim(
      dimension.Scale - subtrahend.Scale,
      dimension.Scale - subtrahend.Offset
    );
  }

  /// <summary>Divides one unified dimension by another one</summary>
  /// <param name="dimension">Base unified dimension to be divided</param>
  /// <param name="divisor">Divisor to divide by</param>
  /// <returns>The result of the division operation</returns>
  public static UDim operator /(
    UDim dimension, UDim divisor
  ) {
    return new UDim(
      dimension.Scale / divisor.Scale,
      dimension.Scale / divisor.Offset
    );
  }

  /// <summary>Multiplies one unified dimension with another one</summary>
  /// <param name="dimension">Base unified dimension to be multiplied</param>
  /// <param name="factor">Factor to multiply by</param>
  /// <returns>The result of the multiplication operation</returns>
  public static UDim operator *(
    UDim dimension, UDim factor
  ) {
    return new UDim(
      dimension.Scale * factor.Scale,
      dimension.Scale * factor.Offset
    );
  }

  /// <summary>Checks two unified dimensions for inequality</summary>
  /// <param name="first">First dimension to be compared</param>
  /// <param name="second">Second dimension to be compared</param>
  /// <returns>True if the instances differ or exactly one reference is set to null</returns>
  public static bool operator !=(UDim first, UDim second) {
    return !(first == second);
  }

  /// <summary>Checks two unified dimensions for equality</summary>
  /// <param name="first">First dimension to be compared</param>
  /// <param name="second">Second dimension to be compared</param>
  /// <returns>True if both instances are equal or both references are null</returns>
  public static bool operator ==(UDim first, UDim second) {
    if(ReferenceEquals(first, null))
      return ReferenceEquals(second, null);

    return first.Equals(second);
  }

  /// <summary>Checks whether another instance is equal to this instance</summary>
  /// <param name="other">Other instance to compare to this instance</param>
  /// <returns>True if the other instance is equal to this instance</returns>
  public override bool Equals(object other) {
    if(!(other is UDim))
      return false;

    return Equals((UDim)other);
  }

  /// <summary>Checks whether another instance is equal to this instance</summary>
  /// <param name="other">Other instance to compare to this instance</param>
  /// <returns>True if the other instance is equal to this instance</returns>
  public bool Equals(UDim other) {
    if(ReferenceEquals(other, null))
      return false;

    return (this.Scale == other.Scale) && (this.Offset == other.Offset);
  }

  /// <summary>Obtains a hash code of this instance</summary>
  /// <returns>The hash code of the instance</returns>
  public override int GetHashCode() {
    return this.Offset.GetHashCode() ^ this.Scale.GetHashCode();
  }

  /// <summary>Relative portion of the dimension</summary>
  public float Scale;
  /// <summary>Absolute portion of the dimension</summary>
  public float Offset;

}

} // namespace CeGui
