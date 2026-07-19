using System;
using System.Collections.Generic;

using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.StaticData.Facilities;
using ProjectXenocide.Utils;

using Xenocide.Resources;

namespace ProjectXenocide.UI.Screens
{
    public partial class BaseInfoScreen
    {
        /// <summary>
        /// Handles all game logic for the base information screen: outpost name
        /// validation, staff count queries, and facility statistics.
        /// </summary>
        /// <remarks>
        /// ARCHITECTURE: This controller owns all game state queries and mutations
        /// for base information. The Screen class delegates to this controller for
        /// business logic and updates GUI grids based on results.
        ///
        /// GAME MECHANICS:
        /// - Outpost names must be unique (case-insensitive) and non-empty
        /// - Staff counts show idle vs total for each staff type
        /// - Facility stats show in-use, total, and under-construction counts
        /// - Defense strength sums all defense facility values
        /// </remarks>
        private class Controller
        {
            private readonly Outpost outpost;

            public Controller(Outpost outpost)
            {
                this.outpost = outpost;
            }

            /// <summary>
            /// Validates and attempts to rename the outpost.
            /// </summary>
            /// <param name="newName">Proposed new name</param>
            /// <returns>True if name was changed successfully</returns>
            public bool TryRenameOutpost(string newName)
            {
                // If name is identical, do nothing
                if (outpost.Name == newName)
                    return true;

                // Ensure something was given
                if (String.IsNullOrEmpty(newName))
                {
                    Util.ShowMessageBox(Strings.MSGBOX_BASE_NEEDS_NAME);
                    return false;
                }

                // See if name already exists for a different outpost (case-insensitive)
                foreach (Outpost other in Xenocide.GameState.GeoData.Outposts)
                {
                    if ((other != outpost)
                        && newName.Equals(other.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        Util.ShowMessageBox(Strings.MSGBOX_BASE_NAMES_ARE_UNIQUE, newName);
                        return false;
                    }
                }

                // Name is valid, update it
                outpost.Name = newName;
                Util.ShowMessageBox(Strings.MSGBOX_BASE_NAME_CHANGED);
                return true;
            }

            /// <summary>
            /// Gets the current outpost name (for GUI reset on validation failure).
            /// </summary>
            public string GetCurrentName()
            {
                return outpost.Name;
            }

            /// <summary>
            /// Gets staff counts (idle and total) for a given staff type.
            /// </summary>
            /// <param name="staffType">Staff type identifier (e.g., "ITEM_PERSON_SOLDIER")</param>
            /// <returns>Tuple of (idle count, total count)</returns>
            public (int idle, int total) GetStaffCounts(string staffType)
            {
                int total = Util.SequenceLength(outpost.ListStaff(staffType));
                int idle = Util.SequenceLength(outpost.ListStaff(staffType, false));
                return (idle, total);
            }

            /// <summary>
            /// Gets the display name for a staff type.
            /// </summary>
            public static string GetStaffTypeName(string staffType)
            {
                return Xenocide.StaticTables.ItemList[staffType].Name;
            }

            /// <summary>
            /// Calculates the outpost's defense strength.
            /// </summary>
            /// <returns>Tuple of (defensesInUse, totalDefenseStrength, defensesUnderConstruction)</returns>
            public (uint inUse, uint total, uint building) GetDefenseStrength()
            {
                uint totalDefense = 0;
                uint defensesInUse = 0;
                uint defensesUnderConstruction = 0;

                foreach (FacilityHandle f in outpost.Floorplan.Facilities)
                {
                    DefenseFacilityInfo df = f.FacilityInfo as DefenseFacilityInfo;
                    if (df != null)
                    {
                        if (!f.IsUnderConstruction)
                        {
                            totalDefense += (uint)df.DefenseStrength;
                            ++defensesInUse;
                        }
                        else
                        {
                            defensesUnderConstruction += (uint)df.DefenseStrength;
                        }
                    }
                }

                return (defensesInUse, totalDefense, defensesUnderConstruction);
            }

            /// <summary>
            /// Gets stats for a unique facility type.
            /// </summary>
            /// <param name="facilityId">Facility identifier</param>
            /// <returns>Tuple of (inUse, total, building), or null if facility not available</returns>
            public (uint inUse, uint total, uint building)? GetUniqueFacilityStats(string facilityId)
            {
                FacilityHandle facility = outpost.Floorplan.FindUniqueFacility(facilityId);
                String name = Xenocide.StaticTables.FacilityList[facilityId].Name;

                if (null == facility)
                {
                    if (Xenocide.GameState.GeoData.XCorp.TechManager.IsAvailable(facilityId))
                        return (0, 0, 0);
                    return null;
                }
                else if (facility.IsUnderConstruction)
                {
                    return (0, 0, 1);
                }
                else
                {
                    return (1, 1, 0);
                }
            }

            /// <summary>
            /// Gets the display name for a facility.
            /// </summary>
            public static string GetFacilityName(string facilityId)
            {
                return Xenocide.StaticTables.FacilityList[facilityId].Name;
            }
        }
    }
}
