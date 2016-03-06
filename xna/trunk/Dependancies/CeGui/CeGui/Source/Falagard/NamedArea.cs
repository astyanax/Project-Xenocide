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

namespace CeGui {

/// <summary>
/// NamedArea defines an area for a component which may later be obtained
/// and referenced by a name unique to the WidgetLook holding the NamedArea.
/// </summary>
public class NamedArea {
  #region Fields
  protected string name;
  protected ComponentArea area;
  #endregion

  #region Properties
  /// <summary>
  /// Gets the name of this NamedArea.
  /// </summary>
  public string Name {
    get { return name; }
  }

  /// <summary>
  /// Gets/Sets the <see cref="ComponentArea"/> of this NamedArea
  /// </summary>
  public ComponentArea Area {
    get { return area; }
    set { area = value; }
  }
  #endregion

  #region Constructors
  /// <summary>
  /// Constructor
  /// </summary>
  /// <param name="name"></param>
  public NamedArea(string name) {
    this.name = name;
  }
  #endregion
}

} // namespace CeGui
