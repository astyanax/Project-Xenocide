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
/// Summary description for TLSlider.
/// </summary>
[ AlternateWidgetName("TaharezLook.TLSlider") ]
public class TLSlider : Slider {
  #region Constants

  /// <summary>
  ///		Name of the imageset to use for rendering.
  /// </summary>
  const string ImagesetName = "TaharezLook";
  /// <summary>
  ///		Name of the image to use for rendering the slider container.
  /// </summary>
  const string ContainerImageName = "VertSliderBody";
  /// <summary>
  ///		Type of the thumb to create.
  /// </summary>
  const string ThumbType = "TaharezLook.TLSliderThumb";
  /// <summary>
  ///		Layout constant.
  /// </summary>
  const float ContainerPaddingX = 3;

  #endregion Constants

  #region Fields

  /// <summary>
  ///		Reference to the image to render as the slider container.
  /// </summary>
  protected Image containerImage;

  #endregion Fields

  #region Constructor

  /// <summary>
  ///		Constructor.
  /// </summary>
  /// <param name="name"></param>
  public TLSlider(string type, string name)
    : base(type, name) {
    containerImage = ImagesetManager.Instance.GetImageset(ImagesetName).GetImage(ContainerImageName);
  }

  #endregion Constructor

  #region Slider Methods

  /// <summary>
  ///		Create a Thumb based widget to use as the thumb for this slider.
  /// </summary>
  /// <returns>A reference to a thumb widget for use in this slider.</returns>
  protected override Thumb CreateThumb() {
    Thumb newThumb = (Thumb)WindowManager.Instance.CreateWindow(ThumbType, name + "_auto_Thumb");

    newThumb.Vertical = true;

    // set size for thumb
    float height = ImagesetManager.Instance.GetImageset(ImagesetName).GetImage(TLSliderThumb.NormalImageName).Height;
    height /= containerImage.Height;
    newThumb.Size = new SizeF(1.0f, height);

    return newThumb;
  }

  /// <summary>
  ///		Layout the slider component widgets.
  /// </summary>
  protected override void LayoutComponentWidgets() {
    UpdateThumb();
  }

  /// <summary>
  ///		Return value that best represents current slider value given the current location of the thumb.
  /// </summary>
  /// <returns>float value that, given the thumb widget position, best represents the current value for the slider.</returns>
  protected override float GetValueFromThumb() {
    float posExtent = maxValue;
    float slideExtent = AbsoluteHeight - thumb.AbsoluteHeight;

    return maxValue - (thumb.AbsoluteY / (slideExtent / posExtent));
  }

  /// <summary>
  ///		Given window location <paramref name="point"/>, return a value indicating what change should be 
  ///		made to the slider.
  /// </summary>
  /// <param name="point">Point describing a pixel position in window space.</param>
  /// <returns>
  ///		- -1 to indicate slider should be moved to a lower setting.
  ///		-  0 to indicate slider should not be moved.
  ///		- +1 to indicate slider should be moved to a higher setting.
  /// </returns>
  protected override float GetAdjustDirectionFromPoint(PointF point) {
    Rect absRect = thumb.UnclippedPixelRect;

    if(point.Y < absRect.Top) {
      return 1;
    } else if(point.Y > absRect.Bottom) {
      return -1;
    } else {
      return 0;
    }
  }

  /// <summary>
  ///		Update the size and location of the thumb to properly represent the current state of the slider.
  /// </summary>
  protected override void UpdateThumb() {
    float floatVal = currentValue;
    float posExtent = maxValue;
    float slideExtent = AbsoluteHeight - thumb.AbsoluteHeight;

    thumb.SetVerticalRange(0, AbsoluteToRelativeYImpl(this, slideExtent));
    thumb.Position = new PointF(0, AbsoluteToRelativeYImpl(this, slideExtent - (floatVal * (slideExtent / posExtent))));
  }

  /// <summary>
  ///		Perform the actual rendering for this Slider.
  /// </summary>
  /// <param name="z">float value specifying the base Z co-ordinate that should be used when rendering.</param>
  protected override void DrawSelf(float z) {
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

    // adjust rect so thumb will protrude a little at the sides
    absRect.Left += ContainerPaddingX;
    absRect.Right -= ContainerPaddingX;

    // draw the image
    containerImage.Draw(absRect, z, clipper, colors);
  }

  #endregion Slider Methods
}

} // namespace CeGui.WidgetSets.Taharez
