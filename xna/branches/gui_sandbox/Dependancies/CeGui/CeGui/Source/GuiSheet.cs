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

/// <summary>
///		Window class intended to be used as a simple GUISheet.
/// </summary>
/// <remarks>
///		This class does no rendering of its own and so appears totally transparent.  This window defaults
///		to position 0.0f, 0.0f with a SizeF of 1.0f x 1.0f - and so covers the entire display.
/// </remarks>
[ AlternateWidgetName("DefaultGUISheet"), AlternateWidgetName("DefaultWindow") ]
public class GuiSheet : Window {
  /// <summary>
  ///		Constructor.
  /// </summary>
  /// <remarks>
  ///	All windows should be created via <see cref="WindowManager.CreateWindow"/>.
  /// </remarks>
  // PDT: Made this public, as I was getting exceptions about access level when trying to create via layouts
  public GuiSheet(string type, string name)
    : base(type, name) {
  }

  /// <summary>
  ///		No implementation necessary.
  /// </summary>
  /// <param name="z"></param>
  protected override void DrawSelf(float z) {
    // do nothing; this is not a visible widget
  }

  /// <summary>
  ///		Initializes the GUI sheet widget.
  /// </summary>
  /// <remarks>
  ///		SizeF is set to full screen.
  /// </remarks>
  public override void Initialize() {
    MaximumSize = new SizeF(1.0f, 1.0f);
    Size = new SizeF(1.0f, 1.0f);
  }

}

} // namespace CeGui
