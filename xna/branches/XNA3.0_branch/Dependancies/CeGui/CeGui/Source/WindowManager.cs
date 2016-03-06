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
using System.Text;

using System.IO;
using System.Reflection;
using System.CodeDom.Compiler;
using Microsoft.CSharp;

using System.Xml;
using System.Xml.Schema;

namespace CeGui {

/// <summary>Manages creation and lifetime of Window objects</summary>
/// <remarks>
///   The WindowManager is the means by which <see cref="Window"/> objects are created
///   and destroyed. For each sub-class of Window that is to be created, there must exist
///   a <see cref="Window"/> object which is registered with the
///   <see cref="WindowManager"/>. Additionally, the WindowManager tracks every
///   Window object created, and can be used to access those Window objects by name.
/// </remarks>
public class WindowManager : IDisposable {

  struct PropertyContainer {
    public PropertyInfo property;
    public WidgetPropertyAttribute attribute;

    public PropertyContainer(PropertyInfo p, WidgetPropertyAttribute a) {
      property = p;
      attribute = a;
    }
  }

  /// <summary>Singleton instance of this class</summary>
  private static WindowManager instance;

  /// <summary>Default constructor</summary>
  internal WindowManager() {

    // only create one instance
    if(instance == null) {
      instance = this;
      instance.AttachAssembly(Assembly.GetCallingAssembly());

      Logger.Instance.LogEvent("WindowManager singleton created.");
    }

  }

  /// <summary>Gets the singleton class instance.</summary>
  public static WindowManager Instance {
    get {
      return instance;
    }
  }

  #region Window Related Methods

  public bool CompileScripts(string assemblyName, string path, bool debug) {

    // find all CSharp files
    // TODO: Make compilation more generic
    string[] scriptFiles = Directory.GetFiles(path, "*.cs");

    // create a code compiler
    CSharpCodeProvider provider = new CSharpCodeProvider();

    // configure params for the script compilation
    CompilerParameters parms = new CompilerParameters(dynamicAssemblyRefs);

    // this is an in memory assembly
    // TODO: Consider compiling to file and loading if no changes have occurred on the next load
    parms.GenerateInMemory = true;
    parms.GenerateExecutable = false;
    parms.IncludeDebugInformation = debug;

    // compile the list of files found in the script directory
    CompilerResults results = provider.CompileAssemblyFromFile(parms, scriptFiles);

    // check for errors and log them
    if(results.Errors.Count != 0) {
      StringBuilder errorBuilder = new StringBuilder(results.Errors.Count);

      string errorMsg = "Script Error [File: {0}, Line: {1}]: {2}]" + Environment.NewLine;

      foreach(CompilerError error in results.Errors) {
        errorBuilder.AppendFormat(errorMsg, error.FileName, error.Line, error.ErrorText);
      }

      Logger.Instance.LogEvent(errorBuilder.ToString(), LoggingLevel.Errors);

      return false;
    }

    AttachAssembly(results.CompiledAssembly);

    // success
    return true;
  }

  /// <summary>
  ///   Registers all the classes in an assembly that derive from type Window so that
  ///   they can be created using <see cref="CreateWindow"/>.
  /// </summary>
  /// <param name="assembly"></param>
  public void AttachAssembly(Assembly assembly) {

    // find all window types and add them to the map
    foreach(Type type in assembly.GetTypes()) {

      if(type.IsSubclassOf(typeof(Window))) {
        RegisterType(type, assembly);

      }

    }

  }

  /// <summary>
  ///   Creates a new <see cref="Window"/> object of the specified type, and gives 
  ///   it the specified unique name.
  /// </summary>
  /// <param name="type">
  ///   String that describes the type of Window to be created.  
  ///   A valid <see cref="Window"/> for the specified type must be registered.</param>
  /// <param name="name">A unique name that is to be given to the new window.</param>
  /// <returns>Reference to the newly created window.</returns>
  /// <exception cref="AlreadyExistsException">
  ///   A <see cref="Window"/> object with the specified <paramref name="name"/> already exists.
  /// </exception>
  /// <exception cref="UnknownObjectException">
  ///   No <see cref="Window"/> is registered for <paramref name="type"/> <see cref="Window"/> objects.
  /// </exception>
  /// <exception cref="GenericException">Some other error occurred (Exception message has details).</exception>
  public Window CreateWindow(string type, string name) {
    string typeName = type;

    // does the requested type exist?
    if(!typeMap.ContainsKey(type)) {
      // check for alternate names
      if(alternateNamesMap.ContainsKey(type)) {
        typeName = (string)alternateNamesMap[type];
      }
      else {
        throw new UnknownObjectException("Window type '{0}' is not currently loaded.", type);
      }
    }

    // does a window with the specified name already exist?
    if(IsWindowPresent(name)) {
      throw new InvalidRequestException("A window with the name '{0}' already exists.", name);
    }

    // grab the assembly which hosts this type
    Assembly assembly = (Assembly)typeMap[typeName];

    // create an instance of the window and initialize it
    Window window = (Window)assembly.CreateInstance(typeName, true, 0, null, new object[] { "", name }, null, null);

    // keep track of windows we have created
    AttachWindow(window);

    // return the newly created window
    return window;
  }

