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
using System.Drawing;

namespace CeGui {

/// <summary>
/// Class that encapsulates a re-usable collection of imagery specifications.
/// </summary>
public class ImagerySection {
  #region Fields
  /// <summary>
  /// Holds the name of the ImagerySection.
  /// </summary>
  protected string name;
  /// <summary>
  /// Master colours for the the ImagerySection (combined with colours of each ImageryComponent).
  /// </summary>
  protected ColourRect masterColors;
  /// <summary>
  /// Collection of FrameComponent objects to be drawn for this ImagerySection.
  /// </summary>
  protected List<FrameComponent> frames;
  /// <summary>
  /// Collection of ImageryComponent objects to be drawn for this ImagerySection.
  /// </summary>
  protected List<ImageryComponent> images;
  /// <summary>
  /// Collection of TextComponent objects to be drawn for this ImagerySection.
  /// </summary>
  protected List<TextComponent> texts;
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
  /// Return the current master colours set for this ImagerySection.
  /// </summary>
  public ColourRect MasterColors {
    get { return masterColors; }
    set { masterColors = value; }
  }

  /// <summary>
  /// Return the name of this ImagerySection.
  /// </summary>
  public string Name {
    get { return name; }
  }

  /// <summary>
  /// Set the name of the property where master colour values can be obtained.
  /// </summary>
  public string MasterColorPropertySource {
    get { return colorPropertyName; }
    set { colorPropertyName = value; }
  }

  /// <summary>
  /// Set whether the master colours property source represents a full ColourRect.
  /// </summary>
  public bool MasterColorPropertyIsRect {
    get { return colorPropertyIsRect; }
    set { colorPropertyIsRect = value; }
  }
  #endregion

  #region Constructors
  /// <summary>
  /// Constructors
  /// </summary>
  public ImagerySection() {
    masterColors = new ColourRect(new Colour(1.0f, 1.0f, 1.0f));
    colorPropertyIsRect = false;
  }

  /// <summary>
  /// ImagerySection constructor.  Name must be supplied, masterColours are set to 0xFFFFFFFF by default.
  /// </summary>
  /// <param name="name">Name of the new ImagerySection.</param>
  public ImagerySection(string name) {
    masterColors = new ColourRect(new Colour(1.0f, 1.0f, 1.0f));
    colorPropertyIsRect = false;
    this.name = name;
  }
  #endregion

  #region Methods
  /// <summary>
  /// Render this ImagerySection.
  /// </summary>
  /// <param name="srcWindow">Window to use when calculating pixel values from BaseDim values.</param>
  /// <param name="base_z">base level z value to use for all imagery in the layer.</param>
  /// <param name="modcols">ColourRect specifying colours to be modulated with the ImagerySection's master colours.  May be 0.</param>
  /// <param name="clipper"></param>
  /// <param name="clipToDisplay"></param>
  public void Render(Window srcWindow, float base_z, ColourRect modcols, Rect clipper, bool clipToDisplay) {
    throw new NotImplementedException();
  }

  /// <summary>
  /// Render this ImagerySection.
  /// </summary>
  /// <param name="srcWindow">Window to use when calculating pixel values from BaseDim values.</param>
  /// <param name="base_z">base level z value to use for all imagery in the layer.</param>
  /// <param name="modcols">ColourRect specifying colours to be modulated with the ImagerySection's master colours.  May be 0.</param>
  /// <param name="clipper"></param>
  public void Render(Window srcWindow, float base_z, ColourRect modcols, Rect clipper) {
    Render(srcWindow, base_z, modcols, clipper, false);
  }

  /// <summary>
  /// Render this ImagerySection.
  /// </summary>
  /// <param name="srcWindow">Window to use when calculating pixel values from BaseDim values.</param>
  /// <param name="base_z">base level z value to use for all imagery in the layer.</param>
  /// <param name="modcols">ColourRect specifying colours to be modulated with the ImagerySection's master colours.  May be 0.</param>
  public void Render(Window srcWindow, float base_z, ColourRect modcols) {
    Render(srcWindow, base_z, modcols, Rect.Empty, false);
  }

