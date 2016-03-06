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
/// Base class for list header segment widget.
/// </summary>
[ AlternateWidgetName("ListHeaderSegment") ]
public abstract class ListHeaderSegment : Window {
  #region Constants
  /// <summary>
  ///		Default SizeF of the segment sizing area.
  /// </summary>
  const float DefaultSizingAreaSize = 8.0f;

  /// <summary>
  ///		Amount mouse must move before dragging of the segment commences.
  /// </summary>
  const float SegmentMoveThreshold = 12.0f;

  #endregion

  #region Fields
  /// <summary>
  ///		Image to use for mouse when not sizing (typically set by derived class).
  /// </summary>
  protected Image normalMouseCursor;

  /// <summary>
  ///		Image to use for mouse when sizing (typically set by derived class).
  /// </summary>
  protected Image sizingMouseCursor;

  /// <summary>
  ///		Image to use for mouse when moving (typically set by derived class).
  /// </summary>
  protected Image movingMouseCursor;

  /// <summary>
  ///		pixel width of the sizing area.
  /// </summary>
  protected float splitterSize;

  /// <summary>
  ///		true if the mouse is over the splitter
  /// </summary>
  protected bool splitterHover;

  /// <summary>
  ///		true when we are being sized.
  /// </summary>
  protected bool dragSizing;

  /// <summary>
  ///		point we are being dragged at when sizing or moving.
  /// </summary>
  protected PointF dragPoint;

  /// <summary>
  ///		Direction for sorting (used for deciding what icon to display).
  /// </summary>
  protected SortDirection sortingDirection;

  /// <summary>
  ///		true when the mouse is within the segment area (and not in sizing area).
  /// </summary>
  protected bool segmentHover;

  /// <summary>
  ///		true when the left mouse button has been pressed within the confines of the segment.
  /// </summary>
  protected bool segmentPushed;

  /// <summary>
  ///		true when sizing is enabled for this segment.
  /// </summary>
  protected bool sizingEnabled;

  /// <summary>
  ///		True when drag-moving is enabled for this segment;
  /// </summary>
  protected bool movingEnabled;

  /// <summary>
  ///		true when segment is being drag moved.
  /// </summary>
  protected bool dragMoving;

  /// <summary>
  ///		true if the segment can be clicked.
  /// </summary>
  protected bool allowClicks;

  /// <summary>
  ///		position of dragged segment.
  /// </summary>
  protected PointF dragPosition;

  #endregion

  #region Constructor

  /// <summary>
  ///		Constructor.
  /// </summary>
  /// <param name="type"></param>
  /// <param name="name"></param>
  public ListHeaderSegment(string type, string name)
    : base(type, name) {
    normalMouseCursor = null;
    sizingMouseCursor = null;
    movingMouseCursor = null;

    segmentHover = false;
    segmentPushed = false;
    splitterHover = false;
    dragSizing = false;
    dragMoving = false;
    sizingEnabled = true;
    movingEnabled = true;
    allowClicks = true;

    sortingDirection = SortDirection.None;
    splitterSize = DefaultSizingAreaSize;
  }

  #endregion Constructor

  #region Properties

  /// <summary>
  ///		Get/Set whether the segment can be sized by the user.
  /// </summary>
  [WidgetProperty("Sizable")]
  public bool Sizable {
    get {
      return sizingEnabled;
    }

    set {
      if(sizingEnabled != value) {
        sizingEnabled = value;

        // if sizing is now disabled, ensure current sizing operation is cancelled
        if(!sizingEnabled && dragSizing) {
          ReleaseInput();
        }

        OnSizingSettingChanged(new WindowEventArgs(this));
      }

    }
  }

  /// <summary>
  ///		Get/Set the currently set sort direction for the segment.
  /// </summary>
  [SortDirectionProperty("SortDirection")]
  public SortDirection SortDirection {
    get {
      return sortingDirection;
    }

    set {
      if(sortingDirection != value) {
        sortingDirection = value;
        OnSortDirectionChanged(new WindowEventArgs(this));
      }
    }
  }

  /// <summary>
  ///		Get/Set whether the segment can be moved via drag and drop.
  /// </summary>
  [WidgetProperty("Dragable")]
  public bool Draggable {
    get {
      return movingEnabled;
    }

    set {
      if(movingEnabled != value) {
        movingEnabled = value;
        OnMovableSettingChanged(new WindowEventArgs(this));
      }
    }
  }

  /// <summary>
  ///		Get the current dragging offset, which is specified in pixels relative to the
  ///		top-left corner of the segments current location.
  /// </summary>
  public PointF DraggingOffset {
    get {
      return dragPosition;
    }
  }