  /// <summary>Attaches a window to the window manager</summary>
  /// <param name="window">The window object to attach.</param>
  /// <remarks>You need to call this after you create each window if you don't use <see cref="CreateWindow"/>.</remarks>
  public void AttachWindow(Window window) {

    if(window.Name == null)
      throw new ArgumentException("The window does not have a name specified and can therefore not be registered.");

    // does a window with the specified name already exist? 
    if(IsWindowPresent(window.Name)) {
      throw new InvalidRequestException("A window with the name '{0}' already exists.", window.Name);
    }

    // TODO: Put this Initialize call back into CreateWindow?
    // (PDT) This method is doing more than it states.  It should be the job of whoever creates the window to call
    // the Initialize method.  How do we know that when attaching a window created externally that Initialize has
    // not already been called?
    window.Initialize();

    // keep track of windows we have created 
    windowRegistry.Add(window);
  }

  /// <summary>Destroys all <see cref="Window"/> objects within the system</summary>
  /// <exception cref="InvalidRequestException">If the factory for any window has been removed.</exception>
  public void DestroyAllWindows() {
    GuiSystem.Instance.GuiSheet = null;
    for(int i = 0; i < windowRegistry.Count; i++) {
      Window window = windowRegistry[i];

      window.Dispose();
    }

    // clear the current window list
    windowRegistry.Clear();
  }

  /// <summary>
  ///		Destroy the specified <see cref="Window"/> object.
  /// </summary>
  /// <param name="window">Reference to the <see cref="Window"/> object to be destroyed.  
  ///		If <paramref name="window"/> is not recognized, nothing happens.
  /// </param>
  /// <exception cref="InvalidRequestException">
  ///		If the factory for <paramref name="window"/> has been removed.
  /// </exception>
  /// <exception cref="ArgumentNullException">If <paramref name="window"/> is null.</exception>
  public void DestroyWindow(Window window) {
    // remove the window from the list
    windowRegistry.Remove(window);

    // destroy the window
    window.Destroy();
  }

  /// <summary>
  ///		Destroy the <see cref="Window"/> object with the specified name.
  /// </summary>
  /// <param name="name">
  ///		Name of the <see cref="Window"/> object to destroy.
  /// 	If <paramref name="name"/> is not recognized, nothing happens.
  /// </param>
  /// <exception cref="InvalidRequestException">
  ///		If the factory for <paramref name="name"/> has been removed.
  /// </exception>
  /// <exception cref="ArgumentNullException">If <paramref name="name"/> is null.</exception>
  public void DestroyWindow(string name) {
    // find the window by name
    Window window = GetWindow(name);

    // destroy the window if there is one with this name
    if(window != null) {
      DestroyWindow(window);
    }
  }

  /// <summary>
  ///		Return a reference to the specified <see cref="Window"/> object.
  /// </summary>
  /// <param name="name">Name of the window to be returned.</param>
  /// <returns>A reference to a <see cref="Window"/> with the specified <paramref name="name"/></returns>
  /// <exception cref="UnknownObjectException">
  ///		If a window with the specified <paramref name="name"/> does not exist in the system.
  /// </exception>
  public Window GetWindow(string name) {
    Window window = windowRegistry[name];

    if(window != null) {
      return window;
    }

    throw new UnknownObjectException("A Window with the name {0} does not exist within the system.", name);
  }

  /// <summary>
  ///		Examines the list of <see cref="Window"/> objects to see if one exists with the given name.
  /// </summary>
  /// <param name="name">Name of the window to look for.</param>
  /// <returns>
  ///		True if the window was found with the specified <paramref name="name"/>.
  ///		False if no matching window was found.
  /// </returns>
  public bool IsWindowPresent(string name) {
    return windowRegistry.Contains(name);
  }

  /// <summary>
  /// Examines the list of Properties of this type to find any that are flagged to be exported.
  /// </summary>
  /// <param name="type">The type of the object to check</param>
  public void RegisterProperties(Type type) {
    Hashtable propertyMap = new Hashtable();

    PropertyInfo[] properties = type.GetProperties();
    foreach(PropertyInfo property in properties) {
      WidgetPropertyAttribute[] attributes =
          (WidgetPropertyAttribute[])property.GetCustomAttributes(typeof(WidgetPropertyAttribute), true);

      foreach(WidgetPropertyAttribute attrib in attributes) {
        propertyMap[attrib.Name] = new PropertyContainer(property, attrib);
      }
    }

    mPropertyMap[type.FullName] = propertyMap;
  }

