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
///	Class representing some kind of dimension.
///
///	The key thing to understand about Dimension is that it contains not just a dimensional value,
///	but also a record of what the dimension value is supposed to represent. (e.g. a co-ordinate on
///	the x axis, or the height of something).
/// </summary>
public class Dimension {
  #region Fields
  private BaseDimension dimension;
  private DimensionType type;
  #endregion

  #region Properties
  /// <summary>
  /// Gets/Sets the BaseDimension object used as the value for this Dimension
  /// </summary>
  public BaseDimension BaseDimension {
    get { return dimension; }
    set { dimension = value; }
  }

  /// <summary>
  /// Gets/Sets the type of the Dimension.  See <see cref="DimensionType" />
  /// </summary>
  public DimensionType DimensionType {
    get { return type; }
    set { type = value; }
  }
  #endregion

  #region Constructors
  /// <summary>
  /// Constructor
  /// </summary>
  public Dimension() {
  }

  /// <summary>
  /// Constructor
  /// </summary>
  /// <param name="dim">object based on subclass of BaseDim which holds the dimensional value.</param>
  /// <param name="type">DimensionType value indicating what dimension this object is to represent.</param>
  public Dimension(BaseDimension dim, DimensionType type) {
    dimension = dim;
    this.type = type;
  }
  #endregion

  #region Methods
  /// <summary>
  /// Writes an xml representation of this Dimension to \a out_stream.
  /// </summary>
  /// <param name="writer"></param>
  public void WriteToXml(XmlWriter writer) {
    writer.WriteStartElement("Dim");
    writer.WriteAttributeString("type", dimension.ToString());

    if(dimension != null) {
      dimension.WriteToXml(writer);
    }
    writer.WriteEndElement();
  }
  #endregion
}

} // namespace CeGui
