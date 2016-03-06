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
* @file Misc.cs
* @date Created: 2007/01/25
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

using System;
using System.Collections.Generic;
using System.Text;

using ProjectXenocide.Utils;
using ProjectXenocide.Model.Geoscape.Outposts;

namespace ProjectXenocide.UI.Screens
{
    /// <summary>
    /// Assorted Utility functions I haven't figured out a better place for
    /// </summary>
    internal static class Misc
    {
        /// <summary>
        /// Fill a combo box with a list of X-Corp outposts
        /// </summary>
        /// <param name="basesList">the combo box to fill</param>
        /// <param name="baseIndex">Outpost to initially select</param>
        public static void PopulateHumanBasesList(CeGui.Widgets.ComboBox basesList, int baseIndex)
        {
            // TODO: Not the proper place for this functionality

            foreach (Outpost outpost in Xenocide.GameState.GeoData.Outposts)
            {
                basesList.AddItem(outpost.Name);
            }

            //... set base combo selection to base currently being viewed
            basesList.SetItemSelectState(baseIndex, true);
            basesList.Text = basesList.SelectedItem.Text;

            //... tag the edit box of the combo for viewing only.
            basesList.ReadOnly = true;
        }
    }
}
