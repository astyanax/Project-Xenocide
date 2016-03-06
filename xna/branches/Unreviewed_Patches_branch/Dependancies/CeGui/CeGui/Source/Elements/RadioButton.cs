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

namespace CeGui.Widgets {

/// <summary>
/// Summary description for RadioButton.
/// </summary>
/// C++ Version Sync
/// .cpp:	1.4
/// .h:		1.2
[ AlternateWidgetName("RadioButton") ]
public abstract class RadioButton : BaseButton {
  #region Fields

  /// <summary>
  ///		true if radio button is selected (has checkmark).
  /// </summary>
  protected bool isChecked;
  /// <summary>
  ///		Radio button group ID.
  /// </summary>
  protected int groupID;

  #endregion Fields

  #region Constructor

  /// <summary>
  ///		Constructor.
  /// </summary>
  /// <param name="type"></param>
  /// <param name="name"></param>
  public RadioButton(string type, string name)
    : base(type, name) {
  }

  #endregion Constructor

  #region Properties

  /// <summary>
  ///		Get/Set the checked state of the checkbox.
  /// </summary>
  /// <value>
  ///		true to select the widget and give it the checkmark.  
  ///		false to deselect the widget and remove the checkmark.
  /// </value>
  [WidgetProperty("Selected")]
  public bool Checked {
    get {
      return isChecked;
    }
    set {
      if(value != isChecked) {
        isChecked = value;

        // if new state is 'checked', de-select any other buttons in our group
        if(isChecked) {
          DeselectOtherButtonsInGroup();
        }

        // event notification
        OnCheckStateChanged(new WindowEventArgs(this));
      }
    }
  }

  /// <summary>
  ///		Get/Set the group ID for this radio button.
  /// </summary>
  /// <value>Integer value that holds the group ID which this radio button belongs to.</value>
  [WidgetProperty("GroupID")]
  public int GroupID {
    get {
      return groupID;
    }
    set {
      groupID = value;

      // if new state is 'checked', de-select any other buttons in our group
      if(isChecked) {
        DeselectOtherButtonsInGroup();
      }
    }
  }

  #endregion Properties

  #region Methods

  /// <summary>
  ///		Deselect any selected radio buttons attached to the same parent within the same group
  ///		(but not do not deselect 'this').
  /// </summary>
  protected void DeselectOtherButtonsInGroup() {
    // nothing to do unless we are attached to another window
    if(parent != null) {
      int childCount = parent.ChildCount;

      // scan all children
      for(int i = 0; i < childCount; i++) {
        Window child = parent.GetChildAtIndex(i);

        if(child is RadioButton) {
          RadioButton button = (RadioButton)child;

          // if it is enabled, is not this, and belongs to our group..
          if(button.Checked && button != this && button.GroupID == groupID) {
            // .. disable it
            button.Checked = false;
          }
        }
      }
    }
  }

  #endregion Methods

  #region Events

  #region Event Declarations

  /// <summary>
  ///		Event triggered internally when state of check-box changes.
  /// </summary>
  public event WindowEventHandler CheckStateChanged;

  #endregion Event Declarations

  #region Trigger Methods

  /// <summary>
  ///		Event triggered internally when state of checkbox changes.
  /// </summary>
  /// <param name="e">Event arguments.</param>
  protected internal void OnCheckStateChanged(WindowEventArgs e) {
    // redraw the widget
    RequestRedraw();

    if(CheckStateChanged != null) {
      CheckStateChanged(this, e);
    }
  }

  #endregion Trigger Methods

  #region Overridden Trigger Methods

  /// <summary>
  ///		When the mouse up occurs, we change the selected state.
  /// </summary>
  /// <param name="e"></param>
  protected internal override void OnMouseButtonsUp(MouseEventArgs e) {
    if((e.Button == System.Windows.Forms.MouseButtons.Left) && IsPushed) {
      Window sheet = GuiSystem.Instance.GuiSheet;

      if(sheet != null) {
        // if mouse was released over this widget
        if(this == sheet.GetChildAtPosition(e.Position)) {
          // toggle selected state, which will deselect all other radio buttons in our group
          if(!isChecked) {
            Checked = true;
          }
        }
      }

      // we handled this one
      e.Handled = true;
    }

    // default processing
    base.OnMouseButtonsUp(e);
  }

  #endregion Overridden Trigger Methods

  #endregion Events
}

} // namespace CeGui.Widgets
