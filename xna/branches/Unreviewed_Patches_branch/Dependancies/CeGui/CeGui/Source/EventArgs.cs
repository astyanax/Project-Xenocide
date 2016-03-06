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

namespace CeGui {

public class GuiEventArgs : EventArgs {
  /// <summary>
  ///		Has this event been handled by the callee?
  /// </summary>
  public bool Handled;
}

/// <summary>
///		GuiEventArgs based class that is used for objects passed to input event handlers
///		concerning mouse input.
/// </summary>
public class MouseEventArgs : GuiEventArgs {
  #region Fields

  /// <summary>
  ///		Holds current mouse position.
  /// </summary>
  public PointF Position;
  /// <summary>
  ///		Holds variation of mouse position from last mouse input.
  /// </summary>
  public PointF MoveDelta;
  /// <summary>
  ///		MouseButtons enum value describing the mouse button causing the event (for button inputs only).
  /// </summary>
  public System.Windows.Forms.MouseButtons Button;
  /// <summary>
  ///		Current state of the system keys and mouse buttons.
  /// </summary>
  public ModifierKeys SysKeys;
  /// <summary>
  ///		Value that the mouse wheel was scrolled.
  /// </summary>
  public int WheelDelta;

  #endregion Fields
}

/// <summary>
///		GuiEventArgs based class that is used for objects passed to handlers triggered for events
///		concerning some Window object.
/// </summary>
public class WindowEventArgs : GuiEventArgs {
  #region Fields

  /// <summary>
  ///		Reference to a Window object of relevance to the event.
  /// </summary>
  public Window Window;

  #endregion Fields

  #region Constructor

  /// <summary>
  ///		Constructor.
  /// </summary>
  /// <param name="window">Window reference relevant to the event.</param>
  public WindowEventArgs(Window window) {
    this.Window = window;
  }

  #endregion Constructor
}

/// <summary>
///		GuiEventArgs based class that is used for objects passed to input event handlers
///		concerning keyboard input.
/// </summary>
public class KeyEventArgs : GuiEventArgs {
  #region Fields

  /// <summary>
  ///		Character representation of the key pressed.
  /// </summary>
  public char Character;
  /// <summary>
  ///		Enum val representing the key pressed.
  /// </summary>
  public System.Windows.Forms.Keys KeyCode;
  /// <summary>
  ///		Special input modifiers.
  /// </summary>
  public ModifierKeys Modifiers;

  #endregion Fields
}

/// <summary>
///		GuiEventArgs based class that is used for objects passed to input event handlers
///		concerning movement of segments within a ListHeader widget.
/// </summary>
public class HeaderSequenceEventArgs : GuiEventArgs {
  #region Fields
  /// <summary>
  /// The original column index of the segment that has moved.
  /// </summary>
  public int OldIndex;

  /// <summary>
  /// The new column index of the segment that has moved.
  /// </summary>
  public int NewIndex;

  #endregion

  #region Constructor
  /// <summary>
  /// Constructor
  /// </summary>
  /// <param name="oldIndex">Index of segment before it was moved.</param>
  /// <param name="newIndex">Index of segment after it has been moved.</param>
  public HeaderSequenceEventArgs(int oldIndex, int newIndex) {
    this.OldIndex = oldIndex;
    this.NewIndex = newIndex;
  }

  #endregion
}

#region Public Delegates

/// <summary>
///		Standard event handler.
/// </summary>
public delegate void GuiEventHandler(object sender, GuiEventArgs e);

/// <summary>
///		Delegate for window event handlers.
/// </summary>
public delegate void WindowEventHandler(object sender, WindowEventArgs e);

/// <summary>
///		Delegate for mouse event handlers.
/// </summary>
public delegate void MouseEventHandler(object sender, MouseEventArgs e);

/// <summary>
///		Delegate for key event handlers.
/// </summary>
public delegate void KeyEventHandler(object sender, KeyEventArgs e);

/// <summary>
///		Delegate for ListHeader segment order change event handlers.
/// </summary>
public delegate void HeaderSequenceEventHandler(object sender, HeaderSequenceEventArgs e);
#endregion Public Delegates

} // namespace CeGui
