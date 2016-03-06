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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Drawing;
using System.IO;

namespace CeGui {

/// <summary>
///   Offers functions to define, access, and draw, a set of image components 
///   on a single graphical surface or <see cref="Texture"/>
/// </summary>
/// <remarks>
///   Imageset objects are a means by which a single graphical image (file, Texture, etc),
///   can be split into a number of 'components' which can later be accessed via name.
///   The components of an Imageset can queried for various details, and sent to the
///   <see cref="Renderer"/> object for drawing
/// </remarks>
public class Imageset : IDisposable, IEnumerable<Image> {

  /// <summary>Name of the XSD file to use for validation</summary>
  const string SchemaFileName = "Imageset.xsd";

  // TODO: Move into a central location
  /// <summary>Default native horizontal resolution (for fonts and imagesets)</summary>
  const float DefaultNativeHorzRes = 640.0f;
  /// <summary>Default native vertical resolution (for fonts and imagesets)</summary>
  const float DefaultNativeVertRes = 480.0f;

  /// <summary>
  ///   Construct a new Imageset object. Object will initially have no Images defined
  /// </summary>
  /// <param name="name">Name that will be assigned to this image set</param>
  /// <param name="texture">
  ///   Texture object that holds the imagery for the Imageset being created
  /// </param>
  internal Imageset(string name, Texture texture) {
    this.name = name;
    this.texture = texture;

    imageRegistry = new Dictionary<string,Image>();

    if(texture == null) {
      throw new ArgumentNullException(
        "Texture object supplied for Imageset creation must not be null."
      );
    }

    // set scaling defaults
    autoScale = false;
    NativeResolution = new SizeF(DefaultNativeHorzRes, DefaultNativeVertRes);
  }

  /// <summary>
  ///   Internal constructor. Constructs a new Imageset object using data contained
  ///   in the specified file
  /// </summary>
  /// <remarks>
  ///   This is internal, and only needs to be accessible via the various CreateXXX
  ///   methods of the <see cref="ImagesetManager"/>
  /// </remarks>
  /// <param name="fileName">
  ///   The name of the Imageset data file that is to be processed
  /// </param>
  /// <param name="resourceGroup">
  ///   Resource group identifier to be passed to the resource manager.
  ///   NB: This affects the imageset xml file only, the texture loaded may have
  ///   its own group specified in the XML file
  /// </param>
  /// <exception cref="System.IO.FileNotFoundException">
  ///   If the specified file simply does not exist
  /// </exception>
  internal Imageset(string fileName, string resourceGroup) {
    // set scaling defaults
    autoScale = false;
    NativeResolution = new SizeF(DefaultNativeHorzRes, DefaultNativeVertRes);

    imageRegistry = new Dictionary<string,Image>();

    // load the texture from file
    Load(fileName, resourceGroup);
  }

  /// <summary>
  ///   Construct a new Imageset using the specified image file and imageset name. The
  ///   created imageset will, by default, have a single Image defined named "full_image"
  ///   which represents the entire area of the loaded image file.
  /// </summary>
  /// <remarks>
  ///   Under certain renderers it may be required that the source image dimensions be
  ///   some power of 2, if this condition is not met then stretching and other undesired
  ///   side-effects may be experienced. To be safe from such effects it is generally
  ///   recommended that all images that you load have dimensions that are some power of 2
  /// </remarks>
  /// <param name="name">Name to be assigned to the created Imageset</param>
  /// <param name="fileName">Filename of the Image that is to be loaded</param>
  /// <param name="resourceGroup">
  ///   Resource group identifier to be passed to the resource manager, which may specify
  ///   a group from which the image file is to be loaded
  /// </param>
  internal Imageset(string name, string fileName, string resourceGroup) {
    this.name = name;

    // Try to load the image file using the renderer
    texture = GuiSystem.Instance.Renderer.CreateTexture(
      fileName,
      (resourceGroup == string.Empty || resourceGroup == null) ?
         DefaultResourceGroup : resourceGroup
    );

    // Initialize the auto-scaling for this Imageset
    this.autoScale = true;
    NativeResolution = texture.Size;

    // define the default Image for this Imageset
    DefineImage("full_image", new Rect(0, 0, texture.Width, texture.Height), PointF.Empty);
  }

  /// <summary>Cleans up resources in use</summary>
  public void Dispose() {
    Unload();
  }

