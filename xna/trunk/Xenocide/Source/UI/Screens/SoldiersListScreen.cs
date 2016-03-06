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
* @file SoldiersListScreen.cs
* @date Created: 2007/10/06
* @author File creator: David Cameron
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using CeGui;
using ProjectXenocide.Model.Geoscape;

using ProjectXenocide.Utils;
using CeGui.Widgets;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.Model.Battlescape.Combatants;
using Xenocide.Resources;
using ProjectXenocide.UI.Dialogs;


#endregion

namespace ProjectXenocide.UI.Screens
{
    /// <summary>
    /// Lists soldiers currently at the base.
    /// </summary>
    public class SoldiersListScreen : Screen
    {
        /// <summary>
        /// Constructs a screen listing the soldiers stationed at the given base.
        /// </summary>
        public SoldiersListScreen(int selectedOutpostIndex)
            : base("SoldiersListScreen", @"Content\Textures\UI\BasesScreenBackground.png")
        {
            this.selectedOutpostIndex = selectedOutpostIndex;

            // can only equip the soldiers currently at the outpost
            soldiers = new List<Person>();
            foreach (Person soldier in SelectedOutpost.ListStaff("ITEM_PERSON_SOLDIER"))
            {
                Aircraft aircraft = soldier.Aircraft;
                if ((null == aircraft) || (aircraft.InBase))
                {
                    soldiers.Add(soldier);
                }
            }
        }

        #region Create the CeGui widgets

        /// <summary>
        /// Add all the widgets to the screen. We'll delegate to a different method for each
        /// part of the screen.
        /// </summary>
        protected override void CreateCeguiWidgets()
        {
            InitializeSoldiersGrid();
            InitializeSoldierDetailPanel();
            CreateRightHandButtons();
        }

        /// <summary>
        /// Section at the top of the left hand panel that shows the soldier's details.
        /// </summary>
        private void InitializeSoldierDetailPanel()
        {
            // edit box for soldier name
            nameEditBox = AddEditBox("EDITBOX_NAME", 0.01f, 0.06f, 0.70f, 0.12f);
            nameEditBox.Font = FontManager.Instance.GetFont("LargeBaseName");
            nameEditBox.TextAccepted += new WindowEventHandler(OnSoldierNameChanged);

            // attributes
            attributesGrid = AddGrid(0.01f, 0.20f, 0.70f, 0.75f,
                Strings.SCREEN_SOLDIERS_LIST_COLUMN_ATTRIBUTE, 0.49f,
                Strings.SCREEN_SOLDIERS_LIST_COLUMN_VALUE, 0.50f
            );
            PopulateSoldierDetailPanel();
        }

        /// <summary>
        /// List of soldiers in the bottom of the left hand panel.
        /// </summary>
        private void InitializeSoldiersGrid()
        {
            soldiersListGrid = GuiBuilder.CreateListBox("soldiersListGrid");
            AddWidget(soldiersListGrid, 0.7475f, 0.06f, 0.24f, 0.70f);
            soldiersListGrid.SelectionChanged += new WindowEventHandler(OnSelectedSoldierChanged);

            RefreshSoldiersGrid();
        }

        /// <summary>
        /// Display (or redisplay) soldiers stationed at this base. If a soldier is selected
        /// when this is called, the same soldier will be selected afterwards.
        /// </summary>
        private void RefreshSoldiersGrid()
        {
            // if no soldier selected, select first one in list
            int selectedSoldierIndex = 0;
            if (null != SelectedSoldier)
            {
                selectedSoldierIndex = soldiers.IndexOf(SelectedSoldier);
            }

            soldiersListGrid.ResetList();
            for (int i = 0; i < soldiers.Count; i++)
            {
                ListboxItem item = soldiersListGrid.AddItem(soldiers[i].Name);
                item.ID = i;
                item.Selected = (i == selectedSoldierIndex);
            }
        }

        /// <summary>
        /// Fill the details pannel with the details of the currently selected soldier
        /// </summary>
        private void PopulateSoldierDetailPanel()
        {
            if (null == SelectedSoldier)
            {
                nameEditBox.Text = String.Empty;
            }
            else
            {
                nameEditBox.Text = SelectedSoldier.Name;
            }
            PopulateAttributes();
        }

