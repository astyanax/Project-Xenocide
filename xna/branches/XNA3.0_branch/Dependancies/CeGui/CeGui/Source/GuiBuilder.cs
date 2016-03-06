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
using CeGui.Widgets;
using System.Xml;
using System.Drawing;

namespace CeGui {

/// <summary>
/// Provides the base class for theme-specific widget factories as well as convience creation overloads which allow it to act as a state-driven GUI builder.
/// </summary>
/// <remarks>If the Create* methods are called directly then RegisterWindow must be called on them before use.
/// Non-null, unique ID's must be specified for all windows, though auto-generated IDs may be supported later when null is specified.
/// The New* methods create a window and provide overloads to specify the most common properties as well to be set. They also add the created
/// window as a child of the ParentWindow if one is specified or if the last created window is set to it via StartChildren().
/// </remarks>
public abstract class GuiBuilder {
  #region Fields and Properties
  protected Window parentWindow;
  public Window ParentWindow { get { return parentWindow; } set { parentWindow = value; } }
  protected Window lastCreated;
  public Window LastCreated { get { return lastCreated; } }
  public string ImagesetName;
  public string Name;
  protected bool initialized = false;

  #endregion

  #region Constructors
  public GuiBuilder() {
  }
  #endregion

  #region Methods

  #region Builder Methods
  #region Non-Themed
  #region Text
  public StaticText NewText(string name, string text, PointF position, SizeF Size) {
    StaticText label = CreateText(name);
    if(text != null)
      label.Text = text;
    label.Position = position;
    label.Size = Size;
    return label;
  }

  public StaticText NewText(string name, string text, Colour textColor, PointF position, SizeF Size) {
    StaticText label = NewText(name, text, position, Size);
    label.SetTextColor(textColor);
    return label;
  }

  public StaticText NewText(string name, string text, Colour textColor,
      HorizontalTextFormat horzFormat, VerticalTextFormat vertFormat, PointF position, SizeF Size) {
    StaticText label = NewText(name, text, textColor, position, Size);
    label.HorizontalFormat = horzFormat;
    label.VerticalFormat = vertFormat;
    return label;
  }


  public StaticText NewText(string name, string text, Colour textColor, HorizontalTextFormat horzFormat, PointF position, SizeF Size) {
    return NewText(name, text, textColor, horzFormat, VerticalTextFormat.Centered, position, Size);
  }

  #endregion

  #region Image

  public Static NewPanel(string name, bool showFrame, Image backImage, MouseEventHandler clickHandler, PointF position, SizeF Size) {
    Static panel = CreatePanel(name);
    RegisterWindow(panel);
    panel.FrameEnabled = showFrame;
    panel.BackgroundImage = backImage;
    panel.Position = position;
    panel.Size = Size;
    if(clickHandler != null)
      panel.MouseClicked += clickHandler;
    return panel;
  }
  public GuiSheet NewGroup(string name) {
    GuiSheet group = CreateGroup(name);
    RegisterWindow(group);
    return group;
  }

  public GuiSheet NewGroup(string name, PointF position, SizeF Size) {
    GuiSheet group = NewGroup(name);
    group.Position = position;
    group.Size = Size;
    return group;
  }

  public Static NewPanel(string name, bool showFrame, Image backImage, PointF position, SizeF Size) {
    return NewPanel(name, showFrame, backImage, null, position, Size);
  }

  public Static NewPanel(string name, bool showFrame, MouseEventHandler clickHandler, PointF position, SizeF Size) {
    return NewPanel(name, showFrame, null, clickHandler, position, Size);
  }

  public Static NewPanel(string name, bool showFrame, PointF position, SizeF Size) {
    return NewPanel(name, showFrame, (MouseEventHandler)null, position, Size);
  }


  public Static NewPanel(string name, MouseEventHandler clickHandler, PointF position, SizeF Size) {
    return NewPanel(name, true, clickHandler, position, Size);
  }
  public Static NewPanel(string name, PointF position, SizeF Size) {
    return NewPanel(name, null, position, Size);
  }


  public StaticImage NewImage(string name, Image image, MouseEventHandler clickHandler, PointF position, SizeF Size) {
    StaticImage staticImage = CreateImage(name);
    staticImage.Image = image;
    staticImage.Position = position;
    staticImage.Size = Size;
    if(clickHandler != null)
      staticImage.MouseClicked += clickHandler;
    return staticImage;
  }

  public StaticImage NewImage(string name, Image image, PointF position, SizeF Size) {
    return NewImage(name, image, null, position, Size);
  }

