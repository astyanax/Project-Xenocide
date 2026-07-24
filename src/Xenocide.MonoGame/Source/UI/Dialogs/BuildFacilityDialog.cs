using System;
using System.Globalization;
using System.Text;

using Gum.Forms.Controls;

using NLog;

using ProjectXenocide.Assets;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.StaticData.Facilities;
using ProjectXenocide.UI.Screens;
using ProjectXenocide.Utils;

using Xenocide.Resources;

namespace ProjectXenocide.UI.Dialogs
{
    sealed class BuildFacilityDialog : ModalDialog
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public BuildFacilityDialog(BasesScreen basesScreen) : base("Select Facility")
        {
            this.basesScreen = basesScreen;
            PanelWidth = 600;
            PanelHeight = 500;
        }

        protected override void CreateDialogWidgets()
        {
            int index = 0;
            int buttonCount = 0;
            foreach (FacilityInfo facility in Xenocide.StaticTables.FacilityList)
            {
                if (CanBuildFacility(facility.Id))
                {
                    int idx = index;
                    // Compact single-button-per-facility: embed name, cost, build time
                    // and monthly maintenance in one clickable row so the dialog stays
                    // short enough to fit on screen without scrolling.
                    var rowBtn = new Button();
                    rowBtn.Text = string.Format(CultureInfo.InvariantCulture,
                        "{0}  —  ${1}  ({2}d, ${3}/mo)",
                        facility.Name, facility.BuildCost, facility.BuildDays, facility.MonthlyMaintenance);
                    // Stretch the button to fill the content panel width so the
                    // entire row is clickable and the text doesn't get clipped.
                    rowBtn.Visual.Width = 0;
                    rowBtn.Visual.WidthUnits = Gum.DataTypes.DimensionUnitType.RelativeToParent;
                    rowBtn.Click += (s, e) => OnFacilitySelected(idx);
                    ContentArea.AddChild(rowBtn);
                    ++buttonCount;
                }
                ++index;
            }

            Logger.Debug("CreateDialogWidgets: added {0} facility buttons (plus Cancel)", buttonCount);

            var cancelBtn = new Button();
            cancelBtn.Text = Strings.BUTTON_CANCEL;
            cancelBtn.Visual.Width = 0;
            cancelBtn.Visual.WidthUnits = Gum.DataTypes.DimensionUnitType.RelativeToParent;
            cancelBtn.Click += OnCancelClicked;
            ContentArea.AddChild(cancelBtn);
        }

        private void OnFacilitySelected(int facilityIndex)
        {
            FacilityInfo info = Xenocide.StaticTables.FacilityList[facilityIndex];
            Logger.Info("OnFacilitySelected: {0} (id={1}, cost=${2})",
                info.Name, info.Id, info.BuildCost);

            if (Xenocide.GameState.GeoData.XCorp.Bank.CanAfford(info.BuildCost))
            {
                if (info.LimitIsOnePerOutpost && (null != basesScreen.SelectedBaseFloorplan.FindUniqueFacility(info.Id)))
                {
                    Logger.Warn("OnFacilitySelected: {0} is limited to one per outpost and already built", info.Id);
                    Util.ShowMessageBox(Strings.MSGBOX_ONLY_ONE_FACILITY_PER_BASE, info.Name);
                }
                else
                {
                    Logger.Info("OnFacilitySelected: proceeding to placement for {0}", info.Id);
                    basesScreen.BuildFacility(new FacilityHandle(facilityIndex));
                    Close();
                }
            }
            else
            {
                Logger.Info("OnFacilitySelected: cannot afford {0} (cost=${1}, balance=${2})",
                    info.Id, info.BuildCost,
                    Xenocide.GameState.GeoData.XCorp.Bank.CurrentBalance);
                Xenocide.AudioSystem?.PlaySound(SoundId.Error);
                Util.ShowMessageBox(Strings.MSGBOX_INSUFFICIENT_FUNDS);
            }
        }

        public void OnCancelClicked(object sender, EventArgs e)
        {
            Close();
        }

        private static bool CanBuildFacility(string facilityId)
        {
            return (facilityId != "FAC_BASE_ACCESS_FACILITY") &&
                Xenocide.GameState.GeoData.XCorp.TechManager.IsAvailable(facilityId);
        }

        private BasesScreen basesScreen;
    }
}
