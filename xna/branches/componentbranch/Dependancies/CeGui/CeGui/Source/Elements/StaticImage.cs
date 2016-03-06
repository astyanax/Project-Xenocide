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
///		Static image widget class.
/// </summary>
/// <remarks>
///		This base class performs it's own rendering.  There is no need to override this widget to perform rendering
///		of static images.
/// </remarks>
[ AlternateWidgetName("StaticImage") ]
public class StaticImage : Static {
  #region Fields

  /// <summary>
  ///		RenderableImage that does most of the work for us.
  /// </summary>
  protected RenderableImage image = new RenderableImage();
  /// <summary>
  ///		Colors to use for the image.
  /// </summary>
  protected ColourRect imageColors = new ColourRect();

  #endregion Fields

  #region Constructor

  /// <summary>
  ///		Constructor.
  /// </summary>
  /// <param name="name">Name of this widget.</param>
  public StaticImage(string type, string name)
    : base(type, name) {
    Colour white = new Colour(1, 1, 1, 1);

    imageColors = new ColourRect(white, white, white, white);

    // default to stretched image
    image.HorizontalFormat = HorizontalImageFormat.Stretched;
    image.VerticalFormat = VerticalImageFormat.Stretched;
  }

  #endregion Constructor

  #region Members

  #region Properties

  /// <summary>
  ///		Return a reference to the current image displayed by this widget.
  /// </summary>
  [WidgetProperty("Image")]
  public Image Image {
    set {
      SetImage(value);
    }
    get {
      return image.Image;
    }
  }

  /// <summary>
  ///		Set the formatting required for the image.
  /// </summary>
  /// <value>One of the <see cref="HorizontalImageFormat"/> enumerated values specifying the formatting required.</value>
  [HorzizontalImageFormatProperty("HorzFormatting")]
  public HorizontalImageFormat HorizontalFormat {
    get {
      return image.HorizontalFormat;
    }
    set {
      image.HorizontalFormat = value;
      RequestRedraw();
    }
  }

  /// <summary>
  ///		Set the formatting required for the image.
  /// </summary>
  /// <value>One of the <see cref="VerticalImageFormat"/> enumerated values specifying the formatting required.</value>
  [VerticalImageFormatProperty("VertFormatting")]
  public VerticalImageFormat VerticalFormat {
    get {
      return image.VerticalFormat;
    }
    set {
      image.VerticalFormat = value;
      RequestRedraw();
    }
  }

  #endregion Properties

  #region Methods

  /// <summary>
  ///		Set the <see cref="Image"/> object to be drawn by this widget.
  /// </summary>
  /// <param name="image">
  ///		Reference to the <see cref="Image"/> object to be rendered.  
  ///		Can be 'null' to specify no image is to be rendered.
  ///	</param>
  public void SetImage(Image image) {
    this.image.Image = image;
    RequestRedraw();
  }

  /// <summary>
  ///		Set the <see cref="Image"/> object to be drawn by this widget.
  /// </summary>
  /// <param name="imagesetName">Imageset that the image is contained in.</param>
  /// <param name="imageName">Name of the image within the specified imageset.</param>
  public void SetImage(string imagesetName, string imageName) {
    SetImage(ImagesetManager.Instance.GetImageset(imagesetName).GetImage(imageName));
  }

  /// <summary>
  ///		
  /// </summary>
  /// <param name="colors"></param>
  public void SetImageColors(ColourRect colors) {
    imageColors = colors;
    UpdateRenderableImageColors();
    RequestRedraw();
  }

  /// <summary>
  ///		
  /// </summary>
  /// <param name="topLeft"></param>
  /// <param name="topRight"></param>
  /// <param name="bottomLeft"></param>
  /// <param name="bottomRight"></param>
  public void SetImageColors(Colour topLeft, Colour topRight, Colour bottomLeft, Colour bottomRight) {
    imageColors.topLeft = topLeft;
    imageColors.topRight = topRight;
    imageColors.bottomLeft = bottomLeft;
    imageColors.bottomRight = bottomRight;

    UpdateRenderableImageColors();

    RequestRedraw();
  }

  /// <summary>
  ///		Update the internal RenderableImaeg with currently set colors and alpha settings.
  /// </summary>
  protected void UpdateRenderableImageColors() {
    float alpha = EffectiveAlpha;

    ColourRect colors = image.Colors;
    colors.topLeft = imageColors.topLeft;
    colors.topRight = imageColors.topRight;
    colors.bottomLeft = imageColors.bottomLeft;
    colors.bottomRight = imageColors.bottomRight;
    colors.SetAlpha(alpha);
  }

  #endregion Methods

  #endregion Members

  #region Window Members

  /// <summary>
  ///		Perform the actual rendering for this Window.
  /// </summary>
  /// <param name="z">float value specifying the base Z co-ordinate that should be used when rendering.</param>
  protected override void DrawSelf(float z) {
    // do base class rendering first
    base.DrawSelf(z);

    // render the image
    Rect area = UnclippedInnerRect;
    image.Draw(new Vector3(area.Left, area.Top, z), PixelRect.GetIntersection(area));
  }


  #endregion Window Members

  #region Events

  #region Overridden Event Trigger Methods

  /// <summary>
  /// 
  /// </summary>
  /// <param name="e"></param>
  protected internal override void OnSized(GuiEventArgs e) {
    // base class handling
    base.OnSized(e);

    image.Size = UnclippedInnerRect.Size;

    e.Handled = true;
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="e"></param>
  protected internal override void OnAlphaChanged(GuiEventArgs e) {
    // base class handling
    base.OnAlphaChanged(e);

    UpdateRenderableImageColors();

    e.Handled = true;
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="e"></param>
  protected override void OnStaticFrameChanged(WindowEventArgs e) {
    // base class processing
    base.OnStaticFrameChanged(e);

    // update the SizeF of the image to reflect changes made to the frame in the base class
    image.Size = UnclippedInnerRect.Size;

    e.Handled = true;
  }

  #endregion Overridden Event Trigger Methods

  #endregion Events
}

} // namespace CeGui.Widgets
