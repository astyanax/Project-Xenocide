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
using System.Text;
using System.Drawing;

namespace CeGui {

/// <summary>Class that represents a single Image of an <see cref="Imageset" /></summary>
public class Image : ICloneable {

  /// <summary>Default constructor</summary>
  internal Image() {}

  #region Overloaded constructors for default arguments

  /// <summary>
  ///   Constructor for Image objects. This is not normally used directly by client code,
  ///   use the Imageset interface instead.
  /// </summary>
  /// <param name="name">String object describing the name of the image being created</param>
  /// <param name="owner">
  ///   Pointer to a Imageset object that owns this Image. This must not be NULL
  /// </param>
  /// <param name="area">
  ///   Rect object describing an area that will be associated with this image
  /// </param>
  /// <param name="renderOffset">
  ///   Point object that describes the offset to be applied when rendering this image
  /// </param>
  internal Image(string name, Imageset owner, Rect area, PointF renderOffset) :
    this(name, owner, area, renderOffset, 1.0f, 1.0f) {}

  /// <summary>
  ///   Constructor for Image objects. This is not normally used directly by client code,
  ///   use the Imageset interface instead.
  /// </summary>
  /// <param name="name">String object describing the name of the image being created</param>
  /// <param name="owner">
  ///   Pointer to a Imageset object that owns this Image. This must not be NULL
  /// </param>
  /// <param name="area">
  ///   Rect object describing an area that will be associated with this image
  /// </param>
  /// <param name="renderOffset">
  ///   Point object that describes the offset to be applied when rendering this image
  /// </param>
  /// <param name="horzScaling">
  ///   float value indicating the initial horizontal scaling to be applied to this image
  /// </param>
  internal Image(
    string name, Imageset owner, Rect area, PointF renderOffset,
    float horzScaling
  ) : this(name, owner, area, renderOffset, horzScaling, 1.0f) {}

  #endregion

  /// <summary>
  ///   Constructor for Image objects. This is not normally used directly by client code,
  ///   use the Imageset interface instead.
  /// </summary>
  /// <param name="name">String object describing the name of the image being created</param>
  /// <param name="owner">
  ///   Pointer to a Imageset object that owns this Image. This must not be NULL
  /// </param>
  /// <param name="area">
  ///   Rect object describing an area that will be associated with this image
  /// </param>
  /// <param name="renderOffset">
  ///   Point object that describes the offset to be applied when rendering this image
  /// </param>
  /// <param name="horzScaling">
  ///   float value indicating the initial horizontal scaling to be applied to this image
  /// </param>
  /// <param name="vertScaling">
  ///   float value indicating the initial vertical scaling to be applied to this image
  /// </param>
  internal Image(
    string name, Imageset owner, Rect area, PointF renderOffset,
    float horzScaling, float vertScaling
  ) {
    if(owner == null)
      throw new NullReferenceException("A null owner cannot be specified for an Image");

    this.name = name;
    this.owner = owner;
    this.area = area;
    this.offset = renderOffset;

    // setup initial image scaling
    SetHorizontalScaling(horzScaling);
    SetVerticalScaling(vertScaling);

    // TODO: if we ever store texture co-ordinates, they should be calculated here.
  }

  /// <summary>Queue the image to be drawn</summary>
  /// <remarks>
  ///		The final position of the Image will be adjusted by the offset values defined for this Image object.  If absolute positioning is
  ///		essential then these values should be taken into account prior to calling the draw() methods.  However, by doing this you take
  ///		away the ability of the Imageset designer to adjust the alignment and positioning of Images, therefore your component is far
  ///		less useful since it requires code changes to modify image positioning that could have been handled from a data file.
  /// </remarks>
  /// <param name="position"></param>
  /// <param name="size"></param>
  /// <param name="clipRect"></param>
  /// <param name="colors"></param>
  public void Draw(Vector3 position, SizeF size, Rect clipRect, ColourRect colors) {
    Draw(position, size, clipRect, colors, QuadSplitMode.TopLeftToBottomRight);
  }
  public void Draw(Vector3 position, SizeF size, Rect clipRect, ColourRect colors, QuadSplitMode quadSplitMode) {
    // build a destination Rect based on position and size info
    Rect destRect = new Rect(
      position.x, position.y, position.x + size.Width, position.y + size.Height
    );

    // call the overloaded method
    Draw(destRect, position.z, clipRect, colors, quadSplitMode);
  }

