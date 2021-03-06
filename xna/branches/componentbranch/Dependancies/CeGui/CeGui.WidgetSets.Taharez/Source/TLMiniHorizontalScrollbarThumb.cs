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
/// Widget used as the thumb for the mini horizontal scrollbar in the Taharez Gui Scheme.
/// </summary>
[ AlternateWidgetName("TaharezLook.TLMiniHorizontalScrollbarThumb") ]
public class TLMiniHorizontalScrollbarThumb : Thumb {
  #region Constants
  /// <summary>
  ///		Name of the imageset to use for rendering.
  /// </summary>
  public const string ImagesetName = "TaharezLook";

  /// <summary>
  ///		Name of the image to use for normal rendering (used for sizes only)..
  /// </summary>
  public const string NormalImageName = "MiniHorzScrollThumbNormal";

  /// <summary>
  ///		Name of the image to use for normal rendering (top end).
  /// </summary>
  public const string NormalLeftImageName = "MiniHorzScrollThumbLeftNormal";

  /// <summary>
  ///		Name of the image to use for normal rendering (mid section).
  /// </summary>
  public const string NormalMiddleImageName = "MiniHorzScrollThumbMiddleNormal";

  /// <summary>
  ///		Name of the image to use for normal rendering (bottom end).
  /// </summary>
  public const string NormalRightImageName = "MiniHorzScrollThumbRightNormal";

  /// <summary>
  ///		Name of the image to use for hover / highlighted rendering (top end).
  /// </summary>
  public const string HighlightLeftImageName = "MiniHorzScrollThumbLeftHover";

  /// <summary>
  ///		Name of the image to use for hover / highlighted rendering (mid section).
  /// </summary>
  public const string HighlightMiddleImageName = "MiniHorzScrollThumbMiddleHover";

  /// <summary>
  ///		Name of the image to use for hover / highlighted rendering (bottom end).
  /// </summary>
  public const string HighlightRightImageName = "MiniHorzScrollThumbRightHover";

  #endregion

  #region Fields

  /// <summary>
  ///		Image to render in normal state (used to get sizes only)
  /// </summary>
  protected Image normalImage;

  /// <summary>
  ///		Image to render in normal state (left end)
  /// </summary>
  protected Image normalLeftImage;

  /// <summary>
  ///		Image to render in normal state (mid section)
  /// </summary>
  protected Image normalMiddleImage;

  /// <summary>
  ///		Image to render in normal state (right end)
  /// </summary>
  protected Image normalRightImage;

  /// <summary>
  ///		Image to render in highlighted state (left end).
  /// </summary>
  protected Image highlightLeftImage;

  /// <summary>
  ///		Image to render in highlighted state (mid section).
  /// </summary>
  protected Image highlightMiddleImage;

  /// <summary>
  ///		Image to render in highlighted state (right end).
  /// </summary>
  protected Image highlightRightImage;

  #endregion

  #region Constructor
  /// <summary>
  ///		Constructor.
  /// </summary>
  /// <param name="name"></param>
  public TLMiniHorizontalScrollbarThumb(string type, string name)
    : base(type, name) {
    // load the images for the set
    Imageset imageSet = ImagesetManager.Instance.GetImageset(ImagesetName);

    normalImage = imageSet.GetImage(NormalImageName);
    normalLeftImage = imageSet.GetImage(NormalLeftImageName);
    normalMiddleImage = imageSet.GetImage(NormalMiddleImageName);
    normalRightImage = imageSet.GetImage(NormalRightImageName);
    highlightLeftImage = imageSet.GetImage(HighlightLeftImageName);
    highlightMiddleImage = imageSet.GetImage(HighlightMiddleImageName);
    highlightRightImage = imageSet.GetImage(HighlightRightImageName);
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
    float minWidth = absRect.Width * 0.5f;
    float leftWidth = Math.Min(normalLeftImage.Width, minWidth);
    float rightWidth = Math.Min(normalRightImage.Width, minWidth);
    float middleWidth = absRect.Width - leftWidth - rightWidth;

    // draw the images
    Vector3 pos = new Vector3(absRect.Left, absRect.Top, z);
    SizeF sz = new SizeF(leftWidth, absRect.Height);
    normalLeftImage.Draw(pos, sz, clipper, colors);

    pos.x += sz.Width;
    sz.Width = middleWidth;
    normalMiddleImage.Draw(pos, sz, clipper, colors);

    pos.x += sz.Width;
    sz.Width = rightWidth;
    normalRightImage.Draw(pos, sz, clipper, colors);
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
    float minWidth = absRect.Width * 0.5f;
    float leftWidth = Math.Min(highlightLeftImage.Width, minWidth);
    float rightWidth = Math.Min(highlightRightImage.Width, minWidth);
    float middleWidth = absRect.Width - leftWidth - rightWidth;

    // draw the images
    Vector3 pos = new Vector3(absRect.Left, absRect.Top, z);
    SizeF sz = new SizeF(leftWidth, absRect.Height);
    highlightLeftImage.Draw(pos, sz, clipper, colors);

    pos.x += sz.Width;
    sz.Width = middleWidth;
    highlightMiddleImage.Draw(pos, sz, clipper, colors);

    pos.x += sz.Width;
    sz.Width = rightWidth;
    highlightRightImage.Draw(pos, sz, clipper, colors);
  }

  #endregion

  #region Window Members
  /// <summary>
  /// 
  /// </summary>
  /// <param name="e"></param>
  protected override void OnSized(GuiEventArgs e) {
    // calculate preferred width from height(which is known).
    float prefWidth = normalImage.Width * (AbsoluteHeight / normalImage.Height);

    // Only proceed if parent is not null
    if(parent != null) {
      // calculate scaled width.
      // TODO: Magic number removal?
      float scaledWidth = (parent.AbsoluteWidth - (2 * parent.AbsoluteHeight)) * 0.575f;

      // use preferred width if there is room, else use the scaled width.
      if(scaledWidth < prefWidth) {
        prefWidth = scaledWidth;
      }
    }

    // install new width values.
    absArea.Width = prefWidth;
    relArea.Width = AbsoluteToRelativeXImpl(parent, prefWidth);

    // base class processing.
    base.OnSized(e);

    e.Handled = true;
  }

  #endregion

}

} // namespace CeGui.WidgetSets.Taharez