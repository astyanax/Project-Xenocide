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
/// Summary description for TLComboDropList.
/// </summary>
[ AlternateWidgetName("TaharezLook.TLComboDropList") ]
public class TLComboDropList : ComboDropList {
  #region Constants

  /// <summary>
  ///		Name of the imageset to use for rendering.
  /// </summary>
  const string ImagesetName = "TaharezLook";
  /// <summary>
  ///		Name of the image to use for the top-left corner of the box.
  /// </summary>
  const string TopLeftImageName = "ComboboxListTopLeft";
  /// <summary>
  ///		Name of the image to use for the top-right corner of the box.
  /// </summary>
  const string TopRightImageName = "ComboboxListTopRight";
  /// <summary>
  ///		Name of the image to use for the bottom left corner of the box.
  /// </summary>
  const string BottomLeftImageName = "ComboboxListBottomLeft";
  /// <summary>
  ///		Name of the image to use for the bottom right corner of the box.
  /// </summary>
  const string BottomRightImageName = "ComboboxListBottomRight";
  /// <summary>
  ///		Name of the image to use for the left edge of the box.
  /// </summary>
  const string LeftEdgeImageName = "ComboboxListLeft";
  /// <summary>
  ///		Name of the image to use for the right edge of the box.
  /// </summary>
  const string RightEdgeImageName = "ComboboxListRight";
  /// <summary>
  ///		Name of the image to use for the top edge of the box.
  /// </summary>
  const string TopEdgeImageName = "ComboboxListTop";
  /// <summary>
  ///		Name of the image to use for the bottom edge of the box.
  /// </summary>
  const string BottomEdgeImageName = "ComboboxListBottom";
  /// <summary>
  ///		Name of the image to use for the box background.
  /// </summary>
  const string BackgroundImageName = "ComboboxListBackdrop";
  /// <summary>
  ///		Name of the image to use for the selection highlight brush.
  /// </summary>
  const string SelectionBrushImageName = "ComboboxSelectionBrush";
  /// <summary>
  ///		Name of the image to use for the mouse cursor.
  /// </summary>
  const string MouseCursorImageName = "MouseTarget";

  // component widget type names
  /// <summary>
  ///		Type name of widget to be created as horizontal scroll bar.
  /// </summary>
  const string HorzScrollbarTypeName = "TaharezLook.TLMiniHorizontalScrollbar";
  /// <summary>
  ///		Type name of widget to be created as vertical scroll bar.
  /// </summary>
  const string VertScrollbarTypeName = "TaharezLook.TLMiniVerticalScrollbar";

  #endregion Constants

  #region Fields

  /// <summary>
  ///		Used for the frame of the drop-list box.
  /// </summary>
  protected RenderableFrame frame = new RenderableFrame();
  /// <summary>
  /// `Used for the background area of the drop-list box.
  /// </summary>
  protected RenderableImage background = new RenderableImage();

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

  public TLComboDropList(string type, string name)
    : base(type, name) {
  }

  #endregion Constructor

  #region Window Members

  public override void Initialize() {
    base.Initialize();

    StoreFrameSizes();

    // setup frame images
    Imageset iset = ImagesetManager.Instance.GetImageset(ImagesetName);
    frame.SetImages(
        iset.GetImage(TopLeftImageName), iset.GetImage(TopRightImageName),
        iset.GetImage(BottomLeftImageName), iset.GetImage(BottomRightImageName),
        iset.GetImage(LeftEdgeImageName), iset.GetImage(TopEdgeImageName),
        iset.GetImage(RightEdgeImageName), iset.GetImage(BottomEdgeImageName)
        );

    // setup background brush
    background.Image = iset.GetImage(BackgroundImageName);
    background.Position = new PointF(frameLeftSize, frameTopSize);
    background.HorizontalFormat = HorizontalImageFormat.Stretched;
    background.VerticalFormat = VerticalImageFormat.Stretched;

    // set cursor for this window
    SetMouseCursor(iset.GetImage(MouseCursorImageName));
  }

  #endregion Window Members

  #region Listbox Members

  #region Properties

