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
using System.Xml.Serialization;

namespace CeGui.WindowProperties {

#region ID
/// <summary>Property to access window ID field</summary>
/// <remarks>
///   This property offers access to the client specified ID for the window
/// </remarks>
public class ID : Property {

  /// <summary>Initializes a new instance of the ID property</summary>
  public ID() :
    base(
      "ID",
      "Property to get/set the ID value of the Window. Value is an unsigned integer number.",
      "0"
    ) {}

  /// <summary>Return the current value of the Property as a String</summary>
  /// <param name="reciever">target object</param>
  /// <returns>
  ///   A string containing a textual representation of the current value of
  ///   the property
  /// </returns>
  public override string Get(PropertySet receiver) {
    throw new Exception("The method or operation is not implemented.");
  }

  /// <summary>Sets the value of the property</summary>
  /// <param name="reciever">target object</param>
  /// <param name="val">
  ///   A string that contains a textual representation of the new value to
  ///   assign to the property
  /// </param>
  public override void Set(PropertySet receiver, string val) {
    throw new Exception("The method or operation is not implemented.");
  }

}
#endregion

#region Alpha
/// <summary>Property to access window alpha setting</summary>
/// <remarks>
///   This property offers access to the alpha-blend setting for the window
/// </remarks>
public class Alpha : Property {

  /// <summary>Initializes a new instance of the Alpha property</summary>
  public Alpha() :
    base(
      "Alpha",
      "Property to get/set the alpha value of the Window. Value is a floating point number.",
      "1"
    ) {}

  /// <summary>Return the current value of the Property as a String</summary>
  /// <param name="reciever">target object</param>
  /// <returns>
  ///   A string containing a textual representation of the current value of
  ///   the property
  /// </returns>
  public override string Get(PropertySet receiver) {
    throw new Exception("The method or operation is not implemented.");
  }

  /// <summary>Sets the value of the property</summary>
  /// <param name="reciever">target object</param>
  /// <param name="val">
  ///   A string that contains a textual representation of the new value to
  ///   assign to the property
  /// </param>
  public override void Set(PropertySet receiver, string val) {
    throw new Exception("The method or operation is not implemented.");
  }

}
#endregion

#region Font
/// <summary>Property to access window Font setting</summary>
/// <remarks>
///   This property offers access to the current Font setting for the window
/// </remarks>
public class Font : Property {

  /// <summary>Initializes a new instance of the font property</summary>
  public Font() :
    base(
      "Font",
      "Property to get/set the font for the Window. " +
        "Value is the name of the font to use (must be loaded already).",
      ""
    ) {}

  /// <summary>Return the current value of the Property as a String</summary>
  /// <param name="reciever">target object</param>
  /// <returns>
  ///   A string containing a textual representation of the current value of
  ///   the property
  /// </returns>
  public override string Get(PropertySet receiver) {
    throw new Exception("The method or operation is not implemented.");
  }

  /// <summary>Sets the value of the property</summary>
  /// <param name="reciever">target object</param>
  /// <param name="val">
  ///   A string that contains a textual representation of the new value to
  ///   assign to the property
  /// </param>
  public override void Set(PropertySet receiver, string val) {
    throw new Exception("The method or operation is not implemented.");
  }

}
#endregion

#region Text
/// <summary>Property to access window text setting</summary>
/// <remarks>
///   This property offers access to the current text for the window
/// </remarks>
public class Text : Property {

  /// <summary>Initializes a new instance of the Text property</summary>
  public Text() :
    base(
      "Text", 
      "Property to get/set the text / caption for the Window. Value is the text string to use.",
      ""
    ) {}

  /// <summary>Return the current value of the Property as a String</summary>
  /// <param name="reciever">target object</param>
  /// <returns>
  ///   A string containing a textual representation of the current value of
  ///   the property
  /// </returns>
  public override string Get(PropertySet receiver) {
    throw new Exception("The method or operation is not implemented.");
  }

  /// <summary>Sets the value of the property</summary>
  /// <param name="reciever">target object</param>
  /// <param name="val">
  ///   A string that contains a textual representation of the new value to
  ///   assign to the property
  /// </param>
  public override void Set(PropertySet receiver, string val) {
    throw new Exception("The method or operation is not implemented.");
  }

}
#endregion

#region MouseCursorImage
/// <summary>Property to access window mouse cursor setting</summary>
/// <remarks>
///   This property offers access to the current mouse cursor image for the window
/// </remarks>
public class MouseCursorImage : Property {

  /// <summary>Initializes a new instance of the MouseCursorImage property</summary>
  public MouseCursorImage() :
    base(
      "MouseCursorImage",
      "Property to get/set the mouse cursor image for the Window. " +
        "Value should be \"set:<imageset name> image:<image name>\".",
      ""
    ) {}

  /// <summary>Return the current value of the Property as a String</summary>
  /// <param name="reciever">target object</param>
  /// <returns>
  ///   A string containing a MouseCursorImageual representation of the current value of
  ///   the property
  /// </returns>
  public override string Get(PropertySet receiver) {
    throw new Exception("The method or operation is not implemented.");
  }

  /// <summary>Sets the value of the property</summary>
  /// <param name="reciever">target object</param>
  /// <param name="val">
  ///   A string that contains a MouseCursorImageual representation of the new value to
  ///   assign to the property
  /// </param>
  public override void Set(PropertySet receiver, string val) {
    throw new Exception("The method or operation is not implemented.");
  }

