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
/// Class the encapsulates imagery for a given widget state.
/// </summary>
public class StateImagery {
  #region Fields
  /// <summary>
  /// Name of this state.
  /// </summary>
  protected string name;
  /// <summary>
  /// Collection of LayerSpecification objects to be drawn for this state.
  /// </summary>
  protected List<LayerSpecification> layers;
  /// <summary>
  /// true if Imagery for this state should be clipped to the display instead of winodw (effectively, not clipped).
  /// </summary>
  protected bool clipToDisplay;
  #endregion

  #region Properties
  /// <summary>
  /// Return the name of this state.
  /// </summary>
  public string Name {
    get { return name; }
  }

  /// <summary>
  ///    Return whether this state imagery should be clipped to the display rather than the target window.
  ///
  ///    Clipping to the display effectively implies that the imagery should be rendered unclipped.
  /// </summary>
  public bool ClippedToDisplay {
    get { return clipToDisplay; }
    set { clipToDisplay = value; }
  }
  #endregion

  #region Constructors
  /// <summary>
  /// Constructor
  /// </summary>
  public StateImagery()
    : this("") {
  }

  /// <summary>
  /// Constructor
  /// </summary>
  /// <param name="name">Name of the state</param>
  public StateImagery(string name) {
    this.name = name;
    clipToDisplay = false;
  }
  #endregion

  #region Methods
  /// <summary>
  /// Render imagery for this state.
  /// </summary>
  /// <param name="srcWindow">Window to use when convering BaseDim values to pixels.</param>
  /// <param name="modcols"></param>
  /// <param name="clipper"></param>
  public void Render(Window srcWindow, ColourRect modcols, Rect clipper) {
    float base_z;

    // render all layers defined for this state
    foreach(LayerSpecification layer in layers) {
      // TODO: Magic number removal
      base_z = -0.0000001f * (float)layer.LayerPriority;
      layer.Render(srcWindow, base_z, modcols, clipper, clipToDisplay);
    }
  }
  public void Render(Window srcWindow, ColourRect modcols) {
    Render(srcWindow, modcols, Rect.Empty);
  }
  public void Render(Window srcWindow) {
    Render(srcWindow, null, Rect.Empty);
  }

  /// <summary>
  /// Render imagery for this state.
  /// </summary>
  /// <param name="srcWindow">Window to use when convering BaseDim values to pixels.</param>
  /// <param name="baseRect">Rect to use when convering BaseDim values to pixels.</param>
  /// <param name="modcols"></param>
  /// <param name="clipper"></param>
  public void Render(Window srcWindow, Rect baseRect, ColourRect modcols, Rect clipper) {
    float base_z;

    // render all layers defined for this state
    foreach(LayerSpecification layer in layers) {
      // TODO: Magic number removal
      base_z = -0.0000001f * (float)layer.LayerPriority;
      layer.Render(srcWindow, baseRect, base_z, modcols, clipper, clipToDisplay);
    }
  }
  public void Render(Window srcWindow, Rect baseRect, ColourRect modcols) {
    Render(srcWindow, baseRect, modcols, Rect.Empty);
  }
  public void Render(Window srcWindow, Rect baseRect) {
    Render(srcWindow, baseRect, null, Rect.Empty);
  }

  /// <summary>
  /// Add an imagery LayerSpecification to this state.
  /// </summary>
  /// <param name="layer">LayerSpecification to be added to this state (will be copied)</param>
  public void AddLayer(LayerSpecification layer) {
    layers.Add(layer);
  }

  /// <summary>
  /// Removed all LayerSpecifications from this state.
  /// </summary>
  public void ClearLayers() {
    layers.Clear();
  }
  #endregion
}

} // namespace CeGui
