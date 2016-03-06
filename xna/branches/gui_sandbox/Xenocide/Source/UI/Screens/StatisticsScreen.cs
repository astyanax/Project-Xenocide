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
* @file StatisticsScreen.cs
* @date Created: 2007/01/21
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace Xenocide.UI.Screens
{
    /// <summary>
    /// Screen that shows historical statistics
    /// </summary>
    class StatisticsScreen : Screen
    {
         /// <summary>
        /// Default constructor (obviously)
        /// </summary>
        public StatisticsScreen(ScreenManager screenManager)
            : base("StatisticsScreen", screenManager)
        {
        }

        #region Create the CeGui widgets

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        protected override void CreateCeguiWidgets()
        {
            ufoByRegionButton   = AddButton("BUTTON_UFO_BY_REGION",    0.7475f, 0.46f, 0.2275f, 0.04125f);
            ufoByCountryButton  = AddButton("BUTTON_UFO_BY_COUNTRY",   0.7475f, 0.51f, 0.2275f, 0.04125f);
            xcomByRegionButton  = AddButton("BUTTON_XCORP_BY_REGION",  0.7475f, 0.56f, 0.2275f, 0.04125f);
            xcomByCountryButton = AddButton("BUTTON_XCORP_BY_COUNTRY", 0.7475f, 0.61f, 0.2275f, 0.04125f);
            fundsButton         = AddButton("BUTTON_FUNDS",            0.7475f, 0.66f, 0.2275f, 0.04125f);
            geoscapeButton      = AddButton("BUTTON_GEOSCAPE",         0.7475f, 0.71f, 0.2275f, 0.04125f);

            ufoByRegionButton.Clicked   += new CeGui.GuiEventHandler(unimplemented);
            ufoByCountryButton.Clicked  += new CeGui.GuiEventHandler(unimplemented);
            xcomByRegionButton.Clicked  += new CeGui.GuiEventHandler(unimplemented);
            xcomByCountryButton.Clicked += new CeGui.GuiEventHandler(unimplemented);
            fundsButton.Clicked         += new CeGui.GuiEventHandler(unimplemented);
            geoscapeButton.Clicked      += OnGoToGeoscapeScreen;
        }

        private CeGui.Widgets.PushButton ufoByRegionButton;
        private CeGui.Widgets.PushButton ufoByCountryButton;
        private CeGui.Widgets.PushButton xcomByRegionButton;
        private CeGui.Widgets.PushButton xcomByCountryButton;
        private CeGui.Widgets.PushButton fundsButton;
        private CeGui.Widgets.PushButton geoscapeButton;

        #endregion Create the CeGui widgets

        #region event handlers


        #endregion event handlers
   }
}
