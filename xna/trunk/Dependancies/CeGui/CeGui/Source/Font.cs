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
using System.Drawing;
using System.IO;
using System.Text;

namespace CeGui {

/// <summary>Class that encapsulates text rendering functionality for a typeface</summary>
/// <remarks>
///   <para>
///     A Font object is created for each unique typeface required. The Font class provides
///     methods for loading typefaces from various sources, and then for outputting text via
///     the <see cref="Renderer"/> object
///   </para>
///   <para>
///     Based on the font generation code found in the Axiom Engine (http://axiomengine.sf.net)
///   </para>
/// </remarks>
public class Font {

  /// <summary>
  ///   Default color to use for rendering glyphs when no color is specified.
  /// </summary>
  public static readonly Colour DefaultColor = new Colour(1, 1, 1, 1);

  const float InterGlyphPadSpace = 2.0f;

  /// <summary>
  ///   Needed to offset the start of the in memory bitmap to exclude this data.
  /// </summary>
  const int BitmapHeaderSize = 54;

  /// <summary>Constructor which creates a Font from an xml file</summary>
  /// <param name="fileName"></param>
  internal Font(string fileName) {
    throw new NotImplementedException();
  }

  /// <summary>Contructor</summary>
  /// <param name="name">What to call this font</param>
  /// <param name="fontName">The windows FontFamily to use for the font</param>
  /// <param name="size"></param>
  /// <param name="flags"></param>
  /// <param name="firstCodePoint"></param>
  /// <param name="lastCodePoint"></param>
  internal Font(
    string name, string fontName, int size, FontFlags flags,
    char firstCodePoint, char lastCodePoint
  ) {
    // create a blank image set
    glyphImages = ImagesetManager.Instance.CreateImageset(
      name + " auto_glyph_images",
      GuiSystem.Instance.Renderer.CreateTexture()
    );

    int horzDpi = GuiSystem.Instance.Renderer.HorizontalScreenDPI;
    int vertDpi = GuiSystem.Instance.Renderer.VerticalScreenDPI;

    this.name = name;
    this.fontName = fontName;

    CreateFontGlyphSet(firstCodePoint, lastCodePoint, size);
  }

  /// <summary>
  ///		Constructor.
  /// </summary>
  /// <param name="name">What to call this font</param>
  /// <param name="fontName">The windows FontFamily to use for the font</param>
  /// <param name="size"></param>
  /// <param name="flags"></param>
  /// <param name="glyphSet"></param>
  internal Font(string name, string fontName, int size, FontFlags flags, string glyphSet) {
    // create a blank image set
    glyphImages = ImagesetManager.Instance.CreateImageset(
        name + " auto_glyph_images",
        GuiSystem.Instance.Renderer.CreateTexture());

    int horzDpi = GuiSystem.Instance.Renderer.HorizontalScreenDPI;
    int vertDpi = GuiSystem.Instance.Renderer.VerticalScreenDPI;

    this.name = name;

    CreateFontGlyphSet(glyphSet, size);
  }

  /// <summary>
  ///		Constructor.  Creates fonts from ASCII 32-127.
  /// </summary>
  /// <param name="name">What to call this font</param>
  /// <param name="fontName">The windows FontFamily to use for the font</param>
  /// <param name="size"></param>
  /// <param name="flags"></param>
  internal Font(string name, string fontName, int size, FontFlags flags)
    : this(name, fontName, size, flags, (char)32, (char)127) { }

