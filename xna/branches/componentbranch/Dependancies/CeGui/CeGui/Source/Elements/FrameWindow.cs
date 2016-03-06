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
///		Abstract base class for a movable, sizable, window with a title-bar and a frame.
/// </summary>
/// C++ Version Sync
/// .cpp:	1.8
/// .h:		1.6
[ AlternateWidgetName("FrameWindow") ]
public abstract class FrameWindow : Window {
  #region Constants

  /// <summary>
  ///		Default SizeF for the sizing border (in pixels).
  /// </summary>
  const float DefaultSizingBorderSize = 8.0f;

  #endregion Constants

  #region Fields

  /// <summary>
  ///		true if window frame should be drawn.
  /// </summary>
  protected bool isFrameEnabled;
  /// <summary>
  ///		true if roll-up of window is allowed.
  /// </summary>
  protected bool isRollupEnabled;
  /// <summary>
  ///		true if window is rolled up.
  /// </summary>
  protected bool isRolledUp;
  /// <summary>
  ///		Stores original SizeF of window when rolled-up.
  /// </summary>
  protected SizeF absOpenSize;
  /// <summary>
  ///		Stores original SizeF of window when rolled-up.
  /// </summary>
  protected SizeF relOpenSize;

  /// <summary>
  ///		true if sizing is enabled for this window.
  /// </summary>
  protected bool isSizingEnabled;
  /// <summary>
  ///		true if window is being sized.
  /// </summary>
  protected bool isBeingSized;
  /// <summary>
  ///		Thickness of the sizing border around this window
  /// </summary>
  protected float borderSize;
  /// <summary>
  ///		Point window is being dragged at.
  /// </summary>
  protected PointF dragPoint;

  /// <summary>
  ///		Reference to the title bar widget.
  /// </summary>
  protected TitleBar titleBar;
  /// <summary>
  ///		Reference to the close button widget.
  /// </summary>
  protected PushButton closeButton;

  /// <summary>
  ///		North/South sizing cursor image.
  /// </summary>
  protected Image sizingCursorNS;
  /// <summary>
  ///		East/West sizing cursor image.
  /// </summary>
  protected Image sizingCursorEW;
  /// <summary>
  ///		North-West/South-East cursor image.
  /// </summary>
  protected Image sizingCursorNWSE;
  /// <summary>
  ///		North-East/South-West cursor image.
  /// </summary>
  protected Image sizingCursorNESW;

  /// <summary>
  ///		true if the window will move when dragged by the title bar.
  /// </summary>
  protected bool isDragMovable;

  #endregion Fields

  #region Constructor

  /// <summary>
  ///		Constructor.
  /// </summary>
  /// <param name="type">Type of the window.</param>
  /// <param name="name">Name of the window.</param>
  public FrameWindow(string type, string name)
    : base(type, name) {
    isFrameEnabled = true;
    isSizingEnabled = true;
    isRolledUp = false;
    isRollupEnabled = true;
    isSizingEnabled = true;
    isBeingSized = false;
    isDragMovable = true;

    borderSize = DefaultSizingBorderSize;
  }

  #endregion Constructor

  #region Abstract Members

  #region Methods

  //njk-patch
  /// <summary>
  ///		Create a control based upon the Titlebar base class to be used as the title bar for this window.
  /// </summary>
  /// <returns>Reference to an object who's class derives from Titlebar.</returns>
  public abstract TitleBar CreateTitleBar();

  /// <summary>
  ///		Create a control based upon the PushButton base class, to be used at the close button for the window.
  /// </summary>
  /// <returns>Reference to an object who's class derives from PushButton.</returns>
  public abstract PushButton CreateCloseButton();

  /// <summary>
  ///		Setup SizeF and position for the title bar and close button widgets attached to this window.
  /// </summary>
  public abstract void LayoutComponentWidgets();

  #endregion Methods

  #endregion Abstract Members

  #region Base Members

  #region Properties

  /// <summary>
  ///		Get/Set whether the close button for the frame window is enabled.
  /// </summary>
  /// <value>Set to true to enable the close button (if one is attached), or false to disable the close button.</value>
  [WidgetProperty("CloseButtonEnabled")]
  public bool CloseButtonEnabled {
    get {
      return (closeButton != null) && titleBar.IsEnabled;
    }
    set {
      if(closeButton != null) {
        closeButton.IsEnabled = value;
      }
    }
  }

