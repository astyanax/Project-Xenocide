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
using System.IO;
using System.Drawing;

namespace CeGui {

/// <summary>
///		Class providing a shared library of Imageset objects to the system.
/// </summary>
/// <remarks>
///		The ImagesetManager is used to create, access, and destroy <see cref="Imageset"/> objects.  The idea is that
///		the ImagesetManager will function as a central repository for imagery used within the GUI system,
///		and that such imagery can be accessed, via a unique name, by any interested party within the system.
/// </remarks>
public class ImagesetManager {
  #region Singleton Implementation

  /// <summary>
  ///		Singlton instance of this class.
  /// </summary>
  private static ImagesetManager instance;

  /// <summary>
  ///		Default constructor.
  /// </summary>
  internal ImagesetManager() {
    // only create once instance
    if(instance == null) {
      instance = this;
    }
  }

  /// <summary>
  ///		Gets the singleton class instance.
  /// </summary>
  /// <value></value>
  public static ImagesetManager Instance {
    get {
      return instance;
    }
  }

  #endregion Singleton Implementation

  #region Fields

  /// <summary>
  /// Gets the file extension for an Imageset including the period prefix
  /// </summary>
  public const string ImageSetFileExtension = ".imageset";
  /// <summary>
  ///		List of all image sets that exist within the system.
  /// </summary>
  private ImagesetList imagesets = new ImagesetList();

  #endregion Fields

#if PORT_OMITTED

  public Image LoadImageFromFile(string name, string fileName) {
    return LoadImageFromFile(name, fileName, new SizeF(800, 600), true);
  }
  public Image LoadImageFromFile(string name, string fileName, SizeF nativeScreenResolution, bool autoScale) {
    Imageset imageset = CreateImageset(name, GuiSystem.Instance.Renderer.CreateTexture(fileName));
    imageset.NativeResolution = nativeScreenResolution;
    imageset.AutoScaled = autoScale;
    imageset.DefineImage(name, PointF.Empty, imageset.Texture.Size, PointF.Empty);
    return imageset.GetImage(name);
  }
  public Imageset CreateImageset(string name, string fileName) {
    return CreateImageset(name, GuiSystem.Instance.Renderer.CreateTexture(fileName));
  }

#endif // PORT_OMITTED

  /// <summary>
  ///		Create a Imageset object with the given name and Texture.
  /// </summary>
  /// <remarks>
  ///		The created Imageset will be of limited use, and will require one or more images to be defined for the set.
  /// </remarks>
  /// <param name="name">The unique name for the Imageset being created.</param>
  /// <param name="texture">Texture object to be associated with the Imageset.</param>
  /// <returns>A reference to the newly created Imageset object.</returns>
  /// <exception cref="AlreadyExistsException">If an Imageset with the specified <paramref name="name"/> already exists in the system.</exception>
  public Imageset CreateImageset(string name, Texture texture) {
    if(IsImagesetPresent(name)) {
      throw new AlreadyExistsException(string.Format("An Imageset object named '{0}' already exists.", name));
    }

    Imageset imageset = new Imageset(name, texture);

    imagesets.Add(name, imageset);

    Logger.Instance.LogEvent(string.Format("Imageset '{0}' has been created with a texture only.", name), LoggingLevel.Informative);

    return imageset;
  }

  /// <summary>
  ///		Create an Imageset object from the specified file.
  /// </summary>
  /// <param name="fileName">The name of the Imageset definition file which should be used to create the Imageset.</param>
  /// <returns>A reference to the newly created Imageset object.</returns>
  /// <exception cref="AlreadyExistsException">If an Imageset with the name specified in the file already exists in the system.</exception>
  /// <exception cref="FileIOException">If an error occurs while processing the file <paramref name="fileName"/>.</exception>
  public Imageset CreateImageset(string fileName/*, string resourceGroup*/) {
    if(!Path.HasExtension(fileName))
      fileName += ImageSetFileExtension;
    string fullPath = Path.GetFullPath(fileName);
    if(!File.Exists(fullPath))
      throw new ArgumentException(string.Format("The specified imageset file '{0}' does not exist.", fileName));

    // FIX: Use the argument instead when this class is updated
    string resourceGroup = string.Empty;
    Imageset imageset = new Imageset(fileName, resourceGroup);

    string actualName = imageset.Name;

    if(IsImagesetPresent(actualName)) {
      throw new AlreadyExistsException("An Imageset object named '{0}' already exists.", actualName);
    }

    imagesets.Add(actualName, imageset);

    Logger.Instance.LogEvent(string.Format("Imageset '{0}' has been created from the information specified in file '{1}'.", actualName, fileName), LoggingLevel.Informative);

    return imageset;
  }

  /// <summary>
  ///		Destroys all Imageset objects registered in the system.
  /// </summary>
  public void DestroyAllImagesets() {
    throw new NotImplementedException();
  }

  /// <summary>
  ///		Destroys the specified Imageset.
  /// </summary>
  /// <param name="imageset">Reference to the Imageset to be destroyed.  If the Imageset is null, nothing happens.</param>
  public void DestroyImageset(Imageset imageset) {
    if(imageset != null) {
      DestroyImageset(imageset.Name);
    }
  }

  /// <summary>
  ///		Destroys the Imageset with the specified name.
  /// </summary>
  /// <param name="name">The name of the Imageset to be destroyed.  If no such Imageset exists, nothing happens.</param>
  public void DestroyImageset(string name) {
    Imageset imageset = imagesets[name];

    // log and dispose of resources
    if(imageset != null) {
      Logger.Instance.LogEvent(string.Format("Imageset '{0}' has been destroyed", name), LoggingLevel.Informative);
      imageset.Dispose();
      imagesets.Remove(name);
    }
  }

  /// <summary>
  ///		Returns a reference to the Imageset object with the specified name.
  /// </summary>
  /// <param name="name">The name of the Imageset to return a reference to.</param>
  /// <return>Reference to the requested Imageset object.</return>
  public Imageset GetImageset(string name) {
    Imageset imageset = imagesets[name];

    if(imageset == null) {
      throw new UnknownObjectException("No Imageset named '{0}' is present in the system.", name);
    }

    return imageset;
  }

  /// <summary>
  ///		Check for the existence of a named Imageset.
  /// </summary>
  /// <param name="name">The name of the Imageset to look for.</param>
  /// <returns>Ttue if an Imageset named <paramref name="name"/> is presently loaded in the system, else false.</returns>
  public bool IsImagesetPresent(string name) {
    return imagesets[name] != null;
  }

  /// <summary>
  ///		Notify the ImagesetManager of the current (usually new) display resolution.
  /// </summary>
  /// <param name="SizeF">SizeF describing the display resolution.</param>
  public void NotifyScreenResolution(SizeF Size) {
    throw new NotImplementedException();
  }

}

} // namespace CeGui
