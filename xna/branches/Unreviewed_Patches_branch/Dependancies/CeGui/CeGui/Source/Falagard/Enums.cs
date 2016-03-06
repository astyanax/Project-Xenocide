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
/// Enumeration of possible values to indicate what a given dimension represents.
/// </summary>
public enum DimensionType {
  /// <summary>
  /// Dimension represents the left edge of some entity (same as DT_X_POSITION).
  /// </summary>
  LeftEdge,
  /// <summary>
  /// Dimension represents the x position of some entity (same as DT_LEFT_EDGE).
  /// </summary>
  XPosition,
  /// <summary>
  /// Dimension represents the top edge of some entity (same as DT_Y_POSITION).
  /// </summary>
  TopEdge,
  /// <summary>
  /// Dimension represents the y position of some entity (same as DT_TOP_EDGE).
  /// </summary>
  YPosition,
  /// <summary>
  /// Dimension represents the right edge of some entity.
  /// </summary>
  RightEdge,
  /// <summary>
  /// Dimension represents the bottom edge of some entity.
  /// </summary>
  BottomEdge,
  /// <summary>
  /// Dimension represents the width of some entity.
  /// </summary>
  Width,
  /// <summary>
  /// Dimension represents the height of some entity.
  /// </summary>
  Height,
  /// <summary>
  /// Dimension represents the x offset of some entity (usually only applies to an Image entity).
  /// </summary>
  XOffset,
  /// <summary>
  /// Dimension represents the y offset of some entity (usually only applies to an Image entity).
  /// </summary>
  YOffset,
  /// <summary>
  /// Invalid / uninitialised DimensionType.
  /// </summary>
  Invalid
}

/// <summary>
/// Enumeration of possible values to indicate the vertical formatting to be used for an image component.
/// </summary>
public enum VerticalFormatting {
  /// <summary>
  /// Top of Image should be aligned with the top of the destination area.
  /// </summary>
  TopAligned,
  /// <summary>
  /// Image should be vertically centred within the destination area.
  /// </summary>
  CenterAligned,
  /// <summary>
  /// Bottom of Image should be aligned with the bottom of the destination area.
  /// </summary>
  BottomAligned,
  /// <summary>
  /// Image should be stretched vertically to fill the destination area.
  /// </summary>
  Stretched,
  /// <summary>
  /// Image should be tiled vertically to fill the destination area (bottom-most tile may be clipped).
  /// </summary>
  Tiled
}

/// <summary>
/// Enumeration of possible values to indicate the horizontal formatting to be used for an image component.
/// </summary>
public enum HorizontalFormatting {
  /// <summary>
  /// Left of Image should be aligned with the left of the destination area.
  /// </summary>
  LeftAligned,
  /// <summary>
  /// Image should be horizontally centred within the destination area.
  /// </summary>
  CenterAligned,
  /// <summary>
  /// Right of Image should be aligned with the right of the destination area.
  /// </summary>
  RightAligned,
  /// <summary>
  /// Image should be stretched horizontally to fill the destination area.
  /// </summary>
  Stretched,
  /// <summary>
  /// Image should be tiled horizontally to fill the destination area (right-most tile may be clipped).
  /// </summary>
  Tiled
}

/// <summary>
/// Enumeration of possible values to indicate the vertical formatting to be used for a text component.
/// </summary>
public enum VerticalTextFormatting {
  /// <summary>
  /// Top of text should be aligned with the top of the destination area.
  /// </summary>
  TopAligned,
  /// <summary>
  /// text should be vertically centred within the destination area.
  /// </summary>
  CenterAligned,
  /// <summary>
  /// Bottom of text should be aligned with the bottom of the destination area.
  /// </summary>
  BottomAligned
}

/// <summary>
/// Enumeration of possible values to indicate the horizontal formatting to be used for a text component.
/// </summary>
public enum HorizontalTextFormatting {
  /// <summary>
  /// Left of text should be aligned with the left of the destination area (single line of text only).
  /// </summary>
  LeftAligned,
  /// <summary>
  /// Right of text should be aligned with the right of the destination area  (single line of text only).
  /// </summary>
  RightAligned,
  /// <summary>
  /// text should be horizontally centred within the destination area  (single line of text only).
  /// </summary>
  CenterAligned,
  /// <summary>
  /// text should be spaced so that it takes the full width of the destination area (single line of text only).
  /// </summary>
  Justigied,
  /// <summary>
  /// Left of text should be aligned with the left of the destination area (word wrapped to multiple lines as needed).
  /// </summary>
  WrapLeftAligned,
  /// <summary>
  /// Right of text should be aligned with the right of the destination area  (word wrapped to multiple lines as needed).
  /// </summary>
  WrapRightAligned,
  /// <summary>
  /// text should be horizontally centred within the destination area  (word wrapped to multiple lines as needed).
  /// </summary>
  WrapCenterAligned,
  /// <summary>
  /// text should be spaced so that it takes the full width of the destination area (word wrapped to multiple lines as needed).
  /// </summary>
  WrapJustified
}

/// <summary>
/// Enumeration of possible values to indicate a particular font metric.
/// </summary>
public enum FontMetricType {
  /// <summary>
  /// Vertical line spacing value for font.
  /// </summary>
  LineSpacing,
  /// <summary>
  /// Vertical baseline value for font.
  /// </summary>
  Baseline,
  /// <summary>
  /// Horizontal extent of a string.
  /// </summary>
  HorxExtent
}

/// <summary>
/// Enumeration of values representing mathematical operations on dimensions.
/// </summary>
public enum DimensionOperator {
  /// <summary>
  /// Do nothing operator.
  /// </summary>
  Noop,
  /// <summary>
  /// Dims should be added.
  /// </summary>
  Add,
  /// <summary>
  /// Dims should be subtracted.
  /// </summary>
  Subtract,
  /// <summary>
  /// Dims should be multiplied.
  /// </summary>
  Multiply,
  /// <summary>
  /// Dims should be divided.
  /// </summary>
  Divide
}

/// <summary>
/// Enumeration of values referencing available images forming a frame component.
/// </summary>
public enum FrameImageComponent {
  /// <summary>
  /// References image used for the background.
  /// </summary>
  Background,
  /// <summary>
  /// References image used for the top-left corner.
  /// </summary>
  TopLeftCorner,
  /// <summary>
  /// References image used for the top-right corner.
  /// </summary>
  TopRightCorner,
  /// <summary>
  /// References image used for the bottom-left corner.
  /// </summary>
  BottomLeftCorner,
  /// <summary>
  /// References image used for the bottom-right corner.
  /// </summary>
  BottomRightCorner,
  /// <summary>
  /// References image used for the left edge.
  /// </summary>
  LeftEdge,
  /// <summary>
  /// References image used for the right edge.
  /// </summary>
  RightEdge,
  /// <summary>
  /// References image used for the top edge.
  /// </summary>
  TopEdge,
  /// <summary>
  /// References image used for the bottom edge.
  /// </summary>
  BottomEdge,
  /// <summary>
  /// Max number of images for a frame.
  /// </summary>
  FrameImageCount
}

} // namespace CeGui
