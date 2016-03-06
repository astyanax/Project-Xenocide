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
///	  This class is the CEGUI class that provides access to all other elements in this system.
/// </summary>
/// <remarks>
///	  This object must be created by the client application. The GuiSystem object requires
///   that you pass it an initialized <see cref="Renderer"/> object which it can use to
///   interface to whatever rendering system will be used to display the GUI imagery.
/// </remarks>
public class GuiSystem {

  #region Constants

  /// <summary>
  ///   Default timeout for generation of single click events.
  /// </summary>
  const float DefaultSingleClickTimeout = 0.2f;
  /// <summary>
  ///   Default timeout for generation of multi-click events.
  /// </summary>
  const float DefaultMultiClickTimeout = 0.33f;
  /// <summary>
  ///   Default allowable mouse movement for multi-click event generation.
  /// </summary>
  public readonly SizeF DefaultMultiClickAreaSize = new SizeF(12, 12);

  #endregion Constants

  #region Fields

  /// <summary>
  ///   The active GUI sheet (root window)
  /// </summary>
  protected Window activeSheet;
  /// <summary>
  ///   Reference to the window that currently contains the mouse.
  /// </summary>
  protected Window windowWithMouse;
  /// <summary>
  ///   True if GUI should be re-drawn, false if render should re-use last times queue.
  /// </summary>
  protected bool guiRedraw;
  /// <summary>
  ///   Holds a reference to the default GUI font.
  /// </summary>
  protected Font defaultFont;
  /// <summary>
  ///   Holds a reference to the default mouse cursor.
  /// </summary>
  protected Image defaultMouseCursor;
  /// <summary>
  ///   Holds the reference to the Renderer object given to us in the constructor.
  /// </summary>
  protected Renderer renderer;

  /// <summary>
  ///   Timeout value, in seconds, used to generate a single-click (button down then up).
  /// </summary>
  protected float clickTimeout;
  /// <summary>
  ///   Timeout value, in seconds, used to generate multi-click events (botton down, then up, then down, and so on).
  /// </summary>
  protected float doubleClickTimeout;
  /// <summary>
  ///   SizeF of area the mouse can move and still make multi-clicks.
  /// </summary>
  protected SizeF doubleClickSize;
  /*
  /// <summary>
  ///		Structs used to keep track of mouse button click generation.
  /// </summary>
  protected MouseClickTracker[] clickTrackers = new MouseClickTracker[InputUtil.MouseButtonCount];
  */
  protected MouseClickTracker leftMouseButton = new MouseClickTracker();
  protected MouseClickTracker rightMouseButton = new MouseClickTracker();
  protected MouseClickTracker middleMouseButton = new MouseClickTracker();

  public MouseClickTracker GetClickTracker(System.Windows.Forms.MouseButtons button) {
    switch(button) {
      case System.Windows.Forms.MouseButtons.Left:
        return leftMouseButton;
      case System.Windows.Forms.MouseButtons.Right:
        return rightMouseButton;
      case System.Windows.Forms.MouseButtons.Middle:
        return middleMouseButton;
      default:
        throw new NotSupportedException();
    }
  }
  /// <summary>System keys that are currently pressed</summary>
  protected ModifierKeys sysKeys;

  /// <summary>State of the left control key</summary>
  protected bool leftCtrl;
  /// <summary>State of the left shift key</summary>
  protected bool leftShift;
  /// <summary>State of the right control key</summary>
  protected bool rightCtrl;
  /// <summary>State of the right shift key</summary>
  protected bool rightShift;

  #endregion Fields

  #region Singleton Implementation

  /// <summary>Singleton instance of this class</summary>
  private static GuiSystem instance;

  public static void Initialize(Renderer renderer) {
    new GuiSystem(renderer);
  }

  /// <summary>
  /// Default constructor.
  /// </summary>
  /// <param name="renderer">Reference to the valid Renderer object that will be used to render GUI imagery</param>
  public GuiSystem(Renderer renderer) {
    // only create once instance
    if(instance == null) {
      // store off the renderer instance
      this.renderer = renderer;

      // mouse input timing defaults
      clickTimeout = DefaultSingleClickTimeout;
      doubleClickTimeout = DefaultMultiClickTimeout;
      doubleClickSize = DefaultMultiClickAreaSize;

      /*
      for(int i = 0; i < clickTrackers.Length - 1; i++) {
          clickTrackers[i] = new MouseClickTracker();
      }
      */

      instance = this;

      // first thing to do is create logger
      new Logger("CEGUI.log");

      Logger.Instance.LogEvent("---- Begining CEGUI System initialisation ----");

      // cause creation of other singleton objects
      new ImagesetManager();
      new FontManager();
      new WindowManager();
      new SchemeManager();
      new MouseCursor();

      // register the GuiSheet widget by default since it is part of the core
      // njk-patch
      //				WindowManager.Instance.RegisterType(typeof(GuiSheet), GetType().Assembly);
      //				WindowManager.Instance.RegisterType(typeof(Widgets.StaticImage), GetType().Assembly);
    }
  }