        /// <summary>
        /// Populate the attributes Grid with the currently selected soldier's attributes
        /// </summary>
        private void PopulateAttributes()
        {
            Person person = SelectedSoldier;
            if ((null != person) && (null != attributesGrid))
            {
                attributesGrid.ResetList();

                // Psi Training
                if (Xenocide.GameState.GeoData.XCorp.TechManager.IsAvailable("FAC_PSIONIC_TRAINING_FACILITY"))
                {
                    String training = Strings.SCREEN_SOLDIERS_LIST_NOT_PSI_TRAINING;
                    if (person.PsiTraining)
                    {
                        training = Strings.SCREEN_SOLDIERS_LIST_IN_PSI_TRAINING;
                    }
                    AddAttributeRow(Strings.SCREEN_SOLDIERS_LIST_ROW_PSI_TRAINING, training);
                }

                // Craft
                Aircraft aircraft = person.Aircraft;
                string aircraftName = (null != aircraft) ? aircraft.Name : String.Empty;
                AddAttributeRow(Strings.SCREEN_SOLDIERS_LIST_ROW_AIRCRAFT, aircraftName);

                // Armor
                // nasty way of doing this, but I think it's the only place where we need it.
                Item armor = person.Combatant.Inventory.ItemAt(4, 3);
                string armorName = (null != armor) ? armor.Name : Strings.ARMOR_TYPE_NONE;
                AddAttributeRow(Strings.SCREEN_SOLDIERS_LIST_ROW_ARMOR, armorName);

                // Stats
                for (Statistic s = Statistic.TimeUnits; s <= Statistic.DaysHired; ++s)
                {
                    AddStatistic(person.Combatant, s);
                }

                AddAttributeRow("In Psi Training", person.PsiTraining.ToString());

                // Others ToDo
                AddAttributeRow("ToDo", "ToDo");
            }
        }

