using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using ProjectXenocide.Assets;

namespace ProjectXenocide.UI.Screens
{
    /// <summary>
    /// Base class for all displayable UI frames. Provides lifecycle hooks,
    /// content loading, and escape key handling.
    /// </summary>
    /// <remarks>
    /// ARCHITECTURE: Root of the screen hierarchy. Manages the CeguiId used to
    /// look up Gum .gusx layouts. Provides disposable pattern for cleanup.
    /// 
    /// LIFECYCLE: Show() → LoadContent() → Update()/Draw() → UnloadContent() → Dispose()
    /// </remarks>
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

        /// <summary>
        /// Optional software-cursor override for this frame. When set (and the
        /// pointer is not over a Gum control) the <see cref="UI.SoftwareCursor"/>
        /// draws this cursor instead of the context default. Used for the
        /// base-placement cursor.
        /// </summary>
        public virtual UI.SoftwareCursor.CursorType? RequestedCursor => null;

        protected string CeguiId { get { return ceguiId; } }

        protected static ScreenManager ScreenManager { get { return Xenocide.ScreenManager; } }

        public const SoundId DefaultButtonClickSound = SoundId.ButtonClick1;

        private string ceguiId;
    }
}
