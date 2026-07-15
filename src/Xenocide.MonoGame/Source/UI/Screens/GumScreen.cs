using System;

using Gum.Forms;
using Gum.Forms.Controls;
using Gum.Wireframe;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MonoGameGum;

using ProjectXenocide.Assets;

namespace ProjectXenocide.UI.Screens
{
    public abstract class GumScreen : Screen
    {
        /// <summary>
        /// When false (default), screens that fail to load their GumX layout throw
        /// instead of using the programmatic CeGui fallback. Set to true during
        /// development to keep the old code path working.
        /// </summary>
        public static bool EnableProgrammaticFallback { get; set; }

        protected StackPanel RootContainer { get; private set; }
        protected GraphicalUiElement GumRoot { get; private set; }
        protected static GumService GumUI => GumService.Default;

        /// <summary>
        /// Override and return false for screens that intentionally have no .gusx
        /// (e.g., CreditsScreen with custom 2D rendering).
        /// </summary>
        protected virtual bool HasGumxLayout => true;

        private SpriteBatch _backgroundBatch;
        private Texture2D _background;

        protected GumScreen(string ceguiId, string backgroundFilename)
            : base(ceguiId, backgroundFilename) { }

        protected GumScreen(string ceguiId)
            : base(ceguiId) { }

        public override void Show()
        {
            var gumDisplay = TryLoadScreenFromGumx(CeguiId);

            if (gumDisplay != null)
            {
                gumDisplay.AddToRoot();
                GumRoot = gumDisplay;
            }
            else
            {
                RootContainer = new StackPanel();
                RootContainer.AddToRoot();
                LoadBackground();
            }

            CreateGumControls();

            if (GumRoot == null && HasGumxLayout && !EnableProgrammaticFallback)
                throw new InvalidOperationException(
                    $"Screen '{GetType().Name}' has no GumX layout and programmatic fallback is disabled. " +
                    "Set GumScreen.EnableProgrammaticFallback = true to use the legacy CeGui path.");

            if (GumRoot == null)
            {
                WireClickSounds(RootContainer);
            }
        }

        private GraphicalUiElement TryLoadScreenFromGumx(string screenName)
        {
            if (!HasGumxLayout)
                return null;

            var project = Xenocide.GumProject;
            if (project == null)
                return null;

            var screenSave = project.Screens.Find(s => s.Name == screenName);
            if (screenSave == null)
                return null;

            return screenSave.ToGraphicalUiElement();
        }

        public override void Draw(GameTime gameTime, GraphicsDevice device)
        {
            if (_background != null)
            {
                _backgroundBatch.Begin();
                _backgroundBatch.Draw(
                    _background,
                    new Rectangle(0, 0, device.Viewport.Width, device.Viewport.Height),
                    Color.White);
                _backgroundBatch.End();
            }
        }

        protected abstract void CreateGumControls();

        protected Button WireButton(string name, EventHandler handler)
        {
            if (GumRoot == null)
                return null;

            var btn = GumRoot.GetFrameworkElementByName<Button>(name);
            if (btn != null)
            {
                btn.Click += OnAnyButtonClicked;
                btn.Click += handler;
            }
            return btn;
        }

        protected void AddChild(FrameworkElement child)
        {
            if (GumRoot != null)
                GumRoot.Children.Add(child.Visual);
            else if (RootContainer != null)
                RootContainer.AddChild(child);
        }

        private static void WireClickSounds(FrameworkElement element)
        {
            if (element is Button button)
                button.Click += OnAnyButtonClicked;

            if (element is StackPanel panel)
                foreach (var child in panel.Children)
                    WireClickSounds(child);
        }

        private static void OnAnyButtonClicked(object sender, EventArgs e)
        {
            Xenocide.AudioSystem?.PlaySound(SoundId.ButtonClick1);
        }

        private void LoadBackground()
        {
            var filename = BackgroundFilename;
            if (string.IsNullOrEmpty(filename))
                return;

            var device = Xenocide.Instance.GraphicsDevice;
            _background = Texture2D.FromFile(device, filename);
            _backgroundBatch = new SpriteBatch(device);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _backgroundBatch?.Dispose();
                _backgroundBatch = null;
                _background?.Dispose();
                _background = null;

                if (GumRoot != null)
                {
                    GumRoot.RemoveFromRoot();
                    GumRoot = null;
                }

                if (RootContainer != null)
                {
                    GumService.Default.Root.Children.Remove(RootContainer.Visual);
                    RootContainer = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}