  /// <summary>
  ///		Get/Set whether this FrameWindow can be moved by dragging the title bar.
  /// </summary>
  /// <value>
  ///		true if the Window will move when the user drags the title bar. 
  ///		false if the window will not move.
  /// </value>
  [WidgetProperty("DragMovingEnabled")]
  public bool DragMovingEnabled {
    get {
      return isDragMovable;
    }
    set {
      if(isDragMovable != value) {
        isDragMovable = value;

        if(titleBar != null) {
          titleBar.DraggingEnabled = value;
        }
      }
    }
  }

  /// <summary>
  ///		Get/Set whether the frame for this window is enabled.
  /// </summary>
  /// <value>true if the frame for this window is enabled, false if the frame for this window is disabled.</value>
  [WidgetProperty("FrameEnabled")]
  public bool FrameEnabled {
    get {
      return isFrameEnabled;
    }
    set {
      isFrameEnabled = value;
      RequestRedraw();
    }
  }

  /// <summary>
  ///		Get/Set whether the window is currently rolled up (a.k.a shaded).
  /// </summary>
  /// <value>true if the window is rolled up, false if the window is not rolled up.</value>
  [WidgetProperty("RollUpState")]
  public bool IsRolledUp {
    get {
      return isRolledUp;
    }
    set {
      if(isRolledUp != value) {
        ToggleRollup();
      }
    }
  }

  /// <summary>
  ///		Return whether roll up (a.k.a shading) is enabled for this window.
  /// </summary>
  /// <value>true if roll up is enabled, false if roll up is disabled.</value>
  [WidgetProperty("RollUpEnabled")]
  public bool RollupEnabled {
    get {
      return isRollupEnabled;
    }
    set {
      if((value == false) && IsRolledUp) {
        ToggleRollup();
      }

      isRollupEnabled = value;
    }
  }

  /// <summary>
  ///		Get/Set whether this window is sizable.
  /// </summary>
  /// <remarks>
  ///		Note that this requires that the window have an enabled frame and that sizing itself is enabled
  /// </remarks>
  /// <value>true if the window can be sized, false if the window can not be sized.</value>
  [WidgetProperty("SizingEnabled")]
  public bool SizingEnabled {
    get {
      return isSizingEnabled && FrameEnabled;
    }
    set {
      isSizingEnabled = value;
    }
  }

  /// <summary>
  ///		Get/Set whether the title bar for this window is enabled.
  /// </summary>
  /// <value>
  ///		true if the window has a title bar and it is enabled, 
  ///		false if the window has no title bar or if the title bar is disabled.
  ///	</value>
  [WidgetProperty("TitlebarEnabled")]
  public bool TitleBarEnabled {
    get {
      return (titleBar != null) && titleBar.IsEnabled;
    }
    set {
      if(titleBar != null) {
        titleBar.IsEnabled = value;
      }
    }
  }

  /// <summary>
  ///		Get/Set the thickness of the sizing border.
  /// </summary>
  /// <value>float value describing the thickness of the sizing border in screen pixels.</value>
  [WidgetProperty("SizingBorderThickness")]
  public float SizingBorderThickness {
    get {
      return borderSize;
    }
    set {
      borderSize = value;
    }
  }

  /// <summary>
  /// Get/Set the Font of the titlebar.
  /// </summary>
  /// <value>Titlebar Font</value>
  [WidgetProperty("TitlebarFont")]
  public Font TitlebarFont {
    get {
      return titleBar.Font;
    }
    set {
      titleBar.Font = value;
    }
  }

  #endregion Properties

  #region Methods

