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
* @file Combatant.cs
* @date Created: 2007/11/18
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json.Serialization;

using Microsoft.Xna.Framework;

using NLog;

using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.StaticData;
using ProjectXenocide.Model.StaticData.Battlescape;
using ProjectXenocide.Model.StaticData.Items;

namespace ProjectXenocide.Model.Battlescape.Combatants
{
    /// <summary>
    /// A combat-capable entity (soldier, alien or civilian) that belongs to a team
    /// and takes part in ground engagements.
    /// </summary>
    [Serializable]
    public partial class Combatant
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>Canon UFO Defense: stun damage caps at 255</summary>
        private const int MaxStunLevel = 255;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="info">Properties of this type of combatant</param>
        /// <param name="teamId">Team (alien/X-Corp/Civilian) that owns this combatant</param>
        public Combatant(CombatantInfo info, int teamId)
        {
            this.inventory = new CombatantInventory(this);
            this.combatantInfo = info;
            this.teamId = teamId;
            if (null != info)
            {
                info.GenerateStats(this.stats);
                this.flyer = info.Flyer;
                this.armorIndex = info.ArmorIndex;
                if (null != info.Graphic)
                {
                    this.graphic = info.Graphic;
                }
                else
                {
                    UseUnarmoredXCorpSolider();
                }
            }
        }

        /// <summary>Update Combatant in response to a turn starting</summary>
        // TODO: used once ground engagements gain a turn-based model (refresh Time Units and energy).
        public void OnStartTurn()
        {
            stats.OnStartTurn();
        }

        /// <summary>Record that combatant did something that counts as a "learning experience"</summary>
        /// <param name="act">what was done</param>
        // TODO: wired into the planned engagement experience/stat-advancement model.
        public void RecordAchievement(Experience.Act act)
        {
            experience.RecordAchievement(act);

            // handle acts that are also recorded elsewhere
            switch (act)
            {
                case Experience.Act.KilledTarget:
                    ++stats[Statistic.Kills];
                    break;
            }
        }

        /// <summary>Apply one day of healing to combatant</summary>
        public void DailyHealing()
        {
            if (IsInjured)
            {
                --stats[Statistic.InjuryDamage];
            }
        }

        /// <summary>Called after a mission if an X-Corp soldier died</summary>
        /// <param name="bodyRecovered">true if body was recovered</param>
        /// <param name="outpostInventory">where to put items soldier was carrying</param>
        /// <remarks>basically, salvage soldiers equipement, if possible</remarks>
        public void DiedOnMission(bool bodyRecovered, OutpostInventory outpostInventory)
        {
            if (bodyRecovered)
            {
                Inventory.Unload(outpostInventory);
            }
        }

        /// <summary>Clear per-mission state after an engagement (currently, heal fatal wounds)</summary>
        public void PostMissionCleanup()
        {
            // Fatal wounds are healed automatically after mission
            foreach (Statistic s in fatalWoundsStat)
            {
                stats[s] = 0;
            }
        }

        /// <summary>Update combatant's members, to reflect the armor being worn</summary>
        /// <remarks>This should only be called for X-Corp soldiers</remarks>
        private void AdjustStatsFromArmor()
        {
            if ("None" == Armor.Id)
            {
                UseUnarmoredXCorpSolider();
            }
            else
            {
                var itemList = Xenocide.StaticTables.ItemList;
                if (itemList.IndexOf(Armor.Id) >= 0)
                {
                    this.graphic = itemList[Armor.Id].BattlescapeInfo.Graphic;
                }
                else
                {
                    // Armor references an item id that isn't in the item table.
                    // Fall back to the unarmored graphic rather than silently
                    // leaving the previous model in place.
                    Logger.Warn("Armor '{0}' not found in item list; using unarmored graphic", Armor.Id);
                    UseUnarmoredXCorpSolider();
                }
            }
            this.flyer = Armor.Flyer;
        }

