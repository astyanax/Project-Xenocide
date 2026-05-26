using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using ProjectXenocide.Assets;

namespace ProjectXenocide.UI.Screens
{
    public abstract class Frame : IDisposable
    {
        protected Frame(string ceguiId)
        {
            this.ceguiId = ceguiId;
        }

        public virtual void Show()
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public virtual void LoadContent(ContentManager content, GraphicsDevice device)
        {
        }

        public virtual void UnloadContent()
        {
        }

        public virtual void SaveState()
        {
        }

        public virtual bool HandleEscape()
        {
            return false;
        }

        public virtual void Enable(bool enableFrame)
        {
        }

        public bool EnableButtonSounds { get; set; } = true;

        public virtual bool Visible { get; set; } = true;

        protected string CeguiId { get { return ceguiId; } }

        protected ScreenManager ScreenManager { get { return Xenocide.ScreenManager; } }

        public const SoundId DefaultButtonClickSound = SoundId.ButtonClick1;

        private string ceguiId;
    }
}
