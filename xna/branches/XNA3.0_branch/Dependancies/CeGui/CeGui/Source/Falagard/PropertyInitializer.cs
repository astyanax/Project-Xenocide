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
/// Class that holds information about a property and it's required initial value.
/// </summary>
public class PropertyInitialiser {
  #region Fields
  /// <summary>
  /// Name of a property to be set.
  /// </summary>
  protected string propertyName;
  /// <summary>
  /// Value string to be set on the property.
  /// </summary>
  protected string propertyValue;
  #endregion

  #region Constructors
  /// <summary>
  /// Constructor
  /// </summary>
  /// <param name="prop">String holding the name of the property targetted by this PropertyInitialiser.</param>
  /// <param name="val">String holding the value to be set by this PropertyInitialiser.</param>
  public PropertyInitialiser(string prop, string val) {
    propertyName = prop;
    propertyValue = val;
  }
  #endregion

  #region Properties
  /// <summary>
  /// Return the name of the property targetted by this PropertyInitialiser.
  /// </summary>
  public string TargetPropertyName {
    get { return propertyName; }
  }

  /// <summary>
  /// Return the value string to be set on the property targetted by this PropertyInitialiser.
  /// </summary>
  public string InitializerValue {
    get { return propertyValue; }
  }
  #endregion

  #region Methods
  /// <summary>
  /// Apply this property initialiser to the specified target CEGUI::PropertySet object.
  /// </summary>
  /// <param name="target">PropertySet object to be initialised by this PropertyInitialiser.</param>
  public void Apply(PropertySet target) {
    if(target.IsPropertyPresent(propertyName)) {
      // TODO: Uncomment again
      // target[propertyName].Set(target, propertyValue);
    }
  }
  #endregion
}

} // namespace CeGui
