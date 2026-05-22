using System;

using Gum.Forms.Controls;

using MonoGameGum;

using ProjectXenocide.Assets;

namespace ProjectXenocide.UI.Screens
{
    public abstract class GumScreen : Screen
    {
        protected StackPanel RootContainer { get; private set; }
        protected GumService GumUI => GumService.Default;

        protected GumScreen(string ceguiId, string backgroundFilename)
            : base(ceguiId, backgroundFilename) { }

        protected GumScreen(string ceguiId)
            : base(ceguiId) { }

        public override void Show()
        {
            RootContainer = new StackPanel();
            RootContainer.AddToRoot();
            CreateGumControls();
            WireClickSounds(RootContainer);
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
            if (disposing && RootContainer != null)
            {
                GumService.Default.Root.Children.Remove(RootContainer.Visual);
                RootContainer = null;
            }
        }
    }
}
