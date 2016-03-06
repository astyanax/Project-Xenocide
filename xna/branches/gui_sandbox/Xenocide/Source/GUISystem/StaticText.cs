using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Xenocide.GUISystem
{
    public class StaticText : GUIComponent
    {
        public StaticText(IGUIManager manager)
            : base(manager)
        {

        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            /*
            SpriteFont font = Manager.GetFont(fontName);

            Rectangle absolutePos = TransformPosition(viewport);

            spriteBatch.DrawString(font, text, new Vector2(absolutePos.Left, absolutePos.Top), Color);
             */
        }

        public string FontName
        {
            get
            {
                return fontName;
            }
            set
            {
                fontName = value;
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

        private string fontName;
        private string text = "";
    }
}
