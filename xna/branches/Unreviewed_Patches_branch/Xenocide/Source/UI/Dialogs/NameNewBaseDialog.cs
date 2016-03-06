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
* @file NameNewBaseDialog.cs
* @date Created: 2007/05/20
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;

using CeGui;


using ProjectXenocide.Utils;
using ProjectXenocide.UI.Screens;

using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Outposts;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.UI.Dialogs
{
    /// <summary>
    /// Dialog where user gives new base a name
    /// </summary>
    public class NameNewBaseDialog : Dialog
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pos">location of new base</param>
        /// <param name="isFirstBase">Is this the very first base?</param>
        public NameNewBaseDialog(GeoPosition pos, bool isFirstBase)
            : base("Content/Layouts/NameNewBaseDialog.layout")
        {
            this.pos = pos;
            this.isFirstBase = isFirstBase;
        }

        #region Create the CeGui widgets

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        protected override void CreateCeguiWidgets()
        {
            WindowManager.Instance.GetWindow(edtBaseNameName).Activate();
        }

        #endregion Create the CeGui widgets

        #region event handlers

        /// <summary>
        /// User has supplied base name and hit the "enter" key.
        /// </summary>
        /// <param name="sender">not used</param>
        /// <param name="e">not used</param>
        [GuiEvent()]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        public void OnEditBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == System.Windows.Forms.Keys.Enter)
            {
                nameBase();
            }
        }

        /// <summary>user has supplied base name</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        [GuiEvent()]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        public void OnOkClicked(object sender, CeGui.GuiEventArgs e)
        {
            nameBase();
        }

        private void nameBase()
        {
            String name = WindowManager.Instance.GetWindow(edtBaseNameName).Text;

            if (!IsNameLegal(name))
            {
                return;
            }

            // create the base
            Outpost outpost = new Outpost(pos, name);
            if (isFirstBase)
            {
                outpost.SetupPlayersFirstBase();
            }
            Xenocide.GameState.GeoData.Outposts.Add(outpost);

            // Go to bases screen, to allow player to add facilities to this base
            BasesScreen basesScreen = new BasesScreen(
                Xenocide.GameState.GeoData.Outposts.Count - 1
            );
            if (!isFirstBase)
            {
                basesScreen.State = BasesScreen.BasesScreenState.AddAccessLift;
            }
            ScreenManager.ScheduleScreen(basesScreen);
            ScreenManager.CloseDialog(this);
        }

        /// <summary>
        /// Check that the name user has given for this base is legal
        /// </summary>
        /// <param name="name">proposed name for base</param>
        /// <returns>true if name is legal</returns>
        private static bool IsNameLegal(String name)
        {
            // user needs to supply a name for the base
            if (String.IsNullOrEmpty(name))
            {
                Util.ShowMessageBox(Strings.MSGBOX_BASE_NEEDS_NAME);
                return false;
            }

            // Name can't be the same as any existing bases
            foreach (Outpost outpost in Xenocide.GameState.GeoData.Outposts)
            {
                if (outpost.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
                {
                    Util.ShowMessageBox(Strings.MSGBOX_BASE_NAMES_ARE_UNIQUE, name);
                    return false;
                }
            }

            // if get here, name is OK
            return true;
        }

        #endregion event handlers

        #region Fields

        /// <summary>
        /// Position for new base
        /// </summary>
        private GeoPosition pos;

        /// <summary>
        /// Is this the very first base?
        /// </summary>
        private bool isFirstBase;

        #endregion Fields

        #region Constants

        private const string edtBaseNameName = "edtBaseName";

        #endregion
    }
}
