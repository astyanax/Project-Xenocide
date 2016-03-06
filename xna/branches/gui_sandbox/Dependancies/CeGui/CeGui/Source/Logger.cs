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

namespace CeGui {

/// <summary>
///		Class that implements logging for the GUI system.
/// </summary>
public class Logger : IDisposable {
  #region Fields

  /// <summary>
  ///		Level of logging that will be written to the log file.
  /// </summary>
  protected LoggingLevel loggingLevel = LoggingLevel.Standard;
  /// <summary>
  ///		Stream writer used to output the log messages.
  /// </summary>
  protected StreamWriter writer;

  #endregion Fields

  #region Singleton Implementation

  /// <summary>
  ///		Singlton instance of this class.
  /// </summary>
  private static Logger instance;

  /// <summary>
  ///		Constructor.
  /// </summary>
  /// <param name="name">Name of the log file to create.</param>
  public Logger(string name)
    :
  this(name, false) { }

  /// <summary>
  ///		Constructor.
  /// </summary>
  /// <param name="name">Name of the log file to create.</param>
  /// <param name="append">
  ///		If true, events will be added to the end of the current file.
  ///		If false, the current contents of the file will be discarded.
  /// </param>
  public Logger(string name, bool append) {
    // only create once instance
    if(instance == null) {
      instance = this;

      // we need the full path
      string fullPath = Path.Combine(Environment.CurrentDirectory, name);

      // open the file stream, appending if requested
      writer = new StreamWriter(fullPath, append);

      // create the log header
      LogEvent("+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+");
      LogEvent("+                     Crazy Eddie's GUI System - Event log                    +");
      LogEvent("+                     .Net Framework Port by Chris McGuirk                    +");
      LogEvent("+                          (http://axiomengine.sf.net)		                  +");
      LogEvent("+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+");
      LogEvent("Logger singleton created.");
    }
  }

  /// <summary>
  ///		Gets the singleton GuiSystem instance.
  /// </summary>
  /// <value></value>
  public static Logger Instance {
    get {
      return instance;
    }
  }

  #endregion Singleton Implementation

  #region Properties

  /// <summary>
  ///		Gets/Set the level of logging information that will get out to the log file.
  /// </summary>
  /// <value>Level of logging to do.</value>
  public LoggingLevel LoggingLevel {
    get {
      return loggingLevel;
    }
    set {
      loggingLevel = value;
    }
  }

  #endregion Properties

  #region Methods

  /// <summary>
  ///		Adds an event to the log.
  /// </summary>
  /// <remarks>Uses <see cref="LoggingLevel.Standard"/> by default.</remarks>
  /// <param name="message">Message to be added to the log.</param>
  public void LogEvent(string message) {
    LogEvent(message, LoggingLevel.Standard);
  }

  /// <summary>
  ///		Adds an event to the log.
  /// </summary>
  /// <param name="message">Message to be added to the log.</param>
  /// <param name="level"></param>
  public void LogEvent(string message, LoggingLevel level) {
    // only log the message if the current logging level is >= the level for the message
    if(level >= loggingLevel) {
      // write the message with the date/time and level, flushing afterwards
      writer.WriteLine("{0} ({1}):\t{2}", DateTime.Now, level, message);
      writer.Flush();
    }
  }

  #endregion Methods

  #region IDisposable Members

  /// <summary>
  ///		Ensures the log file has been closed properly.
  /// </summary>
  public void Dispose() {
    if(writer != null) {
      writer.Close();
    }
  }

  #endregion IDisposable Members
}

} // namespace CeGui