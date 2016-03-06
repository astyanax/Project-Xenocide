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
* @file LandMaskedGeoBitmap.cs
* @date Created: 2007/08/19
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using ProjectXenocide.Utils;

namespace ProjectXenocide.Model.Geoscape.Geography
{
    /// <summary>
    /// This class holds geographical information that has been encoded into a texture.
    /// Specifically, the colour of a pixel on the bitmap indicates a property of the area
    /// However, it also masks off any areas that do not correspond to land
    /// </summary>
    [Serializable]
    class LandMaskedGeoBitmap : GeoBitmap
    {
        /// <summary>
        /// Creates a new GeoBitmap for the given texture and list of members
        /// </summary>
        /// <param name="filename">the file holding the texture</param>
        /// <param name="allowUndefinedAreas">Is the bitmap allowed to have areas that have no properties?</param>
        public LandMaskedGeoBitmap(String filename, bool allowUndefinedAreas)
            :
            base(filename, allowUndefinedAreas)
        {
        }

        /// <summary>
        /// Load the file containing the bitmap and process it
        /// </summary>
        /// <param name="properties">The properties encoded into the bitmap</param>
        /// <param name="terrain">Used to mask off water areas</param>
        /// <param name="water">Code for water areas on terrain</param>
        public void Load(IList properties, GeoBitmap terrain, int water)
        {
            this.terrainBitmap = terrain;
            this.waterIndex    = water;
            Load(properties);
        }

        /// <summary>
        /// Return the property index of the pixel at the specified co-ordinates,
        /// masking off any pixels that correspond to water locations
        /// </summary>
        /// <param name="lookup">convert color to property index</param>
        /// <param name="x">column of pixel</param>
        /// <param name="y">rox of pixel</param>
        /// <param name="pixels">the pixels</param>
        /// <returns>property index</returns>
        protected override sbyte ToIndex(ColorToPropertyIndex lookup, int x, int y, uint[] pixels)
        {
            // if this location is over water, pixel is undefined
            if (IsWater(FromXY(x, y)))
            {
                return NoProperty;
            }
            else
            {
                return base.ToIndex(lookup, x, y, pixels);
            }
        }

        /// <summary>
        /// Is the specified location over water
        /// </summary>
        /// <param name="location">location</param>
        /// <returns>true if over water</returns>
        private bool IsWater(GeoPosition location)
        {
            return terrainBitmap.GetPropertyIndexOfLocation(location) == waterIndex;
        }

        /// <summary>
        /// Takes a geo-position and attempts to work out the closest land
        /// </summary>
        /// <param name="pos"></param>
        /// <returns>'Closest' land position to input geo-position</returns>
        private GeoPosition GetClosestLand(GeoPosition location)
        {
            return terrainBitmap.GetClosestLand(location, waterIndex);
        }

        #region fields

        /// <summary>
        /// Used to mask off water areas
        /// </summary>
        private GeoBitmap terrainBitmap;

        /// <summary>
        /// Code for water areas on terrainBitmap
        /// </summary>
        private int waterIndex;

        #endregion fields
    }
}