  /// <summary>
  ///		Draw text into a specified area of the display.
  /// </summary>
  /// <param name="text">The text to be drawn.</param>
  /// <param name="drawArea">
  ///		Rect object describing the area of the display where the text is to be rendered.  The text is not clipped to this Rect, but is formatted
  ///		using this Rect depending upon the option specified in <paramref name="format"/>.
  /// </param>
  /// <param name="z">float value specifying the z co-ordinate for the drawn text.</param>
  /// <param name="clipRect">Rect object describing the clipping area for the drawing.  No drawing will occur outside this Rect.</param>
  /// <param name="format">The text formatting required.</param>
  /// <param name="colors">
  ///		ColorRect object describing the colors to be applied when drawing the text.
  ///		The colors specified in here are applied to each glyph, rather than the text as a whole.
  /// </param>
  /// <returns>The number of lines output.  This does not consider clipping, so if all text was clipped, this would still return >=1.</returns>
  public int DrawText(
    string text, Rect drawArea, float z, Rect clipRect,
    HorizontalTextFormat format, ColourRect colors
  ) {
    int thisCount = 0;
    int lineCount = 0;
    int lineStart = 0;
    int lineEnd = 0;
    string currentLine = string.Empty;

    // TODO: Y Bearing
    float baseY = drawArea.Top;

    Rect tmpDrawArea = drawArea;

    while(lineEnd < text.Length) {
      if((lineEnd = text.IndexOf('\n', lineStart)) == -1) {
        lineEnd = text.Length;
      }

      currentLine = text.Substring(lineStart, lineEnd - lineStart);
      // +1 to skip \n char
      lineStart = lineEnd + 1;

      // TODO: Complete formats
      switch(format) {
        case HorizontalTextFormat.Left:
          DrawTextLine(currentLine, new Vector3(tmpDrawArea.Left, baseY, z), clipRect, colors);
          thisCount = 1;
          baseY += LineSpacing;
          break;

        case HorizontalTextFormat.Right:
          DrawTextLine(
              currentLine,
              new Vector3(tmpDrawArea.Right - GetTextExtent(currentLine), baseY, z),
              clipRect, colors);

          thisCount = 1;
          baseY += LineSpacing;
          break;

        case HorizontalTextFormat.Center:
          DrawTextLine(
              currentLine,
              new Vector3(drawArea.Left + ((tmpDrawArea.Width - GetTextExtent(currentLine)) * 0.5f), baseY, z),
              clipRect, colors);

          thisCount = 1;
          baseY += LineSpacing;
          break;

        case HorizontalTextFormat.WordWrapLeft:
          thisCount = DrawWrappedText(currentLine, tmpDrawArea, z, clipRect, HorizontalTextFormat.Left, colors);
          tmpDrawArea.Top += thisCount * LineSpacing;
          break;

        case HorizontalTextFormat.WordWrapCentered:
          thisCount = DrawWrappedText(currentLine, tmpDrawArea, z, clipRect, HorizontalTextFormat.Center, colors);
          tmpDrawArea.Top += thisCount * LineSpacing;
          break;

        case HorizontalTextFormat.WordWrapRight:
          thisCount = DrawWrappedText(currentLine, tmpDrawArea, z, clipRect, HorizontalTextFormat.Right, colors);
          tmpDrawArea.Top += thisCount * LineSpacing;
          break;

        default:
          throw new InvalidRequestException("Unknown text format option '{0}'", format);
      }

      lineCount = thisCount;
    }

    return lineCount;
  }

  /// <summary>
  ///		Draw text into a specified area of the display using the default colors and default text format (Left aligned).
  /// </summary>
  /// <param name="text">The text to be drawn.</param>
  /// <param name="drawArea">
  ///		Rect object describing the area of the display where the text is to be rendered.  The text is not clipped to this Rect, but is formatted
  ///		using this Rect depending upon the option specified in <paramref name="format"/>.
  /// </param>
  /// <param name="z">float value specifying the z co-ordinate for the drawn text.</param>
  /// <param name="clipRect">Rect object describing the clipping area for the drawing.  No drawing will occur outside this Rect.</param>
  /// <param name="format">The text formatting required.</param>
  /// <returns>The number of lines output.  This does not consider clipping, so if all text was clipped, this would still return >=1.</returns>
  public int DrawText(string text, Rect drawArea, float z, Rect clipRect, HorizontalTextFormat format) {
    return DrawText(text, drawArea, z, clipRect, format, new ColourRect(DefaultColor, DefaultColor, DefaultColor, DefaultColor));
  }

