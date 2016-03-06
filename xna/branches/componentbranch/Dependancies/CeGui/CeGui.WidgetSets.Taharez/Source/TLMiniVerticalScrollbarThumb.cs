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
using CeGui;
using CeGui.Widgets;

namespace CeGui.WidgetSets.Taharez {

/// <summary>
/// Widget used as the thumb for the mini vertical scrollbar in the Taharez Gui Scheme.
/// </summary>
[ AlternateWidgetName("TaharezLook.TLMiniVerticalScrollbarThumb") ]
public class TLMiniVerticalScrollbarThumb : Thumb {
  #region Constants
  /// <summary>
  ///		Name of the imageset to use for rendering.
  /// </summary>
  public const string ImagesetName = "TaharezLook";

  /// <summary>
  ///		Name of the image to use for normal rendering (used for sizes only)..
  /// </summary>
  public const string NormalImageName = "MiniVertScrollThumbNormal";

  /// <summary>
  ///		Name of the image to use for normal rendering (top end).
  /// </summary>
  public const string NormalTopImageName = "MiniVertScrollThumbTopNormal";

  /// <summary>
  ///		Name of the image to use for normal rendering (mid section).
  /// </summary>
  public const string NormalMiddleImageName = "MiniVertScrollThumbMiddleNormal";

  /// <summary>
  ///		Name of the image to use for normal rendering (bottom end).
  /// </summary>
  public const string NormalBottomImageName = "MiniVertScrollThumbBottomNormal";

  /// <summary>
  ///		Name of the image to use for hover / highlighted rendering (top end).
  /// </summary>
  public const string HighlightTopImageName = "MiniVertScrollThumbTopHover";

  /// <summary>
  ///		Name of the image to use for hover / highlighted rendering (mid section).
  /// </summary>
  public const string HighlightMiddleImageName = "MiniVertScrollThumbMiddleHover";

  /// <summary>
  ///		Name of the image to use for hover / highlighted rendering (bottom end).
  /// </summary>
  public const string HighlightBottomImageName = "MiniVertScrollThumbBottomHover";

  #endregion

  #region Fields

  /// <summary>
  ///		Image to render in normal state (used to get sizes only)
  /// </summary>
  protected Image normalImage;

  /// <summary>
  ///		Image to render in normal state (top end)
  /// </summary>
  protected Image normalTopImage;

  /// <summary>
  ///		Image to render in normal state (mid section)
  /// </summary>
  protected Image normalMiddleImage;

  /// <summary>
  ///		Image to render in normal state (bottom end)
  /// </summary>
  protected Image normalBottomImage;

  /// <summary>
  ///		Image to render in highlighted state (top end).
  /// </summary>
  protected Image highlightTopImage;

  /// <summary>
  ///		Image to render in highlighted state (mid section).
  /// </summary>
  protected Image highlightMiddleImage;

  /// <summary>
  ///		Image to render in highlighted state (bottom end).
  /// </summary>
  protected Image highlightBottomImage;

  #endregion

  #region Constructor
  /// <summary>
  ///		Constructor.
  /// </summary>
  /// <param name="name"></param>
  public TLMiniVerticalScrollbarThumb(string type, string name)
    : base(type, name) {
    // load the images for the set
    Imageset imageSet = ImagesetManager.Instance.GetImageset(ImagesetName);

    normalImage = imageSet.GetImage(NormalImageName);
    normalTopImage = imageSet.GetImage(NormalTopImageName);
    normalMiddleImage = imageSet.GetImage(NormalMiddleImageName);
    normalBottomImage = imageSet.GetImage(NormalBottomImageName);
    highlightTopImage = imageSet.GetImage(HighlightTopImageName);
    highlightMiddleImage = imageSet.GetImage(HighlightMiddleImageName);
    highlightBottomImage = imageSet.GetImage(HighlightBottomImageName);
  }
  #endregion

  #region PushButton Members
  /// <summary>
  ///		Render thumb in the normal state.
  /// </summary>
  /// <param name="z">Z value for rendering.</param>
  protected override void DrawNormal(float z) {
    Rect clipper = PixelRect;

    // do nothing if totally clipped
    if(clipper.Width == 0) {
      return;
    }

    // get the destination screen area for this widget
    Rect absRect = UnclippedPixelRect;

    // calculate colours to use.
    Colour colorVal = new Colour(1, 1, 1, EffectiveAlpha);
    ColourRect colors = new ColourRect(colorVal, colorVal, colorVal, colorVal);

    // calculate segment sizes
    float minHeight = absRect.Height * 0.5f;
    float topHeight = Math.Min(normalTopImage.Height, minHeight);
    float bottomHeight = Math.Min(normalBottomImage.Height, minHeight);
    float middleHeight = absRect.Height - topHeight - bottomHeight;

    // draw the images
    Vector3 pos = new Vector3(absRect.Left, absRect.Top, z);
    SizeF sz = new SizeF(absRect.Width, topHeight);
    normalTopImage.Draw(pos, sz, clipper, colors);

    pos.y += sz.Height;
    sz.Height = middleHeight;
    normalMiddleImage.Draw(pos, sz, clipper, colors);

    pos.y += sz.Height;
    sz.Height = bottomHeight;
    normalBottomImage.Draw(pos, sz, clipper, colors);
  }

  /// <summary>
  ///		Render thumb in the normal state.
  /// </summary>
  /// <param name="z">Z value for rendering.</param>
  protected override void DrawHover(float z) {
    Rect clipper = PixelRect;

    // do nothing if totally clipped
    if(clipper.Width == 0) {
      return;
    }

    // get the destination screen area for this widget
    Rect absRect = UnclippedPixelRect;

    // calculate colours to use.
    Colour colorVal = new Colour(1, 1, 1, EffectiveAlpha);
    ColourRect colors = new ColourRect(colorVal, colorVal, colorVal, colorVal);

    // calculate segment sizes
    float minHeight = absRect.Height * 0.5f;
    float topHeight = Math.Min(highlightTopImage.Height, minHeight);
    float bottomHeight = Math.Min(highlightBottomImage.Height, minHeight);
    float middleHeight = absRect.Height - topHeight - bottomHeight;

    // draw the images
    Vector3 pos = new Vector3(absRect.Left, absRect.Top, z);
    SizeF sz = new SizeF(absRect.Width, topHeight);
    highlightTopImage.Draw(pos, sz, clipper, colors);

    pos.y += sz.Height;
    sz.Height = middleHeight;
    highlightMiddleImage.Draw(pos, sz, clipper, colors);

    pos.y += sz.Height;
    sz.Height = bottomHeight;
    highlightBottomImage.Draw(pos, sz, clipper, colors);
  }

  #endregion

  #region Window Members
  /// <summary>
  /// 
  /// </summary>
  /// <param name="e"></param>
  protected override void OnSized(GuiEventArgs e) {
    // calculate preferred height from width (which is known).
    float prefHeight = normalImage.Height * (AbsoluteWidth / normalImage.Width);

    // Only proceed if parent is not null
    if(parent != null) {
      // calculate scaled height.
      // TODO: Magic number removal?
      float scaledHeight = (parent.AbsoluteHeight - (2 * parent.AbsoluteWidth)) * 0.575f;

      // use preferred height if there is room, else use the scaled height.
      if(scaledHeight < prefHeight) {
        prefHeight = scaledHeight;
      }
    }

    // install new height values.
    absArea.Height = prefHeight;
    relArea.Height = AbsoluteToRelativeYImpl(parent, prefHeight);

    // base class processing.
    base.OnSized(e);

    e.Handled = true;
  }

  #endregion

}

} // namespace CeGui.WidgetSets.Taharez