using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Xenocide.GUISystem
{
    public class OneQuadComponent : GUIComponent
    {
        public OneQuadComponent(IGUIManager manager)
            : base(manager)
        {

        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            Manager.DrawQuad(spriteBatch, quad, AbsolutePosition, Color, QuadRenderMode.Tiled, QuadRenderMode.Tiled);            
        }

        public Quad Quad
        {
            get
            {
                return quad;
            }
            set
            {
                quad = value;
            }
        }

        private Quad quad;
    }
}