  /// <summary>
  ///		Get/Set whether the segment may be clicked by the user.
  /// </summary>
  [WidgetProperty("Clickable")]
  public bool Clickable {
    get {
      return allowClicks;
    }

    set {
      if(allowClicks != value) {
        allowClicks = value;
        OnClickableSettingChanged(new WindowEventArgs(this));
      }
    }
  }

  #endregion

  #region Methods
  #endregion

  #region Implementation Members

  #region Properties
  #endregion

  #region Methods
  /// <summary>
  ///		Update state for drag sizing.
  /// </summary>
  /// <param name="localMousePosition">Mouse position as a pixel offset from the top-left corner of this window.</param>
  protected void DoDragSizing(PointF localMousePosition) {
    // calculate sizing delta
    float deltaX = localMousePosition.X - dragPoint.X;

    // limit SizeF to within max & min settings
    float width = absArea.Width;

    if((width + deltaX) < minSize.Width) {
      deltaX = minSize.Width - width;
    } else if((width + deltaX) > maxSize.Width) {
      deltaX = maxSize.Width - width;
    }

    // update window state
    absArea.Right += deltaX;
    dragPoint.X += deltaX;

    relArea = AbsoluteToRelativeImpl(Parent, absArea);

    OnSized(new GuiEventArgs());
    OnSegmentSized(new WindowEventArgs(this));
  }

  /// <summary>
  ///		Update state for drag moving.
  /// </summary>
  /// <param name="localMousePosition">Mouse position as a pixel offset from the top-left corner of this window.</param>
  protected void DoDragMoving(PointF localMousePosition) {
    // calculate movement deltas.
    float deltaX = localMousePosition.X - dragPoint.X;
    float deltaY = localMousePosition.Y - dragPoint.Y;

    // update 'ghost' position
    dragPosition.X += deltaX;
    dragPosition.Y += deltaY;

    // update drag point.
    dragPoint.X += deltaX;
    dragPoint.Y += deltaY;

    OnSegmentDragPositionChanged(new WindowEventArgs(this));
  }

  /// <summary>
  ///		Initialise the required states to put the widget into drag-moving mode.
  /// </summary>
  protected void InitialiseDragMoving() {
    if(movingEnabled) {
      // initialise drag moving state
      dragMoving = true;
      segmentPushed = false;
      segmentHover = false;

      dragPosition.X = 0.0f;
      dragPosition.Y = 0.0f;

      // setup new cursor
      MouseCursor.Instance.SetImage(movingMouseCursor);

      // Trigger the event
      OnSegmentDragStart(new WindowEventArgs(this));
    }

  }

  /// <summary>
  ///		Initialise the required states when we are hovering over the sizing area.
  /// </summary>
  protected void InitialiseSizingHoverState() {
    // only react if settings are changing.
    if(!splitterHover && !segmentPushed) {
      splitterHover = true;

      // change the mouse cursor.
      MouseCursor.Instance.SetImage(sizingMouseCursor);

      // trigger redraw so 'sizing' area can be highlighted if needed.
      RequestRedraw();
    }

    // reset segment hover as needed.
    if(segmentHover) {
      segmentHover = false;
      RequestRedraw();
    }

  }

  /// <summary>
  ///		Initialise the required states when we are hovering over the main segment area.
  /// </summary>
  protected void InitialiseSegmentHoverState() {
    // reset sizing area hover state if needed.
    if(splitterHover) {
      splitterHover = false;
      MouseCursor.Instance.SetImage(normalMouseCursor);
      RequestRedraw();
    }

    // set segment hover state if not already set.
    if((!segmentHover) && Clickable) {
      segmentHover = true;
      RequestRedraw();
    }
  }

  /// <summary>
  ///		Return whether the required minimum movement threshold before initiating
  ///		drag-moving has been exceeded.
  /// </summary>
  /// <param name="localMousePosition">Mouse position as a pixel offset from the top-left corner of this window.</param>
  /// <returns>
  ///		- true if the threshold has been exceeded and drag-moving should be initiated
  ///		- false if the threshold has not been exceeded.
  /// </returns>
  protected bool IsDragMoveThresholdExceeded(PointF localMousePosition) {
    // see if mouse has moved far enough to start move operation
    // calculate movement deltas.
    float deltaX = localMousePosition.X - dragPoint.X;
    float deltaY = localMousePosition.Y - dragPoint.Y;

    if((deltaX > SegmentMoveThreshold) || (deltaX < -SegmentMoveThreshold) ||
        (deltaY > SegmentMoveThreshold) || (deltaY < -SegmentMoveThreshold)) {
      return true;
    } else {
      return false;
    }

  }