  /// <summary>
  ///		Draw text into a specified area of the display using the default colors.
  /// </summary>
  /// <param name="text">The text to be drawn.</param>
  /// <param name="drawArea">
  ///		Rect object describing the area of the display where the text is to be rendered.  The text is not clipped to this Rect, but is formatted
  ///		using this Rect depending upon the option specified in <paramref name="format"/>.
  /// </param>
  /// <param name="z">float value specifying the z co-ordinate for the drawn text.</param>
  /// <param name="clipRect">Rect object describing the clipping area for the drawing.  No drawing will occur outside this Rect.</param>
  public void DrawText(string text, Rect drawArea, float z, Rect clipRect) {
    DrawText(text, drawArea, z, clipRect, HorizontalTextFormat.Left, new ColourRect(DefaultColor, DefaultColor, DefaultColor, DefaultColor));
  }

  /// <summary>
  ///		Draw text into a specified area of the display using the default colors.
  /// </summary>
  /// <param name="text">The text to be drawn.</param>
  /// <param name="drawArea">
  ///		Rect object describing the area of the display where the text is to be rendered.  The text is not clipped to this Rect, but is formatted
  ///		using this Rect depending upon the option specified in <paramref name="format"/>.
  /// </param>
  /// <param name="z">float value specifying the z co-ordinate for the drawn text.</param>
  /// <param name="format">The text formatting required.</param>
  /// <param name="colors">
  ///		ColorRect object describing the colors to be applied when drawing the text.
  ///		The colors specified in here are applied to each glyph, rather than the text as a whole.
  /// </param>
  /// <returns>The number of lines output.  This does not consider clipping, so if all text was clipped, this would still return >=1.</returns>
  public int DrawText(string text, Rect drawArea, float z, HorizontalTextFormat format, ColourRect colors) {
    return DrawText(text, drawArea, z, drawArea, format, colors);
  }

  /// <summary>
  ///		Draw text into a specified area of the display using the default colors.
  /// </summary>
  /// <param name="text">The text to be drawn.</param>
  /// <param name="drawArea">
  ///		Rect object describing the area of the display where the text is to be rendered.  The text is not clipped to this Rect, but is formatted
  ///		using this Rect depending upon the option specified in <paramref name="format"/>.
  /// </param>
  /// <param name="z">float value specifying the z co-ordinate for the drawn text.</param>
  /// <param name="format">The text formatting required.</param>
  /// <returns>The number of lines output.  This does not consider clipping, so if all text was clipped, this would still return >=1.</returns>
  public int DrawText(string text, Rect drawArea, float z, HorizontalTextFormat format) {
    return DrawText(text, drawArea, z, drawArea, format, new ColourRect(DefaultColor, DefaultColor, DefaultColor, DefaultColor));
  }

  /// <summary>
  ///		Draw text into a specified area of the display using the default colors.
  /// </summary>
  /// <param name="text">The text to be drawn.</param>
  /// <param name="drawArea">
  ///		Rect object describing the area of the display where the text is to be rendered.  The text is not clipped to this Rect, but is formatted
  ///		using this Rect depending upon the option specified in <paramref name="format"/>.
  /// </param>
  /// <param name="z">float value specifying the z co-ordinate for the drawn text.</param>
  public void DrawText(string text, Rect drawArea, float z) {
    DrawText(text, drawArea, z, drawArea, HorizontalTextFormat.Left, new ColourRect(DefaultColor, DefaultColor, DefaultColor, DefaultColor));
  }

  /// <summary>
  ///		Draw text at the specified location.
  /// </summary>
  /// <param name="text">Text to be drawn.</param>
  /// <param name="position">
  ///		Vector3 object describing the location for the text.  NB: The position specified here corresponds to the text baseline and not the
  ///		top of any glyph.  The baseline spacing required can be retrieved by checking the <see cref="Baseline"/> property.
  /// </param>
  /// <param name="clipRect">Rect describing the clipping area for the drawing.  No drawing will occur outside this Rect.</param>
  /// <param name="colors">
  ///		ColorRect describing the colors to be applied when drawing the text.  
  ///		The colors specified in here are applied to each glyph, rather than the text as a whole.
  /// </param>
  public void DrawText(string text, Vector3 position, Rect clipRect, ColourRect colors) {
    DrawText(text, new Rect(position.x, position.y, position.x, position.y), position.z, clipRect, HorizontalTextFormat.Left, colors);
  }