  /// <summary>
  ///		Check local pixel co-ordinate point <paramref name="point"/> and return one of the
  ///		<see cref="SizingLocation"/> enumerated values depending where the point falls on
  ///		the sizing border.
  /// </summary>
  /// <param name="point">The window relative offset to check (in pixels).</param>
  /// <returns>
  ///		One of the <see cref="SizingLocation"/> enumerated values that describe which part of
  ///		the sizing border that <paramref name="point"/> corresponded to, if any.
  /// </returns>
  protected SizingLocation GetSizingBorderAtPoint(PointF point) {
    Rect frameRect = GetSizingRect();

    // we can only SizeF if the frame is enabled and sizing is on
    if(SizingEnabled && FrameEnabled) {
      // point must be inside the outer edge
      if(frameRect.IsPointInRect(point)) {
        // adjust Rect to get inner edge
        frameRect.Left += borderSize;
        frameRect.Top += borderSize;
        frameRect.Right -= borderSize;
        frameRect.Bottom -= borderSize;

        // detect which edges we are on
        bool top = (point.Y < frameRect.Top);
        bool bottom = (point.Y >= frameRect.Bottom);
        bool left = (point.X < frameRect.Left);
        bool right = (point.X >= frameRect.Right);

        // return appropriate 'SizingLocation' value
        if(top && left) {
          return SizingLocation.TopLeft;
        } else if(top && right) {
          return SizingLocation.TopRight;
        } else if(bottom && left) {
          return SizingLocation.BottomLeft;
        } else if(bottom && right) {
          return SizingLocation.BottomRight;
        } else if(top) {
          return SizingLocation.Top;
        } else if(bottom) {
          return SizingLocation.Bottom;
        } else if(left) {
          return SizingLocation.Left;
        } else if(right) {
          return SizingLocation.Right;
        }
      }
    }

    // default value
    return SizingLocation.None;
  }

  /// <summary>
  ///		Return a Rect that describes, in window relative pixel co-ordinates, the outer edge of the sizing area for this window.
  /// </summary>
  /// <returns>Sizing Rect.</returns>
  protected virtual Rect GetSizingRect() {
    return new Rect(0, 0, absArea.Width, absArea.Height);
  }

  /// <summary>
  ///		Returns true if given <see cref="SizingLocation"/> is on the left edge.
  /// </summary>
  /// <param name="location">SizingLocation value to be checked.</param>
  /// <returns>
  ///		true if <paramref name="location"/> is on the left edge.  
  ///		false if <paramref name="location"/> is not on the left edge.
  ///	</returns>
  protected bool IsLeftSizingLocation(SizingLocation location) {
    return (location == SizingLocation.Left) ||
            (location == SizingLocation.TopLeft) ||
            (location == SizingLocation.BottomLeft);
  }

  /// <summary>
  ///		Returns true if given <see cref="SizingLocation"/> is on the right edge.
  /// </summary>
  /// <param name="location">SizingLocation value to be checked.</param>
  /// <returns>
  ///		true if <paramref name="location"/> is on the right edge.  
  ///		false if <paramref name="location"/> is not on the right edge.
  ///	</returns>
  protected bool IsRightSizingLocation(SizingLocation location) {
    return (location == SizingLocation.Right) ||
            (location == SizingLocation.TopRight) ||
            (location == SizingLocation.BottomRight);
  }

  /// <summary>
  ///		Returns true if given <see cref="SizingLocation"/> is on the top edge.
  /// </summary>
  /// <param name="location">SizingLocation value to be checked.</param>
  /// <returns>
  ///		true if <paramref name="location"/> is on the top edge.  
  ///		false if <paramref name="location"/> is not on the top edge.
  ///	</returns>
  protected bool IsTopSizingLocation(SizingLocation location) {
    return (location == SizingLocation.Top) ||
            (location == SizingLocation.TopLeft) ||
            (location == SizingLocation.TopRight);
  }

  /// <summary>
  ///		Returns true if given <see cref="SizingLocation"/> is on the bottom edge.
  /// </summary>
  /// <param name="location">SizingLocation value to be checked.</param>
  /// <returns>
  ///		true if <paramref name="location"/> is on the bottom edge.  
  ///		false if <paramref name="location"/> is not on the bottom edge.
  ///	</returns>
  protected bool IsBottomSizingLocation(SizingLocation location) {
    return (location == SizingLocation.Bottom) ||
            (location == SizingLocation.BottomLeft) ||
            (location == SizingLocation.BottomRight);
  }

  /// <summary>
  ///		Move the window's left edge by <paramref name="delta"/>.
  /// </summary>
  /// <remarks>
  ///		The rest of the window does not move, thus this changes the SizeF of the Window.
  /// </remarks>
  /// <param name="delta">
  ///		float value that specifies the amount to move the window edge, and in which direction.  
  ///		Positive values make window smaller.
  ///	</param>
  protected void MoveLeftEdge(float delta) {
    float width = absArea.Width;

    // limit SizeF to within max/min values
    if((width - delta) < minSize.Width) {
      delta = width - minSize.Width;
    } else if((width - delta) > maxSize.Width) {
      delta = width - maxSize.Width;
    }

    // update window state
    absArea.Left += delta;

    relArea = AbsoluteToRelativeImpl(Parent, absArea);

    // event notification
    OnSized(new GuiEventArgs());
  }