  /// <summary>
  ///		Gets the singleton class instance.
  /// </summary>
  /// <value></value>
  public static GuiSystem Instance {
    get {
      return instance;
    }
  }

  #endregion Singleton Implementation

  #region Properties

  /// <summary>
  ///		Return a reference to the default <see cref="Font"/> for the GUI system.
  /// </summary>
  /// <value>Reference to a <see cref="Font"/> object that is the default font in the system.</value>
  public Font DefaultFont {
    get {
      return defaultFont;
    }
  }

  /// <summary>
  ///		Return the currently set default mouse cursor image.
  /// </summary>
  /// <value>
  ///		Reference to the current default image used for the mouse cursor.  May return null if default cursor has not been set,
  ///		or has intentionally been set to NULL - which results in a blank default cursor.
  /// </value>
  public Image DefaultMouseCursor {
    get {
      return defaultMouseCursor;
    }
  }

  /// <summary>
  ///		Gets/Sets the active GUI sheet (root) window.
  /// </summary>
  /// <value>Reference to the <see cref="Window"/> object that has been set as the GUI root element.</value>
  public Window GuiSheet {
    get {
      return activeSheet;
    }
    set {
      activeSheet = value;
    }
  }

  /// <summary>
  ///		Gets a reference to the <see cref="Renderer"/> object being used by the system.
  /// </summary>
  /// <value>Reference to the <see cref="Renderer"/> object being used by the system.</value>
  public Renderer Renderer {
    get {
      return renderer;
    }
  }

  /// <summary>
  ///		Return the Window object that the mouse is presently within.
  /// </summary>
  /// <value>Reference to the Window object that currently contains the mouse cursor, or 'null' if none.</value>
  public Window WindowContainingMouse {
    get {
      return windowWithMouse;
    }
  }

  #endregion Properties

  #region Methods

  /// <summary>
  ///		Render the GUI.
  /// </summary>
  /// <remarks>
  ///		Depending upon the internal state, this may either re-use rendering from last time, 
  ///		or trigger a full re-draw from all elements.
  /// </remarks>
  public void RenderGui() {
    //////////////////////////////////////////////////////////////////////////
    // This makes use of some tricks the Renderer can do so that we do not
    // need to do a full redraw every frame - only when some UI element has
    // changed.
    //
    // Since the mouse is likely to move very often, and in order not to
    // short-circuit the above optimisation, the mouse is not queued, but is
    // drawn directly to the display every frame.
    //////////////////////////////////////////////////////////////////////////
    if(guiRedraw) {
      renderer.ResetZValue();
      renderer.QueueingEnabled = true;
      renderer.ClearRenderList();

      if(activeSheet != null) {
        activeSheet.Render();
      }

      guiRedraw = false;
    }

    // Render queued objects
    renderer.DoRender();

    renderer.QueueingEnabled = false;
    MouseCursor.Instance.Draw();
  }

  /// <summary>
  ///		Set the default font to be used by the system.
  /// </summary>
  /// <param name="name">Name of the default font to be used by the system.</param>
  public void SetDefaultFont(string name) {
    if(name == null || name.Length == 0) {
      Font tmp = null;
      SetDefaultFont(tmp);
    } else {
      SetDefaultFont(FontManager.Instance.GetFont(name));
    }
  }

  /// <summary>
  ///		Set the default font to be used by the system.
  /// </summary>
  /// <param name="font">Reference to the default font to be used by the system.</param>
  public void SetDefaultFont(Font font) {
    defaultFont = font;

    // TODO: Add a 'system default font' changed event and fire it here.
  }

  /// <summary>
  ///		Set the image to be used as the default mouse cursor.
  /// </summary>
  /// <param name="imagesetName">The name of the Imageset  that contains the image to be used.</param>
  /// <param name="imageName">The name of the Image on <paramref name="imageset"/> that is to be used.</param>
  public void SetDefaultMouseCursor(string imagesetName, string imageName) {
    defaultMouseCursor = ImagesetManager.Instance.GetImageset(imagesetName).GetImage(imageName);
  }

