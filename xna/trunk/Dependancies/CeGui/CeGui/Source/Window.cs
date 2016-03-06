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
using System.Collections.Generic;
using System.Drawing;
using CeGui.Widgets;
using System.Diagnostics;

namespace CeGui {

/// <summary>
///     Definition of a Window class.
/// </summary>
public abstract class Window : PropertySet, IDisposable {
  // -----------------------------
  #region Fields

  #region General Data

  /// <summary>
  /// String holding the type name for the Window (is also the name of the WindowFactory that created us)
  /// </summary>
  protected string type;
  /// <summary>
  ///		GuiSystem unique name of this window.
  /// </summary>
  protected string name;
  /// <summary>
  /// Type name of the window as defined in a Falagard mapping.
  /// </summary>
  protected string falagardType;

  /// <summary>
  /// Name of the Look assigned to this window (if any).
  /// </summary>
  protected string lookName;

  /// <summary>
  ///     List of child windows attached to this window.
  /// </summary>
  protected WindowList children = new WindowList();
  /// <summary>
  /// Child window objects arranged in rendering order.
  /// </summary>
  protected WindowList drawList = new WindowList();

  /// <summary>
  ///		Holds the active metrics mode for this window.
  /// </summary>
  protected MetricsMode metricsMode;
  /// <summary>
  ///     Window that has captured inputs.
  /// </summary>
  protected static Window captureWindow;
  /// <summary>
  ///     Previous window to have mouse capture.
  /// </summary>
  protected Window oldCapture;
  /// <summary>
  ///     This window's parent window.
  /// </summary>
  protected Window parent;
  /// <summary>
  ///		Holds reference to the Window object's current Font.
  /// </summary>
  protected Font font;
  /// <summary>
  ///     Text / label/ caption for this window.
  /// </summary>
  protected string text;
  /// <summary>
  ///     Optional user defined ID for this window.
  /// </summary>
  protected int id;
  /// <summary>
  ///		Alpha transparency setting for the Window.
  /// </summary>
  protected float alpha;
  /// <summary>
  ///		This Window objects area (pixels relative to parent.
  /// TODO: convert this over to a UDim/URect structure
  /// </summary>
  protected Rect absArea;
  /// <summary>
  ///		This Window objects area (decimal fractions relative to parent).
  /// </summary>
  protected Rect relArea;
  /// <summary>
  /// Current constrained pixel SizeF of the window.
  /// </summary>
  protected SizeF pixelSize;
  /// <summary>
  ///		Holds reference to the Window objects current mouse cursor image.
  /// </summary>
  protected Image mouseCursor;
  /// <summary>
  /// Holds pointer to some user assigned data.
  /// </summary>
  protected object userData;
  /// <summary>
  /// Holds a collection of named user string values.
  /// </summary>
  protected StringDictionary userStrings;

  /// <summary>
  /// Specifies the base for horizontal alignment.
  /// </summary>
  protected HorizontalAlignment horzAlignment;
  /// <summary>
  /// Specifies the base for vertical alignment.
  /// </summary>
  protected VerticalAlignment vertAlignment;

  #endregion General Data

    /// <summary>
    /// 
    /// </summary>
  public void PerformChildWindowLayout() {
    // TODO: Implement me!
  }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="definition"></param>
  public void AddProperty(PropertyDefinition definition) {
    // TODO: Implement me!
  }    

  #region Min/Max Sizes

  /// <summary>
  ///		Current minimum SizeF for the window (this is always stored in pixels).
  /// </summary>
  protected SizeF minSize;
  /// <summary>
  ///		Current maximum SizeF for the window (this is always stored in pixels).
  /// </summary>
  protected SizeF maxSize;

  #endregion Min/Max Sizes

  #region Settings

  /// <summary>
  ///     True when window is enabled.
  /// </summary>
  protected bool isEnabled;
  /// <summary>
  ///     True if window is visible (not hidden).
  /// </summary>
  protected bool isVisible;
  /// <summary>
  ///     True if the window is active (has input focus).
  /// </summary>
  protected bool isActive;
  /// <summary>
  ///     True if this window is to be kept within it's parents area.
  /// </summary>
  protected bool isClippedByParent;
  /// <summary>
  ///		True when Window will be auto-destroyed by parent.
  /// </summary>
  protected bool isDestroyedByParent;
  /// <summary>
  ///     True if this is a top-most window (i.e. Always on top).
  /// </summary>
  protected bool isAlwaysOnTop;
  /// <summary>
  ///		True if the Window inherits alpha from the parent Window.
  /// </summary>
  protected bool inheritsAlpha;
  /// <summary>
  ///     True if window should restore any 'old' capture when it releases capture.
  /// </summary>
  protected bool restoreOldCapture;
  /// <summary>
  /// true if the Window responds to z-order change requests.
  /// </summary>
  protected bool zOrderingEnabled;
  /// <summary>
  /// true if the Window wishes to hear about multi-click mouse events.
  /// </summary>
  protected bool wantsMultiClicks;
  /// <summary>
  /// true if unhandled captured inputs should be distributed to child windows.
  /// </summary>
  protected bool distCapturedInputs;
  /// <summary>
  /// True if the window should come to the front of the z order in respose to a left mouse button down event.
  /// </summary>
  protected bool riseOnClick;

  #endregion Settings

  #region Mouse Data
  /// <summary>
  /// true if button will auto-repeat mouse button down events while mouse button is held down,
  /// </summary>
  protected bool autoRepeat;
  /// <summary>
  /// seconds before first repeat event is fired
  /// </summary>
  protected float repeatDelay;
  /// <summary>
  /// secons between further repeats after delay has expired.
  /// </summary>
  protected float repeatRate;
  /// <summary>
  /// implements repeating - is true after delay has elapsed,
  /// </summary>
  protected bool repeating;
  /// <summary>
  /// implements repeating - tracks time elapsed.
  /// </summary>
  protected float repeatElapsed;
  /// <summary>
  /// Button we're tracking (implication of this is that we only support one button at a time).
  /// </summary>
  protected System.Windows.Forms.MouseButtons repeatButton;
  #endregion Mouse Data

  #region Tooltips
  /// <summary>
  /// Text string used as tip for this window.
  /// </summary>
  protected string tooltipText;
  ///// <summary>
  ///// Possible custom Tooltip for this window.
  ///// TODO: Implement Tooltip class
  ///// </summary>
  //protected Tooltip customTip;
  /// <summary>
  /// true if this Window created the custom Tooltip.
  /// </summary>
  protected bool weOwnTip;
  /// <summary>
  /// true if the Window inherits tooltip text from its parent (when none set for itself).
  /// </summary>
  protected bool inheritsTipText;
  #endregion Tooltips

  #region Rendering
  /// <summary>
  /// Object which acts as a cache for Images to be drawn by this Window.
  /// </summary>
  protected RenderCache renderCache;
  /// <summary>
  /// true if window image cache needs to be regenerated.
  /// </summary>
  protected bool needsRedraw;
  #endregion Rendering

  #endregion Fields
  // -----------------------------

  // -----------------------------
  #region Constructor

  /// <summary>
  ///     Constructor for Window base class
  /// </summary>
  /// <param name="type">String object holding Window type (usually provided by WindowFactory).</param>
  /// <param name="name">String object holding unique name for the Window.</param>
  public Window(string type, string name) {
    this.type = type;
    this.name = name;

    // setup
    metricsMode = MetricsMode.Relative;
    parent = null;
    font = null;
    id = 0;
    alpha = 1.0f;
    userData = null;
    needsRedraw = false;

    mouseCursor = GuiSystem.Instance.DefaultMouseCursor;

    // settings
    isEnabled = true;
    isVisible = true;
    isActive = false;
    isClippedByParent = true;
    isDestroyedByParent = true;
    isAlwaysOnTop = false;
    inheritsAlpha = true;
    restoreOldCapture = false;
    zOrderingEnabled = true;
    wantsMultiClicks = true;
    distCapturedInputs = false;
    riseOnClick = true;

    // initialise mouse button auto-repeat state
    // TODO: Add the MouseButtons.None enumeration to the list
    repeatButton = System.Windows.Forms.MouseButtons.Left;
    autoRepeat = false;
    repeating = false;
    repeatDelay = 0.3f;
    repeatRate = 0.06f;

    // tooltip setup
    //customTip = null;
    weOwnTip = false;
    inheritsTipText = false;

    // position and SizeF
    absArea = new Rect(0, 0, 0, 0);
    relArea = new Rect(0, 0, 0, 0);
    pixelSize = new SizeF(0, 0);

    horzAlignment = HorizontalAlignment.Left;
    vertAlignment = VerticalAlignment.Top;

    // TODO: Change these to named constants
    minSize = new SizeF(0, 0);
    maxSize = new SizeF(1024, 768);  //640x480

    // init to empty text
    text = string.Empty;
  }

  #endregion Constructor
  // -----------------------------

  // -----------------------------
  #region Properties

  /// <summary>
  /// Get the vertical alignment.
  /// 
  /// Returns the vertical alignment for the window.  This setting affects how the windows position is
  /// interpreted relative to its parent.
  /// </summary>
  /// <value>
  /// One of the VerticalAlignment enumerated values.
  /// </value>
  public VerticalAlignment VerticalAlignment {
    get { return vertAlignment; }
    set {
      if(vertAlignment != value) {
        vertAlignment = value;
        // TODO: Trigger an event for the update
      }
    }
  }

  /// <summary>
  /// Get the horizontal alignment.
  /// 
  /// Returns the horizontal alignment for the window.  This setting affects how the windows position is
  /// interpreted relative to its parent.
  /// </summary>
  /// <value>
  /// One of the HorizontalAlignment enumerated values.
  /// </value>
  public HorizontalAlignment HorizontalAlignment {
    get { return horzAlignment; }
    set {
      horzAlignment = value;
      // TODO: Trigger an event for the update
    }
  }

  /// <summary>
  ///		Get/Set whether or not this Window will automatically be destroyed when its parent Window is destroyed.
  /// </summary>
  /// <value>
  ///		true to have the Window auto-destroyed when its parent is destroyed (default), or false to have the Window
  ///		remain after its parent is destroyed.
  /// </value>
  [WidgetProperty("DesroyedByParent")]
  public bool DestroyedByParent {
    get {
      return isDestroyedByParent;
    }
    set {
      isDestroyedByParent = value;
    }
  }

  /// <summary>
  ///		Gets the effective alpha value that will be used when rendering this window, taking into account inheritance of parent
  ///		window(s) alpha.
  /// </summary>
  /// <value>The effective alpha that will be applied to this Window when rendering.  Will be between 0.0f and 1.0f.</value>
  public float EffectiveAlpha {
    get {
      if((parent == null) || (!this.inheritsAlpha)) {
        return alpha;
      }

      return alpha * parent.EffectiveAlpha;
    }
  }

