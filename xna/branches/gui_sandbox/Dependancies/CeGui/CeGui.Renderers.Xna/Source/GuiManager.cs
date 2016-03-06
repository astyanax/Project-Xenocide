using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CeGui.Renderers.Xna {

  /// <summary>Interface for controlling the game's GUI</summary>
  public interface IGuiService {

    /// <summary>The renderer used by the GUI component</summary>
    XnaRenderer Renderer { get; }

    /// <summary>Injects a character for text input into the GUI system</summary>
    /// <param name="enteredCharacter">Character that has been entered by the user</param>
    //void InjectCharacter(char enteredCharacter);

  }

  /// <summary>Game component for integrating CeGui in XNA applications</summary>
  public partial class GuiManager : DrawableGameComponent, IGuiService {

    /// <summary>Initializes a new GUI manager</summary>
    /// <param name="game">Game that has ownership of this GUI manager</param>
    public GuiManager(Game game)
      : base(game) {

      this.DrawOrder = 10000; // The GUI usually sits on top of everything else
      game.Services.AddService(typeof(IGuiService), this);
    }

    /// <summary>
    ///   Allows the game component to perform any initialization it needs to before starting
    ///   to run. This is where it can query for any required services and load content.
    /// </summary>
    public override void Initialize() {
      base.Initialize();

      // Query for the graphics device service through which the graphics device
      // can be accessed
      IGraphicsDeviceService graphicsDeviceService =
        (IGraphicsDeviceService)Game.Services.GetService(typeof(IGraphicsDeviceService));

      // We need these events in order to properly release resources when the
      // device is shutting down or resetting
      graphicsDeviceService.DeviceCreated += new EventHandler(graphicsDeviceCreated);
      graphicsDeviceService.DeviceDisposing += new EventHandler(graphicsDeviceDisposing);
      graphicsDeviceService.DeviceResetting += new EventHandler(graphicsDeviceResetting);
      graphicsDeviceService.DeviceReset += new EventHandler(graphicsDeviceReset);

      // If the graphics device already existed before we had a chance to attach
      // ourselfes to the service's events, create the effects now!
      GraphicsDevice graphicsDevice = graphicsDeviceService.GraphicsDevice;
      if(graphicsDevice != null)
        createRenderer();
    }

    /// <summary>Allows the game component to draw itself</summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    /// <remarks>
    ///   Draws the GUI. You can either have this called automatically by adding the GUI
    ///   component to your game's 'Components' collection (in which case you should
    ///   adjust the drawing order using the .DrawOrder property) or you can call this
    ///   method yourself in your code. Just make sure you're not doing both, that would
    ///   be quite a waste of time ;)
    /// </remarks>
    public override void Draw(GameTime gameTime) {
      base.Draw(gameTime);

      // We do input processing here instead of in Update() because it makes no sense to
      // handle input on another basis than per frame. The Update() calls in XNA are done
      // batch-wise and not regularly in a background thread, so there's nothing to gain
      // from moving this into update, not even better responsiveness (user still would
      // have to hold the mouse button down for an entire frame to achieve any effect).
      CeGui.Renderers.Xna.Source.InputInjector.processMouseInput();
      CeGui.Renderers.Xna.Source.InputInjector.processKeyboardInput(gameTime);

      GuiSystem.Instance.RenderGui();
    }

    /// <summary>The renderer used by the GUI component</summary>
    public XnaRenderer Renderer {
      get { return this.renderer; }
    }

    /// <summary>Creates the renderer and resources depending on it</summary>
    private void createRenderer() {
      destroyRenderer();

      // Create the CeGui XNA renderer. This renderer is used by CeGui to draw its
      // GUI without knowing anything about the underlying graphics api
      this.renderer = new XnaRenderer(GraphicsDevice, 4096);

      // Initialize the CeGui system. This should be the first method called before
      // using any of the CeGui routines.
      CeGui.GuiSystem.Initialize(this.renderer);

      // This has to be done *after* the GuiSystem was initialized. The bad thing is
      // that this is a global initialization step that doesn't require a renderer to
      // be available. So thanks to this design quirk, our only chance to properly load
      // the resources is to load them the first time GuiSystem is initialized.
      if(!resourcesLoaded) {
        loadCeGuiResources();
        resourcesLoaded = true;
      }
    }

    /// <summary>Destroys the renderer</summary>
    /// <remarks>
    ///   It is safe to call this method even if no renderer has been created.
    /// </remarks>
    private void destroyRenderer() {
      if(this.renderer != null) {
        this.renderer.Dispose();
        this.renderer = null;
      }
    }

    /// <summary>Loads dynamic resources and extensions</summary>
    private static void loadCeGuiResources() {

      // Widget sets are collections of widgets that provide the widget classes defined
      // in CeGui (like PushButton, CheckBox and so on) with their own distinctive look
      // (like a theme) and possibly even custom behavior.
      //
      // Here we load all compiled widget sets we can find in the current directory. This
      // is done to demonstrate how you could add widget set dynamically to your
      // application. Other possibilities would be to hardcode the widget set an
      // application uses or determining the assemblies to load from a configuration file.
      string[] assemblyFiles = System.IO.Directory.GetFiles(
        System.IO.Directory.GetCurrentDirectory(), "CeGui.WidgetSets.*.dll"
      );
      foreach(string assemblyFile in assemblyFiles) {
        WindowManager.Instance.AttachAssembly(
          System.Reflection.Assembly.LoadFile(assemblyFile)
        );
      }

      // Imagesets area a collection of named areas within a texture or image file. Each
      // area becomes an Image, and has a unique name by which it can be referenced. Note
      // that an Imageset would normally be specified as part of a scheme file, although
      // as this example is demonstrating, it is not a requirement.
      //
      // Again, we load all image sets we can find, this time searching the resources folder.
      string dir = System.IO.Directory.GetCurrentDirectory() + "\\Resources";

      if(System.IO.Directory.Exists(dir)) {
        string[] imageSetFiles = System.IO.Directory.GetFiles(dir, "*.imageset");

        foreach(string imageSetFile in imageSetFiles)
          ImagesetManager.Instance.CreateImageset(imageSetFile);
      }
    }

    /// <summary>Called when the graphics device has been created</summary>
    /// <param name="sender">The component that is managing the graphics device</param>
    /// <param name="e">Not used</param>
    private void graphicsDeviceCreated(object sender, EventArgs e) {
      createRenderer();
    }

    /// <summary>Called when the graphics device wants to reset</summary>
    /// <param name="sender">The component that is managing the graphics device</param>
    /// <param name="e">Not used</param>
    private void graphicsDeviceResetting(object sender, EventArgs e) {
      if(this.renderer != null)
        this.renderer.PreDeviceReset();
    }

    /// <summary>Called when the graphics device has been reset</summary>
    /// <param name="sender">The component that is managing the graphics device</param>
    /// <param name="e">Not used</param>
    private void graphicsDeviceReset(object sender, EventArgs e) {
      if(this.renderer != null)
        this.renderer.PostDeviceReset();
    }

    /// <summary>Called when the graphics device wants to shut down</summary>
    /// <param name="sender">The component that is managing the graphics device</param>
    /// <param name="e">Not used</param>
    private void graphicsDeviceDisposing(object sender, EventArgs e) {
      destroyRenderer();
    }

    /// <summary>Whether the global CeGui resources have been loaded yet</summary>
    private static bool resourcesLoaded;
    /// <summary>The CeGui XNA renderer</summary>
    private XnaRenderer renderer;
    /// <summary>CeGui's equivalent of Windows' "desktop window"</summary>
    private GuiSheet rootGuiSheet;
  }

} // namespace CeGui.Renderers.Xna
