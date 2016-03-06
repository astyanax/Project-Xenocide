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

public class PropertyDimension : BaseDimension {
  protected string property;
  protected string childSuffix;

  public PropertyDimension(string name, string property) {
    this.property = property;
    childSuffix = name;
  }

  protected override float GetValueImpl(Window wnd) {
    // get window to use.
    Window sourceWindow = (childSuffix.Length == 0) ? wnd : WindowManager.Instance.GetWindow(wnd.Name + childSuffix);
    // return property value.
    //return PropertyHelper.stringToFloat (sourceWindow.GetProperty (property));
    throw new NotImplementedException();
  }

  protected override float GetValueImpl(Window wnd, Rect container) {
    return GetValueImpl(wnd);
  }

  protected override void WriteToXmlImpl(XmlWriter writer) {
  }
}

} // namespace CeGui