  /// <summary>
  ///		Returns the reference to the active Font for this window.
  /// </summary>
  /// <value>
  ///		Reference to the Font being used by this Window.  
  ///		If the window has no assigned font, the default font is returned.
  ///	</value>
  [WidgetProperty("Font")]
  public Font Font {
    get {
      // just use the default system font if this window doesn't have
      // its own specified
      if(font == null) {
        return GuiSystem.Instance.DefaultFont;
      }

      return font;
    }
    set {
      font = value;
      OnFontChanged(new GuiEventArgs());
    }
  }

  /// <summary>
  ///		Get/Set whether this Window will inherit alpha from its parent windows.
  /// </summary>
  /// <value>true if the Window should use inherited alpha, or false if the Window should have an independant alpha value.</value>
  [WidgetProperty("InheritsAlpha")]
  public bool InheritsAlpha {
    get {
      return inheritsAlpha;
    }
    set {
      inheritsAlpha = value;
      OnInheritsAlphaChanged(new GuiEventArgs());
    }
  }

  /// <summary>
  ///     Gets/Sets whether this window is 'always on top' or not.
  /// </summary>
  /// <value>
  ///		true if this Window is always show on top of other normal windows.  
  ///		false if the Window has normal z-order behavior.
  ///	</value>
  [WidgetProperty("AlwaysOnTop")]
  public bool AlwaysOnTop {
    get {
      return isAlwaysOnTop;
    }
    set {
      // only react to an actual change
      if(isAlwaysOnTop != value) {
        isAlwaysOnTop = value;

        // move us infront of sibling windows with the same 'always-on-top' setting as we have.
        if(parent != null) {
          Window origParent = parent;

          // remove the window from parents list
          origParent.RemoveChildImpl(this);

          // re-add window to parent, which will place it behind all top-most windows,
          // which in either case is the right place for this window
          origParent.AddChildImpl(this);

          OnZChangedImpl();
        }

        OnAlwaysOnTopChanged(new GuiEventArgs());
      }
    }
  }

  /// <summary>
  ///     Returns true if this window is in a disabled state.
  /// </summary>
  public bool IsEnabled {
    get {
      bool parentEnabled =
          (parent == null) ? true : parent.IsEnabled;

      return (isEnabled && parentEnabled);
    }
    set {
      if(isEnabled != value) {
        isEnabled = value;

        if(isEnabled) {
          OnEnabled(new GuiEventArgs());
        } else {
          OnDisabled(new GuiEventArgs());
        }
      }
    }
  }

  /// <summary>
  ///     Returns true if this window is in a disabled state.
  /// </summary>
  [WidgetProperty("Disabled")]
  public bool IsDisabled {
    get {
      return !IsEnabled;
    }
    set {
      IsEnabled = !value;
    }
  }

  /// <summary>
  ///     Returns true if this window is visible (not hidden).
  /// </summary>
  [WidgetProperty("Visible")]
  public bool Visible {
    get {
      bool parentVisible =
          (parent == null) ? true : parent.Visible;

      return (isVisible && parentVisible);
    }
    set {
      if(isVisible != value) {
        isVisible = value;

        if(isVisible) {
          OnShown(new GuiEventArgs());
        } else {
          OnHidden(new GuiEventArgs());
        }
      }
    }
  }

  /// <summary>
  ///     Returns true if this window is active. 
  /// </summary>
  /// <remarks>
  ///     The active window is always the front most window.
  /// </remarks>
  public bool IsActive {
    get {
      bool parentActive =
          (parent == null) ? true : parent.IsActive;

      return (isActive && parentActive);
    }
  }

  /// <summary>
  ///     Returns true if this window is to be clipped by it's parent.
  /// </summary>
  [WidgetProperty("ClippedByParent")]
  public bool IsClippedByParent {
    get {
      return isClippedByParent;
    }
    set {
      if(isClippedByParent != value) {
        isClippedByParent = value;
        OnClippingChanged(new GuiEventArgs());
      }
    }
  }

  /// <summary>
  ///     Gets/Sets the width of the window (in unspecified units).
  /// </summary>
  [WidgetProperty("Width")]
  public float Width {
    get {
      if(this.MetricsMode == MetricsMode.Relative) {
        return relArea.Width;
      }

      return absArea.Width;
    }
    set {
      this.Size = new SizeF(value, Height);
    }
  }

  /// <summary>
  ///     Gets/Sets the height of the window (in unspecified units).
  /// </summary>
  /// <remarks>
  ///		Interpretation of the value is dependant upon the current metrics system set for the Window.
  /// </remarks>
  [WidgetProperty("Height")]
  public float Height {
    get {
      if(this.MetricsMode == MetricsMode.Relative) {
        return relArea.Height;
      }

      return absArea.Height;
    }
    set {
      this.Size = new SizeF(Width, value);
    }
  }

  /// <summary>
  ///     Get/Set the x position of the window.
  /// </summary>
  /// <remarks>Interpretation of return value depends upon the metric type in use by this window.</remarks>
  /// <value>
  ///		float value that specifies the x position of the Window relative to it's parent, depending on the metrics system in use for this
  ///		Window, this value will specify either pixels or a decimal fraction of the width of the parent Window.
  /// </value>
  [WidgetProperty("XPosition")]
  public float X {
    get {
      if(this.MetricsMode == MetricsMode.Relative) {
        return relArea.Left;
      }

      return absArea.Left;
    }
    set {
      this.Position = new PointF(value, Y);
    }
  }

  /// <summary>
  ///     Get/Set the y position of the window.
  /// </summary>
  /// <remarks>Interpretation of return value depends upon the metric type in use by this window.</remarks>
  /// <value>
  ///		float value that specifies the y position of the Window relative to it's parent, depending on the metrics system in use for this
  ///		Window, this value will specify either pixels or a decimal fraction of the width of the parent Window.
  /// </value>
  [WidgetProperty("YPosition")]
  public float Y {
    get {
      if(this.MetricsMode == MetricsMode.Relative) {
        return relArea.Top;
      }

      return absArea.Top;
    }
    set {
      this.Position = new PointF(X, value);
    }
  }

  /// <summary>
  ///     Gets/Sets the ID assigned to this window.
  /// </summary>
  [WidgetProperty("ID")]
  public int ID {
    get {
      return id;
    }
    set {
      id = value;
    }
  }

  /// <summary>
  ///		Get/Set the maximum SizeF for this window.
  /// </summary>
  /// <value>
  ///		SizeF describing the maximum SizeF for the window.  For absolute metrics, the SizeF values are in screen pixels,
  ///		for relative metrics the SizeF values are relative to the display SizeF.
  /// </value>
  [WidgetProperty("MaxSize")]
  public SizeF MaximumSize {
    get {
      if(this.MetricsMode == MetricsMode.Absolute) {
        return maxSize;
      } else {
        return AbsoluteToRelativeImpl(null, maxSize);
      }
    }
    set {
      if(this.MetricsMode == MetricsMode.Absolute) {
        maxSize = value;
      } else {
        maxSize = RelativeToAbsoluteImpl(null, value);
      }

      absArea.ConstrainSizeMax(maxSize);
    }
  }

  /// <summary>
  ///		Set the minimum SizeF for this window.
  /// </summary>
  /// <value>
  ///		SizeF describing the minimum SizeF for the window.  For absolute metrics, the SizeF values are in screen pixels,
  ///		for relative metrics the SizeF values are relative to the display SizeF
  /// </value>
  [WidgetProperty("MinSize")]
  public SizeF MinimumSize {
    get {
      if(this.MetricsMode == MetricsMode.Absolute) {
        return minSize;
      } else {
        return AbsoluteToRelativeImpl(null, minSize);
      }
    }
    set {
      if(this.MetricsMode == MetricsMode.Absolute) {
        minSize = value;
      } else {
        minSize = RelativeToAbsoluteImpl(null, value);
      }

      absArea.ConstrainSizeMin(minSize);
    }
  }

  /// <summary>
  ///		Gets/Sets the current metrics mode employed by the window.
  /// </summary>
  /// <value>One of the values of the <see cref="MetricsMode"/> enumerated type, that describes the metrics mode to be used by the Window.</value>
  [MetricsModeProperty("MetricsMode")]
  public MetricsMode MetricsMode {
    get {
      if(metricsMode == MetricsMode.Inherited) {
        if(parent != null) {
          return parent.MetricsMode;
        }

        return MetricsMode.Relative;
      }

      return metricsMode;
    }
    set {
      metricsMode = value;
    }
  }

  /// <summary>
  ///		Get a reference to the mouse cursor image to use when the mouse is within this window.
  /// </summary>
  [WidgetProperty("MouseCursorImage")]
  public Image Cursor {
    get {
      if(mouseCursor != null && mouseCursor != GuiSystem.Instance.DefaultMouseCursor) {
        return mouseCursor;
      } else {
        return GuiSystem.Instance.DefaultMouseCursor;
      }
    }
  }

  /// <summary>
  ///		The name of this window.
  /// </summary>
  /// <value>The unique name of this window.</value>
  public string Name {
    get {
      return name;
    }
  }

  /// <summary>
  ///     Gets/Sets a reference to this window's parent window.
  /// </summary>
  public Window Parent {
    get {
      return parent;
    }
    set {
      parent = value;
    }
  }

  /// <summary>
  ///     Gets the number of child windows attached to this window.
  /// </summary>
  public int ChildCount {
    get {
      return children.Count;
    }
  }

  /// <summary>
  ///     Gets a reference to the top-most active child window starting at 'this'.
  /// </summary>
  /// <remarks>
  ///     Returns 'this' if it is 'this' window which is active.
  ///     Returns null if neither this window nor any children are active.
  /// </remarks>
  public Window ActiveChild {
    get {
      // if this window is not active just return null, since it's children can't be
      // active if 'this' is not.
      if(!this.IsActive) {
        return null;
      }

      // Travel through the child list(s) until we find the active window
      for(int i = 0; i < children.Count; i++) {
        Window child = children[i];

        if(child.IsActive) {
          return child.ActiveChild;
        }
      }

      // no child is active (or has no children), so return 'this' as active window
      return this;
    }
  }

  /// <summary>
  ///		Get/Set the current alpha value for this window.
  /// </summary>
  /// <remarks>
  ///		The alpha value set for any given window may or may not be the final alpha value that is used when rendering.  All window
  ///		objects, by default, inherit alpha from thier parent window(s) - this will blend child windows, relatively, down the line of
  ///		inheritance.  This behavior can be overridden via <see cref="InheritsAlpha"/>.  To return the true alpha value that will be
  ///		applied when rendering, use <see cref="EffectiveAlpha"/>.
  /// </remarks>
  [WidgetProperty("Alpha")]
  public float Alpha {
    get {
      return alpha;
    }
    set {
      alpha = value;
      OnAlphaChanged(new GuiEventArgs());
    }
  }