  /// <summary>Returns whether the property is at it's default value</summary>
  /// <param name="receiver">Pointer to the target object.</param>
  /// <returns>
  ///   - true if the property has its default value.
  ///   - false if the property has been modified from its default value.
  /// </returns>
  public override bool IsDefault(PropertySet receiver) {
    return base.IsDefault(receiver);
  }

}
#endregion

#region ClippedByParent
/// <summary>Property to access window "clipped by parent" setting</summary>
/// <remarks>
///   This property offers access to the clipped by parent setting for the window
/// </remarks>
public class ClippedByParent : Property {

  /// <summary>Initializes a new instance of the ClippedByParent property</summary>
  public ClippedByParent() :
    base(
      "ClippedByParent",
      "Property to get/set the 'clipped by parent' setting for the Window. " +
        "Value is either \"True\" or \"False\".",
      "True"
    ) {}

  /// <summary>Return the current value of the Property as a String</summary>
  /// <param name="reciever">target object</param>
  /// <returns>
  ///   A string containing a ClippedByParentual representation of the current value of
  ///   the property
  /// </returns>
  public override string Get(PropertySet receiver) {
    throw new Exception("The method or operation is not implemented.");
  }

  /// <summary>Sets the value of the property</summary>
  /// <param name="reciever">target object</param>
  /// <param name="val">
  ///   A string that contains a ClippedByParentual representation of the new value to
  ///   assign to the property
  /// </param>
  public override void Set(PropertySet receiver, string val) {
    throw new Exception("The method or operation is not implemented.");
  }

}
#endregion

#region InheritsAlpha
/// <summary>Property to access window "Inherits Alpha" setting</summary>
/// <remarks>
///   This property offers access to the inherits alpha setting for the window
/// </remarks>
public class InheritsAlpha : Property {

  /// <summary>Initializes a new instance of the InheritsAlpha property</summary>
  public InheritsAlpha() :
    base(
      "InheritsAlpha",
      "Property to get/set the 'inherits alpha' setting for the Window. " +
        "Value is either \"True\" or \"False\".",
      "True"
    ) {}

  /// <summary>Return the current value of the Property as a String</summary>
  /// <param name="reciever">target object</param>
  /// <returns>
  ///   A string containing a InheritsAlphaual representation of the current value of
  ///   the property
  /// </returns>
  public override string Get(PropertySet receiver) {
    throw new Exception("The method or operation is not implemented.");
  }

  /// <summary>Sets the value of the property</summary>
  /// <param name="reciever">target object</param>
  /// <param name="val">
  ///   A string that contains a InheritsAlphaual representation of the new value to
  ///   assign to the property
  /// </param>
  public override void Set(PropertySet receiver, string val) {
    throw new Exception("The method or operation is not implemented.");
  }

}
#endregion

#region AlwaysOnTop
/// <summary>Property to access window "Always-On-Top" setting</summary>
/// <remarks>
///   This property offers access to the always on top / topmost setting for the window
/// </remarks>
public class AlwaysOnTop : Property {

  /// <summary>Initializes a new instance of the AlwaysOnTop property</summary>
  public AlwaysOnTop() :
    base(
      "AlwaysOnTop",
      "Property to get/set the 'always on top' setting for the Window. " +
        "Value is either \"True\" or \"False\".",
      "False"
    ) {}

  /// <summary>Return the current value of the Property as a String</summary>
  /// <param name="reciever">target object</param>
  /// <returns>
  ///   A string containing a AlwaysOnTopual representation of the current value of
  ///   the property
  /// </returns>
  public override string Get(PropertySet receiver) {
    throw new Exception("The method or operation is not implemented.");
  }

  /// <summary>Sets the value of the property</summary>
  /// <param name="reciever">target object</param>
  /// <param name="val">
  ///   A string that contains a AlwaysOnTopual representation of the new value to
  ///   assign to the property
  /// </param>
  public override void Set(PropertySet receiver, string val) {
    throw new Exception("The method or operation is not implemented.");
  }

}
#endregion

#region Disabled
/// <summary>Property to access window Disabled setting</summary>
/// <remarks>
///   This property offers access to the enabled / disabled setting for the window
/// </remarks>
public class Disabled : Property {

  /// <summary>Initializes a new instance of the Disabled property</summary>
  public Disabled() :
    base(
      "Disabled",
      "Property to get/set the 'disabled state' setting for the Window. " +
        "Value is either \"True\" or \"False\".",
      "False"
    ) {}

  /// <summary>Return the current value of the Property as a String</summary>
  /// <param name="reciever">target object</param>
  /// <returns>
  ///   A string containing a Disabledual representation of the current value of
  ///   the property
  /// </returns>
  public override string Get(PropertySet receiver) {
    throw new Exception("The method or operation is not implemented.");
  }

  /// <summary>Sets the value of the property</summary>
  /// <param name="reciever">target object</param>
  /// <param name="val">
  ///   A string that contains a Disabledual representation of the new value to
  ///   assign to the property
  /// </param>
  public override void Set(PropertySet receiver, string val) {
    throw new Exception("The method or operation is not implemented.");
  }

  /// <summary>Returns whether the property is at it's default value</summary>
  /// <param name="receiver">Pointer to the target object.</param>
  /// <returns>
  ///   - true if the property has its default value.
  ///   - false if the property has been modified from its default value.
  /// </returns>
  public override bool IsDefault(PropertySet receiver) {
    return base.IsDefault(receiver);
  }

}
#endregion

#region Visible
/// <summary>Property to access window Visible setting</summary>
/// <remarks>
///   This property offers access to the visible setting for the window
/// </remarks>
public class Visible : Property {

