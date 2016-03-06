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
///		Summary description for Slider.
/// </summary>
[ AlternateWidgetName("Slider") ]
public abstract class Slider : Window {
  #region Fields

  /// <summary>
  ///		Widget used to represent the 'thumb' of the slider.
  /// </summary>
  protected Thumb thumb;

  /// <summary>
  ///		Current slider value.
  /// </summary>
  protected float currentValue;
  /// <summary>
  ///		Slider maximum value (minimum is fixed at 0).
  /// </summary>
  protected float maxValue;
  /// <summary>
  ///		Amount to adjust slider by when clicked (and not dragged).
  /// </summary>
  protected float step;

  #endregion Fields

  #region Constructor

  /// <summary>
  ///		Constructor.
  /// </summary>
  /// <param name="type"></param>
  /// <param name="name"></param>
  public Slider(string type, string name)
    : base(type, name) {
    maxValue = 1.0f;
    step = 0.01f;
  }

  #endregion Constructor

  #region Properties

  /// <summary>
  ///		Get/Set the maximum value for the slider.
  /// </summary>
  /// <remarks>Note that the minimum value is fixed at 0.</remarks>
  /// <value>float value specifying the maximum value for this slider widget.</value>
  [WidgetProperty("MaximumValue")]
  public float MaxValue {
    get {
      return maxValue;
    }
    set {
      maxValue = value;

      float oldVal = currentValue;

      // limit current value to be within the new max
      if(currentValue > maxValue) {
        currentValue = maxValue;
      }

      UpdateThumb();

      // send notification if slider value changed
      if(currentValue != oldVal) {
        OnValueChanged(new WindowEventArgs(this));
      }
    }
  }

  /// <summary>
  ///		Get/Set the current click step setting for the slider.
  /// </summary>
  /// <remarks>
  ///		The click step SizeF is the amount the slider value will be adjusted when the widget
  ///		is clicked wither side of the slider thumb.
  /// </remarks>
  /// <value>float value representing the click step setting to use.</value>
  [WidgetProperty("ClickStepSize")]
  public float Step {
    get {
      return step;
    }
    set {
      step = value;
    }
  }

  /// <summary>
  ///		Get/Set the current value of the slider.
  /// </summary>
  /// <value>Float value of the slider.</value>
  [WidgetProperty("CurrentValue")]
  public float Value {
    get {
      return currentValue;
    }
    set {
      float oldVal = currentValue;

      // range for value 0 <= value <= maxValue
      currentValue = (value <= maxValue) ? value : maxValue;

      UpdateThumb();

      // send notification if slider value changed
      if(currentValue != oldVal) {
        OnValueChanged(new WindowEventArgs(this));
      }
    }
  }

  #endregion Properties

  #region Abstract Members

  #region Methods

  // njk-patch
  /// <summary>
  ///		Create a Thumb based widget to use as the thumb for this slider.
  /// </summary>
  /// <returns>A custom Thumb widget for this slider.</returns>
  protected abstract Thumb CreateThumb();

  /// <summary>
  ///		Given window location <paramref name="point"/>, return a value indicating what change should be 
  ///		made to the slider.
  /// </summary>
  /// <param name="point">Point object describing a pixel position in window space.</param>
  /// <returns>
  ///		- -1 to indicate slider should be moved to a lower setting.
  ///		-  0 to indicate slider should not be moved.
  ///		- +1 to indicate slider should be moved to a higher setting.
  /// </returns>
  protected abstract float GetAdjustDirectionFromPoint(PointF point);

  /// <summary>
  ///		Return value that best represents current slider value given the current location of the thumb.
  /// </summary>
  /// <returns>float value that, given the thumb widget position, best represents the current value for the slider.</returns>
  protected abstract float GetValueFromThumb();

  /// <summary>
  ///		Layout the slider component widgets.
  /// </summary>
  protected abstract void LayoutComponentWidgets();

  /// <summary>
  ///		Update the SizeF and location of the thumb to properly represent the current state of the slider.
  /// </summary>
  protected abstract void UpdateThumb();

  #endregion Methods

  #endregion Abstract Members

  #region Window Members

  // njk-patch
  /// <summary>
  ///		Intialize this slider widget.
  /// </summary>
  public override void Initialize() {
    // calling in case anything is ever added to the base method
    base.Initialize();

    // create and attach thumb
    thumb = CreateThumb();
    AddChild(thumb);

    // subscribe to the position changed event for the thumb
    thumb.PositionChanged += new WindowEventHandler(ThumbMovedHandler);

    LayoutComponentWidgets();
  }


  #endregion Window Members

  #region Events

  #region Event Declarations

  /// <summary>
  ///		Event fired when the slider value changes.
  /// </summary>
  public event WindowEventHandler ValueChanged;

  #endregion Event Declarations

  #region Trigger Methods

  /// <summary>
  ///		Triggered when the slider value changes.
  /// </summary>
  /// <param name="e"></param>
  protected internal void OnValueChanged(WindowEventArgs e) {
    if(ValueChanged != null) {
      ValueChanged(this, e);
    }
  }

  #endregion Trigger Methods

  #region Overridden Trigger Methods

  /// <summary>
  ///		Handler for when the mouse button is pressed.
  /// </summary>
  /// <param name="e">Event args.</param>
  protected internal override void OnMouseButtonsDown(MouseEventArgs e) {
    // default processing
    base.OnMouseButtonsDown(e);

    if(e.Button == System.Windows.Forms.MouseButtons.Left) {
      float adjust = GetAdjustDirectionFromPoint(e.Position);

      // adjust slider position in whichever direction as required.
      if(adjust != 0) {
        Value = currentValue + (adjust * step);
      }

      e.Handled = true;
    }
  }

  /// <summary>
  ///		Steps the slider thumb when scrolling the mouse.
  /// </summary>
  /// <param name="e"></param>
  protected internal override void OnMouseWheel(MouseEventArgs e) {
    base.OnMouseWheel(e);

    if(e.WheelDelta != 0) {
      Value = currentValue + (e.WheelDelta * step);
    }

    e.Handled = true;
  }

  /// <summary>
  ///		Handler for when the SizeF of the slider widget changes.
  /// </summary>
  /// <param name="e">Event args.</param>
  protected internal override void OnSized(GuiEventArgs e) {
    // default processing
    base.OnSized(e);

    LayoutComponentWidgets();

    e.Handled = true;
  }

  #endregion Overridden Trigger Methods

  #region Handlers

  /// <summary>
  ///		Handler for notification when our thumb is moved.
  /// </summary>
  /// <param name="e">Event args.</param>
  protected virtual void ThumbMovedHandler(object sender, WindowEventArgs e) {
    Value = GetValueFromThumb();
  }

  #endregion Handlers

  #endregion Events
}

} // namespace CeGui.Widgets
