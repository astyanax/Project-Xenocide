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
using System.Drawing;

namespace CeGui {

/// <summary>
/// Class that represents a simple 'link' to an ImagerySection.
/// This class enables sections to be easily re-used, by different states and/or layers, by allowing
/// sections to be specified by name rather than having mutiple copies of the same thing all over the place.
/// </summary>
public class SectionSpecification {
  #region Fields
  /// <summary>
  /// Name of the WidgetLookFeel containing the required section.
  /// </summary>
  protected string owner;
  /// <summary>
  /// Name of the required section within the specified WidgetLookFeel.
  /// </summary>
  protected string sectionName;
  /// <summary>
  /// Colours to use when override is enabled.
  /// </summary>
  protected ColourRect colorsOverride;
  /// <summary>
  /// true if colour override is enabled.
  /// </summary>
  protected bool usingColorOverride;
  /// <summary>
  /// name of property to fetch colours from.
  /// </summary>
  protected string colorPropertyName;
  /// <summary>
  /// true if the colour property will fetch a full ColourRect.
  /// </summary>
  protected bool colorPropertyIsRect;
  #endregion

  #region Properties
  /// <summary>
  /// Return the name of the WidgetLookFeel object containing the target section.
  /// </summary>
  public string OwnerWidgetLookFeel {
    get { return owner; }
  }

  /// <summary>
  /// Return the name of the target ImagerySection.
  /// </summary>
  public string SectionName {
    get { return sectionName; }
  }

  /// <summary>
  /// Return the current override colours.
  /// </summary>
  public ColourRect OverrideColors {
    get { return colorsOverride; }
    set { colorsOverride = value; }
  }

  /// <summary>
  /// return whether the use of override colours is enabled on this SectionSpecification.
  /// </summary>
  public bool UsingOverrideColors {
    get { return usingColorOverride; }
    set { usingColorOverride = value; }
  }

  /// <summary>
  /// Set the name of the property where override colour values can be obtained.
  /// </summary>
  public string OverrideColorsPropertySource {
    get { return colorPropertyName; }
    set { colorPropertyName = value; }
  }

  /// <summary>
  /// Set whether the override colours property source represents a full ColourRect.
  /// </summary>
  public bool OverrideColorsPropertyIsColorRect {
    get { return colorPropertyIsRect; }
    set { colorPropertyIsRect = value; }
  }
  #endregion

  #region Constructors
  /// <summary>
  /// Constructor
  /// </summary>
  /// <param name="owner">String holding the name of the WidgetLookFeel object that contains the target section.</param>
  /// <param name="sectionName">String holding the name of the target section.</param>
  public SectionSpecification(string owner, string sectionName) {
    throw new NotImplementedException();
  }
  /// <summary>
  /// Constructor
  /// </summary>
  /// <param name="owner">String holding the name of the WidgetLookFeel object that contains the target section.</param>
  /// <param name="sectionName">String holding the name of the target section.</param>
  /// <param name="colors">Override colours to be used (modulates sections master colours).</param>
  public SectionSpecification(string owner, string sectionName, ColourRect colors) {
    throw new NotImplementedException();
  }
  #endregion

  #region Methods
  /// <summary>
  /// Render the section specified by this SectionSpecification.
  /// </summary>
  /// <param name="srcWindow">Window object to be used when calculating pixel values from BaseDim values.</param>
  /// <param name="base_z">base z co-ordinate to use for all imagery in the linked section.</param>
  /// <param name="modcols"></param>
  /// <param name="clipper"></param>
  /// <param name="clipToDisplay"></param>
  public void Render(Window srcWindow, float base_z, ColourRect modcols, Rect clipper, bool clipToDisplay) {
    throw new NotImplementedException();
  }

  /// <summary>
  /// Render the section specified by this SectionSpecification.
  /// </summary>
  /// <param name="srcWindow">Window object to be used when calculating pixel values from BaseDim values.</param>
  /// <param name="base_z">base z co-ordinate to use for all imagery in the linked section.</param>
  /// <param name="modcols"></param>
  /// <param name="clipper"></param>
  public void Render(Window srcWindow, float base_z, ColourRect modcols, Rect clipper) {
    Render(srcWindow, base_z, modcols, clipper, false);
  }

  /// <summary>
  /// Render the section specified by this SectionSpecification.
  /// </summary>
  /// <param name="srcWindow">Window object to be used when calculating pixel values from BaseDim values.</param>
  /// <param name="base_z">base z co-ordinate to use for all imagery in the linked section.</param>
  /// <param name="modcols"></param>
  public void Render(Window srcWindow, float base_z, ColourRect modcols) {
    Render(srcWindow, base_z, modcols, Rect.Empty, false);
  }