  /// <summary>Initializes a new instance of the Visible property</summary>
  public Visible() :
    base(
      "Visible",
      "Property to get/set the 'visible state' setting for the Window. " +
        "Value is either \"True\" or \"False\".",
      "True"
    ) {}

  /// <summary>Return the current value of the Property as a String</summary>
  /// <param name="reciever">target object</param>
  /// <returns>
  ///   A string containing a Visibleual representation of the current value of
  ///   the property
  /// </returns>
  public override string Get(PropertySet receiver) {
    throw new Exception("The method or operation is not implemented.");
  }

  /// <summary>Sets the value of the property</summary>
  /// <param name="reciever">target object</param>
  /// <param name="val">
  ///   A string that contains a Visibleual representation of the new value to
  ///   assign to the property
  /// </param>
  public override void Set(PropertySet receiver, string val) {
    throw new Exception("The method or operation is not implemented.");
  }

  /// <summary>Returns whether the property is at it's default value</summary>
  /// <param name="receiver">Pointer to the target object.</param>
  /// <returns>
  ///   - true if the property has its default value.
  ///   - false if the property has been modified from its default value.
  /// </returns>
  public override bool IsDefault(PropertySet receiver) {
    return base.IsDefault(receiver);
  }

}
#endregion

#region RestoreOldCapture
/// <summary>Property to access window Restore Old Capture setting</summary>
/// <remarks>
///   This property offers access to the restore old capture setting for the window.
///   This setting is of generally limited use, its primary purpose is for certain
///   operations required for compound widgets.
/// </remarks>
public class RestoreOldCapture : Property {

  /// <summary>Initializes a new instance of the RestoreOldCapture property</summary>
  public RestoreOldCapture() :
    base(
      "RestoreOldCapture",
      "Property to get/set the 'restore old capture' setting for the Window. " +
        "Value is either \"True\" or \"False\".",
      "False"
    ) {}

  /// <summary>Return the current value of the Property as a String</summary>
  /// <param name="reciever">target object</param>
  /// <returns>
  ///   A string containing a RestoreOldCaptureual representation of the current value of
  ///   the property
  /// </returns>
  public override string Get(PropertySet receiver) {
    throw new Exception("The method or operation is not implemented.");
  }

  /// <summary>Sets the value of the property</summary>
  /// <param name="reciever">target object</param>
  /// <param name="val">
  ///   A string that contains a RestoreOldCaptureual representation of the new value to
  ///   assign to the property
  /// </param>
  public override void Set(PropertySet receiver, string val) {
    throw new Exception("The method or operation is not implemented.");
  }

}
#endregion

#region DestroyedByParent
/// <summary>Property to access window Destroyed by Parent setting</summary>
/// <remarks>
///   This property offers access to the destroyed by parent setting for the window
/// </remarks>
public class DestroyedByParent : Property {

  /// <summary>Initializes a new instance of the DestroyedByParent property</summary>
  public DestroyedByParent() :
    base(
      "DestroyedByParent",
      "Property to get/set the 'destroyed by parent' setting for the Window. " +
        "Value is either \"True\" or \"False\".",
      "True"
    ) {}

  /// <summary>Return the current value of the Property as a String</summary>
  /// <param name="reciever">target object</param>
  /// <returns>
  ///   A string containing a DestroyedByParentual representation of the current value of
  ///   the property
  /// </returns>
  public override string Get(PropertySet receiver) {
    throw new Exception("The method or operation is not implemented.");
  }

  /// <summary>Sets the value of the property</summary>
  /// <param name="reciever">target object</param>
  /// <param name="val">
  ///   A string that contains a DestroyedByParentual representation of the new value to
  ///   assign to the property
  /// </param>
  public override void Set(PropertySet receiver, string val) {
    throw new Exception("The method or operation is not implemented.");
  }

}
#endregion

#region ZOrderChangeEnabled
/// <summary>Property to access window Z-Order changing enabled setting</summary>
/// <remarks>
///   This property offers access to the setting that controls whether z-order
///   changes are enabled for the window.
/// </remarks>
public class ZOrderChangeEnabled : Property {

  /// <summary>Initializes a new instance of the ZOrderChangeEnabled property</summary>
  public ZOrderChangeEnabled() :
    base(
      "ZOrderChangeEnabled",
      "Property to get/set the 'z-order changing enabled' setting for the Window. " +
        "Value is either \"True\" or \"False\".",
      "True"
    ) {}

  /// <summary>Return the current value of the Property as a String</summary>
  /// <param name="reciever">target object</param>
  /// <returns>
  ///   A string containing a ZOrderChangeEnabledual representation of the current value of
  ///   the property
  /// </returns>
  public override string Get(PropertySet receiver) {
    throw new Exception("The method or operation is not implemented.");
  }

  /// <summary>Sets the value of the property</summary>
  /// <param name="reciever">target object</param>
  /// <param name="val">
  ///   A string that contains a ZOrderChangeEnabledual representation of the new value to
  ///   assign to the property
  /// </param>
  public override void Set(PropertySet receiver, string val) {
    throw new Exception("The method or operation is not implemented.");
  }

}
#endregion

#region WantsMultiClickEvents
/// <summary>
///   Property to control whether the window will receive double/triple-click events
/// </summary>
/// <remarks>
///   This property offers access to the setting that controls whether a window will
///   receive double-click and triple-click events, or whether the window will receive
///   multiple single mouse button down events instead
/// </remarks>
public class WantsMultiClickEvents : Property {