        /// <summary>Add a soldier's statistic to the list of stats shown screen</summary>
        /// <param name="combatant">soldier who has statistic</param>
        /// <param name="s">Statistic to show</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity",
            Justification = "Not possible to reduce")]
        private void AddStatistic(Combatant combatant, Statistic s)
        {
            string rowName;
            string rowValue;
            switch (s)
            {
                // these are not shown
                case Statistic.EnergyRecharge:
                case Statistic.VictoryPoints:
                case Statistic.Aggression:
                case Statistic.Melee:
                case Statistic.Intelligence:
                case Statistic.StandingHeight:
                case Statistic.KneelingHeight:
                case Statistic.FloatingHeight:
                case Statistic.MotionScannerBlipSize:
                case Statistic.TimeUnitsLeft:
                case Statistic.StunDamage:

                // ToDo: not shown at moment, should be moved out of stats into a FatalWounds class
                // Note, they're only relevant on the battlescape
                case Statistic.FatalWoundsHead:
                case Statistic.FatalWoundsBody:
                case Statistic.FatalWoundsLeftArm:
                case Statistic.FatalWoundsRightArm:
                case Statistic.FatalWoundsLeftLeg:
                case Statistic.FatalWoundsRightLeg:
                case Statistic.StaminaLeft:
                    return;

                // these have simple hanlding (just show the number)
                case Statistic.InjuryDamage:
                case Statistic.Kills:
                case Statistic.Missions:
                    rowName = StaticticNames.DisplayString(s);
                    rowValue = Util.ToString(combatant.Stats[s]);
                    break;

                // ToDo: these will show starting and current value
                case Statistic.TimeUnits:
                case Statistic.Stamina:
                case Statistic.Health:
                case Statistic.Bravery:
                case Statistic.Reactions:
                case Statistic.FiringAccuracy:
                case Statistic.ThrowingAccuracy:
                case Statistic.Strength:
                case Statistic.PsiStrength:
                case Statistic.PsiSkill:
                    rowName = StaticticNames.DisplayString(s);
                    rowValue = Util.ToString(combatant.Stats[s]);
                    break;

                // special cases
                case Statistic.DaysHired:
                    rowName = StaticticNames.DisplayString(s);
                    rowValue = Util.ToString(combatant.Stats.DaysHired());
                    break;

                default:
                    // should never get here
                    Debug.Assert(false);
                    return;
            }
            Debug.Assert(null != rowName);
            AddAttributeRow(rowName, rowValue);
        }

        /// <summary>
        /// Add a row to the attributes grid
        /// </summary>
        /// <param name="attribute">text to put in the attributes column</param>
        /// <param name="value">text to put in the value column</param>
        private void AddAttributeRow(string attribute, string value)
        {
            ListboxItem listboxItem = Util.CreateListboxItem(attribute);
            int row = attributesGrid.AddRow(listboxItem, 0);
            listboxItem.ID = row;
            Util.AddStringElementToGrid(attributesGrid, 1, row, value);
        }

        /// <summary>
        /// Adds buttons to the right hand panel of the screen.
        /// </summary>
        private void CreateRightHandButtons()
        {
            // Psi Training is only available if base has working facility
            if (SelectedOutpost.Floorplan.HasWorkingFacility("FAC_PSIONIC_TRAINING_FACILITY"))
            {
                psiTrainButton = AddButton("BUTTON_PSI_TRAIN", 0.7475f, 0.80f, 0.2275f, 0.04125f);
                psiTrainButton.Clicked += new CeGui.GuiEventHandler(OnPsiTraining);
            }
            craftButton = AddButton("BUTTON_ASSIGN_TO_CRAFT", 0.7475f, 0.85f, 0.2275f, 0.04125f);
            equipButton = AddButton("BUTTON_EQUIP_SOLDIER", 0.7475f, 0.90f, 0.2275f, 0.04125f);
            closeButton = AddButton("BUTTON_CLOSE", 0.7475f, 0.95f, 0.2275f, 0.04125f);

            craftButton.Clicked += new CeGui.GuiEventHandler(ShowAssignScreen);
            equipButton.Clicked += new CeGui.GuiEventHandler(OnEquipButton);
            closeButton.Clicked += new CeGui.GuiEventHandler(ShowBasesScreen);
        }

        private CeGui.Widgets.EditBox nameEditBox;
        private CeGui.Widgets.Listbox soldiersListGrid;
        private CeGui.Widgets.MultiColumnList attributesGrid;
        private CeGui.Widgets.PushButton craftButton;
        private CeGui.Widgets.PushButton equipButton;
        private CeGui.Widgets.PushButton psiTrainButton;
        private CeGui.Widgets.PushButton closeButton;

        #endregion

        #region Event Handlers

        /// <summary>
        /// Event handler called when the name edit box value is changed.
        /// </summary>
        /// <param name="sender">not used</param>
        /// <param name="e">not used</param>
        private void OnSoldierNameChanged(object sender, CeGui.GuiEventArgs e)
        {
            if (null != SelectedSoldier)
            {
                SelectedSoldier.Rename(nameEditBox.Text);
                RefreshSoldiersGrid();
            }
        }

        /// <summary>
        /// Called when a soldier is clicked in the list. Displays appropriate values in the
        /// soldier detail section.
        /// 
        /// If the listbox is clicked on a row that doesn't contain a soldier, then SelectedSoldier
        /// will be null.
        /// </summary>
        /// <param name="sender">not used</param>
        /// <param name="e">not used</param>
        private void OnSelectedSoldierChanged(object sender, CeGui.GuiEventArgs e)
        {
            PopulateSoldierDetailPanel();
        }

        /// <summary>React to user pressing the Equip button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnEquipButton(object sender, CeGui.GuiEventArgs e)
        {
            ShowEquipSoldiersScreen();
        }

        /// <summary>Replace this screen with matching BasesScreen</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void ShowBasesScreen(object sender, CeGui.GuiEventArgs e)
        {
            ScreenManager.ScheduleScreen(new BasesScreen(selectedOutpostIndex));
        }

        /// <summary>Replace this screen with matching Assign to craft screen</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void ShowAssignScreen(object sender, CeGui.GuiEventArgs e)
        {
            ScreenManager.ScheduleScreen(new AssignToCraftScreen(selectedOutpostIndex));
        }

        /// <summary>User has clicked on "Psi Training" button</summary>
        /// <param name="sender">not used</param>
        /// <param name="e">not used</param>
        private void OnPsiTraining(object sender, CeGui.GuiEventArgs e)
        {
            TogglePsiTraining();
        }

        #endregion

        private void TogglePsiTraining()
        {
            // if no soldier seleted, nothing to do
            Person person = SelectedSoldier;
            if (null != person)
            {
                if (!person.PsiTraining)
                {
                    // Can only start training if it's the first of the month
                    // and base has free psi training pods
                    if (Xenocide.GameState.GeoData.GeoTime.Time.Day != 1)
                    {
                        Util.ShowMessageBox(Strings.MSGBOX_TOO_LATE_FOR_PSI_TRAINING);
                    }
                    else if (0 == SelectedOutpost.Statistics.Capacities["STORAGE_PSI_TRAINING"].Available)
                    {
                        Util.ShowMessageBox(Strings.MSGBOX_NO_PSI_TRAINING_SPACE);
                    }
                    else
                    {
                        person.PsiTraining = true;
                    }
                }
                else
                {
                    // ToDo: ask user to confirm cancel training
                    YesNoDialog dlg = YesNoDialog.OkCancelDialog(
                        Util.StringFormat(Strings.YESNOMSG_CANCEL_PSI_TRAINING, person.Name)
                    );
                    dlg.YesAction += delegate()
                    {
                        person.PsiTraining = false;
                        // ToDo: Remove after showing "In Psi Training" in list hack is removed
                        PopulateAttributes();
                    };
                    Xenocide.ScreenManager.ShowDialog(dlg);
                }
                PopulateAttributes();
            }
        }

        /// <summary>
        /// Show screen that allows player to equip the selected soldier
        /// </summary>
        private void ShowEquipSoldiersScreen()
        {
            if (null != SelectedSoldier)
            {
                ScreenManager.ScheduleScreen(new EquipSoldierScreen(soldiers, SelectedSoldier));
            }
        }

        #region Fields

        /// <summary>
        /// Returns the domain object for the soldier currently selected in the list.
        /// </summary>
        private Person SelectedSoldier
        {
            get
            {
                ListboxItem selection = soldiersListGrid.GetFirstSelectedItem();
                return (null == selection) ? null : soldiers[selection.ID];
            }
        }

        /// <summary>
        /// The soldiers listed on this screen.
        /// </summary>
        private readonly List<Person> soldiers;

        /// <summary>
        /// The outpost this screen deals with.
        /// </summary>
        private readonly int selectedOutpostIndex;

        /// <summary>
        /// The outpost we're showing the details for
        /// </summary>
        private Outpost SelectedOutpost { get { return Xenocide.GameState.GeoData.Outposts[selectedOutpostIndex]; } }

        #endregion
    }
}
