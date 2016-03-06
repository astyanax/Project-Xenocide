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

namespace CeGui {

/// <summary>
/// Class that encapsulates information for a frame with background (9 images in total)
///
/// Corner images are always drawn at their natural SizeF, edges are stretched between the corner
/// pieces for a particular edge, the background image will cover the inner Rect formed by
/// the edge images and can be stretched or tiled in either dimension.
/// </summary>
public class FrameComponent : FalagardComponentBase {
  #region Fields
  /// <summary>
  /// Vertical formatting to be applied when rendering the background for the component.
  /// </summary>
  protected VerticalFormatting vertFormatting;
  /// <summary>
  /// Horizontal formatting to be applied when rendering the background for the component.
  /// </summary>
  protected HorizontalFormatting horzFormatting;
  /// <summary>
  /// Array that holds the assigned images.
  /// </summary>
  protected Image[] frameImages;
  #endregion

  #region Constructor
  public FrameComponent() {
    vertFormatting = VerticalFormatting.Stretched;
    horzFormatting = HorizontalFormatting.Stretched;
    frameImages = new Image[(int)FrameImageComponent.FrameImageCount];
  }
  #endregion

  #region Properties
  /// <summary>
  /// Gets/Sets the current vertical formatting setting for this FrameComponent.
  /// </summary>
  public VerticalFormatting BackgroundVerticalFormatting {
    get { return vertFormatting; }
    set { vertFormatting = value; }
  }

  /// <summary>
  /// Gets/Sets the current horizontal formatting setting for this FrameComponent.
  /// </summary>
  public HorizontalFormatting BackgroundHorizontalFormatting {
    get { return horzFormatting; }
    set { horzFormatting = value; }
  }
  #endregion

  #region Methods
  /// <summary>
  /// Return the Image object that will be drawn by this FrameComponent for a specified frame part.
  /// </summary>
  /// <param name="part">One of the FrameImageComponent enumerated values specifying the component image to be accessed.</param>
  /// <returns>Image object</returns>
  public Image GetImage(FrameImageComponent part) {
    return frameImages[(int)part];
  }

  /// <summary>
  /// Set the Image that will be drawn by this ImageryComponent.
  /// </summary>
  /// <param name="part">One of the FrameImageComponent enumerated values specifying the component image to be accessed.</param>
  /// <param name="image">Pointer to the Image object to be drawn by this FrameComponent.</param>
  public void SetImage(FrameImageComponent part, Image image) {
    frameImages[(int)part] = image;
  }

  /// <summary>
  /// Set the Image that will be drawn by this FrameComponent.
  /// </summary>
  /// <param name="part">One of the FrameImageComponent enumerated values specifying the component image to be accessed.</param>
  /// <param name="imageset">String holding the name of the Imagset that contains the Image to be rendered.</param>
  /// <param name="image">String holding the name of the Image to be rendered.</param>
  public void SetImage(FrameImageComponent part, string imageset, string image) {
    try {
      frameImages[(int)part] = ImagesetManager.Instance.GetImageset(imageset).GetImage(image);
    }
    catch(Exception) {
      frameImages[(int)part] = null;
    }
  }
  #endregion

  #region Protected Methods
  protected override void RenderImpl(Window srcWindow, Rect destRect, float base_z, ColourRect modColors, Rect clipper, bool clipToDisplay) {
    throw new NotImplementedException();
  }

  protected void DoBackgroundRender(Window srcWindow, Rect destRect, float base_z, ColourRect colors, Rect clipper, bool clipToDisplay) {
    throw new NotImplementedException();
  }
  #endregion
}

} // namespace CeGui
