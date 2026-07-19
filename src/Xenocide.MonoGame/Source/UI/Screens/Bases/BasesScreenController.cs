using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.StaticData.Facilities;
using ProjectXenocide.Utils;

using Xenocide.Resources;

namespace ProjectXenocide.UI.Screens
{
    public partial class BasesScreen
    {
        /// <summary>
        /// Handles all game logic for the base management screen: facility placement
        /// validation, demolition with undo support, and base creation limits.
        /// </summary>
        /// <remarks>
        /// ARCHITECTURE: This controller owns all game state mutations for base management.
        /// The Screen class delegates to this controller for business logic and updates
        /// GUI elements based on results.
        ///
        /// GAME MECHANICS:
        /// - Facility placement requires valid position (adjacency for non-access-lift)
        ///   and sufficient funds
        /// - Demolition credits scrap revenue and can be undone once (Ctrl+Z)
        /// - Maximum 8 bases allowed
        /// - Only aircraft InBase and CanCarrySoldiers are available for assignment
        /// </remarks>
        private class Controller
        {
            private readonly Outpost outpost;

            /// <summary>
            /// Last demolished facility, stored for potential undo. Null if no undo available.
            /// </summary>
            private FacilityHandle lastDemolishedFacility;

            /// <summary>
            /// Base index from which the last facility was demolished (for undo validation).
            /// </summary>
            private int lastDemolishedBaseIndex = -1;

            public Controller(Outpost outpost)
            {
                this.outpost = outpost;
            }

            /// <summary>
            /// Checks if a new base can be created (under the 8-base limit).
            /// Shows message box if limit reached.
            /// </summary>
            public static bool CanCreateNewBase()
            {
                if (8 <= Xenocide.GameState.GeoData.Outposts.Count)
                {
                    Util.ShowMessageBox(Strings.MSGBOX_MAX_EIGHT_BASES);
                    return false;
                }
                return true;
            }

            /// <summary>
            /// Checks if a transfer can be started (at least 2 outposts required).
            /// Shows message box if insufficient.
            /// </summary>
            public static bool CanStartTransfer()
            {
                int numOutposts = Xenocide.GameState.GeoData.Outposts.Count;
                if (numOutposts < 2)
                {
                    Util.ShowMessageBox(Strings.MSGBOX_NEED_2_BASES_TO_TRANSFER);
                    return false;
                }
                return true;
            }

            /// <summary>
            /// Validates and places a facility at the specified cell coordinates.
            /// Debits the build cost from the bank if successful.
            /// </summary>
            /// <param name="handle">The facility to place</param>
            /// <param name="cellCoords">Top-left cell in floorplan grid</param>
            /// <param name="isAccessLiftMode">True if placing the first access lift (relaxed adjacency rules)</param>
            /// <returns>True if facility was placed successfully</returns>
            public bool TryAddFacility(FacilityHandle handle, Vector2 cellCoords, bool isAccessLiftMode)
            {
                // Set position on the handle
                handle.X = (SByte)cellCoords.X;
                handle.Y = (SByte)cellCoords.Y;

                // Check affordability
                int cost = handle.FacilityInfo.BuildCost;
                if (!Xenocide.GameState.GeoData.XCorp.Bank.CanAfford(cost))
                {
                    Util.ShowMessageBox(Strings.MSGBOX_INSUFFICIENT_FUNDS);
                    return false;
                }

                // Validate position
                XenoError error = outpost.Floorplan.IsPositionLegal(handle);

                if ((XenoError.None == error) ||
                    (isAccessLiftMode && (XenoError.CellHasNoNeighbours == error)))
                {
                    // Place the facility
                    Xenocide.GameState.GeoData.XCorp.Bank.Debit(cost);
                    outpost.Floorplan.AddFacility(handle);
                    return true;
                }
                else
                {
                    Util.ShowMessageBox(Util.GetErrorMessage(error));
                    return false;
                }
            }

            /// <summary>
            /// Checks if a facility at the given cell can be removed.
            /// Shows message box if removal is not possible.
            /// </summary>
            /// <param name="cellCoords">Cell coordinates to check</param>
            /// <returns>The facility handle if removable, null otherwise</returns>
            public FacilityHandle GetRemovableFacility(Vector2 cellCoords)
            {
                FacilityHandle facility = outpost.Floorplan.GetFacilityAt((int)cellCoords.X, (int)cellCoords.Y);
                if (facility == null)
                    return null;

                XenoError error = outpost.Floorplan.CanRemoveFacility(facility);
                if (XenoError.None != error)
                {
                    Util.ShowMessageBox(Util.GetErrorMessage(error));
                    return null;
                }

                return facility;
            }

            /// <summary>
            /// Removes a facility from the base and credits scrap revenue.
            /// Stores the demolished facility for potential undo.
            /// </summary>
            /// <param name="facility">The facility to remove</param>
            /// <param name="baseIndex">Index of the base being modified</param>
            public void DemolishFacility(FacilityHandle facility, int baseIndex)
            {
                // Store for undo
                lastDemolishedFacility = facility;
                lastDemolishedBaseIndex = baseIndex;

                // Credit scrap revenue and remove
                Xenocide.GameState.GeoData.XCorp.Bank.Credit(facility.FacilityInfo.ScrapRevenue);
                outpost.Floorplan.RemoveFacility(facility);
            }

            /// <summary>
            /// Undoes the last demolition by restoring the facility and debiting the refund.
            /// Only works if the undo buffer is valid and matches the current base.
            /// </summary>
            /// <param name="currentBaseIndex">Index of the currently selected base</param>
            /// <returns>True if undo was performed</returns>
            public bool TryUndoDemolition(int currentBaseIndex)
            {
                if (lastDemolishedFacility == null || lastDemolishedBaseIndex != currentBaseIndex)
                    return false;

                // Refund the scrap revenue
                Xenocide.GameState.GeoData.XCorp.Bank.Debit(lastDemolishedFacility.FacilityInfo.ScrapRevenue);

                // Re-add the facility
                outpost.Floorplan.AddFacility(lastDemolishedFacility);

                // Clear undo buffer
                lastDemolishedFacility = null;
                lastDemolishedBaseIndex = -1;

                return true;
            }

            /// <summary>
            /// Gets the current bank balance for display.
            /// </summary>
            public static string GetFundsDisplay()
            {
                return Util.StringFormat(Strings.SCREEN_BASES_FUNDS,
                    Xenocide.GameState.GeoData.XCorp.Bank.CurrentBalance);
            }
        }
    }
}
