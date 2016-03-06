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
using System.Globalization;

namespace CeGui {

/// <summary>Class representing colour values within the system</summary>
public struct Colour {

  /// <summary>Initializes the color from seperate R, G and B values</summary>
  /// <remarks>Alpha defaults to 1.0f</remarks>
  /// <param name="red">Red component</param>
  /// <param name="green">Green component</param>
  /// <param name="blue">Blue component</param>
  public Colour(float red, float green, float blue) :
    this(red, green, blue, 1.0f) {}

  /// <summary>Initializes the color from seperate R, G, B and alpha values</summary>
  /// <param name="red">Red component</param>
  /// <param name="green">Green component</param>
  /// <param name="blue">Blue component</param>
  /// <param name="alpha">Alpha component</param>
  public Colour(float red, float green, float blue, float alpha) {
    this.red = red;
    this.green = green;
    this.blue = blue;
    this.alpha = alpha;

    this.argb = 0;
    this.argbValid = false;
  }

  /// <summary>Constructor for creating a color from a hex (HTML) value</summary>
  /// <param name="hexValue">Color value in the format 0xAARRGGBB</param>
  public Colour(int hexValue) : this((uint)hexValue) {}

  /// <summary>Constructor for creating a color from a hex (HTML) value</summary>
  /// <param name="hexValue">Color value in the format 0xAARRGGBB</param>
  public Colour(uint hexValue) {
    this.alpha = ((hexValue & 0xFF000000) >> 24) / 255.0f;
    this.red = ((hexValue & 0x00FF0000) >> 16) / 255.0f;
    this.green = ((hexValue & 0x0000FF00) >> 8) / 255.0f;
    this.blue = (hexValue & 0x000000FF) / 255.0f;

    this.argb = hexValue;
    this.argbValid = true;
  }

  /// <summary>Modulates a color by another color</summary>
  /// <param name="left">Color to be modulated</param>
  /// <param name="right">Color to modulate by</param>
  /// <returns>The modulated color</returns>
  public static Colour operator *(Colour left, Colour right) {
    return new Colour(
      left.red * right.red,
      left.green * right.green,
      left.blue * right.blue,
      left.alpha * right.alpha
    );
  }

  /// <summary>Multiplies the components of a color by a factor</summary>
  /// <param name="color">Color whose components are to be multiplied</param>
  /// <param name="factor">Factor to multiply by</param>
  /// <returns>The product of the multiplication</returns>
  public static Colour operator *(Colour color, float factor) {
    return new Colour(
      color.red * factor,
      color.green * factor,
      color.blue * factor,
      color.alpha * factor
    );
  }

  /// <summary>Adds a color to another color</summary>
  /// <param name="left">Color to add to</param>
  /// <param name="right">Color to add</param>
  /// <returns>The sum of the two colors</returns>
  public static Colour operator +(Colour left, Colour right) {
    return new Colour(
      left.red + right.red,
      left.green + right.green,
      left.blue + right.blue,
      left.alpha + right.alpha
    );
  }

  /// <summary>Subtracts a color from another color</summary>
  /// <param name="left">Color to subtract from</param>
  /// <param name="right">Color to subtract</param>
  /// <returns>The color that resulted from the subtraction</returns>
  public static Colour operator -(Colour left, Colour right) {
    return new Colour(
      left.red - right.red,
      left.green - right.green,
      left.blue - right.blue,
      left.alpha - right.alpha
    );
  }

  /// <summary>Returns this color as a 32-bit 0xAARRGGBB integer</summary>
  /// <returns>A 32-bit 0xAARRGGBB representation of this color object.</returns>
  public int ToARGB() {
    if(!this.argbValid) {
      this.argb = calculateArgb();
      this.argbValid = true;
    }

    return (int)this.argb;
  }

  /// <summary>Implicit conversion from System.Drawing.Color</summary>
  /// <param name="color">Color that will be converted to a CeGui.Colour</param>
  /// <returns>The matching CeGui.Colour for the System.Drawing.Color</returns>
  public static implicit operator Colour(System.Drawing.Color color) {
    return new Colour(color.ToArgb());
  }

  /// <summary>Implicit conversion to System.Drawing.Color</summary>
  /// <param name="colour">CeGui.Color that will be converted</param>
  /// <returns>The matching System.Drawing.Color for the CeGui.Colour</returns>
  public static implicit operator System.Drawing.Color(Colour colour) {
    return System.Drawing.Color.FromArgb(colour.ToARGB());
  }

  /// <summary>Sets the color from hue, saturation and luminance parameters</summary>
  /// <param name="hue">Hue used to define the color tone</param>
  /// <param name="saturation">Saturation used to define the intensity of the color</param>
  /// <param name="luminance">Luminance used to define the lightness of the color</param>
  public void SetHsl(float hue, float saturation, float luminance) {
    SetHsl(hue, saturation, luminance, 1.0f);
  }

  /// <summary>Sets the color from hue, saturation and luminance parameters</summary>
  /// <param name="hue">Hue used to define the color tone</param>
  /// <param name="saturation">Saturation used to define the intensity of the color</param>
  /// <param name="luminance">Luminance used to define the lightness of the color</param>
  /// <param name="alpha">Opacity of the color ranging from 0.0f to 1.0f</param>
  public void SetHsl(float hue, float saturation, float luminance, float alpha) {
    throw new NotImplementedException("Not implemented yet, sorry!");
  }

  /// <summary>Inverts the colour's rgb values</summary>
  public void Invert() {
    throw new NotImplementedException("Not implemented yet, sorry!");
  }