  public StaticImage NewImage(string name, Imageset imageset, string imageName, MouseEventHandler clickHandler, PointF position, SizeF Size) {
    return NewImage(name, imageset.GetImage(imageName), clickHandler, position, Size);
  }

  public StaticImage NewImage(string name, Imageset imageset, string imageName, PointF position, SizeF Size) {
    return NewImage(name, imageset, imageName, null, position, Size);
  }

  public StaticImage NewImage(string name, string imagesetName, string imageName, MouseEventHandler clickHandler, PointF position, SizeF Size) {
    return NewImage(name, ImagesetManager.Instance.GetImageset(imagesetName), imageName, clickHandler, position, Size);
  }
  public StaticImage NewImage(string name, string imagesetName, string imageName, PointF position, SizeF Size) {
    return NewImage(name, imagesetName, imageName, null, position, Size);
  }
  #endregion
  #endregion

  #region Buttons

  public PushButton NewButton(string name, string text, GuiEventHandler clickHandler, PointF position, SizeF Size) {
    PushButton button = CreateButton(name);
    RegisterWindow(button);
    if(text != null)
      button.Text = text;
    button.Position = position;
    button.Size = Size;
    if(clickHandler != null)
      button.Clicked += clickHandler;
    return button;
  }


  public PushButton NewButton(string name, string text, PointF position, SizeF Size) {
    return NewButton(name, text, null, position, Size);
  }

  public PushButton NewButton(string name, string text, GuiEventHandler clickHandler, PointF position, SizeF Size, Colour textColor, Colour hoverColor, Colour pushedColor, Colour disabledColor) {
    PushButton button = NewButton(name, text, clickHandler, position, Size);
    button.NormalTextColor = textColor;
    button.HoverTextColor = hoverColor;
    button.DisabledTextColor = disabledColor;
    button.PushedTextColor = pushedColor;
    return button;
  }

  public Checkbox NewCheckbox(string name, string text, bool isChecked, PointF position, SizeF Size) {
    Checkbox button = CreateCheckbox(name);
    RegisterWindow(button);
    if(text != null)
      button.Text = text;
    button.Checked = isChecked;
    button.Position = position;
    button.Size = Size;
    return button;
  }

  public RadioButton NewRadioButton(string name, string text, int groupID, bool isChecked, PointF position, SizeF Size) {
    RadioButton button = CreateRadioButton(name);
    RegisterWindow(button);
    if(text != null)
      button.Text = text;
    button.Checked = isChecked;
    button.GroupID = groupID;
    button.Position = position;
    button.Size = Size;
    return button;
  }
  #endregion

  #region Lists

  public ComboBox NewComboBox(string name, string text, PointF position, SizeF Size) {
    ComboBox list = CreateComboBox(name);
    RegisterWindow(list);
    if(text != null)
      list.Text = text;
    list.Position = position;
    list.Size = Size;
    return list;
  }
  public ComboBox NewComboBox(string name, PointF position, SizeF Size) {
    return NewComboBox(name, null, position, Size);
  }


  public Listbox NewListBox(string name, string text, PointF position, SizeF Size) {
    Listbox list = CreateListBox(name);
    RegisterWindow(list);
    if(text != null)
      list.Text = text;
    list.Position = position;
    list.Size = Size;
    return list;
  }
  public Listbox NewListBox(string name, PointF position, SizeF Size) {
    return NewListBox(name, null, position, Size);
  }

  public MultiColumnList NewGrid(string name, string text, PointF position, SizeF Size, GridSelectionMode selectionMode) {
    MultiColumnList list = CreateGrid(name);
    RegisterWindow(list);
    if(text != null)
      list.Text = text;
    list.Position = position;
    list.Size = Size;
    list.SelectionMode = selectionMode;
    return list;
  }

  public MultiColumnList NewGrid(string name, string text, PointF position, SizeF Size, GridSelectionMode selectionMode, float[] columnWidths, params string[] columnHeaders) {
    MultiColumnList grid = NewGrid(name, text, position, Size, selectionMode);
    grid.AddColumns(columnWidths, columnHeaders);
    return grid;
  }
  #endregion

  #region Common Widgets

  public EditBox NewEditBox(string name, string text, PointF position, SizeF Size) {
    EditBox box = CreateEditBox(name);
    RegisterWindow(box);
    if(text != null)
      box.Text = text;
    box.Position = position;
    box.Size = Size;
    return box;
  }

  public EditBox NewEditBox(string name, PointF position, SizeF Size) {
    return NewEditBox(name, string.Empty, position, Size);
  }

  public FrameWindow NewFrameWindow(string name, string text, PointF position, SizeF Size) {
    FrameWindow window = CreateFrameWindow(name);
    RegisterWindow(window);
    if(text != null)
      window.Text = text;
    window.Position = position;
    window.Size = Size;
    return window;
  }

