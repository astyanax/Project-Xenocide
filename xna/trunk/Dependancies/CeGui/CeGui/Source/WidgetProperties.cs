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
using System.Globalization;
using System.Reflection;

namespace CeGui {

/// <summary>
///	This is an attribute intended to be attached to a widget to specify an alternate
///	name to use for identifying the widget.
/// </summary>
[ AttributeUsage(AttributeTargets.Class, AllowMultiple = true) ]
public class AlternateWidgetNameAttribute : Attribute {
  string mName;

  public AlternateWidgetNameAttribute(string name) {
    mName = name;
  }

  public string Name {
    get {
      return mName;
    }
  }
}

/// <summary>
/// Base attribute class for exposing properties to the layout code.
/// </summary>
public class WidgetPropertyAttribute : Attribute {
  string mName;

  public WidgetPropertyAttribute(string name) {
    mName = name;
  }

  /// <summary>
  /// Name of the property
  /// </summary>
  /// <value>Name of the property</value>
  public string Name {
    get {
      return mName;
    }
  }

  /// <summary>
  /// Gets the string representation of the value of the property.
  /// </summary>
  /// <param name="obj">Object to check</param>
  /// <param name="property">Property to check</param>
  /// <returns>String representation of the property</returns>
  public virtual string GetValue(object obj, PropertyInfo property) {
    return property.GetValue(obj, null).ToString();
  }
  /// <summary>
  /// Sets the property to the value represented by the string.
  /// </summary>
  /// <param name="obj">Object to set the property of</param>
  /// <param name="property">Property to set</param>
  /// <param name="value">String representation of the property.</param>
  public virtual void SetValue(object obj, PropertyInfo property, string value) {
    MethodInfo[] methods = property.PropertyType.GetMethods(BindingFlags.Static | BindingFlags.Public);

    // search for a static Parse method that's culture insenstive
    for(int i = 0; i < methods.Length; i++) {
      if(methods[i].Name.CompareTo("Parse") == 0 &&
           methods[i].GetParameters().Length == 2 &&
           methods[i].GetParameters()[0].ParameterType == typeof(string) &&
           methods[i].GetParameters()[1].ParameterType == typeof(IFormatProvider)) {
        property.SetValue(obj, methods[i].Invoke(null, new object[] { value, CultureInfo.InvariantCulture }), null);
        return;
      }
    }

    // search for a static Parse method
    for(int i = 0; i < methods.Length; i++) {
      if(methods[i].Name.CompareTo("Parse") == 0 &&
           methods[i].GetParameters().Length == 1 &&
           methods[i].GetParameters()[0].ParameterType == typeof(string)) {
        property.SetValue(obj, methods[i].Invoke(null, new object[] { value }), null);
      }
    }
  }
}

/// <summary>
/// Specialized property class because the string object does not have a Parse method.
/// </summary>
public class StringPropertyAttribute : WidgetPropertyAttribute {
  public StringPropertyAttribute(string name)
    : base(name) {
  }

  public override void SetValue(object obj, PropertyInfo property, string value) {
    property.SetValue(obj, value, null);
  }
}

/// <summary>
/// Spectialized property attribute for MetricsMode properties.
/// </summary>
public class MetricsModePropertyAttribute : WidgetPropertyAttribute {
  public MetricsModePropertyAttribute(string name)
    : base(name) {
  }

  public override string GetValue(object obj, PropertyInfo property) {
    MetricsMode mode = (MetricsMode)property.GetValue(obj, null);
    switch(mode) {
      case MetricsMode.Absolute:
        return "Absolute";

      case MetricsMode.Inherited:
        return "Inherited";

      case MetricsMode.Relative:
      default:
        return "Relative";
    }
  }