  /// <summary>The Texture object to be used by this Imageset</summary>
  /// <remarks>
  ///   <para>
  ///     Changing textures on an Imageset that is in use is not a good idea!
  ///   </para>
  ///   <para>
  ///     The old texture is NOT disposed of, that is the clients responsibility.
  ///   </para>
  /// </remarks>
  public Texture Texture {
    get { return texture; }
    set {
      if(value == null)
        throw new ArgumentNullException("An Imageset cannot be set to use a null texture");

      texture = value;
    }
  }

  /// <summary>Name of the Imageset</summary>
  /// <value>String holding the name of this Imageset.</value>
  public string Name {
    get { return name; }
  }

  /// <summary>Gets the number of images defined for this Imageset</summary>
  /// <value>Integer value equal to the number of Image objects defined for the Imageset</value>
  public int ImageCount {
    get { return imageRegistry.Count; }
  }

  /// <summary>Returns true if an Image with the specified name exists</summary>
  /// <param name="name">The name of the image to look for</param>
  /// <returns>
  ///   True if an Image with the given name is defined for this Imageset, else false
  /// </returns>
  public bool IsImageDefined(string name) {
    return this.imageRegistry.ContainsKey(name);
  }

  /// <summary>Gets the image with the specified name</summary>
  /// <param name="name">The name of the Image object to be returned</param>
  /// <returns>Image object that has the requested name</returns>
  public Image GetImage(string name) {
    if(!IsImageDefined(name)) {
      throw new UnknownObjectException(
        "Image '{0}' could not be found in Imageset '{1}'", name, this.name
      );
    }

    return this.imageRegistry[name];
  }
  
  /// <summary>
  ///   Remove the definition for the Image with the specified name.  
  ///   If no such Image exists, nothing happens
  /// </summary>
  /// <param name="name">The name of the Image object to be removed from the Imageset</param>
  public void UndefineImage(string name) {
    this.imageRegistry.Remove(name);
  }

  /// <summary>
  ///   Removes the definitions for all Image objects currently defined in the Imageset
  /// </summary>
  public void UndefineAllImages() {
    this.imageRegistry.Clear();
  }

#if OMITTED_IN_PORT
  /// <summary>Returns a size describing the dimensions of the named image</summary>
  /// <param name="name">The name of the Image</param>
  /// <returns>The dimensions of the requested Image</returns>
  public SizeF GetImageSize(string name) {
    return GetImage(name).Size;
  }

  /// <summary>Return the width of the named image</summary>
  /// <param name="name">The name of the Image</param>
  /// <returns>A float value equalling the width of the requested Image</returns>
  public float GetImageWidth(string name) {
    return GetImage(name).Width;
  }
  
  /// <summary>Return the height of the named image</summary>
  /// <param name="name">The name of the Image</param>
  /// <returns>A float value equalling the height of the requested Image</returns>
  public float GetImageWidth(string name) {
    return GetImage(name).Height;
  }

  /// <summary>Returns the rendering offsets applied to the named image</summary>
  /// <param name="name">The name of the Image</param>
  /// <returns>The rendering offsets for the requested Image</returns>
  public PointF GetImageOffset(string name) {
    return GetImage(name).Offset;
  }

  /// <summary>Returns the x rendering offset for the named image</summary>
  /// <param name="name">The name of the Image</param>
  /// <returns>The x rendering offset applied when drawing the requested Image</returns>
  public float GetImageOffsetX(string name) {
    return GetImage(Name).OffsetX;
  }

  /// <summary>Returns the y rendering offset for the named image</summary>
  /// <param name="name">The name of the Image</param>
  /// <returns>The y rendering offset applied when drawing the requested Image</returns>
  public float GetImageOffsetY(string name) {
    return GetImage(Name).OffsetY;
  }

#endif // OMITTED_IN_PORT

  /// <summary>Define a new Image for this Imageset</summary>
  /// <param name="name">
  ///   The name that will be assigned to the new Image, which must be unique within the Imageset
  /// </param>
  /// <param name="position">
  ///   Point describing the pixel location of the Image on the image file / texture associated
  ///   with this Imageset
  /// </param>
  /// <param name="size">Size describing the dimensions of the Image, in pixels</param>
  /// <param name="renderOffset">
  ///   Point describing the offsets, in pixels, that are to be applied to the Image when it
  ///   is drawn
  /// </param>
  /// <exception cref="AlreadyExistsException">
  ///   If an image named <paramref name="name"/> already exists
  /// </exception>
  public void DefineImage(string name, PointF position, SizeF size, PointF renderOffset) {
    Rect imageRect = new Rect(
      position.X,
      position.Y,
      position.X + size.Width,
      position.Y + size.Height
    );

    DefineImage(name, imageRect, renderOffset);
  }

