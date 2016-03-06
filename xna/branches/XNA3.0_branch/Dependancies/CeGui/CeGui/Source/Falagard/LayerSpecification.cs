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
/// Class that encapsulates a single layer of imagery.
/// </summary>
public class LayerSpecification {
  #region Fields
  /// <summary>
  /// Collection of SectionSpecification objects descibing the sections to be drawn for this layer.
  /// </summary>
  List<SectionSpecification> sections;
  /// <summary>
  /// Priority of the layer.
  /// </summary>
  uint layerPriority;
  #endregion

  #region Properties
  /// <summary>
  /// Return the priority of this layer.
  /// </summary>
  public uint LayerPriority {
    get { return layerPriority; }
  }
  #endregion

  #region Constructors
  /// <summary>
  /// Constructor
  /// </summary>
  /// <param name="priority">
  /// Specifies the priority of the layer.  Layers with higher priorities will be drawn on top
  /// of layers with lower priorities.
  /// </param>
  public LayerSpecification(uint priority) {
    layerPriority = priority;
  }
  #endregion

  #region Methods
  /// <summary>
  /// Render this layer.
  /// </summary>
  /// <param name="srcWindow">Window to use when calculating pixel values from BaseDim values.</param>
  /// <param name="base_z">base level z value to use for all imagery in the layer.</param>
  /// <param name="modcols"></param>
  /// <param name="clipper"></param>
  /// <param name="clipToDisplay"></param>
  public void Render(Window srcWindow, float base_z, ColourRect modcols, Rect clipper, bool clipToDisplay) {
    throw new NotImplementedException();
  }
  public void Render(Window srcWindow, float base_z, ColourRect modcols, Rect clipper) {
    Render(srcWindow, base_z, modcols, clipper, false);
  }
  public void Render(Window srcWindow, float base_z, ColourRect modcols) {
    Render(srcWindow, base_z, modcols, Rect.Empty, false);
  }
  public void Render(Window srcWindow, float base_z) {
    Render(srcWindow, base_z, null, Rect.Empty, false);
  }

  /// <summary>
  /// Render this layer.
  /// </summary>
  /// <param name="srcWindow">Window to use when calculating pixel values from BaseDim values.</param>
  /// <param name="baseRect">Rect to use when calculating pixel values from BaseDim values.</param>
  /// <param name="base_z">base level z value to use for all imagery in the layer.</param>
  /// <param name="modcols"></param>
  /// <param name="clipper"></param>
  /// <param name="clipToDisplay"></param>
  public void Render(Window srcWindow, Rect baseRect, float base_z, ColourRect modcols, Rect clipper, bool clipToDisplay) {
    throw new NotImplementedException();
  }
  public void Render(Window srcWindow, Rect baseRect, float base_z, ColourRect modcols, Rect clipper) {
    Render(srcWindow, baseRect, base_z, modcols, clipper, false);
  }
  public void Render(Window srcWindow, Rect baseRect, float base_z, ColourRect modcols) {
    Render(srcWindow, baseRect, base_z, modcols, Rect.Empty, false);
  }
  public void Render(Window srcWindow, Rect baseRect, float base_z) {
    Render(srcWindow, baseRect, base_z, null, Rect.Empty, false);
  }

  /// <summary>
  /// Add a section specification to the layer.
  /// 
  /// A section specification is a reference to a named ImagerySection within the WidgetLook.
  /// </summary>
  /// <param name="spec">SectionSpecification object descibing the section that should be added to this layer.</param>
  public void AddSpecification(SectionSpecification spec) {
    sections.Add(spec);
  }

  /// <summary>
  /// Clear all section specifications from this layer,
  /// </summary>
  public void ClearSpecifications() {
    sections.Clear();
  }
  #endregion
}

} // namespace CeGui
