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
///		Base class for progress bars.
/// </summary>
[ AlternateWidgetName("ProgressBar") ]
public abstract class ProgressBar : Window {
  #region Fields

  /// <summary>
  ///		Current progress (from 0 to 1)
  /// </summary>
  protected float progress;
  /// <summary>
  ///		Amount to 'step' progress by on a call to <see cref="Step"/>.
  /// </summary>
  protected float step;

  #endregion Fields

  #region Constructor

  /// <summary>
  ///		Constructor.
  /// </summary>
  /// <param name="type"></param>
  /// <param name="name"></param>
  public ProgressBar(string type, string name)
    : base(type, name) {
    step = 0.01f;
  }

  #endregion Constructor

  #region Properties

  /// <summary>
  ///		Get/Set the current level of progress.
  /// </summary>
  /// <remarks>
  ///		If this value is > 1.0f (100%) progress will be limited to 1.0f.
  /// </remarks>
  /// <value>Value between 0.0f and 1.0f indicating current progress.</value>
  [WidgetProperty("CurrentProgress")]
  public float Progress {
    get {
      return progress;
    }
    set {
      // legal progress range is : 0.0f <= progress <= 1.0f
      float newProgress = (value < 0.0f) ? 0.0f : (value > 1.0f) ? 1.0f : value;

      if(newProgress != progress) {
        // update progress and fire off event.
        progress = newProgress;

        OnProgressChanged(new WindowEventArgs(this));

        // if new progress is 100%, fire off the 'done' event as well.
        if(progress == 1.0f) {
          OnProgressDone(new WindowEventArgs(this));
        }
      }
    }
  }

  /// <summary>
  ///		Get/Set the SizeF of the 'step' (in percentage points).
  /// </summary>
  /// <value>Default is 0.01f or 1%.</value>
  [WidgetProperty("StepSize")]
  public float StepSize {
    get {
      return step;
    }
    set {
      step = value;
    }
  }

  #endregion Properties

  #region Methods

  /// <summary>
  ///		Modify the progress level by a specified delta.
  /// </summary>
  /// <param name="delta">
  ///		Amount to adjust the progress by.  Whatever this value is, the progress of the bar will be kept
  ///		within the range: [0.0f, 1.0f]
  /// </param>
  public void AdjustProgress(float delta) {
    Progress = progress + delta;
  }

  /// <summary>
  ///		Cause the progress to step.
  /// </summary>
  /// <remarks>
  ///		The amount the progress bar will step can be changed via the <see cref="StepSize"/> property.  The
  ///		default step SizeF is 0.01f which is equal to 1%.
  /// </remarks>
  public void Step() {
    Progress = progress + step;
  }

  #endregion Methods

  #region Events

  #region Event Declarations

  /// <summary>
  ///		Triggered when the progress changes.
  /// </summary>
  public event WindowEventHandler ProgressChanged;

  /// <summary>
  ///		Triggered when the progress reaches 100%.
  /// </summary>
  public event WindowEventHandler ProgressDone;

  #endregion Event Declarations

  #region Trigger Methods

  /// <summary>
  ///		Event triggered when progress changes.
  /// </summary>
  /// <param name="e">Event arguments.</param>
  protected internal virtual void OnProgressChanged(WindowEventArgs e) {
    RequestRedraw();
    if(ProgressChanged != null) {
      ProgressChanged(this, e);
    }
  }

  /// <summary>
  ///		Event triggered when progress is complete.
  /// </summary>
  /// <param name="e">Event arguments.</param>
  protected internal virtual void OnProgressDone(WindowEventArgs e) {
    if(ProgressDone != null) {
      ProgressDone(this, e);
    }
  }

  #endregion Trigger Methods

  #endregion Events
}

} // namespace CeGui.Widgets