  public override void SetValue(object obj, PropertyInfo property, string value) {
    MetricsMode mode = MetricsMode.Relative;
    string temp = value.ToLower();
    if(0 == temp.CompareTo("absolute")) {
      mode = MetricsMode.Absolute;
    } else if(0 == temp.CompareTo("inherited")) {
      mode = MetricsMode.Inherited;
    } else if(0 == temp.CompareTo("relative")) {
      mode = MetricsMode.Relative;
    } else {
      Logger.Instance.LogEvent("\"" + value + "\" is not a valid MetricsMode", LoggingLevel.Informative);
    }

    property.SetValue(obj, mode, null);
  }
}

/// <summary>
/// Spectialized property attribute for HorizontalImageFormat properties.
/// </summary>
public class HorzizontalImageFormatPropertyAttribute : WidgetPropertyAttribute {
  public HorzizontalImageFormatPropertyAttribute(string name)
    : base(name) {
  }

  public override string GetValue(object obj, PropertyInfo property) {
    HorizontalImageFormat format = (HorizontalImageFormat)property.GetValue(obj, null);
    switch(format) {
      case HorizontalImageFormat.RightAligned:
        return "RightAligned";

      case HorizontalImageFormat.Centered:
        return "HorzCentred";

      case HorizontalImageFormat.Stretched:
        return "HorzStretched";

      case HorizontalImageFormat.Tiled:
        return "HorzTiled";

      case HorizontalImageFormat.LeftAligned:
      default:
        return "LeftAligned";
    }
  }

  public override void SetValue(object obj, PropertyInfo property, string value) {
    HorizontalImageFormat format = HorizontalImageFormat.Centered;
    string temp = value.ToLower();
    if(0 == temp.CompareTo("rightaligned")) {
      format = HorizontalImageFormat.RightAligned;
    } else if(0 == temp.CompareTo("horzcentered"))//FIXED: Spelling form horzcentred
		{
      format = HorizontalImageFormat.Centered;
    } else if(0 == temp.CompareTo("horzstretched")) {
      format = HorizontalImageFormat.Stretched;
    } else if(0 == temp.CompareTo("horztiled")) {
      format = HorizontalImageFormat.Tiled;
    } else if(0 == temp.CompareTo("leftaligned")) {
      format = HorizontalImageFormat.LeftAligned;
    } else {
      Logger.Instance.LogEvent("\"" + value + "\" is not a valid HorizontalImageFormat", LoggingLevel.Informative);
    }

    property.SetValue(obj, format, null);
  }
}

/// <summary>
/// Spectialized property attribute for VerticalImageFormat properties.
/// </summary>
public class VerticalImageFormatPropertyAttribute : WidgetPropertyAttribute {
  public VerticalImageFormatPropertyAttribute(string name)
    : base(name) {
  }

  public override string GetValue(object obj, PropertyInfo property) {
    VerticalImageFormat format = (VerticalImageFormat)property.GetValue(obj, null);
    switch(format) {
      case VerticalImageFormat.BottomAligned:
        return "BottomAligned";

      case VerticalImageFormat.Centered:
        return "VertCentred";

      case VerticalImageFormat.Stretched:
        return "VertStretched";

      case VerticalImageFormat.Tiled:
        return "VertTiled";

      case VerticalImageFormat.TopAligned:
      default:
        return "TopAligned";
    }
  }

  public override void SetValue(object obj, PropertyInfo property, string value) {
    VerticalImageFormat format = VerticalImageFormat.Centered;
    string temp = value.ToLower();
    if(0 == temp.CompareTo("bottomaligned")) {
      format = VerticalImageFormat.BottomAligned;
    } else if(0 == temp.CompareTo("vertcentred")) {
      format = VerticalImageFormat.Centered;
    } else if(0 == temp.CompareTo("vertstretched")) {
      format = VerticalImageFormat.Stretched;
    } else if(0 == temp.CompareTo("verttiled")) {
      format = VerticalImageFormat.Tiled;
    } else if(0 == temp.CompareTo("topaligned")) {
      format = VerticalImageFormat.TopAligned;
    } else {
      Logger.Instance.LogEvent("\"" + value + "\" is not a valid HorizontalImageFormat", LoggingLevel.Informative);
    }

    property.SetValue(obj, format, null);
  }
}

/// <summary>
/// Spectialized property attribute for HorizontalTextFormat properties.
/// </summary>
public class HorizontalTextFormatPropertyAttribute : WidgetPropertyAttribute {
  public HorizontalTextFormatPropertyAttribute(string name)
    : base(name) {
  }

