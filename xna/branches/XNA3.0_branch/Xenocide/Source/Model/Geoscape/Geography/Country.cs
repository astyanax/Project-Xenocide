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
* @file Country.cs
* @date Created: 2007/06/03
* @author File creator: dteviot
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


using ProjectXenocide.Utils;
using ProjectXenocide.Model;
using ProjectXenocide.Model.Geoscape;

using CeGui;

#endregion

namespace ProjectXenocide.Model.Geoscape.Geography
{
	/// <summary>
	/// The attitiude of countries to X-Corp
	/// </summary>
	public enum CountryAttitude
	{
		/// <summary>
		/// Country is neither happy or unhappy with X-Corp
		/// </summary>
		Neutral,

		/// <summary>
		/// Country is pleased with X-Corp
		/// </summary>
		Happy,

		/// <summary>
		/// Country is not pleased with X-Corp
		/// </summary>
		Unhappy,

		/// <summary>
		/// Country has signed an alliance with Aliens
		/// </summary>
		Hostile
	}
	
	/// <summary>
    /// The information for a country in the Geoscape
    /// </summary>
    [Serializable]
    public class Country : IGeoBitmapProperty
    {
        /// <summary>
        /// Construct a Country from an XML file
        /// </summary>
        /// <param name="countryNode">XML node holding data to construct country</param>
        /// <param name="manager">Namespace used in planets.xml</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if countryNode == null")]
        public Country(XPathNavigator countryNode, XmlNamespaceManager manager)
        {
            this.name        = Util.GetStringAttribute(countryNode, "name");

            XPathNavigator funds = countryNode.SelectSingleNode("p:funds", manager);
            this.fundingSeed = Util.GetIntAttribute(funds, "seed");
            this.fundingCap  = Util.GetIntAttribute(funds, "cap");

            this.colorKey    = Util.GetColorKey(countryNode, manager);
        }

        /// <summary>
        /// Do start of month processing
        /// </summary>
        public void StartOfMonth()
        {
            UpdateAttitude();
			funds[MonthlyLog.ThisMonth] = CalcNewFunding();
            ScoreLog.StartOfMonth();
        }

        /// <summary>
        /// Tag this country as having been infiltrated
        /// </summary>
        public void OnInfiltrated()
        {
            attitude = CountryAttitude.Hostile;
        }

		/// <summary>
		/// Set attitude for this month, based on X-Corp and UFO activity
		/// </summary>
		private void UpdateAttitude()
		{
            // if country is hostile, it's going to stay hostile
            if (CountryAttitude.Hostile != attitude)
            {
                attitude = CountryAttitude.Neutral;
                int alienScore = ScoreLog[Participant.Alien][MonthlyLog.LastMonth];
                int xcorpScore = ScoreLog[Participant.XCorp][MonthlyLog.LastMonth];

                // if aliens are even slightly better than X-Corp, country isn't happy
                // X-Corp must be at least 100 points more than aliens for country to be happy
                if (xcorpScore < alienScore)
                {
                    attitude = CountryAttitude.Unhappy;
                }
                else if (alienScore + 100 < xcorpScore)
                {
                    attitude = CountryAttitude.Happy;
                }
            }
		}
		
		/// <summary>
        /// Figure out how much $ this country will give to X-Corp this month
        /// </summary>
        /// <returns>the amount</returns>
        private int CalcNewFunding()
        {
            float scale = 1.0f;
			switch (attitude)
			{
				case CountryAttitude.Neutral:
					// nothing more to do
					break;

				case CountryAttitude.Happy:
                    scale += Xenocide.Rng.Next(21) / 100.0f;
					break;

				case CountryAttitude.Unhappy:
                    scale -= Xenocide.Rng.Next(21) / 100.0f;
					break;

				case CountryAttitude.Hostile:
					scale = 0;
					break;

				default:
					Debug.Assert(false);
					break;
			}
            int newFunds = Util.RoundTo1000(funds[MonthlyLog.LastMonth] * scale);
            if (fundingCap < newFunds)
			{
                newFunds = fundingCap;
			}
            return newFunds;
		}

        /// <summary>
        /// override ToString
        /// </summary>
        /// <returns>name of country</returns>
        public override string ToString()
        {
            return name;
        }

        #region Fields

        /// <summary>
        /// Name of the country
        /// </summary>
        public String Name { get { return name; } }

		/// <summary>
		/// Seed used to calculate initial $ per month country gives X-Corp
		/// </summary>
		public int FundingSeed { get { return fundingSeed; } }

		/// <summary>
		/// The funds the country gave to X-Corp this month
		/// </summary>
		public MonthlyLog Funds { get { return funds; } }

        /// <summary>
        /// Alien and X-Corp scores for this country
        /// </summary>
        public ScoreLog ScoreLog { get { return scoreLog; } }

        /// <summary>
        /// Country's current attitude towards X-Corp, as viewable string
        /// </summary>
        public String Attitude { get { return Util.LoadString(attitudeDisplayStrings[(int)attitude]); } }

        /// <summary>
        /// Have the aliens infiltrated this country?
        /// </summary>
        public bool IsInfiltrated { get { return attitude == CountryAttitude.Hostile; } }

        /// <summary>
        /// Name of the country
        /// </summary>
        private String name;

        /// <summary>
        /// The funds the country gave to X-Corp this month and each of the last 11.
        /// </summary>
        private MonthlyLog funds = new MonthlyLog();

        /// <summary>
        /// Alien and X-Corp scores for this country
        /// </summary>
        private ScoreLog scoreLog = new ScoreLog();

        /// <summary>
		/// Seed used to calculate initial $ per month country gives X-Corp
		/// </summary>
		private int fundingSeed;

		/// <summary>
		/// Maximum $ per month country can give X-Corp
		/// </summary>
		private int fundingCap;

		/// <summary>
		/// Country's attitude to X-Corp
		/// </summary>
		private CountryAttitude attitude;

        /// <summary>
        /// RGB color associated with this country
        /// </summary>
        private uint colorKey;

        /// <summary>
        /// Number of pixels this country takes up in country bitmap
        /// </summary>
        private uint size;

        /// <summary>
        /// The names of the attitude enumerations, for display to user
        /// </summary>
        private readonly static String[] attitudeDisplayStrings =
        {
            "COUNTRY_ATTITUDE_NEUTRAL",
            "COUNTRY_ATTITUDE_HAPPY",
            "COUNTRY_ATTITUDE_UNHAPPY",
            "COUNTRY_ATTITUDE_HOSTILE"
        };

        #endregion Fields

        #region IBitmapMember Members        

        /// <summary>
        /// RGB value representing country on country bitmap file
        /// </summary>
        public uint ColorKey { get { return colorKey; } }

        /// <summary>
        /// Number of pixels this country takes up in country bitmap
        /// </summary>
        public uint Size { get { return size; } set { size = value; } }

        #endregion
    }
}
