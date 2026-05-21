using MonoGameGum;
using Gum.Forms.Controls;

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
        }

        protected abstract void CreateGumControls();

        protected override void CreateCeguiWidgets() { }

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
