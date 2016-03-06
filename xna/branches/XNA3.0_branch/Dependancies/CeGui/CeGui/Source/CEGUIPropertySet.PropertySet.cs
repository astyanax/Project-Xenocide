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

namespace CeGui {

public abstract class PropertyReceiver {}

/// <summary>Contains a collection of Property objects</summary>
public class PropertySet : PropertyReceiver {

  /// <summary>Initializes a new instance of PropertySet</summary>
  public PropertySet() {
    this.properties = new Dictionary<string, Property>();
  }

  /// <summary>Adds a new Property to the PropertySet</summary>
  /// <param name="property">Property to be added to the PropertySet</param>
  public void AddProperty(Property property) {
    if(this.properties.ContainsKey(property.Name))
      throw new AlreadyExistsException(
        "A Property named '" + property.Name + "' already exists in the PropertySet"
      );

    this.properties.Add(property.Name, property);
  }

  /// <summary>Removes a Property from the PropertySet</summary>
  /// <param name="name">
  ///   String containing the name of the Property to be removed. If Property
  ///   'name' is not in the set, nothing happens
  /// </param>
  public void RemoveProperty(string name) {
    if(this.properties.ContainsKey(name))
      this.properties.Remove(name);
  }

  /// <summary>Removes all Properties from the PropertySet</summary>
  public void ClearProperties() {
    this.properties.Clear();
  }

  /// <summary>Checks to see if a Property with the given name is in the PropertySet</summary>
  /// <param name="name">String containing the name of the Property to check for</param>
  /// <returns>
  ///   true if a Property named 'name' is in the PropertySet. false if no Property named
  ///   'name' is in the PropertySet
  /// </returns>
  public bool IsPropertyPresent(string name) {
    return this.properties.ContainsKey(name);
  }

  /// <summary>Return the help text for the specified Property</summary>
  /// <param name="name">
  ///   String holding the name of the Property who's help text is to be returned
  /// </param>
  /// <returns>String containing the help text for the Property 'name'</returns>
  public string GetPropertyHelp(string name) {
    if(!this.properties.ContainsKey(name))
      throw new UnknownObjectException(
        "There is no Property named '" + name + "' available in the set"
      );

    return this.properties[name].Help;
  }

  /// <summary>Gets the current value of the specified Property</summary>
  /// <param name="name">
  ///   String containing the name of the Property who's value is to be returned
  /// </param>
  /// <returns>String containing a textual representation of the requested Property</returns>
  public string GetProperty(string name) {
    if(!this.properties.ContainsKey(name))
      throw new UnknownObjectException(
        "There is no Property named '" + name + "' available in the set"
      );

    return this.properties[name].Get(this);
  }

  /// <summary>Sets the current value of a Property</summary>
  /// <param name="name">
  ///   String containing the name of the Property who's value is to be set
  /// </param>
  /// <param name="value">
  ///   String containing a textual representation of the new value for the Property
  /// </param>
  public void SetProperty(string name, string value) {
    if(!this.properties.ContainsKey(name))
      throw new UnknownObjectException(
        "There is no Property named '" + name + "' available in the set"
      );

    this.properties[name].Set(this, value);
  }

  /// <summary>Returns whether a Property is at it's default value</summary>
  /// <param name="name">
  ///   String containing the name of the Property who's default state is to be tested
  /// </param>
  /// <returns>
  ///   - true if the property has it's default value
  ///   - false if the property has been modified from it's default value
  /// </returns>
  public bool IsPropertyDefault(string name) {
    if(!this.properties.ContainsKey(name))
      throw new UnknownObjectException(
        "There is no Property named '" + name + "' available in the set"
      );

    return this.properties[name].IsDefault(this);
  }

  /// <summary>Returns the default value of a Property as a String</summary>
  /// <param name="name">
  ///   String containing the name of the Property who's default string is to be returned
  /// </param>
  /// <returns>
  ///   String containing a textual representation of the default value for this property
  /// </returns>
  public string GetPropertyDefault(string name) {
    if(!this.properties.ContainsKey(name))
      throw new UnknownObjectException(
        "There is no Property named '" + name + "' available in the set"
      );

    return this.properties[name].GetDefault(this);
  }

  /// <summary>The properties contained in this set</summary>
  private Dictionary<string, Property> properties;

}

} // namespace CeGui