  /// <summary>Initializes a new instance of the WantsMultiClickEvents property</summary>
  public WantsMultiClickEvents() :
    base(
      "WantsMultiClickEvents",
      "Property to get/set whether the window will receive double-click and triple-click " +
        "events. Value is either \"True\" or \"False\".",
      "True"
    ) {}

  /// <summary>Return the current value of the Property as a String</summary>
  /// <param name="reciever">target object</param>
  /// <returns>
  ///   A string containing a WantsMultiClickEventsual representation of the current value of
  ///   the property
  /// </returns>
  public override string Get(PropertySet receiver) {
    throw new Exception("The method or operation is not implemented.");
  }

  /// <summary>Sets the value of the property</summary>
  /// <param name="reciever">target object</param>
  /// <param name="val">
  ///   A string that contains a WantsMultiClickEventsual representation of the new value to
  ///   assign to the property
  /// </param>
  public override void Set(PropertySet receiver, string val) {
    throw new Exception("The method or operation is not implemented.");
  }

}
#endregion

#region MouseButtonDownAutoRepeat
/// <summary>
///   Property to control whether the window will receive autorepeat mouse button down events
/// </summary>
/// <remarks>
///   This property offers access to the setting that controls whether a window will receive
///   autorepeat mouse button down events
/// </remarks>
public class MouseButtonDownAutoRepeat : Property {

  /// <summary>Initializes a new instance of the MouseButtonDownAutoRepeat property</summary>
  public MouseButtonDownAutoRepeat() :
    base(
      "MouseButtonDownAutoRepeat",
      "Property to get/set whether the window will receive autorepeat mouse button down " +
        "events. Value is either \"True\" or \"False\".",
      "False"
    ) {}

  /// <summary>Return the current value of the Property as a String</summary>
  /// <param name="reciever">target object</param>
  /// <returns>
  ///   A string containing a MouseButtonDownAutoRepeatual representation of the current value of
  ///   the property
  /// </returns>
  public override string Get(PropertySet receiver) {
    throw new Exception("The method or operation is not implemented.");
  }

  /// <summary>Sets the value of the property</summary>
  /// <param name="reciever">target object</param>
  /// <param name="val">
  ///   A string that contains a MouseButtonDownAutoRepeatual representation of the new value to
  ///   assign to the property
  /// </param>
  public override void Set(PropertySet receiver, string val) {
    throw new Exception("The method or operation is not implemented.");
  }

}
#endregion

#region AutoRepeatDelay
/// <summary>
///   Property to access window autorepeat delay value
/// </summary>
/// <remarks>
///   This property offers access to the value that controls the initial delay for autorepeat
///   mouse button down events
/// </remarks>
public class AutoRepeatDelay : Property {

  /// <summary>Initializes a new instance of the AutoRepeatDelay property</summary>
  public AutoRepeatDelay() :
    base(
      "AutoRepeatDelay",
      "Property to get/set the autorepeat delay. " +
        "Value is a floating point number indicating the delay required in seconds.",
      "0.3"
    ) {}

  /// <summary>Return the current value of the Property as a String</summary>
  /// <param name="reciever">target object</param>
  /// <returns>
  ///   A string containing a AutoRepeatDelayual representation of the current value of
  ///   the property
  /// </returns>
  public override string Get(PropertySet receiver) {
    throw new Exception("The method or operation is not implemented.");
  }

  /// <summary>Sets the value of the property</summary>
  /// <param name="reciever">target object</param>
  /// <param name="val">
  ///   A string that contains a AutoRepeatDelayual representation of the new value to
  ///   assign to the property
  /// </param>
  public override void Set(PropertySet receiver, string val) {
    throw new Exception("The method or operation is not implemented.");
  }

}
#endregion

#region AutoRepeatRate
/// <summary>
///   Property to access window autorepeat rate value
/// </summary>
/// <remarks>
///   This property offers access to the value that controls the generation rate for
///   autorepeat mouse button down events.
/// </remarks>
public class AutoRepeatRate : Property {

  /// <summary>Initializes a new instance of the AutoRepeatRate property</summary>
  public AutoRepeatRate() :
    base(
      "AutoRepeatRate",
      "Property to get/set the autorepeat rate. " +
        "Value is a floating point number indicating the rate required in seconds.",
      "0.06"
    ) {}

  /// <summary>Return the current value of the Property as a String</summary>
  /// <param name="receiver">target object</param>
  /// <returns>
  ///   A string containing a AutoRepeatRateual representation of the current value of
  ///   the property
  /// </returns>
  public override string Get(PropertySet receiver) {
    throw new Exception("The method or operation is not implemented.");
  }

  /// <summary>Sets the value of the property</summary>
  /// <param name="receiver">target object</param>
  /// <param name="val">
  ///   A string that contains a AutoRepeatRateual representation of the new value to
  ///   assign to the property
  /// </param>
  public override void Set(PropertySet receiver, string val) {
    throw new Exception("The method or operation is not implemented.");
  }

}
#endregion

#region DistributeCapturedInputs
/// <summary>
///   Property to access whether inputs are passed to child windows when input is
///   captured to this window
/// </summary>
public class DistributeCapturedInputs : Property {

  /// <summary>Initializes a new instance of the DistributeCapturedInputs property</summary>
  public DistributeCapturedInputs() :
    base(
      "DistributeCapturedInputs",
      "Property to get/set whether captured inputs are passed to child windows. " +
        "Value is either \"True\" or \"False\".",
      "False"
    ) {}