  /// <summary>
  /// Render this ImagerySection.
  /// </summary>
  /// <param name="srcWindow">Window to use when calculating pixel values from BaseDim values.</param>
  /// <param name="base_z">base level z value to use for all imagery in the layer.</param>
  public void Render(Window srcWindow, float base_z) {
    Render(srcWindow, base_z, null, Rect.Empty, false);
  }

  /// <summary>
  /// Render this ImagerySection.
  /// </summary>
  /// <param name="srcWindow">Window to use when calculating pixel values from BaseDim values.</param>
  /// <param name="baseRect">Rect to use when calculating pixel values from BaseDim values.</param>
  /// <param name="base_z">base level z value to use for all imagery in the layer.</param>
  /// <param name="modcols">ColourRect specifying colours to be modulated with the ImagerySection's master colours.  May be 0.</param>
  /// <param name="clipper"></param>
  /// <param name="clipToDisplay"></param>
  void Render(Window srcWindow, Rect baseRect, float base_z, ColourRect modcols, Rect clipper, bool clipToDisplay) {
    throw new NotImplementedException();
  }

  /// <summary>
  /// Render this ImagerySection.
  /// </summary>
  /// <param name="srcWindow">Window to use when calculating pixel values from BaseDim values.</param>
  /// <param name="baseRect">Rect to use when calculating pixel values from BaseDim values.</param>
  /// <param name="base_z">base level z value to use for all imagery in the layer.</param>
  /// <param name="modcols">ColourRect specifying colours to be modulated with the ImagerySection's master colours.  May be 0.</param>
  /// <param name="clipper"></param>
  public void Render(Window srcWindow, Rect baseRect, float base_z, ColourRect modcols, Rect clipper) {
    Render(srcWindow, baseRect, base_z, modcols, clipper, false);
  }

  /// <summary>
  /// Render this ImagerySection.
  /// </summary>
  /// <param name="srcWindow">Window to use when calculating pixel values from BaseDim values.</param>
  /// <param name="baseRect">Rect to use when calculating pixel values from BaseDim values.</param>
  /// <param name="base_z">base level z value to use for all imagery in the layer.</param>
  /// <param name="modcols">ColourRect specifying colours to be modulated with the ImagerySection's master colours.  May be 0.</param>
  public void Render(Window srcWindow, Rect baseRect, float base_z, ColourRect modcols) {
    Render(srcWindow, baseRect, base_z, modcols, Rect.Empty, false);
  }

  /// <summary>
  /// Render this ImagerySection.
  /// </summary>
  /// <param name="srcWindow">Window to use when calculating pixel values from BaseDim values.</param>
  /// <param name="baseRect">Rect to use when calculating pixel values from BaseDim values.</param>
  /// <param name="base_z">base level z value to use for all imagery in the layer.</param>
  public void Render(Window srcWindow, Rect baseRect, float base_z) {
    Render(srcWindow, baseRect, base_z, null, Rect.Empty, false);
  }

  /// <summary>
  /// Add an ImageryComponent to this ImagerySection.
  /// </summary>
  /// <param name="image">ImageryComponent to be added to the section (a copy is made)</param>
  public void AddImageryComponent(ImageryComponent image) {
    images.Add(image);
  }

  /// <summary>
  /// Clear all ImageryComponents from this ImagerySection.
  /// </summary>
  public void ClearImageryComponents() {
    images.Clear();
  }

  /// <summary>
  /// Add a TextComponent to this ImagerySection.
  /// </summary>
  /// <param name="text">TextComponent to be added to the section (a copy is made)</param>
  public void AddTextComponent(TextComponent text) {
    texts.Add(text);
  }

  /// <summary>
  /// Clear all TextComponents from this ImagerySection.
  /// </summary>
  public void ClearTextComponents() {
    texts.Clear();
  }

  /// <summary>
  /// Add a FrameComponent to this ImagerySection.
  /// </summary>
  /// <param name="frame">FrameComponent to be added to the section (a copy is made)</param>
  public void AddFrameComponent(FrameComponent frame) {
    frames.Add(frame);
  }

  /// <summary>
  /// Clear all FrameComponents from this ImagerySection.
  /// </summary>
  public void ClearFrameComponents() {
    frames.Clear();
  }

