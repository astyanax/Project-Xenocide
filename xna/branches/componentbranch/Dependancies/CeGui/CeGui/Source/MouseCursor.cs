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
using System.Text;
using System.Drawing;

namespace CeGui {

/// <summary>
///		Class that allows access to the GUI system mouse cursor.
/// </summary>
/// <remarks>
///		The MouseCursor provides functionality to access the position and imagery of the 
///		mouse cursor / pointer.
/// </remarks>
/// In sync with original version as of:
/// .h:		1.2
/// .cpp:	1.2 
public class MouseCursor {
  #region Fields

  /// <summary>
  ///		Image that is currently set as the mouse cursor.
  /// </summary>
  protected Image cursorImage;
  /// <summary>
  ///		Current location of the cursor.
  /// </summary>
  protected Vector3 position;
  /// <summary>
  ///		True if the cursor will be drawn, else false.
  /// </summary>
  protected bool isVisible;
  /// <summary>
  ///		Specifies the area (in screen pixels) that the mouse can move around in.
  /// </summary>
  protected Rect constraints;

  #endregion Fields

  #region Singleton Implementation

  /// <summary>
  ///		Singlton instance of this class.
  /// </summary>
  private static MouseCursor instance;

  /// <summary>
  ///		Default constructor.
  /// </summary>
  internal MouseCursor() {
    // only create once instance
    if(instance == null) {
      instance = this;

      // default constraint is the whole screen
      constraints = GuiSystem.Instance.Renderer.Rect;

      // mouse default to middle of the constrained area
      position.x = constraints.Width * 0.5f;
      position.y = constraints.Height * 0.5f;
      position.z = 1.0f;

      // mouse defaults to visible
      isVisible = true;

      Logger.Instance.LogEvent("MouseCursor singleton created.");
    }
  }

  /// <summary>
  ///		Gets the singleton class instance.
  /// </summary>
  /// <value></value>
  public static MouseCursor Instance {
    get {
      return instance;
    }
  }

  #endregion Singleton Implementation

  #region Public Properties

  /// <summary>
  ///		Gets/Sets the current constraint area of the mouse cursor.
  /// </summary>
  /// <value><see cref="Rect"/> object describing the active area that the mouse cursor is constrained to.</value>
  public Rect ConstraintArea {
    get {
      return constraints;
    }
    set {
      constraints = value;
    }
  }

  /// <summary>
  ///		Returns whether the mouse cursor is visible.
  /// </summary>
  /// <value>True if the cursor is visible, false otherwise.</value>
  public bool IsVisible {
    get {
      return isVisible;
    }
  }

  /// <summary>
  ///		Gets/Sets the current mouse cursor position.
  /// </summary>
  /// <value><see cref="Point"/> object describing the mouse cursor position.</value>
  public PointF Position {
    get {
      return new PointF(position.x, position.y);
    }
    set {
      position.x = value.X;
      position.y = value.Y;

      ConstrainPosition();
    }
  }

  #endregion Public Properties

  #region Public Methods

  /// <summary>
  ///		Makes the cursor draw itself.
  /// </summary>
  public void Draw() {
    if(isVisible && (cursorImage != null)) {
      // TODO: Add overload with default color Rect
      cursorImage.Draw(
          position,
          GuiSystem.Instance.Renderer.Rect,
          new ColourRect());
    }
  }

  /// <summary>
  ///		Hides the mouse cursor.
  /// </summary>
  public void Hide() {
    isVisible = false;
  }

  /// <summary>
  ///		Offset the mouse cursor position by the deltas specified in <paramref name="offset"/>.
  /// </summary>
  /// <param name="offset">Point which describes the amount to move the cursor in each axis.</param>
  public void OffsetPosition(PointF offset) {
    position.x += offset.X;
    position.y += offset.Y;

    ConstrainPosition();
  }

  /// <summary>
  ///		Set the current mouse cursor image.
  /// </summary>
  /// <param name="imagesetName">
  ///		The name of the <see cref="Imageset"/> that contains the desired <see cref="Image"/>.
  /// </param>
  /// <param name="imageName">Name of a desired image within the specified image set.</param>
  /// <exception cref="UnknownObjectException">
  ///		Thrown if <paramref name="imagesetName"/> is not known, or if <paramref name="imagesetName"/> 
  ///		contains no <see cref="Image"/> named <paramref name="imageName"/>.
  ///	</exception>
  public void SetImage(string imagesetName, string imageName) {
    cursorImage = ImagesetManager.Instance.GetImageset(imagesetName).GetImage(imageName);
  }

  /// <summary>
  ///		Set the current mouse cursor image.
  /// </summary>
  /// <param name="image">Reference to an existing <see cref="Image"/> object to use.</param>
  public void SetImage(Image image) {
    cursorImage = image;
  }

  /// <summary>
  ///		Shows the mouse cursor.
  /// </summary>
  public void Show() {
    isVisible = true;
  }

  #endregion Public Methods

  #region Private Methods

  /// <summary>
  ///		Checks the mouse cursor position is within the current 'constrain' Rect and adjusts as required.
  /// </summary>
  private void ConstrainPosition() {
    if(position.x >= constraints.Right) {
      position.x = constraints.Right - 1;
    }

    if(position.y >= constraints.Bottom) {
      position.y = constraints.Bottom - 1;
    }

    if(position.y < constraints.Top) {
      position.y = constraints.Top;
    }

    if(position.x < constraints.Left) {
      position.x = constraints.Left;
    }
  }

  #endregion Private Methods
}

} // namespace CeGui