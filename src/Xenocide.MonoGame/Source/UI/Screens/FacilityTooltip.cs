using System;
using System.Text;

using Gum.Forms.Controls;
using Gum.Wireframe;

using Microsoft.Xna.Framework;

using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.StaticData.Facilities;

namespace ProjectXenocide.UI.Screens
{
    /// <summary>
    /// Simple tooltip that displays facility information when hovering over
    /// cells in the base layout.  Uses a Gum Label added to the screen's
    /// RootContainer so the Gum system handles rendering automatically.
    /// </summary>
    internal sealed class FacilityTooltip : IDisposable
    {
        private readonly StackPanel panel;
        private readonly Label nameLabel;
        private readonly Label costLabel;
        private readonly Label maintenanceLabel;
        private readonly Label buildTimeLabel;
        private readonly Label capacityLabel;
        private bool visible;

        public FacilityTooltip(GraphicalUiElement root)
        {
            // Create a stack panel to hold the tooltip labels
            panel = new StackPanel();
            panel.Visual.Width = 260;
            panel.Visual.Height = 0; // auto
            panel.Visual.WidthUnits = Gum.DataTypes.DimensionUnitType.Absolute;
            panel.Visual.HeightUnits = Gum.DataTypes.DimensionUnitType.Absolute;
            panel.Visual.X = 0;
            panel.Visual.Y = 0;

            nameLabel = new Label();
            nameLabel.Text = "";
            costLabel = new Label();
            costLabel.Text = "";
            maintenanceLabel = new Label();
            maintenanceLabel.Text = "";
            buildTimeLabel = new Label();
            buildTimeLabel.Text = "";
            capacityLabel = new Label();
            capacityLabel.Text = "";

            panel.AddChild(nameLabel);
            panel.AddChild(costLabel);
            panel.AddChild(maintenanceLabel);
            panel.AddChild(buildTimeLabel);
            panel.AddChild(capacityLabel);

            // Add to Gum root so it renders automatically
            root.Children.Add(panel.Visual);
            visible = false;
        }

        public void ShowForFacility(Model.StaticData.Facilities.FacilityInfo info, Vector2 screenPos)
        {
            nameLabel.Text = info.Name;
            costLabel.Text = $"Cost: ${info.BuildCost:N0}";
            maintenanceLabel.Text = $"Maintenance: ${info.MonthlyMaintenance:N0}/mo";
            buildTimeLabel.Text = $"Build Time: {info.BuildDays} days";
            capacityLabel.Text = GetCapacityText(info);

            PositionTooltip(screenPos);
            panel.Visual.Visible = true;
            visible = true;
        }

        public void ShowForPlacement(FacilityHandle handle, Vector2 screenPos)
        {
            var info = handle.FacilityInfo;
            nameLabel.Text = info.Name + " (placing...)";
            costLabel.Text = $"Cost: ${info.BuildCost:N0}";
            maintenanceLabel.Text = $"Maintenance: ${info.MonthlyMaintenance:N0}/mo";
            buildTimeLabel.Text = $"Build Time: {info.BuildDays} days";
            capacityLabel.Text = GetCapacityText(info);

            PositionTooltip(screenPos);
            panel.Visual.Visible = true;
            visible = true;
        }

        public void ShowEmpty(Vector2 screenPos)
        {
            nameLabel.Text = "Empty Cell";
            costLabel.Text = "Left-click: Place facility (Build mode)";
            maintenanceLabel.Text = "Right-click: Demolish";
            buildTimeLabel.Text = "";
            capacityLabel.Text = "";

            PositionTooltip(screenPos);
            panel.Visual.Visible = true;
            visible = true;
        }

        public void Hide()
        {
            panel.Visual.Visible = false;
            visible = false;
        }

        private void PositionTooltip(Vector2 screenPos)
        {
            int x = (int)screenPos.X + 20;
            int y = (int)screenPos.Y - 20;

            // Keep tooltip on screen (assuming 1280x1024 default)
            if (x + 260 > 1280) x = 1280 - 260;
            if (y + 120 > 1024) y = 1024 - 120;

            panel.Visual.X = x;
            panel.Visual.Y = y;
        }

        private static string GetCapacityText(Model.StaticData.Facilities.FacilityInfo info)
        {
            // Check facility type via name pattern since we can't easily access specific types
            string name = info.Name.ToLowerInvariant();
            if (name.Contains("storage") || name.Contains("store"))
            {
                // Storage facilities - show generic capacity
                return "Capacity: Varies by type";
            }
            if (name.Contains("radar") || name.Contains("scanner") || name.Contains("neudar"))
            {
                return "Range/Decode: Varies";
            }
            if (name.Contains("defense") || name.Contains("missile") || name.Contains("laser") || name.Contains("plasma") || name.Contains("gravity"))
            {
                return "Range/Accuracy/Damage: Varies";
            }
            return "";
        }

        public void Dispose()
        {
            if (visible)
            {
                panel.RemoveFromRoot();
            }
        }
    }
}