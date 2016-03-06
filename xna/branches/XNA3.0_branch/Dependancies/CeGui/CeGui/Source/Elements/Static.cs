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
///		Base class for static widgets.
/// </summary>
[ AlternateWidgetName("Static") ]
public class Static : Window {
  #region Fields

  /// <summary>
  ///		True when the frame is enabled.
  /// </summary>
  protected bool isFrameEnabled;
  /// <summary>
  ///		Used to store frame colors.
  /// </summary>
  protected ColourRect frameColors;
  /// <summary>
  ///		Frame object used for rendering a frame for the static element.
  /// </summary>
  protected RenderableFrame frame = new RenderableFrame();
  /// <summary>
  ///		true when the background is enabled.
  /// </summary>
  protected bool isBackgroundEnabled;
  /// <summary>
  ///		Colors to use when drawing background.
  /// </summary>
  protected ColourRect backgroundColors;
  /// <summary>
  ///		Image to use for widget background.
  /// </summary>
  protected Image backgroundImage;

  /// <summary>
  ///		Width of the left edge image for the current frame.
  /// </summary>
  protected float leftWidth;
  /// <summary>
  ///		Width of the right edge image for the current frame.
  /// </summary>
  protected float rightWidth;
  /// <summary>
  ///		Width of the top edge image for the current frame.
  /// </summary>
  protected float topHeight;
  /// <summary>
  ///		Width of the bottom edge image for the current frame.
  /// </summary>
  protected float bottomHeight;

  #endregion Fields

  #region Constructor

  /// <summary>
  ///		Constructor.
  /// </summary>
  /// <param name="name">Unique name of this widget.</param>
  public Static(string type, string name)
    : base(type, name) {
    Colour white = new Colour(1, 1, 1, 1);

    frameColors = new ColourRect();
    backgroundColors = new ColourRect();
  }

  #endregion Constructor

  #region Base Members

  #region Properties

  /// <summary>
  ///		Gets the ColorRect containing the colors used when rendering this widget.
  /// </summary>
  /// <value>ColorRect initialized with the colors used when rendering the background for this widget.</value>
  [WidgetProperty("BackgroundColours")]
  public ColourRect BackgroundColors {
    get {
      return backgroundColors;
    }
    set {
      SetBackgroundColors(value);
    }
  }

  /// <summary>
  ///		Get/Set whether the background for this static widget is enabled or disabled.
  /// </summary>
  /// <value>
  ///		true if the background is enabled and will be rendered.  
  ///		false if the background is disabled and will not be rendered.
  /// </value>
  [WidgetProperty("BackgroundEnabled")]
  public bool BackgroundEnabled {
    get {
      return isBackgroundEnabled;
    }
    set {
      if(isBackgroundEnabled != value) {
        isBackgroundEnabled = value;
        RequestRedraw();
      }
    }
  }

  /// <summary>
  ///		Gets the ColorRect containing the colors used when rendering this widget.
  /// </summary>
  /// <value>ColorRect initialized with the colors used when rendering the frame for this widget.</value>
  [WidgetProperty("FrameColours")]
  public ColourRect FrameColors {
    get {
      return frameColors;
    }
    set {
      SetFrameColors(value);
    }
  }

  /// <summary>
  ///		Get/Set whether the frame for this static widget is enabled or disabled.
  /// </summary>
  /// <value>
  ///		true if the frame is enabled and will be rendered.  
  ///		false is the frame is disabled and will not be rendered.
  /// </value>
  [WidgetProperty("FrameEnabled")]
  public bool FrameEnabled {
    get {
      return isFrameEnabled;
    }
    set {
      if(isFrameEnabled != value) {
        isFrameEnabled = value;
        OnStaticFrameChanged(new WindowEventArgs(this));
        RequestRedraw();
      }
    }
  }

