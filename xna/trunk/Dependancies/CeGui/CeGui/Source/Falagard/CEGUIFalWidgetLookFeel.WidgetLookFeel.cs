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
///   Class that encapsulates look & feel information for a particular widget type
/// </summary>
public class WidgetLookFeel {

  /// <summary>Initializes the look and feel informations</summary>
  public WidgetLookFeel() {}

  /// <summary>Initializes the look and feel informations with the given name</summary>
  /// <param name="name">Name to assign to this widget look and feel</param>
  public WidgetLookFeel(string name) {
    lookName = name;
  }

  /// <summary>Returns the StateImagery object for the specified state</summary>
  /// <param name="state">State for which to get the imagery object</param>
  /// <returns>StateImagery object for the requested state</returns>
  public StateImagery GetStateImagery(string state) {
    if(!stateImagery.ContainsKey(state))
      throw new Exception(
        "WidgetLookFeel::getStateImagery - unknown state '" +
          state + "' in look '" + lookName + "'."
      );

    return stateImagery[state];
  }

  /// <summary>Returns the ImagerySection object with the specified name</summary>
  /// <param name="section">Name of the section to return</param>
  /// <returns>ImagerySection object with the specified name</returns>
  public ImagerySection GetImagerySection(string section) {
    if(!imagerySections.ContainsKey(section))
      throw new Exception(
        "WidgetLookFeel::getImagerySection - unknown imagery section '" +
          section + "' in look '" + lookName + "'."
      );

    return imagerySections[section];
  }

  /// <summary>Return the name of the widget look</summary>
  /// <value>String object holding the name of the WidgetLookFeel</value>
  public string Name {
    get { return lookName; }
  }

  /// <summary>Add an ImagerySection to the WidgetLookFeel</summary>
  /// <param name="section">ImagerySection object to be added</param>
  public void AddImagerySection(ImagerySection section) {

    // If an imagery section with the name already exists, replace it
    if(imagerySections.ContainsKey(section.Name))
      imagerySections.Remove(section.Name);

    imagerySections.Add(section.Name, section);
  }

  /// <summary>Add a WidgetComponent to the WidgetLookFeel</summary>
  /// <param name="widget">WidgetComponent object to be added</param>
  public void AddWidgetComponent(WidgetComponent widget) {
    this.childWidgets.Add(widget);
  }

  /// <summary>Add a state specification (StateImagery object) to the WidgetLookFeel</summary>
  /// <param name="state">StateImagery object to be added</param>
  public void addStateSpecification(StateImagery state) {

    // If a state specification with the name already exists, replace it
    if(this.stateImagery.ContainsKey(state.Name))
      this.stateImagery.Remove(state.Name);

    this.stateImagery.Add(state.Name, state);
  }

  /// <summary>Add a property initialiser to the WidgetLookFeel</summary>
  /// <param name="initialiser">PropertyInitialiser object to be added</param>
  public void AddPropertyInitialiser(PropertyInitialiser initialiser) {
    this.properties.Add(initialiser);
  }

  /// <summary>Clear all ImagerySections from the WidgetLookFeel</summary>
  public void ClearImagerySections() {
    this.imagerySections.Clear();
  }

  /// <summary>Clear all WidgetComponents from the WidgetLookFeel</summary>
  public void ClearWidgetComponents() {
    this.childWidgets.Clear();
  }

  /// <summary>Clear all StateImagery objects from the WidgetLookFeel</summary>
  public void ClearStateSpecifications() {
    this.stateImagery.Clear();
  }

  /// <summary>Clear all PropertyInitialiser objects from the WidgetLookFeel</summary>
  public void ClearPropertyInitialisers() {
    this.properties.Clear();
  }

  /// <summary>
  ///   Initialise the given window using PropertyInitialsers and component widgets
  ///   specified for this WidgetLookFeel
  /// </summary>
  /// <param name="widget">Window based object to be initialised</param>
  public void InitialiseWidget(Window widget) {

    // add required child widgets
    foreach(WidgetComponent component in this.childWidgets)
      component.Create(widget);

    // add new property definitions
    foreach(PropertyDefinition definition in this.propertyDefinitions) {

      // add the property to the window
      widget.AddProperty(definition);
      // write default value to get things set up properly
      widget.SetProperty(definition.Name, definition.GetDefault(widget));

    }

    // add new property link definitions
    foreach(PropertyLinkDefinition linkDefinition in this.propertyLinkDefinitions) {

      // add the property to the window
      widget.AddProperty(linkDefinition);
      // write default value to get things set up properly
      widget.SetProperty(linkDefinition.Name, linkDefinition.GetDefault(widget));

    }

    // apply properties to the parent window
    foreach(PropertyInitialiser initialiser in this.properties)
      initialiser.Apply(widget);

  }

#if false