  public override string GetValue(object obj, PropertyInfo property) {
    HorizontalTextFormat format = (HorizontalTextFormat)property.GetValue(obj, null);
    switch(format) {
      case HorizontalTextFormat.Center:
        return "HorzCentred";

      case HorizontalTextFormat.Right:
        return "RightAligned";

      case HorizontalTextFormat.WordWrapLeft:
        return "WordWrapLeftAligned";

      case HorizontalTextFormat.WordWrapCentered:
        return "WordWrapCentred";

      case HorizontalTextFormat.WordWrapRight:
        return "WordWrapRightAligned";

      case HorizontalTextFormat.Left:
      default:
        return "LeftAligned";
    }
  }

  public override void SetValue(object obj, PropertyInfo property, string value) {
    HorizontalTextFormat format = HorizontalTextFormat.Center;
    string temp = value.ToLower();
    if(0 == temp.CompareTo("leftaligned")) {
      format = HorizontalTextFormat.Left;
    } else if(0 == temp.CompareTo("rightaligned")) {
      format = HorizontalTextFormat.Right;
    } else if(0 == temp.CompareTo("horzcentred")) {
      format = HorizontalTextFormat.Center;
    } else if(0 == temp.CompareTo("wordwrapleftaligned")) {
      format = HorizontalTextFormat.WordWrapLeft;
    } else if(0 == temp.CompareTo("wordwraprightaligned")) {
      format = HorizontalTextFormat.WordWrapRight;
    } else if(0 == temp.CompareTo("wordwrapcentred")) {
      format = HorizontalTextFormat.WordWrapCentered;
    } else {
      Logger.Instance.LogEvent("\"" + value + "\" is not a valid HorizontalTextFormat", LoggingLevel.Informative);
    }

    property.SetValue(obj, format, null);
  }
}

/// <summary>
/// Spectialized property attribute for VerticalTextFormat properties.
/// </summary>
public class VerticalTextFormatPropertyAttribute : WidgetPropertyAttribute {
  public VerticalTextFormatPropertyAttribute(string name)
    : base(name) {
  }

  public override string GetValue(object obj, PropertyInfo property) {
    VerticalTextFormat format = (VerticalTextFormat)property.GetValue(obj, null);
    switch(format) {
      case VerticalTextFormat.Top:
        return "TopAligned";

      case VerticalTextFormat.Bottom:
        return "BottomAligned";

      case VerticalTextFormat.Centered:
      default:
        return "VertCentred";
    }
  }

  public override void SetValue(object obj, PropertyInfo property, string value) {
    VerticalTextFormat format = VerticalTextFormat.Centered;
    string temp = value.ToLower();
    if(0 == temp.CompareTo("topaligned")) {
      format = VerticalTextFormat.Top;
    } else if(0 == temp.CompareTo("bottomaligned")) {
      format = VerticalTextFormat.Bottom;
    } else if(0 == temp.CompareTo("vertcentred")) {
      format = VerticalTextFormat.Centered;
    } else {
      Logger.Instance.LogEvent("\"" + value + "\" is not a valid VerticalTextFormat", LoggingLevel.Informative);
    }

    property.SetValue(obj, format, null);
  }
}

/// <summary>
/// Flags a function as an event handler
/// </summary>
public class GuiEventAttribute : Attribute {
  public GuiEventAttribute() {
  }
}

/// <summary>
/// Specialised property attribute for SortDirection properties.
/// </summary>
public class SortDirectionPropertyAttribute : WidgetPropertyAttribute {
  public SortDirectionPropertyAttribute(string name)
    : base(name) {
  }