  /// <summary>Define a new Image for this Imageset</summary>
  /// <param name="name">
  ///   The name that will be assigned to the new Image, which must be unique within the Imageset
  /// </param>
  /// <param name="imageRect">
  ///   The area on the image file / texture associated with this Imageset that will be used for
  ///   the Image
  /// </param>
  /// <param name="renderOffset">
  ///   Point describing the offsets, in pixels, that are to be applied to the Image when it
  ///   is drawn
  /// </param>
  /// <exception cref="AlreadyExistsException">
  ///   If an image named <paramref name="name"/> already exists
  /// </exception>
  public void DefineImage(string name, Rect imageRect, PointF renderOffset) {
    if(IsImageDefined(name)) {
      throw new AlreadyExistsException(
        "Imageset '{0}' already has a definition for Image '{1}'", this.name, name
      );
    }

    // get scaling factor
    float hScale = autoScale ? horzScaling : 1.0f;
    float vScale = autoScale ? vertScaling : 1.0f;

    // add the new image definition
    imageRegistry.Add(name, new Image(name, this, imageRect, renderOffset, hScale, vScale));
  }

  /// <summary>Queues an area of the associated Texture the be drawn on the screen</summary>
  /// <remarks>Low-level routine to be used carefully!</remarks>
  /// <param name="sourceRect">
  ///   Rect describing the area of the image file / texture that is to be queued for drawing
  /// </param>
  /// <param name="destRect">
  ///   Rect describing the area of the screen that will be filled with the imagery
  ///   from <paramref name="sourceRect"/>
  /// </param>
  /// <param name="z">
  ///   float value specifying 'z' order. 0 is topmost with increasing values moving back
  ///   into the screen
  /// </param>
  /// <param name="clipRect">
  ///   Rect describing a 'clipping Rect' that will be applied when drawing the requested imagery
  /// </param>
  /// <param name="colors">
  ///   ColorRect holding the ARGB colors to be applied to the four corners of the
  ///   rendered imagery
  /// </param>
  /// <param name="quadSplitMode">
  ///   One of the QuadSplitMode values specifying the way quads are split into triangles
  /// </param>
  public void Draw(
    Rect sourceRect, Rect destRect, float z, Rect clipRect,
    ColourRect colors, QuadSplitMode quadSplitMode
  ) {
    // get the Rect area that we will actually draw to (i.e. perform clipping)
    Rect finalRect = destRect.GetIntersection(clipRect);

    // check if Rect was totally clipped
    if(finalRect.Width != 0) {
      float xScale = 1.0f / (float)texture.Width;
      float yScale = 1.0f / (float)texture.Height;

      float xTexPerPix = sourceRect.Width / destRect.Width;
      float yTexPerPix = sourceRect.Height / destRect.Height;

      // calculate final, clipped, texture co-ordinates
      Rect texRect = new Rect(
        (sourceRect.Left + ((finalRect.Left - destRect.Left) * xTexPerPix)) * xScale,
        (sourceRect.Top + ((finalRect.Top - destRect.Top) * yTexPerPix)) * yScale,
        (sourceRect.Right + ((finalRect.Right - destRect.Right) * xTexPerPix)) * xScale,
        (sourceRect.Bottom + ((finalRect.Bottom - destRect.Bottom) * yTexPerPix)) * yScale
      );

      // queue a quad to be rendered
      texture.Renderer.AddQuad(finalRect, z, texture, texRect, colors, quadSplitMode);
    }
  }

#if OMITTED_IN_PORT

