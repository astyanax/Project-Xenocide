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
* @file Floorplan.cs
* @date Created: 2007/04/28
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;

using ProjectXenocide.Utils;
using ProjectXenocide.Model.StaticData;
using ProjectXenocide.Model.StaticData.Facilities;
using ProjectXenocide.Model.Geoscape.GeoEvents;
using ProjectXenocide.UI.Dialogs;

#endregion

namespace ProjectXenocide.Model.Geoscape.Outposts
{
    /// <summary>
    /// Represents the layout of facilities in an outpost
    /// </summary>
    [Serializable]
    public class Floorplan
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="statistics">Attributes of base that adding/subtracting facilities will modify</param>
        public Floorplan(OutpostStatistics statistics)
        {
            this.statistics = statistics;
        }
        
        /// <summary>
        /// Return the Facility at the given position
        /// </summary>
        /// <param name="x">position of facility</param>
        /// <param name="y">position of facility</param>
        /// <returns>the facility</returns>
        /// <remarks>returns null if there is no facility at that position</remarks>
        public FacilityHandle GetFacilityAt(int x, int y)
        {
            foreach (FacilityHandle handle in facilities)
            {
                if ((handle.X <= x) && (x < (handle.X + handle.FacilityInfo.XSize))
                    && (handle.Y <= y) && (y < (handle.Y + handle.FacilityInfo.YSize)))
                {
                    return handle;
                }
            }
            // no facility found
            return null;
        }

        /// <summary>
        /// Add a facility to the base
        /// </summary>
        /// <param name="handle">facility to add to base</param>
        public void AddFacility(FacilityHandle handle)
        {
            Debug.Assert(!AreCellsOccupied(handle));
            Debug.Assert(IsInsideBase(handle));
            
            facilities.Add(handle);
            handle.StartBuilding(statistics);
        }

        /// <summary>
        /// Remove a Facility from the base
        /// </summary>
        /// <param name="handle">position of facility</param>
        public void RemoveFacility(FacilityHandle handle)
        {
            facilities.Remove(handle);
            handle.Destroy(statistics);
        }

        /// <summary>
        /// Does base have any facilities?
        /// </summary>
        /// <returns>true if base has no facilities</returns>
        public bool IsBaseEmpty()
        {
            return (0 == facilities.Count);
        }

        /// <summary>
        /// Check if we can build the facility at this position in the base
        /// </summary>
        /// <param name="newFacility">facility to add to base</param>
        /// <returns>XenoError.None if is legal, otherwise a XenoError indicating why position isn't legal</returns>
        public XenoError IsPositionLegal(FacilityHandle newFacility)
        {
            // check facility is completely inside the base
            if (!IsInsideBase(newFacility))
            {
                return XenoError.PositionNotInBase;
            }

            // Check that all the cells wanted by this facility are empty
            if (AreCellsOccupied(newFacility))
            {
                return XenoError.PositionAlreadyOccupied;
            }

            // Check that there is a fully constructed facility adjacent to the new facility
            if (0 == GetNeighbours(newFacility, false).Count)
            {
                return XenoError.CellHasNoNeighbours;
            }

            // if get here, position is legal
            return XenoError.None;
        }

        /// <summary>
        /// Check if we can remove facility from the base
        /// </summary>
        /// <param name="facility">facility to remove</param>
        /// <returns>XenoError.None if is legal, otherwise a XenoError indicating why position isn't legal</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if facility == null")]
        public XenoError CanRemoveFacility(FacilityHandle facility)
        {
            // Can't delete storage facilities that are being used
            StorageFacilityInfo storage = facility.FacilityInfo as StorageFacilityInfo;
            if ((null != storage) && storage.IsFacilityInUse(statistics))
            {
                return XenoError.FacilityIsInUse;
            }
 
            // check if removing facility will split base
            if (WillRemovalSpiltBase(facility))
            {
                return XenoError.DeleteWillSplitBase;
            }

            // if get here, we can remove facility from the base
            return XenoError.None;
        }

