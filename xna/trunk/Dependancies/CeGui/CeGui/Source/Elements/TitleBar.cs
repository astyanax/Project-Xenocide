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
///		Class representing the title bar for Frame Windows.
/// </summary>
/// C++ Version Sync
/// .cpp:	1.5
/// .h:		1.4
[ AlternateWidgetName("TitleBar") ]
public abstract class TitleBar : Window {
  #region Fields

  /// <summary>
  ///		true when the window is being dragged.
  /// </summary>
  protected bool isDragging;
  /// <summary>
  ///		Point at which we are being dragged.
  /// </summary>
  protected PointF dragPoint;
  /// <summary>
  ///		Used to backup cursor restraint area.
  /// </summary>
  protected Rect oldCursorArea;
  /// <summary>
  ///		true when dragging for the widget is enabled.
  /// </summary>
  protected bool isDraggingEnabled;

  #endregion Fields

  #region Constructor

  public TitleBar(string type, string name)
    : base(type, name) {
    // the title bar will always be on top of the frame window
    AlwaysOnTop = true;
  }

  #endregion Constructor

  #region Properties

  /// <summary>
  ///		Get/Set whether this title bar will respond to dragging.
  /// </summary>
  /// <value>true if the title bar will respond to dragging, false if the title bar will not respond.</value>
  [WidgetProperty("DraggingEnabled")]
  public bool DraggingEnabled {
    get {
      return isDraggingEnabled;
    }
    set {
      if(isDraggingEnabled != value) {
        isDraggingEnabled = value;

        // stop dragging now if the setting has been disabled.
        if((!isDraggingEnabled) && isDragging) {
          ReleaseInput();
        }

        // call event handler.
        OnDraggingModeChanged(new WindowEventArgs(this));
      }
    }
  }

  #endregion Properties

  #region Window Members

  #region Overridden Event Trigger Methods

  protected internal override void OnMouseMove(MouseEventArgs e) {
    // default processing
    base.OnMouseMove(e);

    if(isDragging && (parent != null)) {
      PointF delta = ScreenToWindow(e.Position);

      if(this.MetricsMode == MetricsMode.Relative) {
        delta = RelativeToAbsolute(delta);
      }

      // calculate amount that window has been moved
      delta.X -= dragPoint.X;
      delta.Y -= dragPoint.Y;

      // move the window.
      // Important: TitleBar objects should only be attached to a FrameWindow
      ((FrameWindow)parent).OffsetPixelPosition(delta);

      e.Handled = true;
    }
  }

  protected internal override void OnMouseButtonsDown(MouseEventArgs e) {
    // default processing
    base.OnMouseButtonsDown(e);

    if(e.Button == System.Windows.Forms.MouseButtons.Left) {
      if((parent != null) && isDraggingEnabled) {
        // we want all mouse input from now on
        CaptureInput();

        // initialize the dragging state
        isDragging = true;
        dragPoint = ScreenToWindow(e.Position);

        if(this.MetricsMode == MetricsMode.Relative) {
          dragPoint = RelativeToAbsolute(dragPoint);
        }

        // store old constraint area
        oldCursorArea = MouseCursor.Instance.ConstraintArea;

        Rect newRestraintArea;

        // setup new constraint area to be the intersection of the old area and our grand-parent's clipped inner-area
        if((parent == null) || parent.Parent == null) {
          newRestraintArea = GuiSystem.Instance.Renderer.Rect.GetIntersection(oldCursorArea);
        } else {
          newRestraintArea = parent.Parent.InnerRect.GetIntersection(oldCursorArea);
        }

        MouseCursor.Instance.ConstraintArea = newRestraintArea;
      }

      e.Handled = true;
    }
  }

  protected internal override void OnMouseButtonsUp(MouseEventArgs e) {
    // default processing
    base.OnMouseButtonsUp(e);

    if(e.Button == System.Windows.Forms.MouseButtons.Left) {
      ReleaseInput();
      e.Handled = true;
    }
  }

  protected internal override void OnMouseDoubleClicked(MouseEventArgs e) {
    // default processing
    base.OnMouseDoubleClicked(e);

    if(e.Button == System.Windows.Forms.MouseButtons.Left) {
      // if we do not have a parent window, then obviously nothing should happen.
      if(parent != null) {
        // we should only ever be attached to a FrameWindow (or derived) class
        ((FrameWindow)parent).ToggleRollup();
      }

      e.Handled = true;
    }
  }

  protected internal override void OnCaptureLost(GuiEventArgs e) {
    // default processing
    base.OnCaptureLost(e);

    // when we lose out hold on the mouse inputs, we are no longer dragging.
    isDragging = false;

    // restore old constraint area
    MouseCursor.Instance.ConstraintArea = oldCursorArea;
  }

  #endregion Overridden Event Trigger Methods

  #endregion Window Members

  #region Events

  /// <summary>
  ///		Trigger for when the dragging mode of the window is changed.
  /// </summary>
  /// <param name="e">Event args.</param>
  protected internal void OnDraggingModeChanged(WindowEventArgs e) {
    // TODO: Implementation
  }

  #endregion Events
}

} // namespace CeGui.Widgets