  /// <summary>Return the current value of the Property as a String</summary>
  /// <param name="receiver">target object</param>
  /// <returns>
  ///   A string containing a DistributeCapturedInputsual representation of the current value of
  ///   the property
  /// </returns>
  public override string Get(PropertySet receiver) {
    throw new Exception("The method or operation is not implemented.");
  }

  /// <summary>Sets the value of the property</summary>
  /// <param name="receiver">target object</param>
  /// <param name="val">
  ///   A string that contains a DistributeCapturedInputsual representation of the new value to
  ///   assign to the property
  /// </param>
  public override void Set(PropertySet receiver, string val) {
    throw new Exception("The method or operation is not implemented.");
  }

}
#endregion

#region CustomTooltipType
/// <summary>
///   Property to access the custom tooltip for this Window
/// </summary>
public class CustomTooltipType : Property {

  /// <summary>Initializes a new instance of the CustomTooltipType property</summary>
  public CustomTooltipType() :
    base(
      "CustomTooltipType",
      "Property to get/set the custom tooltip for the window. " +
        "Value is the type name of the custom tooltip.",
      ""
    ) {}

  /// <summary>Return the current value of the Property as a String</summary>
  /// <param name="receiver">target object</param>
  /// <returns>
  ///   A string containing a CustomTooltipTypeual representation of the current value of
  ///   the property
  /// </returns>
  public override string Get(PropertySet receiver) {
    throw new Exception("The method or operation is not implemented.");
  }

  /// <summary>Sets the value of the property</summary>
  /// <param name="receiver">target object</param>
  /// <param name="val">
  ///   A string that contains a CustomTooltipTypeual representation of the new value to
  ///   assign to the property
  /// </param>
  public override void Set(PropertySet receiver, string val) {
    throw new Exception("The method or operation is not implemented.");
  }

}
#endregion

#region Tooltip
/// <summary>
///   Property to access the tooltip text for this Window
/// </summary>
public class Tooltip : Property {

  /// <summary>Initializes a new instance of the Tooltip property</summary>
  public Tooltip() :
    base(
      "Tooltip",
      "Property to get/set the tooltip text for the window. " +
        "Value is the tooltip text for the window.",
      ""
    ) {}

  /// <summary>Return the current value of the Property as a String</summary>
  /// <param name="receiver">target object</param>
  /// <returns>
  ///   A string containing a Tooltipual representation of the current value of
  ///   the property
  /// </returns>
  public override string Get(PropertySet receiver) {
    throw new Exception("The method or operation is not implemented.");
  }

  /// <summary>Sets the value of the property</summary>
  /// <param name="receiver">target object</param>
  /// <param name="val">
  ///   A string that contains a Tooltipual representation of the new value to
  ///   assign to the property
  /// </param>
  public override void Set(PropertySet receiver, string val) {
    throw new Exception("The method or operation is not implemented.");
  }

}
#endregion

#region InheritsTooltipText
/// <summary>
///   Property to access whether the window inherits its tooltip text from its parent
///   when it has no tooltip text of its own
/// </summary>
public class InheritsTooltipText : Property {

  /// <summary>Initializes a new instance of the InheritsTooltipText property</summary>
  public InheritsTooltipText() :
    base(
      "InheritsTooltipText",
      "Property to get/set whether the window inherits its parents tooltip text when " +
      "it has none of its own. Value is either \"True\" or \"False\".",
      "False"
    ) {}

  /// <summary>Return the current value of the Property as a String</summary>
  /// <param name="receiver">target object</param>
  /// <returns>
  ///   A string containing a InheritsTooltipTextual representation of the current value of
  ///   the property
  /// </returns>
  public override string Get(PropertySet receiver) {
    throw new Exception("The method or operation is not implemented.");
  }

  /// <summary>Sets the value of the property</summary>
  /// <param name="receiver">target object</param>
  /// <param name="val">
  ///   A string that contains a InheritsTooltipTextual representation of the new value to
  ///   assign to the property
  /// </param>
  public override void Set(PropertySet receiver, string val) {
    throw new Exception("The method or operation is not implemented.");
  }

}
#endregion

#region RiseOnClick
/// <summary>
///   Property to access whether the window rises to the top of the z order when clicked
/// </summary>
public class RiseOnClick : Property {

  /// <summary>Initializes a new instance of the RiseOnClick property</summary>
  public RiseOnClick() :
    base(
      "RiseOnClick",
      "Property to get/set whether the window will come to the top of the z order when " +
        "clicked. Value is either \"True\" or \"False\".",
      "True"
    ) {}

  /// <summary>Return the current value of the Property as a String</summary>
  /// <param name="receiver">target object</param>
  /// <returns>
  ///   A string containing a RiseOnClickual representation of the current value of
  ///   the property
  /// </returns>
  public override string Get(PropertySet receiver) {
    throw new Exception("The method or operation is not implemented.");
  }

  /// <summary>Sets the value of the property</summary>
  /// <param name="receiver">target object</param>
  /// <param name="val">
  ///   A string that contains a RiseOnClickual representation of the new value to
  ///   assign to the property
  /// </param>
  public override void Set(PropertySet receiver, string val) {
    throw new Exception("The method or operation is not implemented.");
  }

}
#endregion

#region VerticalAlignment
/// <summary>
///   Property to access the vertical alignment setting for the window
/// </summary>
public class VerticalAlignment : Property {

