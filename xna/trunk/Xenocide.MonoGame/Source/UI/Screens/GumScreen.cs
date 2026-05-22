using System;

using Gum.Forms.Controls;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MonoGameGum;

using ProjectXenocide.Assets;

namespace ProjectXenocide.UI.Screens
{
    public abstract class GumScreen : Screen
    {
        protected StackPanel RootContainer { get; private set; }
        protected GumService GumUI => GumService.Default;

        private SpriteBatch _backgroundBatch;
        private Texture2D _background;

        protected GumScreen(string ceguiId, string backgroundFilename)
            : base(ceguiId, backgroundFilename) { }

        protected GumScreen(string ceguiId)
            : base(ceguiId) { }

        public override void Show()
        {
            RootContainer = new StackPanel();
            RootContainer.AddToRoot();

            LoadBackground();

            CreateGumControls();
            WireClickSounds(RootContainer);
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

        protected override void CreateCeguiWidgets() { }

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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _backgroundBatch?.Dispose();
                _backgroundBatch = null;
                _background?.Dispose();
                _background = null;

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