        /*!
        \brief
            Clean up the given window from all properties and component widgets created by this WidgetLookFeel

        \param widget
            Window based object to be cleaned up.

        \return
            Nothing.
        */
        void cleanUpWidget(Window& widget) const;

        /*!
        \brief
            Return whether imagery is defined for the given state.

        \param state
            String object containing name of state to look for.

        \return
            - true if imagery exists for the specified state,
            - false if no imagery exists for the specified state.
        */
        bool isStateImageryPresent(const String& state) const;

        /*!
        \brief
            Adds a named area to the WidgetLookFeel.

        \param area
            NamedArea to be added.

        \return
            Nothing.
        */
        void addNamedArea(const NamedArea& area);

        /*!
        \brief
            Clear all defined named areas from the WidgetLookFeel

        \return
            Nothing.
        */
        void clearNamedAreas();

        /*!
        \brief
            Return the NamedArea with the specified name.

        \param name
            String object holding the name of the NamedArea to be returned.

        \return
            The requested NamedArea object.
        */
        const NamedArea& getNamedArea(const String& name) const;

        /*!
        \brief
            return whether a NamedArea object with the specified name exists for this WidgetLookFeel.

        \param name
            String holding the name of the NamedArea to check for.

        \return
            - true if a named area with the requested name is defined for this WidgetLookFeel.
            - false if no such named area is defined for this WidgetLookFeel.
        */
        bool isNamedAreaDefined(const String& name) const;

        /*!
        \brief
            Layout the child widgets defined for this WidgetLookFeel which are attached to the given window.

        \param owner
            Window object that has the child widgets that require laying out.

        \return
            Nothing.
        */
        void layoutChildWidgets(const Window& owner) const;

        /*!
        \brief
            Adds a property definition to the WidgetLookFeel.

        \param propdef
            PropertyDefinition object to be added.

        \return
            Nothing.
        */
        void addPropertyDefinition(const PropertyDefinition& propdef);

        /*!
        \brief
            Adds a property link definition to the WidgetLookFeel.

        \param propdef
            PropertyLinkDefinition object to be added.

        \return
            Nothing.
        */
        void addPropertyLinkDefinition(const PropertyLinkDefinition& propdef);

        /*!
        \brief
            Clear all defined property definitions from the WidgetLookFeel

        \return
            Nothing.
        */
        void clearPropertyDefinitions();

        /*!
        \brief
            Clear all defined property link definitions from the WidgetLookFeel

        \return
            Nothing.
        */
        void clearPropertyLinkDefinitions();

        /*!
        \brief
            Writes an xml representation of this WidgetLookFeel to \a out_stream.

        \param xml_stream
            Stream where xml data should be output.


        \return
            Nothing.
        */
        void writeXMLToStream(XMLSerializer& xml_stream) const;

        /*!
        \brief
            Uses the WindowManager to rename the child windows that are
            created for this WidgetLookFeel.

        \param widget
            The target Window containing the child windows that are to be
            renamed.

        \param newBaseName
            String object holding the new base name that will be used when
            constructing new names for the child windows.
        */
        void renameChildren(const Window& widget, const String& newBaseName) const;

        /*!
        \brief
            Takes the name of a property and returns a pointer to the last
            PropertyInitialiser for this property or 0 if the is no
            PropertyInitialiser for this property in the WidgetLookFeel

        \param propertyName
            The name of the property to look for.
        */
        const PropertyInitialiser* findPropertyInitialiser(const String& propertyName) const;

        /*!
        \brief
            Takes the namesuffix for a widget component and returns a pointer to
            it if it exists or 0 if it does'nt.

        \param nameSuffix
            The name suffix of the Child component to look for.
        */
        const WidgetComponent* findWidgetComponent(const String& nameSuffix) const;
#endif

  /// <summary>Name of this WidgetLookFeel</summary>
  protected string lookName;
  /// <summary>Collection of ImagerySection objects</summary>
  protected Dictionary<string, ImagerySection> imagerySections;
  /// <summary>Collection of WidgetComponent objects</summary>
  protected List<WidgetComponent> childWidgets;
  /// <summary>Collection of StateImagery objects</summary>
  protected Dictionary<string, StateImagery> stateImagery;
  /// <summary>Collection of PropertyInitialser objects</summary>
  protected List<PropertyInitialiser> properties;
  /// <summary>Collection of NamedArea objects</summary>
  protected Dictionary<string, NamedArea> namedAreas;
  /// <summary>Collection of PropertyDefinition objects</summary>
  protected List<PropertyDefinition> propertyDefinitions;
  /// <summary>Collection of PropertyLinkDefinition objects</summary>
  protected List<PropertyLinkDefinition> propertyLinkDefinitions;

}

} // namespace CeGui