        /// <summary>
        /// If there's a facility of this type in the base, return the handle to it
        /// </summary>
        /// <param name="id">Type of facility to look for</param>
        /// <returns>The facility, or null if no such type exists</returns>
        public FacilityHandle FindUniqueFacility(String id)
        {
            // Don't handle case of multiple facilities of same type in base
            Debug.Assert(Xenocide.StaticTables.FacilityList[id].LimitIsOnePerOutpost);
            return facilities.Find(delegate(FacilityHandle f) { return f.FacilityInfo.Id == id; } );
        }

        /// <summary>
        /// Check if floorplan has a working facility of specified type
        /// </summary>
        /// <param name="id">Type of facility to look for</param>
        /// <returns>true if base has facility, and it's fully built</returns>
        public bool HasWorkingFacility(String id)
        {
            return (null != facilities.Find(delegate(FacilityHandle f) {
                return (f.FacilityInfo.Id == id) && !f.IsUnderConstruction;
            }));
        }

        /// <summary>
        /// Set this floorplan with the facilities that the 1st base built gets
        /// </summary>
        public void SetupPlayersFirstBase()
        {
            foreach(StartSettings.StartingFacility sf in Xenocide.StaticTables.StartSettings.Facilities)
            {
                AddFacility(sf.Construct());
            }
        }

        /// <summary>
        /// Calculate how much the maintenance of the facilities in the base will cost this month
        /// </summary>
        /// <returns>the calculated cost</returns>
        public int CalcFacilityMaintenance()
        {
            // I'm a bastard, even facilities being built cost maintenance.
            int cost = 0;
            foreach (FacilityHandle facility in facilities)
            {
                cost += facility.FacilityInfo.MonthlyMaintenance;
            }
            return cost;
        }

        /// <summary>
        /// Owning outpost has been destroyed (by the aliens)
        /// </summary>
        public void OnOutpostDestroyed()
        {
            // any facilities under construction will have appointments.
            foreach (FacilityHandle facility in facilities)
            {
                facility.Destroy(statistics);
            }
            facilities.Clear();
        }

        /// <summary>
        /// Check that this facility will be entirely inside the base
        /// </summary>
        /// <param name="facility">facility to check</param>
        /// <returns>true if facility will be entirely within the base</returns>
        private bool IsInsideBase(FacilityHandle facility)
        {
            return
                (0.0f <= facility.X) &&
                (0.0f <= facility.X) &&
                ((facility.X + facility.FacilityInfo.XSize) <= CellsWide) &&
                ((facility.Y + facility.FacilityInfo.YSize) <= CellsHigh);
        }

        /// <summary>
        /// Check that all the cells wanted by this facility are empty
        /// </summary>
        /// <param name="facility">facility to check</param>
        /// <returns>true if something is already using one or more of the cells</returns>
        private bool AreCellsOccupied(FacilityHandle facility)
        {
            FacilityInfo info = facility.FacilityInfo;

            for (int x = facility.X; x < facility.X + info.XSize; ++x)
            {
                for (int y = facility.Y; y < facility.Y + info.YSize; ++y)
                {
                    if (null != GetFacilityAt(x, y))
                    {
                        return true;
                    }
                }
            }
            
            // if get here, no cell was in use
            return false;
        }

