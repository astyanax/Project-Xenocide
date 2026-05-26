using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectXenocide.UI.Screens
{
    public abstract class Screen : Frame
    {
        protected Screen(string ceguiId, string backgroundFilename)
            : base(ceguiId)
        {
            this.backgroundFilename = backgroundFilename;
        }

        protected Screen(string ceguiId)
            : this(ceguiId, @"Content/Textures/UI/GeoscapeScreenBackground.png")
        {
        }

        public virtual void Update(GameTime gameTime)
        {
        }

        public virtual void Draw(GameTime gameTime, GraphicsDevice device)
        {
        }

        protected string BackgroundFilename { get { return backgroundFilename; } }

        private string backgroundFilename;
    }
}
