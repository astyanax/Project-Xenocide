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
using System.Drawing;

namespace CeGui {

/// <summary>
/// Class that acts as a cache for images that need to be rendered.
///
/// This is in many ways an optimisation cache, it allows a full image redraw
/// to occur while limiting the amount of information that needs to be re-calculated.
/// <para>Basically, unless the actual images (or their SizeF) change, or the colours (or alpha) change
/// then imagery cached in here will suffice for a full redraw.  The reasoning behind this is that
/// when some window underneath window 'X' changes, a full image redraw is required by the
/// renderer, however, since window 'X' is unchanged, performing a total recalculation of all
/// imagery is very wasteful, and so we use this cache to limit such waste.</para>
/// <para>As another example, when a window is simply moved, there is no need to perform a total imagery
/// recalculation; we can still use the imagery cached here since it is position independant.</para>
/// </summary>
public class RenderCache {
  #region Private helper structs
  /// <summary>
  /// internal struct that holds info about a single image to be drawn.
  /// </summary>
  internal struct ImageInfo {
    public Image source_image;
    public Rect target_area;
    public float zOffset;
    public ColourRect colors;
    public Rect customClipper;
    public bool usingCustomClipper;
    public bool clipToDisplay;
  }

  /// <summary>
  /// internal struct that holds info about text to be drawn.
  /// </summary>
  internal struct TextInfo {
    public string text;
    public Font source_font;
    public TextFormatting formatting;
    public Rect target_area;
    public float zOffset;
    public ColourRect colors;
    public Rect customClipper;
    public bool usingCustomClipper;
    public bool clipToDisplay;
  }
  #endregion Private helper structs

  #region Fields


  #region ImageInfoList Collection
  public class ImageInfoList : CollectionBase, ICollection {
    public ImageInfoList()
      : base() {
    }

    public ImageInfoList(ImageInfoList images) {
      this.Add(images);
    }

    void ICollection.CopyTo(Array array, int index) {
      this.List.CopyTo(array, index);
    }

    internal ImageInfoList(ImageInfo image) {
      this.Add(image);
    }

    internal ImageInfo this[int index] {
      get {
        return ((ImageInfo)List[index]);
      }
      set {
        List[index] = value;
      }
    }

    internal int Add(ImageInfo image) {
      return (List.Add(image));
    }

    public int Add(ImageInfoList imageInfoCollection) {
      for(int i = 0; i < imageInfoCollection.Count; i++)
        this.List.Add(imageInfoCollection[i]);
      return this.Count;
    }

    internal void Insert(int index, ImageInfo info) {
      List.Insert(index, info);
    }

    internal void Remove(ImageInfo info) {
      List.Remove(info);
    }

    internal int IndexOf(ImageInfo info) {
      return (List.IndexOf(info));
    }

    internal bool Contains(ImageInfo info) {
      return (List.Contains(info));
    }

    internal virtual void CopyTo(ImageInfo[] array, int index) {
      ((ICollection)this).CopyTo(array, index);
    }

    public virtual void CopyTo(Array array, int index) {
      this.List.CopyTo(array, index);
    }
  }

  #endregion ImageInfo List
  #region TextInfoList Collection
  public class TextInfoList : CollectionBase, ICollection {
    public TextInfoList()
      : base() {
    }

    public TextInfoList(TextInfoList list) {
      this.Add(list);
    }

    void ICollection.CopyTo(Array array, int index) {
      this.List.CopyTo(array, index);
    }

    internal TextInfoList(TextInfo text) {
      this.Add(text);
    }

    internal TextInfo this[int index] {
      get {
        return ((TextInfo)List[index]);
      }
      set {
        List[index] = value;
      }
    }

    internal int Add(TextInfo text) {
      return (List.Add(text));
    }

    public int Add(TextInfoList textInfoCollection) {
      for(int i = 0; i < textInfoCollection.Count; i++)
        this.List.Add(textInfoCollection[i]);
      return this.Count;
    }

    internal void Insert(int index, TextInfo info) {
      List.Insert(index, info);
    }

    internal void Remove(TextInfo info) {
      List.Remove(info);
    }

    internal int IndexOf(TextInfo info) {
      return (List.IndexOf(info));
    }

    internal bool Contains(TextInfo info) {
      return (List.Contains(info));
    }

    internal virtual void CopyTo(TextInfo[] array, int index) {
      ((ICollection)this).CopyTo(array, index);
    }

    public virtual void CopyTo(Array array, int index) {
      this.List.CopyTo(array, index);
    }
  }

  #endregion TextInfo List

  /// <summary>
  /// Collection of ImageInfo structs.
  /// </summary>
  private ImageInfoList imageryList;
  /// <summary>
  /// Collection of TextInfo structs.
  /// </summary>
  private TextInfoList textList;
  #endregion Fields

  #region Constructors
  /// <summary>
  /// Constructor
  /// </summary>
  public RenderCache() {
    imageryList = new ImageInfoList();
    textList = new TextInfoList();
  }
  #endregion Constructors

  #region Properties
  /// <summary>
  /// Return whether the cache contains anything to draw.
  /// </summary>
  /// <value>
  /// - true if the cache contains information about images to be drawn.
  /// - false if the cache is empty.
  /// </value>
  public bool HasCachedImagery {
    get {
      return (imageryList.Count > 0) || (textList.Count > 0);
    }
  }
  #endregion