        /// <summary>
        /// Return a list of all the constructed facilities that border a facility
        /// </summary>
        /// <param name="facility">Facility to find the neighbours of</param>
        /// <param name="includeUnderConstruction">true if list should include the under construction facilities</param>
        /// <returns>The neighbours</returns>
        private List<FacilityHandle> GetNeighbours(FacilityHandle facility, bool includeUnderConstruction)
        {
            List<FacilityHandle> list = new List<FacilityHandle>();
            int xSize = facility.FacilityInfo.XSize;
            int ySize = facility.FacilityInfo.YSize;

            // top and bottom rows
            for (int x = facility.X; x < facility.X + xSize; ++x)
            {
                if (0 < facility.Y)
                {
                    AddUniqueToList(list, GetFacilityAt(x, facility.Y - 1), includeUnderConstruction);
                }

                if (facility.Y + ySize < CellsHigh)
                {
                    AddUniqueToList(list, GetFacilityAt(x, facility.Y + ySize), includeUnderConstruction);
                }
            }

            // left and right rows
            for (int y = facility.Y; y < facility.Y + ySize; ++y)
            {
                if (0 < facility.X)
                {
                    AddUniqueToList(list, GetFacilityAt(facility.X - 1, y), includeUnderConstruction);
                }

                if (facility.X + xSize < CellsWide)
                {
                    AddUniqueToList(list, GetFacilityAt(facility.X + xSize, y), includeUnderConstruction);
                }
            }

            return list;
        }

        /// <summary>
        /// Add a facility to list, if it's not already present in list
        /// </summary>
        /// <param name="list">the list</param>
        /// <param name="includeUnderConstruction">true if list should include the under construction facilities</param>
        /// <param name="facility">the facility</param>
        private static void AddUniqueToList(List<FacilityHandle> list, FacilityHandle facility, bool includeUnderConstruction)
        {
            if ((null != facility) && (!facility.IsUnderConstruction || includeUnderConstruction))
            {
                foreach (FacilityHandle f in list)
                {
                    if (f == facility)
                    {
                        return;
                    }
                }

                // if get here, facility isn't already in the list
                list.Add(facility);
            }
        }

        /// <summary>
        /// Check if removing facility will split base
        /// </summary>
        /// <param name="facility">the facility</param>
        /// <returns>true if removing facility will split the base</returns>
        private bool WillRemovalSpiltBase(FacilityHandle facility)
        {
            // Removing the access lift is not legal
            if ("FAC_BASE_ACCESS_FACILITY" == facility.FacilityInfo.Id)
            {
                return true;
            }

            // basic algorithm is start with access lift and build a spanning tree of
            // all the connected facilities.  When we're done, if the tree has
            // all the facilities, then removing the facility won't split the floorplan
            // Note that we can't have under construction facilities in the tree
            List<FacilityHandle> tree       = new List<FacilityHandle>();
            List<FacilityHandle> incomplete = new List<FacilityHandle>();
            
            //... first node is access lift (which should be 
            Debug.Assert("FAC_BASE_ACCESS_FACILITY" == facilities[0].FacilityInfo.Id);
            tree.Add(facilities[0]);
            int i = 0;
            while (i < tree.Count)
            {
                List<FacilityHandle> neighbours = GetNeighbours(tree[i], true);
                foreach (FacilityHandle f in neighbours)
                {
                    if (f != facility)
                    {
                        if (f.IsUnderConstruction)
                        {
                            AddUniqueToList(incomplete, f, true);
                        }
                        else
                        {
                            AddUniqueToList(tree, f, false);
                        }
                    }
                }
                ++i;
            }

            return tree.Count + incomplete.Count < (facilities.Count - 1);
        }

        #region Fields

        /// <summary>
        /// Width of base (along X axis) in cells
        /// </summary>
        public int CellsWide { get { return 6; } }

        /// <summary>
        /// Height of base (along Z axis) in cells
        /// </summary>
        public int CellsHigh { get { return 6; } }

        /// <summary>
        /// The facilities in the base
        /// </summary>
        public IList<FacilityHandle> Facilities { get { return facilities; } }

        /// <summary>
        /// The facilities in the base
        /// </summary>
        private List<FacilityHandle> facilities = new List<FacilityHandle>();

        /// <summary>
        /// Attributes of base that adding/subtracting facilities will modify
        /// </summary>
        private OutpostStatistics statistics;

        #endregion Fields

        #region UnitTests

