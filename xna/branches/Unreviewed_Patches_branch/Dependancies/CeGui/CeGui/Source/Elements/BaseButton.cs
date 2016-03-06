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
/// Summary description for BaseButton.
/// </summary>
///	C++ Version Sync
/// .cpp:	1.6
/// .h:		1.6
[ AlternateWidgetName("BaseButton") ]
public abstract class BaseButton : Window {
  #region Constants

  /// <summary>
  ///		Default color used when rendering label text in normal state.
  /// </summary>
  public static readonly Colour DefaultNormalLabelColor = new Colour(1, 1, 1, 0);
  /// <summary>
  ///		Default color used when rendering label text in hover / highlight state.
  /// </summary>
  public static readonly Colour DefaultHoverLabelColor = new Colour(1, 1, 1, 0);
  /// <summary>
  ///		Default color used when rendering label text in pushed state.
  /// </summary>
  public static readonly Colour DefaultPushedLabelColor = new Colour(1, 1, 1, 0);
  /// <summary>
  ///		Default color used when rendering label text in disabled state.
  /// </summary>
  public static readonly Colour DefaultDisabledLabelColor = new Colour(.7f, .7f, .7f, 0);

  #endregion Constants

  #region Fields

  /// <summary>
  ///		true when this button is pushed.
  /// </summary>
  protected bool isPushed;
  /// <summary>
  ///		true when the button is in 'hover' state and requires the hover rendering.
  /// </summary>
  protected bool isHovering;

  /// <summary>
  ///		Color used for label text when rendering in normal state.
  /// </summary>
  protected Colour normalColor;
  /// <summary>
  ///		Color used for label text when rendering in highlighted state.
  /// </summary>
  protected Colour hoverColor;
  /// <summary>
  ///		Color used for label text when rendering in pushed state.
  /// </summary>
  protected Colour pushedColor;
  /// <summary>
  ///		Color used for label text when rendering in disabled state.
  /// </summary>
  protected Colour disabledColor;

  #endregion Fields

  #region Constructor

  /// <summary>
  ///		Constructor.
  /// </summary>
  /// <param name="type"></param>
  /// <param name="name"></param>
  public BaseButton(string type, string name)
    : base(type, name) {
    // set the default colors
    normalColor = DefaultNormalLabelColor;
    hoverColor = DefaultHoverLabelColor;
    pushedColor = DefaultPushedLabelColor;
    disabledColor = DefaultDisabledLabelColor;
  }

  #endregion Constructor

  #region Base Members

  #region Properties

  /// <summary>
  ///		Returns true if user is hovering over this widget (or it's pushed and user is not over it for highlight).
  /// </summary>
  /// <value>true if the user is hovering or if the button is pushed and the mouse is not over the button.  Otherwise return false.</value>
  public bool IsHovering {
    get {
      return isHovering;
    }
  }

  /// <summary>
  ///		Returns true if the button widget is in the pushed state.
  /// </summary>
  /// <value>true if the button-type widget is pushed, false if the widget is not pushed.</value>
  public bool IsPushed {
    get {
      return isPushed;
    }
  }

  /// <summary>
  ///		Get/Set the text label color used for normal rendering.
  /// </summary>
  /// <value>Color value.</value>
  [WidgetProperty("NormalTextColour")]
  public Colour NormalTextColor {
    get {
      return normalColor;
    }
    set {
      if(normalColor != value) {
        // alpha part comes from window alpha
        normalColor = new Colour(value.Red, value.Green, value.Blue);
        RequestRedraw();
      }
    }
  }

  /// <summary>
  ///		Get/Set the text label color used for hovered state rendering.
  /// </summary>
  /// <value>Color value.</value>
  [WidgetProperty("HoverTextColour")]
  public Colour HoverTextColor {
    get {
      return hoverColor;
    }
    set {
      if(hoverColor != value) {
        // alpha part comes from window alpha
        hoverColor = new Colour(value.Red, value.Green, value.Blue);
        RequestRedraw();
      }
    }
  }

  /// <summary>
  ///		Get/Set the text label color used for pushed state rendering.
  /// </summary>
  /// <value>Color value.</value>
  [WidgetProperty("PushedTextColour")]
  public Colour PushedTextColor {
    get {
      return pushedColor;
    }
    set {
      if(pushedColor != value) {
        // alpha part comes from window alpha
        pushedColor = new Colour(value.Red, value.Green, value.Blue);
        RequestRedraw();
      }
    }
  }

  /// <summary>
  ///		Get/Set the text label color used for disabled state rendering.
  /// </summary>
  /// <value>Color value.</value>
  [WidgetProperty("DisabledTextColour")]
  public Colour DisabledTextColor {
    get {
      return disabledColor;
    }
    set {
      if(disabledColor != value) {
        // alpha part comes from window alpha
        disabledColor = new Colour(value.Red, value.Green, value.Blue);
        RequestRedraw();
      }
    }
  }

