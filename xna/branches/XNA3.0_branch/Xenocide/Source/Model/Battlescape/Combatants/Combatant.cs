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
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;

using ProjectXenocide.Model.StaticData;
using ProjectXenocide.Model.StaticData.Battlescape;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Model.Geoscape.Outposts;

namespace ProjectXenocide.Model.Battlescape.Combatants
{
    /// <summary>
    /// An entity that fights on a battlescape
    /// </summary>
    [Serializable]
    public partial class Combatant
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="info">Properties of this type of combatant</param>
        /// <param name="teamId">Team (alien/X-Corp/Civilian) that owns this combatant</param>
        public Combatant(CombatantInfo info, int teamId)
        {
            this.inventory     = new CombatantInventory(this);
            this.combatantInfo = info;
            this.teamId        = teamId;
            if (null != info)
            {
                info.GenerateStats(this.stats);
                this.flyer      = info.Flyer;
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

        /// <summary>Update Combatant in response to a turn on the battlescape starting</summary>
        public void OnStartTurn()
        {
            stats.OnStartTurn();
        }

        /// <summary>Update combatant's state, based on passage of time</summary>
        /// <param name="seconds">length of time that has passed</param>
        /// <returns>true if order is finished</returns>
        public bool BattlescapeUpdate(double seconds)
        {
            // If combatant has no order, then obviously it's done
            if (null == order)
            {
                return true;
            }
            
            // update order, and check if still running.
            order.Update(seconds);
            if (FinishCode.Executing != order.Finished)
            {
                // order is finished, so dispose of it
                order = null;
                return true;
            }

            // if get here, order still running
            return false;
        }

        /// <summary>Record that combatant did something that counts as a "learning experience"</summary>
        /// <param name="act">what was done</param>
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

        /// <summary>Update state to reflect being attacked</summary>
        /// <param name="damageInfo">attack damage information</param>
        /// <param name="direction">the attack came from</param>
        public void Hit(DamageInfo damageInfo, Vector3 direction)
        {
            // ToDo: need to treat explosive (grenade) differently, as it attacks underside

            // normalize attack direction
            direction.Y = 0;
            direction.Normalize();

            // angle between attack direction and combatant's facing
            Armor.Side side = Armor.Side.Side;
            float dot   = Vector3.Dot(HeadingVector, direction);
            float limit = (float)Math.Sqrt(0.5);
            if (limit < dot)
            {
                side = Armor.Side.Rear;
            }
            else if (dot <= -limit)
            {
                side = Armor.Side.Front;
            }

            Vector2 damage = Armor.DamageInflicted(damageInfo, side);
            stats[Statistic.InjuryDamage] += (int)damage.X;
            stats[Statistic.StunDamage]  += (int)damage.Y;

            // additional processing of injury
            // dead & unconscious combatants don't see anything.
            if (!CanTakeOrders)
            {
                ClearOpponentsInView();
            }
            // dead combatants don't block travel or line of sight
            if (IsDead)
            {
                battlescape.Terrain.RemoveCombatant(this);
                //ToDo: Play(ActionSound.Death);
            }
        }

        /// <summary>Find path from combatant's current location to other place on battlescape</summary>
        /// <param name="destination">combatant's destination</param>
        /// <param name="path">found path to destination</param>
        /// <returns>true if a path was found</returns>
        public bool FindPath(Vector3 destination, IList<MoveData> path)
        {
            return battlescape.Terrain.Pathfinder.FindPath(
                (int)position.X, (int)position.Y, (int)position.Z,
                Flyer,
                (int)destination.X, (int)destination.Y, (int)destination.Z,
                path);
        }

        /// <summary>
        /// Calculate shortest angle combatant needs to turn through to face a specifed position on the terrain
        /// </summary>
        /// <param name="pos">target cell combatant is to face</param>
        /// <returns>shortest angle, in radians</returns>
        public double CalcTurnAngle(Vector3 pos)
        {
            MoveData origin = new MoveData(Position);
            MoveData dest = new MoveData(pos);
            return Terrain.CalcTurnAngle(Heading, dest.X - origin.X, dest.Z - origin.Z);
        }

        /// <summary>Apply one day of healing to combatant</summary>
        public void DailyHealing()
        {
            if (IsInjured)
            {
                --stats[Statistic.InjuryDamage];
            }
        }

        /// <summary>Called after battlescape if X-Corp soldier died</summary>
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

        /// <summary>Null out battlescape references, so garbage collector gets battlescape</summary>
        public void PostMissionCleanup()
        {
            ai          = null;
            battlescape = null;
        }

        /// <summary>Is combatant standing on an X-Corp exit tile?</summary>
        /// <returns>true if combatant is on an exit tile</returns>
        public bool IsOnExitTile()
        {
            return battlescape.Terrain.GetGroundFace((int)position.X, (int)position.Y, (int)position.Z).IsExitTile;
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
                this.graphic = Xenocide.StaticTables.ItemList[Armor.Id].BattlescapeInfo.Graphic;
            }
            this.flyer = Armor.Flyer;
        }

        /// <summary>Set 3D Model to that of X-Corp soldier with no armor</summary>
        private void UseUnarmoredXCorpSolider()
        {
            this.graphic = new Graphic(@"Characters\XCorp\FemaleShirt", 0, MathHelper.PiOver2, 0);
        }

        /// <summary>Tag combatant as seeing nothing</summary>
        /// <remarks>Used when unit is unconscious/dead</remarks>
        private void ClearOpponentsInView()
        {
            int opposingTeam  = (Team.Aliens == teamId) ? Team.XCorp : Team.Aliens;
            int combatantFlag = ~(1 << PlaceInTeam);
            foreach(Combatant combatant in battlescape.Teams[opposingTeam].Combatants)
            {
                combatant.OponentsViewing &= combatantFlag;
            }
            OpponentsInView = 0;
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

        /// <summary>Location on battlescape (in cells)</summary>
        public Vector3 Position { get { return position; } set { position = value; } }

        /// <summary>Direction facing, in radians.  0 = along positive X axis, clockwise is positive</summary>
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
        public Vector3 HeadingVector
        {
            get { return new Vector3((float)Math.Cos(heading), 0, (float)-Math.Sin(heading)); }
        }

        /// <summary>Assorted properties</summary>
        public CombatantInfo CombatantInfo { get { return combatantInfo; } }

        /// <summary>3D model to draw on battlescpe</summary>
        public Graphic Graphic { get { return graphic; } set { graphic = value; } }

        /// <summary>The various numerical values describing a soldier's capabilities</summary>
        public Stats Stats { get { return stats; } }

        /// <summary>Order combatant is currently performing</summary>
        public Order Order
        {
            get { return order; }
            set { Debug.Assert((null == order) || (null == value)); order = value; }
        }

        /// <summary>Can this combatant fly?</summary>
        public bool Flyer { get { return flyer; } }

        /// <summary>Team (alien/X-Corp/Civilian) that owns this combatant</summary>
        public int TeamId { get { return teamId; } }

        /// <summary>Combatant's position in team array</summary>
        public int PlaceInTeam { get { return placeInTeam; } set { placeInTeam = value; } }

        /// <summary>Unique code to ID this combatant on a battlescape</summary>
        public byte CombatantId { get { return (byte)(((TeamId + 1) << 6) + PlaceInTeam); } }

        /// <summary>Does combatant have injuries</summary>
        public bool IsInjured { get { return 0 < stats[Statistic.InjuryDamage]; } }

        /// <summary>Has combatant been killed?</summary>
        public bool IsDead { get { return stats[Statistic.Health] < stats[Statistic.InjuryDamage]; } }

        /// <summary>Is combatant not dead and not stunned?</summary>
        public bool CanTakeOrders
        {
            get { return (stats[Statistic.InjuryDamage] + stats[Statistic.StunDamage]) <= stats[Statistic.Health]; }
        }

        /// <summary>Set of bits indicating the enemy forces that this combatant can see</summary>
        public int OpponentsInView { get { return opponentsInView; } set { opponentsInView = value; } }

        /// <summary>Set of bits indicating the enemy forces that can see this combatant</summary>
        public int OponentsViewing { get { return oponentsViewing; } set { oponentsViewing = value; } }

        /// <summary>The AI directly responsible for this combatant</summary>
        public CombatantAI AI { get { return ai; } set { ai = value; } }

        /// <summary>Battlescape combatant is currently on</summary>
        public Battle Battlescape { get { return battlescape; } set { battlescape = value; } }

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

        /// <summary>Order combatant is currently performing</summary>
        [NonSerialized]
        private Order order;

        /// <summary>Can this combatant fly?</summary>
        private bool flyer;

        /// <summary>Team (alien/X-Corp/Civilian) that owns this combatant</summary>
        private int teamId;

        /// <summary>Combatant's position in team array</summary>
        private int placeInTeam;

        /// <summary>Acts done this battlescape mission that qualify as learning experience</summary>
        private Experience experience = new Experience();

        /// <summary>Set of bits indicating the enemy forces that this combatant can see</summary>
        private int opponentsInView;

        /// <summary>Set of bits indicating the enemy forces that can see this combatant</summary>
        private int oponentsViewing;

        /// <summary>The AI directly responsible for this combatant</summary>
        private CombatantAI ai;

        /// <summary>Battlescape combatant is currently on</summary>
        private Battle battlescape;

        #endregion Fields
    }
}
