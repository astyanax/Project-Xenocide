using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Xenocide.GUISystem
{
    public interface IGUIBuilder
    {
        StaticImage CreateStaticImage(Rectangle position, Quad skin);
        StaticText CreateStaticText(Point position, string text);
        Frame CreateWindow(Rectangle position, string title);
    }
}