  /// <summary>
  ///		Set the image to be used as the default mouse cursor.
  /// </summary>
  /// <param name="image">
  ///		Reference to an image object that is to be used as the default mouse cursor.  To have no cursor rendered by default, you
  ///		can specify null here.
  /// </param>
  public void SetDefaultMouseCursor(Image image) {
    defaultMouseCursor = image;
  }

  /// <summary>
  ///		Causes a full re-draw next time <see cref="RenderGui"/> is called.
  /// </summary>
  public void SignalRedraw() {
    guiRedraw = true;
  }

  #endregion Methods

  #region Input Injection Methods

  /// <summary>
  ///		Method that injects a typed character into the system.
  /// </summary>
  /// <param name="c">Character that was typed.</param>
  /// <returns>Returns true if the event was handled, and false if it is not.</returns>
  public bool InjectChar(char c) {
    if(activeSheet != null) {
      KeyEventArgs e = new KeyEventArgs();
      e.Character = c;
      e.Modifiers = sysKeys;

      Window destWindow = activeSheet.ActiveChild;

      while((destWindow != null) && !e.Handled) {
        destWindow.OnCharacter(e);
        destWindow = destWindow.Parent;
      }

      return e.Handled;
    }

    return false;
  }

  /// <summary>
  ///		Injects a key down event into the system.
  /// </summary>
  /// <param name="keyCode">Keyboard key pressed.</param>
  /// <returns>Returns true if the event was handled, and false if it is not.</returns>
  public bool InjectKeyDown(System.Windows.Forms.Keys keyCode) {
    // update sys keys
    sysKeys |= KeyCodeToSysKey(keyCode, true);

    if(activeSheet != null) {
      KeyEventArgs e = new KeyEventArgs();
      e.KeyCode = keyCode;
      e.Modifiers = sysKeys;

      Window destWindow = activeSheet.ActiveChild;

      while((destWindow != null) && !e.Handled) {
        destWindow.OnKeyDown(e);
        destWindow = destWindow.Parent;
      }

      return e.Handled;
    }

    return false;
  }

  /// <summary>
  ///		Injects a key up event into the system.
  /// </summary>
  /// <param name="keyCode">Keyboard key pressed.</param>
  /// <returns>Returns true if the event was handled, and false if it is not.</returns>
  public bool InjectKeyUp(System.Windows.Forms.Keys keyCode) {
    // update sys keys
    sysKeys &= ~KeyCodeToSysKey(keyCode, true);

    if(activeSheet != null) {
      KeyEventArgs e = new KeyEventArgs();
      e.KeyCode = keyCode;
      e.Modifiers = sysKeys;

      Window destWindow = activeSheet.ActiveChild;

      while((destWindow != null) && !e.Handled) {
        destWindow.OnKeyUp(e);
        destWindow = destWindow.Parent;
      }

      return e.Handled;
    }

    return false;
  }

  /// <summary>
  ///		Injects a mouse down event into the system.
  /// </summary>
  /// <param name="button">Mouse button being pressed.</param>
  /// <returns>Returns true if the event was handled, and false if it is not.</returns>
  public bool InjectMouseDown(System.Windows.Forms.MouseButtons button) {
    MouseEventArgs e = new MouseEventArgs();
    e.Position = MouseCursor.Instance.Position;
    e.MoveDelta = new PointF(0, 0);
    e.Button = button;
    bool handled = false;

    Window destWindow = GetTargetWindow(e.Position);

    Window eventWindow = destWindow;

    while(!e.Handled && (eventWindow != null)) {
      eventWindow.OnMouseButtonsDown(e);
      eventWindow = eventWindow.Parent;
    }

    // Double/Triple click
    MouseClickTracker tracker = this.GetClickTracker(button);//clickTrackers[(int)button];

    tracker.clickCount++;

    if(tracker.timer.Elapsed > doubleClickTimeout ||
        !tracker.clickArea.IsPointInRect(e.Position) ||
        tracker.clickCount > 3) {

      // single down event
      tracker.clickCount = 1;

      // build allowable area for multi-clicks
      tracker.clickArea.Position = e.Position;
      tracker.clickArea.Size = doubleClickSize;
      tracker.clickArea.Offset(new PointF(-(doubleClickSize.Width * 0.5f), -(doubleClickSize.Height * 0.5f)));
    }

    // reuse same event args from earlier
    handled = e.Handled;
    e.Handled = false;

    switch(tracker.clickCount) {
      case 2:
        eventWindow = destWindow;

        while(!e.Handled && (eventWindow != null)) {
          eventWindow.OnMouseDoubleClicked(e);
          eventWindow = eventWindow.Parent;
        }

        break;

      case 3:
        eventWindow = destWindow;

        while(!e.Handled && (eventWindow != null)) {
          eventWindow.OnMouseTripleClicked(e);
          eventWindow = eventWindow.Parent;
        }

        break;
    }

    // reset timer for this button
    tracker.timer.Restart();

    return (handled || e.Handled);
  }

