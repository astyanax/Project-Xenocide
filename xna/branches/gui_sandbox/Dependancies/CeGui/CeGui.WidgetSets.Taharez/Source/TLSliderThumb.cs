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
using CeGui;
using CeGui.Widgets;

namespace CeGui.WidgetSets.Taharez {

/// <summary>
/// Summary description for TLSliderThumb.
/// </summary>
[ AlternateWidgetName("TaharezLook.TLSliderThumb") ]
public class TLSliderThumb : Thumb {
  #region Constants

  /// <summary>
  ///		Name of the imageset to use for rendering.
  /// </summary>
  public const string ImagesetName = "TaharezLook";
  /// <summary>
  ///		Name of the image to use for normal rendering.
  /// </summary>
  public const string NormalImageName = "VertSliderThumbNormal";
  /// <summary>
  ///		Name of the image to use for hover / highlighted rendering.
  /// </summary>
  public const string HighlightImageName = "VertSliderThumbHover";

  #endregion Constants

  #region Fields

  /// <summary>
  ///		Image to render in normal state.
  /// </summary>
  protected Image normalImage;
  /// <summary>
  ///		Image to render in highlighted state.
  /// </summary>
  protected Image highlightImage;

  #endregion Fields

  #region Constructor

  /// <summary>
  ///		Constructor.
  /// </summary>
  /// <param name="name"></param>
  public TLSliderThumb(string type, string name)
    : base(type, name) {
    // load the images for the set
    Imageset imageSet = ImagesetManager.Instance.GetImageset(ImagesetName);

    normalImage = imageSet.GetImage(NormalImageName);
    highlightImage = imageSet.GetImage(HighlightImageName);
  }

  #endregion Constructor

  #region PushButton Members

  /// <summary>
  ///		Render thumb in the normal state.
  /// </summary>
  /// <param name="z">Z value for rendering.</param>
  protected override void DrawNormal(float z) {
    Rect clipper = PixelRect;

    // do nothing if the widget is totally clipped.
    if(clipper.Width == 0) {
      return;
    }

    // get the destination screen rect for this window
    Rect absRect = UnclippedPixelRect;

    // calculate colours to use.
    Colour colorVal = new Colour(1, 1, 1, EffectiveAlpha);
    ColourRect colors = new ColourRect(colorVal, colorVal, colorVal, colorVal);

    // draw the image
    normalImage.Draw(absRect, z, clipper, colors);
  }

  /// <summary>
  ///		Render thumb in the hover state.
  /// </summary>
  /// <param name="z">Z value for rendering.</param>
  protected override void DrawHover(float z) {
    Rect clipper = PixelRect;

    // do nothing if the widget is totally clipped.
    if(clipper.Width == 0) {
      return;
    }

    // get the destination screen rect for this window
    Rect absRect = UnclippedPixelRect;

    // calculate colours to use.
    Colour colorVal = new Colour(1, 1, 1, EffectiveAlpha);
    ColourRect colors = new ColourRect(colorVal, colorVal, colorVal, colorVal);

    // draw the image
    highlightImage.Draw(absRect, z, clipper, colors);
  }

  /// <summary>
  ///		Render thumb in the pushed state.
  /// </summary>
  /// <param name="z">Z value for rendering.</param>
  protected override void DrawPushed(float z) {
    DrawHover(z);
  }

  #endregion PushButton Members
}

} // namespace CeGui.WidgetSets.Taharez
