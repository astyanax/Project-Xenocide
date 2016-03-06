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
* @file ScoreLog.cs
* @date Created: 2007/07/29
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using ProjectXenocide.Model;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Geography;

using CeGui;

#endregion

namespace ProjectXenocide.Model
{
    /// <summary>
    /// Make it easier to specify which set of scores we're after
    /// </summary>
    public enum Participant
    {
        /// <summary>
        /// The X-Corp scores
        /// </summary>
        XCorp,

        /// <summary>
        /// The Alien scores
        /// </summary>
        Alien,
    }

    /// <summary>
    /// Records the alien and X-Corp scores, on a per month basis for
    /// this month and the preceeding 11.
    /// </summary>
    [Serializable]
    public class ScoreLog
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ScoreLog()
        {
            scores = new MonthlyLog[2];
            scores[(int)Participant.XCorp] = new MonthlyLog();
            scores[(int)Participant.Alien] = new MonthlyLog();
        }

        /// <summary>
        /// Retreive monthly scores for one of the sides
        /// </summary>
        /// <param name="side">side to get the scores for</param>
        /// <returns>the scores</returns>
        public MonthlyLog this[Participant side] { get { return scores[(int)side]; } }

        /// <summary>
        /// Increase the monthly score of one of the sides
        /// </summary>
        /// <param name="points">Amount to increase score by</param>
        /// <param name="side">Side to increase score of</param>
        public void AddScore(Participant side, float points)
        {
            scores[(int)side][MonthlyLog.ThisMonth] += (int)points;
        }

        /// <summary>
        /// Set to begining of month state.
        /// Basically, set all scores for this month to 0.
        /// </summary>
        public void StartOfMonth()
        {
            int ThisMonthIndex = MonthlyLog.ThisMonth;
            foreach (MonthlyLog log in scores)
            {
                log[ThisMonthIndex] = 0;
            }
        }

        /// <summary>
        /// Return the player's points minus the Alien's points for given month
        /// </summary>
        /// <param name="month">the month.  See Monthly log</param>
        /// <returns>net score</returns>
        public int NetScore(int month)
        {
            return scores[(int)Participant.XCorp][month] - scores[(int)Participant.Alien][month];
        }

        #region Fields
        
        /// <summary>
        /// The actual scores
        /// </summary>
        private MonthlyLog[] scores;

        #endregion Fields

        #region UnitTests

        /// <summary>
        /// Run set of tests
        /// </summary>
        [Conditional("DEBUG")]
        public static void RunTests()
        {
            Xenocide.GameState.GeoData.SetToStartGameCondition();
            TestPointsToCountry();
            TestPointsToRegion();
        }

        /// <summary>
        /// Check adding points to a country
        /// </summary>
        [Conditional("DEBUG")]
        public static void TestPointsToCountry()
        {
            // Location of France
            GeoPosition pos = new GeoPosition(0.0f, 0.8f);
            Country country = Xenocide.GameState.GeoData.Planet.GetCountryAtLocation(pos);
            PlanetRegion region = Xenocide.GameState.GeoData.Planet.GetRegionAtLocation(pos);
            Debug.Assert((country != null) && (region != null));

            Xenocide.GameState.GeoData.AddScore(Participant.Alien, 10, pos);
            Debug.Assert(country.ScoreLog[Participant.Alien][MonthlyLog.ThisMonth] == 10);
            Debug.Assert(country.ScoreLog[Participant.XCorp][MonthlyLog.ThisMonth] == 0);
            Debug.Assert(region.ScoreLog[Participant.Alien][MonthlyLog.ThisMonth] == 10);
            Debug.Assert(region.ScoreLog[Participant.XCorp][MonthlyLog.ThisMonth] == 0);
            Debug.Assert(Xenocide.GameState.GeoData.XCorp.TotalScores[Participant.Alien][MonthlyLog.ThisMonth] == 20);
            Debug.Assert(Xenocide.GameState.GeoData.XCorp.TotalScores[Participant.XCorp][MonthlyLog.ThisMonth] == 0);

            Xenocide.GameState.GeoData.StartOfMonth();
            Debug.Assert(country.ScoreLog[Participant.Alien][MonthlyLog.ThisMonth] == 0);
            Debug.Assert(country.ScoreLog[Participant.XCorp][MonthlyLog.ThisMonth] == 0);
            Debug.Assert(region.ScoreLog[Participant.Alien][MonthlyLog.ThisMonth] == 0);
            Debug.Assert(region.ScoreLog[Participant.XCorp][MonthlyLog.ThisMonth] == 0);
            Debug.Assert(Xenocide.GameState.GeoData.XCorp.TotalScores[Participant.Alien][MonthlyLog.ThisMonth] == 0);
            Debug.Assert(Xenocide.GameState.GeoData.XCorp.TotalScores[Participant.XCorp][MonthlyLog.ThisMonth] == 0);
        }

        /// <summary>
        /// Check adding points to a region
        /// </summary>
        [Conditional("DEBUG")]
        public static void TestPointsToRegion()
        {
            // Atlantic ocean
            GeoPosition pos = new GeoPosition(0.0f, 0.0f);
            Debug.Assert(Xenocide.GameState.GeoData.Planet.GetCountryAtLocation(pos) == null);
            PlanetRegion region = Xenocide.GameState.GeoData.Planet.GetRegionAtLocation(pos);
            Debug.Assert(region != null);
            Xenocide.GameState.GeoData.AddScore(Participant.XCorp, 5, pos);
            Debug.Assert(region.ScoreLog[Participant.Alien][MonthlyLog.ThisMonth] == 0);
            Debug.Assert(region.ScoreLog[Participant.XCorp][MonthlyLog.ThisMonth] == 5);
            Debug.Assert(Xenocide.GameState.GeoData.XCorp.TotalScores[Participant.Alien][MonthlyLog.ThisMonth] == 0);
            Debug.Assert(Xenocide.GameState.GeoData.XCorp.TotalScores[Participant.XCorp][MonthlyLog.ThisMonth] == 5);
            
            // adding to existing score
            Xenocide.GameState.GeoData.AddScore(Participant.XCorp, 5, pos);
            Debug.Assert(region.ScoreLog[Participant.Alien][MonthlyLog.ThisMonth] == 0);
            Debug.Assert(region.ScoreLog[Participant.XCorp][MonthlyLog.ThisMonth] == 10);
            Debug.Assert(Xenocide.GameState.GeoData.XCorp.TotalScores[Participant.Alien][MonthlyLog.ThisMonth] == 0);
            Debug.Assert(Xenocide.GameState.GeoData.XCorp.TotalScores[Participant.XCorp][MonthlyLog.ThisMonth] == 10);
        }

        #endregion UnitTests
    }
}