  /// <summary>
  /// Tell CeGui that the mouse has left the application window
  /// </summary>
  public bool InjectMouseLeaves() {
    MouseEventArgs e = new MouseEventArgs();

    // if there is no window that currently contains the mouse, then
    // there is nowhere to send input
    if (windowWithMouse != null) {
      e.Position   = MouseCursor.Instance.Position;
      e.MoveDelta  = new PointF(0, 0);
      windowWithMouse.OnMouseLeaves(e);
      windowWithMouse = null;
    }
    return e.Handled;
  }

  /// <summary>
  ///		Injects a mouse up event into the system.
  /// </summary>
  /// <param name="button">Mouse button being pressed.</param>
  /// <returns>Returns true if the event was handled, and false if it is not.</returns>
  public bool InjectMouseUp(System.Windows.Forms.MouseButtons button) {
    MouseEventArgs e = new MouseEventArgs();
    e.Position = MouseCursor.Instance.Position;
    e.MoveDelta = new PointF(0, 0);
    e.Button = button;

    Window destWindow = GetTargetWindow(e.Position);

    Window eventWindow = destWindow;

    while(!e.Handled && (eventWindow != null)) {
      eventWindow.OnMouseButtonsUp(e);
      eventWindow = eventWindow.Parent;
    }

    // check timer for 'button' to see if this up event also constitutes a single click
    if(this.GetClickTracker(button).timer.Elapsed <= clickTimeout) {
      e.Handled = false;
      destWindow = GetTargetWindow(e.Position);

      while(!e.Handled && (destWindow != null)) {
        destWindow.OnMouseClicked(e);
        destWindow = destWindow.Parent;
      }
    }

    return e.Handled;
  }

  /// <summary>
  ///		Method that injects a mouse movement event into the system.
  /// </summary>
  /// <param name="deltaX">Amount the mouse moved on the x axis.</param>
  /// <param name="deltaY">Amount the mouse moved on the y axis.</param>
  /// <returns>Returns true if the event was handled, and false if it is not.</returns>
  public bool InjectMouseMove(int deltaX, int deltaY) {
    MouseCursor mouse = MouseCursor.Instance;

    MouseEventArgs e = new MouseEventArgs();

    e.MoveDelta.X = (float)deltaX;
    e.MoveDelta.Y = (float)deltaY;

    // move the mouse cursor
    mouse.OffsetPosition(e.MoveDelta);
    e.Position = mouse.Position;

    Window destWindow = GetTargetWindow(e.Position);

    // if there is no GUI sheet, then there is nowhere to send input
    // TODO: keep an eye on passing the same args around
    if(destWindow != null) {
      if(destWindow != windowWithMouse) {
        if(windowWithMouse != null) {
          windowWithMouse.OnMouseLeaves(e);
        }

        windowWithMouse = destWindow;
        destWindow.OnMouseEnters(e);
      }

      while(!e.Handled && (destWindow != null)) {
        destWindow.OnMouseMove(e);
        destWindow = destWindow.Parent;
      }
    }

    return e.Handled;
  }

  /// <summary>
  ///		Method that injects a mouse position event into the system.
  /// </summary>
  /// <param name="deltaX">The x coordinate.</param>
  /// <param name="deltaY">The y coordinate.</param>
  /// <returns>Returns true if the event was handled, and false if it is not.</returns>
  public bool InjectMousePosition(int x, int y)
  {
      MouseCursor mouse = MouseCursor.Instance;

      MouseEventArgs e = new MouseEventArgs();

      e.MoveDelta.X = (float) mouse.Position.X - x;
      e.MoveDelta.Y = (float) mouse.Position.Y - y;

      // move the mouse cursor
      mouse.Position = new PointF(x, y);
      e.Position = mouse.Position;

      Window destWindow = GetTargetWindow(e.Position);

      // if there is no GUI sheet, then there is nowhere to send input
      // TODO: keep an eye on passing the same args around
      if (destWindow != null)
      {
          if (destWindow != windowWithMouse)
          {
              if (windowWithMouse != null)
              {
                  windowWithMouse.OnMouseLeaves(e);
              }

              windowWithMouse = destWindow;
              destWindow.OnMouseEnters(e);
          }

          while (!e.Handled && (destWindow != null))
          {
              destWindow.OnMouseMove(e);
              destWindow = destWindow.Parent;
          }
      }

      return e.Handled;
  }

