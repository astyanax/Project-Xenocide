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
* @file Item.cs
* @date Created: 2007/10/03
* @author File creator: David Cameron
* @author Credits: nil
*/
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;

using ProjectXenocide.Utils;
using Xenocide.Resources;


namespace ProjectXenocide.Model.StaticData.Items
{
    /// <summary>
    /// Holds information about possible names for characters, as defined in the given XML file.
    /// </summary>
    public class PeopleNames
    {
        /// <summary>
        /// Constructs an instance, parsing the possible names of of the given xml file. 
        /// 
        /// Expects a tree like
        /// &lt;database&gt;
        ///   &lt;namegroups&gt;
        ///     &lt;group&gt;
        ///       &lt;name&gt;&lt;/name&gt;
        ///       &lt;lastname&gt;&lt;/lastname&gt;
        ///     &lt;/group&gt;
        ///   &lt;/namegroups&gt;
        /// &lt;/database&gt;
        /// 
        /// The contents of the &lt;name&gt; and &lt;lastname&gt; tags become the possible nodes.
        /// </summary>
        /// <param name="filename">path to the file to parse (generally, a relative path)</param>
        public PeopleNames(string filename)
        {
            XPathNavigator nav = (new XPathDocument(filename)).CreateNavigator();
            IList<string> countriesWithDupes = ParseAllValuesFromXPath(nav, "/database/namegroups/group[count(name) > 0 and count(lastname) > 0]/@origin");

            countries = new List<string>();
            foreach (string country in countriesWithDupes)
            {
                if (!countries.Contains(country))
                {
                    countries.Add(country);
                }
            }


            foreach (string country in countries)
            {
                IList<string> givenNamess;// = ParseAllValuesFromXPath(nav, "/database/namegroups/group/name");
                IList<string> familyNamess;// = ParseAllValuesFromXPath(nav, "/database/namegroups/group/lastname");

                givenNamess = ParseAllValuesFromXPath(nav, "/database/namegroups/group[@origin = '" + country + "']/name");
                familyNamess = ParseAllValuesFromXPath(nav, "/database/namegroups/group[@origin = '" + country + "']/lastname");

                givenNamesByCountry.Add(country, givenNamess);
                familyNamesByCountry.Add(country, familyNamess);

            }
        }

        /// <summary>
        /// Create a name for a person based on the people-names.xml data files.
        /// </summary>
        /// <returns>the created name</returns>
        public String CreatePersonName()
        {
            string country = PickRandomItem(countries);
            return Util.StringFormat(Strings.PERSON_NAME_ORDER, PickRandomItem(givenNamesByCountry[country]), PickRandomItem(familyNamesByCountry[country]));
        }

        /// <summary>
        /// Pick a string at random from a list of strings
        /// </summary>
        /// <param name="list">List of strings to pick from</param>
        /// <returns>a string</returns>
        private static String PickRandomItem(IList<String> list)
        {
            return list[Xenocide.Rng.Next(list.Count)];
        }

        /// <summary>
        /// Extract values of elements satisfying an XPath from an XML document
        /// </summary>
        /// <param name="nav">Navigator for XML document</param>
        /// <param name="xPath">XPath the elements must satisfy</param>
        /// <returns>the values</returns>
        private static IList<String> ParseAllValuesFromXPath(XPathNavigator nav, string xPath)
        {
            IList<String> valuesList = new List<String>();

            // Extract all the given tags from the file, ignoring groups and so on
            foreach (XPathNavigator element in nav.Select(xPath))
            {
                if (!String.IsNullOrEmpty(element.Value))
                {
                    valuesList.Add(element.Value);
                }
            }

            return valuesList;
        }

        #region Fields

        /// <summary>
        /// Countries (origins) to choose from
        /// </summary>
        private IList<String> countries;

        /// <summary>
        /// Given names arranged by country
        /// </summary>
        private IDictionary<string, IList<string>> givenNamesByCountry = new Dictionary<string, IList<string>>();

        /// <summary>
        /// Family names arranged by country
        /// </summary>
        private IDictionary<string, IList<string>> familyNamesByCountry = new Dictionary<string, IList<string>>();

        #endregion Fields
    }
}