  /// <summary>
  ///		Draw text at the specified location, using the default colors.
  /// </summary>
  /// <param name="text">Text to be drawn.</param>
  /// <param name="position">
  ///		Vector3 object describing the location for the text.  NB: The position specified here corresponds to the text baseline and not the
  ///		top of any glyph.  The baseline spacing required can be retrieved by checking the <see cref="Baseline"/> property.
  /// </param>
  /// <param name="clipRect">Rect describing the clipping area for the drawing.  No drawing will occur outside this Rect.</param>
  public void DrawText(string text, Vector3 position, Rect clipRect) {
    DrawText(text, new Rect(position.x, position.y, position.x, position.y), position.z, clipRect, HorizontalTextFormat.Left, new ColourRect(DefaultColor, DefaultColor, DefaultColor, DefaultColor));
  }

  /// <summary>
  ///		
  /// </summary>
  /// <param name="text"></param>
  /// <param name="position"></param>
  /// <param name="clipRect"></param>
  /// <param name="colors"></param>
  public void DrawTextLine(string text, Vector3 position, Rect clipRect, ColourRect colors) {
    Vector3 currentPos = position;

    for(int i = 0; i < text.Length; i++) {
      char c = text[i];

      int index = glyphs.IndexOf(c);

      if(index != -1) {
        Image glyphImage = glyphData[index].Image;

        glyphImage.Draw(currentPos, clipRect, colors);
        currentPos.x += glyphData[index].HorizontalAdvance;
      }
    }
  }

  /// <summary>
  ///		Draws wrapped text, returning the final line count.
  /// </summary>
  /// <param name="text"></param>
  /// <param name="drawArea"></param>
  /// <param name="z"></param>
  /// <param name="clipRect"></param>
  /// <param name="format"></param>
  /// <param name="colors"></param>
  /// <returns>The number of lines in the formatted text.</returns>
  public int DrawWrappedText(string text, Rect drawArea, float z, Rect clipRect, HorizontalTextFormat format, ColourRect colors) {
    int lineCount = 0;
    Rect destRect = drawArea;
    float wrapWidth = drawArea.Width;

    string whitespace = TextUtil.DefaultWhitespace;
    string thisLine, thisWord;
    int currentPos = 0;

    // get the first word of the string
    thisLine = TextUtil.GetNextWord(text, currentPos);
    currentPos += thisLine.Length;

    // while there are words left in the string
    while(TextUtil.IndexNotOf(text, whitespace, currentPos) != -1) {
      // get the next word of the string
      thisWord = TextUtil.GetNextWord(text, currentPos);
      currentPos += thisWord.Length;

      // if the new word would make the string too long
      if((GetTextExtent(thisLine) + GetTextExtent(thisWord)) > wrapWidth) {
        // output what we had until this new word
        lineCount += DrawText(thisLine, destRect, z, clipRect, format, colors);

        // remove whitespace from next word - it will form start of next line
        thisWord = thisWord.Substring(TextUtil.IndexNotOf(thisWord, whitespace, 0));

        // reset for a new line
        thisLine = string.Empty;

        // update y coordinate for the next line
        destRect.Top += LineSpacing;
      }

      // add the next word to the line
      thisLine += thisWord;
    }

    // output last bit of string
    lineCount += DrawText(thisLine, destRect, z, clipRect, format, colors);

    return lineCount;
  }