  /// <summary>Queues an area of the associated Texture the be drawn on the screen</summary>
  /// <remarks>Low-level routine to be used carefully!</remarks>
  /// <param name="sourceRect">
  ///   Rect describing the area of the image file / texture that is to be queued for drawing
  /// </param>
  /// <param name="destRect">
  ///   Rect describing the area of the screen that will be filled with the imagery
  ///   from <paramref name="sourceRect"/>
  /// </param>
  /// <param name="z">
  ///   float value specifying 'z' order.  0 is topmost with increasing values moving back
  ///   into the screen
  /// </param>
  /// <param name="clipRect">
  ///   Rect describing a 'clipping Rect' that will be applied when drawing the requested imagery
  /// </param>
  /// <param name="topLeftColor">
  ///   Colour to be applied to the top left corner of the rendered imagery
  /// </param>
  /// <param name="topRightColor">
  ///   Colour to be applied to the top right corner of the rendered imagery
  /// </param>
  /// <param name="bottomLeftColor">
  ///   Colour to be applied to the bottom left corner of the rendered imagery
  /// </param>
  /// <param name="bottomRightColor">
  ///   Colour to be applied to the bottom right corner of the rendered imagery
  /// </param>
  /// <param name="quadSplitMode">
  ///   One of the QuadSplitMode values specifying the way quads are split into triangles
  /// </param>
  public void Draw(
    Rect sourceRect, Rect destRect, float z, Rect clipRect,
    Colour topLeftColor, Colour topRightColor, Colour bottomLeftColor, Colour bottomRightColor,
    QuadSplitMode quadSplitMode
  );

#endif

  /// <summary>Get/Set flag indicating whether to use auto-scaling for this Imageset</summary>
  /// <value>true to enable auto-scaling, false to disable auto-scaling.</value>
  public bool AutoScaled { // TODO: This is originally named AutoScalingEnabled
    get {
      return autoScale;
    }
    set {
      if(value != autoScale) {
        autoScale = value;
        UpdateImageScalingFactors();
      }
    }
  }

  /// <summary>Get/Set the native resolution for this Imageset</summary>
  /// <value>SizeF describing the new native screen resolution for this Imageset</value>
  public SizeF NativeResolution {
    get {
      return nativeResolution;
    }
    set {
      nativeResolution = value;

      // re-calculate scaling factors and notify images as required 
      NotifyScreenResolution(GuiSystem.Instance.Renderer.Size);
    }
  }

  /// <summary>Notify the Imageset of the current (usually new) display resolution</summary>
  /// <param name="size">Size describing the display resolution</param>
  public void NotifyScreenResolution(SizeF size) {
    horzScaling = size.Width / nativeResolution.Width;
    vertScaling = size.Height / nativeResolution.Height;

    if(autoScale) {
      UpdateImageScalingFactors();
    }
  }

#if PORT_OMITTED

  /// <summary>
  ///   Returns an Imageset::ImageIterator object that can be used to iterate over the
  ///   Image objects in the Imageset
  /// </summary>
  /// <returns>The image iterator</returns>
  public ImageIterator GetIterator();

#endif // PORT_OMITTED

  /// <summary>
  ///   Returns an enumerator that can be used to iterate over the
  ///   Image objects in the Imageset
  /// </summary>
  IEnumerator<Image> IEnumerable<Image>.GetEnumerator() {
    throw new Exception("The method or operation is not implemented.");
  }

  /// <summary>
  ///   Returns an enumerator that can be used to iterate over the
  ///   Image objects in the Imageset
  /// </summary>
  IEnumerator IEnumerable.GetEnumerator() {
    throw new Exception("The method or operation is not implemented.");
  }

  /// <summary>Writes an xml representation of this Imageset</summary>
  /// <param name="writer">Stream to write to</param>
  public void WriteToXml(XmlWriter writer) {
    writer.WriteStartElement("Image");
    writer.WriteAttributeString("Name", name);
    writer.WriteAttributeString("FileName", this.textureFileName);

    if(nativeResolution.Width != DefaultNativeHorzRes) {
      writer.WriteAttributeString("NativeHorzRes", nativeResolution.Width.ToString());
    }
    if(nativeResolution.Height != DefaultNativeVertRes) {
      writer.WriteAttributeString("NativeVertRes", nativeResolution.Height.ToString());
    }
    if(autoScale) {
      writer.WriteAttributeString("AutoScaled", "True");
    }

    foreach(Image image in imageRegistry.Values) {
      image.WriteToXml(writer);
    }
  }

  /// <summary>Default resource group to be used when loading Imageset data</summary>
  public static string DefaultResourceGroup;

