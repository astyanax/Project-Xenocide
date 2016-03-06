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
* @file IGeoBitmapProperty.cs
* @date Created: 2007/06/28
* @author File creator: darkside
* @author Credits: dteviot
*/
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectXenocide.Model.Geoscape.Geography
{
    /// <summary>
    /// A geographical property encoded into a pixel in a GeoBitmap
    /// </summary>
    public interface IGeoBitmapProperty
    {
        /// <summary>
        /// All pixels on the bitmap with this colour have this property
        /// </summary>
        uint ColorKey { get; }

        /// <summary>
        /// The total number of pixels in the GeoBitmap that have this property
        /// </summary>
        uint Size { get; set; }
    }

    /// <summary>
    /// Minimal implementation of IGeoBitmapProperty
    /// </summary>
    [Serializable]
    public class GeoBitmapProperty : IGeoBitmapProperty
    {
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="colorKey">All pixels on the bitmap with this colour have this property</param>
        public GeoBitmapProperty(uint colorKey)
        {
            this.colorKey = colorKey;
        }

        #region fields

        /// <summary>
        /// All pixels on the bitmap with this colour have this property
        /// </summary>
        public uint ColorKey { get { return colorKey; } }

        /// <summary>
        /// The total number of pixels in the GeoBitmap that have this property
        /// </summary>
        public uint Size { get {return size;} set {size = value;} }

        /// <summary>
        /// All pixels on the bitmap with this colour have this property
        /// </summary>
        private uint colorKey;

        /// <summary>
        /// The total number of pixels in the GeoBitmap that have this property
        /// </summary>
        private uint size;

        #endregion fields
    }
}