  #endregion

  #endregion

  #region Events

  #region Event Declarations

  /// <summary>
  ///		The segment was clicked.
  /// </summary>
  public event WindowEventHandler SegmentClicked;

  /// <summary>
  ///		The sizer for the segment was double clicked.
  /// </summary>
  public event WindowEventHandler SplitterDoubleClicked;

  /// <summary>
  ///		The 'sizable' setting for the segment has changed.
  /// </summary>
  public event WindowEventHandler SizingSettingChanged;

  /// <summary>
  ///		The sort direction of the segment has changed.
  /// </summary>
  public event WindowEventHandler SortDirectionChanged;

  /// <summary>
  ///		The 'movable' setting for the segment has changed.
  /// </summary>
  public event WindowEventHandler MovableSettingChanged;

  /// <summary>
  ///		The user has started dragging the segment.
  /// </summary>
  public event WindowEventHandler SegmentDragStart;

  /// <summary>
  ///		The user has stopped dragging the segment.
  /// </summary>
  public event WindowEventHandler SegmentDragStop;

  /// <summary>
  ///		The dragging position of the segment has changed.
  /// </summary>
  public event WindowEventHandler SegmentDragPositionChanged;

  /// <summary>
  ///		The segment has been re-sized.
  /// </summary>
  public event WindowEventHandler SegmentSized;

  /// <summary>
  ///		The 'clickable' setting for the segment has changed.
  /// </summary>
  public event WindowEventHandler ClickableSettingChanged;


  #endregion Event Declarations

  #region Trigger Methods

  /// <summary>
  ///		Handler invoked internally when segment is clicked.
  /// </summary>
  /// <param name="e">Events args.</param>
  protected internal void OnSegmentClicked(WindowEventArgs e) {
    if(SegmentClicked != null) {
      SegmentClicked(this, e);
    }
  }

  /// <summary>
  ///		Handler invoked internally when segment sizer is double clicked.
  /// </summary>
  /// <param name="e">Events args.</param>
  protected internal void OnSplitterDoubleClicked(WindowEventArgs e) {
    if(SplitterDoubleClicked != null) {
      SplitterDoubleClicked(this, e);
    }
  }

  /// <summary>
  ///		Handler invoked internally when the sizable setting is changed.
  /// </summary>
  /// <param name="e">Events args.</param>
  protected internal void OnSizingSettingChanged(WindowEventArgs e) {
    if(SizingSettingChanged != null) {
      SizingSettingChanged(this, e);
    }
  }

  /// <summary>
  ///		Handler invoked internally when the sort direction changes.
  /// </summary>
  /// <param name="e">Events args.</param>
  protected internal void OnSortDirectionChanged(WindowEventArgs e) {
    RequestRedraw();

    if(SortDirectionChanged != null) {
      SortDirectionChanged(this, e);
    }
  }

  /// <summary>
  ///		Handler invoked internally when the movable setting is changed.
  /// </summary>
  /// <param name="e">Events args.</param>
  protected internal void OnMovableSettingChanged(WindowEventArgs e) {
    if(MovableSettingChanged != null) {
      MovableSettingChanged(this, e);
    }
  }

  /// <summary>
  ///		Handler invoked internally when the user begins dragging the segment.
  /// </summary>
  /// <param name="e">Events args.</param>
  protected internal void OnSegmentDragStart(WindowEventArgs e) {
    if(SegmentDragStart != null) {
      SegmentDragStart(this, e);
    }
  }

  /// <summary>
  ///		Handler invoked internally when the user stops dragging the segment.
  /// </summary>
  /// <param name="e">Events args.</param>
  protected internal void OnSegmentDragStop(WindowEventArgs e) {
    if(SegmentDragStop != null) {
      SegmentDragStop(this, e);
    }
  }

  /// <summary>
  ///		Handler invoked internally when dragged position of the segment changes.
  /// </summary>
  /// <param name="e">Events args.</param>
  protected internal void OnSegmentDragPositionChanged(WindowEventArgs e) {
    RequestRedraw();

    if(SegmentDragPositionChanged != null) {
      SegmentDragPositionChanged(this, e);
    }
  }

  /// <summary>
  ///		Handler invoked internally when the segment is sized.
  /// </summary>
  /// <param name="e">Events args.</param>
  protected internal void OnSegmentSized(WindowEventArgs e) {
    RequestRedraw();

    if(SegmentSized != null) {
      SegmentSized(this, e);
    }
  }

