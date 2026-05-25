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

using Gum.Forms;
using Gum.Forms.Controls;

using ProjectXenocide.Model.Battlescape.Combatants;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.UI.Controls;
using ProjectXenocide.UI.Dialogs;
using ProjectXenocide.Utils;

using Xenocide.Resources;


#endregion

namespace ProjectXenocide.UI.Screens
{
    /// <summary>
    /// Lists soldiers currently at the base.
    /// </summary>
    public class SoldiersListScreen : GumScreen
    {
        /// <summary>
        /// Constructs a screen listing the soldiers stationed at the given base.
        /// </summary>
        public SoldiersListScreen(int selectedOutpostIndex)
            : base("SoldiersListScreen", @"Content/Textures/UI/BasesScreenBackground.png")
        {
            this.selectedOutpostIndex = selectedOutpostIndex;

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

        #region Create the Gum controls

        /// <summary>
        /// Add all the widgets to the screen. We'll delegate to a different method for each
        /// part of the screen.
        /// </summary>
        protected override void CreateGumControls()
        {
            if (GumRoot != null)
            {
                WireButton("craftButton", ShowAssignScreen);
                WireButton("equipButton", OnEquipButton);
                WireButton("closeButton", ShowBasesScreen);

                if (SelectedOutpost.Floorplan.HasWorkingFacility("FAC_PSIONIC_TRAINING_FACILITY"))
                {
                    psiTrainButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_PSI_TRAIN") };
                    AddChild(psiTrainButton);
                    psiTrainButton.Click += OnPsiTraining;
                }

                InitializeSoldiersGrid();
                InitializeSoldierDetailPanel();
                return;
            }

            InitializeSoldiersGrid();
            InitializeSoldierDetailPanel();
            CreateRightHandButtons();
        }

        /// <summary>
        /// Section at the top of the left hand panel that shows the soldier's details.
        /// </summary>
        private void InitializeSoldierDetailPanel()
        {
            nameEditBox = new Label() { Text = XenocideResourceManager.Get("EDITBOX_NAME") };
            AddChild(nameEditBox);

            attributesGrid = new GridPanel();
            attributesGrid.AddColumn("Attribute", 250);
            attributesGrid.AddColumn("Value", 250);
            AddChild(attributesGrid.Visual);

            PopulateSoldierDetailPanel();
        }

        /// <summary>
        /// List of soldiers in the bottom of the left hand panel.
        /// </summary>
        private void InitializeSoldiersGrid()
        {
            soldiersListGrid = new GridPanel();
            soldiersListGrid.AddColumn(XenocideResourceManager.Get("soldiersListGrid"), 300);
            AddChild(soldiersListGrid.Visual);
            soldiersListGrid.SelectionChanged += OnSelectedSoldierChanged;

            RefreshSoldiersGrid();
        }

        /// <summary>
        /// Display (or redisplay) soldiers stationed at this base. If a soldier is selected
        /// when this is called, the same soldier will be selected afterwards.
        /// </summary>
        private void RefreshSoldiersGrid()
        {
            soldiersListGrid.Clear();

            foreach (Person soldier in soldiers)
            {
                soldiersListGrid.AddRow(soldier, soldier.Name);
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
            attributesGrid.Clear();

            Person person = SelectedSoldier;
            if ((null != person))
            {
                if (Xenocide.GameState.GeoData.XCorp.TechManager.IsAvailable("FAC_PSIONIC_TRAINING_FACILITY"))
                {
                    String training = Strings.SCREEN_SOLDIERS_LIST_NOT_PSI_TRAINING;
                    if (person.PsiTraining)
                    {
                        training = Strings.SCREEN_SOLDIERS_LIST_IN_PSI_TRAINING;
                    }
                    AddAttributeRow(Strings.SCREEN_SOLDIERS_LIST_ROW_PSI_TRAINING, training);
                }

                Aircraft aircraft = person.Aircraft;
                string aircraftName = (null != aircraft) ? aircraft.Name : String.Empty;
                AddAttributeRow(Strings.SCREEN_SOLDIERS_LIST_ROW_AIRCRAFT, aircraftName);

                Item armor = person.Combatant.Inventory.ItemAt(4, 3);
                string armorName = (null != armor) ? armor.Name : Strings.ARMOR_TYPE_NONE;
                AddAttributeRow(Strings.SCREEN_SOLDIERS_LIST_ROW_ARMOR, armorName);

                for (Statistic s = Statistic.TimeUnits; s <= Statistic.DaysHired; ++s)
                {
                    AddStatistic(person.Combatant, s);
                }

                AddAttributeRow("In Psi Training", person.PsiTraining.ToString());
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
                case Statistic.FatalWoundsHead:
                case Statistic.FatalWoundsBody:
                case Statistic.FatalWoundsLeftArm:
                case Statistic.FatalWoundsRightArm:
                case Statistic.FatalWoundsLeftLeg:
                case Statistic.FatalWoundsRightLeg:
                case Statistic.StaminaLeft:
                    return;

                case Statistic.InjuryDamage:
                case Statistic.Kills:
                case Statistic.Missions:
                    rowName = StaticticNames.DisplayString(s);
                    rowValue = Util.ToString(combatant.Stats[s]);
                    break;

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

                case Statistic.DaysHired:
                    rowName = StaticticNames.DisplayString(s);
                    rowValue = Util.ToString(combatant.Stats.DaysHired());
                    break;

                default:
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
            attributesGrid.AddRow(null, attribute, value);
        }

        /// <summary>
        /// Adds buttons to the right hand panel of the screen.
        /// </summary>
        private void CreateRightHandButtons()
        {
            if (SelectedOutpost.Floorplan.HasWorkingFacility("FAC_PSIONIC_TRAINING_FACILITY"))
            {
                psiTrainButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_PSI_TRAIN") };
                RootContainer.AddChild(psiTrainButton);
                psiTrainButton.Click += OnPsiTraining;
            }
            craftButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_ASSIGN_TO_CRAFT") };
            RootContainer.AddChild(craftButton);
            equipButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_EQUIP_SOLDIER") };
            RootContainer.AddChild(equipButton);
            closeButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_CLOSE") };
            RootContainer.AddChild(closeButton);

            craftButton.Click += ShowAssignScreen;
            equipButton.Click += OnEquipButton;
            closeButton.Click += ShowBasesScreen;
        }

        private Label nameEditBox;
        private GridPanel soldiersListGrid;
        private GridPanel attributesGrid;
        private Button craftButton;
        private Button equipButton;
        private Button psiTrainButton;
        private Button closeButton;

        #endregion

        #region Event Handlers

        private void OnSoldierNameChanged(object sender, EventArgs e)
        {
            if (null != SelectedSoldier)
            {
                SelectedSoldier.Rename(nameEditBox.Text);
                RefreshSoldiersGrid();
            }
        }

        private void OnSelectedSoldierChanged(object sender, EventArgs e)
        {
            PopulateSoldierDetailPanel();
        }

        private void OnEquipButton(object sender, EventArgs e)
        {
            ShowEquipSoldiersScreen();
        }

        private void ShowBasesScreen(object sender, EventArgs e)
        {
            ScreenManager.ScheduleScreen(new BasesScreen(selectedOutpostIndex));
        }

        private void ShowAssignScreen(object sender, EventArgs e)
        {
            ScreenManager.ScheduleScreen(new AssignToCraftScreen(selectedOutpostIndex));
        }

        private void OnPsiTraining(object sender, EventArgs e)
        {
            TogglePsiTraining();
        }

        #endregion

        private void TogglePsiTraining()
        {
            Person person = SelectedSoldier;
            if (null != person)
            {
                if (!person.PsiTraining)
                {
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
                    GumYesNoDialog dlg = GumYesNoDialog.OkCancelDialog(
                        Util.StringFormat(Strings.YESNOMSG_CANCEL_PSI_TRAINING, person.Name)
                    );
                    dlg.YesAction += delegate ()
                    {
                        person.PsiTraining = false;
                        PopulateAttributes();
                    };
                    Xenocide.ScreenManager.ShowDialog(dlg);
                }
                PopulateAttributes();
            }
        }

        private void ShowEquipSoldiersScreen()
        {
            if (null != SelectedSoldier)
            {
                ScreenManager.ScheduleScreen(new EquipSoldierScreen(soldiers, SelectedSoldier));
            }
        }

        #region Fields

        private Person SelectedSoldier
        {
            get
            {
                object tag = soldiersListGrid.GetSelectedTag();
                return tag as Person;
            }
        }

        private readonly List<Person> soldiers;

        private readonly int selectedOutpostIndex;

        private Outpost SelectedOutpost { get { return Xenocide.GameState.GeoData.Outposts[selectedOutpostIndex]; } }

        #endregion
    }
}
