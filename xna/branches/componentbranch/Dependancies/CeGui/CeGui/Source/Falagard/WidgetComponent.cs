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

namespace CeGui {

/// <summary>
/// Class that encapsulates information regarding a sub-widget required for a widget.
/// 
/// TODO: Much more porting work to be done here.
/// 
/// TODO: This is not finished in the slightest!  There will be many changes here...
/// </summary>
public class WidgetComponent {

  /// <summary>
  /// Destination area for the widget (relative to it's parent).
  /// </summary>
  protected ComponentArea area;
  /// <summary>
  /// Type of widget to be created.
  /// </summary>
  protected string baseType;
  /// <summary>
  /// Name of a WidgetLookFeel to be used for the widget.
  /// </summary>
  protected string imageryName;
  /// <summary>
  /// Suffix to apply to the parent Window name to create this widgets unique name.
  /// </summary>
  protected string nameSuffix;
  /// <summary>
  /// Vertical alignment to be used for this widget.
  /// </summary>
  protected VerticalAlignment vertAlignment;
  /// <summary>
  /// Horizontal alignment to be used for this widget.
  /// </summary>
  protected HorizontalAlignment horzAlignment;
  /// <summary>
  /// Collection of PropertyInitialisers to be applied the the widget upon creation.
  /// </summary>
  protected List<PropertyInitialiser> properties;

  #region Constructors
  /// <summary>
  /// Constructor
  /// </summary>
  /// <param name="type">Type of widget to be created</param>
  /// <param name="look">Name of WidgetLookFeel to be used for the widget</param>
  /// <param name="suffix">Suffix to apply to the parent Window name to create this widget's unique name.</param>
  public WidgetComponent(string type, string look, string suffix) {
    baseType = type;
    imageryName = look;
    nameSuffix = suffix;
    vertAlignment = VerticalAlignment.Top;
    horzAlignment = HorizontalAlignment.Left;
  }
  #endregion

  #region Properties
  /// <summary>
  /// Area of the widget (relative to it's parent).
  /// </summary>
  public ComponentArea ComponentArea {
    get { return area; }
    set { area = value; }
  }

  /// <summary>
  /// Type of widget
  /// </summary>
  public string BaseWidgetType
  {
    get { return baseType; }
    set { baseType = value; }
  }

  /// <summary>
  /// Name of WidgetLookFeel used for the widget
  /// </summary>
  public string WidgetLookName
  {
    get { return imageryName; }
    set { imageryName = value; }
  }

  /// <summary>
  /// Suffix applied to the parent Window name to create this widget's unique name.
  /// </summary>
  public string WidgetNameSuffix
  {
    get { return nameSuffix; }
    set { nameSuffix = value; }
  }

  /// <summary>
  /// Vertical alignment to be used for this widget.
  /// </summary>
  public VerticalAlignment VerticalAlignment
  {
    get { return vertAlignment; }
    set { vertAlignment = value; }
  }

  /// <summary>
  /// Horizontal alignment to be used for this widget.
  /// </summary>
  public HorizontalAlignment HorizontalAlignment
  {
    get { return horzAlignment; }
    set { horzAlignment = value; }
  }
  #endregion

  #region Methods
  /// <summary>
  /// Create an instance of this widget component adding it as a child to the given Window.
  /// </summary>
  /// <param name="parent"></param>
  public void Create(Window parent) {
    // build final name and create widget.
    string widgetName = parent.Name + nameSuffix;
    Window widget = WindowManager.Instance.CreateWindow(baseType, widgetName);

    // set the widget look
    if(imageryName.Length > 0)
      widget.SetLookNFeel("", imageryName);

    // add the new widget to its parent
    parent.AddChild(widget);

    // set alignment options
    widget.VerticalAlignment = vertAlignment;
    widget.HorizontalAlignment = horzAlignment;

    // TODO: We still need code to specify position and SizeF.  Due to the advanced
    // TODO: possibilities, this is better handled via a 'layout' method instead of
    // TODO: setting this once and forgetting about it.

    // initialise properties.  This is done last so that these properties can
    // override properties specified in the look assigned to the created widget.
    foreach(PropertyInitialiser property in properties) {
      property.Apply(widget);
    }
  }

  /// <summary>
  /// Add an initialiser to the Collection of PropertyInitialisers to be applied the the widget upon creation.
  /// </summary>
  /// <param name="initializer">initialise to add</param>
  public void AddPropertyInitializer(PropertyInitialiser initializer) {
    properties.Add(initializer);
  }

  /// <summary>
  /// Get rid of all PropertyInitialisers to apply the the widget upon creation.
  /// </summary>
  public void ClearPropertyInitializers()
  {
    properties.Clear();
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="owner"></param>
  public void Layout(Window owner)
  {
    try {
      WindowManager.Instance.GetWindow(owner.Name + nameSuffix).Rect = area.GetPixelRect(owner);
    }
    catch(Exception) { }
  }
  #endregion
}

} // namespace CeGui
