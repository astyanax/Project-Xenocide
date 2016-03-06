using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Xenocide.GUISystem
{
    public struct Quad
    {
        public Quad(string textureName, Rectangle source)
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

    public class NineQuad
    {
        public Quad Top = new Quad(null, new Rectangle());
        public Quad Bottom = new Quad(null, new Rectangle());
        public Quad Left = new Quad(null, new Rectangle());
        public Quad Right = new Quad(null, new Rectangle());
        public Quad Center = new Quad(null, new Rectangle());
        public Quad TopLeft = new Quad(null, new Rectangle());
        public Quad TopRight = new Quad(null, new Rectangle());
        public Quad BottomLeft = new Quad(null, new Rectangle());
        public Quad BottomRight = new Quad(null, new Rectangle());
    }
}
