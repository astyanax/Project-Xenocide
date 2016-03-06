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

public class FontDimension : BaseDimension {
  protected string font;
  protected string text;
  protected string childSuffix;
  protected FontMetricType metric;
  protected float padding;

  public FontDimension(string name, string font, string text, FontMetricType metric)
    : this(name, font, text, metric, 0.0f) {
  }
  public FontDimension(string name, string font, string text, FontMetricType metric, float padding) {
    this.font = font;
    this.text = text;
    this.childSuffix = name;
    this.metric = metric;
    this.padding = padding;
  }

  protected override float GetValueImpl(Window wnd) {
    // get window to use.
    Window sourceWindow = (childSuffix.Length == 0) ? wnd : WindowManager.Instance.GetWindow(wnd.Name + childSuffix);
    // get font to use
    Font fontObj = (font.Length == 0) ? sourceWindow.Font : FontManager.Instance.GetFont(font);

    if(fontObj != null) {
      switch(metric) {
        case FontMetricType.LineSpacing:
          return fontObj.LineSpacing + padding;

        case FontMetricType.Baseline:
          return fontObj.Baseline + padding;

        case FontMetricType.HorxExtent:
          return fontObj.GetTextExtent((text.Length == 0) ? sourceWindow.Text : text) + padding;

        default:
          throw new InvalidRequestException("FontDim::getValue - unknown or unsupported FontMetricType encountered.");
      }
    }
      // no font, return padding value only.
  else {
      return padding;
    }
  }

  protected override float GetValueImpl(Window wnd, Rect container) {
    return GetValueImpl(wnd);
  }

  protected override void WriteToXmlImpl(XmlWriter writer) {
    writer.WriteStartElement("FontDim");

    if(childSuffix.Length > 0) {
      writer.WriteAttributeString("widget", childSuffix);
    }

    if(font.Length > 0) {
      writer.WriteAttributeString("font", font);
    }

    if(text.Length > 0) {
      writer.WriteAttributeString("string", text);
    }

    if(padding != 0) {
      writer.WriteAttributeString("padding", padding.ToString());
    }

    writer.WriteAttributeString("type", metric.ToString());
  }
}

} // namespace CeGui