  public FrameWindow NewFrameWindow(string name, PointF position, SizeF Size) {
    return NewFrameWindow(name, string.Empty, position, Size);
  }


  public ProgressBar NewProgressBar(string name, PointF position, SizeF Size, float progress) {
    ProgressBar bar = CreateProgressBar(name);
    RegisterWindow(bar);
    bar.Position = position;
    bar.Size = Size;
    bar.Progress = progress;
    return bar;
  }


  public ProgressBar NewProgressBar(string name, PointF position, SizeF Size) {
    return NewProgressBar(name, position, Size, 0f);
  }

  public Slider NewSlider(string name, PointF position, SizeF Size, float value) {
    Slider slider = CreateSlider(name);
    RegisterWindow(slider);
    slider.Position = position;
    slider.Size = Size;
    slider.Value = value;
    return slider;
  }


  public Slider NewSlider(string name, PointF position, SizeF Size) {
    return NewSlider(name, position, Size, 0f);
  }

  #endregion

  #region Other Widgets


  public TitleBar NewTitleBar(string name, string text, PointF position, SizeF Size) {
    TitleBar bar = CreateTitleBar(name);
    RegisterWindow(bar);
    if(text != null)
      bar.Text = text;
    bar.Position = position;
    bar.Size = Size;
    return bar;
  }


  public Scrollbar NewHorzScrollbar(string name, PointF position, SizeF Size) {
    Scrollbar bar = CreateHorzScrollbar(name);
    RegisterWindow(bar);
    bar.Position = position;
    bar.Size = Size;
    return bar;
  }
  public Scrollbar NewVertScrollbar(string name, PointF position, SizeF Size) {
    Scrollbar bar = CreateVertScrollbar(name);
    RegisterWindow(bar);
    bar.Position = position;
    bar.Size = Size;
    return bar;
  }
  #endregion
  #endregion

  #region Utility Methods
  public void Initialize(string resourcePath) {
    if(!initialized) //don't load again as will throw exception
		{
      string path = ImagesetName;
      if(resourcePath != null && resourcePath != string.Empty) {
        path = System.IO.Path.Combine(resourcePath, path);
      }
      //will automatically append the file extension if needed
      ImagesetManager.Instance.CreateImageset(path);
      initialized = true;
    }
  }
  public void RegisterWindow(Window window) {
    //following call will error if null
    if(window.Name == null)
      throw new ArgumentException("A non-null name must be specified for a window for it to be registered.");
    //registers and calls the Initialize() method
    WindowManager.Instance.AttachWindow(window);
    lastCreated = window;
    //add as a child to the default parent if there is one
    if(parentWindow != null)
      parentWindow.AddChild(window);
  }
  public void StartChildren() {
    parentWindow = lastCreated;
  }
  public void EndChildren() {
    if(parentWindow != null)
      parentWindow = parentWindow.Parent;
  }
  public void SetLastCreatedAsRoot() {
    GuiSystem.Instance.GuiSheet = lastCreated;
  }
  #endregion

  #region Virtual Factory Methods
  public virtual GuiSheet CreateGroup(string name) {
    return new GuiSheet("", name);
  }
  public virtual Static CreatePanel(string name) {
    return new Static("", name);
  }

  public virtual StaticImage CreateImage(string name) {
    return (StaticImage)WindowManager.Instance.CreateWindow("StaticImage", name);
  }
  public StaticText CreateText(string name) {
    return (StaticText)WindowManager.Instance.CreateWindow("StaticText", name);
  }
  #endregion

  #region Abstract Factory Methods
  public abstract PushButton CreateButton(string name);
  public abstract Checkbox CreateCheckbox(string name);
  public abstract RadioButton CreateRadioButton(string name);

  public abstract ComboBox CreateComboBox(string name);
  public abstract Listbox CreateListBox(string name);
  public abstract MultiColumnList CreateGrid(string name);

  public abstract EditBox CreateEditBox(string name);
  public abstract FrameWindow CreateFrameWindow(string name);
  public abstract ProgressBar CreateProgressBar(string name);
  public abstract Slider CreateSlider(string name);

  public abstract TitleBar CreateTitleBar(string name);
  public abstract Scrollbar CreateVertScrollbar(string name);
  public abstract Scrollbar CreateHorzScrollbar(string name);

  public abstract ListHeader CreateListHeader(string name);
  public abstract ListHeaderSegment CreateListHeaderSegment(string name);
  public abstract ComboDropList CreateComboDropList(string name);

  #endregion
  #endregion
}

} // namespace CeGui