  /// <summary>
  /// Get/Set the background image
  /// </summary>
  /// <value>Background image</value>
  [WidgetProperty("BackgroundImage")]
  public Image BackgroundImage {
    get {
      return backgroundImage;
    }
    set {
      SetBackgroundImage(value);
    }
  }

  /// <summary>
  /// Get/Set the top left frame image
  /// </summary>
  /// <value>image</value>
  [WidgetProperty("TopLeftFrameImage")]
  public Image TopLeftFrameImage {
    get {
      return frame.TopLeft;
    }
    set {
      frame.TopLeft = value;
      if(FrameEnabled) {
        RequestRedraw();
      }
    }
  }

  /// <summary>
  /// Get/Set the top frame image
  /// </summary>
  /// <value>image</value>
  [WidgetProperty("TopFrameImage")]
  public Image TopFrameImage {
    get {
      return frame.Top;
    }
    set {
      frame.Top = value;
      if(FrameEnabled) {
        RequestRedraw();
      }
    }
  }

  /// <summary>
  /// Get/Set the top right frame image
  /// </summary>
  /// <value>image</value>
  [WidgetProperty("TopRightFrameImage")]
  public Image TopRightFrameImage {
    get {
      return frame.TopRight;
    }
    set {
      frame.TopRight = value;
      if(FrameEnabled) {
        RequestRedraw();
      }
    }
  }

  /// <summary>
  /// Get/Set the left frame image
  /// </summary>
  /// <value>image</value>
  [WidgetProperty("LeftFrameImage")]
  public Image LeftFrameImage {
    get {
      return frame.Left;
    }
    set {
      frame.Left = value;
      if(FrameEnabled) {
        RequestRedraw();
      }
    }
  }

  /// <summary>
  /// Get/Set the right frame image
  /// </summary>
  /// <value>image</value>
  [WidgetProperty("RightFrameImage")]
  public Image RightFrameImage {
    get {
      return frame.Right;
    }
    set {
      frame.Right = value;
      if(FrameEnabled) {
        RequestRedraw();
      }
    }
  }

  /// <summary>
  /// Get/Set the bottom left frame image
  /// </summary>
  /// <value>image</value>
  [WidgetProperty("BottomLeftFrameImage")]
  public Image BottomLeftFrameImage {
    get {
      return frame.BottomLeft;
    }
    set {
      frame.BottomLeft = value;
      if(FrameEnabled) {
        RequestRedraw();
      }
    }
  }

  /// <summary>
  /// Get/Set the bottom frame image
  /// </summary>
  /// <value>image</value>
  [WidgetProperty("BottomFrameImage")]
  public Image BottomFrameImage {
    get {
      return frame.Bottom;
    }
    set {
      frame.Bottom = value;
      if(FrameEnabled) {
        RequestRedraw();
      }
    }
  }

  /// <summary>
  /// Get/Set the bottom right frame image
  /// </summary>
  /// <value>image</value>
  [WidgetProperty("BottomRightFrameImage")]
  public Image BottomRightFrameImage {
    get {
      return frame.BottomRight;
    }
    set {
      frame.BottomRight = value;
      if(FrameEnabled) {
        RequestRedraw();
      }
    }
  }

  #endregion Properties

  #region Methods

  /// <summary>
  ///		Sets the color to be applied when rendering the background.
  /// </summary>
  /// <param name="color">Color value to be used when rendering.</param>
  public void SetBackgroundColors(ColourRect colors) {
    backgroundColors = colors;

    if(isBackgroundEnabled) {
      RequestRedraw();
    }
  }

  /// <summary>
  ///		Sets the colors to be applied when rendering the background.
  /// </summary>
  /// <param name="topLeft">Color to apply to the top-left corner of the background.</param>
  /// <param name="topRight">Color to apply to the top-right corner of the background.</param>
  /// <param name="bottomLeft">Color to apply to the bottom-left corner of the background.</param>
  /// <param name="bottomRight">Color to apply to the bottom-right corner of the background.</param>
  public void SetBackgroundColors(Colour topLeft, Colour topRight, Colour bottomLeft, Colour bottomRight) {
    SetBackgroundColors(new ColourRect(topLeft, topRight, bottomLeft, bottomRight));
  }

