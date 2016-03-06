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
/// class representing a new property to be available on all widgets that use the WidgetLook
/// that this PropertyDefinition is defiend for.
/// </summary>
public class PropertyDefinition : Property {
  #region Fields
  protected string userStrinName;
  protected bool writeCausesRedraw;
  protected bool writeCausesLayout;
  #endregion

  #region Constructors
  public PropertyDefinition(string name, string initialValue, bool redrawOnWrite, bool layoutOnWrite)
    : base(name, "Falagard custom property definition - gets/sets a named user string.", initialValue) {
    userStrinName = name + "_fal_auto_prop__";
    writeCausesRedraw = redrawOnWrite;
    writeCausesLayout = layoutOnWrite;
  }
  #endregion

  #region Methods
  /// <summary>
  /// Return the current value of the Property as a String
  /// </summary>
  /// <param name="reciever">target object</param>
  /// <returns>String object containing a textual representation of the current value of the Property</returns>
  public override string Get(PropertySet reciever) {
    throw new NotImplementedException();
  }

  /// <summary>
  /// Sets the value of the property
  /// </summary>
  /// <param name="reciever">target object</param>
  /// <param name="val">A String object that contains a textual representation of the new value to assign to the Property.</param>
  public override void Set(PropertySet reciever, string val) {
    throw new NotImplementedException();
  }
  #endregion
}

} // namespace CeGui