  /// <summary>
  ///		Move the window's right edge by <paramref name="delta"/>.
  /// </summary>
  /// <remarks>
  ///		The rest of the window does not move, thus this changes the SizeF of the Window.
  /// </remarks>
  /// <param name="delta">
  ///		float value that specifies the amount to move the window edge, and in which direction.  
  ///		Positive values make window smaller.
  ///	</param>
  protected void MoveRightEdge(float delta) {
    float width = absArea.Width;

    // limit SizeF to within max/min values
    if((width + delta) < minSize.Width) {
      delta = minSize.Width - width;
    } else if((width + delta) > maxSize.Width) {
      delta = maxSize.Width - width;
    }

    // update window state
    absArea.Right += delta;
    dragPoint.X += delta;

    relArea = AbsoluteToRelativeImpl(Parent, absArea);

    // event notification
    OnSized(new GuiEventArgs());
  }

  /// <summary>
  ///		Move the window's top edge by <paramref name="delta"/>.
  /// </summary>
  /// <remarks>
  ///		The rest of the window does not move, thus this changes the SizeF of the Window.
  /// </remarks>
  /// <param name="delta">
  ///		float value that specifies the amount to move the window edge, and in which direction.  
  ///		Positive values make window smaller.
  ///	</param>
  protected void MoveTopEdge(float delta) {
    float height = absArea.Height;

    // limit SizeF to within max/min values
    if((height - delta) < minSize.Height) {
      delta = height - minSize.Height;
    } else if((height - delta) > maxSize.Height) {
      delta = height - maxSize.Height;
    }

    // update window state
    absArea.Top += delta;

    relArea = AbsoluteToRelativeImpl(Parent, absArea);

    // event notification
    OnSized(new GuiEventArgs());
  }

  /// <summary>
  ///		Move the window's bottom edge by <paramref name="delta"/>.
  /// </summary>
  /// <remarks>
  ///		The rest of the window does not move, thus this changes the SizeF of the Window.
  /// </remarks>
  /// <param name="delta">
  ///		float value that specifies the amount to move the window edge, and in which direction.  
  ///		Positive values make window smaller.
  ///	</param>
  protected void MoveBottomEdge(float delta) {
    float height = absArea.Height;

    // limit SizeF to within max/min values
    if((height + delta) < minSize.Height) {
      delta = minSize.Height - height;
    } else if((height + delta) > maxSize.Height) {
      delta = maxSize.Height - height;
    }

    // update window state
    absArea.Bottom += delta;
    dragPoint.Y += delta;

    relArea = AbsoluteToRelativeImpl(Parent, absArea);

    // event notification
    OnSized(new GuiEventArgs());
  }

  /// <summary>
  ///		Move the window by the pixel offsets specified in <paramref name="offset"/>.
  /// </summary>
  /// <remarks>
  ///		This is intended for internal system use - it is the method by which the title bar moves the frame window.
  /// </remarks>
  /// <param name="offset">The offsets to apply (offsets are in screen pixels).</param>
  public void OffsetPixelPosition(PointF offset) {
    PointF pos = absArea.Position;
    pos.X += offset.X;
    pos.Y += offset.Y;
    absArea.Position = pos;

    relArea = AbsoluteToRelativeImpl(Parent, absArea);

    // event notification
    OnMoved(new GuiEventArgs());
  }

  /// <summary>
  ///		Set the appropriate mouse cursor for the given window-relative pixel point.
  /// </summary>
  /// <param name="point">Point to determine the cursor for (if any).</param>
  public void SetCursorForPoint(PointF point) {
    switch(GetSizingBorderAtPoint(point)) {
      case SizingLocation.Top:
      case SizingLocation.Bottom:
        MouseCursor.Instance.SetImage(sizingCursorNS);
        break;

      case SizingLocation.Left:
      case SizingLocation.Right:
        MouseCursor.Instance.SetImage(sizingCursorEW);
        break;

      case SizingLocation.TopLeft:
      case SizingLocation.BottomRight:
        MouseCursor.Instance.SetImage(sizingCursorNWSE);
        break;

      case SizingLocation.TopRight:
      case SizingLocation.BottomLeft:
        MouseCursor.Instance.SetImage(sizingCursorNESW);
        break;

      default:
        MouseCursor.Instance.SetImage(this.Cursor);
        break;
    }
  }

