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
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;

using Xenocide.Model.Geoscape;
using Xenocide.Model.Geoscape.Research;
using Microsoft.Xna.Framework;

#endregion

namespace Xenocide.Model
{
    /// <summary>
    /// The root class that holds all "model" (as in model-view) data that is loaded/saved to file
    /// </summary>
    /// RK: Too many "Singletons" laying arround, make yourself a favor in the long run... rule them out (you will understand when you need to do save and load ;) ).
    /// Everything should come from there... try a Game/Player or something like that.
    [Serializable]
    public class GameState : GameStateComponent
    {
        /// <summary>
        /// Construct a new GameState
        /// </summary>
        /// <param name="game"></param>
        public GameState()
        {
            //this will have to be moved to Initialize once we have load/save stuff inside a proper service
            geodata = new GeoData();
            xnetState = new XNetState();
        }

        /// <summary>
        /// Data specific to the geoscape 
        /// </summary>
        public GeoData GeoData { get { return geodata; } }

        /// <summary>
        /// Availablity of different XNETEntries
        /// </summary>
        public XNetState XNetState { get { return xnetState; } }

        /// <summary>
        /// Data specific to the geoscape 
        /// </summary>
        private GeoData geodata;

        private XNetState xnetState;

        protected override void  OnGameSet()
        {
            geodata.Game = Game;
            //TODO: pass Game to xnetstate once ready
        }
    }
}

