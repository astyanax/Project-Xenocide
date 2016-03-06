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
* @file StaticTables.cs
* @date Created: 2007/03/24
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

using Microsoft.Xna.Framework.Storage;

using Xenocide.Model.StaticData;
using Xenocide.Model.StaticData.Facilities;

#endregion

namespace Xenocide.Model
{
    /// <summary>
    /// The root class that holds all "model" (as in model-view) data that is fixed, but loaded from XML files
    /// </summary>
    /// <remarks>That is, things like weapon and craft stats, facilities, etc.</remarks>
    public class StaticTables
    {
        /// <summary>
        /// Read the static data from the files it's stored in
        /// </summary>
        public StaticTables()
        {
            xnetEntryList = new XNetEntryCollection(DataDirectory + "xnet.xml");
            facilityList = new FacilityInfoCollection(DataDirectory + "facility.xml", xnetEntryList);
        }

        /// <summary>
        /// The X-Net entries
        /// </summary>
        public XNetEntryCollection XNetEntryList { get { return xnetEntryList; } }

        /// <summary>
        /// Stats for all the different types of Facilities
        /// </summary>
        public FacilityInfoCollection FacilityList { get { return facilityList; } }

        /// <summary>
        /// The directory holding all the XML data files.
        /// </summary>
        public string DataDirectory { get { return StorageContainer.TitleLocation + "/Content/Schema/"; } }

        /// <summary>
        /// The X-Net entries
        /// </summary>
        private XNetEntryCollection xnetEntryList;

        /// <summary>
        /// Stats for all the different types of Facilities
        /// </summary>
        private FacilityInfoCollection facilityList;
    }
}

