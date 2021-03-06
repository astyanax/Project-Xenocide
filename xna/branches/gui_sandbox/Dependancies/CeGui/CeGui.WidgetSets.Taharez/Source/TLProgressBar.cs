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
///		Standard progress bar for the Taharez Gui Scheme.
/// </summary>
[ AlternateWidgetName("TaharezLook.TLProgressBar") ]
public class TLProgressBar : ProgressBar {
  #region Constants

  /// <summary>
  ///		Name of the imageset to use for rendering.
  /// </summary>
  const string ImagesetName = "TaharezLook";
  /// <summary>
  ///		Name of the image to use for the left end of the progress container.
  /// </summary>
  const string ContainerLeftImageName = "ProgressBarLeft";
  /// <summary>
  ///		Name of the image to use for the middle of the progress container.
  /// </summary>
  const string ContainerMiddleImageName = "ProgressBarMiddle";
  /// <summary>
  ///		Name of the image to use for the right end of the progress container.
  /// </summary>
  const string ContainerRightImageName = "ProgressBarRight";
  /// <summary>
  ///		Name of the image to use for dim / unlit segments.
  /// </summary>
  const string DimSegmentImageName = "ProgressBarDimSegment";
  /// <summary>
  ///		Name of the image to use for bright / lit segments.
  /// </summary>	
  const string BrightSegmentImageName = "ProgressBarLitSegment";

  /// <summary>
  ///		Value used to calculate required offset for first segment.
  /// </summary>
  const float FirstSegmentOffsetRatioX = 0.28571f;
  /// <summary>
  ///		Value used to calculate amount of overlap required for segments.
  /// </summary>
  const float SegmentOverlapRatio = 0.25f;

  #endregion Constants

  #region Fields

  /// <summary>
  ///		Container left end image.
  /// </summary>
  protected Image left;
  /// <summary>
  ///		Container middle image.
  /// </summary>
  protected Image middle;
  /// <summary>
  ///		Container right end image.
  /// </summary>
  protected Image right;
  /// <summary>
  ///		Dim segment image.
  /// </summary>
  protected Image dimSegment;
  /// <summary>
  ///		Lit segment image.
  /// </summary>
  protected Image litSegment;

  #endregion Fields

  #region Constructor

  /// <summary>
  ///		Constructor.
  /// </summary>
  /// <param name="name"></param>
  public TLProgressBar(string type, string name)
    : base(type, name) {
    // load and store the images to be used
    Imageset imageSet = ImagesetManager.Instance.GetImageset(ImagesetName);

    left = imageSet.GetImage(ContainerLeftImageName);
    middle = imageSet.GetImage(ContainerMiddleImageName);
    right = imageSet.GetImage(ContainerRightImageName);
    dimSegment = imageSet.GetImage(DimSegmentImageName);
    litSegment = imageSet.GetImage(BrightSegmentImageName);
  }

  #endregion Constructor

  #region Window Members

  /// <summary>
  ///		Perform rendering for this widget.
  /// </summary>
  /// <param name="z">Z value to use for rendering.</param>
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

    //
    // Render the container
    //
    float leftWidth = left.Width;
    float rightWidth = right.Width;
    float midWidth = middle.Width;

    // calculate the number of segments in the mid-section
    int segCount = (int)((absRect.Width - leftWidth - rightWidth) / midWidth);
    Vector3 pos = new Vector3(absRect.Left, absRect.Top, z);
    SizeF size = new SizeF(leftWidth, absRect.Height);

    // left end
    left.Draw(pos, size, clipper, colors);
    pos.x += leftWidth;

    // middle segments
    size.Width = midWidth;
    for(int mid = 0; mid < segCount; ++mid) {
      middle.Draw(pos, size, clipper, colors);
      pos.x += midWidth;
    }

    // right end
    size.Width = rightWidth;
    right.Draw(pos, size, clipper, colors);

    //
    // Render the 'lit' portion
    //
    // this increment is because the 'ends' of the container form 1 segment in addition to the middle segments
    segCount++;

    float segWidth = litSegment.Width;
    segWidth -= (segWidth * SegmentOverlapRatio);

    // HACK: added so the lit section doesnt fight with the container, but the C++ version doesn't do it
    // find the source
    pos.z = GuiSystem.Instance.Renderer.GetZLayer(1);

    // construct rect for segment area
    Rect segClipper = new Rect();
    segClipper.Position = new PointF(absRect.Left + (litSegment.Width * FirstSegmentOffsetRatioX), absRect.Top);
    segClipper.Size = new SizeF(segCount * segWidth * progress, absRect.Height);

    // clip the clipper to 'lit area'
    clipper = segClipper.GetIntersection(clipper);

    pos.x = absRect.Left + (litSegment.Width * FirstSegmentOffsetRatioX);

    size.Width = segWidth;

    for(int seg = 0; seg < segCount; ++seg) {
      litSegment.Draw(pos, size, clipper, colors);
      pos.x += segWidth;
    }
  }

  #endregion Window Members
}

} // namespace CeGui.WidgetSets.Taharez
