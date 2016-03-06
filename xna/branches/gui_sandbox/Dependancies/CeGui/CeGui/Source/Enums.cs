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
using System.Text;

namespace CeGui {

/// <summary>Keys that modify the default behavior of gui elements when pressed</summary>
public enum ModifierKeys {

  /// <summary>No modifier is being applied</summary>
  None,
  /// <summary>One of the control keys is being held</summary>
  Control,
  /// <summary>One of the alternate keys is being held</summary>
  Alt,
  /// <summary>One of the shift keys is being held</summary>
  Shift,
  /// <summary>The caps lock key is being held</summary>
  CapsLock

}

/// <summary>Various levels of logging during runtime</summary>
public enum LoggingLevel {

  /// <summary>Only actual error conditions will be logged</summary>
  Errors,
  /// <summary>Basic events will be logged (default level)</summary>
  Standard,
  /// <summary>Useful tracing (object creations etc) information will be logged</summary>
  Informative,
  /// <summary>
  ///   Mostly everything gets logged (use for heavy tracing only, log WILL be big)
  /// </summary>
  Insane

}

#region Graphics/Formatting

/// <summary>
///		Horizontal formatting options for a <see cref="RenderableImage"/>.
/// </summary>
public enum HorizontalImageFormat {
  /// <summary>
  ///		Image will be rendered at it's natural SizeF and with it's left edge aligned
  ///   with the left edge of the RenderableImage Rect.
  /// </summary>
  LeftAligned,
  /// <summary>
  ///		Image will be rendered at it's natural SizeF and with it's right edge aligned
  ///   with the right edge of the RenderableImage Rect.
  /// </summary>
  RightAligned,
  /// <summary>
  ///		Image will be rendered at it's natural SizeF and horizontally centered
  ///   within the RenderableImage Rect.
  /// </summary>
  Centered,
  /// <summary>
  ///		Image will be horizontally stretched to cover the entire width of
  ///   the RenderableImage Rect.
  /// </summary>
  Stretched,
  /// <summary>
  ///		Image will be tiled horizontally across the width of the RenderableImage Rect.
  ///   The rightmost tile will be clipped to remain within the Rect.
  /// </summary>
  Tiled
}

/// <summary>
///		Contains valid formatting types that can be specified when rendering text into
///   a <see cref="Rect"/> area (the formatting Rect).
/// </summary>
public enum HorizontalTextFormat {
  /// <summary>
  ///		All text is printed on a single line.
  ///		The left-most character is aligned with the left edge of the formatting Rect.
  /// </summary>
  Left,
  /// <summary>
  ///		All text is printed on a single line.
  ///		The right-most character is aligned with the right edge of the formatting Rect.
  /// </summary>
  Right,
  /// <summary>
  ///		All text is printed on a single line.
  ///		The text is centred horizontally in the formatting Rect.
  /// </summary>
  Center,
  /// <summary>
  ///		Text is broken into multiple lines no wider than the formatting Rect.
  ///		The left-most character of each line is aligned with the left edge of the formatting Rect.
  /// </summary>
  WordWrapLeft,
  /// <summary>
  ///		Text is broken into multiple lines no wider than the formatting Rect.
  ///		The right-most character of each line is aligned with the right edge of
  ///   the formatting Rect.
  /// </summary>
  WordWrapRight,
  /// <summary>
  ///		Text is broken into multiple lines no wider than the formatting Rect.
  ///		Each line is centred horizontally in the formatting Rect.
  /// </summary>
  WordWrapCentered
}

/// <summary>
///		Vertical formatting options for a static text widgets.
/// </summary>
public enum VerticalTextFormat {

