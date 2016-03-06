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
* @file GeoPosition.cs
* @date Created: 2007/02/04
* @author File creator: dteviot
* @author Credits: reist
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Xml.XPath;

using Microsoft.Xna.Framework;

using ProjectXenocide.Utils;
using ProjectXenocide.Model.Geoscape.Geography;

#endregion

namespace ProjectXenocide.Model.Geoscape
{
    /// <summary>
    /// Represents a Postion on the globe
    /// </summary>
    [Serializable]
    public class GeoPosition
    {
        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public GeoPosition() : this(0.0f, 0.0f) { }

        /// <summary>
        /// Copy constructor
        /// </summary>
        public GeoPosition(GeoPosition rhs) : this(rhs.longitude, rhs.latitude) {}

        /// <summary>
        /// Radian Longitude, Latitude constructor.
        /// </summary>
        /// <param name="longitude">in radians</param>
        /// <param name="latitude">in radians</param>
        public GeoPosition(float longitude, float latitude)
        {
            CheckBounds(longitude, latitude);
            this.longitude = longitude;
            this.latitude = latitude;
        }

        /// <summary>
        /// Create from XML node
        /// </summary>
        /// <param name="node">XML node holding data to construct geoposition</param>
        public GeoPosition(XPathNavigator node)
            :this(
                Util.GetDegreesAttribute(node, "longitude"),
                Util.GetDegreesAttribute(node, "latitude")
            )
        {
        }

        /// <summary>
        /// The longitude (X - Z plane) in radians 
        /// </summary>
        public float Longitude { get { return longitude; } set { longitude = value; } }

        /// <summary>
        /// The latitude, (Y) in radians 
        /// </summary>
        public float Latitude { get { return latitude; } set { latitude = value; } }

        /// <summary>
        /// Return the position as polar co-ordinates
        /// </summary>
        public Vector3 Polar { get { return new Vector3(longitude, latitude, 1.0f); } }

        /// <summary>
        /// Return the position as cartesian co-ordinates
        /// </summary>
        public Vector3 Cartesian { get { return PolarToCartesian(longitude, latitude); } }

        /// <summary>
        /// Get position in a format suitable for display
        /// </summary>
        /// <returns>GeoPosition in a format suitable for display</returns>
        public override string ToString() 
        { 
            return Util.StringFormat("({0}, {1})", MathHelper.ToDegrees(longitude), MathHelper.ToDegrees(latitude)); 
        }
        #endregion

        #region Public (instance) methods
        /// <summary>
        /// Calcuate the distance between this Geoposition and another 
        /// </summary>
        /// <param name="position">The other geoposition to examine</param>
        /// <returns>distance, in radians</returns>
        /// <remarks>Code was copied from original Xenocide, which was copied from
        /// http://www.codeguru.com/Cpp/Cpp/algorithms/general/article.php/c5115/
        /// http://williams.best.vwh.net/avform.htm
        /// </remarks>
        public float Distance(GeoPosition position)
        {
            double dlon = position.longitude - longitude;
            double dlat = position.latitude - latitude;
            double a = Math.Sin(dlat / 2.0f) * Math.Sin(dlat / 2.0f) + Math.Cos(latitude) * Math.Cos(position.latitude) * Math.Sin(dlon / 2.0f) * Math.Sin(dlon / 2.0f);
            return (float)(2.0 * Math.Asin(MathHelper.Min(1.0f, (float)Math.Sqrt(a))));
        }

        /// <summary>
        /// Check if a position is within specified distance of this position
        /// </summary>
        /// <param name="position">position to check</param>
        /// <param name="distance">distance (in radians)</param>
        /// <returns>true if within distance</returns>
        public bool IsWithin(GeoPosition position, float distance)
        {
            // short circuit test for easy case
            // 2x faster for Tachyon detector, and 6x faster for craft
            if (distance < Math.Abs(position.latitude - latitude))
            {
                return false;
            }
            else
            {
                return (Distance(position) < distance);
            }
        }

