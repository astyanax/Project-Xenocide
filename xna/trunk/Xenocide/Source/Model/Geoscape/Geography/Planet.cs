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
* @file Planet.cs
* @date Created: 2007/06/21
* @author File creator: Darkside
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
    /// <summary>
    /// Holds all the information for a planet
    /// </summary>
    [Serializable]
    public partial class Planet
    {
        /// <summary>
        /// Construct a planet from an XML file
        /// </summary>
        /// <param name="planetNode">XML element holding the planet element</param>        
        /// <param name="manager">Namespace used in planet.xml</param>
        public Planet(XPathNavigator planetNode, XmlNamespaceManager manager)
        {
            regions       = new List<PlanetRegion>();
            countries     = new List<Country>();
            cities        = new List<City>();
            terrains      = new List<Terrain>();
            landedRegions = new List<GeoBitmapProperty>();

            this.name           = Util.GetStringAttribute(planetNode, "name");
            this.countryBitmap  = new GeoBitmap(Util.GetStringAttribute(planetNode, "countryBitmap"), true);
            this.regionBitmap   = new GeoBitmap(Util.GetStringAttribute(planetNode, "regionBitmap"),  false);
            this.terrainBitmap  = new GeoBitmap(Util.GetStringAttribute(planetNode, "terrainBitmap"), false);

            this.landedRegionsBitmap = new LandMaskedGeoBitmap(Util.GetStringAttribute(planetNode, "regionBitmap"), false);

            // load lists of regions, countries, cities and terrain
            foreach (XPathNavigator regionNode in planetNode.Select("p:region", manager))
            {
                PlanetRegion region = new PlanetRegion(regionNode, manager);
                region.LandedProperty = new GeoBitmapProperty(region.ColorKey);
                AddRegion(region);
                landedRegions.Add(region.LandedProperty);
            }
            foreach (XPathNavigator countryNode in planetNode.Select("p:country", manager))
            {
                AddCountry(new Country(countryNode, manager));
            }
            foreach (XPathNavigator cityNode in planetNode.Select("p:city", manager))
            {
                AddCity(new City(cityNode, manager));
            }
            foreach (XPathNavigator terrainNode in planetNode.Select("p:terrain", manager))
            {
                terrains.Add(new Terrain(terrainNode, manager));
            }
            FindWaterTerrain();
            SetInitialFunding();
            LoadBitmaps();
        }

        /// <summary>
        /// Adds a region to this planet's list of regions
        /// </summary>
        /// <param name="region">the region to add</param>
        public void AddRegion(PlanetRegion region)
        {
            Debug.Assert(!IsColorKeyPresent(region.ColorKey, regions));
            regions.Add(region);
        }

        /// <summary>
        /// Adds a country to this planet's list of countries
        /// </summary>
        /// <param name="country">the country to add</param>
        public void AddCountry(Country country)
        {
            Debug.Assert(!IsColorKeyPresent(country.ColorKey, countries));
            countries.Add(country);
        }

        /// <summary>
        /// Adds the city to this planets list of all cities
        /// </summary>
        /// <param name="city">the city to add</param>
        public void AddCity(City city)
        {
            cities.Add(city);
        }

        /// <summary>
        /// Load and parse the GeoBitmaps so they are ready to be used
        /// </summary>
        public void LoadBitmaps()
        {
            regionBitmap.Load(regions);
            countryBitmap.Load(countries);
            terrainBitmap.Load(terrains);

            // synthesised from terrain and region Bitmaps
            landedRegionsBitmap.Load(landedRegions, terrainBitmap, waterIndex);
        }

        /// <summary>
        /// Compute the initial per month funding each country will give X-Corp 
        /// </summary>
        public void SetInitialFunding()
        {
            // start by setting funds to between 100% and 200% of seed
            int total = 0;
            int thisMonth = MonthlyLog.ThisMonth;
            foreach (Country c in countries)
            {
                int quantity = (int)(c.FundingSeed * (101.0 + Xenocide.Rng.Next(100)) / 100);
                c.Funds[thisMonth] = quantity;
                total += quantity;
            }

            // now normalize total to approx $6,000,000, with each country being multiple of 1k
            double scale = 6000000.0 / total;
            foreach (Country c in countries)
            {
                c.Funds[thisMonth] = Util.RoundTo1000(c.Funds[thisMonth] * scale);
            }
        }

        /// <summary>
        /// Do start of month processing
        /// </summary>
        public void StartOfMonth()
        {
            foreach (Country c in countries)
            {
                c.StartOfMonth();
            }
            foreach (PlanetRegion r in regions)
            {
                r.StartOfMonth();
            }
        }

        /// <summary>
        /// Adjust score
        /// </summary>
        /// <param name="side">which side is being changed</param>
        /// <param name="points">size of the change</param>
        /// <param name="location">location on geoscape of event</param>
        /// <returns>total points (points are double if location belongs to a country</returns>
        public float AddScore(Participant side, float points, GeoPosition location)
        {
            // Assign score to region
            GetRegionAtLocation(location).ScoreLog.AddScore(side, points);

            // see if location also belongs to a country (doubles the score)
            Country country = GetCountryAtLocation(location);
            if (null != country)
            {
                country.ScoreLog.AddScore(side, points);
                points += points;
            }
            return points;
        }

        /// <summary>
        ///  Retrieves a random region from this planet
        /// </summary>
        /// <returns>a random region</returns>
        public PlanetRegion SelectRandomRegion()
        {
            // first, find total options.
            int totalPriority = 0;
            foreach (PlanetRegion region in regions)
            {
                totalPriority += region.AlienAttackPriority;
            }
            Debug.Assert(0 < totalPriority);

            // now pick at random
            totalPriority = Xenocide.Rng.Next(totalPriority) + 1;
            for (int i = 0; i < regions.Count; ++i)
            {
                totalPriority -= regions[i].AlienAttackPriority;
                if (totalPriority <= 0)
                {
                    return regions[i];
                }
            }

            // If get here, something went wrong
            Debug.Assert(false);
            return regions[0];
        }

        /// <summary>
        /// Picks a random coordinate in the passed region
        /// </summary>
        /// <param name="region">The region to pick a random coordinate in</param>
        /// <returns>A random coordinate within the passed region</returns>
        public GeoPosition GetRandomPositionInRegion(PlanetRegion region)
        {
            return regionBitmap.GetRandomCoord(regions.IndexOf(region), region.Size);
        }

        /// <summary>
        /// Picks a random land coordinate in the passed region
        /// </summary>
        /// <param name="region">The region to pick a random coordinate in</param>
        /// <returns>A random coordinate within the passed region</returns>
        public GeoPosition GetRandomLandPositionInRegion(PlanetRegion region)
        {
            return landedRegionsBitmap.GetRandomCoord(regions.IndexOf(region), region.LandedProperty.Size);
        }

        /// <summary>
        ///  Retrieves a random country from this planet
        /// </summary>
        /// <returns>a random country</returns>
        public Country SelectRandomCountry()
        {
            return countries[Xenocide.Rng.Next(countries.Count)];
        }

        /// <summary>
        /// Randomly pick a country that hasn't been infiltrated
        /// </summary>
        /// <returns>the country, or null if all have been infiltrated</returns>
        public Country SelectCountryToInfiltrate()
        {
            // get list of uninfiltrated countries
            List<Country> free = new List<Country>();
            foreach (Country country in AllCountries)
            {
                if (!country.IsInfiltrated)
                {
                    free.Add(country);
                }
            }

            if (0 == free.Count)
            {
                return null;
            }
            else
            {
                return free[Xenocide.Rng.Next(free.Count)];
            }
        }

        /// <summary>
        /// Mark all regions as no longer supporting infiltration
        /// Call this when all countries have been infiltrated
        /// </summary>
        /// <remarks>this is a bit of a hack.  Problem is infiltration works on COUNTRIES, but mission type selection
        /// works on REGIONS, and we've decoupled countries from regions</remarks>
        public void ClearInfiltrationMissions()
        {
            foreach (PlanetRegion region in AllRegions)
            {
                region.ClearAlienMissionPriority(AlienMission.Infiltration);
            }
        }

        /// <summary>
        /// Picks a random coordinate in the passed country
        /// </summary>
        /// <param name="country">The country to pick a random coordinate in</param>
        /// <returns>A random coordinate within the passed country</returns>
        public GeoPosition GetRandomPositionInCountry(Country country)
        {
            return countryBitmap.GetRandomCoord(countries.IndexOf(country), country.Size);
        }

        /// <summary>
        /// Gets a random city from the list of all cities on the planet
        /// </summary>
        /// <returns>a random city from this planet</returns>
        public City SelectRandomCity()
        {
            return cities[Xenocide.Rng.Next(cities.Count)];
        }

        /// <summary>
        /// Discovers which region, if any, is under the given GeoPosition
        /// </summary>
        /// <param name="pos">The position we want to try finding the region of</param>
        /// <returns>A region which exists under the given position, or null if no match exists</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification="will throw if pos == null")]
        public PlanetRegion GetRegionAtLocation(GeoPosition pos)
        {
            return regions[regionBitmap.GetPropertyIndexOfLocation(pos)];
        }

        /// <summary>
        /// Discovers which country, if any, is under the given GeoPosition
        /// </summary>
        /// <param name="pos">The position we want to try finding the country of</param>
        /// <returns>A country which exists under the given position, or null if no match exists</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if pos == null")]
        public Country GetCountryAtLocation(GeoPosition pos)
        {
            int index = countryBitmap.GetPropertyIndexOfLocation(pos);
            return (GeoBitmap.NoProperty == index) ? null : countries[index];
        }

        /// <summary>
        /// Tests to see if the specified position lies over water
        /// </summary>
        /// <param name="pos">The GeoPosition contianing the coordinates to check</param>
        /// <returns>true if the position lies over water</returns>
        public bool IsPositionOverWater(GeoPosition pos)
        {
            return terrainBitmap.GetPropertyIndexOfLocation(pos) == waterIndex;
        }

        /// <summary>
        /// Takes a geo-position and attempts to work out the closest land
        /// </summary>
        /// <param name="pos"></param>
        /// <returns>'Closest' land position to input geo-position</returns>
        public GeoPosition GetClosestLand(GeoPosition pos)
        {
            return terrainBitmap.GetClosestLand(pos, waterIndex);
        }

        /// <summary>
        /// Override of ToString
        /// </summary>
        /// <returns>the name of this planet</returns>
        public override string ToString()
        {
            return name;
        }

        /// <summary>
        /// Check if a property has been assigned this color
        /// </summary>
        /// <param name="color">Color to test</param>
        /// <param name="properties">Set of properties to search</param>
        /// <returns>true if one of the properties is using the color</returns>
        private static bool IsColorKeyPresent<T>(uint color, List<T> properties) where T : IGeoBitmapProperty
        {
            foreach (IGeoBitmapProperty property in properties)
            {
                if (property.ColorKey == color)
                {
                    return true;
                }
            }
            // if get here, color was not present
            return false;
        }

        /// <summary>
        /// Restore static information that is not saved to file
        /// </summary>
        /// <param name="context">unused</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "context",
            Justification="StreamingContext required to match Serializable signature")] 
        [OnDeserialized]
        private void OnDeserializedMethod(StreamingContext context)
        {
            LoadBitmaps();
        }

        /// <summary>
        /// Find element of Terrain that contains water
        /// </summary>
        private void FindWaterTerrain()
        {
            for (int i = 0; i < terrains.Count; ++i)
            {
                if ("water" == terrains[i].Name)
                {
                    waterIndex = i;
                    return;
                }
            }

            // if get here, we didn't find any water
            Debug.Assert(false, "No water terrain found in planets.xml");
        }

        #region Fields

        /// <summary>
        /// Name of the planet
        /// </summary>
        public String Name { get { return name; } }

        /// <summary>
        /// List of all Regions on this planet
        /// </summary>
        public IList<PlanetRegion> AllRegions { get { return regions; } }

        /// <summary>
        /// List of all countries on this planet
        /// </summary>
        public IList<Country> AllCountries { get { return countries; } }

        /// <summary>
        /// List of all cities on this planet
        /// </summary>
        public IList<City> AllCities { get { return cities; } }
        
        /// <summary>
        /// Name of the planet
        /// </summary>
        private String name;

        /// <summary>
        /// List of all regions belonging to this planet
        /// </summary>
        private List<PlanetRegion> regions;

        /// <summary>
        /// All countries on the planet
        /// </summary>
        private List<Country> countries;

        /// <summary>
        /// All cities on the planet
        /// </summary>
        private List<City> cities;

        /// <summary>
        /// All terrains on the planet
        /// </summary>
        private List<Terrain> terrains;

        /// <summary>
        /// element of terrains that indicates water
        /// </summary>
        private int waterIndex;

        /// <summary>
        /// Data needed by landedRegionsBitmap
        /// </summary>
        private List<GeoBitmapProperty> landedRegions;

        /// <summary>
        /// the bitmap which defines all the regions
        /// </summary>
        private GeoBitmap regionBitmap;

        /// <summary>
        ///  the bitmap which defines all the countries
        /// </summary>
        private GeoBitmap countryBitmap;

        /// <summary>
        ///  the bitmap which defines all the terrains
        /// </summary>
        private GeoBitmap terrainBitmap;

        /// <summary>
        /// The land in each of the regions
        /// </summary>
        private LandMaskedGeoBitmap landedRegionsBitmap;

        #endregion Fields
    }
}
