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
* @file LineMeshBuilder.cs
* @date Created: 2008/01/04
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

#endregion

namespace ProjectXenocide.UI.Scenes
{
    /// <summary>
    /// Puts to together list of verices and indices that define a mesh of lines
    /// </summary>
    public abstract class LineMeshBuilder
    {
        /// <summary>
        /// Fill the lists that define the lines
        /// </summary>
        /// <param name="verts">the endpoints of the lines</param>
        /// <param name="indexes">order to draw lines</param>
        public abstract void Build(IList<VertexPositionColor> verts, IList<short> indexes);
    }
}