        /// <summary>Set 3D Model to that of X-Corp soldier with no armor</summary>
        private void UseUnarmoredXCorpSolider()
        {
            this.graphic = new Graphic(@"Characters/XCorp/FemaleShirt", 0, MathHelper.PiOver2, 0);
        }

        /// <summary>
        /// Calculate the probablity of hitting a target
        /// </summary>
        /// <param name="activeArm"></param>
        /// <returns></returns>
        public double Accuracy(ActiveArm activeArm)
        {
            double kneelingFactor = Kneeling ? 1.15 : 1;
            double otherArmOccupiedFactor;
            if (activeArm == ActiveArm.Both)
            {
                otherArmOccupiedFactor = 1;
            }
            else
            {
                otherArmOccupiedFactor = Inventory.ItemAt(activeArm == ActiveArm.Left ? 0 : 1, 0) != null ? 0.8 : 1;
            }
            return stats.Accuracy(activeArm) * kneelingFactor * otherArmOccupiedFactor;
        }

        /// <summary>Apply a field dressing to one body part</summary>
        /// <param name="bodyPart">Body part being treated</param>
        // TODO: used by the planned wound model (battlefield medkits and post-mission treatment).
        public void Heal(BodyParts bodyPart)
        {
            int healedWounds = GameBalanceClass.HealFatalWounds();
            int healedInjuryDamage = GameBalanceClass.HealInjuryDamage();

            Statistic bodyPartStat = fatalWoundsStat[(int)bodyPart];
            if (0 < stats[bodyPartStat])
            {
                stats[bodyPartStat] -= healedWounds;
            }
            else
            {
                // Heal some injury damage
                stats[Statistic.InjuryDamage] -= healedInjuryDamage;
            }
        }

        #region Constants

        /// <summary>Combatant's field of view. (Computed as dot product of heading and unit vector to target)</summary>
        public const float DotFieldOfView = 0.7071f;

        /// <summary>How far can a combatant see (in cells)?</summary>
        public const int VisionRange = 20;

        /// <summary>Square of VisionRange</summary>
        public const int VisionRangeSquared = VisionRange * VisionRange;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The items being carried by the combatant
        /// </summary>
        public CombatantInventory Inventory { get { return inventory; } }

