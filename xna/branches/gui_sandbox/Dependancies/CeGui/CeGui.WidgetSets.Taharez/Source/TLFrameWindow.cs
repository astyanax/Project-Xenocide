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

/// <summary>A frame window using the 'Taharez' look</summary>
[ AlternateWidgetName("TaharezLook.TLFrameWindow") ]
public class TLFrameWindow : FrameWindow {
  #region Constants

  /// <summary>Name of the imageset to use for rendering</summary>
  const string ImagesetName = "TaharezLook";

  /// <summary>The image for the top left corner of the window</summary>
  const string TopLeftFrameImageName = "WindowTopLeft";
  /// <summary>The image for the top right corner of the window</summary>
  const string TopRightFrameImageName = "WindowTopRight";
  /// <summary>The image for the bottom left corner of the window</summary>
  const string BottomLeftFrameImageName = "WindowBottomLeft";
  /// <summary>The image for the bottom right corner of the window</summary>
  const string BottomRightFrameImageName = "WindowBottomRight";
  /// <summary>The image for the left side of the window</summary>
  const string LeftFrameImageName = "WindowLeftEdge";
  /// <summary>The image for the right side of the window</summary>
  const string RightFrameImageName = "WindowRightEdge";
  /// <summary>The image for the top side of the window</summary>
  const string TopFrameImageName = "WindowTopEdge";
  /// <summary>The image for the bottom side of the window</summary>
  const string BottomFrameImageName = "WindowBottomEdge";
  const string ClientBrushImageName = "ClientBrush";

  const string CloseButtonNormalImageName = "NewCloseButtonNormal";
  const string CloseButtonHoverImageName = "NewCloseButtonHover";
  const string CloseButtonPushedImageName = "NewCloseButtonPressed";

  // cursor images
  const string NormalCursorImageName = "MouseArrow";
  const string NorthSouthCursorImageName = "MouseNoSoCursor";
  const string EastWestCursorImageName = "MouseEsWeCursor";
  const string NWestSEastCursorImageName = "MouseNwSeCursor";
  const string NEastSWestCursorImageName = "MouseNeSwCursor";

  // window type stuff
  const string TitlebarType = "Taharez Titlebar";
  const string CloseButtonType = "Taharez Close Button";

  // layout constants
  const float TitlebarXOffset = 0;
  const float TitlebarYOffset = 0;
  const float TitlebarTextPadding = 8;
  const float TitlebarWidthPercentage = 66;

  // colors
  const uint CloseButtonNormalColor = 0xFFBBBBBB;
  const uint CloseButtonHoverColor = 0xFFFFFFFF;
  const uint CloseButtonPushedColor = 0xFF999999;

  #endregion Constants

  #region Fields

  /// <summary>
  ///		Handles the frame for the window.
  /// </summary>
  protected RenderableFrame frame = new RenderableFrame();
  /// <summary>
  ///		Handles the client clearing brush for the window.
  /// </summary>
  protected RenderableImage clientBrush = new RenderableImage();

  /// <summary>
  ///		Width of the left frame edge in pixels.
  /// </summary>
  protected float frameLeftSize;
  /// <summary>
  ///		Width of the right frame edge in pixels.
  /// </summary>
  protected float frameRightSize;
  /// <summary>
  ///		Height of the top frame edge in pixels.
  /// </summary>
  protected float frameTopSize;
  /// <summary>
  ///		Height of the bottom frame edge in pixels.
  /// </summary>
  protected float frameBottomSize;

  #endregion Fields

  #region Constructor