  #endregion Properties

  #region Methods

  /// <summary>
  ///		Update the internal state of the widget with the mouse at the given position.
  /// </summary>
  /// <param name="mousePosition">The location of the mouse cursor (in screen pixel co-ordinates).</param>
  protected void UpdateInternalState(PointF mousePosition) {
    bool oldstate = isHovering;

    // assume not hovering 
    isHovering = false;

    // if input is captured, but not by 'this', then we never hover highlight
    Window captureWnd = Window.CaptureWindow;

    if((captureWnd == null) || (captureWnd == this)) {
      Window sheet = GuiSystem.Instance.GuiSheet;

      if(sheet != null) {
        // check if hovering highlight is required, which is basically ("mouse over widget" XOR "widget pushed").
        if((this == sheet.GetChildAtPosition(mousePosition)) != isPushed) {
          isHovering = true;
        }
      }
    }

    // if state has changed, trigger a re-draw
    if(oldstate != isHovering) {
      RequestRedraw();
    }
  }

  #endregion Methods

  #endregion Base Members

  #region Abstract/Virtual Members

  #region Methods

  /// <summary>
  ///		Render the button-type widget in it's 'normal' state.
  /// </summary>
  /// <param name="z">float value specifying the base Z co-ordinate that should be used when rendering.</param>
  protected abstract void DrawNormal(float z);

  /// <summary>
  ///		Render the button-type widget in it's 'hover' (highlighted) state.
  /// </summary>
  /// <param name="z">float value specifying the base Z co-ordinate that should be used when rendering.</param>
  protected virtual void DrawHover(float z) {
    DrawNormal(z);
  }

  /// <summary>
  ///		Render the button-type widget in it's 'pushed' state.
  /// </summary>
  /// <param name="z">float value specifying the base Z co-ordinate that should be used when rendering.</param>
  protected virtual void DrawPushed(float z) {
    DrawNormal(z);
  }

  /// <summary>
  ///		Render the button-type widget in it's 'disabled' state.
  /// </summary>
  /// <param name="z">float value specifying the base Z co-ordinate that should be used when rendering.</param>
  protected virtual void DrawDisabled(float z) {
    DrawNormal(z);
  }

  #endregion Methods

  #endregion Abstract/Virtual Members

  #region Window Members

  /// <summary>
  ///		Perform the rendering for this widget.
  /// </summary>
  /// <param name="z">float value specifying the base Z co-ordinate that should be used when rendering.</param>
  protected override void DrawSelf(float z) {
    if(IsHovering) {
      DrawHover(z);
    } else if(IsPushed) {
      DrawPushed(z);
    } else if(!IsEnabled) {
      DrawDisabled(z);
    } else {
      DrawNormal(z);
    }
  }

  #region Overridden Event Trigger Methods

  protected internal override void OnMouseMove(MouseEventArgs e) {
    // this is needed to discover whether mouse is in the widget area or not.
    // The same thing used to be done each frame in the rendering method,
    // but in this version the rendering method may not be called every frame
    // so we must discover the internal widget state here - which is actually
    // more efficient anyway.

    // base class processing
    base.OnMouseMove(e);

    UpdateInternalState(e.Position);
    e.Handled = true;
  }

  protected internal override void OnMouseButtonsDown(MouseEventArgs e) {
    // default processing
    base.OnMouseButtonsDown(e);

    if(e.Button == System.Windows.Forms.MouseButtons.Left) {
      CaptureInput();
      isPushed = true;
      UpdateInternalState(e.Position);
      RequestRedraw();

      // we handled this one
      e.Handled = true;
    }
  }


  protected internal override void OnMouseButtonsUp(MouseEventArgs e) {
    // default processing
    base.OnMouseButtonsDown(e);

    if(e.Button == System.Windows.Forms.MouseButtons.Left) {
      ReleaseInput();

      // event was handled by us
      e.Handled = true;
    }
  }

  protected internal override void OnCaptureLost(GuiEventArgs e) {
    // default processing
    base.OnCaptureLost(e);

    isPushed = false;
    UpdateInternalState(MouseCursor.Instance.Position);
    RequestRedraw();

    // event was handled by us
    e.Handled = true;
  }

  protected internal override void OnMouseLeaves(MouseEventArgs e) {
    // default processing
    base.OnMouseLeaves(e);

    isHovering = false;
    RequestRedraw();

    e.Handled = true;
  }

  #endregion Overridden Event Trigger Methods

  #endregion Window Members
}

} // namespace CeGui.Widgets