  /// <summary>
  ///		Gets a Rect describing the clipped inner area for this window.
  /// </summary>
  /// <value>Rect describing, in appropriately clipped screen pixel co-ordinates, the window object's inner Rect area.</value>
  public Rect InnerRect {
    get {
      // clip to parent?
      if(IsClippedByParent && parent != null) {
        return UnclippedInnerRect.GetIntersection(parent.InnerRect);
      }

      // clip to screen
      return UnclippedInnerRect.GetIntersection(GuiSystem.Instance.Renderer.Rect);
    }
  }

  /// <summary>
  ///     Returns true if input is captured by 'this'.
  /// </summary>
  public bool IsCapturedByThis {
    get {
      return captureWindow == this;
    }
  }

  /// <summary>
  ///     Returns true if input is capptured by some ancestor of 'this'.
  /// </summary>
  public bool IsCapturedByAncestor {
    get {
      return IsAncestor(captureWindow);
    }
  }

  /// <summary>
  ///     Returns true if input is captured by a child of 'this'.
  /// </summary>
  public bool IsCapturedByChild {
    get {
      return IsChild(captureWindow);
    }
  }

  /// <summary>
  ///		Gets a Rect describing the Window area in screen space.
  /// </summary>
  /// <remarks>
  ///		This has been made virtual to ease some customisations that require more specialised clipping requirements.
  /// </remarks>
  /// <value>
  ///		Rect object that describes the area covered by the Window.  The values in the returned Rect are in screen pixels.  The
  ///		returned Rect is clipped as appropriate and depending upon the 'ClippedByParent' setting.
  /// </value>
  public virtual Rect PixelRect {
    get {
      // clip to parent?
      if(IsClippedByParent && parent != null) {
        return UnclippedPixelRect.GetIntersection(parent.InnerRect);
      }

      // clip to screen
      return UnclippedPixelRect.GetIntersection(GuiSystem.Instance.Renderer.Rect);
    }
  }

  /// <summary>
  ///		Get/Set the position of the window.
  /// </summary>
  /// <remarks>Interpretation of return value depends upon the metric type in use by this window.</remarks>
  /// <value>
  ///		Point that describes the position of the Window relative to it's parent, depending on the metrics system in use for this
  ///		Window, the values in the Point will specify either pixels or decimal fractions of the total width and height of the parent.
  /// </value>
  [WidgetProperty("Position")]
  public PointF Position {
    get {
      if(this.MetricsMode == MetricsMode.Relative) {
        return relArea.Position;
      }

      return absArea.Position;
    }
    set {
      if(this.MetricsMode == MetricsMode.Relative) {
        relArea.Position = value;
        absArea.Position = RelativeToAbsoluteImpl(parent, value);
      } else {
        absArea.Position = value;
        relArea.Position = AbsoluteToRelativeImpl(parent, value);
      }

      OnMoved(new GuiEventArgs());
    }
  }

  /// <summary>
  ///		Gets a Rect object that describes the Window area.
  /// </summary>
  /// <value>
  ///		Rect that describes the area covered by the Window.  The values in the returned Rect are in whatever form is set
  ///		as the current metric type.  The returned Rect is unclipped and relative to the Window objects parent.
  /// </value>
  [WidgetProperty("Rect")]
  public Rect Rect {
    get {
      if(this.metricsMode == MetricsMode.Relative) {
        return relArea;
      }

      return absArea;
    }
    set {
      Position = value.Position;
      Size = value.Size;
    }
  }

  /// <summary>
  ///		Get/Set the SizeF of the window.
  /// </summary>
  /// <remarks>Interpretation of return value depends upon the metric type in use by this window.</remarks>
  /// <value>
  ///		SizeF that describes the dimensions of the Window.  Depending upon the metrics system in use for this window, the
  ///		values will either be in pixels, or as decimal fractions of the width and height of the parent Window.
  /// </value>
  [WidgetPropertyAttribute("SizeF")]
  public SizeF Size {
    get {
      if(this.MetricsMode == MetricsMode.Relative) {
        return relArea.Size;
      }

      return absArea.Size;
    }
    set {
      if(this.MetricsMode == MetricsMode.Relative) {
        relArea.Size = value;

        // update Rect for the other metrics system
        absArea.Size = RelativeToAbsoluteImpl(parent, value);
        absArea.ConstrainSize(maxSize, minSize);
      } else {
        absArea.Size = value;
        absArea.ConstrainSize(maxSize, minSize);

        // update Rect for the other metrics system
        relArea.Size = AbsoluteToRelativeImpl(parent, value);
      }

      OnSized(new GuiEventArgs());
    }
  }

  /// <summary>
  ///     Gets/Sets the window's text string.
  /// </summary>
  [StringPropertyAttribute("Text")]
  public string Text {
    get {
      return text;
    }
    set {
      text = value;

      // event notification
      OnTextChanged(new WindowEventArgs(this));
    }
  }

  /// <summary>
  ///		Return a Rect that describes, unclipped, the inner Rect for this window.  The inner Rect is
  ///		typically an area that excludes some frame or other rendering that should not be touched by subsequent rendering.
  /// </summary>
  /// <value>
  ///		Rect that describes, in unclipped screen pixel co-ordinates, the window object's inner Rect area.
  /// </value>
  public virtual Rect UnclippedInnerRect {
    get {
      return UnclippedPixelRect;
    }
  }

  /// <summary>
  ///		Gets a Rect describing the Window area unclipped, in screen space.
  /// </summary>
  /// <value>
  ///		Rect that describes the area covered by the Window.  The values in the returned 
  ///		Rect are in screen pixels.  The returned Rect is fully unclipped.
  /// </value>
  public Rect UnclippedPixelRect {
    get {
      if(this.MetricsMode == MetricsMode.Relative) {
        return WindowToScreen(new Rect(0, 0, 1, 1));
      } else {
        return WindowToScreen(new Rect(0, 0, absArea.Width, absArea.Height));
      }
    }
  }

//  public PropertySet Properties {
//    get { return (PropertySet)this; }
//  }

  #endregion Properties
  // -----------------------------

  // -----------------------------
  #region Static Properties

  /// <summary>
  ///     Returns a references to the window that currently has input capture, or null if none.
  /// </summary>
  public static Window CaptureWindow {
    get {
      return captureWindow;
    }
  }

  #endregion Static Properties
  // -----------------------------   

  #region Abstract Members

  #region Methods

  /// <summary>
  ///     Perform the actual rendering for this Window.
  /// </summary>
  /// <param name="z">float value specifying the base Z co-ordinate that should be used when rendering.</param>
  protected abstract void DrawSelf(float z);

  #endregion Methods

  #endregion Abstract Members

  // -----------------------------      
  #region Methods

  /// <summary>
  /// Set the LookNFeel that shoule be used for this window
  /// </summary>
  /// <param name="falagardType"></param>
  /// <param name="look">name of LookNFeel</param>
  public void SetLookNFeel(string falagardType, string look) {
    if(lookName.Length != 0) {
      this.falagardType = falagardType;
      lookName = look;
      Logger.Instance.LogEvent("Assigning LookNFeel '" + look + "' to window '" + name + "'.", LoggingLevel.Informative);

      // Work to initialse the look and feel...
      // TODO: Port the WidgetLookFeel class
    } else {
      throw new Exception("Window::SetLookNFeel - The window '" + name + "' already has a look assigned (" + lookName + ").");
    }
  }

  /// <summary>
  /// Set position of window
  /// </summary>
  /// <param name="x">window's left edge</param>
  /// <param name="y">window's top edge</param>
  public void MoveTo(float x, float y) {
    this.Position = new PointF(x, y);
  }

  /// <summary>
  /// Set size of window
  /// </summary>
  /// <param name="width">window's width</param>
  /// <param name="height">window's height</param>
  public void Resize(float width, float height) {
    this.Size = new SizeF(width, height);
  }

  /// <summary>
  ///		Activate the Window giving it input focus and bringing it to the top of all non always-on-top Windows.
  /// </summary>
  /// <remarks>
  ///		A Window cannot be programmatically 'disabled', as such.  To disable a Window, you must activate another one.
  /// </remarks>
  public void Activate() {
    MoveToFront();
  }

  /// <summary>
  ///		Add the named Window as a child of this Window.  If the Window \a name is already attached to a Window, it is detached before
  ///		being added to this Window.
  /// </summary>
  /// <param name="name">The name of the Window to be added.</param>
  public void AddChild(string name) {
    AddChild(WindowManager.Instance.GetWindow(name));
  }

  /// <summary>
  ///     Add the specified Window as a child of this Window.  If the Window \a window is already attached to a Window, it is detached before
  ///     being added to this Window.
  /// </summary>
  /// <param name="window">Reference to a window to add as a child.</param>
  public void AddChild(Window window) {
    AddChildImpl(window);
    OnChildAdded(new WindowEventArgs(window));
    window.OnZChangedImpl();
  }

  /// <summary>
  ///		Removes the child window with the specified name.
  /// </summary>
  /// <param name="name">Name of the child window to remove.</param>
  public void RemoveChild(string name) {
    Window child = GetChild(name);

    if(child != null) {
      RemoveChild(child);
    }
  }

  /// <summary>
  ///		Removes the child window with the specified ID.
  /// </summary>
  /// <param name="id">ID of the child to remove.</param>
  public void RemoveChild(int id) {
    Window child = GetChild(id);

    if(child != null) {
      RemoveChild(child);
    }
  }

  /// <summary>
  ///     Removes 'window' from this window's child list.
  /// </summary>
  /// <param name="window">Reference to a window to add as a child.</param>
  public void RemoveChild(Window window) {
    RemoveChildImpl(window);
    OnChildRemoved(new WindowEventArgs(window));
    window.OnZChangedImpl();
  }

  /// <summary>
  ///		Return a reference to the child window with the specified name.
  /// </summary>
  /// <remarks>
  ///		This function will throw an exception if no child object with the given name is attached.  This decision
  ///		was made (over returning 'null' if no window was found) so that client code can assume that if the call
  ///		returns it has a valid window pointer.  We provide the <see cref="IsChild(string)"/> functions for checking if a given window
  ///		is attached.
  /// </remarks>
  /// <param name="name">The name of the child window to return.</param>
  /// <returns>Window object attached to this window that has the specified <paramref name="name"/>.</returns>
  /// <exception cref="UnknownObjectException">Thrown if no window named <paramref name="name"/> is attached to this Window.</exception>
  public Window GetChild(string name) {
    for(int i = 0; i < children.Count; i++) {
      if(children[i].Name == name) {
        return children[i];
      }
    }

    throw new UnknownObjectException("There is no child named '{0}' attached to window '{1}'", name, this.Name);
  }

