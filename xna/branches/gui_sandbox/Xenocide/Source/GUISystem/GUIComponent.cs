using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Xenocide.GUISystem
{
    public abstract class GUIComponent
    {
        public GUIComponent(IGUIManager manager)
        {
            this.manager = manager;
            color = Color.White;
        }

        public virtual void Update(Rectangle viewport)
        {
            absolutePosition = TransformPosition(viewport);
        }

        abstract public void Draw(SpriteBatch spriteBatch);

        private Rectangle TransformPosition(Rectangle viewport)
        {
            Rectangle result = Position;
            result.Width = Math.Min(result.Width, Math.Max(0, viewport.Width - result.Left));
            result.Height = Math.Min(result.Height, Math.Max(0, viewport.Height - result.Top));

            result.X += viewport.Left;
            result.Y += viewport.Top;

            return result;
        }

        public Rectangle Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
            }
        }

        protected Rectangle AbsolutePosition
        {
            get
            {
                return absolutePosition;
            }
        }

        public Color Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
            }
        }

        public IGUIManager Manager
        {
            get
            {
                return manager;
            }
        }

        private Rectangle position;
        private Rectangle absolutePosition;
        private Color color = Color.White;
        private IGUIManager manager;

        private bool mouseWasInside = false;

        public virtual bool CheckCollision(MouseEvent e, bool wasClick)
        {
            if (IsInsidePosition(e.Position))
            {
                if (!mouseWasInside)
                {
                    mouseWasInside = true;
                    OnMouseEnter(e);
                }
                if (wasClick)
                    OnClick(e);

                return true;
            }
            else
            {
                if (mouseWasInside)
                {
                    mouseWasInside = false;
                    OnMouseLeave(e);
                }
                return false;
            }
        }

        protected bool IsInsidePosition(Point point)
        {
            return (point.X >= AbsolutePosition.X
                && point.X <= AbsolutePosition.X + AbsolutePosition.Width
                && point.Y >= AbsolutePosition.Y
                && point.Y <= AbsolutePosition.Y + AbsolutePosition.Height);
        }

        public virtual void OnClick(MouseEvent e)
        {

        }

        public virtual void OnMouseEnter(MouseEvent e) { }
        public virtual void OnMouseLeave(MouseEvent e) { }
    }
}