  /// <summary>
  ///		Constructor.
  /// </summary>
  /// <param name="name"></param>
  public TLFrameWindow(string type, string name)
    : base(type, name) {
    Imageset imageSet = ImagesetManager.Instance.GetImageset(ImagesetName);

    // setup frame images
    frame.SetImages(
        imageSet.GetImage(TopLeftFrameImageName),
        imageSet.GetImage(TopRightFrameImageName),
        imageSet.GetImage(BottomLeftFrameImageName),
        imageSet.GetImage(BottomRightFrameImageName),
        imageSet.GetImage(LeftFrameImageName),
        imageSet.GetImage(TopFrameImageName),
        imageSet.GetImage(RightFrameImageName),
        imageSet.GetImage(BottomFrameImageName));

    StoreFrameSizes();

    // setup client area clearing brush
    clientBrush.Image = imageSet.GetImage(ClientBrushImageName);
    clientBrush.Position = new PointF(frameLeftSize, frameTopSize);
    clientBrush.HorizontalFormat = HorizontalImageFormat.Tiled;
    clientBrush.VerticalFormat = VerticalImageFormat.Tiled;

    // setup cursor images for this window
    SetMouseCursor(imageSet.GetImage(NormalCursorImageName));
    sizingCursorNS = imageSet.GetImage(NorthSouthCursorImageName);
    sizingCursorEW = imageSet.GetImage(EastWestCursorImageName);
    sizingCursorNWSE = imageSet.GetImage(NWestSEastCursorImageName);
    sizingCursorNESW = imageSet.GetImage(NEastSWestCursorImageName);
  }

  #endregion Constructor

  #region Methods

  /// <summary>
  ///		Return a Rect that describes, in window relative pixel co-ordinates,
  ///		the outer edge of the sizing area for this window.
  /// </summary>
  /// <returns></returns>
  protected override Rect GetSizingRect() {
    return frame.Rect;
  }

  /// <summary>
  ///		Store the sizes for the frame edges.
  /// </summary>
  protected void StoreFrameSizes() {
    Imageset imageSet = ImagesetManager.Instance.GetImageset(ImagesetName);

    frameLeftSize = imageSet.GetImage(TopLeftFrameImageName).Width;
    frameRightSize = imageSet.GetImage(TopRightFrameImageName).Width;
    frameTopSize = imageSet.GetImage(TopFrameImageName).Height;
    frameBottomSize = imageSet.GetImage(BottomFrameImageName).Height;
  }

  #endregion Methods

  #region Window Members

  /// <summary>
  ///		Return a Rect object that describes, unclipped, the inner rectangle for this window.	
  /// </summary>
  public override Rect UnclippedInnerRect {
    get {
      Rect tempRect = UnclippedPixelRect;

      if(FrameEnabled) {
        PointF pos = frame.Position;

        tempRect.Left += pos.X + frameLeftSize;
        tempRect.Right -= frameRightSize;
        tempRect.Top += pos.Y + frameTopSize;
        tempRect.Bottom -= frameBottomSize;
      }

      return tempRect;
    }
  }

  /// <summary>
  /// 
  /// </summary>
  /// <returns></returns>
  public override PushButton CreateCloseButton() {
    // TODO: Clean up
    TLCloseButton button = (TLCloseButton)WindowManager.Instance.CreateWindow(
        "TaharezLook.TLCloseButton", name + "_auto_PushButton");

    button.StandardImageryEnabled = false;
    button.CustomImageryAutoSized = true;

    // setup close button imagery
    RenderableImage image = new RenderableImage();
    image.HorizontalFormat = HorizontalImageFormat.Stretched;
    image.VerticalFormat = VerticalImageFormat.Stretched;

    Imageset imageSet = ImagesetManager.Instance.GetImageset(ImagesetName);

    image.Image = imageSet.GetImage(CloseButtonNormalImageName);
    image.SetColors(new Colour(CloseButtonNormalColor), new Colour(CloseButtonNormalColor), new Colour(CloseButtonNormalColor), new Colour(CloseButtonNormalColor));
    button.SetNormalImage(image);

    image = new RenderableImage();
    image.HorizontalFormat = HorizontalImageFormat.Stretched;
    image.VerticalFormat = VerticalImageFormat.Stretched;
    image.Image = imageSet.GetImage(CloseButtonHoverImageName);
    image.SetColors(new Colour(CloseButtonHoverColor), new Colour(CloseButtonHoverColor), new Colour(CloseButtonHoverColor), new Colour(CloseButtonHoverColor));
    button.SetHoverImage(image);

    image = new RenderableImage();
    image.HorizontalFormat = HorizontalImageFormat.Stretched;
    image.VerticalFormat = VerticalImageFormat.Stretched;
    image.Image = imageSet.GetImage(CloseButtonPushedImageName);
    image.SetColors(new Colour(CloseButtonPushedColor), new Colour(CloseButtonPushedColor), new Colour(CloseButtonPushedColor), new Colour(CloseButtonPushedColor));
    button.SetPushedImage(image);

    button.MetricsMode = MetricsMode.Absolute;
    button.AlwaysOnTop = true;

    return button;
  }

