using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Xenocide.GUISystem
{
    public abstract class GUIContainer : GUIComponent
    {
        public GUIContainer(IGUIManager manager)
            : base(manager)
        {

        }

        public override bool CheckCollision(MouseEvent e, )
        {
            if (base.CheckCollision(e, wasClick))
            {
                foreach (GUIComponent child in Children)
                {
                    child.CheckCollision(e, wasClick);
                }
                return true;
            }
            else
                return false;
        }

        public override void Update(Rectangle viewport)
        {
            Rectangle ownViewport = GetOwnViewport(viewport);
            
            foreach(GUIComponent child in Children)
            {
                child.Update(ownViewport);
            }

            base.Update(viewport);
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            foreach(GUIComponent child in Children)
            {
                child.Draw(spriteBatch);
            }
        }

        protected Microsoft.Xna.Framework.Rectangle GetOwnViewport(Microsoft.Xna.Framework.Rectangle viewport)
        {
            viewport.X += Position.Left;
            viewport.Y += Position.Top;

            viewport.Width = Position.Width;
            viewport.Height = Position.Height;
            return viewport;
        }

        public IList<GUIComponent> Children
        {
            get
            {
                return children;
            }
        }

        private List<GUIComponent> children = new List<GUIComponent>();
    }
}
