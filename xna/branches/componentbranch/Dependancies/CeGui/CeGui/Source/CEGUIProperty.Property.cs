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
///   An abstract class that defines the interface to access object properties by name
/// </summary>
/// <remarks>
///   Property objects allow (via a PropertySet) access to certain properties of objects
///   by using simple get/set functions and the name of the property to be accessed.
/// </remarks>
public abstract class Property {

  /// <summary>Creates a new Property object</summary>
  /// <param name="name">String containing the name of the new Property</param>
  /// <param name="help">String containing a description of the Property and it's usage</param>
  /// <param name="defaultValue">
  ///   String holding the textual representation of the default value for this Property
  /// </param>
  /// <param name="writesXml">
  ///   Specifies whether the writeXMLToStream method should do anything for this Property.
  ///   This enables selectivity in what properties within a PropertySet will get output as XML.
  /// </param>
  public Property(string name, string help, string defaultValue, bool writesXml) {
    this.name = name;
    this.help = help;
    this.def = defaultValue;
    this.writeXml = writesXml;
  }

  /// <summary>Creates a new Property object</summary>
  /// <param name="name">String containing the name of the new Property</param>
  /// <param name="help">String containing a description of the Property and it's usage</param>
  /// <param name="defaultValue">
  ///   String holding the textual representation of the default value for this Property
  /// </param>
  public Property(string name, string help, string defaultValue) :
    this(name, help, defaultValue, true) {}

  /// <summary>Creates a new Property object</summary>
  /// <param name="name">String containing the name of the new Property</param>
  /// <param name="help">String containing a description of the Property and it's usage</param>
  public Property(string name, string help) :
    this(name, help, "", true) {}

  /// <summary>A String that describes the purpose and usage of this Property</summary>
  public string Help {
    get { return help; }
  }

  /// <summary>The name of this Property</summary>
  public string Name {
    get { return name; }
  }

  /// <summary>Return the current value of the Property as a String</summary>
  /// <param name="reciever">target object</param>
  /// <returns>
  ///   A string containing a textual representation of the current value of
  ///   the property
  /// </returns>
  public abstract string Get(PropertySet receiver);

  /// <summary>Sets the value of the property</summary>
  /// <param name="reciever">target object</param>
  /// <param name="val">
  ///   A string that contains a textual representation of the new value to
  ///   assign to the property
  /// </param>
  public abstract void Set(PropertySet receiver, string val);

  /// <summary>Returns whether the property is at it's default value</summary>
  /// <param name="receiver">Pointer to the target object.</param>
  /// <returns>
  ///   - true if the property has its default value.
  ///   - false if the property has been modified from its default value.
  /// </returns>
  public virtual bool IsDefault(PropertySet receiver) {
    return Get(receiver).Equals(def);
  }

  /// <summary>Returns the default value of the Property as a String</summary>
  /// <param name="receiver">target object</param>
  /// <returns>
  ///   String object containing a textual representation of the default
  ///   value for this property
  /// </returns>
  public virtual string GetDefault(PropertySet receiver) {
    return def;
  }

  /// <summary>String that stores the Property name</summary>
  protected string name;
  /// <summary>String that stores the Property help text</summary>
  protected string help;
  /// <summary>String that stores the Property default value string</summary>
  protected string def;
  /// <summary>Specifies whether writeXMLToStream should do anything for this property</summary>
  protected bool writeXml;

}

} // namespace CeGui