  /// <summary>
  ///		Method that injects a mouse wheel scroll event into the system.
  /// </summary>
  /// <param name="delta">Amount that the mouse wheel was moved.</param>
  /// <returns>Returns true if the event was handled, and false if it is not.</returns>
  public bool InjectMouseWheel(int delta) {
    MouseEventArgs e = new MouseEventArgs();
    e.WheelDelta = delta;
    e.Position = MouseCursor.Instance.Position;

    Window destWindow = GetTargetWindow(e.Position);

    if(destWindow != null) {
      destWindow.OnMouseWheel(e);
    }

    return e.Handled;
  }

  #endregion Input Injection Methods

  #region Private Helper Methods

  /// <summary>
  ///		Given <paramref name="point"/>, return a reference to the <see cref="Window"/> that should receive a 
  ///		mouse input if <paramref name="point"/> is the mouse location.
  /// </summary>
  /// <param name="point">Screen location (in pixels).</param>
  /// <returns>
  ///		Reference to a <see cref="Window"/> object at mouse locatioj <paramref name="point"/> that
  ///		is to receive input.
  /// </returns>
  private Window GetTargetWindow(PointF point) {
    Window destWindow = null;

    // if there is no GUI sheet, then there is nowhere to send input
    if(activeSheet != null) {
      destWindow = Window.CaptureWindow;

      if(destWindow == null) {
        destWindow = activeSheet.GetChildAtPosition(point);

        if(destWindow == null) {
          destWindow = activeSheet;
        }
      } else {
        Window childWindow = destWindow.GetChildAtPosition(point);

        if(childWindow != null) {
          destWindow = childWindow;
        }
      }
    }

    return destWindow;
  }

  /// <summary>
  ///		Converts the specified keycode into a system key value.
  /// </summary>
  /// <param name="key">KeyCode to consider.</param>
  /// <param name="direction">True if down, false if coming back up.</param>
  /// <returns>System key associated with the specified keycode.</returns>
  private ModifierKeys KeyCodeToSysKey(System.Windows.Forms.Keys key, bool direction) {
    switch(key) {
      case System.Windows.Forms.Keys.Shift:
        leftShift = direction;

        if(!rightShift) {
          return ModifierKeys.Shift;
        }

        break;

      case System.Windows.Forms.Keys.ShiftKey:
        rightShift = direction;

        if(!leftShift) {
          return ModifierKeys.Shift;
        }

        break;

      case System.Windows.Forms.Keys.Control:
        leftCtrl = direction;

        if(!rightCtrl) {
          return ModifierKeys.Control;
        }

        break;

      case System.Windows.Forms.Keys.ControlKey:
        rightCtrl = direction;

        if(!leftCtrl) {
          return ModifierKeys.Control;
        }

        break;
    }

    // if not a system key or the state hasn't changed, return None
    return ModifierKeys.None;
  }

  #endregion Private Helper Methods

  #region Nested Classes

  /// <summary>
  ///		Structure to use for tracking mouse click timing for a single button.
  /// </summary>
  /// <remarks>
  ///		This would be a struct, but we need to be able to create the timer here.
  /// </remarks>
  public class MouseClickTracker {
    /// <summary>
    ///		Timer to use for timing mouse clicks.
    /// </summary>
    public Timer timer = new Timer();
    /// <summary>
    ///		Number of times the button has been clicked.
    /// </summary>
    public int clickCount;
    /// <summary>
    ///		Area where the mouse was last clicked (with tolerance).
    /// </summary>
    public Rect clickArea;
  }

  /// <summary>
  ///		Simple class for tracking elasped time (in milliseconds).
  /// </summary>
  public class Timer {
    /// <summary>
    ///		Elapsed time since the last call to <see cref="Restart"/>.
    /// </summary>
    private float elapsed;

    /// <summary>
    ///		Constructor.
    /// </summary>
    public Timer() {
      Restart();
    }

    /// <summary>
    ///		Gets the elapsed time (in milliseconds) since the last call to <see cref="Restart"/>.
    /// </summary>
    public float Elapsed {
      get {
        return (Environment.TickCount - elapsed) / 1000;
      }
    }

    /// <summary>
    ///		Restarts the timer.
    /// </summary>
    public void Restart() {
      // tick count is more than sufficient for our purposes
      elapsed = Environment.TickCount;
    }
  }

  #endregion Nested Classes
}

} // namespace CeGui
