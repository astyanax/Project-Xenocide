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
///		Base scroll bar class.
/// </summary>
/// <remarks>
///		This base class for scroll bars does not have any idea of direction - a derived class would
///		add whatever meaning is appropriate according to what that derived class
///		represents to the user.
/// </remarks>
[ AlternateWidgetName("Scrollbar") ]
public abstract class Scrollbar : Window {
  #region Fields

  /// <summary>
  ///		The SizeF of the document / data being scrolled thorugh.
  /// </summary>
  protected float documentSize;
  /// <summary>
  ///		The SizeF of a single 'page' of data.
  /// </summary>
  protected float pageSize;
  /// <summary>
  ///		Step SizeF used for increaseButton / decreaseButton button clicks.
  /// </summary>
  protected float stepSize;
  /// <summary>
  ///		Amount of overlap when jumping by a page.
  /// </summary>
  protected float overlapSize;
  /// <summary>
  ///		Current scroll position.
  /// </summary>
  protected float position;

  /// <summary>
  ///		Widget used to represent the 'thumb' of the scroll bar.
  /// </summary>
  protected Thumb thumb;
  /// <summary>
  ///		Widget used for the increaseButton button of the scroll bar.
  /// </summary>
  protected PushButton increaseButton;
  /// <summary>
  ///		Widget used for the decreaseButton button of the scroll bar.
  /// </summary>
  protected PushButton decreaseButton;

  #endregion Fields

  #region Constructor

  /// <summary>
  ///		Constructor.
  /// </summary>
  /// <param name="name">Name of this scrollbar.</param>
  public Scrollbar(string type, string name)
    : base(type, name) {
    documentSize = 1.0f;
    stepSize = 1.0f;
  }

  #endregion Constructor

  #region Base Members

  #region Properties

  /// <summary>
  ///		Get/Set the SizeF of the document or data.
  /// </summary>
  /// <remarks>
  ///		<para>
  ///		The document SizeF should be thought of as the total SizeF of the data that
  ///		is being scrolled through (the number of lines in a text file for example).
  ///		</para>
  ///		<para>
  ///		The returned value has no meaning within the Gui system, it is left up to the
  ///		application to assign appropriate values for the application specific use of the scroll bar.
  ///		</para>
  /// </remarks>
  /// <value>float value specifying the currently set document SizeF.</value>
  public float DocumentSize {
    get {
      return documentSize;
    }
    set {
      documentSize = value;
      UpdateThumb();
    }
  }

  /// <summary>
  ///		Get/Set the overlap SizeF for this scroll bar.
  /// </summary>
  /// <remarks>
  ///		<para>
  ///		The overlap SizeF is the amount of data from the end of a 'page' that will
  ///		remain visible when the position is moved by a page.  This is usually used
  ///		so that the user keeps some context of where they were within the document's
  ///		data when jumping a page at a time.
  ///		</para>
  ///		<para>
  ///		The returned value has no meaning within the Gui system, it is left up to the
  ///		application to assign appropriate values for the application specific use of the scroll bar.
  ///		</para>
  /// </remarks>
  /// <value>float value specifying the currently set overlap SizeF.</value>
  public float OverlapSize {
    get {
      return overlapSize;
    }
    set {
      overlapSize = value;
    }
  }

  /// <summary>
  ///		Get/Set the page SizeF for this scroll bar.
  /// </summary>
  /// <remarks>
  ///		<para>
  ///		The page SizeF is typically the amount of data that can be displayed at one
  ///		time.  This value is also used when calculating the amount the position will
  ///		change when you click either side of the scroll bar thumb - the amount the
  ///		position changes will is (pageSize - overlapSize).
  ///		</para>
  ///		<para>
  ///		The returned value has no meaning within the Gui system, it is left up to the
  ///		application to assign appropriate values for the application specific use of the scroll bar.
  ///		</para>
  /// </remarks>
  /// <value>float value specifying the currently set page SizeF.</value>
  public float PageSize {
    get {
      return pageSize;
    }
    set {
      pageSize = value;
      UpdateThumb();
    }
  }

