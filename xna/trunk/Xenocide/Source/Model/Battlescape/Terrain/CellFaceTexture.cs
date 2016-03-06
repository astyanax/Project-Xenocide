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
* @file CellFaceTexture.cs
* @date Created: 2008/01/01
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

using System;
using System.Text;

namespace ProjectXenocide.Model.Battlescape
{
    /// <summary>
    /// Description of the textures in the default texture atlas
    /// </summary>
    /// <remarks>This will eventually be replaced</remarks>
    public enum CellFaceTexture
    {
        /// <summary>no texture</summary>
        None,

        /// <summary>Walkable terrain</summary>
        GroundGreyTile,

        /// <summary>Ground that can't be walked on, e.g. water</summary>
        Water,

        /// <summary>possible start/end location for an X-Corp combatant</summary>
        XCorpStart,

        /// <summary>Ground floor of a grav lift</summary>
        GravLift,

        /// <summary>A wall</summary>
        WallWallpapered,

        /// <summary>A door</summary>
        Door,

        /// <summary>A wall with a window</summary>
        Window,

        /// <summary>Walkable terrain</summary>
        GroundBrickPath,

        /// <summary>Walkable terrain</summary>
        GroundGrass,

        /// <summary>Walkable terrain</summary>
        GroundBlueTile,

        /// <summary>A wall</summary>
        WallWhiteMetal,

        /// <summary>A wall</summary>
        WallYellowMetal,

    }
}
