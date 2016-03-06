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
/// Summary description for ComboDropList.
/// </summary>
[ AlternateWidgetName("ComboDropList") ]
public abstract class ComboDropList : Listbox {
  #region Constructor

  /// <summary>
  ///		Constructor.
  /// </summary>
  /// <param name="name">Name of this widget.</param>
  public ComboDropList(string type, string name)
    : base(type, name) {
    // hidden by default
    Hide();
  }

  #endregion Constructor

  #region Window Members

  #region Methods

  /// <summary>
  ///		Prepares this widget for use.
  /// </summary>
  public override void Initialize() {
    base.Initialize();

    // set-up scroll bars so they return capture to us.
    vertScrollbar.SetRestoreCapture(true);
    horzScrollbar.SetRestoreCapture(true);
  }

  #endregion Methods

  #endregion Window Members

  #region Events

  #region Declarations

  public event WindowEventHandler ListSelectionAccepted;

  #endregion Declarations

  #region Trigger Methods

  /// <summary>
  ///		Handles an item in the list being selected.
  /// </summary>
  /// <param name="e"></param>
  protected internal void OnListSelectionAccepted(WindowEventArgs e) {
    if(ListSelectionAccepted != null) {
      ListSelectionAccepted(this, e);
    }
  }

  #endregion Trigger Methods

  #region Overridden Trigger Methods

  /// <summary>
  ///		Handles mouse movement within this widget.
  /// </summary>
  /// <param name="e"></param>
  protected internal override void OnMouseMove(MouseEventArgs e) {
    base.OnMouseMove(e);

    // remove current selection
    ClearAllSelectionsImpl();

    // if mouse is within our area (but not children)
    if(IsHit(e.Position) && GetChildAtPosition(e.Position) == null) {
      // Convert mouse position to absolute window pixels
      PointF localPos = ScreenToWindow(e.Position);

      if(this.MetricsMode == MetricsMode.Relative) {
        localPos = RelativeToAbsolute(localPos);
      }

      // check for an item under the mouse
      ListboxItem selectedItem = GetItemAtPoint(localPos);

      // if an item is under the mouse, select it
      if(selectedItem != null) {
        SetItemSelectState(selectedItem, true);
      }

      e.Handled = true;
    } else {
      // TODO: C++ version doesnt have this in an else, seems like value will never be true!
      e.Handled = false;
    }
  }

  /// <summary>
  ///		Handler for mouse button pressed events.
  /// </summary>
  /// <param name="e"></param>
  protected internal override void OnMouseButtonsDown(MouseEventArgs e) {
    base.OnMouseButtonsDown(e);

    if(e.Button == System.Windows.Forms.MouseButtons.Left) {
      if(!IsHit(e.Position)) {
        ReleaseInput();
      }

      e.Handled = true;
    }
  }

  /// <summary>
  ///		Handler for mouse button release events.
  /// </summary>
  /// <param name="e"></param>
  protected internal override void OnMouseButtonsUp(MouseEventArgs e) {
    base.OnMouseButtonsUp(e);

    if(e.Button == System.Windows.Forms.MouseButtons.Left) {
      ReleaseInput();

      // if something was selected, confirm that selection
      if(SelectedCount > 0) {
        OnListSelectionAccepted(new WindowEventArgs(this));
      }

      e.Handled = true;
    }
  }

  /// <summary>
  ///		Handler for when input capture is lost.
  /// </summary>
  /// <param name="e"></param>
  protected internal override void OnCaptureLost(GuiEventArgs e) {
    base.OnCaptureLost(e);

    Hide();

    e.Handled = true;
  }

  /// <summary>
  ///		Handler for when window is activated.
  /// </summary>
  /// <param name="e"></param>
  protected internal override void OnActivated(WindowEventArgs e) {
    base.OnActivated(e);
  }

  #endregion Overridden Trigger Methods

  #endregion Events
}

} // namespace CeGui.Widgets