  /// <summary>
  ///		Return a reference to the first attached child window with the specified ID.
  /// </summary>
  /// <remarks>
  ///		This function will throw an exception if no child object with the given ID is attached.  This decision
  ///		was made (over returning 'null' if no window was found) so that client code can assume that if the call
  ///		returns it has a valid window pointer.  We provide the <see cref="IsChild(int)"/> functions for checking if a given window
  ///		is attached.
  /// </remarks>
  /// <param name="id">The ID of the child window to return.</param>
  /// <returns>The (first) Window object attached to this window that has the specified <paramref name="id"/>.</returns>
  /// <exception cref="UnknownObjectException">Thrown if no window named <paramref name="name"/> is attached to this Window.</exception>
  public Window GetChild(int id) {
    for(int i = 0; i < children.Count; i++) {
      if(children[i].ID == id) {
        return children[i];
      }
    }

    throw new UnknownObjectException("There is no child with ID '{0}' attached to window '{1}'", id, this.Name);
  }

  /// <summary>
  ///     Gets a reference to the child window at the specified index.
  /// </summary>
  /// <param name="index">Index of the child to retreive.</param>
  /// <returns>The child at the specified index, or null if the index is out of bounds.</returns>
  public Window GetChildAtIndex(int index) {
    if(index < 0 || index >= children.Count) {
      return null;
    }

    return (Window)children[index];
  }

  /// <summary>
  ///     Returns the child Window that is 'hit' by the given position
  /// </summary>
  /// <param name="position">Point that describes the position to check in screen pixels.</param>
  /// <returns>Child located at the specified position, or null if none exists.</returns>
  public Window GetChildAtPosition(PointF position) {
    // scan child list backwards (Windows towards the end of the list are considered 'in front of' those towards the begining)
    for(int i = children.Count - 1; i >= 0; i--) {
      Window child = children[i];

      // only check if the child is visible
      if(child.Visible) {
        // recursively scan children of this child windows...
        Window window = child.GetChildAtPosition(position);

        // return window if we found a 'hit' down the chain somewhere
        if(window != null) {
          return window;
        }
          // none of our children hit
      else {
          if(child is GuiSheet)
            continue; // can't click a GuiSheet. -- njk-patch
          // see if this child is hit and return it if it is
          if(child.IsHit(position)) {
            return child;
          }
        }
      }
    }

    // nothing hit
    return null;
  }

  /// <summary>
  ///		returns whether a Window with the specified name is currently 
  ///		attached to this Window as a child.
  /// </summary>
  /// <param name="name">The name of the Window to look for.</param>
  /// <returns>True if a Window named <paramref name="name" /> is currently attached to this Window as a child, else false.</returns>
  public bool IsChild(string name) {
    for(int i = 0; i < this.ChildCount; i++) {
      if(children[i].Name == name) {
        return true;
      }
    }

    return false;
  }

  /// <summary>
  ///		Returns whether at least one window with the given ID code is attached as a child.
  /// </summary>
  /// <remarks>
  ///		ID codes are client assigned and may or may not be unique, and as such, the return from this function
  ///		will only have meaning to the client code.
  /// </remarks>
  /// <param name="id">ID code to look for.</param>
  /// <returns>
  ///		True if a child window was found with the ID code <paramref name="id" />, 
  ///		or false if no child window was found with that id.
  /// </returns>
  public bool IsChild(int id) {
    for(int i = 0; i < this.ChildCount; i++) {
      if(children[i].ID == id) {
        return true;
      }
    }

    return false;
  }

  /// <summary>
  ///     Returns true if <paramref name="window"/> is a child window of 'this'.
  /// </summary>
  /// <param name="window">Window to look for.</param>
  /// <returns>True if <paramref name="window"/> is a child of 'this'.</returns>
  public bool IsChild(Window window) {
    for(int i = 0; i < this.ChildCount; i++) {
      if(children[i] == window) {
        return true;
      }
    }

    return false;
  }

  /// <summary>
  ///     Returns true if the Window named <paramref name="name"/> is some ancestor of 'this'.
  /// </summary>
  /// <param name="name">Name of the Window to look for.</param>
  /// <returns>True if a Window named <paramref name="name"/> is an ancestor of 'this'.</returns>
  public bool IsAncestor(string name) {
    // if parent is false, we have no ancestors
    if(parent == null) {
      return false;
    }

    // return true if our immediate parent is a match
    if(parent.name == name) {
      return true;
    } else {
      // scan back up the line until we get a result
      return parent.IsAncestor(name);
    }
  }

  /// <summary>
  ///     Returns true if the Window with ID <paramref name="id"/> is some ancestor of 'this'.
  /// </summary>
  /// <param name="id">ID of the Window to look for.</param>
  /// <returns>True if a Window with ID <paramref name="id"/> is an ancestor of 'this'.</returns>
  public bool IsAncestor(int id) {
    // if parent is false, we have no ancestors
    if(parent == null) {
      return false;
    }

    // return true if our immediate parent is a match
    if(parent.ID == id) {
      return true;
    } else {
      // scan back up the line until we get a result
      return parent.IsAncestor(id);
    }
  }

  /// <summary>
  ///     Returns true is <paramref name="window"/> is some ancestor of 'this'.
  /// </summary>
  /// <param name="window">Window to look for.</param>
  /// <returns>True if <paramref name="window"/> is an ancestor of 'this'.</returns>
  public bool IsAncestor(Window window) {
    // if parent is false, we have no ancestors
    if(parent == null) {
      return false;
    }

    // return true if our immediate parent is a match
    if(parent == window) {
      return true;
    } else {
      // scan back up the line until we get a result
      return parent.IsAncestor(window);
    }
  }

  /// <summary>
  ///     check if the given position would hit this window.
  /// </summary>
  /// <param name="position">Point describing the position to check in screen pixels.</param>
  /// <returns>True if the given point is within this window's area.</returns>
  public virtual bool IsHit(PointF position) {
    // if window is disabled, default is not to react
    if(!IsEnabled) {
      return false;
    }

    Rect clippedArea = PixelRect;

    if(clippedArea.Width == 0) {
      return false;
    }

    // return whether point is within this area
    return clippedArea.IsPointInRect(position);
  }

  /// <summary>
  ///		Move the Window to the bottom of the Z order.
  /// </summary>
  /// <remarks>
  ///		- If the window is non always-on-top the Window is sent to the very bottom of its sibling windows and the process repeated for all ancestors.
  ///		- If the window is always-on-top, the Window is sent to the bottom of all sibling always-on-top windows and the process repeated for all ancestors.
  /// </remarks>
  public void MoveToBack() {
    // if the window is active, deactivate it
    if(IsActive) {
      OnDeactivated(new WindowEventArgs(this));
    }

    // if the window has no parent then we can have no siblings and have nothing more to do.
    if(parent == null) {
      return;
    }

    // move us behind all sibling windows with the same 'always-on-top' setting as we have.
    Window orgParent = parent;
    parent.RemoveChildImpl(this);

    int pos = 0;

    if(AlwaysOnTop) {
      while((pos != ChildCount - 1) && (!children[pos].AlwaysOnTop)) {
        pos++;
      }
    }

    children.Insert(pos, this);
    Parent = orgParent;

    OnZChangedImpl();

    parent.MoveToBack();
  }

  /// <summary>
  ///		Move the Window to the top of the z order.
  /// </summary>
  /// <remarks>
  ///		- If the Window is a non always-on-top window it is moved the the top of all other non always-on-top sibling windows, and the process
  ///		repeated for all ancestors.
  ///		- If the Window is an always-on-top window it is moved to the of of all sibling Windows, and the process repeated for all ancestors.
  /// </remarks>
  public void MoveToFront() {
    // // if the window has no parent then we can have no siblings and have nothing more to do.
    if(parent == null) {
      // perform initial activation if required
      if(!IsActive) {
        OnActivated(new WindowEventArgs(null));
      }

      return;
    }

    // bring parent window to front of its siblings
    parent.MoveToFront();

    // get our sibling window which is currently active (if any)
    Window activeWindow = null;

    int idx = parent.ChildCount;

    while(--idx >= 0) {
      if(parent.children[idx].IsActive) {
        activeWindow = parent.children[idx];
        break;
      }
    }

    // move us infront of sibling windows with the same 'always-on-top' setting as we have.
    Window orgParent = parent;
    orgParent.RemoveChildImpl(this);
    orgParent.AddChildImpl(this);

    // notify ourselves that we have become active
    if(activeWindow != this) {
      OnActivated(new WindowEventArgs(activeWindow));
    }

    // notify previously active window that it is no longer active
    if((activeWindow != null) && (activeWindow != this)) {
      activeWindow.OnDeactivated(new WindowEventArgs(this));
    }

    OnZChangedImpl();
  }

  /// <summary>
  /// Unhooks this window and all it's children from the environment
  /// </summary>
  public void Destroy()
  {
    // because we know that people do not read the API ref properly,
    // check that they've called WindowManager to do the destruction
    Debug.Assert(!WindowManager.Instance.IsWindowPresent(Name));

    ReleaseInput();

    // signal our imminent destruction
    OnDestructionStarted(new WindowEventArgs(this));

    // double check we are detached from parent
    if(Parent != null) {
      Parent.RemoveChild(this);
    }

    CleanupChildren();
}

  #endregion Methods
  // -----------------------------    

  #region Protected/Private Methods

  /// <summary>
  ///		Cleanup child windows.
  /// </summary>
  private void CleanupChildren() {
    while(ChildCount != 0) {
      Window window = children[0];

      // always remove child
      RemoveChild(window);

      // destroy child if required
      if(window.DestroyedByParent) {
        WindowManager.Instance.DestroyWindow(window);
      }
    }
  }

  /// <summary>
  ///		Add given window to child list at an appropriate position.
  /// </summary>
  /// <param name="window">Window to add.</param>
  private void AddChildImpl(Window window) {
    // if window is already attached, detach it first (will fire normal events)
    if(window.Parent != null) {
      window.Parent.RemoveChild(window);
    }

    int position = (ChildCount == 0) ? 0 : ChildCount;

    if(!window.AlwaysOnTop) {
      // find last non-topmost window
      while((position != 0) && children[position - 1].AlwaysOnTop) {
        position--;
      }
    }

    // add window at the end
    if(position == ChildCount) {
      children.Add(window);
    } else {
      // insert before position
      children.Insert(position, window);
    }

    window.Parent = this;

    // force an update for the area Rects for 'window' so they are correct for its new parent
    window.OnParentSized(new WindowEventArgs(this));
  }

  /// <summary>
  ///		Remove given window from child list.
  /// </summary>
  /// <param name="window">Window to remove.</param>
  private void RemoveChildImpl(Window window) {
    if(children.Count != 0) {
      if(children[window.Name] != null) {
        children.Remove(window.Name);
        window.Parent = null;
      }
    }
  }

  /// <summary>
  ///		Notify 'this' and all siblings of a ZOrder change event.
  /// </summary>
  private void OnZChangedImpl() {
    if(parent == null) {
      OnZChanged(new GuiEventArgs());
    } else {
      GuiEventArgs nullArgs = new GuiEventArgs();

      for(int i = 0; i < parent.ChildCount; i++) {
        parent.children[i].OnZChanged(nullArgs);
      }
    }
  }