  /// <summary>
  ///		Set the font to use for the title bar text.
  /// </summary>
  /// <param name="name">Name of the font to use.</param>
  public void SetTitlebarFont(string name) {
    if(titleBar != null) {
      titleBar.SetFont(name);
    }
  }

  /// <summary>
  ///		Set the font to use for the title bar text.
  /// </summary>
  /// <param name="font">Reference to a font to use.</param>
  public void SetTitlebarFont(Font font) {
    if(titleBar != null) {
      titleBar.SetFont(font);
    }
  }

  /// <summary>
  ///		Toggles the state of the window between rolled-up (shaded) and normal sizes.
  /// </summary>
  /// <remarks>
  ///		This requires roll-up to be enabled.
  /// </remarks>
  public void ToggleRollup() {
    if(RollupEnabled) {
      if(IsRolledUp) {
        isRolledUp = false;

        this.Size = (this.MetricsMode == MetricsMode.Relative) ? relOpenSize : absOpenSize;
      } else {
        // store original sizes for the window
        absOpenSize = absArea.Size;
        relOpenSize = relArea.Size;

        // get the current SizeF of the title bar (if any)
        SizeF titleSize = new SizeF();

        if(titleBar != null) {
          titleSize = titleBar.Size;
        }

        // work around minimum SizeF setting
        SizeF orgMin = minSize;
        minSize.Width = minSize.Height = 0;

        // set SizeF of this window to 0x0, since the title/close controls are not clipped by us, they will still be visible
        this.Size = new SizeF(0.0f, 0.0f);

        // restore original min SizeF
        minSize = orgMin;

        // reset the SizeF of the titlebar
        if(titleBar != null) {
          titleBar.Size = titleSize;
        }

        // this must be done last so OnSized does not store 0x0 as our original SizeF
        isRolledUp = true;
        LayoutComponentWidgets();
      }

      // event notification
      OnRollupToggled(new WindowEventArgs(this));
    }
  }

  #endregion Methods

  #endregion Base Members

  #region Window Members

  //njk-patch
  /// <summary>
  ///		Initialises the Window based object ready for use.
  /// </summary>
  /// <remarks>
  ///		This must be called for every window created.  
  ///		Normally this is handled automatically by the <see cref="WindowFactory"/> for each Window type.
  /// </remarks>
  public override void Initialize() {
    // call base class method in case base functionality is added
    base.Initialize();

    // create child windows
    titleBar = CreateTitleBar();
    closeButton = CreateCloseButton();

    // add child controls
    if(titleBar != null) {
      titleBar.DraggingEnabled = isDragMovable;
      AddChild(titleBar);
    }

    if(closeButton != null) {
      AddChild(closeButton);

      // bind handler to close button 'Click' event
      closeButton.Clicked += new GuiEventHandler(closeButton_Clicked);
    }

    LayoutComponentWidgets();
  }

  #endregion Window Members

  #region Events

  #region Event Declarations

  /// <summary>
  ///		Fired when the rollup (shade) state of the window changes.
  /// </summary>
  public event WindowEventHandler RollupToggled;

  /// <summary>
  ///		Fired when the close button for the window is clicked.
  /// </summary>
  public event WindowEventHandler CloseClicked;

  #endregion Event Declarations

  #region Trigger Methods

  /// <summary>
  ///		Event generated internally whenever the roll-up / shade state of the window changes.
  /// </summary>
  /// <param name="e">Event arguments.</param>
  protected internal void OnRollupToggled(WindowEventArgs e) {
    if(RollupToggled != null) {
      RollupToggled(this, e);
    }
  }

  /// <summary>
  ///		Event generated internally whenever the close button is clicked.
  /// </summary>
  /// <param name="e">Event arguments.</param>
  protected internal void OnCloseClicked(WindowEventArgs e) {
    if(CloseClicked != null) {
      CloseClicked(this, e);
    }
  }

  #endregion Trigger Methods

  #region Overridden Trigger Methods

