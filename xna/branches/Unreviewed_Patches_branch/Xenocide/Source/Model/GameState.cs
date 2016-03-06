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
* @file GameState.cs
* @date Created: 2007/01/28
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Battlescape;

#endregion

namespace ProjectXenocide.Model
{
    /// <summary>
    /// The root class that holds all "model" (as in model-view) data that is loaded/saved to file
    /// </summary>
    [Serializable]
    public class GameState
    {
        /// <summary>
        /// Construct a new GameState
        /// </summary>
        public GameState()
        {
            //this will have to be moved to Initialize once we have load/save stuff inside a proper service
            geodata = new GeoData();
        }

        /// <summary>
        /// Set to "start of new game" condition
        /// </summary>
        public void SetToStartGameCondition()
        {
            GeoData.SetToStartGameCondition();
            battlescape = null;
        }

        #region Fields

        /// <summary>
        /// Data specific to the geoscape
        /// </summary>
        public GeoData GeoData { get { return geodata; } }

        /// <summary>
        /// The current battlescape (if there is one)
        /// </summary>
        public Battle Battlescape { get { return battlescape; } set { battlescape = value; } }

        /// <summary>
        /// Data specific to the geoscape
        /// </summary>
        private GeoData geodata;

        /// <summary>
        /// The current battlescape (if there is one)
        /// </summary>
        private Battle battlescape;

        #endregion Fields
    }
}