  /// <summary>
  ///		
  /// </summary>
  /// <param name="text"></param>
  /// <param name="startChar"></param>
  /// <param name="pixel"></param>
  /// <returns></returns>
  public int GetCharAtPixel(string text, int startChar, float pixel) {
    int curExtent = 0;
    int charCount = text.Length;

    // handle simple cases
    if((pixel <= 0) || (charCount <= startChar)) {
      return startChar;
    }

    for(int c = startChar; c < charCount; c++) {
      if(glyphs.IndexOf(text[c]) != -1) {
        curExtent += (int)glyphData[c].HorizontalAdvance;

        if(pixel < curExtent) {
          return c;
        }
      }
    }

    return charCount;
  }

  /// <summary>
  ///		Return the number of lines the given text would be formatted to.
  /// </summary>
  /// <remarks>
  ///		Since text formatting can result in multiple lines of text being output, it can be useful to know
  ///		how many lines would be output without actually rendering the text.
  /// </remarks>
  /// <param name="text">The text to be measured.</param>
  /// <param name="formatArea">
  ///		Rect describing the area to be used when formatting the text depending upon the option specified 
  ///		in <paramref name="format"/>.</param>
  /// <param name="format">Formatting to consider for the line count.</param>
  /// <returns>The number of lines produced from the specified formatting.</returns>
  public int GetFormattedLineCount(string text, Rect formatArea, HorizontalTextFormat format) {
    // handle simple non-wrapped cases
    if(format == HorizontalTextFormat.Left ||
        format == HorizontalTextFormat.Center ||
        format == HorizontalTextFormat.Right) {

      int count = 0;

      // find the number of line breaks
      for(int i = 0; i < text.Length; i++) {
        if(text[i] == '\n') {
          count++;
        }
      }

      return count;
    }

    // wrapping cases
    int lineStart = 0, lineEnd = 0;
    string sourceLine;
    float wrapWidth = formatArea.Width;
    string whitespace = TextUtil.DefaultWhitespace;
    string thisLine, thisWord;
    int lineCount = 0, currentPos = 0;

    while(lineEnd < text.Length) {
      if((lineEnd = text.IndexOf("\n", lineStart)) == -1) {
        lineEnd = text.Length;
      }

      sourceLine = text.Substring(lineStart, lineEnd - lineStart);
      lineStart = lineEnd + 1;

      // get the first word
      thisLine = TextUtil.GetNextWord(sourceLine, 0);
      currentPos += thisLine.Length;

      // while there are words left in the string
      while(TextUtil.IndexNotOf(sourceLine, whitespace, currentPos) != -1) {
        // get the next word of the string
        thisWord = TextUtil.GetNextWord(sourceLine, currentPos);
        currentPos += thisWord.Length;

        if((GetTextExtent(thisLine) + GetTextExtent(thisWord) > wrapWidth)) {
          // too long, so that is another line of text
          lineCount++;

          // remove whitespace from next word - it will form start of next line
          thisWord = thisWord.Substring(TextUtil.IndexNotOf(thisWord, whitespace, 0));

          // reset for a new line
          thisLine = string.Empty;
        }

        // add the next word to the line
        thisLine += thisWord;
      }

      // plus one for final line
      lineCount++;
    }

    return lineCount;
  }

  /// <summary>
  ///		Return whether this Font can currently draw the specified code-point.
  /// </summary>
  /// <param name="c">Code point that is the subject of the query.</param>
  /// <returns>True if this font contains a mapping for code point 'c', false otherwise.</returns>
  public bool IsCharacterAvailable(char c) {
    return glyphs.IndexOf(c) != -1;
  }

