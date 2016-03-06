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
/// Class that encapsulates information for a single image component.
/// </summary>
public class ImageryComponent : FalagardComponentBase {
  #region Fields
  /// <summary>
  /// Image to be drawn by this image component.
  /// </summary>
  protected Image image;
  /// <summary>
  /// Vertical formatting to be applied when rendering the image component.
  /// </summary>
  protected VerticalFormatting vertFormatting;
  /// <summary>
  /// Horizontal formatting to be applied when rendering the image component.
  /// </summary>
  protected HorizontalFormatting horzFormatting;
  /// <summary>
  /// Name of the property to access to obtain the image to be used.
  /// </summary>
  protected string imagePropertyName;
  #endregion

  #region Constructors
  /// <summary>
  /// Class that encapsulates information for a single image component.
  /// </summary>
  public ImageryComponent() {
    image = null;
    vertFormatting = VerticalFormatting.TopAligned;
    horzFormatting = HorizontalFormatting.LeftAligned;
  }
  #endregion

  #region Properties
  /// <summary>
  /// Gets/Sets the current vertical formatting setting for this ImageryComponent.
  /// </summary>
  public VerticalFormatting VerticalFormatting {
    get { return vertFormatting; }
    set { vertFormatting = value; }
  }

  /// <summary>
  /// Gets/Sets the current horizontal formatting setting for this ImageryComponent.
  /// </summary>
  public HorizontalFormatting HorizontalFormatting {
    get { return horzFormatting; }
    set { horzFormatting = value; }
  }

  /// <summary>
  /// Gets whether this ImageryComponent fetches it's image via a property on the target window.
  /// </summary>
  public bool IsImagePropertyFetchedFromProperty {
    get { return !(imagePropertyName.Length > 0); }
  }

  /// <summary>
  /// Return the name of the property that will be used to determine the image for this ImageryComponent.
  /// </summary>
  public string ImagePropertySource {
    get { return imagePropertyName; }
    set { imagePropertyName = value; }
  }
  #endregion

  #region Methods
  /// <summary>
  /// Return the Image object that will be drawn by this ImageryComponent.
  /// </summary>
  /// <returns></returns>
  public Image GetImage() {
    return image;
  }

  /// <summary>
  /// Set the Image that will be drawn by this ImageryComponent.
  /// </summary>
  /// <param name="image">The Image object to be drawn by this ImageryComponent.</param>
  public void SetImage(Image image) {
    this.image = image;
  }

  /// <summary>
  /// Set the Image that will be drawn by this ImageryComponent.
  /// </summary>
  /// <param name="imageset">String holding the name of the Imagset that contains the Image to be rendered.</param>
  /// <param name="image">String holding the name of the Image to be rendered.</param>
  public void SetImage(string imageset, string image) {
    this.image = ImagesetManager.Instance.GetImageset(imageset).GetImage(image);
  }
  #endregion

  #region Protected Methods
  protected override void RenderImpl(Window srcWindow, Rect destRect, float base_z, ColourRect modColours, Rect clipper, bool clipToDisplay) {
    throw new NotImplementedException();
  }
  #endregion
}

} // namespace CeGui
