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
using System.Collections.Generic;
using System.Text;

using System.Drawing;

namespace CeGui {

/// <summary>
/// Class used as a two dimensional vector (aka a Point)
/// </summary>
public class Vector2 {
  #region Fields
  protected float x;
  protected float y;
  #endregion Fields

  #region Constructors
  public Vector2() { }
  public Vector2(float x, float y) {
    this.x = x;
    this.y = y;
  }
  #endregion Constructors

  #region Operators
  public static Vector2 operator +(Vector2 a, Vector2 b) {
    return new Vector2(a.x + b.x, a.y + b.y);
  }

  public static Vector2 operator -(Vector2 a, Vector2 b) {
    return new Vector2(a.x - b.x, a.y - b.y);
  }

  public static Vector2 operator /(Vector2 a, Vector2 b) {
    return new Vector2(a.x / b.x, a.y / b.y);
  }

  public static Vector2 operator *(Vector2 a, Vector2 b) {
    return new Vector2(a.x * b.x, a.y * b.y);
  }

  public static bool operator ==(Vector2 a, Vector2 b) {
    return ((a.x == b.x) && (a.y == b.y));
  }

  public static bool operator !=(Vector2 a, Vector2 b) {
    return !(a == b);
  }

  public override bool  Equals(object obj)
{
    Vector2 vec = (Vector2)obj;
    return ((this.x == vec.x) && (this.y == vec.y));
}
    public override int GetHashCode()
    {
        return this.x.GetHashCode() ^ this.y.GetHashCode();
    }
  #endregion Operators

  #region Methods
  public SizeF asSize() {
    return new SizeF(x, y);
  }
  #endregion Methods
}

} // namespace CeGui