  /// <summary>
  ///		Get/Set the current position of scroll bar within the document.
  /// </summary>
  /// <remarks>
  ///		<para>
  ///		The range of the scroll bar is from 0 to the SizeF of the document minus the
  ///		SizeF of a page (0 <= position <= (documentSize - pageSize)).
  ///		</para>
  ///		<para>
  ///		The returned value has no meaning within the Gui system, it is left up to the
  ///		application to assign appropriate values for the application specific use of the scroll bar.
  ///		</para>
  /// </remarks>
  /// <value>float value specifying the current position of the scroll bar within its document.</value>
  public float ScrollPosition {
    get {
      return position;
    }
    set {
      float oldPos = position;

      // max position is (docSize - pageSize), but must be at least 0 (in case doc SizeF is very small)
      float maxPos = Math.Max((documentSize - pageSize), 0.0f);

      // limit position to valid range:  0 <= position <= max_pos
      position = (value >= 0) ? ((value <= maxPos) ? value : maxPos) : 0.0f;

      UpdateThumb();

      // notification if required
      if(position != oldPos) {
        OnScrollPositionChanged(new WindowEventArgs(this));
      }
    }
  }

  /// <summary>
  ///		Get/Set the step SizeF for this scroll bar.
  /// </summary>
  /// <remarks>
  ///		<para>
  ///		The step SizeF is typically a single unit of data that can be displayed, this is the
  ///		amount the position will change when you click either of the arrow buttons on the
  ///		scroll bar.  (this could be 1 for a single line of text, for example).
  ///		</para>
  ///		<para>
  ///		The returned value has no meaning within the Gui system, it is left up to the
  ///		application to assign appropriate values for the application specific use of the scroll bar.
  ///		</para>
  /// </remarks>
  /// <value>float value specifying the currently set step SizeF.</value>
  public float StepSize {
    get {
      return stepSize;
    }
    set {
      stepSize = value;
    }
  }

  #endregion Properties

  #endregion Base Members

  #region Abstract Members

  #region Methods

  /// <summary>
  ///		Create a <see cref="PushButton"/> based widget to use as the decreaseButton button for this scroll bar.
  /// </summary>
  /// <returns>A custom PushButton implementation.</returns>
  protected abstract PushButton CreateDecreaseButton();

  /// <summary>
  ///		Create a <see cref="PushButton"/> based widget to use as the increaseButton button for this scroll bar.
  /// </summary>
  /// <returns>A custom PushButton implementation.</returns>
  protected abstract PushButton CreateIncreaseButton();

  /// <summary>
  ///		Create a <see cref="Thumb"/> based widget to use as the thumb for this scroll bar.
  /// </summary>
  /// <returns>A custom thumb implementation.</returns>
  protected abstract Thumb CreateThumb();

  /// <summary>
  ///		Given window location <paramref name="point"/>, return a value indicating what change should be made to the scroll bar.
  /// </summary>
  /// <param name="point">Point object describing a pixel position in window space.</param>
  /// <returns>
  ///		- -1 to indicate scroll bar position should be moved to a lower value.
  ///		-  0 to indicate scroll bar position should not be changed.
  ///		- +1 to indicate scroll bar position should be moved to a higher value.
  /// </returns>
  protected abstract float GetAdjustDirectionFromPoint(PointF point);

  /// <summary>
  ///		Return the value that best represents current scroll bar position given the current location of the thumb.
  /// </summary>
  /// <returns>float value that, given the thumb widget position, best represents the current position for the scroll bar.</returns>
  protected abstract float GetPositionFromThumb();

  /// <summary>
  ///		Layout the scroll bar component widgets
  /// </summary>
  protected abstract void LayoutComponentWidgets();

