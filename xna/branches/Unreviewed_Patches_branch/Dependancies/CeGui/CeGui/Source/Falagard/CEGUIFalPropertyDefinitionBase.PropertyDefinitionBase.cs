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
///   Common base class used for types representing a new property to be available
///   on all widgets that use the WidgetLook that the property definition is a part of
/// </summary>
public abstract class PropertyDefinitionBase : Property {

  /// <summary>Initializes a new instance of PropertyDefinitionBase</summary>
  /// <param name="name">Name of the property definition</param>
  /// <param name="help">Help text for this property definition</param>
  /// <param name="initialValue">Initial value to assign to properties</param>
  /// <param name="redrawOnWrite">Requires redraw when property changes?</param>
  /// <param name="layoutOnWrite">Requires layout update when property changes?</param>
  public PropertyDefinitionBase(
    string name, string help, string initialValue,
    bool redrawOnWrite, bool layoutOnWrite
  ) : base(name, help, initialValue) {

    this.writeCausesRedraw = redrawOnWrite;
    this.writeCausesLayout = layoutOnWrite;
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
  public void Set(PropertyReceiver receiver, string value) {

    if(this.writeCausesLayout)
      (receiver as Window).PerformChildWindowLayout();

    if(this.writeCausesRedraw)
      (receiver as Window).RequestRedraw();

  }

  /// <summary>
  ///   Writes an xml representation of the PropertyDefinitionBase based
  ///   object into xmlStream
  /// </summary>
  /// <param name="xmlStream">Stream where xml data should be output</param>
  public virtual void WriteXmlToStream(XmlWriter xmlStream) {
/*  
    // write out the element type
    writeXMLElementType(xml_stream);
    // write attributes
    writeXMLAttributes(xml_stream);
    // close tag
    xml_stream.closeTag();
*/    
  }

  /// <summary>
  ///   Write out the text of the XML element type. Note that you should not
  ///   write the opening '<' character, nor any other information such as
  ///   attributes in this function. The writeExtraAttributes function can be
  ///   used for writing attributes
  /// </summary>
  /// <param name="xmlStream">Stream where xml data should be output</param>
  protected abstract void WriteXmlElementType(XmlWriter xmlStream);

  /// <summary>
  ///   Write out any xml attributes added in a sub-class. Note that you should
  ///   not write the closing '/>' character sequence, nor any other information
  ///   in this function. You should always call the base class implementation
  ///   of this function when overriding
  /// </summary>
  /// <param name="xmlStream">Stream where xml data should be output</param>
  protected virtual void WriteXmlAttributes(XmlWriter xmlStream) {
/*
    // write the name of the property
    xml_stream.attribute("name", d_name);
 
    // write initial value, if any
    if(!d_default.empty())
      xml_stream.attribute("initialValue", d_default);
 
    // write option to redraw when property is written
    if(d_writeCausesRedraw)
      xml_stream.attribute("redrawOnWrite", "true");
 
    // write option to loayout children when property is written
    if(d_writeCausesLayout)
      xml_stream.attribute("layoutOnWrite", "true");
*/
  }

  /// <summary>Whether the window need to be redrawn when this property changes</summary>
  protected bool writeCausesRedraw;
  /// <summary>Whether the layout needs to updated when this property changes</summary>
  protected bool writeCausesLayout;

}

} // namespace CeGui