  /// <summary>
  ///		Regster an existing widget type with the system.
  /// </summary>
  /// <param name="type"></param>
  /// <param name="assembly"></param>
  public void RegisterType(Type type, Assembly assembly) {
    if(!typeMap.ContainsKey(type.Name)) {
      typeMap.Add(type.FullName, assembly);

      Logger.Instance.LogEvent("WindowManager: Window type '" + type.FullName + "' registered.");
    }

    // register all of the alternate names for the widget
    AlternateWidgetNameAttribute[] attributes =
        (AlternateWidgetNameAttribute[])type.GetCustomAttributes(typeof(AlternateWidgetNameAttribute), true);
    foreach(AlternateWidgetNameAttribute attr in attributes) {
      if(!alternateNamesMap.ContainsKey(attr.Name)) {
        alternateNamesMap.Add(attr.Name, type.FullName);
      }
      else {
        Logger.Instance.LogEvent("WindowManager: Window type '" + attr.Name + "' already registered.", LoggingLevel.Errors);
      }
    }

    // register the properties
    RegisterProperties(type);
  }

  #endregion Window Related Methods

  #region XML Layout Methods
  // (PDT) Changes made to JW's original patch:
  // Modified the public layout loading methods to return a single Window rather than an ArrayList.
  // The XML schema enforces the rule that there can only be one actual 'root' window in a layout, so the
  // ArrayList would only ever have one element anyway.  Various other changes were made that relate to
  // this.
  // Also changed the method names to use the same casing scheme as the rest of the system.
  // NB: There is a slight variation in the behavior here from what the C++ version does, here we may return null
  // to indicate failure, in C++ I basically state that if the method returns the Window is valid (exceptions are used
  // for error cases) Not sure which approach I prefer.

  /// <summary>
  ///		Creates a window from the information contained in the XML node.
  /// </summary>
  /// <param name="windowNode">The XmlNode that contains information about the window</param>
  /// <param name="guiHandler">The object that will handle the gui events</param>
  /// <param name="eventMap">Event handlers to hookup</param>
  /// <returns>Returns the window that was created</returns>
  Window LoadWindow(XmlNode windowNode, object guiHandler, Hashtable eventMap, XmlNamespaceManager nsMgr) {
    string type = windowNode.Attributes["Type"].Value;
    string name = windowNode.Attributes["Name"].Value;

    // if the widget doesn't exist, then issue a warning and return null
    if(!typeMap.ContainsKey(type) && !alternateNamesMap.ContainsKey(type)) {
      Logger.Instance.LogEvent("Window type '" + type + "' does not exist", LoggingLevel.Errors);
      return null;
    }

    Window window = CreateWindow(type, name);

    Hashtable properties;
    if(typeMap.ContainsKey(type)) {
      properties = (Hashtable)mPropertyMap[type];
    }
    else {
      properties = (Hashtable)mPropertyMap[alternateNamesMap[type]];
    }

    if(null != window) {
      // read the properties
      foreach(XmlNode property in windowNode.SelectNodes("cg:Property", nsMgr)) {
        string propName = property.Attributes["Name"].Value;
        string propValue = property.Attributes["Value"].Value;

        if(properties.ContainsKey(propName)) {
          PropertyContainer propertyContainer = (PropertyContainer)properties[propName];
          propertyContainer.attribute.SetValue(window, propertyContainer.property, propValue);
        }
        else {
          Logger.Instance.LogEvent("The '" + propName + "' property does not exist in the " + type + " widget.");
        }
      }

      // map any events
      if(null != guiHandler) {
        // build a list of the widgets events
        Hashtable widgetEvents = new Hashtable();
        Type widgetType = window.GetType();
        foreach(EventInfo evt in widgetType.GetEvents()) {
          widgetEvents.Add(evt.Name, evt);
        }

        // loop through the event nodes, and create the mappings
        foreach(XmlNode evt in windowNode.SelectNodes("cg:Event", nsMgr)) {
          string eventName = evt.Attributes["Name"].Value;
          string eventFunction = evt.Attributes["Function"].Value;

          if(widgetEvents.ContainsKey(eventName) && eventMap.ContainsKey(eventFunction)) {
            EventInfo evtInfo = (EventInfo)widgetEvents[eventName];
            MethodInfo fnInfo = (MethodInfo)eventMap[eventFunction];

            evtInfo.AddEventHandler(window, Delegate.CreateDelegate(evtInfo.EventHandlerType, guiHandler, fnInfo.Name));
          }
          else {
            if(!widgetEvents.Contains(eventName)) {
              Logger.Instance.LogEvent("The '" + window.GetType().Name + "' widget does not contain the event '" + eventName + "'", LoggingLevel.Errors);
            }
            else if(!eventMap.Contains(eventFunction)) {
              Logger.Instance.LogEvent("The gui handler does not contain the event function '" + eventFunction + "'", LoggingLevel.Errors);
            }
          }
        }
      }

      // load any child nodes
      foreach(XmlNode child in windowNode.SelectNodes("cg:Window", nsMgr)) {
        Window temp = LoadWindow(child, guiHandler, eventMap, nsMgr);

        if(null != temp) {
          window.AddChild(temp);
        }
      }

      // import any nested layouts
      foreach(XmlNode child in windowNode.SelectNodes("LayoutImport")) {
        Window importedLayout = LoadWindowLayout(child.Attributes["Filename"].Value);
        window.AddChild(importedLayout);
      }
    }

    return window;
  }