        /// <summary>
        /// Compute the relative bearing from this position to another position
        /// </summary>
        /// <param name="position">the other position</param>
        /// <returns>Bearing, in radians</returns>
        public float GetAzimuth(GeoPosition position)
        {
            float result = 0.0f;
        
            if(Math.Cos(latitude) < EPSILON) //are we at one of the poles?
            {
                if(latitude > 0) {
                    result = (float)Math.PI; //at north pole, going south
                } else {
                    result = (float)-Math.PI; //at south pole, going north
                }
            }
            else
            {
                float d = Distance(position);
                if (Math.Sin(d) < EPSILON)
                {
                    result = 0.0f;
                }
                else
                {
                    float maker = (float)((Math.Sin(position.latitude) - Math.Sin(latitude) * Math.Cos(d)) / (Math.Sin(d) * Math.Cos(latitude)));
                    if (maker - EPSILON < -1.0)
                    {
                        maker = -1.0f;
                    }
                    else if (maker + EPSILON > 1.0)
                    {
                        maker = 1.0f;
                    }
                    result = (float)Math.Acos(maker);
                    if (0 <= Math.Sin(position.longitude - longitude))
                    {

                        result = (float)(2.0 * Math.PI - result);
                    }
                }
            }
        
            return result;
        }

        /// <summary>
        /// Calcuate the GeoPosition that is at specified bearing and distance from here
        /// </summary>
        /// <param name="azimuth">Bearning in radians. 0 == north</param>
        /// <param name="distance">Distance, in radians</param>
        /// <returns>The calcuated GeoPosition</returns>
        public GeoPosition GetEndpoint(double azimuth, double distance)
        {
            //convert everything to double, to minimize precision errors
        
            //we need radial distances
            double latitudeD = latitude;

            double tmp = Math.Cos(latitudeD) * Math.Sin(distance);

            double lat = Math.Asin(Math.Sin(latitudeD) * Math.Cos(distance) + tmp * Math.Cos(azimuth));

            double lon = longitude - Math.Atan2(Math.Sin(azimuth) * tmp, Math.Cos(distance) - Math.Sin(latitudeD) * Math.Sin(lat));
        
            while(lon > Math.PI) {
                lon -= 2.0 * Math.PI;
            }
            while (lon < -Math.PI)
            {
                lon += 2.0 * Math.PI;
            }

            return new GeoPosition((float)lon, (float)lat);
        }

        /// <summary>
        /// Pick a random location a specified distance from a given location
        /// </summary>
        /// <param name="distance">distance (in radians)</param>
        /// <returns>a random position on the globe</returns>
        public GeoPosition RandomLocation(double distance)
        {
            double azimuth = Math.PI * 2.0 * Xenocide.Rng.Next(360) / 360.0;
            return GetEndpoint(azimuth, distance);
        }

        /// <summary>
        /// Pick a random location a specified distance from a given location
        /// </summary>
        /// <param name="km">distance in kilometers</param>
        /// <returns>a random position on the globe</returns>
        public GeoPosition RandomLocationDistantBykm(double km)
        {
            return RandomLocation(KilometersToRadians(km));
        }

        /// <summary>
        /// Test if another geoposition is the same location as this
        /// </summary>
        /// <param name="obj">other position to compare with this</param>
        /// <returns>true if the two positions are the same</returns>
        public override bool Equals(Object obj)
        {
            // error cases
            if ((null == obj) || (obj.GetType() != this.GetType()))
            {
                return false;
            }

            // check if they're the same
            GeoPosition pos = obj as GeoPosition;
            return ((latitude == pos.latitude) && (longitude == pos.longitude));
        }

        /// <summary>
        /// Overrode Equals, so need to re-do hash as well
        /// </summary>
        /// <returns>hash code</returns>
        public override int GetHashCode()
        {
            const double scale = int.MaxValue / Math.PI;
            return ((int)(latitude * scale) ^ (int)(longitude * scale));
        }

