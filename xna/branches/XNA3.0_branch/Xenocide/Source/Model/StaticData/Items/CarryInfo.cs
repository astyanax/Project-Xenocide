#region Copyright
/*
--------------------------------------------------------------------------------
This source file is part of Xenocide
  by  Project Xenocide Team

For the latest info on Xenocide, see http://www.projectxenocide.com/

This work is licensed under the Creative Commons
Attribution-NonCommercial-ShareAlike 2.5 License.

To view a copy of this license, visit
http://creativecommons.org/licenses/by-nc-sa/2.5/
or send a letter to Creative Commons, 543 Howard Street, 5th Floor,
San Francisco, California, 94105, USA.
--------------------------------------------------------------------------------
*/

/*
* @file CarryInfo.cs
* @date Created: 2007/10/06
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;

using Microsoft.Xna.Framework;

using ProjectXenocide.Utils;

#endregion

namespace ProjectXenocide.Model.StaticData.Items
{
    /// <summary>
    /// Information related to a soldier carring an item
    /// </summary>
    /// <remarks>essentially, the size element from item.xml
    /// &lt;size xSize="1" ySize="1" mass="3" equipable="false" /&gt;
    /// </remarks>
    [Serializable]
    public class CarryInfo
    {
        /// <summary>
        /// Construct CarryInfo from information in an XML element
        /// </summary>
        /// <param name="element">XML element holding data to construct CarryInfo</param>
        public CarryInfo(XPathNavigator element)
        {
            x         = Util.GetIntAttribute( element, "xSize");
            y         = Util.GetIntAttribute( element, "ySize");
            mass      = Util.GetIntAttribute( element, "mass");
            equipable = Util.GetBoolAttribute(element, "equipable");

            // spriteRect requires some construction
            const int cellSize   = 64;
            int       top        = Util.GetIntAttribute(element, "spriteTop")  * cellSize;
            int       left       = Util.GetIntAttribute(element, "spriteLeft") * cellSize;
            spriteRect = new Rectangle(left, top, x * cellSize, y * cellSize);
        }

        #region Fields

        /// <summary>
        /// Item's size (in X direction) in soldier's backpack
        /// </summary>
        public int X { get { return x; } }

        /// <summary>
        /// Item's size (in Y direction) in soldier's backpack
        /// </summary>
        public int Y { get { return y; } }

        /// <summary>
        /// How much item encumbers soldier carrying it
        /// </summary>
        public int Mass { get { return mass; } }

        /// <summary>
        /// Can soldier be issued the item when soldier is in an outpost?
        /// </summary>
        /// <remarks>This is used to tag items that a soldier can pick up on a battlescape, but
        /// can't remove from an outpost's inventory when going on a battlescape mission.
        /// e.g. a prisoner.
        /// </remarks>
        public bool Equipable { get { return equipable; } }

        /// <summary>
        /// Source Rectange that can be used in SpriteBatch.Draw() call
        /// </summary>
        public Rectangle SpriteRect { get { return spriteRect; } }

        /// <summary>
        /// Item's size (in X direction) in soldier's backpack
        /// </summary>
        private int x;

        /// <summary>
        /// Item's size (in Y direction) in soldier's backpack
        /// </summary>
        private int y;

        /// <summary>
        /// How much item encumbers soldier carrying it
        /// </summary>
        private int mass;

        /// <summary>
        /// Can soldier be issued the item when soldier is in an outpost?
        /// </summary>
        /// <remarks>This is used to tag items that a soldier can pick up on a battlescape, but
        /// can't remove from an outpost's inventory when going on a battlescape mission.
        /// e.g. a prisoner.
        /// </remarks>
        private bool equipable;

        /// <summary>
        /// Source Rectange that can be used in SpriteBatch.Draw() call
        /// </summary>
        private Rectangle spriteRect;

        #endregion Fields
    }

}
