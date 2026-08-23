using System;
using System.Collections.Generic;

using Gum.DataTypes;
using Gum.Forms;
using Gum.Forms.Controls;
using Gum.Wireframe;
using Microsoft.Xna.Framework;
using MonoGameGum;
using MonoGameGum.GueDeriving;

using ProjectXenocide.UI;

namespace ProjectXenocide.UI.Controls
{
    /// <summary>
    /// Defines how the ScreenLayout arranges its content relative to a 3D/2D scene.
    /// </summary>
    public enum ViewportMode
    {
        /// <summary>Standard layout: content area (left 75%) + button bar (right 190px). No scene viewport.</summary>
        Standard,

        /// <summary>Split layout: scene viewport (left 74.5%) + Gum UI on right 25.5%. Used by GeoscapeScreen, BattlescapeScreen, XNetScreen.</summary>
        SplitViewport,

        /// <summary>Full scene: scene occupies entire screen, Gum UI overlays on top. Used by EquipSoldierScreen, AeroscapeScreen.</summary>
        FullScene
    }

    /// <summary>
    /// Reusable screen layout component providing a standard structure:
    /// scrollable content area (left 75%), button bar (right 190px),
    /// and optional status bar (bottom 30px).
    ///
    /// Also supports viewport modes for screens with 3D/2D scenes:
    /// - SplitViewport: scene on left 74.5%, Gum UI on right 25.5%
    /// - FullScene: scene fills entire screen, Gum UI overlaid
    ///
    /// USAGE:
    ///   var layout = new ScreenLayout();
    ///   layout.Mode = ViewportMode.SplitViewport;
    ///   layout.AddToRoot();
    ///   // scene draws using layout.ViewportRect
    /// </summary>
    public class ScreenLayout
    {
        private const int ButtonBarWidthPx = 190;
        private const int ContentMarginPx = 10;
        private const int StatusBarHeightPx = 30;

        /// <summary>Default scene split: 74.5% for scene, remaining for Gum UI.</summary>
        private const float DefaultSceneWidthFraction = 0.745f;

        public ScreenLayout()
        {
            Visual = new ContainerRuntime();
            Visual.Width = 100;
            Visual.WidthUnits = Gum.DataTypes.DimensionUnitType.PercentageOfParent;
            Visual.Height = 100;
            Visual.HeightUnits = Gum.DataTypes.DimensionUnitType.PercentageOfParent;

            ContentScroll = new ScrollViewer();
            ContentScroll.Width = 75;
            ContentScroll.WidthUnits = Gum.DataTypes.DimensionUnitType.PercentageOfParent;
            ContentScroll.Height = -StatusBarHeightPx - ContentMarginPx;
            ContentScroll.HeightUnits = Gum.DataTypes.DimensionUnitType.RelativeToParent;
            Visual.Children.Add(ContentScroll.Visual);

            ContentPanel = new StackPanel();
            ContentPanel.Visual.Width = 0;
            ContentPanel.Visual.WidthUnits = Gum.DataTypes.DimensionUnitType.RelativeToParent;
            ContentScroll.Visual.Children.Add(ContentPanel.Visual);

            ButtonBar = new StackPanel();
            ButtonBar.Visual.Width = ButtonBarWidthPx;
            ButtonBar.Visual.WidthUnits = Gum.DataTypes.DimensionUnitType.Absolute;
            ButtonBar.Visual.Height = -StatusBarHeightPx - ContentMarginPx;
            ButtonBar.Visual.HeightUnits = Gum.DataTypes.DimensionUnitType.RelativeToParent;
            ButtonBar.Visual.X = 1280 - ButtonBarWidthPx - ContentMarginPx;
            ButtonBar.Visual.Y = ContentMarginPx;
            Visual.Children.Add(ButtonBar.Visual);

            StatusBar = new StackPanel();
            StatusBar.Visual.Height = StatusBarHeightPx;
            StatusBar.Visual.HeightUnits = Gum.DataTypes.DimensionUnitType.Absolute;
            StatusBar.Visual.Width = 0;
            StatusBar.Visual.WidthUnits = Gum.DataTypes.DimensionUnitType.RelativeToParent;
            StatusBar.Visual.Y = 1024 - StatusBarHeightPx - ContentMarginPx;
            Visual.Children.Add(StatusBar.Visual);

            ApplyLayout();
        }

        /// <summary>Root visual element to add to Gum root.</summary>
        public ContainerRuntime Visual { get; }

        /// <summary>Scrollable content area (left 75% of screen).</summary>
        public ScrollViewer ContentScroll { get; }

        /// <summary>StackPanel inside ContentScroll for adding content children.</summary>
        public StackPanel ContentPanel { get; }

        /// <summary>Vertical button bar (right 190px of screen).</summary>
        public StackPanel ButtonBar { get; }

        /// <summary>Horizontal status bar (bottom 30px of screen).</summary>
        public StackPanel StatusBar { get; }

