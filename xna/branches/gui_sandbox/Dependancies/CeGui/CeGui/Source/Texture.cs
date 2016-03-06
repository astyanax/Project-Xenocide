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
using System.IO;
using System.Text;

using System.Drawing;

namespace CeGui {

/// <summary>
///		Abstract base class specifying the required interface for Texture objects.
/// </summary>
/// <remarks>
///		Texture objects are created via the <see cref="Renderer"/>.  The actual inner workings 
///		of any Texture object are dependant upon the Renderer (and underlying API) in use.  
///		This base class defines the minimal set of functions that is required for the rest of the 
///		system to work.  Texture objects are only created through the Renderer object's texture 
///		creation functions.
/// </remarks>
public abstract class Texture {
  #region Fields

  /// <summary>
  ///		<see cref="Renderer"/> object that created and owns this texture.
  /// </summary>
  protected Renderer owner;
  /// <summary>
  ///		Width of this texture (in pixels).
  /// </summary>
  protected int width;
  /// <summary>
  ///		Height of this texture (in pixels).
  /// </summary>
  protected int height;

  #endregion Fields

  #region Constructor

  /// <summary>
  ///		Constructor.
  /// </summary>
  /// <param name="owner"><see cref="Renderer"/> object that created this texture.</param>
  protected Texture(Renderer owner) {
    this.owner = owner;
  }

  #endregion Constructor

  #region Properties

  /// <summary>
  /// Gets the pixel SizeF of this texture
  /// </summary>
  public SizeF Size { get { return new SizeF(width, height); } }

  /// <summary>
  ///		Returns the current pixel height of the texture.
  /// </summary>
  /// <value>Integer representing the height of the texture (in pixels).</value>
  public virtual int Height {
    get {
      return height;
    }
  }

  /// <summary>
  ///		Returns the current pixel width of the texture.
  /// </summary>
  /// <value>Integer representing the width of the texture (in pixels).</value>
  public virtual int Width {
    get {
      return width;
    }
  }

  /// <summary>
  ///		Return a reference to the <see cref="Renderer"/> object that created and owns this texture.
  /// </summary>
  /// <value>Reference to the <see cref="Renderer"/> object that created and owns this texture.</value>
  public Renderer Renderer {
    get {
      return owner;
    }
  }

  #endregion Properties

  #region Abstract Methods

  /// <summary>
  ///		Loads the specified image file into the texture.  
  ///		The texture is resized as required to hold the image.
  /// </summary>
  /// <param name="fileName">The filename of the image file that is to be loaded into the texture.</param>
  public abstract void LoadFromFile(string fileName);

  /// <summary>
  ///		Loads (copies) an image in memory into the texture.  
  ///		The texture is resized as required to hold the image.
  /// </summary>
  /// <param name="buffer">Reference to the stream containing the image data.</param>
  /// <param name="bufferWidth">Width of the buffer.</param>
  /// <param name="bufferHeight">Height of the buffer.</param>
  public abstract void LoadFromMemory(Stream buffer, int bufferWidth, int bufferHeight);

  #endregion Abstract Methods
}

} // namespace CeGui