        /// <summary>
        /// The armor the combatant is "wearing"
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "Will throw if value is null")]
        public Armor Armor
        {
            get { return Xenocide.StaticTables.ArmorList[armorIndex]; }
            set
            {
                armorIndex = Xenocide.StaticTables.ArmorList.IndexOf(value.Id);
                AdjustStatsFromArmor();
            }
        }

        /// <summary>Location in the engagement area (in cells)</summary>
        // TODO: used by the planned positional model (flanking and side/rear armor).
        public Vector3 Position { get { return position; } set { position = value; } }

        /// <summary>Direction facing, in radians.  0 = along positive X axis, clockwise is positive</summary>
        // TODO: used by the planned positional model (flanking and side/rear armor).
        public float Heading
        {
            get { return heading; }
            set
            {
                // normalize value to - Pi <= value <= Pi
                heading = value;
                if (MathHelper.Pi < Math.Abs(heading))
                {
                    heading += MathHelper.Pi * -2 * Math.Sign(heading);
                }
            }
        }

        /// <summary>Direction facing, as a vector</summary>
        // TODO: used by the planned positional model (flanking and side/rear armor).
        public Vector3 HeadingVector
        {
            get { return new Vector3((float)Math.Cos(heading), 0, (float)-Math.Sin(heading)); }
        }

        /// <summary>Assorted properties</summary>
        public CombatantInfo CombatantInfo { get { return combatantInfo; } }

        /// <summary>3D model used to represent this combatant (e.g. on the equip screen)</summary>
        public Graphic Graphic { get { return graphic; } set { graphic = value; } }

        /// <summary>The various numerical values describing a soldier's capabilities</summary>
        public Stats Stats { get { return stats; } }

        /// <summary>Can this combatant fly?</summary>
        public bool Flyer { get { return flyer; } }

        /// <summary>Team (alien/X-Corp/Civilian) that owns this combatant</summary>
        public int TeamId { get { return teamId; } }

        /// <summary>Combatant's position in team array</summary>
        // TODO: set when a team is assembled, for stable ordering in the engagement UI/report.
        public int PlaceInTeam { get { return placeInTeam; } set { placeInTeam = value; } }

        /// <summary>Does combatant have injuries</summary>
        public bool IsInjured { get { return 0 < stats[Statistic.InjuryDamage]; } }

        /// <summary>Has combatant been killed?</summary>
        public bool IsDead { get { return stats[Statistic.Health] < stats[Statistic.InjuryDamage]; } }

        /// <summary>Is combatant not dead and not stunned?</summary>
        public bool CanTakeOrders
        {
            get { return (stats[Statistic.InjuryDamage] + stats[Statistic.StunDamage]) <= stats[Statistic.Health]; }
        }

        /// <summary>The total number of fatal wounds.</summary>
        // TODO: used by the planned wound model (bleeding and treatment).
        public int TotalFatalWounds
        {
            get
            {
                return stats[Statistic.FatalWoundsHead] + stats[Statistic.FatalWoundsBody] +
            stats[Statistic.FatalWoundsLeftArm] + stats[Statistic.FatalWoundsRightArm] + stats[Statistic.FatalWoundsLeftLeg] +
            stats[Statistic.FatalWoundsRightLeg];
            }
        }

        /// <summary>Kneeling status of the combatant (feeds the accuracy bonus above)</summary>
        // TODO: set by the planned stance/cover model.
        public bool Kneeling { get { return kneeling; } }

        /// <summary>
        /// The items being carried by the combatant
        /// </summary>
        private CombatantInventory inventory;

        /// <summary>
        /// Index to armor the combatant is "wearning"
        /// </summary>
        private int armorIndex = Xenocide.StaticTables.ArmorList.NoArmorIndex;

        /// <summary>Location on battlescape (in cells)</summary>
        private Vector3 position;

        /// <summary>Direction facing, in radians.  0 = along positive X axis, clockwise is positive</summary>
        private float heading;

        /// <summary>Assorted properties</summary>
        private CombatantInfo combatantInfo;

        /// <summary>3D model to draw on battlescpe</summary>
        private Graphic graphic;

        /// <summary>The various numerical values describing a soldier's capabilities</summary>
        private Stats stats = new Stats();

        /// <summary>Can this combatant fly?</summary>
        private bool flyer;

        /// <summary>Team (alien/X-Corp/Civilian) that owns this combatant</summary>
        private int teamId;

        /// <summary>Combatant's position in team array</summary>
        private int placeInTeam;

        /// <summary>Acts done this battlescape mission that qualify as learning experience</summary>
        private Experience experience = new Experience();

        /// <summary>Indicates if the combatant is kneeling or not.</summary>
#pragma warning disable CS0649 // Intended future feature - kneeling functionality planned
        private bool kneeling;
#pragma warning restore CS0649

        /// <summary>
        /// Table that holds the Fatal Wounds stat for each body part
        /// </summary>
        // TODO: used by the planned wound model (PostMissionCleanup still clears these).
        static Statistic[] fatalWoundsStat =
        {
            Statistic.FatalWoundsHead,
            Statistic.FatalWoundsBody,
            Statistic.FatalWoundsLeftArm,
            Statistic.FatalWoundsRightArm,
            Statistic.FatalWoundsLeftLeg,
            Statistic.FatalWoundsRightLeg
        };

        #endregion Fields

        #region Enumerations

        /// <summary>Enumeration over the available body parts</summary>
        public enum BodyParts
        {
            // If the order is changed, fatalWoundsStat must be updated
            Head = 0,
            Body = 1,
            LeftArm = 2,
            RightArm = 3,
            LeftLeg = 4,
            RightLeg = 5
        };

        /// <summary>Which arm holds the item the user is using?</summary>
        public enum ActiveArm
        {
            Left = 0,
            Right = 1,
            Both = 2
        };

        #endregion
    }
}