  /// <summary>
  /// Render the section specified by this SectionSpecification.
  /// </summary>
  /// <param name="srcWindow">Window object to be used when calculating pixel values from BaseDim values.</param>
  /// <param name="base_z">base z co-ordinate to use for all imagery in the linked section.</param>
  public void Render(Window srcWindow, float base_z) {
    Render(srcWindow, base_z, null, Rect.Empty, false);
  }

  /// <summary>
  /// Render the section specified by this SectionSpecification.
  /// </summary>
  /// <param name="srcWindow">Window object to be used when calculating pixel values from BaseDim values.</param>
  /// <param name="baseRect">Rect object to be used when calculating pixel values from BaseDim values.</param>
  /// <param name="base_z">base z co-ordinate to use for all imagery in the linked section.</param>
  /// <param name="modcols"></param>
  /// <param name="clipper"></param>
  /// <param name="clipToDisplay"></param>
  public void Render(Window srcWindow, Rect baseRect, float base_z, ColourRect modcols, Rect clipper, bool clipToDisplay) {
    throw new NotImplementedException();
  }

  /// <summary>
  /// Render the section specified by this SectionSpecification.
  /// </summary>
  /// <param name="srcWindow">Window object to be used when calculating pixel values from BaseDim values.</param>
  /// <param name="baseRect">Rect object to be used when calculating pixel values from BaseDim values.</param>
  /// <param name="base_z">base z co-ordinate to use for all imagery in the linked section.</param>
  /// <param name="modcols"></param>
  /// <param name="clipper"></param>
  public void Render(Window srcWindow, Rect baseRect, float base_z, ColourRect modcols, Rect clipper) {
    Render(srcWindow, baseRect, base_z, modcols, clipper, false);
  }

  /// <summary>
  /// Render the section specified by this SectionSpecification.
  /// </summary>
  /// <param name="srcWindow">Window object to be used when calculating pixel values from BaseDim values.</param>
  /// <param name="baseRect">Rect object to be used when calculating pixel values from BaseDim values.</param>
  /// <param name="base_z">base z co-ordinate to use for all imagery in the linked section.</param>
  /// <param name="modcols"></param>
  public void Render(Window srcWindow, Rect baseRect, float base_z, ColourRect modcols) {
    Render(srcWindow, baseRect, base_z, modcols, Rect.Empty, false);
  }

  /// <summary>
  /// Render the section specified by this SectionSpecification.
  /// </summary>
  /// <param name="srcWindow">Window object to be used when calculating pixel values from BaseDim values.</param>
  /// <param name="baseRect">Rect object to be used when calculating pixel values from BaseDim values.</param>
  /// <param name="base_z">base z co-ordinate to use for all imagery in the linked section.</param>
  public void Render(Window srcWindow, Rect baseRect, float base_z) {
    Render(srcWindow, baseRect, base_z, null, Rect.Empty, false);
  }
  #endregion

  #region Protected Methods
  /// <summary>
  /// Helper method to initialise a ColourRect with appropriate values according to the way the
  /// section sepcification is set up.
  ///
  /// This will try and get values from multiple places:
  /// 	- a property attached to \a wnd
  /// 	- the integral d_coloursOverride values.
  /// 	- or default to colour(1,1,1,1);
  /// </summary>
  /// <param name="wnd"></param>
  /// <param name="cr"></param>
  public void InitColorRectForOverride(Window wnd, ColourRect cr) {
    // if no override set
    if(!usingColorOverride) {
      Colour val = new Colour(1, 1, 1, 1);
      cr.topLeft = val;
      cr.topRight = val;
      cr.bottomLeft = val;
      cr.bottomRight = val;
    }
      // if override comes via a colour property
  else if(colorPropertyName.Length > 0) {
      // if property accesses a ColourRect
      if(colorPropertyIsRect) {
        cr = ColourRect.Parse(wnd.GetProperty(colorPropertyName));
      }
        // property accesses a colour
    else {
        Colour val = Colour.Parse(wnd.GetProperty(colorPropertyName));
        cr.topLeft = val;
        cr.topRight = val;
        cr.bottomLeft = val;
        cr.bottomRight = val;
      }
    }
      // override is an explicitly defined ColourRect.
else {
      cr = colorsOverride;
    }
  }
  #endregion
}

} // namespace CeGui