        /// <summary>
        /// Run set of tests
        /// </summary>
        [Conditional("DEBUG")]
        public static void RunTests()
        {
            TestGetNeighbours();
            TestIsPositionLegal();
            TestWillRemovalSpiltBase();
        }

        /// <summary>
        /// Exercise GetNeighbours()
        /// </summary>
        /// <returns></returns>
        [Conditional("DEBUG")]
        private static void TestGetNeighbours()
        {
            // create floorplan with landing pad in middle
            OutpostStatistics statistics = new OutpostStatistics();
            Floorplan floorplan = new Floorplan(statistics);
            FacilityHandle pad1 = new FacilityHandle("FAC_LANDING_PAD", 2, 2);
            floorplan.AddFacility(pad1);

            // Confirm pad has no neighbours
            Debug.Assert(0 == floorplan.GetNeighbours(pad1, false).Count);

            // Now try above and below
            FacilityHandle pad2 = new FacilityHandle("FAC_LANDING_PAD", 2, 0, false);
            Debug.Assert(1 == floorplan.GetNeighbours(pad2, false).Count);
            pad2.Y = 4;
            Debug.Assert(1 == floorplan.GetNeighbours(pad2, false).Count);

            // Left and Right
            pad2.X = 0;
            pad2.Y = 2;
            Debug.Assert(1 == floorplan.GetNeighbours(pad2, false).Count);
            pad2.X = 4;
            Debug.Assert(1 == floorplan.GetNeighbours(pad2, false).Count);

            // Partially completed facility is ignored
            floorplan.RemoveFacility(pad1);
            pad1 = new FacilityHandle("FAC_LANDING_PAD", 2, 2, false);
            floorplan.AddFacility(pad1);
            Debug.Assert(0 == floorplan.GetNeighbours(pad2, false).Count);
            Debug.Assert(1 == floorplan.GetNeighbours(pad2, true).Count);
        }

        /// <summary>
        /// Exercise IsPositionLegal()
        /// </summary>
        /// <returns></returns>
        [Conditional("DEBUG")]
        private static void TestIsPositionLegal()
        {
            // create floorplan with access lift
            OutpostStatistics statistics = new OutpostStatistics();
            Floorplan floorplan = new Floorplan(statistics);
            FacilityHandle lift = new FacilityHandle("FAC_BASE_ACCESS_FACILITY", floorplan.CellsWide - 1, floorplan.CellsHigh - 1);
            floorplan.AddFacility(lift);

            // Check can't drawing a LandingPad too close to right and bottom margins
            FacilityHandle pad = new FacilityHandle("FAC_LANDING_PAD", floorplan.CellsWide - 1, floorplan.CellsHigh - 3, false);
            Debug.Assert(XenoError.PositionNotInBase == floorplan.IsPositionLegal(pad));
            --pad.X;
            Debug.Assert(XenoError.None == floorplan.IsPositionLegal(pad));
            pad.X = floorplan.CellsWide - 3;
            pad.Y = floorplan.CellsHigh - 1;
            Debug.Assert(XenoError.PositionNotInBase == floorplan.IsPositionLegal(pad));
            --pad.Y;
            Debug.Assert(XenoError.None == floorplan.IsPositionLegal(pad));
            
            // Check can't build a facility if there's one already there
            ++pad.X;
            Debug.Assert(XenoError.PositionAlreadyOccupied == floorplan.IsPositionLegal(pad));

            // Check that facility has a fully constructed neighbour
            //... no neighbour case
            pad.X = 2;
            pad.Y = 2;
            Debug.Assert(XenoError.CellHasNoNeighbours == floorplan.IsPositionLegal(pad));
            floorplan.AddFacility(pad);

            //... neighbour is under construction
            FacilityHandle pad2 = new FacilityHandle("FAC_LANDING_PAD", 2, 0);
            Debug.Assert(floorplan.IsPositionLegal(pad2) == XenoError.CellHasNoNeighbours);

            //... neighbour has been built
            pad.FinishedBuilding(floorplan.statistics);
            Debug.Assert(XenoError.None == floorplan.IsPositionLegal(pad2));
        }