  /// <summary>Inverts the colour's rgb and alpha values</summary>
  public void InvertWithAlpha() {
    throw new NotImplementedException("Not implemented yet, sorry!");
  }

  /// <summary>The hue of this color</summary>
  public float Hue {
    get {
      float max = Math.Max(Math.Max(this.red, this.green), this.blue);
      float min = Math.Min(Math.Min(this.red, this.green), this.blue);

      float hue;
      
      if(max == min)
        hue = 0.0f;
      else if(max == this.red)
        hue = (green - blue) / (max - min);
      else if(max == this.green)
        hue = 2.0f + (blue - red) / (max - min);
      else
        hue = 4.0f + (red - green) / (max - min);

      return hue / 6.0f;
    }
  }
  
  /// <summary>The saturation of this color</summary>
  public float Saturation {
    get {
      float max = Math.Max(Math.Max(this.red, this.green), this.blue);
      float min = Math.Min(Math.Min(this.red, this.green), this.blue);

      float lum = (max + min) / 2.0f;
      float sat;

      if(max == min)
        sat = 0.0f;
      else if(lum < 0.5f)
        sat = (max - min) / (max + min);
      else
        sat = (max - min) / (2.0f - max - min);

	  return sat;
    }
  }

  /// <summary>The luminance of this color</summary>
  public float Lumination {
    get {
      float max = Math.Max(Math.Max(this.red, this.green), this.blue);
      float min = Math.Min(Math.Min(this.red, this.green), this.blue);

      return (max + min) / 2.0f;
    }
  }

  /// <summary>Returns a string representation of the color in 0xAARRGGBB format</summary>
  /// <returns>A string 0xAARRGGBB representation of this color object.</returns>
  public override string ToString() {
    return string.Format("{0:x}", ToARGB());
  }

  /// <summary>
  ///   Parses the string representation of a color and returns the corresponding
  ///   Color object.
  /// </summary>
  /// <param name="data">String representation of the Color "AARRGGBB".</param>
  /// <returns>A Color object with the correspoding color value.</returns>
  public static Colour Parse(string data) {
    return new Colour(uint.Parse(data, NumberStyles.HexNumber));
  }

  /// <summary>The red component of the value in range from 0.0 to 1.0</summary>
  public float Red {
    get { return this.red; }
    set {
      this.argbValid = false;
      this.red = value;
    }
  }

  /// <summary>The green component of the value in range from 0.0 to 1.0</summary>
  public float Green {
    get { return this.green; }
    set {
      this.argbValid = false;
      this.green = value;
    }
  }

  /// <summary>The blue component of the value in range from 0.0 to 1.0</summary>
  public float Blue {
    get { return this.blue; }
    set {
      this.argbValid = false;
      this.blue = value;
    }
  }

  /// <summary>The alpha component of the value in range from 0.0 to 1.0</summary>
  public float Alpha {
    get { return this.alpha; }
    set {
      this.argbValid = false;
      this.alpha = value;
    }
  }

  /// <summary>Checks two unified dimensions for inequality</summary>
  /// <param name="first">First dimension to be compared</param>
  /// <param name="second">Second dimension to be compared</param>
  /// <returns>True if the instances differ or exactly one reference is set to null</returns>
  public static bool operator !=(Colour first, Colour second) {
    return !(first == second);
  }

  /// <summary>Checks two unified dimensions for equality</summary>
  /// <param name="first">First dimension to be compared</param>
  /// <param name="second">Second dimension to be compared</param>
  /// <returns>True if both instances are equal or both references are null</returns>
  public static bool operator ==(Colour first, Colour second) {
    if(ReferenceEquals(first, null))
      return ReferenceEquals(second, null);

    return first.Equals(second);
  }

  /// <summary>Checks whether another instance is equal to this instance</summary>
  /// <param name="other">Other instance to compare to this instance</param>
  /// <returns>True if the other instance is equal to this instance</returns>
  public override bool Equals(object other) {
    if(!(other is Colour))
      return false;

    return Equals((Colour)other);
  }

  /// <summary>Checks whether another instance is equal to this instance</summary>
  /// <param name="other">Other instance to compare to this instance</param>
  /// <returns>True if the other instance is equal to this instance</returns>
  public bool Equals(Colour other) {
    if(ReferenceEquals(other, null))
      return false;

    return
      (this.red == other.red) && (this.green == other.green) && (this.blue == other.blue) &&
      (this.alpha == other.alpha);
  }

  /// <summary>Obtains a hash code of this instance</summary>
  /// <returns>The hash code of the instance</returns>
  public override int GetHashCode() {
    return
      this.red.GetHashCode() ^ this.green.GetHashCode() ^ this.blue.GetHashCode() ^
      this.alpha.GetHashCode();
  }

  /// <summary>Computes the argb value of this color</summary>
  /// <returns>This color's argb value</returns>
  private uint calculateArgb() {
    uint result = 0;

    result += ((uint)(Alpha * 255.0f)) << 24;
    result += ((uint)(Red * 255.0f)) << 16;
    result += ((uint)(Green * 255.0f)) << 8;
    result += ((uint)(Blue * 255.0f));

    return result;
  }

  /// <summary>Alpha component [0.0, 1.0]</summary>
  private float alpha;
  /// <summary>Red component [0.0, 1.0]</summary>
  private float red;
  /// <summary>Green component [0.0, 1.0]</summary>
  private float green;
  /// <summary>Blue component [0.0, 1.0]</summary>
  private float blue;
  /// <summary>Whether the cached ARGB value is still up-to-date</summary>
  private bool argbValid;
  /// <summary>Cached argb value, needs to be recomputed if argbValid is false</summary>
  private uint argb;
}

} // namespace CeGui
