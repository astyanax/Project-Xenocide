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

namespace CeGui {

/// <summary>
///		Class providing a shared library of Font objects to the system.
/// </summary>
/// <remarks>
///		The FontManager is used to create, access, and destroy Font objects.  The idea is that the
///		FontManager will function as a central repository for Font objects used within the GUI system,
///		and that those Font objects can be accessed, via a unique name, by any interested party within
///		the system.
/// </remarks>
public class FontManager {
  #region Singleton Implementation

  /// <summary>
  ///		Singlton instance of this class.
  /// </summary>
  private static FontManager instance;

  /// <summary>
  ///		Default constructor.
  /// </summary>
  internal FontManager() {
    // only create once instance
    if(instance == null) {
      instance = this;

      Logger.Instance.LogEvent("FontManager singleton created.");
    }
  }

  /// <summary>
  ///		Gets the singleton class instance.
  /// </summary>
  /// <value></value>
  public static FontManager Instance {
    get {
      return instance;
    }
  }

  #endregion Singleton Implementation

  #region Fields

  /// <summary>
  ///		List of fonts created within the system.
  /// </summary>
  protected FontList fontList = new FontList();

  #endregion Fields

  #region Methods

  /// <summary>
  ///   Creates a new Font based on a true-type font, and returns a
  ///   pointer to the new Font object.
  /// </summary>
  /// <param name="name">A unique name for the new font.</param>
  /// <param name="fontName">The name of the true-type font to use.</param>
  /// <param name="SizeF">The glyph SizeF (point-SizeF) for the new font.</param>
  /// <param name="flags">Additional flags to alter the creation of the font bitmap.</param>
  /// <returns>A reference to the newly created Font object.</returns>
  /// <exception cref="AlreadyExistsException">
  ///   Thrown if the font named <paramref name="name"/> already exists in the system.
  /// </exception>
  public Font CreateFont(string name, string fontName, int SizeF, FontFlags flags) {
    // first ensure name uniqueness
    if(IsFontPresent(name)) {
      throw new AlreadyExistsException("Font '{0}' already exists in the system.", name);
    }

    Font font = new Font(name, fontName, SizeF, flags);
    fontList.Add(font);

    Logger.Instance.LogEvent(string.Format("Font '{0}' has been created based on font '{1}' with a point SizeF of {2}", name, fontName, SizeF));

    return font;
  }

  /// <summary>
  ///		Returns the Font object with the specified name.
  /// </summary>
  /// <param name="name">Name of the font object to return.</param>
  /// <returns>A Font object with the specified name.</returns>
  public Font GetFont(string name) {
    if(!IsFontPresent(name)) {
      throw new UnknownObjectException("A Font with the specified name '{0}' does not exist in the system.", name);
    }

    return fontList[name];
  }

  /// <summary>
  ///		Checks the existence of a given font.
  /// </summary>
  /// <param name="name">The name of the Font object to look for.</param>
  /// <returns>true if a Font object named <paramref name="name"/> exists in the system, false if no such font exists.</returns>
  public bool IsFontPresent(string name) {
    return fontList[name] != null;
  }

  #endregion Methods
}
  
} // namespace CeGui