  protected internal override void OnMouseMove(MouseEventArgs e) {
    // default processing (this is now essential as it controls event firing).
    base.OnMouseMove(e);

    // if we are not the window containing the mouse, do NOT change the cursor
    if(GuiSystem.Instance.WindowContainingMouse != this) {
      return;
    }

    if(SizingEnabled) {
      PointF localMousePos = ScreenToWindow(e.Position);

      if(this.MetricsMode == MetricsMode.Relative) {
        localMousePos = RelativeToAbsolute(localMousePos);
      }

      if(isBeingSized) {
        SizingLocation dragEdge = GetSizingBorderAtPoint(dragPoint);

        // calculate sizing deltas...
        float deltaX = localMousePos.X - dragPoint.X;
        float deltaY = localMousePos.Y - dragPoint.Y;

        // SizeF left or right edges
        if(IsLeftSizingLocation(dragEdge)) {
          MoveLeftEdge(deltaX);
        } else if(IsRightSizingLocation(dragEdge)) {
          MoveRightEdge(deltaX);
        }

        // SizeF top or bottom edges
        if(IsTopSizingLocation(dragEdge)) {
          MoveTopEdge(deltaY);
        } else if(IsBottomSizingLocation(dragEdge)) {
          MoveBottomEdge(deltaY);
        }
      } else {
        SetCursorForPoint(localMousePos);
      }

    }

    // mark event as handled
    e.Handled = true;
  }

  protected internal override void OnMouseButtonsDown(MouseEventArgs e) {
    // default processing (this is now essential as it controls event firing).
    base.OnMouseButtonsDown(e);

    if(e.Button == System.Windows.Forms.MouseButtons.Left) {
      if(this.SizingEnabled) {
        // get position of mouse as co-ordinates local to this window.
        PointF localPos = ScreenToWindow(e.Position);

        if(this.MetricsMode == MetricsMode.Relative) {
          localPos = RelativeToAbsolute(localPos);
        }

        // if the mouse is on the sizing border
        if(GetSizingBorderAtPoint(localPos) != SizingLocation.None) {
          // ensure all inputs come to us for now
          CaptureInput();

          // setup the 'dragging' state variables
          isBeingSized = true;
          dragPoint = localPos;
        }
      }

      e.Handled = true;
    }
  }

  protected internal override void OnMouseButtonsUp(MouseEventArgs e) {
    // default processing (this is now essential as it controls event firing).
    base.OnMouseButtonsUp(e);

    if(e.Button == System.Windows.Forms.MouseButtons.Left) {
      // release our capture on the input data
      ReleaseInput();
      e.Handled = true;
    }
  }

  protected internal override void OnCaptureLost(GuiEventArgs e) {
    // default processing (this is now essential as it controls event firing).
    base.OnCaptureLost(e);

    // reset sizing state
    isBeingSized = false;

    e.Handled = true;
  }

  protected internal override void OnSized(GuiEventArgs e) {
    if(IsRolledUp) {
      // capture changed SizeF(s)
      relOpenSize = relArea.Size;
      absOpenSize = absArea.Size;

      // re-set window SizeF to 0x0
      SizeF nullSize = new SizeF(0, 0);
      absArea.Size = nullSize;
      relArea.Size = nullSize;
    }

    LayoutComponentWidgets();

    // MUST call base class handler no matter what.  This is now required 100%
    base.OnSized(e);
  }

  protected internal override void OnParentSized(WindowEventArgs e) {
    // if we are rolled up we temporarily need to restore the original sizes so
    // that the required calculations can occur when our parent is sized.
    if(IsRolledUp && (this.MetricsMode == MetricsMode.Relative)) {
      relArea.Size = relOpenSize;
      absArea.Size = absOpenSize;
    }

    // default processing (this is now essential as it controls event firing).
    base.OnParentSized(e);
  }


  #endregion Overridden Trigger Methods

  #endregion Events

  #region Event Handlers

  /// <summary>
  ///		When this frame window's close button is clicked, we fire our own event.
  /// </summary>
  /// <param name="sender">Object which fired the event.</param>
  /// <param name="e">Event args.</param>
  protected void closeButton_Clicked(object sender, GuiEventArgs e) {
    OnCloseClicked(new WindowEventArgs(this));
  }

  #endregion Event Handlers
}

} // namespace CeGui.Widgets
