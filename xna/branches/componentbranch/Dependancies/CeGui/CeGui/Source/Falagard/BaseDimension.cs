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
using System.Xml;
using System.Drawing;

namespace CeGui {

public abstract class BaseDimension {
  protected DimensionOperator op;
  protected BaseDimension operand;

  public DimensionOperator DimensionOperator {
    get { return op; }
    set { op = value; }
  }

  public BaseDimension Operand {
    get { return operand; }
    set { operand = value; }
  }

  /// <summary>
  ///		Return a value that represents this dimension as absolute pixels.
  /// </summary>
  /// <param name="wnd">Window object that may be used by the specialised class to aid in calculating the final value.</param>
  /// <returns>float value which represents, in pixels, the same value as this BaseDim.</returns>
  public float GetValue(Window wnd) {
    // get sub-class to return value for this dimension
    float val = GetValueImpl(wnd);

    // if we have an attached operand, perform math on value as needed
    if(null != operand) {
      switch(op) {
        case DimensionOperator.Add:
          val += operand.GetValue(wnd);
          break;
        case DimensionOperator.Subtract:
          val -= operand.GetValue(wnd);
          break;
        case DimensionOperator.Multiply:
          val *= operand.GetValue(wnd);
          break;
        case DimensionOperator.Divide:
          val /= operand.GetValue(wnd);
          break;
        default:
          // Noop
          break;
      }
    }

    return val;
  }

  /// <summary>
  ///		Return a value that represents this dimension as absolute pixels.
  /// </summary>
  /// <param name="wnd">
  ///		Window object that may be used by the specialised class to aid in 
  ///		calculating the final value (typically would be used to obtain window/widget dimensions).
  /// </param>
  /// <param name="container">
  ///		Rect object which describes an area to be considered as the base area when calculating the 
  ///		final value.  Basically this means that relative values are calculated from the dimensions 
  ///		of this Rect.
  /// </param>
  /// <returns>float value which represents, in pixels, the same value as this BaseDim.</returns>
  public float GetValue(Window wnd, Rect container) {
    // get sub-class to return value for this dimension
    float val = GetValueImpl(wnd, container);

    // if we have an attached operand, perform math on value as needed
    if(null != operand) {
      switch(op) {
        case DimensionOperator.Add:
          val += operand.GetValue(wnd, container);
          break;
        case DimensionOperator.Subtract:
          val -= operand.GetValue(wnd, container);
          break;
        case DimensionOperator.Multiply:
          val *= operand.GetValue(wnd, container);
          break;
        case DimensionOperator.Divide:
          val /= operand.GetValue(wnd, container);
          break;
        default:
          // Noop
          break;
      }
    }

    return val;
  }

  public void WriteToXml(XmlWriter writer) {
    WriteToXmlImpl(writer);

    if(null != operand) {
      writer.WriteStartElement("DimOperator");
      writer.WriteAttributeString("op", op.ToString());

      operand.WriteToXml(writer);

      writer.WriteEndElement();
    }

    writer.WriteEndElement();
  }

  #region Virtual Methods

  protected abstract float GetValueImpl(Window wnd);
  protected abstract float GetValueImpl(Window wnd, Rect container);
  protected abstract void WriteToXmlImpl(XmlWriter writer);

  #endregion
}

} // namespace CeGui