  #region Methods
  /// <summary>
  /// Send the contents of the cache to the Renderer.
  /// </summary>
  /// <param name="basePos">Point that describes a screen offset that cached imagery will be rendered relative to.</param>
  /// <param name="baseZ">Z value that cached imagery will use as a base figure when calculating final z values.</param>
  /// <param name="clipper">Rect object describing a Rect to which imagery will be clipped.</param>
  public void Render(PointF basePos, float baseZ, Rect clipper) {
    Rect displayArea = GuiSystem.Instance.Renderer.Rect;
    Rect custClipper = new Rect();
    Rect finalClipper;
    Rect finalRect;

    // Send all cached images to renderer.
    foreach(ImageInfo image in imageryList) {
      if(image.usingCustomClipper) {
        custClipper.Offset(basePos);
        custClipper = (image.clipToDisplay) ? displayArea.GetIntersection(custClipper) : clipper.GetIntersection(custClipper);
        finalClipper = custClipper;
      } else {
        finalClipper = image.clipToDisplay ? displayArea : clipper;
      }

      finalRect = image.target_area;
      finalRect.Offset(basePos);
      image.source_image.Draw(finalRect, baseZ + image.zOffset, finalClipper, image.colors);
    }

    // send all cached texts to renderer.
    foreach(TextInfo text in textList) {
      if(text.usingCustomClipper) {
        custClipper = text.customClipper;
        custClipper.Offset(basePos);
        custClipper = (text.clipToDisplay) ? displayArea.GetIntersection(custClipper) : clipper.GetIntersection(custClipper);
        finalClipper = custClipper;
      } else {
        finalClipper = text.clipToDisplay ? displayArea : clipper;
      }

      finalRect = text.target_area;
      finalRect.Offset(basePos);
      throw new NotImplementedException();
      //text.source_font.DrawText (text.text, finalRect, baseZ + text.zOffset, finalClipper, text.formatting, text.colors);
    }
  }

  /// <summary>
  /// Erase any stored image information.
  /// </summary>
  public void ClearCachedImagery() {
    imageryList.Clear();
    textList.Clear();
  }

  /// <summary>
  /// Add an image to the cache.
  /// </summary>
  /// <param name="image">Image object to be cached.</param>
  /// <param name="destArea">
  /// Destination area over which the Image object will be rendered.  This area should be position
  /// independant; so position (0,0) will be to top-left corner of whatever it is you're rendering
  /// (like a Window for example).
  /// </param>
  /// <param name="zOffset">Zero based z offset for this image.  Allows imagery to be layered.</param>
  /// <param name="colors">ColourRect object describing the colours to be applied when rendering this image.</param>
  /// <param name="clipper"></param>
  /// <param name="clipToDisplay"></param>
  public void CacheImage(Image image, Rect destArea, float zOffset, ColourRect colors, Rect clipper, bool clipToDisplay) {
    ImageInfo imageInf = new ImageInfo();
    imageInf.source_image = image;
    imageInf.target_area = destArea;
    imageInf.zOffset = zOffset;
    imageInf.colors = colors;
    imageInf.clipToDisplay = clipToDisplay;

    if(clipper.Equals(Rect.Empty)) {
      imageInf.customClipper = clipper;
      imageInf.usingCustomClipper = true;
    } else {
      imageInf.usingCustomClipper = false;
    }

    imageryList.Add(imageInf);
  }
  public void CacheImage(Image image, Rect destArea, float zOffset, ColourRect colors, Rect clipper) {
    CacheImage(image, destArea, zOffset, colors, clipper, false);
  }
  public void CacheImage(Image image, Rect destArea, float zOffset, ColourRect colors) {
    CacheImage(image, destArea, zOffset, colors, Rect.Empty, false);
  }

  /// <summary>
  /// Add a text to the cache.
  /// </summary>
  /// <param name="text">String object to be cached.</param>
  /// <param name="font">Font to be used when rendering.</param>
  /// <param name="format">TextFormatting value specifying the formatting to use when rendering.</param>
  /// <param name="destArea">
  /// Destination area over which the Image object will be rendered.  This area should be position
  /// independant; so position (0,0) will be to top-left corner of whatever it is you're rendering
  /// (like a Window for example).
  /// </param>
  /// <param name="zOffset">Zero based z offset for this image.  Allows imagery to be layered.</param>
  /// <param name="cols">ColorRect object describing the colours to be applied when rendering this image.</param>
  /// <param name="clipper"></param>
  /// <param name="clipToDisplay"></param>
  void CacheText(String text, Font font, TextFormatting format, Rect destArea, float zOffset, ColourRect cols, Rect clipper, bool clipToDisplay) {
    TextInfo textInf = new TextInfo();
    textInf.source_font = font;
    textInf.text = text;
    textInf.target_area = destArea;
    textInf.formatting = format;
    textInf.colors = cols;
    textInf.clipToDisplay = clipToDisplay;
    textInf.zOffset = zOffset;

    if(clipper.Equals(Rect.Empty)) {
      textInf.customClipper = clipper;
      textInf.usingCustomClipper = true;
    } else {
      textInf.usingCustomClipper = false;
    }

    textList.Add(textInf);
  }
  void CacheText(String text, Font font, TextFormatting format, Rect destArea, float zOffset, ColourRect cols, Rect clipper) {
    CacheText(text, font, format, destArea, zOffset, cols, clipper, false);
  }
  void CacheText(String text, Font font, TextFormatting format, Rect destArea, float zOffset, ColourRect cols) {
    CacheText(text, font, format, destArea, zOffset, cols, Rect.Empty, false);
  }
  #endregion Methods
}

} // namespace CeGui