  public override string GetValue(object obj, PropertyInfo property) {
    switch((SortDirection)property.GetValue(obj, null)) {
      case SortDirection.Ascending:
        return "Ascending";

      case SortDirection.Descending:
        return "Descending";

      case SortDirection.None:
      default:
        return "None";
    }
  }

  public override void SetValue(object obj, PropertyInfo property, string value) {
    SortDirection sortDirection = SortDirection.None;

    string temp = value.ToLower();

    if(temp.CompareTo("ascending") == 0) {
      sortDirection = SortDirection.Ascending;
    } else if(temp.CompareTo("descending") == 0) {
      sortDirection = SortDirection.Descending;
    } else if(temp.CompareTo("none") == 0) {
      sortDirection = SortDirection.None;
    } else {
      Logger.Instance.LogEvent("\"" + value + "\" is not a valid SortDirection", LoggingLevel.Informative);
    }

    property.SetValue(obj, sortDirection, null);
  }
}

/// <summary>
/// Specialised property attribute for GridSelectionMode properties.
/// </summary>
public class GridSelectionModePropertyAttribute : WidgetPropertyAttribute {
  public GridSelectionModePropertyAttribute(string name)
    : base(name) {
  }

  public override string GetValue(object obj, PropertyInfo property) {
    switch((GridSelectionMode)property.GetValue(obj, null)) {
      case GridSelectionMode.RowMultiple:
        return "RowMultiple";

      case GridSelectionMode.CellSingle:
        return "CellSingle";

      case GridSelectionMode.CellMultiple:
        return "CellMultiple";

      case GridSelectionMode.NominatedColumnSingle:
        return "NominatedColumnSingle";

      case GridSelectionMode.NominatedColumnMultiple:
        return "NominatedColumnMultiple";

      case GridSelectionMode.ColumnSingle:
        return "ColumnSingle";

      case GridSelectionMode.ColumnMultiple:
        return "ColumnMultiple";

      case GridSelectionMode.NominatedRowSingle:
        return "NominatedRowSingle";

      case GridSelectionMode.NominatedRowMultiple:
        return "NominatedRowMultiple";

      case GridSelectionMode.RowSingle:
      default:
        return "RowSingle";
    }
  }

  public override void SetValue(object obj, PropertyInfo property, string value) {
    GridSelectionMode mode = GridSelectionMode.RowSingle;

    string temp = value.ToLower();

    if(temp.CompareTo("rowmultiple") == 0) {
      mode = GridSelectionMode.RowMultiple;
    } else if(temp.CompareTo("cellsingle") == 0) {
      mode = GridSelectionMode.CellSingle;
    }
    if(temp.CompareTo("cellmultiple") == 0) {
      mode = GridSelectionMode.CellMultiple;
    } else if(temp.CompareTo("nominatedcolumnsingle") == 0) {
      mode = GridSelectionMode.NominatedColumnSingle;
    }
    if(temp.CompareTo("nominatedcolumnmultiple") == 0) {
      mode = GridSelectionMode.NominatedColumnMultiple;
    } else if(temp.CompareTo("columnsingle") == 0) {
      mode = GridSelectionMode.ColumnSingle;
    }
    if(temp.CompareTo("columnmultiple") == 0) {
      mode = GridSelectionMode.ColumnMultiple;
    } else if(temp.CompareTo("nominatedrowsingle") == 0) {
      mode = GridSelectionMode.NominatedRowSingle;
    }
    if(temp.CompareTo("nominatedrowmultiple") == 0) {
      mode = GridSelectionMode.NominatedRowMultiple;
    } else {
      Logger.Instance.LogEvent("\"" + value + "\" is not a valid GridSelectionMode", LoggingLevel.Informative);
    }

    property.SetValue(obj, mode, null);
  }
}

} // namespace CeGui
