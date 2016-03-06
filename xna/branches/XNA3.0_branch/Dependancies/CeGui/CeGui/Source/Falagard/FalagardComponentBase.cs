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
/// Common base class used for renderable components within an ImagerySection.
/// </summary>
public abstract class FalagardComponentBase {
  #region Fields
  /// <summary>
  /// Destination area for this component.
  /// </summary>
  protected ComponentArea area;
  /// <summary>
  /// base colours to be applied when rendering the image component.
  /// </summary>
  protected ColourRect colors;
  /// <summary>
  /// name of property to fetch colours from.
  /// </summary>
  protected string colorPropertyName;
  /// <summary>
  /// true if the colour property will fetch a full ColourRect.
  /// </summary>
  protected bool colorPropertyIsRect;
  /// <summary>
  /// name of property to fetch vertical formatting setting from.
  /// </summary>
  protected string vertFormatPropertyName;
  /// <summary>
  /// name of property to fetch horizontal formatting setting from.
  /// </summary>
  protected string horzFormatPropertyName;
  #endregion

  #region Properties
  /// <summary>
  /// Gets/Sets the ComponentArea of this ImageryComponent.
  /// </summary>
  public ComponentArea ComponentArea {
    get { return area; }
    set { area = value; }
  }

  /// <summary>
  /// Gets/Sets the ColourRect set for use by this ImageryComponent.
  /// </summary>
  public ColourRect Colors {
    get { return colors; }
    set { colors = value; }
  }

  /// <summary>
  /// Gets/Set the name of the property where colour values can be obtained.
  /// </summary>
  public string ColorPropertySource {
    get { return colorPropertyName; }
    set { colorPropertyName = value; }
  }

  /// <summary>
  /// Gets/Set whether the colours property source represents a full ColourRect.
  /// </summary>
  public bool ColorPropertyIsRect {
    get { return colorPropertyIsRect; }
    set { colorPropertyIsRect = value; }
  }

  /// <summary>
  /// Gets/Set the name of the property where vertical formatting option can be obtained.
  /// </summary>
  public string VertFormattingPropertySource {
    get { return vertFormatPropertyName; }
    set { vertFormatPropertyName = value; }
  }

  /// <summary>
  /// Gets/Set the name of the property where horizontal formatting option can be obtained.
  /// </summary>
  public string HorzFormattingPropertySource {
    get { return horzFormatPropertyName; }
    set { horzFormatPropertyName = value; }
  }
  #endregion

  #region Constructors
  /// <summary>
  /// Constructor
  /// </summary>
  public FalagardComponentBase() {
    colorPropertyIsRect = false;
    colors = new ColourRect(new Colour(1.0f, 1.0f, 1.0f));
  }
  #endregion

  #region Public Methods

  /// <summary>
  /// Render this component.  More correctly, the component is cached for rendering.
  /// </summary>
  /// <param name="srcWindow">Window to use as the base for translating the component's ComponentArea into pixel values.</param>
  /// <param name="base_z">
  /// The z value to use for rendering the component.  Note that this is not the final z value to use, but
  /// some z offset from a currently unknown starting value.
  /// </param>
  /// <param name="modColours">
  /// ColourRect describing colours that are to be modulated with the component's stored colour values
  /// to calculate a set of 'final' colour values to be used.  May be 0.
  /// </param>
  /// <param name="clipper"></param>
  /// <param name="clipToDisplay"></param>
  public void Render(Window srcWindow, float base_z, ColourRect modColours, Rect clipper, bool clipToDisplay) {
    Rect destRect = area.GetPixelRect(srcWindow);
    RenderImpl(srcWindow, destRect, base_z, modColours, clipper, clipToDisplay);
  }

  /// <summary>
  /// Render this component.  More correctly, the component is cached for rendering.
  /// </summary>
  /// <param name="srcWindow">Window to use as the base for translating the component's ComponentArea into pixel values.</param>
  /// <param name="base_z">
  /// The z value to use for rendering the component.  Note that this is not the final z value to use, but
  /// some z offset from a currently unknown starting value.
  /// </param>
  /// <param name="modColors">
  /// ColourRect describing colours that are to be modulated with the component's stored colour values
  /// to calculate a set of 'final' colour values to be used.  May be 0.
  /// </param>
  /// <param name="clipper"></param>
  public void Render(Window srcWindow, float base_z, ColourRect modColors, Rect clipper) {
    Render(srcWindow, base_z, modColors, clipper, false);
  }

  /// <summary>
  /// Render this component.  More correctly, the component is cached for rendering.
  /// </summary>
  /// <param name="srcWindow">Window to use as the base for translating the component's ComponentArea into pixel values.</param>
  /// <param name="base_z">
  /// The z value to use for rendering the component.  Note that this is not the final z value to use, but
  /// some z offset from a currently unknown starting value.
  /// </param>
  /// <param name="modColors">
  /// ColourRect describing colours that are to be modulated with the component's stored colour values
  /// to calculate a set of 'final' colour values to be used.  May be 0.
  /// </param>
  public void Render(Window srcWindow, float base_z, ColourRect modColors) {
    Render(srcWindow, base_z, modColors, Rect.Empty, false);
  }