  /// <summary>
  ///		Gets the SizeF of the specified window.
  /// </summary>
  /// <param name="window">Window to get the SizeF for.</param>
  protected SizeF GetWindowSizeImpl(Window window) {
    if(window == null) {
      return GuiSystem.Instance.Renderer.Size;
    } else {
      return window.absArea.Size;
    }
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="window"></param>
  /// <param name="Rect"></param>
  /// <returns></returns>
  protected Rect AbsoluteToRelativeImpl(Window window, Rect Rect) {
    // get SizeF object for whatever we are using as a base for the conversion
    SizeF size = GetWindowSizeImpl(window);

    Rect tmp = new Rect();

    if(size.Width != 0) {
      tmp.Left = Rect.Left / size.Width;
      tmp.Right = Rect.Right / size.Width;
    } else {
      tmp.Left = tmp.Right = 0;
    }

    if(size.Height != 0) {
      tmp.Top = Rect.Top / size.Height;
      tmp.Bottom = Rect.Bottom / size.Height;
    } else {
      tmp.Top = tmp.Bottom = 0;
    }

    return tmp;
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="window"></param>
  /// <param name="size"></param>
  /// <returns></returns>
  protected SizeF AbsoluteToRelativeImpl(Window window, SizeF size) {
    // get SizeF object for whatever we are using as a base for the conversion
    SizeF windowSize = GetWindowSizeImpl(window);

    SizeF tmp = new SizeF();

    if(windowSize.Width != 0) {
      tmp.Width = size.Width / windowSize.Width;
    } else {
      tmp.Width = 0;
    }

    if(windowSize.Height != 0) {
      tmp.Height = size.Height / windowSize.Height;
    } else {
      tmp.Height = 0;
    }

    return tmp;
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="window"></param>
  /// <param name="point"></param>
  /// <returns></returns>
  protected PointF AbsoluteToRelativeImpl(Window window, PointF point) {
    // get SizeF object for whatever we are using as a base for the conversion
    SizeF size = GetWindowSizeImpl(window);

    PointF tmp = new PointF();

    if(size.Width != 0) {
      tmp.X = point.X / size.Width;
    } else {
      tmp.X = 0;
    }

    if(size.Height != 0) {
      tmp.Y = point.Y / size.Height;
    } else {
      tmp.Y = 0;
    }

    return tmp;
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="window"></param>
  /// <param name="x"></param>
  /// <returns></returns>
  protected float AbsoluteToRelativeXImpl(Window window, float x) {
    // get SizeF object for whatever we are using as a base for the conversion
    SizeF size = GetWindowSizeImpl(window);

    if(size.Width != 0) {
      return x / size.Width;
    } else {
      return 0;
    }
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="window"></param>
  /// <param name="y"></param>
  /// <returns></returns>
  protected float AbsoluteToRelativeYImpl(Window window, float y) {
    // get SizeF object for whatever we are using as a base for the conversion
    SizeF size = GetWindowSizeImpl(window);

    if(size.Height != 0) {
      return y / size.Height;
    } else {
      return 0;
    }
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="window"></param>
  /// <param name="Rect"></param>
  /// <returns></returns>
  protected Rect RelativeToAbsoluteImpl(Window window, Rect Rect) {
    // get SizeF object for whatever we are using as a base for the conversion
    SizeF size = GetWindowSizeImpl(window);

    return new Rect(
        Rect.Left * size.Width,
        Rect.Top * size.Height,
        Rect.Right * size.Width,
        Rect.Bottom * size.Height);
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="window"></param>
  /// <param name="Size"></param>
  /// <returns></returns>
  protected SizeF RelativeToAbsoluteImpl(Window window, SizeF Size) {
    // get SizeF object for whatever we are using as a base for the conversion
    SizeF windowSize = GetWindowSizeImpl(window);

    return new SizeF(
     Size.Width * windowSize.Width,
     Size.Height * windowSize.Height
   );
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="window"></param>
  /// <param name="point"></param>
  /// <returns></returns>
  protected PointF RelativeToAbsoluteImpl(Window window, PointF point) {
    // get SizeF object for whatever we are using as a base for the conversion
    SizeF size = GetWindowSizeImpl(window);

    return new PointF(
        point.X * size.Width,
        point.Y * size.Height);
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="window"></param>
  /// <param name="x"></param>
  /// <returns></returns>
  protected float RelativeToAbsoluteXImpl(Window window, float x) {
    // get SizeF object for whatever we are using as a base for the conversion
    SizeF size = GetWindowSizeImpl(window);

    return x * size.Width;
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="window"></param>
  /// <param name="y"></param>
  /// <returns></returns>
  protected float RelativeToAbsoluteYImpl(Window window, float y) {
    // get SizeF object for whatever we are using as a base for the conversion
    SizeF Size = GetWindowSizeImpl(window);

    return y * Size.Height;
  }

  #endregion Private Methods

  // -----------------------------
  #region Co-ordinate/SizeF Conversion Methods

  /// <summary>
  ///		Convert the given X co-ordinate from absolute to relative metrics.
  /// </summary>
  /// <param name="val">X co-ordinate specified in pixels relative to this Window (so 0 is this windows left edge).</param>
  /// <returns>A relative metric value that is equivalent to <paramref name="val"/>, given the Window objects current width.</returns>
  public float AbsoluteToRelativeX(float val) {
    return AbsoluteToRelativeXImpl(this, val);
  }

  /// <summary>
  ///		Convert the given Y co-ordinate from absolute to relative metrics.
  /// </summary>
  /// <param name="val">Y co-ordinate specified in pixels relative to this Window (so 0 is this windows top edge).</param>
  /// <returns>A relative metric value that is equivalent to <paramref name="val"/>, given the Window objects current height.</returns>
  public float AbsoluteToRelativeY(float val) {
    return AbsoluteToRelativeYImpl(this, val);
  }

  /// <summary>
  ///		Convert the given position from absolute to relative metrics.
  /// </summary>
  /// <param name="point">Point that describes a position specified in pixels relative to this Window (so 0,0 is this windows top-left corner).</param>
  /// <returns>A Point describing a relative metric position that is equivalent to <paramref name="point"/>, given the Window objects current SizeF.</returns>
  public PointF AbsoluteToRelative(PointF point) {
    return AbsoluteToRelativeImpl(this, point);
  }

  /// <summary>
  ///		Convert the given SizeF from absolute to relative metrics.
  /// </summary>
  /// <param name="size">SizeF that describes a SizeF specified in pixels.</param>
  /// <returns>A SizeF object describing a relative metric SizeF that is equivalent to <paramref name="SizeF"/>, given the Window objects current SizeF.</returns>
  public SizeF AbsoluteToRelative(SizeF size) {
    return AbsoluteToRelativeImpl(this, size);
  }

  /// <summary>
  ///		Convert the given area from absolute to relative metrics.
  /// </summary>
  /// <param name="Rect">Rect describing the area specified in pixels relative to this Window.</param>
  /// <returns>A Rect describing a relative metric area that is equivalent to <paramref name="Rect"/>, given the Window objects current SizeF.</returns>
  public Rect AbsoluteToRelative(Rect Rect) {
    return AbsoluteToRelativeImpl(this, Rect);
  }

  /// <summary>
  ///		Convert the given X co-ordinate from relative to absolute metrics.
  /// </summary>
  /// <param name="val">X co-ordinate specified in pixels relative to this Window (so 0 is this windows left edge).</param>
  /// <returns>An absolute  metric value that is equivalent to <paramref name="val"/>, given the Window objects current width.</returns>
  public float RelativeToAbsoluteX(float val) {
    return RelativeToAbsoluteXImpl(this, val);
  }

  /// <summary>
  ///		Convert the given Y co-ordinate from relative to absolute metrics.
  /// </summary>
  /// <param name="val">Y co-ordinate specified in pixels relative to this Window (so 0 is this windows top edge).</param>
  /// <returns>An absolute metric value that is equivalent to <paramref name="val"/>, given the Window objects current height.</returns>
  public float RelativeToAbsoluteY(float val) {
    return RelativeToAbsoluteYImpl(this, val);
  }

  /// <summary>
  ///		Convert the given position from relative to absolute metrics.
  /// </summary>
  /// <param name="point">Point that describes a position specified in pixels relative to this Window (so 0,0 is this windows top-left corner).</param>
  /// <returns>A Point describing a absolute metric position that is equivalent to <paramref name="point"/>, given the Window objects current SizeF.</returns>
  public PointF RelativeToAbsolute(PointF point) {
    return RelativeToAbsoluteImpl(this, point);
  }

  /// <summary>
  ///		Convert the given SizeF from relative to absolute metrics.
  /// </summary>
  /// <param name="size">SizeF that describes a SizeF specified in pixels.</param>
  /// <returns>A SizeF object describing an absolute metric SizeF that is equivalent to <paramref name="SizeF"/>, given the Window objects current SizeF.</returns>
  public SizeF RelativeToAbsolute(SizeF size) {
    return RelativeToAbsoluteImpl(this, size);
  }

  /// <summary>
  ///		Convert the given area from relative to absolute metrics.
  /// </summary>
  /// <param name="Rect">Rect describing the area specified in pixels relative to this Window.</param>
  /// <returns>A Rect describing an absolute metric area that is equivalent to <paramref name="Rect"/>, given the Window objects current SizeF.</returns>
  public Rect RelativeToAbsolute(Rect Rect) {
    return RelativeToAbsoluteImpl(this, Rect);
  }

  /// <summary>
  ///		Convert a window co-ordinate value, specified in whichever metrics mode is active, to a screen relative pixel co-ordinate.
  /// </summary>
  /// <param name="x">x co-ordinate value to be converted.</param>
  /// <returns>float value describing a screen co-ordinate that is equivalent to window co-ordinate <paramref name="x"/>.</returns>
  public float WindowToScreenX(float x) {
    Window window = this;
    float baseX = 0;

    while(window != null) {
      baseX += window.absArea.Left;
      window = window.parent;
    }

    if(this.MetricsMode == MetricsMode.Relative) {
      return baseX + RelativeToAbsoluteX(x);
    } else {
      return baseX + x;
    }
  }

  /// <summary>
  ///		Convert a window co-ordinate value, specified in whichever metrics mode is active, to a screen relative pixel co-ordinate.
  /// </summary>
  /// <param name="y">y co-ordinate value to be converted.</param>
  /// <returns>float value describing a screen co-ordinate that is equivalent to window co-ordinate <paramref name="y"/>.</returns>
  public float WindowToScreenY(float y) {
    Window window = this;
    float baseY = 0;

    while(window != null) {
      baseY += window.absArea.Top;
      window = window.parent;
    }

    if(this.MetricsMode == MetricsMode.Relative) {
      return baseY + RelativeToAbsoluteY(y);
    } else {
      return baseY + y;
    }
  }

  /// <summary>
  ///		Convert a window co-ordinate position, specified in whichever metrics mode is active, to a screen relative pixel co-ordinate position.
  /// </summary>
  /// <param name="point">Point describing the position to be converted.</param>
  /// <returns>Point object describing a screen co-ordinate position that is equivalent to window co-ordinate position <paramref name="point"/>.</returns>
  public PointF WindowToScreen(PointF point) {
    Window window = this;
    PointF basePoint = new PointF();

    while(window != null) {
      basePoint.X += window.absArea.Left;
      basePoint.Y += window.absArea.Top;
      window = window.parent;
    }

    if(this.MetricsMode == MetricsMode.Relative) {
      return Support.Add(basePoint, RelativeToAbsolute(point));
    } else {
      return Support.Add(basePoint, point);
    }
  }

  /// <summary>
  ///		Convert a window SizeF value, specified in whichever metrics mode is active, to a SizeF in pixels.
  /// </summary>
  /// <param name="size">SizeF describing the SizeF to be converted.</param>
  /// <returns>SizeF describing describing a SizeF in pixels that is equivalent to the window based <paramref name="Rect"/>.</returns>
  public SizeF WindowToScreen(SizeF size) {
    if(this.MetricsMode == MetricsMode.Relative) {
      return new SizeF(
          size.Width * absArea.Width,
          size.Height * absArea.Height);
    } else {
      return size;
    }
  }

  /// <summary>
  ///		Convert a window area, specified in whichever metrics mode is active, to a screen area.
  /// </summary>
  /// <param name="Rect">Rect describing the area to be converted.</param>
  /// <returns>Rect describing a screen area that is equivalent to window area <paramref name="Rect"/>.</returns>
  public Rect WindowToScreen(Rect Rect) {
    Window window = this;
    PointF basePoint = new PointF();

    while(window != null) {
      basePoint.X += window.absArea.Left;
      basePoint.Y += window.absArea.Top;
      window = window.parent;
    }

    Rect temp;

    if(this.MetricsMode == MetricsMode.Relative) {
      temp = RelativeToAbsolute(Rect);
    } else {
      temp = Rect;
    }

    temp.Offset(basePoint);

    return temp;
  }

  /// <summary>
  ///		Convert a screen relative pixel co-ordinate value to a window co-ordinate value, specified in whichever metrics mode is active.
  /// </summary>
  /// <param name="x">x co-ordinate value to be converted.</param>
  /// <returns>float value describing a window co-ordinate value that is equivalent to screen co-ordinate <paramref name="x"/>.</returns>
  public float ScreenToWindowX(float x) {
    x -= WindowToScreenX(0);

    if(this.MetricsMode == MetricsMode.Relative) {
      x /= absArea.Width;
    }

    return x;
  }

  /// <summary>
  ///		Convert a screen relative pixel co-ordinate value to a window co-ordinate value, specified in whichever metrics mode is active.
  /// </summary>
  /// <param name="y">y co-ordinate value to be converted.</param>
  /// <returns>float value describing a window co-ordinate value that is equivalent to screen co-ordinate <paramref name="y"/>.</returns>
  public float ScreenToWindowY(float y) {
    y -= WindowToScreenY(0);

    if(this.MetricsMode == MetricsMode.Relative) {
      y /= absArea.Height;
    }

    return y;
  }

  /// <summary>
  ///		Convert a screen relative pixel position to a window co-ordinate position, specified in whichever metrics mode is active.
  /// </summary>
  /// <param name="point">Point describing the position to be converted.</param>
  /// <returns>Point describing a window co-ordinate position that is equivalent to screen co-ordinate <paramref name="point"/>.</returns>
  public PointF ScreenToWindow(PointF point) {
    PointF temp = point;

    temp.X -= WindowToScreenX(0);
    temp.Y -= WindowToScreenY(0);

    if(this.MetricsMode == MetricsMode.Relative) {
      temp.X /= absArea.Width;
      temp.Y /= absArea.Height;
    }

    return temp;
  }

  /// <summary>
  ///		Convert a pixel screen SizeF to a window based SizeF, specified in whichever metrics mode is active.
  /// </summary>
  /// <param name="size">SizeF describing the area to be converted.</param>
  /// <returns>SizeF object describing a window based SizeF that is equivalent to screen based SizeF <paramref name="SizeF"/>.</returns>
  public SizeF ScreenToWindow(SizeF size) {
    SizeF temp = size;

    if(this.MetricsMode == MetricsMode.Relative) {
      temp.Width /= absArea.Width;
      temp.Height /= absArea.Height;
    }

    return temp;
  }

  /// <summary>
  ///		Convert a screen area to a window area, specified in whichever metrics mode is active.
  /// </summary>
  /// <param name="Rect">Rect describing the area to be converted.</param>
  /// <returns>Rect object describing a window area that is equivalent to screen area <paramref name="Rect"/>.</returns>
  public Rect ScreenToWindow(Rect Rect) {
    Rect temp = Rect;

    temp.Left -= WindowToScreenX(0);
    temp.Top -= WindowToScreenY(0);
    temp.Right -= WindowToScreenX(0);
    temp.Bottom -= WindowToScreenY(0);

    if(this.MetricsMode == MetricsMode.Relative) {
      temp.Left /= absArea.Width;
      temp.Top /= absArea.Height;
      temp.Right /= absArea.Width;
      temp.Bottom /= absArea.Height;
    }

    return temp;
  }

  #endregion Co-ordinate/SizeF Conversion Methods
  // -----------------------------

  // -----------------------------
  #region Clipboard Methods

  /// <summary>
  ///     Copy information to the clipboard.
  /// </summary>
  public virtual void CopyToClipboard() {
    if(parent != null) {
      parent.CopyToClipboard();
    }
  }

  /// <summary>
  /// 
  /// </summary>
  public virtual void CutToClipboard() {
    if(parent != null) {
      parent.CutToClipboard();
    }
  }

  /// <summary>
  /// 
  /// </summary>
  public virtual void PasteFromClipboard() {
    if(parent != null) {
      parent.PasteFromClipboard();
    }
  }

  #endregion Clipboard Methods
  // -----------------------------

  // -----------------------------
  #region Rendering Methods

  /// <summary>
  ///     Causes the Window object to render itself and all of it's attached children
  /// </summary>
  public void Render() {
    // don't do anything if window is not visible
    if(!Visible) {
      return;
    }

    // perform drawing for this window
    Renderer renderer = GuiSystem.Instance.Renderer;
    DrawSelf(renderer.CurrentZ);
    renderer.AdvanceZValue();

    // render any child windows
    for(int i = 0; i < ChildCount; i++) {
      children[i].Render();
    }
  }

  /// <summary>
  ///		Signal the System object to redraw (at least) this Window on the next render cycle.
  /// </summary>
  public void RequestRedraw() {
    GuiSystem.Instance.SignalRedraw();
  }

  #endregion Rendering Methods
  // -----------------------------

  // -----------------------------
  #region Manipulator Methods

  /// <summary>
  ///     Complete initialiZation of window (required so derived classes can affect initialisation).
  /// </summary>
  /// <remarks>
  ///     *MUST* be called before this window is used.
  /// </remarks>
  /// <returns></returns>
  public virtual void Initialize() {
  }

  /// <summary>
  ///     Enables this window.
  /// </summary>
  public void Enable() {
    isEnabled = true;
  }

  /// <summary>
  ///     Disable this window.
  /// </summary>
  public void Disable() {
    isEnabled = false;
  }

  /// <summary>
  ///     Show this window (make visible).
  /// </summary>
  public void Show() {
    Visible = true;
  }

  /// <summary>
  ///     Hide this window.
  /// </summary>
  public void Hide() {
    Visible = false;
  }

  /// <summary>
  ///     Capture input to this window.
  /// </summary>
  public void CaptureInput() {
    // we can only capture if we are the active window
    if(!IsActive) {
      return;
    }

    Window currentCapture = captureWindow;
    captureWindow = this;

    // inform any window which previously had mouse that it doesn't anymore!
    if((currentCapture != null) && (currentCapture != this) && (!restoreOldCapture)) {
      currentCapture.OnCaptureLost(new GuiEventArgs());
    }

    if(restoreOldCapture) {
      oldCapture = currentCapture;
    }

    // event notification
    OnCaptureGained(new GuiEventArgs());
  }

  /// <summary>
  ///     Releases the capture of input from this window.
  /// </summary>
  /// <remarks>
  ///		If this Window does not have inputs captured, nothing happens.
  /// </remarks>
  public void ReleaseInput() {
    // if we are not the window that has capture, do nothing
    if(!IsCapturedByThis) {
      return;
    }

    // restore old captured window if that mode is set
    if(restoreOldCapture) {
      captureWindow = oldCapture;

      // check for case when there was no previously captured window
      if(oldCapture != null) {
        oldCapture = null;
        captureWindow.MoveToFront();
      }
    } else {
      captureWindow = null;
    }

    // event notification
    OnCaptureLost(new GuiEventArgs());
  }

  /// <summary>
  ///		Set the current area for the Window, this allows for setting of position and SizeF at the same time.  
  ///		Interpretation of the input value <paramref name="area"/> is dependant upon the current metrics system set for the Window.
  /// </summary>
  /// <param name="area">Rect that describes the new area for Window, in units consistent with the current metrics mode.</param>
  public void SetAreaRect(Rect area) {
    if(this.MetricsMode == MetricsMode.Relative) {
      relArea = area;

      absArea = RelativeToAbsoluteImpl(parent, area);
      absArea.ConstrainSize(maxSize, minSize);
    } else {
      absArea = area;
      absArea.ConstrainSize(maxSize, minSize);

      relArea = AbsoluteToRelativeImpl(parent, area);
    }

    OnMoved(new GuiEventArgs());
    OnSized(new GuiEventArgs());
  }

  /// <summary>
  ///		Set the font used by this Window.
  /// </summary>
  /// <param name="name">
  ///		Name of a Font object to be used by this Window.  
  ///		If <paramref name="name"/> is null or "", the default font will be used.
  /// </param>
  /// <exception cref="UnknownObjectException">If the font with the specified name does not exist in the system.</exception>
  public void SetFont(string name) {
    if(name == null || name.Length == 0) {
      // copied here from the overload because of the ambiguity of SetFont(null)
      this.font = null;
      OnFontChanged(new GuiEventArgs());
    } else {
      SetFont(FontManager.Instance.GetFont(name));
    }
  }

  /// <summary>
  ///		Set the font used by this Window.
  /// </summary>
  /// <param name="font">
  ///		Font object to be used by this Window.  
  ///		If <paramref name="font"/> is null, the default font will be used.
  /// </param>
  public void SetFont(Font font) {
    this.font = font;
    OnFontChanged(new GuiEventArgs());
  }

  /// <summary>
  ///		Set the mouse cursor image to be used when the mouse enters this window.	
  /// </summary>
  /// <param name="imagesetName">The name of the Imageset that contains the image to be used.</param>
  /// <param name="imageName">The name of the Image on <paramref name="imageset"/> that is to be used.</param>
  public void SetMouseCursor(string imagesetName, string imageName) {
    mouseCursor = ImagesetManager.Instance.GetImageset(imagesetName).GetImage(imageName);
  }

  /// <summary>
  ///		Sets the mouse cursor to use when the mouse is within this window.
  /// </summary>
  /// <param name="image">Mouse cursor image to use.</param>
  public void SetMouseCursor(Image image) {
    mouseCursor = image;
  }

  /// <summary>
  ///     Sets the 'restore old capture' mode to on / off.
  /// </summary>
  /// <remarks>
  ///     Also sets the mode on all child controls. <p/>
  ///     Note: Intended for use by composite control sub-classes.
  /// </remarks>
  /// <param name="restore">On or off.</param>
  public void SetRestoreCapture(bool restore) {
    restoreOldCapture = restore;

    // trickle the changes down to the children
    for(int i = 0; i < ChildCount; i++) {
      children[i].SetRestoreCapture(restore);
    }
  }

  #region Properties

  /// <summary>
  ///		Return the window X position in absolute metrics.
  /// </summary>
  /// <value>float value describing this windows X position, relative to the parent window, in absolute metrics.</value>
  public float AbsoluteX {
    get {
      return absArea.Left;
    }
  }

  /// <summary>
  ///		Return the window Y position in absolute metrics.
  /// </summary>
  /// <value>float value describing this windows Y position, relative to the parent window, in absolute metrics.</value>
  public float AbsoluteY {
    get {
      return absArea.Top;
    }
  }

  /// <summary>
  ///		Return the window SizeF in absolute metrics.
  /// </summary>
  /// <value>SizeF describing this windows SizeF in absolute metrics.</value>
  public SizeF AbsoluteSize {
    get {
      return absArea.Size;
    }
  }

  /// <summary>
  ///		Return the window width in absolute metrics.
  /// </summary>
  /// <value>float value describing this windows width in absolute metrics.</value>
  public float AbsoluteWidth {
    get {
      return absArea.Width;
    }
  }

  /// <summary>
  ///		Return the window height in absolute metrics.
  /// </summary>
  /// <value>float value describing this windows height in absolute metrics.</value>
  public float AbsoluteHeight {
    get {
      return absArea.Height;
    }
  }

  #endregion Properties

  #endregion Manipulator Methods
  // -----------------------------

  // -----------------------------
  #region Events

  #region Trigger Methods

  /// <summary>
  ///      Event trigger method for the <see cref="Sized"/> event.
  /// </summary>
  /// <param name="e">Event information.</param>
  protected internal virtual void OnSized(GuiEventArgs e) {
    for(int i = 0; i < ChildCount; i++) {
      children[i].OnParentSized(new WindowEventArgs(this));
    }

    RequestRedraw();

    if(Sized != null) {
      Sized(this, e);
    }
  }
  /// <summary>
  ///      Event trigger method for the <see cref="Moved"/> event.
  /// </summary>
  /// <param name="e">Event information.</param>
  protected internal virtual void OnMoved(GuiEventArgs e) {
    RequestRedraw();

    if(Moved != null) {
      Moved(this, e);
    }
  }
  /// <summary>
  ///      Event trigger method for the <see cref="TextChanged"/> event.
  /// </summary>
  /// <param name="e">Event information.</param>
  protected internal virtual void OnTextChanged(WindowEventArgs e) {
    RequestRedraw();

    if(TextChanged != null) {
      TextChanged(this, e);
    }
  }
  /// <summary>
  ///      Event trigger method for the <see cref="FontChanged"/> event.
  /// </summary>
  /// <param name="e">Event information.</param>
  protected internal virtual void OnFontChanged(GuiEventArgs e) {
    RequestRedraw();

    if(FontChanged != null) {
      FontChanged(this, e);
    }
  }
  /// <summary>
  ///      Event trigger method for the <see cref="AlphaChanged"/> event.
  /// </summary>
  /// <param name="e">Event information.</param>
  protected internal virtual void OnAlphaChanged(GuiEventArgs e) {
    // scan child list and call this method for all children that inherit alpha
    for(int i = 0; i < ChildCount; i++) {
      if(children[i].InheritsAlpha) {
        children[i].OnAlphaChanged(e);
      }
    }

    RequestRedraw();

    if(AlphaChanged != null) {
      AlphaChanged(this, e);
    }
  }
  /// <summary>
  ///      Event trigger method for the <see cref="IDChanged"/> event.
  /// </summary>
  /// <param name="e">Event information.</param>
  protected internal virtual void OnIDChanged(GuiEventArgs e) {
    RequestRedraw();

    if(IDChanged != null) {
      IDChanged(this, e);
    }
  }
  /// <summary>
  ///      Event trigger method for the <see cref="Shown"/> event.
  /// </summary>
  /// <param name="e">Event information.</param>
  protected internal virtual void OnShown(GuiEventArgs e) {
    RequestRedraw();

    if(Shown != null) {
      Shown(this, e);
    }
  }
  /// <summary>
  ///      Event trigger method for the <see cref="Hidden"/> event.
  /// </summary>
  /// <param name="e">Event information.</param>
  protected internal virtual void OnHidden(GuiEventArgs e) {
    RequestRedraw();

    if(Hidden != null) {
      Hidden(this, e);
    }
  }
  /// <summary>
  ///      Event trigger method for the <see cref="Enabled"/> event.
  /// </summary>
  /// <param name="e">Event information.</param>
  protected internal virtual void OnEnabled(GuiEventArgs e) {
    RequestRedraw();

    if(Enabled != null) {
      Enabled(this, e);
    }
  }
  /// <summary>
  ///      Event trigger method for the <see cref="Disabled"/> event.
  /// </summary>
  /// <param name="e">Event information.</param>
  protected internal virtual void OnDisabled(GuiEventArgs e) {
    RequestRedraw();

    if(Disabled != null) {
      Disabled(this, e);
    }
  }
  /// <summary>
  ///      Event trigger method for the <see cref="MetricsChanged"/> event.
  /// </summary>
  /// <param name="e">Event information.</param>
  protected internal virtual void OnMetricsChanged(GuiEventArgs e) {
    RequestRedraw();

    if(MetricsChanged != null) {
      MetricsChanged(this, e);
    }
  }
  /// <summary>
  ///      Event trigger method for the <see cref="ClippingChanged"/> event.
  /// </summary>
  /// <param name="e">Event information.</param>
  protected internal virtual void OnClippingChanged(GuiEventArgs e) {
    RequestRedraw();

    if(ClippingChanged != null) {
      ClippingChanged(this, e);
    }
  }
  /// <summary>
  ///      Event trigger method for the <see cref="ParentDestroyChanged"/> event.
  /// </summary>
  /// <param name="e">Event information.</param>
  protected internal virtual void OnParentDestroyChanged(GuiEventArgs e) {
    if(ParentDestroyChanged != null) {
      ParentDestroyChanged(this, e);
    }
  }
  /// <summary>
  ///      Event trigger method for the <see cref="InheritsAlphaChanged"/> event.
  /// </summary>
  /// <param name="e">Event information.</param>
  protected internal virtual void OnInheritsAlphaChanged(GuiEventArgs e) {
    RequestRedraw();

    if(InheritsAlphaChanged != null) {
      InheritsAlphaChanged(this, e);
    }
  }
  /// <summary>
  ///      Event trigger method for the <see cref="AlwaysOnTopChanged"/> event.
  /// </summary>
  /// <param name="e">Event information.</param>
  protected internal virtual void OnAlwaysOnTopChanged(GuiEventArgs e) {
    RequestRedraw();

    if(AlwaysOnTopChanged != null) {
      AlwaysOnTopChanged(this, e);
    }
  }
  /// <summary>
  ///      Event trigger method for the <see cref="CaptureGained"/> event.
  /// </summary>
  /// <param name="e">Event information.</param>
  protected internal virtual void OnCaptureGained(GuiEventArgs e) {
    if(CaptureGained != null) {
      CaptureGained(this, e);
    }
  }
  /// <summary>
  ///      Event trigger method for the <see cref="CaptureLost"/> event.
  /// </summary>
  /// <param name="e">Event information.</param>
  protected internal virtual void OnCaptureLost(GuiEventArgs e) {
    // handle restore of previous capture window as required.
    if(restoreOldCapture && (oldCapture != null)) {
      oldCapture.OnCaptureLost(e);
      oldCapture = null;
    }

    // handle case where mouse is now in a different window
    // (this is a bit of a hack that uses the mouse input injector to handle this for us).
    GuiSystem.Instance.InjectMouseMove(0, 0);

    if(CaptureLost != null) {
      CaptureLost(this, e);
    }
  }
  /// <summary>
  ///      Event trigger method for the <see cref="RenderingStarted"/> event.
  /// </summary>
  /// <param name="e">Event information.</param>
  protected internal virtual void OnRenderingStarted(GuiEventArgs e) {
    if(RenderingStarted != null) {
      RenderingStarted(this, e);
    }
  }
  /// <summary>
  ///      Event trigger method for the <see cref="RenderingEnded"/> event.
  /// </summary>
  /// <param name="e">Event information.</param>
  protected internal virtual void OnRenderingEnded(GuiEventArgs e) {
    if(RenderingEnded != null) {
      RenderingEnded(this, e);
    }
  }
  /// <summary>
  ///      Event trigger method for the <see cref="ZChanged"/> event.
  /// </summary>
  /// <param name="e">Event information.</param>
  protected internal virtual void OnZChanged(GuiEventArgs e) {
    RequestRedraw();

    if(ZChanged != null) {
      ZChanged(this, e);
    }
  }
  /// <summary>
  ///      Event trigger method for the <see cref="DestructionStarted"/> event.
  /// </summary>
  /// <param name="e">Event information.</param>
  protected internal virtual void OnDestructionStarted(GuiEventArgs e) {
    if(DestructionStarted != null) {
      DestructionStarted(this, e);
    }
  }
  /// <summary>
  ///      Event trigger method for the <see cref="Activated"/> event.
  /// </summary>
  /// <param name="e">Event information.</param>
  protected internal virtual void OnActivated(WindowEventArgs e) {
    isActive = true;

    RequestRedraw();

    if(Activated != null) {
      Activated(this, e);
    }
  }
  /// <summary>
  ///      Event trigger method for the <see cref="Deactivated"/> event.
  /// </summary>
  /// <param name="e">Event information.</param>
  protected internal virtual void OnDeactivated(WindowEventArgs e) {
    // first, de-activate all children
    for(int i = 0; i < ChildCount; i++) {
      if(children[i].IsActive) {
        children[i].OnDeactivated(e);
      }
    }

    isActive = false;

    RequestRedraw();

    if(Deactivated != null) {
      Deactivated(this, e);
    }
  }
  /// <summary>
  ///      Event trigger method for the <see cref="ParentSized"/> event.
  /// </summary>
  /// <param name="e">Event information.</param>
  protected internal virtual void OnParentSized(WindowEventArgs e) {
    // synchronise area rects for new parent SizeF
    if(this.MetricsMode == MetricsMode.Relative) {
      absArea = RelativeToAbsoluteImpl(parent, relArea);

      // Check new absolute SizeF and limit to currently set max/min values.  This does not affect relative co-ordinates
      // which must 'recover' after window is again sized so normal relativity can take over.
      absArea.ConstrainSize(maxSize, minSize);
    } else {
      relArea = AbsoluteToRelativeImpl(parent, absArea);
    }

    OnSized(new GuiEventArgs());

    RequestRedraw();

    if(ParentSized != null) {
      ParentSized(this, e);
    }
  }
  /// <summary>
  ///      Event trigger method for the <see cref="ChildAdded"/> event.
  /// </summary>
  /// <param name="e">Event information.</param>
  protected internal virtual void OnChildAdded(WindowEventArgs e) {
    RequestRedraw();

    if(ChildAdded != null) {
      ChildAdded(this, e);
    }
  }
  /// <summary>
  ///      Event trigger method for the <see cref="ChildRemoved"/> event.
  /// </summary>
  /// <param name="e">Event information.</param>
  protected internal virtual void OnChildRemoved(WindowEventArgs e) {
    RequestRedraw();

    if(ChildRemoved != null) {
      ChildRemoved(this, e);
    }
  }
  /// <summary>
  ///      Event trigger method for the <see cref="MouseEnters"/> event.
  /// </summary>
  /// <param name="e">Event information.</param>
  protected internal virtual void OnMouseEnters(MouseEventArgs e) {
    // set the mouse cursor
    MouseCursor.Instance.SetImage(this.Cursor);

    if(MouseEnters != null) {
      MouseEnters(this, e);
    }
  }
  /// <summary>
  ///      Event trigger method for the <see cref="MouseLeaves"/> event.
  /// </summary>
  /// <param name="e">Event information.</param>
  protected internal virtual void OnMouseLeaves(MouseEventArgs e) {
    if(MouseLeaves != null) {
      MouseLeaves(this, e);
    }
  }
  /// <summary>
  ///      Event trigger method for the <see cref="MouseMove"/> event.
  /// </summary>
  /// <param name="e">Event information.</param>
  protected internal virtual void OnMouseMove(MouseEventArgs e) {
    if(MouseMove != null) {
      MouseMove(this, e);
    }
  }
  /// <summary>
  ///      Event trigger method for the <see cref="MouseWheel"/> event.
  /// </summary>
  /// <param name="e">Event information.</param>
  protected internal virtual void OnMouseWheel(MouseEventArgs e) {
    if(MouseWheel != null) {
      MouseWheel(this, e);
    }
  }
  /// <summary>
  ///      Event trigger method for the <see cref="MouseButtonsDown"/> event.
  /// </summary>
  /// <param name="e">Event information.</param>
  protected internal virtual void OnMouseButtonsDown(MouseEventArgs e) {
    if(e.Button == System.Windows.Forms.MouseButtons.Left) {
      MoveToFront();
    }

    if(MouseButtonsDown != null) {
      MouseButtonsDown(this, e);
    }
  }
  /// <summary>
  ///      Event trigger method for the <see cref="MouseButtonsUp"/> event.
  /// </summary>
  /// <param name="e">Event information.</param>
  protected internal virtual void OnMouseButtonsUp(MouseEventArgs e) {
    if(MouseButtonsUp != null) {
      MouseButtonsUp(this, e);
    }
  }
  /// <summary>
  ///      Event trigger method for the <see cref="MouseClicked"/> event.
  /// </summary>
  /// <param name="e">Event information.</param>
  protected internal virtual void OnMouseClicked(MouseEventArgs e) {
    if(MouseClicked != null) {
      MouseClicked(this, e);
    }
  }
  /// <summary>
  ///      Event trigger method for the <see cref="MouseDoubleClicked"/> event.
  /// </summary>
  /// <param name="e">Event information.</param>
  protected internal virtual void OnMouseDoubleClicked(MouseEventArgs e) {
    if(MouseDoubleClicked != null) {
      MouseDoubleClicked(this, e);
    }
  }
  /// <summary>
  ///      Event trigger method for the <see cref="MouseTripleClicked"/> event.
  /// </summary>
  /// <param name="e">Event information.</param>
  protected internal virtual void OnMouseTripleClicked(MouseEventArgs e) {
    if(MouseTripleClicked != null) {
      MouseTripleClicked(this, e);
    }
  }
  /// <summary>
  ///      Event trigger method for the <see cref="KeyDown"/> event.
  /// </summary>
  /// <param name="e">Event information.</param>
  protected internal virtual void OnKeyDown(KeyEventArgs e) {
    if(KeyDown != null) {
      KeyDown(this, e);
    }
  }
  /// <summary>
  ///      Event trigger method for the <see cref="KeyUp"/> event.
  /// </summary>
  /// <param name="e">Event information.</param>
  protected internal virtual void OnKeyUp(KeyEventArgs e) {
    if(KeyUp != null) {
      KeyUp(this, e);
    }
  }
  /// <summary>
  ///      Event trigger method for the <see cref="Character"/> event.
  /// </summary>
  /// <param name="e">Event information.</param>
  protected internal virtual void OnCharacter(KeyEventArgs e) {
    if(Character != null) {
      Character(this, e);
    }
  }

  #endregion Trigger Methods

  #region Event Declarations

  #region Standard Events

  /// <summary>
  ///		Window SizeF has changed.
  /// </summary>
  public event GuiEventHandler Sized;
  /// <summary>
  ///     Window position has changed.
  /// </summary>
  public event GuiEventHandler Moved;
  /// <summary>
  ///		Text string for the Window has changed.
  /// </summary>
  public event WindowEventHandler TextChanged;
  /// <summary>
  ///     Font object for the Window has been changed.
  /// </summary>
  public event GuiEventHandler FontChanged;
  /// <summary>
  ///     Alpha blend value for the Window has changed.
  /// </summary>
  public event GuiEventHandler AlphaChanged;
  /// <summary>
  ///     Client assigned ID code for the Window has changed.
  /// </summary>
  public event GuiEventHandler IDChanged;
  /// <summary>
  ///     Window has been made visible.
  /// </summary>
  public event GuiEventHandler Shown;
  /// <summary>
  ///     Window has been hidden from view.
  /// </summary>
  public event GuiEventHandler Hidden;
  /// <summary>
  ///     Window has been enabled (interaction is possible).
  /// </summary>
  public event GuiEventHandler Enabled;
  /// <summary>
  ///     Window has been disabled (interaction is no longer possible).
  /// </summary>
  public event GuiEventHandler Disabled;
  /// <summary>
  ///     Active metrics mode has been modified.
  /// </summary>
  public event GuiEventHandler MetricsChanged;
  /// <summary>
  ///     Clipping by parent mode has been modified.
  /// </summary>
  public event GuiEventHandler ClippingChanged;
  /// <summary>
  ///     Destruction by parent mode has been modified.
  /// </summary>
  public event GuiEventHandler ParentDestroyChanged;
  /// <summary>
  ///     Alpha inherited from parent mode has been modified.
  /// </summary>
  public event GuiEventHandler InheritsAlphaChanged;
  /// <summary>
  ///     Always on top mode has been modified.
  /// </summary>
  public event GuiEventHandler AlwaysOnTopChanged;
  /// <summary>
  ///     Window has captured all inputs.
  /// </summary>
  public event GuiEventHandler CaptureGained;
  /// <summary>
  ///     Window has lost it's capture on inputs.
  /// </summary>
  public event GuiEventHandler CaptureLost;
  /// <summary>
  ///     Rendering of the Window has started.
  /// </summary>
  public event GuiEventHandler RenderingStarted;
  /// <summary>
  ///     Rendering for the Window has finished.
  /// </summary>
  public event GuiEventHandler RenderingEnded;
  /// <summary>
  ///     The z-order of the window has changed.
  /// </summary>
  public event GuiEventHandler ZChanged;
  /// <summary>
  ///     Destruction of the Window is about to begin.
  /// </summary>
  public event GuiEventHandler DestructionStarted;

  #endregion Standard Events

  #region Window Events

  /// <summary>
  ///		Window has been activated (has input focus).
  /// </summary>
  public event WindowEventHandler Activated;
  /// <summary>
  ///		Window has been deactivated (loses input focus).
  /// </summary>
  public event WindowEventHandler Deactivated;
  /// <summary>
  ///		Parent of this Window has been re-sized.
  /// </summary>
  public event WindowEventHandler ParentSized;
  /// <summary>
  ///		A child Window has been added.
  /// </summary>
  public event WindowEventHandler ChildAdded;
  /// <summary>
  ///		A child window has been removed.
  /// </summary>
  public event WindowEventHandler ChildRemoved;

  #endregion Window Events

  #region Mouse Events

  /// <summary>
  ///		Mouse cursor has entered the Window.
  /// </summary>
  public event MouseEventHandler MouseEnters;
  /// <summary>
  ///		Mouse cursor has left the Window.
  /// </summary>
  public event MouseEventHandler MouseLeaves;
  /// <summary>
  ///		Mouse cursor was moved within the area of the Window.
  /// </summary>
  public event MouseEventHandler MouseMove;
  /// <summary>
  ///		Mouse wheel was scrolled within the Window.
  /// </summary>
  public event MouseEventHandler MouseWheel;
  /// <summary>
  ///		A mouse button was pressed down within the Window.
  /// </summary>
  public event MouseEventHandler MouseButtonsDown;
  /// <summary>
  ///		A mouse button was released within the Window.
  /// </summary>
  public event MouseEventHandler MouseButtonsUp;
  /// <summary>
  ///		A mouse button was clicked (down then up) within the Window.
  /// </summary>
  public event MouseEventHandler MouseClicked;
  /// <summary>
  ///		A mouse button was double-clicked within the Window.
  /// </summary>
  public event MouseEventHandler MouseDoubleClicked;
  /// <summary>
  ///		A mouse button was triple-clicked within the Window.
  /// </summary>
  public event MouseEventHandler MouseTripleClicked;

  #endregion Mouse Events

  #region Key Events

  /// <summary>
  ///		A key on the keyboard was pressed.
  /// </summary>
  public event KeyEventHandler KeyDown;
  /// <summary>
  ///		A key on the keyboard was released.
  /// </summary>
  public event KeyEventHandler KeyUp;
  /// <summary>
  ///		A text character was typed on the keyboard.
  /// </summary>
  public event KeyEventHandler Character;

  #endregion Key Events

  #endregion Event Declarations

  // -----------------------------

  #endregion Events
  // -----------------------------

  // -----------------------------
  #region IDisposable Members

  /// <summary>
  /// Part of implementation of IDisposable
  /// </summary>
  public void Dispose() {
    // TODO:  Add Window.Dispose implementation
  }

  #endregion
  // -----------------------------
}

} // namespace CeGui