  /// <summary>
  ///		Return a Rect object describing, in un-clipped pixels, the window
  ///		relative area that is to be used for rendering list items.
  /// </summary>
  protected override Rect ListRenderArea {
    get {
      Rect tmp = new Rect();

      tmp.Left = frameLeftSize;
      tmp.Top = frameTopSize;
      tmp.Size = new SizeF(AbsoluteWidth - frameLeftSize, AbsoluteHeight - frameTopSize);

      if(vertScrollbar.Visible) {
        tmp.Right -= vertScrollbar.AbsoluteWidth;
      } else {
        tmp.Right -= frameRightSize;
      }

      if(horzScrollbar.Visible) {
        tmp.Bottom -= horzScrollbar.AbsoluteHeight;
      } else {
        tmp.Bottom -= frameBottomSize;
      }

      return tmp;
    }
  }

  #endregion Properties

  #region Methods

  protected override Scrollbar CreateHorizontalScrollbar() {
    Scrollbar sbar = (Scrollbar)WindowManager.Instance.CreateWindow(HorzScrollbarTypeName, name + "__auto_hscrollbar");

    // set the min/max sizes
    sbar.MinimumSize = new SizeF(0.0f, 0.016667f);
    sbar.MaximumSize = new SizeF(1.0f, 0.016667f);

    return sbar;
  }

  protected override Scrollbar CreateVerticalScrollbar() {
    Scrollbar sbar = (Scrollbar)WindowManager.Instance.CreateWindow(VertScrollbarTypeName, name + "__auto_vscrollbar");

    // set the min/max sizes
    sbar.MinimumSize = new SizeF(0.0125f, 0);
    sbar.MaximumSize = new SizeF(0.0125f, 1.0f);

    return sbar;
  }

  protected override void LayoutComponentWidgets() {
    // set desired size for vertical scroll-bar
    SizeF vSize = new SizeF(0.05f, 1.0f);
    vertScrollbar.Size = vSize;

    // get the actual size used for vertical scroll bar.
    vSize = AbsoluteToRelative(vertScrollbar.AbsoluteSize);

    // set desired size for horizontal scroll-bar
    SizeF hSize = new SizeF(1.0f, 0.0f);

    if(absArea.Height != 0.0f) {
      hSize.Height = (absArea.Width * vSize.Width) / absArea.Height;
    }

    // adjust length to consider width of vertical scroll bar if that is visible
    if(vertScrollbar.Visible) {
      hSize.Width -= vSize.Width;
    }

    horzScrollbar.Size = hSize;

    // get actual size used
    hSize = AbsoluteToRelative(horzScrollbar.AbsoluteSize);

    // position vertical scroll bar
    vertScrollbar.Position = new PointF(1.0f - vSize.Width, 0.0f);

    // position horizontal scroll bar
    horzScrollbar.Position = new PointF(0.0f, 1.0f - hSize.Height);
  }

  protected override void RenderListboxBaseImagery(float z) {
    Rect clipper = PixelRect;

    // do nothing if the widget is totally clipped.
    if(clipper.Width == 0) {
      return;
    }

    // get the destination screen rect for this window
    Rect absrect = UnclippedPixelRect;

    // draw the box elements
    Vector3 pos = new Vector3(absrect.Left, absrect.Top, z);
    background.Draw(pos, clipper);
    frame.Draw(pos, clipper);
  }

  #endregion Methods

  #endregion Listbox Members

  #region Base Members

  /// <summary>
  ///		Store the sizes for the frame edges.
  /// </summary>
  protected void StoreFrameSizes() {
    Imageset iset = ImagesetManager.Instance.GetImageset(ImagesetName);

    frameLeftSize = iset.GetImage(LeftEdgeImageName).Width;
    frameRightSize = iset.GetImage(RightEdgeImageName).Width;
    frameTopSize = iset.GetImage(TopEdgeImageName).Height;
    frameBottomSize = iset.GetImage(BottomEdgeImageName).Height;
  }

  #endregion Base Members

  #region Events

  #region Overridden Trigger Methods

  protected override void OnSized(GuiEventArgs e) {
    // base class processing
    base.OnSized(e);

    // update size of frame
    SizeF newsize = AbsoluteSize;
    frame.Size = newsize;

    // update size of background image
    newsize.Width -= (frameLeftSize + frameRightSize);
    newsize.Height -= (frameTopSize + frameBottomSize);

    background.Size = newsize;
  }

  protected override void OnAlphaChanged(GuiEventArgs e) {
    // base class processing
    base.OnAlphaChanged(e);

    // update alpha values for the frame and background brush
    float alpha = EffectiveAlpha;

    ColourRect cr = frame.Colors;
    cr.SetAlpha(alpha);

    cr = background.Colors;
    cr.SetAlpha(alpha);
  }

  #endregion Overridden Trigger Methods

  #endregion Events
}

} // namespace CeGui.WidgetSets.Taharez