  /// <summary>
  /// Render this component.  More correctly, the component is cached for rendering.
  /// </summary>
  /// <param name="srcWindow">Window to use as the base for translating the component's ComponentArea into pixel values.</param>
  /// <param name="base_z">
  /// The z value to use for rendering the component.  Note that this is not the final z value to use, but
  /// some z offset from a currently unknown starting value.
  /// </param>
  public void Render(Window srcWindow, float base_z) {
    Render(srcWindow, base_z, null, Rect.Empty, false);
  }

  /// <summary>
  /// Render this component.  More correctly, the component is cached for rendering.
  /// </summary>
  /// <param name="srcWindow">Window to use as the base for translating the component's ComponentArea into pixel values.</param>
  /// <param name="baseRect">Rect to use as the base for translating the component's ComponentArea into pixel values.</param>
  /// <param name="base_z">
  /// The z value to use for rendering the component.  Note that this is not the final z value to use, but
  /// some z offset from a currently unknown starting value.
  /// </param>
  /// <param name="modColours">
  /// ColourRect describing colours that are to be modulated with the component's stored colour values
  /// to calculate a set of 'final' colour values to be used.  May be 0.
  /// </param>
  /// <param name="clipper"></param>
  /// <param name="clipToDisplay"></param>
  public void Render(Window srcWindow, Rect baseRect, float base_z, ColourRect modColours, Rect clipper, bool clipToDisplay) {
    Rect destRect = area.GetPixelRect(srcWindow, baseRect);
    RenderImpl(srcWindow, destRect, base_z, modColours, clipper, clipToDisplay);
  }

  /// <summary>
  /// Render this component.  More correctly, the component is cached for rendering.
  /// </summary>
  /// <param name="srcWindow">Window to use as the base for translating the component's ComponentArea into pixel values.</param>
  /// <param name="baseRect">Rect to use as the base for translating the component's ComponentArea into pixel values.</param>
  /// <param name="base_z">
  /// The z value to use for rendering the component.  Note that this is not the final z value to use, but
  /// some z offset from a currently unknown starting value.
  /// </param>
  /// <param name="modColours">
  /// ColourRect describing colours that are to be modulated with the component's stored colour values
  /// to calculate a set of 'final' colour values to be used.  May be 0.
  /// </param>
  /// <param name="clipper"></param>
  public void Render(Window srcWindow, Rect baseRect, float base_z, ColourRect modColours, Rect clipper) {
    Render(srcWindow, baseRect, base_z, modColours, clipper, false);
  }

  /// <summary>
  /// Render this component.  More correctly, the component is cached for rendering.
  /// </summary>
  /// <param name="srcWindow">Window to use as the base for translating the component's ComponentArea into pixel values.</param>
  /// <param name="baseRect">Rect to use as the base for translating the component's ComponentArea into pixel values.</param>
  /// <param name="base_z">
  /// The z value to use for rendering the component.  Note that this is not the final z value to use, but
  /// some z offset from a currently unknown starting value.
  /// </param>
  /// <param name="modColours">
  /// ColourRect describing colours that are to be modulated with the component's stored colour values
  /// to calculate a set of 'final' colour values to be used.  May be 0.
  /// </param>
  public void Render(Window srcWindow, Rect baseRect, float base_z, ColourRect modColours) {
    Render(srcWindow, baseRect, base_z, modColours, Rect.Empty, false);
  }

  /// <summary>
  /// Render this component.  More correctly, the component is cached for rendering.
  /// </summary>
  /// <param name="srcWindow">Window to use as the base for translating the component's ComponentArea into pixel values.</param>
  /// <param name="baseRect">Rect to use as the base for translating the component's ComponentArea into pixel values.</param>
  /// <param name="base_z">
  /// The z value to use for rendering the component.  Note that this is not the final z value to use, but
  /// some z offset from a currently unknown starting value.
  /// </param>
  public void Render(Window srcWindow, Rect baseRect, float base_z) {
    Render(srcWindow, baseRect, base_z, null, Rect.Empty, false);
  }
  #endregion

  #region Protected Mehods
  /// <summary>
  /// Helper method to initialise a ColourRect with appropriate values according to the way the
  /// ImageryComponent is set up.
  ///
  /// This will try and get values from multiple places:
  /// 	- a property attached to \a wnd
  /// 	- or the integral d_colours value.
  /// </summary>
  /// <param name="wnd"></param>
  /// <param name="modCols"></param>
  /// <param name="cr"></param>
  protected void InitColorsRect(Window wnd, ColourRect modCols, ColourRect cr) {
    // if colours come via a colour property
    if(colorPropertyName.Length > 0) {
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
      // use explicit ColourRect.
else {
      cr = colors;
    }

    if(modCols != null) {
      throw new NotImplementedException();
      //cr *= modCols;
    }
  }

  /// <summary>
  /// Method to do main render caching work.
  /// </summary>
  /// <param name="srcWindow"></param>
  /// <param name="destRect"></param>
  /// <param name="base_z"></param>
  /// <param name="modColours"></param>
  /// <param name="clipper"></param>
  /// <param name="clipToDisplay"></param>
  protected abstract void RenderImpl(Window srcWindow, Rect destRect, float base_z, ColourRect modColours, Rect clipper, bool clipToDisplay);
  #endregion
}

} // namespace CeGui
