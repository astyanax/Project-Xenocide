using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Xenocide.GUISystem
{
    public class OneImageSkin
    {
        public OneImageSkin(string textureName, Rectangle source)
        {
            this.textureName = textureName;
            this.source = source;
        }

        public string TextureName
        {
            get
            {
                return textureName;
            }
            set
            {
                textureName = value;
            }
        }

        public Rectangle Source
        {
            get
            {
                return source;
            }
            set
            {
                Source = value;
            }
        }
        
        string textureName;
        private Rectangle source;
    }
}
