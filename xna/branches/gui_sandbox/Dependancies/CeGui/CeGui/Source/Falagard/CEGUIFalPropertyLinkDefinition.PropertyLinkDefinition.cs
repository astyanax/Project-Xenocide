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

/// <summary>
///   Class representing a property that links to another property defined on
///   an attached child widget
/// </summary>
public class PropertyLinkDefinition : PropertyDefinitionBase {

  /// <summary>Initializes a new property link definition</summary>
  /// <param name="propertyName">Name of the property</param>
  /// <param name="widgetNameSuffix">TODO: What is this?</param>
  /// <param name="targetProperty">TODO: What does this do?</param>
  /// <param name="initialValue">Initial value to assign to the property</param>
  /// <param name="redrawOnWrite">Redraw required when property changes?</param>
  /// <param name="layoutOnWrite">Layout update required when property changes?</param>
  public PropertyLinkDefinition(
    string propertyName, string widgetNameSuffix, string targetProperty,
    string initialValue, bool redrawOnWrite, bool layoutOnWrite
  ) : base(
    propertyName,
    "Falagard property link definition - links a property on this window to " +
      "another defined on a child window.",
    initialValue,
    redrawOnWrite,
    layoutOnWrite
  ) {
    this.widgetNameSuffix = widgetNameSuffix;
    this.targetProperty = targetProperty;
  }

  /// <summary>Return the current value of the Property as a String</summary>
  /// <param name="receiver">target object</param>
  /// <returns>
  ///   A string containing a textual representation of the current value of
  ///   the property
  /// </returns>
  public override string Get(PropertySet receiver) {
    // TODO: String cast should not be required, GetProperty should return a string
    return (string)GetTargetWindow(receiver).GetProperty(
      (this.targetProperty == string.Empty) ? this.name : this.targetProperty
    );
  }

  /// <summary>Sets the value of the property</summary>
  /// <param name="receiver">Pointer to the target object</param>
  /// <param name="value">
  ///   String that contains a textual representation of the new value to assign
  ///   to the Property
  /// </param>
  /// <remarks>
  ///   When overriding the set() member of PropertyDefinitionBase, you MUST call
  ///   the base class implementation after you have set the property value
  ///   (i.e. you must call PropertyDefinitionBase::set())
  /// </remarks>
  public override void Set(PropertySet receiver, string value) {
    GetTargetWindow(receiver).SetProperty(
      (this.targetProperty == string.Empty) ? this.name : this.targetProperty,
      value
    );
    base.Set(receiver, value);
  }

  /// <summary>
  ///   Write out the text of the XML element type. Note that you should not
  ///   write the opening '<' character, nor any other information such as
  ///   attributes in this function. The writeExtraAttributes function can be
  ///   used for writing attributes
  /// </summary>
  /// <param name="xmlStream">Stream where xml data should be output</param>
  protected override void WriteXmlElementType(System.Xml.XmlWriter xmlStream) {
/*
    xml_stream.openTag("PropertyLinkDefinition");
*/
  }

  /// <summary>
  ///   Write out any xml attributes added in a sub-class. Note that you should
  ///   not write the closing '/>' character sequence, nor any other information
  ///   in this function. You should always call the base class implementation
  ///   of this function when overriding
  /// </summary>
  /// <param name="xmlStream">Stream where xml data should be output</param>
  protected override void WriteXmlAttributes(System.Xml.XmlWriter xmlStream) {
/*
    if (!d_widgetNameSuffix.empty())
        xml_stream.attribute("widget", d_widgetNameSuffix);

    if (!d_targetProperty.empty())
        xml_stream.attribute("targetProperty",  d_targetProperty);
*/
  }

  /// <summary>
  ///   Return a pointer to the window containing the target property to be accessed
  /// </summary>
  /// <param name="receiver">Property receiver</param>
  /// <returns>The window containing the target property</returns>
  protected Window GetTargetWindow(PropertyReceiver receiver) {

    // if no name suffix, we are the target (not very useful, but still...)
    if(this.widgetNameSuffix == string.Empty)
      return (receiver as Window);

    return WindowManager.Instance.GetWindow(
      (receiver as Window).Name + this.widgetNameSuffix
    );

  }

  /// <summary>TODO: What is this?</summary>
  protected string widgetNameSuffix;
  /// <summary>TODO: What does this do?</summary>
  protected string targetProperty;

}

} // namespace CeGui