        /// <summary>
        /// Exercise WillRemovalSpiltBase()
        /// </summary>
        /// <returns></returns>
        [Conditional("DEBUG")]
        private static void TestWillRemovalSpiltBase()
        {
            // create floorplan with simple layout
            //
            //         +------+------+
            //         |  S4  |  S3  |
            //  +------+------+------+
            //  | Lift |  S1  |  S2  |
            //  +------+------+------+
            //  |  S5  |  S6  |  S7  |
            //  +------+------+------+
            // S4 is under construction, rest a built
            Xenocide.GameState.SetToStartGameCondition();
            OutpostStatistics statistics = new OutpostStatistics();
            Floorplan floorplan = new Floorplan(statistics);
            FacilityHandle lift = new FacilityHandle("FAC_BASE_ACCESS_FACILITY", 0, 2);
            floorplan.AddFacility(lift);
            FacilityHandle store1 = new FacilityHandle("FAC_STORAGE_FACILITY", 1, 2);
            floorplan.AddFacility(store1);
            FacilityHandle store2 = new FacilityHandle("FAC_STORAGE_FACILITY", 2, 2);
            floorplan.AddFacility(store2);
            FacilityHandle store3 = new FacilityHandle("FAC_STORAGE_FACILITY", 2, 1);
            floorplan.AddFacility(store3);
            FacilityHandle store4 = new FacilityHandle("FAC_STORAGE_FACILITY", 1, 1, false);
            floorplan.AddFacility(store4);
            FacilityHandle store5 = new FacilityHandle("FAC_STORAGE_FACILITY", 0, 3);
            floorplan.AddFacility(store5);
            FacilityHandle store6 = new FacilityHandle("FAC_STORAGE_FACILITY", 1, 3);
            floorplan.AddFacility(store6);
            FacilityHandle store7 = new FacilityHandle("FAC_STORAGE_FACILITY", 2, 3);
            floorplan.AddFacility(store7);

            Debug.Assert(floorplan.WillRemovalSpiltBase(lift));
            Debug.Assert(floorplan.WillRemovalSpiltBase(store2));
            Debug.Assert(!floorplan.WillRemovalSpiltBase(store4));
            Debug.Assert(!floorplan.WillRemovalSpiltBase(store3));
            Debug.Assert(!floorplan.WillRemovalSpiltBase(store1));

            // use this floorplan to test capacities.
            OutpostCapacityInfo capacity = statistics.Capacities["STORAGE_GEAR"];
            Debug.Assert(capacity.Available == 500 * 6);
            Debug.Assert(capacity.Building == 500);
            floorplan.RemoveFacility(store3);
            Debug.Assert(capacity.Available == 500 * 5);
            floorplan.RemoveFacility(store4);
            Debug.Assert(capacity.Building == 0);

            // Test update
            floorplan.AddFacility(store4);
            Debug.Assert(capacity.Building == 500);
            Debug.Assert(capacity.Available == 500 * 5);
            Debug.Assert(store4.IsUnderConstruction);

            //... max step size is 1 day
            for (int i = 0; i < 11; ++i)
            {
                Xenocide.GameState.GeoData.GeoTime.AddMilliseconds(24.0 * 60 * 60 * 1000);
            }
            Debug.Assert(!store4.IsUnderConstruction);
            Debug.Assert(capacity.Building == 0);
            Debug.Assert(capacity.Available == 500 * 6);

            // more capacity checks
            Debug.Assert(XenoError.DeleteWillSplitBase == floorplan.CanRemoveFacility(lift));
            Debug.Assert(XenoError.None == floorplan.CanRemoveFacility(store4));
            capacity.Use((uint)(500 * 6));
            Debug.Assert(XenoError.FacilityIsInUse == floorplan.CanRemoveFacility(store4));
        }

        #endregion UnitTests
    }
}
