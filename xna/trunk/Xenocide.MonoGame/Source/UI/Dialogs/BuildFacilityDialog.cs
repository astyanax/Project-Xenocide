using System;
using System.Collections.Generic;
using System.Text;

using Gum.Forms.Controls;

using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.StaticData.Facilities;
using ProjectXenocide.UI.Screens;
using ProjectXenocide.Utils;

using Xenocide.Resources;

namespace ProjectXenocide.UI.Dialogs
{
    class BuildFacilityDialog : GumDialog
    {
        public BuildFacilityDialog(BasesScreen basesScreen)
        {
            this.basesScreen = basesScreen;
        }

        protected override void CreateGumWidgets()
        {
            var header = new Label();
            header.Text = "Select Facility";
            RootContainer.AddChild(header);

            int index = 0;
            foreach (FacilityInfo facility in Xenocide.StaticTables.FacilityList)
            {
                if (CanBuildFacility(facility.Id))
                {
                    int idx = index;
                    var row = new Label();
                    row.Text = string.Format("{0} - ${1} ({2}d, ${3}/mo)",
                        facility.Name, facility.BuildCost, facility.BuildDays, facility.MonthlyMaintenance);
                    RootContainer.AddChild(row);

                    var selectBtn = new Button();
                    selectBtn.Text = "Select";
                    selectBtn.Click += (s, e) => OnFacilitySelected(idx);
                    RootContainer.AddChild(selectBtn);
                }
                ++index;
            }

            var cancelBtn = new Button();
            cancelBtn.Text = Strings.BUTTON_CANCEL;
            cancelBtn.Click += OnCancelClicked;
            RootContainer.AddChild(cancelBtn);
        }

        private void OnFacilitySelected(int facilityIndex)
        {
            FacilityInfo info = Xenocide.StaticTables.FacilityList[facilityIndex];

            if (Xenocide.GameState.GeoData.XCorp.Bank.CanAfford(info.BuildCost))
            {
                if (info.LimitIsOnePerOutpost && (null != basesScreen.SelectedBaseFloorplan.FindUniqueFacility(info.Id)))
                {
                    Util.ShowMessageBox(Strings.MSGBOX_ONLY_ONE_FACILITY_PER_BASE, info.Name);
                }
                else
                {
                    basesScreen.BuildFacility(new FacilityHandle(facilityIndex));
                    ScreenManager.CloseDialog(this);
                }
            }
        }

        public void OnCancelClicked(object sender, EventArgs e)
        {
            ScreenManager.CloseDialog(this);
        }

        private static bool CanBuildFacility(string facilityId)
        {
            return (facilityId != "FAC_BASE_ACCESS_FACILITY") &&
                Xenocide.GameState.GeoData.XCorp.TechManager.IsAvailable(facilityId);
        }

        private BasesScreen basesScreen;
    }
}
