using System.Collections.Generic;

using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Utils;

using Xenocide.Resources;

namespace ProjectXenocide.UI.Screens
{
    public partial class AssignToCraftScreen
    {
        /// <summary>
        /// Handles all craft assignment game logic: assigning/unassigning soldiers and
        /// xcaps to aircraft, repositioning soldiers, and capacity validation. Separated
        /// from the GUI layer to enable unit testing of business rules.
        /// </summary>
        /// <remarks>
        /// ARCHITECTURE: This controller owns all game state mutations for craft assignment.
        /// The Screen class delegates to this controller for all business logic and updates
        /// GUI grids based on results.
        ///
        /// GAME MECHANICS:
        /// - Each aircraft has MaxHumans crew slots and MaxXcaps equipment slots
        /// - Soldiers can only be assigned to one aircraft at a time
        /// - Soldiers have a numbered position (1-N) that determines boarding order
        /// - Xcaps are physical items moved between outpost inventory and aircraft storage
        /// - Only aircraft InBase and CanCarrySoldiers are available for assignment
        /// </remarks>
        private class Controller
        {
            private readonly Outpost outpost;

            public Controller(Outpost outpost)
            {
                this.outpost = outpost;
            }

            /// <summary>
            /// Assigns a soldier to an aircraft. Returns true on success.
            /// Shows message box on validation failure.
            /// </summary>
            public static bool TryAssignSoldierToCraft(Aircraft aircraft, Person soldier)
            {
                if (null == aircraft || null == soldier)
                    return false;

                Aircraft craftWithSoldier = soldier.Aircraft;

                if (aircraft.Soldiers.Count < aircraft.MaxHumans)
                {
                    if (null == craftWithSoldier)
                    {
                        aircraft.Soldiers.Add(soldier, GetNextAvailablePosition(aircraft));
                        return true;
                    }
                    else if (aircraft == craftWithSoldier)
                    {
                        Util.ShowMessageBox(Strings.MSGBOX_SOLDIER_ALREADY_ASSIGNED_THIS_CRAFT);
                    }
                    else
                    {
                        Util.ShowMessageBox(Strings.MSGBOX_SOLDIER_ALREADY_ASSIGNED_OTHER_CRAFT);
                    }
                }
                else
                {
                    Util.ShowMessageBox(Strings.MSGBOX_CRAFT_FULL_HUMANS);
                }

                return false;
            }

            /// <summary>
            /// Unassigns a soldier from their aircraft. Returns true on success.
            /// Shows message box if soldier is not assigned.
            /// </summary>
            public static bool TryUnassignSoldierFromCraft(Person soldier)
            {
                if (null == soldier)
                    return false;

                Aircraft craftWithSoldier = soldier.Aircraft;

                if (null == craftWithSoldier)
                {
                    Util.ShowMessageBox(Strings.MSGBOX_SOLDIER_NOT_ASSIGNED);
                }
                else
                {
                    craftWithSoldier.Remove(soldier);
                    return true;
                }

                return false;
            }

            /// <summary>
            /// Assigns an xcap item to an aircraft. Moves item from outpost inventory to aircraft.
            /// Returns true on success. Shows message box on validation failure.
            /// </summary>
            public bool TryAssignXcapToCraft(Aircraft aircraft, Item xcap)
            {
                if (null == aircraft || null == xcap)
                    return false;

                if (outpost.Inventory.NumberInArmory(xcap.ItemInfo.Id) == 0)
                {
                    Util.ShowMessageBox(Strings.MSGBOX_NO_MORE_XCAPS_OUTPOST);
                }
                else if (aircraft.XCaps.Count < aircraft.MaxXcaps)
                {
                    outpost.Inventory.Remove(xcap);
                    aircraft.XCaps.Add(xcap);
                    return true;
                }
                else
                {
                    Util.ShowMessageBox(Strings.MSGBOX_CRAFT_FULL_XCAPS);
                }

                return false;
            }

            /// <summary>
            /// Unassigns an xcap item from an aircraft. Moves item from aircraft to outpost inventory.
            /// Returns true on success. Shows message box on validation failure.
            /// </summary>
            public bool TryUnassignXcapFromCraft(Aircraft aircraft, Item xcap)
            {
                if (null == aircraft || null == xcap)
                    return false;

                if (aircraft.XCaps.Count == 0)
                {
                    Util.ShowMessageBox(Strings.MSGBOX_NO_MORE_XCAPS_CRAFT);
                }
                else
                {
                    aircraft.XCaps.Remove(xcap);
                    outpost.Inventory.Add(xcap, false);
                    return true;
                }

                return false;
            }

            /// <summary>
            /// Repositions a soldier within an aircraft's crew list. Swaps positions
            /// with the soldier at the target position. Returns true on success.
            /// </summary>
            public static bool TryRepositionSoldier(Person soldier, int distance)
            {
                if (null == soldier)
                    return false;

                Aircraft craft = soldier.Aircraft;
                if (null == craft)
                {
                    Util.ShowMessageBox(Strings.MSGBOX_SOLDIER_NOT_ASSIGNED);
                    return false;
                }

                int newPosition = craft.Soldiers[soldier] + distance;

                if (newPosition < 1 || newPosition > craft.MaxHumans)
                {
                    Util.ShowMessageBox(Strings.MSGBOX_NO_POSITION);
                    return false;
                }

                // Swap positions with soldier at target position
                foreach (KeyValuePair<Person, int> pair in craft.Soldiers)
                {
                    if (pair.Value == newPosition)
                    {
                        craft.Soldiers[pair.Key] = craft.Soldiers[soldier];
                        break;
                    }
                }

                craft.Soldiers[soldier] = newPosition;
                return true;
            }

            /// <summary>
            /// Counts how many items of a given type are on an aircraft.
            /// </summary>
            public static int CountItemsOnCraft(string type, Aircraft aircraft)
            {
                int count = 0;
                foreach (Item xcap in aircraft.XCaps)
                {
                    if (xcap.ItemInfo.Id == type)
                        count++;
                }
                return count;
            }

            /// <summary>
            /// Gets the next available crew position on an aircraft (1-based).
            /// </summary>
            public static int GetNextAvailablePosition(Aircraft aircraft)
            {
                for (int i = 1; i <= aircraft.MaxHumans; i++)
                {
                    if (!aircraft.Soldiers.ContainsValue(i))
                        return i;
                }
                return 0;
            }
        }
    }
}
