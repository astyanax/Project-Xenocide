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
/// Summary description for PushButton.
/// </summary>
[ AlternateWidgetName("PushButton") ]
public abstract class PushButton : BaseButton {
  #region Constructor

  /// <summary>
  ///		Constructor.
  /// </summary>
  /// <param name="type"></param>
  /// <param name="name"></param>
  public PushButton(string type, string name) : base(type, name) { }

  #endregion Constructor

  #region Events

  #region Event Declarations

  /// <summary>
  ///		The button was clicked.
  /// </summary>
  public event GuiEventHandler Clicked;

  #endregion Event Declarations

  #region Trigger Methods

  /// <summary>
  ///		Handler invoked internally when the button is clicked.
  /// </summary>
  /// <param name="e">Events args.</param>
  protected internal void OnClicked(WindowEventArgs e) {
    if(Clicked != null) {
      Clicked(this, e);
    }
  }

  #endregion Trigger Methods

  #endregion Events

  #region Window Members

  #region Overridden Event Trigger Methods

  protected internal override void OnMouseButtonsUp(MouseEventArgs e) {
    // default processing
    base.OnMouseButtonsUp(e);

    Window sheet = GuiSystem.Instance.GuiSheet;

    if(sheet != null) {
      // if mouse was released over this widget
      if(this == sheet.GetChildAtPosition(e.Position)) {
        // fire event
        OnClicked(new WindowEventArgs(this));
      }
    }

    // we took care of this event
    e.Handled = true;
  }

  #endregion Overridden Event Trigger Methods

  #endregion Window Members
}

} // namespace CeGui.Widgets
