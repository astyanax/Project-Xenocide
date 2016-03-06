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
* @file UnitTestPlanet.cs
* @date Created: 2007/08/27
* * @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using System.Xml.XPath;
using System.Threading;
using System.IO;
using System.Runtime.Serialization;


using ProjectXenocide.Utils;
using ProjectXenocide.Model;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.AI;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

#endregion

namespace ProjectXenocide.Model.Geoscape.Geography
{
    /// <remarks>Unit Tests for the Planet class</remarks>
    public partial class Planet
    {
        #region UnitTests

        /// <summary>
        /// Run set of tests
        /// </summary>
        [Conditional("DEBUG")]
        public static void RunTests()
        {
            TestSelectRandomRegion();
            TestSelectCountryToInfiltrate();
            TestGetClosestLand();
        }

        /// <summary>
        /// Test SelectRandomRegion()
        /// </summary>
        [Conditional("DEBUG")]
        public static void TestSelectRandomRegion()
        {
            Planet planet = PlanetParser.Parse(Xenocide.StaticTables.DataDirectory + "planets.xml");
            PlanetRegion region = planet.SelectRandomRegion();
            region.SelectRandomMission();
        }

        /// <summary>
        /// Test SelectCountryToInfiltrate() and ClearInfiltrationMissions()
        /// </summary>
        [Conditional("DEBUG")]
        public static void TestSelectCountryToInfiltrate()
        {
            Planet planet = PlanetParser.Parse(Xenocide.StaticTables.DataDirectory + "planets.xml");
            Debug.Assert(null != planet.SelectCountryToInfiltrate());
            foreach (Country country in planet.AllCountries)
            {
                country.OnInfiltrated();
            }
            Debug.Assert(null == planet.SelectCountryToInfiltrate());
            planet.ClearInfiltrationMissions();
        }

        /// <summary>
        /// Test SelectRandomRegion()
        /// </summary>
        [Conditional("DEBUG")]
        public static void TestGetClosestLand()
        {
            Planet planet = PlanetParser.Parse(Xenocide.StaticTables.DataDirectory + "planets.xml");
            for (int i = 0; i < 1000; i++)
            {
                long x = (long)Xenocide.Rng.Next(1350);
                long y = (long)Xenocide.Rng.Next(675);

                GeoPosition pos = planet.terrainBitmap.FromXY(x, y);
                pos = planet.GetClosestLand(pos);
                Debug.Assert(!planet.IsPositionOverWater(pos));
            }
        }

        #endregion UnitTests
    }
}
