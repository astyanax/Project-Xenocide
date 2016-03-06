using System;
using System.Collections.Generic;
using System.Text;

namespace Xenocide.GUISystem
{
    public class Position
    {
        public Position(double relativeX, double relativeY)
        {
            relX = relativeX;
            relY = relativeY;
        }

        private double relX;
        private double relY;
    }
}
