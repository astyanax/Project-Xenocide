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

public class ImageDimension : BaseDimension {
  string imageset;
  string image;
  DimensionType dimension;

  public ImageDimension(string imageset, string image, DimensionType dim) {
    this.imageset = imageset;
    this.image = image;
    this.dimension = dim;
  }

  public void SetSourceImage(string imageset, string image) {
    this.image = image;
    this.imageset = imageset;
  }

  public void SetSourceDimension(DimensionType dim) {
    this.dimension = dim;
  }

  protected override float GetValueImpl(Window wnd) {
    Image img = ImagesetManager.Instance.GetImageset(imageset).GetImage(image);

    switch(dimension) {
      case DimensionType.Width:
        return img.Width;

      case DimensionType.Height:
        return img.Height;

      case DimensionType.XOffset:
        return img.OffsetX;

      case DimensionType.YOffset:
        return img.OffsetY;

      case DimensionType.LeftEdge:
      case DimensionType.XPosition:
        return img.SourceTextureArea.Left;

      case DimensionType.TopEdge:
      case DimensionType.YPosition:
        return img.SourceTextureArea.Top;

      case DimensionType.RightEdge:
        return img.SourceTextureArea.Right;

      case DimensionType.BottomEdge:
        return img.SourceTextureArea.Bottom;

      default:
        throw new InvalidRequestException("ImageDim::getValue - unknown or unsupported DimensionType encountered.");
    }
  }

  protected override float GetValueImpl(Window wnd, Rect container) {
    return GetValueImpl(wnd);
  }

  protected override void WriteToXmlImpl(XmlWriter writer) {
    writer.WriteStartElement("ImageDim");
    writer.WriteAttributeString("imageset", imageset);
    writer.WriteAttributeString("image", image);
    writer.WriteAttributeString("dimension", dimension.ToString());
  }
}

} // namespace CeGui