  /// <summary>Initializes a new instance of the VerticalAlignment property</summary>
  public VerticalAlignment() :
    base(
      "VerticalAlignment",
      "Property to get/set the windows vertical alignment. " +
        "Value is one of \"Top\", \"Centre\" or \"Bottom\".",
      "Top"
    ) {}

  /// <summary>Return the current value of the Property as a String</summary>
  /// <param name="receiver">target object</param>
  /// <returns>
  ///   A string containing a VerticalAlignmentual representation of the current value of
  ///   the property
  /// </returns>
  public override string Get(PropertySet receiver) {
    throw new Exception("The method or operation is not implemented.");
  }

  /// <summary>Sets the value of the property</summary>
  /// <param name="receiver">target object</param>
  /// <param name="val">
  ///   A string that contains a VerticalAlignmentual representation of the new value to
  ///   assign to the property
  /// </param>
  public override void Set(PropertySet receiver, string val) {
    throw new Exception("The method or operation is not implemented.");
  }

}
#endregion

#region HorizontalAlignment
/// <summary>
///   Property to access the horizontal alignment setting for the window
/// </summary>
public class HorizontalAlignment : Property {

  /// <summary>Initializes a new instance of the HorizontalAlignment property</summary>
  public HorizontalAlignment() :
    base(
      "HorizontalAlignment",
      "Property to get/set the windows horizontal alignment. " +
        "Value is one of \"Left\", \"Centre\" or \"Right\".",
      "Left"
    ) {}

  /// <summary>Return the current value of the Property as a String</summary>
  /// <param name="receiver">target object</param>
  /// <returns>
  ///   A string containing a HorizontalAlignmentual representation of the current value of
  ///   the property
  /// </returns>
  public override string Get(PropertySet receiver) {
    throw new Exception("The method or operation is not implemented.");
  }

  /// <summary>Sets the value of the property</summary>
  /// <param name="receiver">target object</param>
  /// <param name="val">
  ///   A string that contains a HorizontalAlignmentual representation of the new value to
  ///   assign to the property
  /// </param>
  public override void Set(PropertySet receiver, string val) {
    throw new Exception("The method or operation is not implemented.");
  }

}
#endregion

#region UnifiedAreaRect
/// <summary>
///   Property to access the unified area rectangle of the window
/// </summary>
public class UnifiedAreaRect : Property {

  /// <summary>Initializes a new instance of the UnifiedAreaRect property</summary>
  public UnifiedAreaRect() :
    base(
      "UnifiedAreaRect",
      "Property to get/set the windows unified area rectangle. " +
        "Value is a \"URect\".",
      "{{0,0},{0,0},{0,0},{0,0}}"
    ) {}

  /// <summary>Return the current value of the Property as a String</summary>
  /// <param name="receiver">target object</param>
  /// <returns>
  ///   A string containing a UnifiedAreaRectual representation of the current value of
  ///   the property
  /// </returns>
  public override string Get(PropertySet receiver) {
    throw new Exception("The method or operation is not implemented.");
  }

  /// <summary>Sets the value of the property</summary>
  /// <param name="receiver">target object</param>
  /// <param name="val">
  ///   A string that contains a UnifiedAreaRectual representation of the new value to
  ///   assign to the property
  /// </param>
  public override void Set(PropertySet receiver, string val) {
    throw new Exception("The method or operation is not implemented.");
  }

}
#endregion

#region UnifiedPosition
/// <summary>
///   Property to access the unified position of the window
/// </summary>
public class UnifiedPosition : Property {

  /// <summary>Initializes a new instance of the UnifiedPosition property</summary>
  public UnifiedPosition() :
    base(
      "UnifiedPosition",
      "Property to get/set the windows unified position. Value is a \"UVector2\".",
      "{{0,0},{0,0}}",
      false
    ) {}

  /// <summary>Return the current value of the Property as a String</summary>
  /// <param name="receiver">target object</param>
  /// <returns>
  ///   A string containing a UnifiedPositionual representation of the current value of
  ///   the property
  /// </returns>
  public override string Get(PropertySet receiver) {
    throw new Exception("The method or operation is not implemented.");
  }

  /// <summary>Sets the value of the property</summary>
  /// <param name="receiver">target object</param>
  /// <param name="val">
  ///   A string that contains a UnifiedPositionual representation of the new value to
  ///   assign to the property
  /// </param>
  public override void Set(PropertySet receiver, string val) {
    throw new Exception("The method or operation is not implemented.");
  }

}
#endregion

#region UnifiedXPosition
/// <summary>
///   Property to access the unified position x-coordinate of the window
/// </summary>
public class UnifiedXPosition : Property {

  /// <summary>Initializes a new instance of the UnifiedXPosition property</summary>
  public UnifiedXPosition() :
    base(
      "UnifiedXPosition",
      "Property to get/set the windows unified position x-coordinate. Value is a \"UDim\".",
      "{0,0}",
      false
    ) {}

  /// <summary>Return the current value of the Property as a String</summary>
  /// <param name="receiver">target object</param>
  /// <returns>
  ///   A string containing a UnifiedPositionual representation of the current value of
  ///   the property
  /// </returns>
  public override string Get(PropertySet receiver) {
    throw new Exception("The method or operation is not implemented.");
  }