  /// <summary>
  ///		Text is output with the top of first line of text aligned with the top edge of the widget.
  /// </summary>
  Top,
  /// <summary>
  ///		Text is output with the bottom of last line of text aligned with the bottom edge of
  ///   the widget.
  /// </summary>
  Bottom,
  /// <summary>
  ///		Text is output vertically centered within the widget.
  /// </summary>
  Centered

}

/// <summary>
///		Vertical formatting options for a <see cref="RenderableImage"/>.
/// </summary>
public enum VerticalImageFormat {
  /// <summary>
  ///		Image will be rendered at it's natural size and with it's top edge aligned with
  ///   the top edge of the RenderableImage Rect.
  /// </summary>
  TopAligned,
  /// <summary>
  ///		Image will be rendered at it's natural size and with it's bottom edge aligned with
  ///   the bottom edge of the RenderableImage Rect.
  /// </summary>
  BottomAligned,
  /// <summary>
  ///		Image will be rendered at it's natural size and vertically centered within
  ///   the RenderableImage Rect.
  /// </summary>
  Centered,
  /// <summary>
  ///		Image will be vertically stretched to cover the entire height of
  ///   the RenderableImage Rect.
  /// </summary>
  Stretched,
  /// <summary>
  ///		Image will be tiled vertically down the height of the RenderableImage Rect.
  ///   The bottommost tile will be clipped to remain within the Rect.
  /// </summary>
  Tiled
}

/// <summary>
///		Mode used for Window SizeF and position metrics.
/// </summary>
/// <remarks>
///		Position information for a Window is always 'relative' to it's parent even in Absolute mode.
///		In Relative mode, layout is maintained for different screen resolutions, and also offers the
///		ability for child windows to properly adjust their layout as their parent is sized.
/// </remarks>
public enum MetricsMode {
  /// <summary>
  ///		Metrics are specified as a decimal fraction of parent Window SizeF.
  /// </summary>
  Relative,
  /// <summary>
  ///		Metrics are specified as whole pixels.
  /// </summary>
  Absolute,
  /// <summary>
  ///		Metrics are inherited from parent.
  /// </summary>
  Inherited
}

/// <summary>
///   Enumerated type used when specifying vertical alignments.
/// </summary>
public enum VerticalAlignment {
  /// <summary>
  ///   Elements position specifies an offset of it's top edge from the top edge of it's parent.
  /// </summary>
  Top,
  /// <summary>
  ///   Elements position specifies an offset of it's vertical centre from
  ///   the vertical centre of it's parent.
  /// </summary>
  Center,
  /// <summary>
  ///   Elements position specifies an offset of it's bottom edge from the bottom edge of it's parent.
  /// </summary>
  Bottom
}

/// <summary>
///   Enumerated type used when specifying horizontal alignments.
/// </summary>
public enum HorizontalAlignment {
  /// <summary>
  ///   Elements position specifies an offset of it's left edge from the left edge of it's parent.
  /// </summary>
  Left,
  /// <summary>
  ///   Elements position specifies an offset of it's horizontal centre from
  ///   the horizontal centre of it's parent.
  /// </summary>
  Center,
  /// <summary>
  ///   Elements position specifies an offset of it's right edge from the right edge of
  ///   it's parent.
  /// </summary>
  Right
}

#endregion Graphics/Formatting

#region Behavior

/// <summary>
///		The set of possible locations for the mouse on a frame windows sizing border.
/// </summary>
public enum SizingLocation {
  /// <summary>
  ///		Position is not a sizing location.
  /// </summary>
  None,
  /// <summary>
  ///		Position will size from the top-left.
  /// </summary>
  TopLeft,
  /// <summary>
  ///		Position will size from the top-right.
  /// </summary>
  TopRight,
  /// <summary>
  ///		Position will size from the bottom left.
  /// </summary>
  BottomLeft,
  /// <summary>
  ///		Position will size from the bottom right.
  /// </summary>
  BottomRight,
  /// <summary>
  ///		Position will size from the top.
  /// </summary>
  Top,
  /// <summary>
  ///		Position will size from the left.
  /// </summary>
  Left,
  /// <summary>
  ///		Position will size from the bottom.
  /// </summary>
  Bottom,
  /// <summary>
  ///		Position will size from the right.
  /// </summary>
  Right
}

/// <summary>
///   Enumeration of possible values for sorting direction
///   as used with ListHeaderSegment, ListHeader, and MultiColumnList classes.
/// </summary>
public enum SortDirection {

