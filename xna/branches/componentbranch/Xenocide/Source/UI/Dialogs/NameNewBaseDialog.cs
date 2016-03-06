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

using Xenocide.Resources;
using Xenocide.Utils;
using Xenocide.UI.Screens;

using Xenocide.Model.Geoscape;
using Xenocide.Model.Geoscape.HumanBases;
using Microsoft.Xna.Framework;

#endregion

namespace Xenocide.UI.Dialogs
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
        public NameNewBaseDialog(Game game, GeoPosition pos, bool isFirstBase)
            : base(game, new System.Drawing.SizeF(0.5f, 0.3f))
        {
            this.pos         = pos;
            this.isFirstBase = isFirstBase;
        }

        public override void Initialize()
        {
            humanBaseService = (IHumanBaseService)Game.Services.GetService(typeof(IHumanBaseService));
            base.Initialize();
        }

        #region Create the CeGui widgets

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        protected override void CreateCeguiWidgets()
        {
            // static text to show the string "Base Name"
            textWindow = GuiBuilder.CreateText(CeguiId + "_text");
            AddWidget(textWindow, 0.02f, 0.073f, 0.96f, 0.42f);
            textWindow.Text = Strings.DLG_NEWBASENAME;

            // Edit box for the Base's name
            baseNameEditBox = GuiBuilder.CreateEditBox("editBox");
            AddWidget(baseNameEditBox, 0.02f, 0.55f, 0.96f, 0.23f);
            baseNameEditBox.Activate();

            // ok buttons
            okButton        = AddButton("BUTTON_OK",  0.25f, 0.80f, 0.2275f, 0.10f);

            okButton.Clicked += new CeGui.GuiEventHandler(OnOkClicked);
        }

        private CeGui.Widgets.StaticText  textWindow;
        private CeGui.Widgets.EditBox     baseNameEditBox;
        private CeGui.Widgets.PushButton  okButton;

        #endregion Create the CeGui widgets

        #region event handlers

        /// <summary>user has supplied base name</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnOkClicked(object sender, CeGui.GuiEventArgs e)
        {
            String name = baseNameEditBox.Text;

            if (!IsNameLegal(name))
            {
                return;
            }

            //ToDo: check user has sufficient funds to build base.

            // create the base
            HumanBase humanBase = new HumanBase(pos, name);
            if (isFirstBase)
            {
                humanBase.SetupPlayersFirstBase();
            }
            humanBaseService.HumanBases.Add(humanBase);

            //ToDo: debit cost of base
            
            // Go to bases screen, to allow player to add facilities to this base
            BasesScreen basesScreen = new BasesScreen(
                Game,
                humanBaseService.HumanBases.Count - 1
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
        private bool IsNameLegal(String name)
        {
            // user needs to supply a name for the base
            if (String.IsNullOrEmpty(name))
            {
                ScreenManager.ShowDialog(new MessageBoxDialog(Game, Strings.MSGBOX_BASE_NEEDS_NAME));
                return false;
            }

            // Name can't be the same as any existing bases
            foreach (HumanBase humanbase in humanBaseService.HumanBases)
            {
                if (humanbase.Name == name)
                {
                    ScreenManager.ShowDialog(new MessageBoxDialog(
                        Game,
                        Util.StringFormat(Strings.MSGBOX_BASE_NAMES_ARE_UNIQUE, name)
                    ));
                    return false;
                }
            }

            // if get here, name is OK
            return true;
        }

        #endregion event handlers

        #region Fields

        private IHumanBaseService humanBaseService;

        /// <summary>
        /// Position for new base
        /// </summary>
        private GeoPosition pos;

        /// <summary>
        /// Is this the very first base?
        /// </summary>
        private bool isFirstBase;

        #endregion Fields
    }
}
