using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Xenocide.GUISystem
{
    public class FrameTitleBar : GUIComponent
    {
        public FrameTitleBar(IGUIManager manager)
            : base(manager)
        {

        }

        public override void Update(Rectangle viewport)
        {
            this.Position = new Rectangle(Position.X, Position.Y, viewport.Width, height);
            base.Update(viewport);
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            if(skin != null)
                Manager.DrawNineQuad(spriteBatch, skin, AbsolutePosition, Color);

            if(!String.IsNullOrEmpty(text))
            {
                Manager.DrawString(spriteBatch, font, text, new Point(AbsolutePosition.X + 2, AbsolutePosition.Y + 2), Color);
            }
        }

        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
            }
        }

        public string Font
        {
            get
            {
                return font;
            }
            set
            {
                font = value;
            }
        }

        public int Height
        {
            get
            {
                return height;
            }
            set
            {
                height = value;
            }
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

        private string text;
        private int width;
        private int height = 10;
        private string font;

        private NineQuad skin;
    }
}
