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
* @file BaseCapacities.cs
* @date Created: 2007/05/13
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;

using Xenocide.Resources;
using Xenocide.Utils;

#endregion

namespace Xenocide.Model.Geoscape.HumanBases
{
    /// <summary>
    /// Represents all the storage capacities in a human base
    /// </summary>
    [Serializable]
    public class BaseCapacities
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public BaseCapacities()
        {
            capacities = new Dictionary<String, BaseCapacityInfo>();
            foreach (String typeName in capacityTypes)
            {
                capacities[typeName] = new BaseCapacityInfo();
            }
        }

        /// <summary>
        /// Retreive a BaseCapacityInfo by name
        /// </summary>
        /// <param name="typeName">internal name used to capacity type</param>
        /// <returns>the BaseCapacityInfo</returns>
        public BaseCapacityInfo this[string typeName] { get { return capacities[typeName]; } }

        /// <summary>
        /// Convert the internal capacity type (as used in XML file) to display string
        /// </summary>
        /// <param name="typeName">name in capacityTypes to look up</param>
        /// <returns>The string to display for this capacity type</returns>
        public static String ToDisplayString(String typeName)
        {
            for (int i = 0; i < capacityTypes.Length; ++i)
            {
                if (typeName == capacityTypes[i])
                {
                    return Util.LoadString(displayStrings[i]);
                }
            }
            
            // if get here, typeName was invalid
            Debug.Assert(false);
            return "";
        }


        #region Fields

        /// <summary>
        /// The types of capacity storage
        /// </summary>
        public static IList<String> CapacityTypes { get { return capacityTypes; } }
        
        /// <summary>
        /// The types of capacity storage
        /// </summary>
        private static String[] capacityTypes =
        {
            "STORAGE_PERSONNEL",
            "STORAGE_GEAR",
            "STORAGE_SCIENTIST",
            "STORAGE_ENGINEER",
            "STORAGE_CRAFT",
            "STORAGE_ALIENS",
            "STORAGE_PSI_TRAINING"
        };

        /// <summary>
        /// The names of the capacity types, for display to user
        /// </summary>
        private static String[] displayStrings =
        {
            "STORAGE_PERSONNEL_DISPLAY",
            "STORAGE_GEAR_DISPLAY",
            "STORAGE_SCIENTIST_DISPLAY",
            "STORAGE_ENGINEER_DISPLAY",
            "STORAGE_CRAFT_DISPLAY",
            "STORAGE_ALIENS_DISPLAY",
            "STORAGE_PSI_TRAINING_DISPLAY"
        };

        private Dictionary<String, BaseCapacityInfo> capacities;

        #endregion Fields
    }
}