  /// <summary>
  ///		Handler invoked internally when the clickable setting changes.
  /// </summary>
  /// <param name="e">Events args.</param>
  protected internal void OnClickableSettingChanged(WindowEventArgs e) {
    if(ClickableSettingChanged != null) {
      ClickableSettingChanged(this, e);
    }
  }

  #endregion Trigger Methods

  #endregion Events

  #region Window Members

  #region Overridden Event Trigger Methods

  protected internal override void OnMouseMove(MouseEventArgs e) {
    // default processing
    base.OnMouseMove(e);

    // convert mouse position to window local co-ordinates
    PointF localMousePosition = ScreenToWindow(e.Position);

    // convert to pixels as needed.
    if(MetricsMode == MetricsMode.Relative) {
      localMousePosition = RelativeToAbsolute(localMousePosition);
    }

    // handle drag sizing
    if(dragSizing) {
      DoDragSizing(localMousePosition);
    }
      // handle drag moving
  else if(dragMoving) {
      DoDragMoving(localMousePosition);
    }
      // not sizing or moving, is mouse in the widget area?
  else if(IsHit(e.Position)) {
      // mouse in sizing area & sizing is enabled
      if((localMousePosition.X > (AbsoluteWidth - splitterSize)) && sizingEnabled) {
        InitialiseSizingHoverState();
      }
        // mouse not in sizing area and/or sizing not enabled
    else {
        InitialiseSegmentHoverState();

        // if we are pushed but not yet drag moving
        if(segmentPushed && !dragMoving) {
          if(IsDragMoveThresholdExceeded(localMousePosition)) {
            InitialiseDragMoving();
          }
        }
      }
    }
      // mouse is no longer within the widget area...
else {
      // only change settings if change is required
      if(splitterHover) {
        splitterHover = false;
        MouseCursor.Instance.SetImage(normalMouseCursor);
        RequestRedraw();
      }

      // reset segment hover state if not already done.
      if(segmentHover) {
        segmentHover = false;
        RequestRedraw();
      }
    }

    e.Handled = true;
  }

  protected internal override void OnMouseButtonsDown(MouseEventArgs e) {
    // default processing
    base.OnMouseButtonsDown(e);

    if(e.Button == System.Windows.Forms.MouseButtons.Left) {
      // ensure all inputs come to us for now
      CaptureInput();

      // get position of mouse as co-ordinates local to this window.
      PointF localPosition = ScreenToWindow(e.Position);

      if(MetricsMode == MetricsMode.Relative) {
        localPosition = RelativeToAbsolute(localPosition);
      }

      // store drag point for possible sizing or moving operation.
      dragPoint = localPosition;

      // if the mouse is in the sizing area
      if(splitterHover) {
        if(Sizable) {
          // setup the 'dragging' state variables
          dragSizing = true;
        }
      } else {
        segmentPushed = true;
      }

      e.Handled = true;
    }
  }

  protected internal override void OnMouseButtonsUp(MouseEventArgs e) {
    // default processing
    base.OnMouseButtonsUp(e);

    if(e.Button == System.Windows.Forms.MouseButtons.Left) {
      // if we were pushed and mouse was released within our segment area
      if(segmentPushed && segmentHover) {
        OnSegmentClicked(new WindowEventArgs(this));
      } else if(dragMoving) {
        MouseCursor.Instance.SetImage(normalMouseCursor);
        OnSegmentDragStop(new WindowEventArgs(this));
      }

      // release our capture on the input data
      ReleaseInput();
      e.Handled = true;
    }
  }

  protected internal override void OnMouseDoubleClicked(MouseEventArgs e) {
    // default processing
    base.OnMouseDoubleClicked(e);

    // if double-clicked on splitter / sizing area
    if((e.Button == System.Windows.Forms.MouseButtons.Left) && splitterHover) {
      OnSplitterDoubleClicked(new WindowEventArgs(this));
      e.Handled = true;
    }
  }

  protected internal override void OnMouseLeaves(MouseEventArgs e) {
    // default processing
    base.OnMouseLeaves(e);

    splitterHover = false;
    dragSizing = false;
    segmentHover = false;
    RequestRedraw();
  }

  protected internal override void OnCaptureLost(GuiEventArgs e) {
    // default processing
    base.OnCaptureLost(e);

    // reset segment state
    dragSizing = false;
    segmentPushed = false;
    dragMoving = false;

    e.Handled = true;
  }

  #endregion Overridden Event Trigger Methods

  #endregion Window Members
}

} // namespace CeGui.Widgets