  /// <summary>
  ///		Creates a font glyph set from all the characters in the given string.
  /// </summary>
  /// <param name="glyphs">String containing all the characters to use for the glyph set.</param>
  /// <param name="size">SizeF of the font.</param>
  protected void CreateFontGlyphSet(string glyphs, int size) {
    this.glyphs = glyphs;

    glyphData = new GlyphData[glyphs.Length];

    // TODO: Determine required SizeF
    int bitmapHeight = 512;
    int bitmapWidth = 512;

    Bitmap bitmap = new Bitmap(bitmapWidth, bitmapHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

    // get a handles to the graphics context of the bitmap
    Graphics g = Graphics.FromImage(bitmap);

    // get a font object for the specified font
    System.Drawing.Font font = new System.Drawing.Font(fontName, size);

    // clear the image to transparent
    g.Clear(System.Drawing.Color.Transparent);

    // these fonts better look good!
    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

    // used for calculating position in the image for rendering the characters
    float x = 0, y = 0;

    StringFormat format = (StringFormat)StringFormat.GenericTypographic.Clone();
    format.FormatFlags = StringFormatFlags.MeasureTrailingSpaces;

    // TODO: Magic numbers, ah!
    float height = font.FontFamily.GetLineSpacing(FontStyle.Regular) / 64;
    ySpacing = font.FontFamily.GetEmHeight(FontStyle.Regular) / 96;

    // loop through each character in the glyph string and draw it to the bitmap
    for(int i = 0; i < glyphs.Length; i++) {
      char c = glyphs[i];

      // measure the width and height of 'A' for reference
      SizeF metrics = g.MeasureString(c.ToString(), font, 1024, format);

      // TODO: Magic number
      float width = (int)Math.Ceiling(metrics.Width + 1.7f);

      // are we gonna wrap?
      if(x + width > bitmapWidth) {
        // increment the y coord and reset x to move to the beginning of next line
        y += height;
        x = 0;
      }

      // draw the character
      g.DrawString(c.ToString(), font, Brushes.White, x, y, StringFormat.GenericTypographic);

      Rect Rect = new Rect();

      // calculate the texture coords for the character
      Rect.Left = x;
      Rect.Right = x + width;
      Rect.Top = y;
      Rect.Bottom = y + height;
      // This made ClearType look better, but it makes normal Antialiased Text look worse :-p
      /*
      // GDI+ does not generate a proper alpha map, so create a proper one
      for(int ix = 0; ix < width; ix++) {
        for(int iy = 0; iy < height; iy++) {
          System.Drawing.Color color = bitmap.GetPixel((int)x + ix, (int)y + iy);

          if(color.A > 0) {
            color = System.Drawing.Color.FromArgb(
              ((int)color.R + (int)color.G + (int)color.B) / 3, color
            );
            
            bitmap.SetPixel((int)x + ix, (int)y + iy, color);
          }
        }
      }
      */

      // TODO: Proper offset
      glyphImages.DefineImage(c.ToString(), Rect, new PointF(0, 0));

      glyphData[i].Image = glyphImages.GetImage(c.ToString());
      // TODO: Hack for testing
      glyphData[i].HorizontalAdvance = width * .95f;
      glyphData[i].Glyph = c;

      // increment X by the width of the current char
      x += (width + InterGlyphPadSpace);
    }

    // flip the image
    //bitmap.RotateFlip(System.Drawing.RotateFlipType.RotateNoneFlipY);

    // save the image to a memory stream
    MemoryStream stream = new MemoryStream();
    bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp); // Bmp

    // destroy the bitmap
    bitmap.Dispose();

    // offset from the beginning to skip the header info
    //stream.Position = BitmapHeaderSize;
    stream.Position = 0; // MDX 2.0 needs this header

    // load the image from memory
    glyphImages.Texture.LoadFromMemory(stream, bitmapWidth, bitmapHeight);
  }

  /// <summary>
  ///		Creates a font glyph set for the given range of characters.
  /// </summary>
  /// <param name="firstCodePoint">Starting character.</param>
  /// <param name="lastCodePoint">Ending character.</param>
  /// <param name="SizeF">SizeF of the font.</param>
  protected void CreateFontGlyphSet(char firstCodePoint, char lastCodePoint, int SizeF) {
    StringBuilder glyphBuilder = new StringBuilder();

    // build a string from the range of characters
    for(int i = firstCodePoint; i <= lastCodePoint; i++) {
      char c = (char)i;

      glyphBuilder.Append(c);
    }

    // create the glyph set using the string built from the glyph range
    CreateFontGlyphSet(glyphBuilder.ToString(), SizeF);
  }

