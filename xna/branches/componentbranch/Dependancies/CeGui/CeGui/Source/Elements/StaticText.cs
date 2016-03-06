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

namespace CeGui.Widgets {

/// <summary>
///		Base class for a static text widget.
/// </summary>
/// <remarks>
///		This base class performs it's own rendering.  There is no need to override this widget to perform rendering
///		of static texts.
/// </remarks>
[ AlternateWidgetName("StaticText") ]
public class StaticText : Static {
  #region Fields

  /// <summary>
  ///		Horizontal text formatting.
  /// </summary>
  protected HorizontalTextFormat horzFormatting;
  /// <summary>
  ///		Vertical text formatting.
  /// </summary>
  protected VerticalTextFormat vertFormatting;
  /// <summary>
  ///		Colors to use for rendering the text of this widget.
  /// </summary>
  protected ColourRect textColors = new ColourRect();

  #endregion Fields

  #region Constructor

  /// <summary>
  ///		Constructor.
  /// </summary>
  /// <param name="name">Name of this widget.</param>
  public StaticText(string type, string name)
    : base(type, name) {
    horzFormatting = HorizontalTextFormat.Left;
    vertFormatting = VerticalTextFormat.Centered;
  }

  #endregion Constructor

  #region Base Members

  #region Properties

  /// <summary>
  ///		Get/Set the horizontal format to use for the text.
  /// </summary>
  /// <value>Enum value specifying the horizontal formatting.</value>
  [HorizontalTextFormatProperty("HorzFormatting")]
  public HorizontalTextFormat HorizontalFormat {
    get {
      return horzFormatting;
    }
    set {
      horzFormatting = value;
      RequestRedraw();
    }
  }

  /// <summary>
  ///		Get/Set the vertical format to use for the text.
  /// </summary>
  /// <value>Enum value specifying the vertical formatting.</value>
  [VerticalTextFormatProperty("VertFormatting")]
  public VerticalTextFormat VerticalFormat {
    get {
      return vertFormatting;
    }
    set {
      vertFormatting = value;
      RequestRedraw();
    }
  }

  /// <summary>
  /// Get/Set the text colors.
  /// </summary>
  /// <value>ColorRect specifying the colors to use when drawing the text.</value>
  [WidgetProperty("TextColours")]
  public ColourRect TextColors {
    get {
      return textColors;
    }
    set {
      textColors = value;
      RequestRedraw();
    }
  }

  #endregion Properties

  #region Methods

  /// <summary>
  ///		Sets the color to be applied when rendering the text.
  /// </summary>
  /// <param name="color">Color to use for text rendering.</param>
  public void SetTextColor(Colour color) {
    SetTextColor(color, color, color, color);
  }

  /// <summary>
  ///		Sets the colors to be applied when rendering the text.
  /// </summary>
  /// <param name="colors">ColorRect describing the colours to be used for each text glyph rendered.</param>
  public void SetTextColor(ColourRect colors) {
    textColors = colors;
    RequestRedraw();
  }

  /// <summary>
  ///		Sets the colors to be applied when rendering the text.
  /// </summary>
  /// <param name="topLeft">Color for the top-left cornder of each text glyph rendered.</param>
  /// <param name="topRight">Color for the top-right cornder of each text glyph rendered.</param>
  /// <param name="bottomLeft">Color for the bottom-left cornder of each text glyph rendered.</param>
  /// <param name="bottomRight">Color for the bottom-right cornder of each text glyph rendered.</param>
  public void SetTextColor(Colour topLeft, Colour topRight, Colour bottomLeft, Colour bottomRight) {
    textColors.topLeft = topLeft;
    textColors.topRight = topRight;
    textColors.bottomLeft = bottomLeft;
    textColors.bottomRight = bottomRight;

    RequestRedraw();
  }

  #endregion Methods

  #endregion Base Members

  #region Window Members

  /// <summary>
  ///		Perform the actual rendering for this Window.
  /// </summary>
  /// <param name="z">float value specifying the base Z co-ordinate that should be used when rendering.</param>
  protected override void DrawSelf(float z) {
    // render what base class needs to render first
    base.DrawSelf(z);

    // render text

    Rect absRect = UnclippedInnerRect;
    Rect clipper = absRect.GetIntersection(PixelRect);

    Font textFont = this.Font;

    // get total pixel height of the text based on its format
    float textHeight = textFont.GetFormattedLineCount(text, absRect, horzFormatting) * textFont.LineSpacing;

    // adjust y positioning according to formatting options
    switch(vertFormatting) {
      case VerticalTextFormat.Centered:
        absRect.Top += ((absRect.Height - textHeight) * 0.5f);
        break;

      case VerticalTextFormat.Bottom:
        absRect.Top = absRect.Bottom - textHeight;
        break;
    }

    textColors.SetAlpha(EffectiveAlpha);

    textFont.DrawText(
        Text,
        absRect,
        GuiSystem.Instance.Renderer.GetZLayer(1),
        clipper,
        horzFormatting,
        textColors);
  }

  #endregion Window Members
}

} // namespace CeGui.Widgets
