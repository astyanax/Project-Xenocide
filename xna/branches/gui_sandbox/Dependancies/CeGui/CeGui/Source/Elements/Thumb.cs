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
/// Range class used for setting the range of the THumb control
/// </summary>
public struct Range {
  // minimum value
  public float min;
  // maximum value
  public float max;

  public Range(float minimum, float maximum) {
    min = minimum;
    max = maximum;
  }

  /// <summary>
  /// Parses the string representation of a Range, and returns the corresponding Range value.
  /// </summary>
  /// <param name="value">String to parse</param>
  /// <returns>Range specified by the string.</returns>
  public static Range Parse(string value) {
    string[] parameters = value.Split(new char[] { ' ', ':' });
    Range range = new Range();

    for(int i = 0; i < parameters.Length; i++) {
      if(0 == parameters[i].CompareTo("min")) {
        range.min = float.Parse(parameters[++i]);
      } else if(0 == parameters[i].CompareTo("max")) {
        range.max = float.Parse(parameters[++i]);
      }
    }
    return range;
  }

  /// <summary>
  /// Returns the string representation of the Range value.
  /// </summary>
  /// <returns>A string representing the range.</returns>
  public override string ToString() {
    return string.Format("min:{0} max:{1}", min, max);
  }
}

/// <summary>
///		Base class for Thumb widget.
/// </summary>
/// <remarks>
///		The thumb widget is used to compose other widgets (like sliders and scroll bars).  You would
///		not normally need to use this widget directly unless you are making a new widget of some type.
/// </remarks>
[ AlternateWidgetName("Thumb") ]
public abstract class Thumb : PushButton {
  #region Fields

  /// <summary>
  ///		true if events are to be sent real-time, else just when thumb is released.
  /// </summary>
  protected bool hotTrack;
  /// <summary>
  ///		true if thumb is movable vertically.
  /// </summary>
  protected bool vertFree;
  /// <summary>
  ///		true if thumb is movable horizontally.
  /// </summary>
  protected bool horzFree;

  /// <summary>
  ///		Vertical minimum range.
  /// </summary>
  protected float vertMin;
  /// <summary>
  ///		Vertical maximum range.
  /// </summary>
  protected float vertMax;
  /// <summary>
  ///		Horizontal minimum range.
  /// </summary>
  protected float horzMin;
  /// <summary>
  ///		Horizontal maximum range.
  /// </summary>
  protected float horzMax;

  /// <summary>
  ///		true if thumb is being dragged.
  /// </summary>
  protected bool isBeingDragged;
  /// <summary>
  ///		Point where we are being dragged at.
  /// </summary>
  protected PointF dragPoint;

  #endregion Fields

  #region Constructor

  /// <summary>
  ///		Constructor.
  /// </summary>
  /// <param name="type"></param>
  /// <param name="name"></param>
  public Thumb(string type, string name)
    : base(type, name) {
    hotTrack = true;
    horzMax = 1.0f;
    vertMax = 1.0f;
  }

  #endregion Constructor

  #region Properties

  /// <summary>
  ///		Get/Set whether hot-tracking is enabled or not.
  /// </summary>
  /// <value>true if hot-tracking is enabled.  false if hot-tracking is disabled.</value>
  [WidgetProperty("HotTracked")]
  public bool HotTrack {
    get {
      return hotTrack;
    }
    set {
      hotTrack = value;
    }
  }

  /// <summary>
  /// Get/Set the vertical range
  /// </summary>
  /// <value>Range</value>
  [WidgetProperty("VertRange")]
  public Range VerticalRange {
    get {
      return new Range(vertMin, vertMax);
    }
    set {
      SetVerticalRange(value.min, value.max);
    }
  }

  /// <summary>
  /// Get/Set the horizontal range
  /// </summary>
  /// <value>Range</value>
  [WidgetProperty("HorzRange")]
  public Range HorizontalRange {
    get {
      return new Range(horzMin, horzMax);
    }
    set {
      SetHorizontalRange(value.min, value.max);
    }
  }

  /// <summary>
  ///		Get/Set whether the thumb is movable on the horizontal axis.
  /// </summary>
  /// <value>
  ///		true if the thumb is movable along the horizontal axis.
  ///		false if the thumb is fixed on the horizontal axis.
  ///	</value>
  [WidgetProperty("HorzFree")]
  public bool Horizontal {
    get {
      return horzFree;
    }
    set {
      horzFree = value;
    }
  }

  /// <summary>
  ///		Get/Set whether the thumb is movable on the vertical axis.
  /// </summary>
  /// <value>
  ///		true if the thumb is movable along the vertical axis.
  ///		false if the thumb is fixed on the vertical axis.
  ///	</value>
  [WidgetProperty("VertFree")]
  public bool Vertical {
    get {
      return vertFree;
    }
    set {
      vertFree = value;
    }
  }

  #endregion Properties

  #region Methods