  /// <summary>
  ///		Set the image to use as the background for the static widget.
  /// </summary>
  /// <param name="image">
  ///		Reference to the Image object to be rendered.  
  ///		Can be null to specify no image is to be rendered.
  /// </param>
  public void SetBackgroundImage(Image image) {
    backgroundImage = image;

    if(isBackgroundEnabled) {
      RequestRedraw();
    }
  }

  /// <summary>
  ///		Set the image to use as the background for the static widget.
  /// </summary>
  /// <param name="imagesetName">The name of the <see cref="Imageset"/> that holds the required image.</param>
  /// <param name="imageName">Name of the <see cref="Image"/> on the specified <see cref="Imageset"/> that is to be used.</param>
  /// <exception cref="UnknownObjectException">
  ///		Thrown if Imageset <paramref name="imagesetName"/> does not exist in the system or if <paramref name="imagesetName"/> 
  ///		does not contain an Image named <paramref name="imageName"/>.
  /// </exception>
  public void SetBackgroundImage(string imagesetName, string imageName) {
    Imageset imageset = ImagesetManager.Instance.GetImageset(imagesetName);
    SetBackgroundImage(imageset.GetImage(imageName));
  }

  /// <summary>
  ///		Sets the colors to be applied when rendering the frame.
  /// </summary>
  /// <param name="colors">ColorRect object describing the colors to be used.</param>
  public void SetFrameColors(ColourRect colors) {
    frameColors = colors;
    UpdateRenderableFrameColors();

    // redraw only if change would be seen
    if(isFrameEnabled) {
      OnStaticFrameChanged(new WindowEventArgs(this));
      RequestRedraw();
    }
  }

  /// <summary>
  ///		Sets the colors to be applied when rendering the frame.
  /// </summary>
  /// <param name="topLeft">Color to apply to the top-left corner of the frame.</param>
  /// <param name="topRight">Color to apply to the top-right corner of the frame.</param>
  /// <param name="bottomLeft">Color to apply to the bottom-left corner of the frame.</param>
  /// <param name="bottomRight">Color to apply to the bottom-right corner of the frame.</param>
  public void SetFrameColors(Colour topLeft, Colour topRight, Colour bottomLeft, Colour bottomRight) {
    SetFrameColors(new ColourRect(topLeft, topRight, bottomLeft, bottomRight));
  }

  /// <summary>
  ///		specify the Image objects to use for each part of the frame.  A NULL may be used to omit any part.
  /// </summary>
  /// <param name="topLeft">Reference to an Image object to render as the top-left corner of the frame.</param>
  /// <param name="topRight">Reference to an Image object to render as the top-right corner of the frame.</param>
  /// <param name="bottomLeft">Reference to an Image object to render as the bottom-left corner of the frame.</param>
  /// <param name="bottomRight">Reference to an Image object to render as the bottom-right corner of the frame.</param>
  /// <param name="left">Reference to an Image object to render as the left corner of the frame.</param>
  /// <param name="top">Reference to an Image object to render as the top corner of the frame.</param>
  /// <param name="right">Reference to an Image object to render as the right corner of the frame.</param>
  /// <param name="bottom">Reference to an Image object to render as the bottom corner of the frame.</param>
  /// <remarks>
  ///		Specifying null for any of the images will omit that part of the frame.
  /// </remarks>
  public void SetFrameImages(Image topLeft, Image topRight, Image bottomLeft, Image bottomRight,
      Image left, Image top, Image right, Image bottom) {

    // install the new images into the RenderableFrame
    frame.SetImages(topLeft, topRight, bottomLeft, bottomRight, left, top, right, bottom);

    // get sizes of frame edges
    leftWidth = (left != null) ? left.Width : 0.0f;
    rightWidth = (right != null) ? right.Width : 0.0f;
    topHeight = (top != null) ? top.Height : 0.0f;
    bottomHeight = (bottom != null) ? bottom.Height : 0.0f;

    // redraw only if change would be seen
    if(isFrameEnabled) {
      OnStaticFrameChanged(new WindowEventArgs(this));
      RequestRedraw();
    }
  }