  /// <summary>Sets the value of the property</summary>
  /// <param name="receiver">target object</param>
  /// <param name="val">
  ///   A string that contains a UnifiedPositionual representation of the new value to
  ///   assign to the property
  /// </param>
  public override void Set(PropertySet receiver, string val) {
    throw new Exception("The method or operation is not implemented.");
  }

}
#endregion

#region UnifiedYPosition
/// <summary>
///   Property to access the unified position y-coordinate of the window
/// </summary>
public class UnifiedYPosition : Property {

  /// <summary>Initializes a new instance of the UnifiedYPosition property</summary>
  public UnifiedYPosition() :
    base(
      "UnifiedYPosition",
      "Property to get/set the windows unified position y-coordinate. Value is a \"UDim\".",
      "{0,0}", false
    ) {}

  /// <summary>Return the current value of the Property as a String</summary>
  /// <param name="receiver">target object</param>
  /// <returns>
  ///   A string containing a UnifiedPositionual representation of the current value of
  ///   the property
  /// </returns>
  public override string Get(PropertySet receiver) {
    throw new Exception("The method or operation is not implemented.");
  }

  /// <summary>Sets the value of the property</summary>
  /// <param name="receiver">target object</param>
  /// <param name="val">
  ///   A string that contains a UnifiedPositionual representation of the new value to
  ///   assign to the property
  /// </param>
  public override void Set(PropertySet receiver, string val) {
    throw new Exception("The method or operation is not implemented.");
  }

}
#endregion

#region UnifiedSize
/// <summary>
///   Property to access the unified size of the window
/// </summary>
public class UnifiedSize : Property {

  /// <summary>Initializes a new instance of the UnifiedSize property</summary>
  public UnifiedSize() :
    base(
      "UnifiedSize",
      "Property to get/set the windows unified size. Value is a \"UVector2\".",
      "{{0,0},{0,0}}",
      false
    ) {}

  /// <summary>Return the current value of the Property as a String</summary>
  /// <param name="receiver">target object</param>
  /// <returns>
  ///   A string containing a UnifiedPositionual representation of the current value of
  ///   the property
  /// </returns>
  public override string Get(PropertySet receiver) {
    throw new Exception("The method or operation is not implemented.");
  }

  /// <summary>Sets the value of the property</summary>
  /// <param name="receiver">target object</param>
  /// <param name="val">
  ///   A string that contains a UnifiedPositionual representation of the new value to
  ///   assign to the property
  /// </param>
  public override void Set(PropertySet receiver, string val) {
    throw new Exception("The method or operation is not implemented.");
  }

}
#endregion

#region UnifiedWidth
/// <summary>
///   Property to access the unified width of the window
/// </summary>
public class UnifiedWidth : Property {

  /// <summary>Initializes a new instance of the UnifiedWidth property</summary>
  public UnifiedWidth() :
    base(
      "UnifiedWidth",
      "Property to get/set the windows unified width. Value is a \"UDim\".",
      "{0,0}",
      false
    ) {}

  /// <summary>Return the current value of the Property as a String</summary>
  /// <param name="receiver">target object</param>
  /// <returns>
  ///   A string containing a UnifiedPositionual representation of the current value of
  ///   the property
  /// </returns>
  public override string Get(PropertySet receiver) {
    throw new Exception("The method or operation is not implemented.");
  }

  /// <summary>Sets the value of the property</summary>
  /// <param name="receiver">target object</param>
  /// <param name="val">
  ///   A string that contains a UnifiedPositionual representation of the new value to
  ///   assign to the property
  /// </param>
  public override void Set(PropertySet receiver, string val) {
    throw new Exception("The method or operation is not implemented.");
  }

}
#endregion

#region UnifiedHeight
/// <summary>
///   Property to access the unified height of the window
/// </summary>
public class UnifiedHeight : Property {

  /// <summary>Initializes a new instance of the UnifiedHeight property</summary>
  public UnifiedHeight() :
    base(
      "UnifiedHeight",
      "Property to get/set the windows unified height. Value is a \"UDim\".",
      "{0,0}",
      false
    ) {}

  /// <summary>Return the current value of the Property as a String</summary>
  /// <param name="receiver">target object</param>
  /// <returns>
  ///   A string containing a UnifiedPositionual representation of the current value of
  ///   the property
  /// </returns>
  public override string Get(PropertySet receiver) {
    throw new Exception("The method or operation is not implemented.");
  }

  /// <summary>Sets the value of the property</summary>
  /// <param name="receiver">target object</param>
  /// <param name="val">
  ///   A string that contains a UnifiedPositionual representation of the new value to
  ///   assign to the property
  /// </param>
  public override void Set(PropertySet receiver, string val) {
    throw new Exception("The method or operation is not implemented.");
  }

}
#endregion

#region UnifiedMinSize
/// <summary>
///   Property to access the unified minimum size of the window
/// </summary>
public class UnifiedMinSize : Property {

  /// <summary>Initializes a new instance of the UnifiedMinSize property</summary>
  public UnifiedMinSize() :
    base(
      "UnifiedMinSize",
      "Property to get/set the windows unified minimum size. Value is a \"UVector2\".",
      "{{0,0},{0,0}}"
    ) {}

  /// <summary>Return the current value of the Property as a String</summary>
  /// <param name="receiver">target object</param>
  /// <returns>
  ///   A string containing a UnifiedPositionual representation of the current value of
  ///   the property
  /// </returns>
  public override string Get(PropertySet receiver) {
    throw new Exception("The method or operation is not implemented.");
  }

