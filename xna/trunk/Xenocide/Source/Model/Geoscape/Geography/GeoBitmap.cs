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
* @file GeoBitmap.cs
* @date Created: 2007/06/28
* @author File creator: darkside
* @author Credits: dteviot
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
    /// that correlates to the location of the pixel.
    /// e.g. A hightmap is an example of a GeoBitmap.
    /// </summary>
    [Serializable]
    class GeoBitmap
    {
        /// <summary>
        /// Creates a new GeoBitmap for the given texture and list of members
        /// </summary>
        /// <param name="filename">the file holding the texture</param>
        /// <param name="allowUndefinedAreas">Is the bitmap allowed to have areas that have no properties?</param>
        public GeoBitmap(String filename, bool allowUndefinedAreas)
        {
            this.filename            = filename;
            this.allowUndefinedAreas = allowUndefinedAreas;
        }

        /// <summary>
        /// Load the file containing the bitmap and process it
        /// </summary>
        /// <param name="properties">The properties encoded into the bitmap</param>
        public void Load(IList properties)
        {
            // load bitmaps from file (not content manager, since content manager cannot load dynamically)
            Texture2D texture = Texture2D.FromFile(Xenocide.Instance.GraphicsDevice, filename);

            this.width = texture.Width;
            this.height = texture.Height;
            tuples = new List<Tuple>();
            rowIndexes = new int[height];
            EncodeTuples(texture, properties);
        }

        /// <summary>
        /// Convert a pixel's location into a GeoPosition
        /// </summary>
        /// <param name="x">pixel's column</param>
        /// <param name="y">pixel's row</param>
        /// <returns>Geoposition of center of pixel</returns>
        public GeoPosition FromXY(long x, long y)
        {
            Debug.Assert((0 <= x) && (x < width));
            Debug.Assert((0 <= y) && (y < height));
            float longitude = (float)(Math.PI * ((1.0 + (2.0 * x)) / width - 1.0));
            float latitude  = (float)(Math.PI * (0.5 - (y + 0.5) / height));
            return new GeoPosition(longitude, latitude);
        }

        /// <summary>
        /// Return location of pixel containing a given GeoPosition
        /// </summary>
        /// <param name="location">GeoPosition to locate</param>
        /// <returns>Row and Column of pixel</returns>
        public Vector2 ToXY(GeoPosition location)
        {
            int x = (int)(((location.Longitude / Math.PI) + 1.0) * width * 0.5);
            // a normalized longitude of 1.0 (or +180 degrees) is off the bitmap
            if (width <= x)
            {
                --x;
            }
            int y = (int)((0.5 - (location.Latitude / Math.PI)) * height);
            // a normalized latitude of 1.0 (or -90 degrees) is off the bitmap
            if (height <= y)
            {
                --y;
            }
            return new Vector2(x, y);
        }

        /// <summary>
        /// Get the property of the specifified location on the GeoBitmap
        /// Specifically, it returns an index to the element of properties that has the property.
        /// </summary>
        /// <param name="location">location to test</param>
        /// <returns>property at this location, or NoProperty if there isn't one for the location</returns>
        public int GetPropertyIndexOfLocation(GeoPosition location)
        {
            Vector2 pixelCoords = ToXY(location);
            long    x           = (int)pixelCoords.X;
            int     index       = rowIndexes[(int)pixelCoords.Y];
            long    position    = 0;

            while (true)
            {
                position += tuples[index].Count;
                if (x < position)
                {
                    return tuples[index].Index;
                }
                ++index;
            }
        }

        /// <summary>
        /// Test a point to see if it's closer than the best find so far
        /// </summary>
        /// <param name="testX">Point to test</param>
        /// <param name="desiredX">Position desired</param>
        /// <param name="closestX">BestMatchSoFar</param>
        /// <param name="closestDistance">How far off best match was</param>
        private void SetClosestDistance(long testX, long desiredX, ref long closestX, ref long closestDistance)
        {
            // Note that world is circular
            long testDistance = Math.Abs(desiredX - testX);
            testDistance = Math.Min(testDistance, this.width - testDistance);
            if (testDistance < closestDistance)
            {
                closestX = testX;
                closestDistance = testDistance;
            }
        }

        public GeoPosition GetClosestLand(GeoPosition location, int waterIndex)
        {
            Vector2 pixelCoords = ToXY(location);
            long x = (int)pixelCoords.X;
            int index = rowIndexes[(int)pixelCoords.Y];
            long position = 0;
            int deltaY = 0;

            // this is the pixel range to search for on this line
            const int NotFound = -1;
            long closestX = NotFound;
            long closestDistance = this.width + 1;
            bool eastLandFound = false;
            bool westLandFound = false;

            while (true)
            {
                if (tuples[index].Index != waterIndex)
                {
                    long right = position + tuples[index].Count - 1;
                    // if the position is to the left of the marked position and is land, it's a candiate for
                    // closeed land to the west
                    if (position <= x)
                    {
                        if (x <= right)
                        {
                            // selected position is ground, so just return it
                            return (0 == deltaY) ? location : FromXY(x, (long)pixelCoords.Y + deltaY);
                        }
                        SetClosestDistance(position, x, ref closestX, ref closestDistance);
                        SetClosestDistance(right, x, ref closestX, ref closestDistance);
                        westLandFound = true;
                    }
                    else
                    {
                        // land is to right of desired position
                        SetClosestDistance(position, x, ref closestX, ref closestDistance);
                        SetClosestDistance(right, x, ref closestX, ref closestDistance);
                        eastLandFound = true;

                        // if we previously found land to west, then we've found the closest land
                        if (westLandFound)
                        {
                            break;
                        }
                    }
                }

                position += tuples[index].Count;
                ++index;
                if (this.width <= position)
                {
                    // we've scaned the whole latitude
                    // if we found land, were done, otherwise, try next latitude south
                    if (westLandFound || eastLandFound)
                    {
                        break;
                    }
                    else
                    {
                        position = 0;
                        deltaY += 1;
                    }
                }
            }
            return FromXY((long)closestX, ((long)pixelCoords.Y) + deltaY);
        }

        /// <summary>
        /// Picks a random coordinate, that has the property given by the 
        /// element at propertyIndex of properties
        /// </summary>
        /// <param name="propertyIndex">The member to pick a random coordinate in</param>
        /// <param name="count">Number of pixels in bitmap with this property</param>
        /// <returns>A random coordinate in the passed member</returns>
        public GeoPosition GetRandomCoord(int propertyIndex, uint count)
        {
            uint position = 0;
            uint counter = 0;

            uint randomPosition = (uint)Xenocide.Rng.Next((int)count);
            foreach (Tuple tuple in tuples)
            {
                if (tuple.Index == propertyIndex)
                {
                    if (randomPosition >= counter && randomPosition < counter + tuple.Count)
                    {
                        position += (randomPosition - counter);
                        return FromXY(position % width, position / width);
                    }
                    counter += tuple.Count;
                }
                position += tuple.Count;
            }

            // should never get here
            Debug.Assert(false);
            return new GeoPosition(0, 0);
        }

        /// <summary>
        /// Magic number, indicating no index
        /// </summary>
        public const sbyte NoProperty = -1;

        /// <summary>
        /// Preforms RLE compression on the bitmap into a List of Tuples
        /// </summary>
        /// <param name="texture">the texture which represents the uncompressed bitmap</param>
        /// <param name="properties">the properties represented by colors in the uncompressed bitmap</param>
        private void EncodeTuples(Texture2D texture, IList properties)
        {
            uint[] pixels = new uint[width * height];
            texture.GetData<uint>(pixels);

            // set size to zero, becuase we're going to recalc it
            foreach (Object o in properties)
            {
                (o as IGeoBitmapProperty).Size = 0;
            }

            ColorToPropertyIndex lookup = new ColorToPropertyIndex(properties);
            for (ushort y = 0; y < height; ++y)
            {
                rowIndexes[y] = tuples.Count;
                Tuple tuple = new Tuple(ToIndex(lookup, 0, y, pixels));
                for (int x = 1; x < width; ++x)
                {
                    sbyte index = ToIndex(lookup, x, y, pixels);
                    if (index == tuple.Index)
                    {
                        ++tuple.Count;
                    }
                    else
                    {
                        AddTuple(tuple, properties);
                        tuple = new Tuple(index);
                    }
                }
                AddTuple(tuple, properties);
            }
            ValidateEncoding(properties);
        }

        /// <summary>
        /// Return the property index of the pixel at the specified co-ordinates
        /// </summary>
        /// <param name="lookup">convert color to property index</param>
        /// <param name="x">column of pixel</param>
        /// <param name="y">rox of pixel</param>
        /// <param name="pixels">the pixels</param>
        /// <returns>property index</returns>
        protected virtual sbyte ToIndex(ColorToPropertyIndex lookup, int x, int y, uint[] pixels)
        {
            sbyte index = lookup.GetIndex(pixels[y * width + x]);
            if (!allowUndefinedAreas && (NoProperty == index))
            {
                string err = Util.StringFormat("File {0}: illegal color at pixel x = {1}, y = {2}", filename, x, y);
                throw new System.IO.IOException(err);
            }
            return index;
        }

        /// <summary>
        /// checking the supplied bitmap was valid
        /// </summary>
        private void ValidateEncoding(IList properties)
        {
            // Bitmap needs to include each property
            foreach(Object o in properties)
            {
                IGeoBitmapProperty property = o as IGeoBitmapProperty;
                if (0 == property.Size)
                {
                    string err = Util.StringFormat("File {0}: ARGB color {1:x} is not used", filename, property.ColorKey);
                    throw new System.IO.IOException(err);
                }
            }
        }

        /// <summary>
        /// Record the tuple and update properties.
        /// </summary>
        /// <param name="tuple">tuple holding index to property to update, and amount of update</param>
        /// <param name="properties">the properties to update</param>
        private void AddTuple(Tuple tuple, IList properties)
        {
            // add tuple to list
            tuples.Add(tuple);

            // if this tuple encodes a property
            // update the count of number of pixels with this property
            if (NoProperty != tuple.Index)
            {
                (properties[tuple.Index] as IGeoBitmapProperty).Size += tuple.Count;
            }
        }
        
        /// <summary>
        /// Name of file holding the bitmap
        /// </summary>
        private string filename;

        /// <summary>
        /// Is the bitmap allowed to have areas that have no properties
        /// </summary>
        private bool allowUndefinedAreas;

        /// <summary>
        /// Width and Height of the texture
        /// </summary>
        [NonSerialized]
        private int width, height;

        /// <summary>
        /// List of tuples which represents the RLE encoded date of the original bitmap
        /// </summary>
        [NonSerialized]
        private List<Tuple> tuples;

        /// <summary>
        /// Index to tuple that is first tuple of a given row
        /// </summary>
        [NonSerialized]
        private int[] rowIndexes;

        /// <summary>
        /// Class to encapsulate going from a Color to an IGeoBitmapProperty[] properties
        /// </summary>
        protected class ColorToPropertyIndex
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="properties">The properties encoded into the bitmap</param>
            public ColorToPropertyIndex(IList properties)
            {
                Debug.Assert(properties.Count < Tuple.MaxIndex);
                colorToIndex = new Dictionary<uint, sbyte>();
                for (sbyte i = 0; i < properties.Count; ++i)
                {
                    colorToIndex.Add((properties[i] as IGeoBitmapProperty).ColorKey, i);
                }
            }

            /// <summary>
            /// Do lookup to convert colour to (index of) property
            /// </summary>
            /// <param name="color">Color to lookup</param>
            /// <returns>index to property having this color, or NoProperty if color not found</returns>
            public sbyte GetIndex(uint color)
            {
                if (colorToIndex.ContainsKey(color))
                {
                    return colorToIndex[color];
                }
                else
                {
                    return NoProperty;
                }
            }

            /// <summary>
            /// Map for converting from known colors to property indexes
            /// </summary>
            private Dictionary<uint, sbyte> colorToIndex;
        }

        /// <summary>
        /// The tuple is used in the RLE compression of bitmap to
        /// take a row of identical pixels and replace it with a class which just stores 
        /// the pixel data and number occurances for that entire section in the row
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes",
            Justification = "Should never compare tuples")]
        private struct Tuple
        {
            /// <summary>
            /// Minimum value for index
            /// </summary>
            public const sbyte MinIndex = -1;

            /// <summary>
            /// Maximum value for index 
            /// </summary>
            public const sbyte MaxIndex = 127;

            /// <summary>
            /// All pixels represented by this tuple have this property index
            /// </summary>
            private sbyte index;

            /// <summary>
            /// the number of occurances the data this touple represents there are for this section
            /// </summary>
            private ushort count;

            /// <summary>
            /// Creates a new Touple with the given Identifier
            /// </summary>
            /// <param name="id">All pixels represented by this tuple have this property index</param>
            public Tuple(sbyte id)
            {
                Debug.Assert((MinIndex <= id) && (id <= MaxIndex));
                this.index = id;
                this.count = 1;
            }

            /// <summary>
            /// Gets the Touple's unique identifier
            /// </summary>
            public sbyte Index { get { return index; } }

            /// <summary>
            /// Gets the number of occurances the data this touple represents there are for this section
            /// </summary>
            public ushort Count { get { return count; } set { count = value; } }
        }

        #region Unit Tests

        /// <summary>
        /// Run set of tests
        /// </summary>
        [Conditional("DEBUG")]
        public static void RunTests()
        {
            TestCoordConversions();
            Xenocide.GameState.SetToStartGameCondition();
            TestLatLongBoundaries();
            TestRegionRoundTrip();
            TestLandRegionRoundTrip();
            TestCountryRoundTrip();
        }

        /// <summary>
        /// Test converting from XY to Geoposition (and back)
        /// </summary>
        [Conditional("DEBUG")]
        public static void TestCoordConversions()
        {
            // fake up a bitmap for testing.  NEVER do it this way for a real one.
            GeoBitmap map = new GeoBitmap("", false);
            map.height = map.width = 4;
            for (int x = 0; x < map.width; ++x)
            {
                for (int y = 0; y < map.height; ++y)
                {
                    GeoPosition pos = map.FromXY(x, y);
                    Vector2 final = map.ToXY(pos);
                    Debug.Assert((x == (int)final.X) && (y == (int)final.Y));
                }
            }
        }

        /// <summary>
        /// Test random region round-trips.
        /// </summary>
        [Conditional("DEBUG")]
        public static void TestRegionRoundTrip()
        {
            for (int i = 0; i < 1000; i++)
            {
                foreach (PlanetRegion r in Xenocide.GameState.GeoData.Planet.AllRegions)
                {
                    GeoPosition pos = Xenocide.GameState.GeoData.Planet.GetRandomPositionInRegion(r);
                    Debug.Assert(Xenocide.GameState.GeoData.Planet.GetRegionAtLocation(pos) == r);
                }
            }
        }

        /// <summary>
        /// Test random land region round-trips.
        /// </summary>
        [Conditional("DEBUG")]
        public static void TestLandRegionRoundTrip()
        {
            for (int i = 0; i < 1000; i++)
            {
                foreach (PlanetRegion r in Xenocide.GameState.GeoData.Planet.AllRegions)
                {
                    GeoPosition pos = Xenocide.GameState.GeoData.Planet.GetRandomLandPositionInRegion(r);
                    Debug.Assert(Xenocide.GameState.GeoData.Planet.GetRegionAtLocation(pos) == r);
                    Debug.Assert(!Xenocide.GameState.GeoData.Planet.IsPositionOverWater(pos));
                }
            }
        }

        /// <summary>
        /// Test random country round trips.
        /// </summary>
        [Conditional("DEBUG")]
        public static void TestCountryRoundTrip()
        {
            for (int i = 0; i < 1000; i++)
            {
                foreach (Country c in Xenocide.GameState.GeoData.Planet.AllCountries)
                {
                    GeoPosition pos = Xenocide.GameState.GeoData.Planet.GetRandomPositionInCountry(c);
                    Debug.Assert(Xenocide.GameState.GeoData.Planet.GetCountryAtLocation(pos) == c);
                }
            }
        }        

        /// <summary>
        /// Test the Lat/Long "corners", and a couple of cities
        /// </summary>
        [Conditional("DEBUG")]
        public static void TestLatLongBoundaries()
        {
            GeoPosition[] locations = { 
                new GeoPosition(GeoPosition.MinLongitude, 0),
                new GeoPosition(GeoPosition.MaxLongitude, 0),
                new GeoPosition(0,                        GeoPosition.MinLatitude),
                new GeoPosition(0,                        GeoPosition.MaxLatitude),
                new GeoPosition(GeoPosition.MaxLongitude, GeoPosition.MinLatitude),
                new GeoPosition(GeoPosition.MaxLongitude, GeoPosition.MaxLatitude),
                new GeoPosition(GeoPosition.MinLongitude, GeoPosition.MinLatitude),
                new GeoPosition(GeoPosition.MinLongitude, GeoPosition.MaxLatitude),
                new GeoPosition(0,                             MathHelper.ToRadians(51.48F)),         // london
                new GeoPosition(MathHelper.ToRadians(-47.92F), MathHelper.ToRadians(-15.87F)),        // brasilia
            };
            foreach (GeoPosition pos in locations)
            {
                try
                {
                    Xenocide.GameState.GeoData.Planet.IsPositionOverWater(pos);
                }
                catch (IndexOutOfRangeException)
                {
                    Debug.Assert(false, Util.StringFormat("Exception thrown checking location. {0}", pos.ToString()));
                }
            }
        }

        #endregion
    }
}