  /// <summary>
  /// 
  /// </summary>
  /// <returns></returns>
  public override TitleBar CreateTitleBar()
  {
    // create a titlebar to use for this frame window
    TitleBar window = (TitleBar)WindowManager.Instance.CreateWindow(
      "TaharezLook.TLTitleBar", name + "_auto_Titlebar");

    window.MetricsMode = MetricsMode.Absolute;
    window.Position = new PointF(TitlebarXOffset, TitlebarYOffset);

    return window;
  }

  /// <summary>
  /// 
  /// </summary>
  public override void LayoutComponentWidgets() {
    // calculate and set height of title bar
    SizeF titleSize = new SizeF();
    if(titleBar.Font != null)
      titleSize.Height = titleBar.Font.LineSpacing + TitlebarTextPadding;
    else
      titleSize.Height = 12 + TitlebarTextPadding;
    titleSize.Width = this.IsRolledUp ? absOpenSize.Width : absArea.Width;

    titleBar.Size = titleSize;

    // set size of close button to be the same as the height for the title bar.
    float closeSize = ImagesetManager.Instance.GetImageset(ImagesetName).GetImage(CloseButtonNormalImageName).Width;
    closeButton.Size = new SizeF(closeSize, closeSize);

    float closeX = titleSize.Width - closeSize - ImagesetManager.Instance.GetImageset(TLTitleBar.ImagesetName).GetImage(TLTitleBar.SysAreaRightImageName).Width;
    float closeY = TitlebarYOffset + ((titleSize.Height - closeSize) / 2);

    closeButton.Position = new PointF(closeX, closeY);
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="z"></param>
  protected override void DrawSelf(float z) {
    // get the destination screen rect for this window
    Rect absRect = UnclippedPixelRect;

    CeGui.Vector3 pos =
        new CeGui.Vector3(absRect.Left, absRect.Top, z);

    clientBrush.Draw(pos, InnerRect);

    if(FrameEnabled) {
      frame.Draw(pos, PixelRect);
    }
  }

  #region Overridden Event Trigger Methods

  /// <summary>
  /// 
  /// </summary>
  /// <param name="e"></param>
  protected override void OnSized(GuiEventArgs e) {
    // MUST call base class handler no matter what.  This is now required 100%
    base.OnSized(e);

    Rect area = UnclippedPixelRect;
    SizeF newSize = new SizeF(area.Width, area.Height);

    // adjust frame and client area rendering objects so that the title bar and close button appear half in and half-out of the frame.
    float frameOffset = 0;

    // if title bar is active, close button is the same height.
    if(TitleBarEnabled) {
      frameOffset = titleBar.UnclippedPixelRect.Height * 0.5f;
    }
      // if no title bar, measure the close button instead.
  else if(CloseButtonEnabled) {
      frameOffset = closeButton.UnclippedPixelRect.Height * 0.5f;
    }

    // move frame into position
    PointF pos = new PointF(0, frameOffset);
    frame.Position = pos;

    // adjust position for client brush
    pos.Y += frameTopSize;
    pos.X += frameLeftSize;
    clientBrush.Position = pos;

    // adjust size of frame
    newSize.Height -= frameOffset;
    frame.Size = newSize;

    // modify size of client so it is within the frame
    if(FrameEnabled) {
      newSize.Width -= (frameLeftSize + frameRightSize);
      newSize.Height -= (frameTopSize + frameBottomSize);
    }

    clientBrush.Size = newSize;
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="e"></param>
  protected override void OnAlphaChanged(GuiEventArgs e) {
    // default processing
    base.OnAlphaChanged(e);

    float alpha = EffectiveAlpha;

    // Set color rect alpha
    ColourRect cr = frame.Colors;
    cr.SetAlpha(alpha);

    cr = clientBrush.Colors;
    cr.SetAlpha(alpha);
  }


  #endregion Overridden Event Trigger Methods

  #endregion Window Members
}

} // namespace CeGui.WidgetSets.Taharez