        /// <summary>
        /// The viewport layout mode. Changing this repositions the button bar
        /// and content area to accommodate a 3D/2D scene.
        /// </summary>
        public ViewportMode Mode
        {
            get => _mode;
            set
            {
                _mode = value;
                ApplyLayout();
            }
        }

        /// <summary>
        /// The scene viewport rectangle in normalized coordinates (0-1).
        /// Computed from Mode and current window dimensions.
        /// Returns null for ViewportMode.Standard (no scene).
        /// </summary>
        public UiRect? ViewportRect
        {
            get
            {
                if (_mode == ViewportMode.Standard)
                    return null;

                var device = Xenocide.Instance?.GraphicsDevice;
                if (device == null)
                    return null;

                int vpW = device.Viewport.Width;
                int vpH = device.Viewport.Height;

                if (_mode == ViewportMode.SplitViewport)
                {
                    float sceneW = vpW * DefaultSceneWidthFraction;
                    float sceneH = vpH;
                    return new UiRect(0, 0, sceneW / vpW, sceneH / vpH);
                }

                // FullScene: entire window
                return new UiRect(0, 0, 1.0f, 1.0f);
            }
        }

        private ViewportMode _mode;

        /// <summary>
        /// Repositions child elements based on the current Mode.
        /// Standard: content on left, button bar on right.
        /// SplitViewport/FullScene: button bar on far right, content hidden (scene renders behind).
        /// </summary>
        private void ApplyLayout()
        {
            switch (_mode)
            {
                case ViewportMode.SplitViewport:
                    // Hide content scroll (scene renders in that area)
                    ContentScroll.Visual.Visible = false;

                    // Button bar on far right
                    ButtonBar.Visual.X = 1280 - ButtonBarWidthPx - ContentMarginPx;
                    ButtonBar.Visual.Y = ContentMarginPx;
                    ButtonBar.Visual.Height = -StatusBarHeightPx - ContentMarginPx;
                    ButtonBar.Visual.HeightUnits = Gum.DataTypes.DimensionUnitType.RelativeToParent;

                    // Status bar at bottom
                    StatusBar.Visual.Y = 1024 - StatusBarHeightPx - ContentMarginPx;
                    break;

                case ViewportMode.FullScene:
                    // Hide content scroll (scene renders across full screen)
                    ContentScroll.Visual.Visible = false;

                    // Button bar on far right
                    ButtonBar.Visual.X = 1280 - ButtonBarWidthPx - ContentMarginPx;
                    ButtonBar.Visual.Y = ContentMarginPx;
                    ButtonBar.Visual.Height = -StatusBarHeightPx - ContentMarginPx;
                    ButtonBar.Visual.HeightUnits = Gum.DataTypes.DimensionUnitType.RelativeToParent;

                    // Status bar at bottom
                    StatusBar.Visual.Y = 1024 - StatusBarHeightPx - ContentMarginPx;
                    break;

                default: // Standard
                    ContentScroll.Visual.Visible = true;

                    ButtonBar.Visual.X = 1280 - ButtonBarWidthPx - ContentMarginPx;
                    ButtonBar.Visual.Y = ContentMarginPx;
                    ButtonBar.Visual.Height = -StatusBarHeightPx - ContentMarginPx;
                    ButtonBar.Visual.HeightUnits = Gum.DataTypes.DimensionUnitType.RelativeToParent;

                    StatusBar.Visual.Y = 1024 - StatusBarHeightPx - ContentMarginPx;
                    break;
            }
        }

        /// <summary>Adds this layout to the Gum root.</summary>
        public void AddToRoot()
        {
            GumService.Default.Root.Children.Add(Visual);
            activeLayouts.Add(this);
        }

        /// <summary>Removes this layout from the Gum root.</summary>
        public void RemoveFromRoot()
        {
            GumService.Default.Root.Children.Remove(Visual);
            activeLayouts.Remove(this);
        }

        /// <summary>
        /// Removes all ScreenLayouts that were added via AddToRoot().
        /// Called by GumScreen.Dispose() to prevent leaked visuals
        /// accumulating in the Gum root across screen transitions.
        /// </summary>
        public static void RemoveAllFromRoot()
        {
            for (int i = activeLayouts.Count - 1; i >= 0; i--)
            {
                var layout = activeLayouts[i];
                GumService.Default.Root.Children.Remove(layout.Visual);
            }
            activeLayouts.Clear();
        }

        private static readonly List<ScreenLayout> activeLayouts = new List<ScreenLayout>();

        /// <summary>
        /// Creates and adds a XenocideButton to the button bar.
        /// Buttons use the textured X-COM 3-slice visual from the XenocideButton
        /// component (XenoNew.png atlas), matching all .gusx screen buttons.
        /// Falls back to a plain Forms Button if the template is unavailable.
        /// </summary>
        /// <param name="text">Button label text.</param>
        /// <param name="onClick">Click event handler.</param>
        /// <returns>The created Button for further customization.</returns>
        public Button AddButton(string text, EventHandler onClick)
        {
            var button = ThemedButton.Create(text, onClick);
            ButtonBar.AddChild(button);
            return button;
        }
    }
}