  /// <summary>
  ///		Update the SizeF and location of the thumb to properly represent the current state of the scroll bar.
  /// </summary>
  protected abstract void UpdateThumb();

  #endregion Methods

  #endregion Abstract Members

  #region Window Members

  #region Methods

  /// <summary>
  ///		Initialises the Scrollbar object ready for use.
  /// </summary>
  /// <remarks>
  ///		This must be called for every window created, which is handled by the window factory.
  /// </remarks>
  public override void Initialize() {
    base.Initialize();

    // setup the thumb
    thumb = CreateThumb();
    AddChild(thumb);
    thumb.PositionChanged += new WindowEventHandler(thumb_PositionChanged);

    // setup the decreaseButton button
    decreaseButton = CreateDecreaseButton();
    AddChild(decreaseButton);
    decreaseButton.Clicked += new GuiEventHandler(decreaseButton_Clicked);

    // setup the increaseButton button
    increaseButton = CreateIncreaseButton();
    AddChild(increaseButton);
    increaseButton.Clicked += new GuiEventHandler(increaseButton_Clicked);

    // do initial layout
    LayoutComponentWidgets();
  }

  #endregion Methods

  #endregion Window Members

  #region Events

  #region Declarations

  /// <summary>
  ///		Occurs when the thumb is moved or the value is modified programatically.
  /// </summary>
  public event WindowEventHandler ScrollPositionChanged;

  #endregion Declarations

  #region Trigger Methods

  /// <summary>
  ///		Triggers the <see cref="ScrollPositionChanged"/> event.
  /// </summary>
  /// <param name="e">Event arguments.</param>
  protected internal void OnScrollPositionChanged(WindowEventArgs e) {
    if(ScrollPositionChanged != null) {
      ScrollPositionChanged(this, e);
    }
  }

  #endregion Trigger Methods

  #region Overridden Trigger Methods

  /// <summary>
  ///		When the scrollbar is being clicked, adjust the position by the step SizeF.
  /// </summary>
  /// <param name="e">Event arguments.</param>
  protected internal override void OnMouseButtonsDown(MouseEventArgs e) {
    // default processing
    base.OnMouseButtonsDown(e);

    if(e.Button == System.Windows.Forms.MouseButtons.Left) {
      float adj = GetAdjustDirectionFromPoint(e.Position);

      // adjust scroll bar position in whichever direction as required.
      if(adj != 0) {
        ScrollPosition = (position + ((pageSize - overlapSize) * adj));
      }

      e.Handled = true;
    }
  }

  /// <summary>
  ///		When we are SizeF, re-layout the component widgets.
  /// </summary>
  /// <param name="e">Event arguments.</param>
  protected internal override void OnSized(GuiEventArgs e) {
    // defualt processing
    base.OnSized(e);

    // lay out the widgets again
    LayoutComponentWidgets();

    e.Handled = true;
  }

  #endregion Overridden Trigger Methods

  #region Handlers

  /// <summary>
  ///		Handles the thumb position moving event.
  /// </summary>
  /// <param name="sender">Source object.</param>
  /// <param name="e">Event arguments.</param>
  private void thumb_PositionChanged(object sender, WindowEventArgs e) {
    // adjust scroll bar position as required.
    ScrollPosition = GetPositionFromThumb();
  }

  /// <summary>
  ///		Handles the decreaseButton button being clicked.
  /// </summary>
  /// <param name="sender">Source object.</param>
  /// <param name="e">Event arguments.</param>
  private void decreaseButton_Clicked(object sender, GuiEventArgs e) {
    ScrollPosition = position - stepSize;
  }

  /// <summary>
  ///		Handles the increaseButton button being clicked.
  /// </summary>
  /// <param name="sender">Source object.</param>
  /// <param name="e">Event arguments.</param>
  private void increaseButton_Clicked(object sender, GuiEventArgs e) {
    ScrollPosition = position + stepSize;
  }

  #endregion Handlers

  #endregion Events
}

} // namespace CeGui.Widgets