  /// <summary>
  ///		Loads a window layout file.
  /// </summary>
  /// <remarks>
  ///		This will load a layout file that follows the same format as the c++ CEGUI format.
  /// </remarks>
  /// <param name="fileName">Name of the layout file.</param>
  /// <returns>Returns the root window of the layout.</returns>
  public Window LoadWindowLayout(string fileName) {
    return LoadWindowLayout(fileName, null);
  }

  /// <summary>
  ///		Loads a window layout file.
  /// </summary>
  /// <remarks>
  ///		This will load a layout file that follows the same format as the c++ CEGUI format.
  /// </remarks>
  /// <param name="fileName">Name of the layout file.</param>
  /// <param name="guiHandler">The object that will handle gui events.</param>
  /// <returns>Returns the root window of the layout.</returns>
  public Window LoadWindowLayout(string fileName, object guiHandler) {
    Window root = null;
    XmlReaderSettings settings = new XmlReaderSettings();
    settings.ValidationType = ValidationType.Schema;
    settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
    settings.Schemas.Add(SchemaNamespace, SchemaFileName);
    XmlReader reader = XmlReader.Create(fileName, settings);

    XmlDocument doc = new XmlDocument();
    doc.Load(reader);

    XmlNamespaceManager nsMgr = new XmlNamespaceManager(doc.NameTable);
    nsMgr.AddNamespace("cg", SchemaNamespace);

    XmlNode rootNode = doc.SelectSingleNode("//cg:GUILayout", nsMgr);

    // check the guiHandler for GuiEvents
    Hashtable eventMap = null;
    if(null != guiHandler) {
      eventMap = new Hashtable();
      Type handlerType = guiHandler.GetType();

      if(handlerType != typeof(object)) {
        foreach(MethodInfo method in handlerType.GetMethods()) {
          GuiEventAttribute[] attributes = (GuiEventAttribute[])method.GetCustomAttributes(typeof(GuiEventAttribute), true);

          if(attributes.Length > 0) {
            eventMap.Add(method.Name, method);
          }
        }
      }
    }

    // load the windows
    foreach(XmlNode windowNode in rootNode.SelectNodes("cg:Window", nsMgr)) {
      Window window = LoadWindow(windowNode, guiHandler, eventMap, nsMgr);

      if(null != window) {
        root = window;
      }
    }

    reader.Close();

    return root;
  }

  #endregion

  #region IDisposable Members

  /// <summary>
  ///		Called when the object should release and Strings.
  /// </summary>
  public void Dispose() {
    // destroy existing windows
    DestroyAllWindows();

    Logger.Instance.LogEvent("WindowManager singleton destroyed.");
  }

  #endregion

  /// <summary>The container that forms the Window registry</summary>
  protected WindowTable windowRegistry = new WindowTable();

  /// <summary>Assembly list used for compiling dynamic scripts</summary>
  protected static string[] dynamicAssemblyRefs = new string[] {
      "System.dll", "CeGui.dll" }; // njk-patch

  /// <summary>The CeGui widget types that have been registered, and the assembly each is in</summary>
  protected Hashtable typeMap = new Hashtable();

  /// <summary>Contains currently defined aliases for Window types</summary>
  protected Hashtable alternateNamesMap = new Hashtable();

  /// <summary>The Properties each type of CeGui widget has</summary>
  protected Hashtable mPropertyMap = new Hashtable();

  #region Constants

  /// <summary>Name of the XSD file to use for validation.</summary>
  const string SchemaFileName = "GUILayout.xsd";

  /// <summary>Name of the schema namespace.</summary>
  const string SchemaNamespace = "http://ceguisharp.sourceforge.net/0.5/GUILayout";

  #endregion
}

} // namespace CeGui