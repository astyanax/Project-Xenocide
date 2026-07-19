using System;

using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Utils;

using Xenocide.Resources;

namespace ProjectXenocide.UI.Screens
{
    public partial class EquipCraftScreen
    {
        /// <summary>
        /// Handles all game logic for equipping aircraft with weapons: emptying pods,
        /// equipping weapons, and inventory management.
        /// </summary>
        /// <remarks>
        /// ARCHITECTURE: This controller owns all game state mutations for craft equipping.
        /// The Screen class delegates to this controller for business logic and updates
        /// GUI elements based on results.
        ///
        /// GAME MECHANICS:
        /// - Aircraft have weapon pods (typically 2) that hold craft weapons
        /// - Weapons are taken from outpost inventory and placed in pods
        /// - Emptying a pod returns the weapon to outpost inventory
        /// - Only aircraft InBase can be modified
        /// </remarks>
        private class Controller
        {
            private readonly Outpost outpost;

            public Controller(Outpost outpost)
            {
                this.outpost = outpost;
            }

            /// <summary>
            /// Empties a weapon pod, returning the weapon to outpost inventory.
            /// </summary>
            /// <param name="aircraft">The aircraft to modify</param>
            /// <param name="podId">1-based pod index</param>
            /// <returns>True if pod was emptied</returns>
            public bool TryEmptyPod(Aircraft aircraft, int podId)
            {
                if (aircraft == null)
                    return false;

                if ((podId <= aircraft.WeaponPods.Count) && (null != aircraft.WeaponPods[podId - 1]))
                {
                    outpost.Inventory.Add(aircraft.WeaponPods[podId - 1], false);
                    aircraft.WeaponPods[podId - 1] = null;
                    return true;
                }

                return false;
            }

            /// <summary>
            /// Equips a weapon to a pod. The weapon must be in outpost inventory.
            /// </summary>
            /// <param name="aircraft">The aircraft to modify</param>
            /// <param name="podId">1-based pod index</param>
            /// <param name="weaponRow">The weapon row containing the weapon to equip</param>
            /// <returns>True if weapon was equipped</returns>
            public bool TryEquipPod(Aircraft aircraft, int podId, WeaponRow weaponRow)
            {
                if (aircraft == null || weaponRow == null)
                    return false;

                if (podId <= aircraft.WeaponPods.Count)
                {
                    return weaponRow.EquipPod(aircraft, podId);
                }

                return false;
            }

            /// <summary>
            /// Checks if a pod is empty and can be equipped.
            /// </summary>
            public static bool IsPodEmpty(Aircraft aircraft, int podId)
            {
                return aircraft != null &&
                       podId <= aircraft.WeaponPods.Count &&
                       aircraft.WeaponPods[podId - 1] == null;
            }

            /// <summary>
            /// Gets the number of empty pods on an aircraft.
            /// </summary>
            public static int GetEmptyPodCount(Aircraft aircraft)
            {
                if (aircraft == null)
                    return 0;

                int count = 0;
                foreach (var pod in aircraft.WeaponPods)
                {
                    if (pod == null)
                        count++;
                }
                return count;
            }
        }

        /// <summary>
        /// Represents a weapon row in the weapons grid, providing display data
        /// and equip functionality for a craft weapon item.
        /// </summary>
        private sealed class WeaponRow
        {
            public WeaponRow(Item item, OutpostInventory inventory)
            {
                this.item = item;
                this.inventory = inventory;
            }

            /// <summary>
            /// Equips this weapon to the specified pod on an aircraft.
            /// </summary>
            /// <param name="aircraft">The aircraft to equip</param>
            /// <param name="podId">1-based pod index</param>
            /// <returns>True if weapon was equipped</returns>
            public bool EquipPod(Aircraft aircraft, int podId)
            {
                if (null == aircraft.WeaponPods[podId - 1])
                {
                    WeaponPod pod = (WeaponPod)Weapon.Manufacture();
                    aircraft.WeaponPods[podId - 1] = pod;
                    inventory.Remove(pod);
                    return true;
                }
                else
                {
                    Util.ShowMessageBox(Strings.MSGBOX_POD_ALREADY_HAS_WEAPON);
                    return false;
                }
            }

            public int OnHand { get { return inventory.NumberInInventory(item.ItemInfo); } }

            public string ClipSize { get { return Weapon.ClipSizeString(); } }

            public string ClipsInBase
            {
                get
                {
                    if (null == Weapon.Clip)
                    {
                        return Strings.SCREEN_EQUIP_CRAFT_IRRELEVANT;
                    }
                    else
                    {
                        return Util.StringFormat("{0}", inventory.NumberInArmory(Weapon.Clip.Id));
                    }
                }
            }

            public String Name { get { return item.Name; } }

            private CraftWeaponItemInfo Weapon { get { return item.ItemInfo as CraftWeaponItemInfo; } }

            #region Fields

            private Item item;

            private OutpostInventory inventory;

            #endregion Fields
        }
    }
}