  /// <summary>
  /// Return smallest Rect that could contain all imagery within this section.
  /// </summary>
  /// <param name="wnd"></param>
  /// <returns></returns>
  public Rect GetBoundingRect(Window wnd) {
    Rect compRect;
    Rect bounds = new Rect(0, 0, 0, 0);

    // measure all frame components
    foreach(FrameComponent frame in frames) {
      compRect = frame.ComponentArea.GetPixelRect(wnd);

      bounds.Left = Math.Min(bounds.Left, compRect.Left);
      bounds.Top = Math.Min(bounds.Top, compRect.Top);
      bounds.Right = Math.Max(bounds.Right, compRect.Right);
      bounds.Bottom = Math.Max(bounds.Bottom, compRect.Bottom);
    }
    // measure all imagery components
    foreach(ImageryComponent image in images) {
      compRect = image.ComponentArea.GetPixelRect(wnd);

      bounds.Left = Math.Min(bounds.Left, compRect.Left);
      bounds.Top = Math.Min(bounds.Top, compRect.Top);
      bounds.Right = Math.Max(bounds.Right, compRect.Right);
      bounds.Bottom = Math.Max(bounds.Bottom, compRect.Bottom);
    }
    // measure all text components
    foreach(TextComponent text in texts) {
      compRect = text.ComponentArea.GetPixelRect(wnd);

      bounds.Left = Math.Min(bounds.Left, compRect.Left);
      bounds.Top = Math.Min(bounds.Top, compRect.Top);
      bounds.Right = Math.Max(bounds.Right, compRect.Right);
      bounds.Bottom = Math.Max(bounds.Bottom, compRect.Bottom);
    }

    return bounds;
  }

  /// <summary>
  /// Return smallest Rect that could contain all imagery within this section.
  /// </summary>
  /// <param name="wnd"></param>
  /// <param name="Rect">section of window to examine</param>
  /// <returns></returns>
  public Rect GetBoundingRect(Window wnd, Rect Rect) {
    Rect compRect;
    Rect bounds = new Rect(0, 0, 0, 0);

    // measure all frame components
    foreach(FrameComponent frame in frames) {
      compRect = frame.ComponentArea.GetPixelRect(wnd, Rect);

      bounds.Left = Math.Min(bounds.Left, compRect.Left);
      bounds.Top = Math.Min(bounds.Top, compRect.Top);
      bounds.Right = Math.Max(bounds.Right, compRect.Right);
      bounds.Bottom = Math.Max(bounds.Bottom, compRect.Bottom);
    }
    // measure all imagery components
    foreach(ImageryComponent image in images) {
      compRect = image.ComponentArea.GetPixelRect(wnd, Rect);

      bounds.Left = Math.Min(bounds.Left, compRect.Left);
      bounds.Top = Math.Min(bounds.Top, compRect.Top);
      bounds.Right = Math.Max(bounds.Right, compRect.Right);
      bounds.Bottom = Math.Max(bounds.Bottom, compRect.Bottom);
    }
    // measure all text components
    foreach(TextComponent text in texts) {
      compRect = text.ComponentArea.GetPixelRect(wnd, Rect);

      bounds.Left = Math.Min(bounds.Left, compRect.Left);
      bounds.Top = Math.Min(bounds.Top, compRect.Top);
      bounds.Right = Math.Max(bounds.Right, compRect.Right);
      bounds.Bottom = Math.Max(bounds.Bottom, compRect.Bottom);
    }

    return bounds;
  }
  #endregion

  #region Protected Methods
  /// <summary>
  ///    Helper method to initialise a ColourRect with appropriate values according to the way the
  ///    ImagerySection is set up.
  ///
  ///    This will try and get values from multiple places:
  ///        - a property attached to \a wnd
  ///        - or the integral d_masterColours value.
  /// </summary>
  /// <param name="wnd"></param>
  /// <param name="cr"></param>
  protected void InitMasterColorRect(Window wnd, ColourRect cr) {
    // if colours come via a colour property
    if(colorPropertyName.Length > 0) {
      // if property accesses a ColourRect
      if(colorPropertyIsRect) {
        cr = ColourRect.Parse(wnd.GetProperty(colorPropertyName));
      }
        // property accesses a colour
    else {
        cr = new ColourRect(Colour.Parse(wnd.GetProperty(colorPropertyName)));
      }
    }
      // use explicit ColourRect.
else {
      cr = masterColors;
    }
  }
  #endregion
}

} // namespace CeGui