        /// <summary>
        /// Find item (in a list) that is closest to this GeoPosition
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">list of items</param>
        /// <param name="maxDistance">search radius (radians). only consider items nearer than this</param>
        /// <returns>item found, or null if none found</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
           Justification = "FxCop bug, reporting false positive")]
        public T FindClosest<T>(IEnumerable<T> list, double maxDistance) where T : class, IGeoPosition
        {
            double distance = maxDistance;
            T      nearest  = null;
            foreach (T candidate in list)
            {
                double newDistance = Distance(candidate.Position);
                if ((newDistance < distance) && candidate.IsKnownToXCorp)
                {
                    nearest  = candidate;
                    distance = newDistance;
                }
            }
            return nearest;
        }

        #endregion
    
        #region Public Static Members
        /// <summary>
        /// Returns the 3D cartesian co-ordinates equivelent to a set of 3D polar co-ords
        /// (Where globe's equator is on the X-Z plane, and longitute 0 is on +ve Z axis)
        /// </summary>
        /// <param name="polarCoords">The polar co-ordinates</param>
        /// <returns></returns>
        public static Vector3 PolarToCartesian(Vector3 polarCoords)
        {
            return PolarToCartesian(polarCoords.X, polarCoords.Y) * polarCoords.Z;
        }

        /// <summary>
        /// Returns the 3D cartesian co-ordinates
        /// of position on sphere of radius 1 in polar co-ordinates
        /// Where globe's equator is on the X-Z plane, and longitute 0 is on +ve Z axis
        /// </summary>
        /// <param name="longitude">The longitude, in radians</param>
        /// <param name="latitude">The latitude, in radians</param>
        /// <returns></returns>
        public static Vector3 PolarToCartesian(float longitude, float latitude)
        {
            float y = (float)(Math.Sin(latitude));
            double c = Math.Cos(latitude);
            float x = (float)(c * Math.Sin(longitude));
            float z = (float)(c * Math.Cos(longitude));

            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Convert a distance in Kilometers to radians (assumes we're on earth)
        /// </summary>
        /// <param name="kilometers">distance in kilometers</param>
        /// <returns>distance in radians</returns>
        public static double KilometersToRadians(double kilometers)
        {
            return kilometers / EarthRadiusInkm;
        }

        /// <summary>
        /// Convert a distance in Nautical Miles to radians (assumes we're on earth)
        /// </summary>
        /// <param name="knots">distance in knots</param>
        /// <returns>distance in radians</returns>
        public static double KnotsToRadians(double knots)
        {
            return KilometersToRadians(knots * 1.852);
        }

        /// <summary>
        /// Convert a distance in radians to Kilometers (assumes we're on earth)
        /// </summary>
        /// <param name="radians">distance in radians</param>
        /// <returns>distance in kilometers</returns>
        public static double RadiansToKilometers(double radians)
        {
            return radians * EarthRadiusInkm;
        }

        /// <summary>
        /// Pick a random point on the globe
        /// </summary>
        /// <returns>a random position on the globe</returns>
        public static GeoPosition RandomLocation()
        {
            return  new GeoPosition(
                ((Xenocide.Rng.Next(32760) - 16380) / 16380.0f) * (float)Math.PI,
                ((Xenocide.Rng.Next(32760) - 16380) / 32760.0f) * (float)Math.PI
           );
        }

        /// <summary>
        /// Maximum value for longitude
        /// </summary>
        public const float MaxLongitude = (float)Math.PI;

        /// <summary>
        /// Minimum value for longitude
        /// </summary>
        public const float MinLongitude = (float)(Math.PI * -1.0);

        /// <summary>
        /// Maximum value for latitude
        /// </summary>
        public const float MaxLatitude = (float)(Math.PI * 0.5);

        /// <summary>
        /// Minimum value for latitude
        /// </summary>
        public const float MinLatitude = (float)(Math.PI * -0.5);

        /// <summary>
        /// The earth's radius.  (to 1st order aproximation)
        /// </summary>
        public const double EarthRadiusInkm = 6400;

        #endregion

        #region private members and consts
        /// <summary>
        /// The longitude (X - Z plane) in radians 
        /// </summary>
        private float longitude;

        /// <summary>
        /// The latitude, (Y) in radians 
        /// </summary>
        private float latitude;

        private const float EPSILON = 0.0001f;

        #endregion

        /// <summary>
        /// Verify that given co-ordinates are a position on a sphere
        /// </summary>
        /// <param name="longitude">in radians</param>
        /// <param name="latitude">in radians</param>
        [Conditional("DEBUG")]
        public static void CheckBounds(float longitude, float latitude)
        {
            Debug.Assert(!Single.IsNaN(longitude) && !Single.IsNaN(latitude));

            if (longitude < MinLongitude || MaxLongitude < longitude)
            {
                string err = Util.StringFormat("Longitude is expressed in radians and has a range of {0} to {1}", MinLongitude, MaxLongitude);
                throw new ArgumentException(err);
            }

            if (latitude < MinLatitude || MaxLatitude < latitude)
            {
                string err = Util.StringFormat("Latitude is expressed in radians and has a range of {0} to {1}", MinLatitude, MaxLatitude);
                throw new ArgumentException(err);
            }
        }

        #region private methods

        #endregion

        #region UnitTests

        /// <summary>
        /// Run set of tests
        /// </summary>
        [Conditional("DEBUG")]
        public static void RunTests()
        {
            TestDistance();
            TestAzimuth();
            TestEquals();
            TestIsWithinRange((float)GeoPosition.KilometersToRadians(4448));
            TestIsWithinRange((float)GeoPosition.KnotsToRadians(700));
        }

        /// <summary>
        /// Check Distance calculations
        /// </summary>
        [Conditional("DEBUG")]
        private static void TestDistance()
        {
            // distance between poles
            GeoPosition northPole = new GeoPosition(0, (float)Math.PI * 0.5f);
            GeoPosition southPole = new GeoPosition((float)Math.PI, (float)Math.PI * -0.5f);
            CheckIsCloseTo(northPole.Distance(southPole), Math.PI);

            // same point
            CheckIsCloseTo(northPole.Distance(northPole), 0.0);

            // distance between pole and the equator
            GeoPosition origin = new GeoPosition(0, 0);
            CheckIsCloseTo(origin.Distance(southPole), (float)Math.PI * 0.5f);
        }

        /// <summary>
        /// Check Distance calculations
        /// </summary>
        [Conditional("DEBUG")]
        private static void TestAzimuth()
        {
            GeoPosition start = new GeoPosition(0.1f, 0.1f);
            // with old code, this would produce NaN
            Debug.Assert(start.GetAzimuth(start) == 0.0f);
        }

        /// <summary>
        /// Check is distance less than calcs.
        /// </summary>
        /// <param name="distance">distance (in radians) to use for testing IsWithin()</param>
        [Conditional("DEBUG")]
        private static void TestIsWithinRange(float distance)
        {
            // generate a series of random positions
            const int numPositions = 100;
            GeoPosition[] points = new GeoPosition[numPositions];
            for (int i = 0; i < numPositions; ++i)
            {
                points[i] = GeoPosition.RandomLocation();
            }

            // check distance between each pair of points
            foreach (GeoPosition p1 in points)
            {
                foreach (GeoPosition p2 in points)
                {
                    if (p1.IsWithin(p2, distance) != (p1.Distance(p2) < distance))
                    {
                        Debug.Assert(false);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Test Equals() 
        /// </summary>
        [Conditional("DEBUG")]
        private static void TestEquals()
        {
            GeoPosition start = new GeoPosition(0.1f, 0.1f);
            GeoPosition same  = new GeoPosition(0.1f, 0.1f);;
            GeoPosition far   = new GeoPosition(0.1f, 0.2f);;
            Debug.Assert(start.Equals(same));
            Debug.Assert(!start.Equals(far));
            Debug.Assert(!start.Equals(null));
            Debug.Assert(!start.Equals("a string"));

            GeoPosition abuse = new GeoPosition((float)Math.PI, (float)(Math.PI / 2.0f));
            Debug.Assert(0 != abuse.GetHashCode());
        }
        
        /// <summary>
        /// Return Throws if the two parameters are not almost the same
        /// (used to compare doubles for equality.)
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        [Conditional("DEBUG")]
        private static void CheckIsCloseTo(double lhs, double rhs)
        {
            Debug.Assert((Math.Abs(lhs - rhs) < 0.000001));
        }

        #endregion UnitTests
    }

    /// <summary>
    /// Mixin Interface, for objects that have a position on the Geoscape
    /// </summary>
    public interface IGeoPosition
    {
        /// <summary>
        /// Location of this object on the Geoscape
        /// </summary>
        GeoPosition Position { get; }

        /// <summary>
        /// Does X-Corp know about the position?
        /// </summary>
        bool IsKnownToXCorp { get; }
    }
}