  /// <summary>Items should not be sorted</summary>
  None,

  /// <summary>Items should be sorted in ascending order</summary>
  Ascending,

  /// <summary>Items should be sorted in descending order</summary>
  Descending
}


/// <summary>
///   Enumerated values for the selection modes
///   possible with a Multi-column list / grid widget.
/// </summary>
public enum GridSelectionMode {
  /// <summary>
  ///   Any single row may be selected. All items in the row are selected.
  /// </summary>
  RowSingle,

  /// <summary>
  ///   Multiple rows may be selected. All items in the row are selected.
  /// </summary>
  RowMultiple,

  /// <summary>
  ///   Any single cell may be selected.
  /// </summary>
  CellSingle,

  /// <summary>
  ///   Multiple cells bay be selected.
  /// </summary>
  CellMultiple,

  /// <summary>
  ///   Any single item in a nominated column may be selected.
  /// </summary>
  NominatedColumnSingle,

  /// <summary>
  ///   Multiple items in a nominated column may be selected.
  /// </summary>
  NominatedColumnMultiple,

  /// <summary>
  ///   Any single column may be selected. All items in the column are selected.
  /// </summary>
  ColumnSingle,

  /// <summary>
  ///   Multiple columns may be selected. All items in the column are selected.
  /// </summary>
  ColumnMultiple,

  /// <summary>
  ///   Any single item in a nominated row may be selected.
  /// </summary>
  NominatedRowSingle,

  /// <summary>
  ///   Multiple items in a nominated row may be selected.
  /// </summary>
  NominatedRowMultiple
}

#endregion Behavior

#region Fonts

/// <summary>
///   Enumerated type that contains valid formatting types that can be specified when
///   rendering text into a Rect area (the formatting Rect).
/// </summary>
enum TextFormatting {
  /// <summary>
  ///   All text is printed on a single line. The left-most character is aligned with
  ///   the left edge of the formatting Rect.
  /// </summary>
  LeftAligned,
  /// <summary>
  ///   All text is printed on a single line. The right-most character is aligned with
  ///   the right edge of the formatting Rect.
  /// </summary>
  RightAligned,
  /// <summary>
  ///   All text is printed on a single line. The text is centred horizontally in
  ///   the formatting Rect.
  /// </summary>
  Centred,
  /// <summary>
  ///   All text is printed on a single line. The left-most and right-most characters
  ///   are aligned with the edges of the formatting Rect.
  /// </summary>
  Justified,
  /// <summary>
  ///   Text is broken into multiple lines no wider than the formatting Rect.
  ///   The left-most character of each line is aligned with the left edge of
  ///   the formatting Rect.
  /// </summary>
  WordWrapLeftAligned,
  /// <summary>
  ///   Text is broken into multiple lines no wider than the formatting Rect.
  ///   The right-most character of each line is aligned with the right edge of
  ///   the formatting Rect.
  /// </summary>
  WordWrapRightAligned,
  /// <summary>
  ///   Text is broken into multiple lines no wider than the formatting Rect.
  ///   Each line is centred horizontally in the formatting Rect.
  /// </summary>
  WordWrapCentred,
  /// <summary>
  ///   Text is broken into multiple lines no wider than the formatting Rect.
  ///   The left-most and right-most characters of each line are aligned with
  ///   the edges of the formatting Rect.
  /// </summary>
  WordWrapJustified
};

/// <summary>
///		Available options for use when creating fonts.
/// </summary>
[Flags]
public enum FontFlags {
  /// <summary>
  ///		No options.
  /// </summary>
  None = 0,
  /// <summary>
  ///		Bold glyphs.
  /// </summary>
  Bold = 1
}

#endregion Fonts

} // namespace CeGui
