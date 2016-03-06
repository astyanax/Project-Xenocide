using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Xenocide.GUISystem
{
    public class Frame : GUIContainer
    {
        public Frame(IGUIManager manager)
            : base(manager)
        {

        }

        public override void Update(Rectangle viewport)
        {
            if (titleBar != null)
            {
                Rectangle ownViewport = GetOwnViewport(viewport);
                titleBar.Update(ownViewport);
            }
            
            base.Update(viewport);
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            if (skin != null)
            {
                Manager.DrawNineQuad(spriteBatch, skin, AbsolutePosition, Color);

                if (titleBar != null)
                    titleBar.Draw(spriteBatch);
            }

            base.Draw(spriteBatch);
        }

        public NineQuad Skin
        {
            get
            {
                return skin;
            }
            set
            {
                skin = value;
            }
        }

        public FrameTitleBar TitleBar
        {
            get
            {
                return titleBar;
            }
            set
            {
                titleBar = value;
            }
        }

        private NineQuad skin;
        private FrameTitleBar titleBar;
    }
}
