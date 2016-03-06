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

using Microsoft.Xna.Framework;

using Xenocide.Utils;

#endregion

namespace Xenocide.Model.Geoscape
{
    /// <summary>
    /// Represents a Postion on the globe
    /// </summary>
    [Serializable]
    public class GeoPosition
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public GeoPosition() : this(0.0f, 0.0f) { }

        /// <summary>
        /// Copy constructor
        /// </summary>
        public GeoPosition(GeoPosition rhs) : this(rhs.longitude, rhs.latitude) {}

        /// <summary>
        /// Another constructor
        /// </summary>
        /// <param name="longitude">in radians</param>
        /// <param name="latitude">in radians</param>
        public GeoPosition(float longitude, float latitude)
        {
            this.longitude = longitude;
            this.latitude = latitude;
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

        /// <summary>
        /// Calcuate the distance between this Geoposition and another 
        /// </summary>
        /// <param name="position">The other geoposition to examine</param>
        /// <returns>distance, in radians</returns>
        /// <remarks>Code was copied from original Xenocide, which was copied from
        /// http://www.codeguru.com/Cpp/Cpp/algorithms/general/article.php/c5115/
        /// http://williams.best.vwh.net/avform.htm
        /// </remarks>
        public float GetDistance(GeoPosition position)
        {
            double dlon = position.longitude - longitude;
            double dlat = position.latitude - latitude;
            double a = Math.Sin(dlat / 2.0f) * Math.Sin(dlat / 2.0f) + Math.Cos(latitude) * Math.Cos(position.latitude) * Math.Sin(dlon / 2.0f) * Math.Sin(dlon / 2.0f);
            return (float)(2.0 * Math.Asin(MathHelper.Min(1.0f, (float)Math.Sqrt(a))));
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
                float d = (float)(2.0f*Math.Asin(Math.Sqrt(
                            (Math.Sin((latitude-position.latitude)/2.0f))*(Math.Sin((latitude-position.latitude)/2.0f)) + 
                            Math.Cos(latitude)*Math.Cos(position.latitude)
                            *(Math.Sin((longitude-position.longitude)/2))*(Math.Sin((longitude-position.longitude)/2))
                            )));
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
        /// Convert a distance in Kilometers to radians (assumes we're on earth
        /// </summary>
        /// <param name="kilometers">distance in kilometers</param>
        /// <returns>distance in radians</returns>
        public static double KilometersToRadians(double kilometers)
        {
            const double EarthRadiusInKm = 6400;
            return kilometers / EarthRadiusInKm;
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
        /// Pick a random location a specified distance from a given location
        /// </summary>
        /// <param name="origin">position to move from</param>
        /// <param name="distance">distance (in radians)</param>
        /// <returns>a random position on the globe</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods", Justification = "will throw if origin == null")]
        public static GeoPosition RandomLocation(GeoPosition origin, double distance)
        {
            double azimuth = Math.PI * 2.0 * Xenocide.Rng.Next(360) / 360.0;
            return origin.GetEndpoint(azimuth, distance);
        }
        
        /// <summary>
        /// The longitude (X - Z plane) in radians 
        /// </summary>
        private float longitude;

        /// <summary>
        /// The latitude, (Y) in radians 
        /// </summary>
        private float latitude;

        private const float EPSILON = 0.0001f;
    }
}