  /// <summary>
  ///		Update the internal RenderableFrame with currently set colours and alpha settings.
  /// </summary>
  protected void UpdateRenderableFrameColors() {
    // modifying the alpha directly should be fine since it is only used here
    frameColors.SetAlpha(EffectiveAlpha);

    frame.SetColors(frameColors);
  }

  #endregion Methods

  #endregion Base Members

  #region Window Members

  #region Properties

  /// <summary>
  ///		overridden so derived classes are auto-clipped to within the 
  ///		inner area of the frame when it's active.
  /// </summary>
  public override Rect UnclippedInnerRect {
    get {
      Rect temp = base.UnclippedInnerRect;

      // if frame is enabled, return Rect for area inside frame
      if(isFrameEnabled) {
        temp.Left += leftWidth;
        temp.Right -= rightWidth;
        temp.Top += topHeight;
        temp.Bottom -= bottomHeight;
      }

      return temp;
    }
  }

  #endregion Properties

  #region Methods

  /// <summary>
  ///		Perform the actual rendering for this Window.
  /// </summary>
  /// <param name="z">float value specifying the base Z co-ordinate that should be used when rendering.</param>
  protected override void DrawSelf(float z) {
    Rect clipper = PixelRect;

    // do nothing if the widget is totally clipped.
    if(clipper.IsEmpty) {
      return;
    }

    Rect absRect = UnclippedPixelRect;

    // draw frame
    if(isFrameEnabled) {
      frame.Draw(new Vector3(absRect.Left, absRect.Top, z), clipper);

      // adjust absRect and clipper so that later stages of render to not overwite frame
      absRect.Left += leftWidth;
      absRect.Right -= rightWidth;
      absRect.Top += topHeight;
      absRect.Bottom -= bottomHeight;

      clipper = clipper.GetIntersection(absRect);
    }

    // draw backdrop
    if(isBackgroundEnabled && (backgroundImage != null)) {
      // factor window alpha into colors to use when rendering background
      backgroundColors.SetAlpha(EffectiveAlpha);

      backgroundImage.Draw(absRect, z, clipper, backgroundColors);
    }
  }

  #endregion Methods

  #endregion Window Members

  #region Events

  #region Handlers

  /// <summary>
  ///		This is used internally to indicate that the frame for the static widget has been modified, and as such
  ///		derived classes may need to adjust their layouts or reconfigure their rendering somehow.
  /// </summary>
  /// <param name="e">Event args.</param>
  protected virtual void OnStaticFrameChanged(WindowEventArgs e) {
    // nothing
  }

  #endregion Handlers

  #region Overridden Event Triggers

  /// <summary>
  ///		Handler for when window is sized.
  /// </summary>
  /// <param name="e">Event args.</param>
  protected internal override void OnSized(GuiEventArgs e) {
    // default processing
    base.OnSized(e);

    // update frame SizeF
    frame.Size = AbsoluteSize;

    e.Handled = true;
  }

  /// <summary>
  ///		Handler for when alpha value changes.
  /// </summary>
  /// <param name="e">Event args.</param>
  protected internal override void OnAlphaChanged(GuiEventArgs e) {
    // default processing
    base.OnAlphaChanged(e);

    // update frame colors to use new alpha value
    UpdateRenderableFrameColors();

    e.Handled = true;
  }

  #endregion Overridden Event Triggers

  #endregion Events
}

} // namespace CeGui.Widgets