  /// <summary>
  ///		Set the movement range of the thumb for the horizontal axis.
  /// </summary>
  /// <remarks>
  ///		The values specified here are relative to the parent window for the thumb, and are specified in whichever
  ///		metrics mode is active for the widget.
  /// </remarks>
  /// <param name="min">The minimum setting for the thumb on the horizontal axis.</param>
  /// <param name="max">The maximum setting for the thumb on the horizontal axis.</param>
  public void SetHorizontalRange(float min, float max) {
    // ensure min <= max, swap if not.
    if(min > max) {
      float tmp = min;
      max = min;
      min = tmp;
    }

    horzMax = max;
    horzMin = min;

    // validate current position.
    float currentPos = X;

    if(currentPos < min) {
      X = min;
    } else if(currentPos > max) {
      X = max;
    }
  }

  /// <summary>
  ///		Set the movement range of the thumb for the vertical axis.
  /// </summary>
  /// <remarks>
  ///		The values specified here are relative to the parent window for the thumb, and are specified in whichever
  ///		metrics mode is active for the widget.
  /// </remarks>
  /// <param name="min">The minimum setting for the thumb on the vertical axis.</param>
  /// <param name="max">The maximum setting for the thumb on the vertical axis.</param>
  public void SetVerticalRange(float min, float max) {
    // ensure min <= max, swap if not.
    if(min > max) {
      float tmp = min;
      max = min;
      min = tmp;
    }

    vertMax = max;
    vertMin = min;

    // validate current position.
    float currentPos = Y;

    if(currentPos < min) {
      Y = min;
    } else if(currentPos > max) {
      Y = max;
    }
  }

  #endregion Methods

  #region Events

  #region Event Declarations

  /// <summary>
  ///		The position of the thumb widget has changed.
  /// </summary>
  public event WindowEventHandler PositionChanged;

  #endregion Event Declarations

  #region Trigger Methods

  /// <summary>
  ///		Triggers an event when the position of the thumb widget has changed.
  /// </summary>
  /// <param name="e">Event arguments.</param>
  protected internal void OnPositionChanged(WindowEventArgs e) {
    if(PositionChanged != null) {
      PositionChanged(this, e);
    }
  }

  #endregion Trigger Methods

  #region Overridden Trigger Methods

  /// <summary>
  ///		Track mouse movement for dragging behavior.
  /// </summary>
  /// <param name="e">Event args.</param>
  protected internal override void OnMouseMove(MouseEventArgs e) {
    // default processing
    base.OnMouseMove(e);

    // only react if we are being dragged
    if(isBeingDragged) {
      PointF delta;
      float hmin, hmax, vmin, vmax;

      // get some values as absolute pixel offsets
      if(this.MetricsMode == MetricsMode.Relative) {
        delta = RelativeToAbsolute(ScreenToWindow(e.Position));

        hmax = RelativeToAbsoluteXImpl(parent, horzMax);
        hmin = RelativeToAbsoluteXImpl(parent, horzMin);
        vmax = RelativeToAbsoluteYImpl(parent, vertMax);
        vmin = RelativeToAbsoluteYImpl(parent, vertMin);
      } else {
        delta = ScreenToWindow(e.Position);

        hmin = horzMin;
        hmax = horzMax;
        vmin = vertMin;
        vmax = vertMax;
      }

      // calculate amount of movement in pixels
      delta.X -= dragPoint.X;
      delta.Y -= dragPoint.Y;

      //
      // Calculate new (pixel) position for thumb
      //
      PointF newPos = absArea.Position;

      if(horzFree) {
        newPos.X += delta.X;

        // limit value to within currently set range
        newPos.X = (newPos.X < hmin) ? hmin : (newPos.X > hmax) ? hmax : newPos.X;
      }

      if(vertFree) {
        newPos.Y += delta.Y;

        // limit new position to within currently set range
        newPos.Y = (newPos.Y < vmin) ? vmin : (newPos.Y > vmax) ? vmax : newPos.Y;
      }

      // update thumb position if needed
      if(newPos != absArea.Position) {
        if(this.MetricsMode == MetricsMode.Relative) {
          newPos = AbsoluteToRelativeImpl(parent, newPos);
        }

        Position = newPos;

        // send notification as required
        if(hotTrack) {
          OnPositionChanged(new WindowEventArgs(this));
        }
      }
    }

    e.Handled = true;
  }

  /// <summary>
  ///		Track mouse down for dragging behavior.
  /// </summary>
  /// <param name="e">Event args.</param>
  protected internal override void OnMouseButtonsDown(MouseEventArgs e) {
    // default processing
    base.OnMouseButtonsDown(e);

    if(e.Button == System.Windows.Forms.MouseButtons.Left) {
      // initialise the dragging state
      isBeingDragged = true;
      dragPoint = ScreenToWindow(e.Position);

      if(this.MetricsMode == MetricsMode.Relative) {
        dragPoint = RelativeToAbsolute(dragPoint);
      }

      e.Handled = true;
    }
  }

  /// <summary>
  ///		Track capture lost to cancel dragging (if currently dragging).
  /// </summary>
  /// <param name="e">Event args.</param>
  protected internal override void OnCaptureLost(GuiEventArgs e) {
    // default processing
    base.OnCaptureLost(e);

    // we are no longer dragging no that the thumb is released
    isBeingDragged = false;

    // send notification whenever thumb is released
    OnPositionChanged(new WindowEventArgs(this));
  }


  #endregion Overridden Trigger Methods

  #endregion Events
}

} // namespace CeGui.Widgets