  /// <summary>Sets the value of the property</summary>
  /// <param name="receiver">target object</param>
  /// <param name="val">
  ///   A string that contains a UnifiedPositionual representation of the new value to
  ///   assign to the property
  /// </param>
  public override void Set(PropertySet receiver, string val) {
    throw new Exception("The method or operation is not implemented.");
  }

}
#endregion

#region UnifiedMaxSize
/// <summary>
///   Property to access the unified maximum size of the window
/// </summary>
public class UnifiedMaxSize : Property {

  /// <summary>Initializes a new instance of the UnifiedMinSize property</summary>
  public UnifiedMaxSize() :
    base(
      "UnifiedMaxSize",
      "Property to get/set the windows unified maximum size. Value is a \"UVector2\".",
      "{{0,0},{0,0}}"
    ) {}

  /// <summary>Return the current value of the Property as a String</summary>
  /// <param name="receiver">target object</param>
  /// <returns>
  ///   A string containing a UnifiedPositionual representation of the current value of
  ///   the property
  /// </returns>
  public override string Get(PropertySet receiver) {
    throw new Exception("The method or operation is not implemented.");
  }

  /// <summary>Sets the value of the property</summary>
  /// <param name="receiver">target object</param>
  /// <param name="val">
  ///   A string that contains a UnifiedPositionual representation of the new value to
  ///   assign to the property
  /// </param>
  public override void Set(PropertySet receiver, string val) {
    throw new Exception("The method or operation is not implemented.");
  }

}
#endregion

#region MousePassThroughEnabled
/// <summary>
///   Property to access whether the window ignores mouse events and pass them through to any
///   windows behind it.
/// </summary>
public class MousePassThroughEnabled : Property {

  /// <summary>Initializes a new instance of the MousePassThroughEnabled property</summary>
  public MousePassThroughEnabled() :
    base(
      "MousePassThroughEnabled",
      "Property to get/set whether the window ignores mouse events and pass them through" +
        "to any windows behind it. Value is either \"True\" or \"False\".",
      "False"
    ) {}

  /// <summary>Return the current value of the Property as a String</summary>
  /// <param name="receiver">target object</param>
  /// <returns>
  ///   A string containing a UnifiedPositionual representation of the current value of
  ///   the property
  /// </returns>
  public override string Get(PropertySet receiver) {
    throw new Exception("The method or operation is not implemented.");
  }

  /// <summary>Sets the value of the property</summary>
  /// <param name="receiver">target object</param>
  /// <param name="val">
  ///   A string that contains a UnifiedPositionual representation of the new value to
  ///   assign to the property
  /// </param>
  public override void Set(PropertySet receiver, string val) {
    throw new Exception("The method or operation is not implemented.");
  }

}
#endregion

#region WindowRenderer
/// <summary>
///   roperty to access/change the assigned window renderer object
/// </summary>
public class WindowRenderer : Property {

  /// <summary>Initializes a new instance of the WindowRenderer property</summary>
  public WindowRenderer() :
    base(
      "WindowRenderer",
      "Property to get/set the windows assigned window renderer objects name. " +
        "Value is a string.",
      ""
    ) {}

  /// <summary>Return the current value of the Property as a String</summary>
  /// <param name="receiver">target object</param>
  /// <returns>
  ///   A string containing a UnifiedPositionual representation of the current value of
  ///   the property
  /// </returns>
  public override string Get(PropertySet receiver) {
    throw new Exception("The method or operation is not implemented.");
  }

  /// <summary>Sets the value of the property</summary>
  /// <param name="receiver">target object</param>
  /// <param name="val">
  ///   A string that contains a UnifiedPositionual representation of the new value to
  ///   assign to the property
  /// </param>
  public override void Set(PropertySet receiver, string val) {
    throw new Exception("The method or operation is not implemented.");
  }

  /// <summary>Writes the property into an Xml stream</summary>
  /// <param name="receiver">No idea what this parameter could ever be good for</param>
  /// <param name="xmlStream">Stream into which the property is written</param>
  public void WriteXmlToStream(PropertyReceiver receiver, XmlSerializer xmlStream) {
    throw new Exception("The method or operation is not implemented.");
  }

}
#endregion

#region LookNFeel
/// <summary>
///   Property to access/change the assigned look'n'feel
/// </summary>
public class LookNFeel : Property {

  /// <summary>Initializes a new instance of the LookNFeel property</summary>
  public LookNFeel() :
    base(
      "LookNFeel",
      "Property to get/set the windows assigned look'n'feel. Value is a string.",
      ""
    ) {}

  /// <summary>Return the current value of the Property as a String</summary>
  /// <param name="receiver">target object</param>
  /// <returns>
  ///   A string containing a UnifiedPositionual representation of the current value of
  ///   the property
  /// </returns>
  public override string Get(PropertySet receiver) {
    throw new Exception("The method or operation is not implemented.");
  }

  /// <summary>Sets the value of the property</summary>
  /// <param name="receiver">target object</param>
  /// <param name="val">
  ///   A string that contains a UnifiedPositionual representation of the new value to
  ///   assign to the property
  /// </param>
  public override void Set(PropertySet receiver, string val) {
    throw new Exception("The method or operation is not implemented.");
  }

  /// <summary>Writes the property into an Xml stream</summary>
  /// <param name="receiver">No idea what this parameter could ever be good for</param>
  /// <param name="xmlStream">Stream into which the property is written</param>
  public void WriteXmlToStream(PropertyReceiver receiver, XmlSerializer xmlStream) {
    throw new Exception("The method or operation is not implemented.");
  }

}
#endregion

} // namespace CeGui.WindowProperties