  /// <summary>
  ///		Gets the width in pixels of the specified text.
  /// </summary>
  /// <param name="text">Text to measure.</param>
  /// <returns>Width of the specified string for this font.</returns>
  public float GetTextExtent(string text) {
    float extent = 0;

    // loop through each character and total up the horizontal space
    for(int i = 0; i < text.Length; i++) {
      int index = glyphs.IndexOf(text[i]);
      if (index != -1)
      {
          extent += glyphData[index].HorizontalAdvance;
      }
    }

    return extent;
  }

  /// <summary>
  ///		Load and complete construction of 'this' via an XML file.
  /// </summary>
  /// <param name="fileName">The name of the XML file that holds details about the font to create.</param>
  protected void Load(string fileName) {
    throw new NotImplementedException();
  }

  /// <summary>
  /// Return the number of pixels from the top of the highest glyph to the baseline
  /// </summary>
  public float Baseline {
    get { throw new NotImplementedException(); }
  }

  /// <summary>
  ///		Return the pixel height for this Font.
  /// </summary>
  /// <value>
  ///		Number of pixels between vertical base lines, 
  ///		i.e. The minimum pixel space between two lines of text.
  ///	</value>
  public float LineSpacing {
    get {
      return ySpacing;
    }
  }

  /// <summary>
  ///		Gets the name of this font object.
  /// </summary>
  public string Name {
    get {
      return name;
    }
  }

  /// <summary>
  ///		Struct for hold per-glyph data.
  /// </summary>
  public struct GlyphData {
    /// <summary>
    /// The bitmap holding an image of the glyph
    /// </summary>
    public Image Image;

    /// <summary>
    /// Width of the glyph
    /// </summary>
    public float HorizontalAdvance;

    /// <summary>
    /// Currently unused
    /// </summary>
    public float HorizontalAdvanceUnscaled;

    /// <summary>
    /// Unicode value of the Glyph
    /// </summary>
    public char Glyph;
  }

  /// <summary>
  ///		Returns a string representation of this Font.
  /// </summary>
  /// <returns>String representation of the Font.</returns>
  public override string ToString() {
    return Name;
  }

  /// <summary>
  ///		Parses the String representation of a Font, and returns the corresponding
  ///		Font object.
  /// </summary>
  /// <param name="data">String representation of a Font.</param>
  /// <returns>Font object that matches the string representation.</returns>
  public static Font Parse(string data) {
    return FontManager.Instance.GetFont(data);
  }

  /// <summary>
  ///		Name of this font.
  /// </summary>
  protected string name;

  /// <summary>
  /// The windows FontFamily to use for the font
  /// </summary>
  protected string fontName;
  /// <summary>
  ///		Imageset that holds the glyphs for this font.
  /// </summary>
  protected Imageset glyphImages;

  /// <summary>
  ///		Point SizeF of the font.
  /// </summary>
  protected int pointSize;
  /// <summary>
  ///		True if this font is created from a true type font.
  /// </summary>
  protected bool trueType;
  /// <summary>
  ///		Height of font in pixels, a.k.a Line spacing.
  /// </summary>
  protected float ySpacing;
  /// <summary>
  ///		Maximum bearingY value (gives required spacing down to baseline).
  /// </summary>
  protected int maxBearingY;

  /// <summary>
  ///		true when auto-scaling is enabled.
  /// </summary>
  protected bool autoScale;
  /// <summary>
  ///		Current horizontal scaling factor.
  /// </summary>
  protected float horzScaling;
  /// <summary>
  ///		Current vertical scaling factor.
  /// </summary>
  protected float vertScaling;
  /// <summary>
  ///		Native horizontal resolution for this Imageset.
  /// </summary>
  protected float nativeHorzRes;
  /// <summary>
  ///		Native vertical resolution for this Imageset.
  /// </summary>
  protected float verticalHorzRes;

  /// <summary>
  /// Information on each of the glyphs in the font
  /// </summary>
  protected GlyphData[] glyphData;
  
  /// <summary>
  /// The characters in the font
  /// </summary>
  protected string glyphs;

}

} // namespace CeGui