  /// <summary>
  ///		
  /// </summary>
  /// <param name="position"></param>
  /// <param name="clipRect"></param>
  /// <param name="colors"></param>
  public void Draw(Vector3 position, Rect clipRect, ColourRect colors) {
    Draw(position, clipRect, colors, QuadSplitMode.TopLeftToBottomRight);
  }
  public void Draw(Vector3 position, Rect clipRect, ColourRect colors, QuadSplitMode quadSplitMode) {
    // build a destionation Rect based on position and SizeF info
    Rect destRect = new Rect(
        position.x,
        position.y,
        position.x + this.Width,
        position.y + this.Height);

    // call the overloaded method
    Draw(destRect, position.z, clipRect, colors, quadSplitMode);
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="destRect"></param>
  /// <param name="z"></param>
  /// <param name="clipRect"></param>
  /// <param name="colors"></param>
  public void Draw(Rect destRect, float z, Rect clipRect, ColourRect colors) {
    Draw(destRect, z, clipRect, colors, QuadSplitMode.TopLeftToBottomRight);
  }
  public void Draw(Rect destRect, float z, Rect clipRect, ColourRect colors, QuadSplitMode quadSplitMode) {
    Rect dest = destRect;

    // apply rendering offset to the destination Rect
    dest.Offset(scaledOffset);

    // draw
    owner.Draw(area, dest, z, clipRect, colors, quadSplitMode);
  }

  public void WriteToXml(System.Xml.XmlWriter writer) {
    writer.WriteStartElement("Image");
    writer.WriteAttributeString("Name", name);
    writer.WriteAttributeString("XPos", area.Left.ToString());
    writer.WriteAttributeString("YPos", area.Top.ToString());
    writer.WriteAttributeString("Width", area.Width.ToString());
    writer.WriteAttributeString("Height", area.Height.ToString());

    if(offset.X != 0.0f) {
      writer.WriteAttributeString("XOffset", offset.X.ToString());
    }
    if(offset.Y != 0.0f) {
      writer.WriteAttributeString("YOffset", offset.Y.ToString());
    }

    writer.WriteEndElement();
  }

  /// <summary>A <see cref="SizeF"/> object containing the dimensions of the Image</summary>
  /// <value>
  ///   SizeF object holding the width and height of the Image.
  ///   Note: The size may be scaled from the original size of the image.
  /// </value>
  public SizeF Size {
    get { return scaledSize; }
  }

  /// <summary>Return the pixel width of the image</summary>
  /// <value>Width of this Image in pixels</value>
  public float Width {
    get { return scaledSize.Width; }
  }

  /// <summary>Return the pixel height of the image</summary>
  /// <value>Height of this Image in pixels</value>
  public float Height {
    get { return scaledSize.Height; }
  }

  /// <summary>
  ///   Gets a <see cref="Point"/> that contains the offset applied when rendering this Image
  /// </summary>
  /// <value><see cref="Point"/> containing the offsets applied when rendering this Image</value>
  public PointF Offset {
    get { return scaledOffset; }
  }

  /// <summary>Return the X rendering offset</summary>
  /// <value>
  ///   X rendering offset. This is the number of pixels that the image is offset by when
  ///   rendering at any given location
  /// </value>
  public float OffsetX {
    get { return scaledOffset.X; }
  }

  /// <summary>Return the Y rendering offset</summary>
  /// <value>
  ///   Y rendering offset. This is the number of pixels that the image is offset by
  ///   when rendering at any given location
  /// </value>
  public float OffsetY {
    get { return scaledOffset.X; }
  }

  /// <summary>Returns the name of this image</summary>
  /// <value>String containing the name of this image</value>
  public string Name {
    get { return name; }
  }

  /// <summary>Returns the name of the <see cref="Imageset"/> who owns this image</summary>
  /// <value>String containing the imageset who owns this image.</value>
  public string ImagesetName {
    get { return owner.Name; }
  }

  /// <summary>
  ///   Returns a <see cref="Rect" /> describing the source texture area used by this Image
  /// </summary>
  /// <value>
  ///   Rect object that describes, in pixels, the area upon the source texture which is
  ///   used when rendering this Image.
  /// </value>
  public Rect SourceTextureArea {
    get { return area; }
  }

  /// <summary>Set the horizontal scaling factor to be applied to this Image</summary>
  /// <param name="factor">Float value describing the scaling factor required</param>
  internal void SetHorizontalScaling(float factor) {
    scaledSize.Width = area.Width * factor;
    scaledOffset.X = offset.X * factor;
  }

  /// <summary>Set the vertical scaling factor to be applied to this Image</summary>
  /// <param name="factor">Float value describing the scaling factor required</param>
  internal void SetVerticalScaling(float factor) {
    scaledSize.Height = area.Height * factor;
    scaledOffset.Y = offset.Y * factor;
  }

  /// <summary>Returns a string representation of this image</summary>
  /// <remarks>
  ///   The format of the returned string will be "set:[ImageSet Name] image:[Image Name]"
  /// </remarks>
  /// <returns>The string representation of this image</returns>
  public override string ToString() {
    return string.Format("set:{0} image:{1}", ImagesetName, Name);
  }

  /// <summary>
  ///   Parses a string in the form, "set:[ImageSet Name] image:[Image Name]", and returns
  ///   the corresponding image if it is available
  /// </summary>
  /// <param name="data">String to parse</param>
  /// <returns>Returns the image corresponding to the string representation</returns>
  public static Image Parse(string data) {
    string set = "";
    string image = "";

    // break the string into parameter lists
    string[] parameters = data.Split(new char[] { ' ', ':' });

    // parse the parameter list
    for(int i = 0; i < parameters.Length; i++) {
      if(parameters[i].CompareTo("set") == 0) {
        set = parameters[++i];
      } else if(parameters[i].CompareTo("image") == 0) {
        image = parameters[++i];
      }
    }

    // return the requested image
    if(image != "")
      return ImagesetManager.Instance.GetImageset(set).GetImage(image);
    else
      return null;
  }

  /// <summary>Creates a copy of this Image</summary>
  /// <returns>A copy of this image</returns>
  public object Clone() {
    Image clone = (Image)this.MemberwiseClone();

    return clone;
    // TODO: Test me
  }

  /// <summary>Link back to Imageset that owns this image</summary>
  protected readonly Imageset owner;
  /// <summary>Rect defining the area on the texture that makes up this image</summary>
  protected Rect area;
  /// <summary>Offset to use when rendering</summary>
  protected PointF offset;
  /// <summary>Scaled image size</summary>
  protected SizeF scaledSize;
  /// <summary>Scaled rendering offset</summary>
  protected PointF scaledOffset;
  /// <summary>Name of this image</summary>
  protected string name;

}

} // namespace CeGui
