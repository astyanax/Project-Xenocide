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
/// Summary description for TLComboBox.
/// </summary>
[ AlternateWidgetName("TaharezLook.TLComboBox") ]
public class TLComboBox : ComboBox {
  #region Constants

  const string ImagesetName = "TaharezLook";
  const string ButtonNormalImageName = "ComboboxListButtonNormal";
  const string ButtonHighlightedImageName = "ComboboxListButtonHover";

  // component widget type names
  const string EditboxTypeName = "TaharezLook.TLEditBox";
  const string DropListTypeName = "TaharezLook.TLComboDropList";
  const string ButtonTypeName = "TaharezLook.TLButton";

  #endregion Costants

  /// <summary>
  /// 
  /// </summary>
  /// <param name="name"></param>
  public TLComboBox(string type, string name)
    : base(type, name) {
  }



  /// <summary>
  /// 
  /// </summary>
  /// <param name="z"></param>
  protected override void DrawSelf(float z) {
    // do nothing, this is based off nothing sub sub-widgets
  }

  /// <summary>
  /// 
  /// </summary>
  protected override void LayoutComponentWidgets() {
    PointF pos = new PointF();
    SizeF sz = new SizeF();

    float ebheight = this.Font.LineSpacing * 1.5f;

    // set the button size
    sz.Height = sz.Width = ebheight;
    button.Size = sz;

    // set-up edit box
    pos.X = pos.Y = 0;
    editBox.Position = pos;

    sz.Width = AbsoluteWidth - ebheight;
    editBox.Size = sz;

    // set button position
    pos.X = sz.Width;
    button.Position = pos;

    // set list position and size (relative)
    pos.X = 0;
    pos.Y = (AbsoluteHeight == 0.0f) ? 0.0f : (ebheight / AbsoluteHeight);
    dropList.Position = pos;

    sz.Width = 1.0f;
    sz.Height = 1.0f - pos.Y;
    dropList.Size = sz;
  }

  /// <summary>
  /// 
  /// </summary>
  /// <returns></returns>
  public override PushButton CreatePushButton() {
    TLButton btn = (TLButton)WindowManager.Instance.CreateWindow(ButtonTypeName, name + "__auto_button__");
    btn.MetricsMode = MetricsMode.Absolute;

    // Set up imagery
    btn.StandardImageryEnabled = false;
    btn.CustomImageryAutoSized = true;

    RenderableImage img = new RenderableImage();
    img.HorizontalFormat = HorizontalImageFormat.Stretched;
    img.VerticalFormat = VerticalImageFormat.Stretched;

    img.Image = ImagesetManager.Instance.GetImageset(ImagesetName).GetImage(ButtonNormalImageName);
    btn.SetNormalImage(img);
    btn.SetDisabledImage(img);

    img = new RenderableImage();
    img.HorizontalFormat = HorizontalImageFormat.Stretched;
    img.VerticalFormat = VerticalImageFormat.Stretched;

    img.Image = ImagesetManager.Instance.GetImageset(ImagesetName).GetImage(ButtonHighlightedImageName);
    btn.SetHoverImage(img);
    btn.SetPushedImage(img);

    return btn;
  }

  /// <summary>
  /// 
  /// </summary>
  /// <returns></returns>
  public override EditBox CreateEditBox() {
    EditBox box = (EditBox)WindowManager.Instance.CreateWindow(EditboxTypeName, name + "__auto_editbox__");
    box.MetricsMode = MetricsMode.Absolute;
    return box;
  }

  /// <summary>
  /// 
  /// </summary>
  /// <returns></returns>
  public override ComboDropList CreateDropList() {
    return (ComboDropList)WindowManager.Instance.CreateWindow(DropListTypeName, name + "__auto_droplist__");
  }

}

} // namespace CeGui.WidgetSets.Taharez