  /// <summary>
  ///	Initialize the image set with information taken from the specified file
  /// </summary>
  /// <param name="fileName">The name of the Imageset data file that is to be processed</param>
  /// <param name="resourceGroup">Resource group to be used when loading Imageset data</param>
  protected void Load(string fileName, string resourceGroup) {

    // unload old data and textures
    Unload();

    if(fileName == null || fileName == string.Empty)
      throw new ArgumentException("Invalid Imageset name requested", "fileName");

    try {
      //get the directory from which images and other resources references in the image set are to be qualified relative to
      string directory = Path.GetDirectoryName(fileName);
      // initialize a validating reader with the schema
      XmlReaderSettings settings = new XmlReaderSettings();
      settings.ValidationType = ValidationType.Schema;
      settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
      settings.Schemas.Add(null, SchemaFileName);
      XmlReader reader = XmlReader.Create(fileName, settings);

      // grab the Imageset and Image data
      XmlDocument doc = new XmlDocument();
      doc.Load(reader);

      // grab the Imageset data
      XmlNode imagesetNode = doc.SelectSingleNode("//Imageset");

      name = imagesetNode.Attributes["Name"].Value;
      int horzRes = XmlConvert.ToInt32(imagesetNode.Attributes["NativeHorzRes"].Value);
      int vertRes = XmlConvert.ToInt32(imagesetNode.Attributes["NativeVertRes"].Value);

      this.NativeResolution = new SizeF(horzRes, vertRes);

      this.AutoScaled = XmlConvert.ToBoolean(imagesetNode.Attributes["AutoScaled"].Value);

      string imageFileName = imagesetNode.Attributes["Imagefile"].Value;

      // create the texture for the imageset texture
      texture = GuiSystem.Instance.Renderer.CreateTexture(
        Path.Combine(directory, imageFileName),
        (resourceGroup == string.Empty || resourceGroup == null) ?
           DefaultResourceGroup : resourceGroup
      );

      // load the data for each image defined
      foreach(XmlNode imageNode in imagesetNode.SelectNodes("Image")) {
        string imageName = imageNode.Attributes["Name"].Value;

        Rect Rect = new Rect();
        Rect.Left = XmlConvert.ToSingle(imageNode.Attributes["XPos"].Value);
        Rect.Top = XmlConvert.ToSingle(imageNode.Attributes["YPos"].Value);
        Rect.Width = XmlConvert.ToSingle(imageNode.Attributes["Width"].Value);
        Rect.Height = XmlConvert.ToSingle(imageNode.Attributes["Height"].Value);

        PointF offset = new PointF();
        offset.X = XmlConvert.ToSingle(imageNode.Attributes["XOffset"].Value);
        offset.Y = XmlConvert.ToSingle(imageNode.Attributes["YOffset"].Value);

        // define a new image with the xml data
        DefineImage(imageName, Rect, offset);
      }
    }
    catch(System.Xml.XmlException ex) {
      // unload anything that was created since this failed
      Unload();

      // just rethrow this one
      throw ex;
    }
  }


  /// <summary>
  ///   Unloads all loaded data and leaves the Imageset in a clean (but un-usable) state
  /// </summary>
  /// <remarks>
  ///   This should be called for cleanup purposes only.
  /// </remarks>
  protected void Unload() {
    UndefineAllImages();

    // cleanup texture
    GuiSystem.Instance.Renderer.DestroyTexture(texture);
    texture = null;
  }

  /// <summary>
  ///   Sets the scaling factor for all Images that are a part of this Imageset
  /// </summary>
  protected void UpdateImageScalingFactors() {
    float hScale = 1.0f, vScale = 1.0f;

    if(autoScale) {
      hScale = horzScaling;
      vScale = vertScaling;
    }

    foreach(Image image in imageRegistry.Values) {
      image.SetHorizontalScaling(hScale);
      image.SetVerticalScaling(vScale);
    }
  }

  /// <summary>Name of this image set</summary>
  protected string name;
  /// <summary>List of images defined in this set</summary>
  Dictionary<string, Image> imageRegistry;
  /// <summary>Texture object that handles imagery for this Imageset</summary>
  protected Texture texture;
  /// <summary>The name of the texture file (if any)</summary>
  protected String textureFileName;

  /// <summary>True when auto-scaling is enabled</summary>
  protected bool autoScale;
  /// <summary>Current horizontal scaling factor</summary>
  protected float horzScaling;
  /// <summary>Current vertical scaling factor</summary>
  protected float vertScaling;
  /// <summary>Native horizontal/vertical resolution for this Imageset</summary>
  protected SizeF nativeResolution;

}

} // namespace CeGui
