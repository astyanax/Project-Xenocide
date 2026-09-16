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
* @file GeoscapeScreen.cs
* @date Created: 2007/01/21
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;

using Gum.Forms;
using Gum.Forms.Controls;
using Gum.Wireframe;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoGameGum.GueDeriving;

using NLog;

using ProjectXenocide.Assets;
using ProjectXenocide.Model;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.AI;
using ProjectXenocide.Model.Geoscape.Geography;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.UI.Controls;
using ProjectXenocide.UI.Dialogs;
using ProjectXenocide.UI.Scenes.Geoscape;
using ProjectXenocide.Utils;

using Xenocide.Resources;


#endregion

namespace ProjectXenocide.UI.Screens
{
    /// <summary>
    /// The Geoscape screen
    /// </summary>
    public partial class GeoscapeScreen : PolarScreen
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// constructor (obviously)
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope",
           Justification = "FxCop False Positive")]
        public GeoscapeScreen()
            : base("GeoscapeScreen")
        {
            Scene = new GeoscapeScene(oldCameraPosition);
            State = new ViewGeoscapeScreenState(this);
            if (Xenocide.AudioSystem != null)
                Xenocide.AudioSystem.PlayRandomMusic("PlanetView");
        }

        /// <summary>
        /// Implement Dispose
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    MessageLog.Changed -= RefreshLogPanel;

                    if (geoscapeScene != null)
                    {
                        geoscapeScene.Dispose();
                        Scene = null;
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        #region Gum controls

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        protected override void CreateGumControls()
        {
            var viewportLayout = new ScreenLayout { Mode = ViewportMode.SplitViewport };
            ViewportRect = viewportLayout.ViewportRect ?? new UiRect(0, 0, 0.745f, 1.0f);

            if (GumRoot != null)
            {
                WireButton("timeStopButton", OnTimeRateButtonClicked);
                WireButton("timeNormalButton", OnTimeRateButtonClicked);
                WireButton("timeHourButton", OnTimeRateButtonClicked);
                WireButton("timeDayButton", OnTimeRateButtonClicked);
                WireButton("interceptButton", OnInterceptButtonClicked);
                WireButton("basesButton", ShowBasesScreen);
                WireButton("researchButton", ShowResearchDialog);
                WireButton("fundingButton", OnFundingButtonClicked);
                WireButton("statisticsButton", ShowStatisticsScreen);
                WireButton("xnetButton", ShowXNetScreen);
                WireButton("optionsButton", ShowOptionsDialog);
                WireButton("cameraUpButton", OnMoveCameraButtonClicked);
                WireButton("cameraDownButton", OnMoveCameraButtonClicked);
                WireButton("cameraLeftButton", OnMoveCameraButtonClicked);
                WireButton("cameraRightButton", OnMoveCameraButtonClicked);
                WireButton("cameraInButton", OnMoveCameraButtonClicked);
                WireButton("cameraOutButton", OnMoveCameraButtonClicked);

                BuildHudPanel();

                InitializeMessageLog();

                State.CreateGumControls();
            }
        }

        private Label gameTimeTop;
        private Label gameTimeHour;
        private Label fundsText;
        private Label fundsAmount;
        private Label timeText;
        private ListBox _messageLogList;

        /// <summary>
        /// Builds the top-left HUD (date, clock, funds, GMT) inside a themed panel.
        /// A Forms <see cref="Panel"/> is used (rather than a bare StackPanel) so
        /// the software cursor treats the HUD as UI (arrow) instead of scene.
        /// </summary>
        private void BuildHudPanel()
        {
            var hud = new Panel();
            hud.Visual.X = 20;
            hud.Visual.Y = 10;
            hud.Visual.XUnits = Gum.Converters.GeneralUnitType.PixelsFromSmall;
            hud.Visual.YUnits = Gum.Converters.GeneralUnitType.PixelsFromSmall;
            hud.Visual.Width = 230;
            hud.Visual.WidthUnits = Gum.DataTypes.DimensionUnitType.Absolute;
            hud.Visual.Height = 104;
            hud.Visual.HeightUnits = Gum.DataTypes.DimensionUnitType.Absolute;

            var background = new ColoredRectangleRuntime();
            background.Color = new Color(10, 16, 28, 210);
            background.X = 0;
            background.Y = 0;
            background.XUnits = Gum.Converters.GeneralUnitType.PixelsFromSmall;
            background.YUnits = Gum.Converters.GeneralUnitType.PixelsFromSmall;
            background.Width = 0;
            background.WidthUnits = Gum.DataTypes.DimensionUnitType.RelativeToParent;
            background.Height = 0;
            background.HeightUnits = Gum.DataTypes.DimensionUnitType.RelativeToParent;
            hud.Visual.Children.Add(background);

            var stack = new StackPanel();
            stack.Visual.X = 8;
            stack.Visual.Y = 6;
            stack.Visual.XUnits = Gum.Converters.GeneralUnitType.PixelsFromSmall;
            stack.Visual.YUnits = Gum.Converters.GeneralUnitType.PixelsFromSmall;
            stack.Visual.Width = -16;
            stack.Visual.WidthUnits = Gum.DataTypes.DimensionUnitType.RelativeToParent;
            hud.AddChild(stack);

            gameTimeTop = ThemedLabel.CreateSection("");
            gameTimeHour = ThemedLabel.CreateBody("");
            fundsText = ThemedLabel.CreateCaption(Strings.SCREEN_GEOSCAPE_FUNDS);
            fundsAmount = ThemedLabel.CreateBody("");
            timeText = ThemedLabel.CreateCaption(Strings.SCREEN_GEOSCAPE_GMT);
            stack.AddChild(gameTimeTop);
            stack.AddChild(gameTimeHour);
            stack.AddChild(fundsText);
            stack.AddChild(fundsAmount);
            stack.AddChild(timeText);

            GumRoot.Children.Add(hud.Visual);

            var gameState = Xenocide.GameState;
            if (gameState?.GeoData?.XCorp?.Bank != null)
                fundsAmount.Text = gameState.GeoData.XCorp.Bank.DisplayCurrentBalance;
        }

        private Panel _logPanel;
        private Button _envelopeButton;

        private const int LogPanelWidth = 600;
        private const int LogPanelHeight = 190;
        private const int LogHeaderHeight = 26;

        /// <summary>
        /// Builds the bottom-left "Situation Log": a themed panel with a header,
        /// an INBOX button (pending-actions count) and the scrolling message list.
        /// </summary>
        private void InitializeMessageLog()
        {
            var vp = Xenocide.Instance.GraphicsDevice.Viewport;

            _logPanel = new Panel();
            _logPanel.Visual.X = 20;
            _logPanel.Visual.Y = vp.Height - LogPanelHeight - 20;
            _logPanel.Visual.XUnits = Gum.Converters.GeneralUnitType.PixelsFromSmall;
            _logPanel.Visual.YUnits = Gum.Converters.GeneralUnitType.PixelsFromSmall;
            _logPanel.Visual.Width = LogPanelWidth;
            _logPanel.Visual.WidthUnits = Gum.DataTypes.DimensionUnitType.Absolute;
            _logPanel.Visual.Height = LogPanelHeight;
            _logPanel.Visual.HeightUnits = Gum.DataTypes.DimensionUnitType.Absolute;

            var background = new ColoredRectangleRuntime();
            background.Color = new Color(10, 16, 28, 215);
            background.X = 0;
            background.Y = 0;
            background.XUnits = Gum.Converters.GeneralUnitType.PixelsFromSmall;
            background.YUnits = Gum.Converters.GeneralUnitType.PixelsFromSmall;
            background.Width = 0;
            background.WidthUnits = Gum.DataTypes.DimensionUnitType.RelativeToParent;
            background.Height = 0;
            background.HeightUnits = Gum.DataTypes.DimensionUnitType.RelativeToParent;
            _logPanel.Visual.Children.Add(background);

            var header = ThemedLabel.CreateSection("SITUATION LOG");
            header.Visual.X = 8;
            header.Visual.Y = 4;
            header.Visual.XUnits = Gum.Converters.GeneralUnitType.PixelsFromSmall;
            header.Visual.YUnits = Gum.Converters.GeneralUnitType.PixelsFromSmall;
            _logPanel.Visual.Children.Add(header.Visual);

            _envelopeButton = ThemedButton.Create("INBOX", OnEnvelopeClicked);
            ThemedButton.SetWidth(_envelopeButton, 130);
            _envelopeButton.Visual.Height = 22;
            _envelopeButton.Visual.HeightUnits = Gum.DataTypes.DimensionUnitType.Absolute;
            _envelopeButton.Visual.Y = 2;
            _envelopeButton.Visual.YUnits = Gum.Converters.GeneralUnitType.PixelsFromSmall;
            _envelopeButton.Visual.XOrigin = RenderingLibrary.Graphics.HorizontalAlignment.Right;
            _envelopeButton.Visual.XUnits = Gum.Converters.GeneralUnitType.PixelsFromLarge;
            _envelopeButton.Visual.X = -8;
            _logPanel.Visual.Children.Add(_envelopeButton.Visual);

            _messageLogList = new ListBox();
            _messageLogList.Visual.X = 6;
            _messageLogList.Visual.Y = LogHeaderHeight;
            _messageLogList.Visual.XUnits = Gum.Converters.GeneralUnitType.PixelsFromSmall;
            _messageLogList.Visual.YUnits = Gum.Converters.GeneralUnitType.PixelsFromSmall;
            _messageLogList.Visual.Width = -12;
            _messageLogList.Visual.WidthUnits = Gum.DataTypes.DimensionUnitType.RelativeToParent;
            _messageLogList.Visual.Height = -(LogHeaderHeight + 6);
            _messageLogList.Visual.HeightUnits = Gum.DataTypes.DimensionUnitType.RelativeToParent;
            _logPanel.Visual.Children.Add(_messageLogList.Visual);

            GumRoot.Children.Add(_logPanel.Visual);

            MessageLog.Changed += RefreshLogPanel;
            RefreshLogPanel();
        }

        /// <summary>Open the pending-actions inbox.</summary>
        private void OnEnvelopeClicked(object sender, EventArgs e)
        {
            ScreenManager.ShowDialog(new PendingActionsDialog());
        }

        /// <summary>Rebuild the log rows, badge and visibility from the message log.</summary>
        private void RefreshLogPanel()
        {
            if ((_messageLogList == null) || (_logPanel == null))
                return;

            _messageLogList.Items.Clear();
            foreach (var entry in MessageLog.Entries)
                _messageLogList.Items.Add(FormatLogEntry(entry));

            int pending = MessageLog.RequiredCount;
            _envelopeButton.Text = pending > 0 ? $"INBOX ({pending})" : "INBOX";

            _logPanel.Visual.Visible = MessageLog.Entries.Count > 0;

            // Auto-scroll to the newest message.
            if (_messageLogList.Items.Count > 0)
                _messageLogList.SelectedIndex = _messageLogList.Items.Count - 1;
        }

        private static string FormatLogEntry(MessageEntry entry)
        {
            string prefix = entry.Type switch
            {
                MessageType.Warning => "! ",
                MessageType.Error => "X ",
                MessageType.Required => "* ",
                _ => "  ",
            };
            return $"{entry.TimeString} {prefix}{entry.DisplayText}";
        }

        //Used to keep track of time and avoid updating if needed.
        private string gameTimeText;

        /// <summary>
        /// Adjust static text widget for displaying the time
        /// </summary>
        /// <param name="widget">static text widget to adjust</param>
        /// <param name="fontName">Name of font widget should be using</param>
        internal void AddControl(FrameworkElement control)
        {
            if (GumRoot != null)
                GumRoot.Children.Add(control.Visual);
            else if (RootContainer != null)
                RootContainer.AddChild(control);
        }

        #endregion Gum controls

        #region 3D scene management

        /// <summary>
        /// Update any model data
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            state.Update(gameTime);

            // update time shown on screen
            // Only update the time text if it has changed to avoid unnecessary UI recomposition
            DateTime time = Xenocide.GameState.GeoData.GeoTime.Time;
            String newTime = time.ToString(CultureInfo.InvariantCulture);

            if (gameTimeText != newTime)
            {
                CultureInfo culture = Thread.CurrentThread.CurrentCulture;

                //First we create the day, add on to it, add the month then the year.
                StringBuilder gametime = new StringBuilder(time.ToString("%d", culture));

                if (gametime.ToString() == "1")
                    gametime.Append("st");
                else if (gametime.ToString() == "2")
                    gametime.Append("nd");
                else if (gametime.ToString() == "3")
                    gametime.Append("rd");
                else
                    gametime.Append("th");

                //Change month to uppercase
                string upper = time.ToString("MMMM", culture);
                gametime.Append(' ');
                gametime.Append(upper.ToUpper(culture));
                gametime.Append(' ');
                gametime.Append(time.ToString("yyyy", culture));

                gameTimeTop.Text = gametime.ToString();
                gameTimeHour.Text = time.ToString("HH:mm:ss", culture);

                //Set time
                gameTimeText = newTime;
            }

            // Keep the funds readout current.
            string balance = Xenocide.GameState?.GeoData?.XCorp?.Bank?.DisplayCurrentBalance;
            if (balance != null && fundsAmount.Text != balance)
                fundsAmount.Text = balance;
        }

        /// <summary>
        /// Called to tell the Window prior to destruction to tell it to save any state information
        /// needed to reconstruct it's state.
        /// Basically, used to get the Geoscape to store the camera's position 
        /// when we go to a different screen.
        /// </summary>
        public override void SaveState()
        {
            oldCameraPosition = Scene.CameraPosition;
        }

        #endregion

        #region event handlers

        /// <summary>Replace this screen on display with the Bases Screen</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void ShowBasesScreen(object sender, EventArgs e)
        {
            state.OnBasesButton();
        }

        /// <summary>
        /// React to "Research" button being pressed
        /// </summary>
        private void ShowResearchDialog(object sender, EventArgs e)
        {
            state.OnResearchButton();
        }

        /// <summary>Put the options dialog on the display</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void ShowOptionsDialog(object sender, EventArgs e)
        {
            state.OnOptionsButton();
        }

        /// <summary>Respond to user clicking the "Funding" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnFundingButtonClicked(object sender, EventArgs e)
        {
            state.OnFundingButton();
        }

        /// <summary>Replace this screen on display with the Statistics Screen</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void ShowStatisticsScreen(object sender, EventArgs e)
        {
            state.OnStatisticsButton();
        }

        /// <summary>Replace this screen on display with the X-Net Screen</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void ShowXNetScreen(object sender, EventArgs e)
        {
            state.OnXNetButton();
        }

        /// <summary>Launch interceptor from base to attack UFO</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnInterceptButtonClicked(object sender, EventArgs e)
        {
            state.OnInterceptButton();
        }

        /// <summary>
        /// Return true if Button is the specified button
        /// </summary>
        /// <param name="button">button to examine</param>
        /// <param name="resourceName">Name of resource string to look for</param>
        /// <returns>True if Button's Name ends with the specified resource string</returns>
        private static bool IsButton(Button button, string resourceName)
        {
            string label = XenocideResourceManager.Get(resourceName);
            Debug.Assert(!String.IsNullOrEmpty(label));
            return button.Text.EndsWith(label, StringComparison.Ordinal);
        }

        /// <summary>React to user clicking on one of the "move camera" buttons</summary>
        /// <param name="sender">Button the user clicked</param>
        /// <param name="e">Not used</param>
        private void OnMoveCameraButtonClicked(object sender, EventArgs e)
        {
            const float rotation = (float)(Math.PI / 4);
            const float zoomStep = 0.5f;

            if (sender is Button btn)
            {
                string name = btn.Name ?? string.Empty;
                if (name.Contains("Left", StringComparison.OrdinalIgnoreCase))
                    Scene.RotateCamera(-rotation, 0.0f);
                else if (name.Contains("Right", StringComparison.OrdinalIgnoreCase))
                    Scene.RotateCamera(rotation, 0.0f);
                else if (name.Contains("Up", StringComparison.OrdinalIgnoreCase))
                    Scene.RotateCamera(0.0f, rotation);
                else if (name.Contains("Down", StringComparison.OrdinalIgnoreCase))
                    Scene.RotateCamera(0.0f, -rotation);
                else if (name.Contains("In", StringComparison.OrdinalIgnoreCase))
                    Scene.ZoomCamera(-zoomStep);
                else if (name.Contains("Out", StringComparison.OrdinalIgnoreCase))
                    Scene.ZoomCamera(zoomStep);
            }
        }

        /// <summary>React to user clicking on one of the "time rate" buttons</summary>
        /// <param name="sender">Button the user clicked</param>
        /// <param name="e">Not used</param>
        private void OnTimeRateButtonClicked(object sender, EventArgs e)
        {
            state.OnTimeRateButtonClicked(sender as Button);
        }

        /// <summary>React to user clicking left mouse button in the 3D geoscape scene</summary>
        /// <param name="relX">X co-ordinate relative to viewport (0-1)</param>
        /// <param name="relY">Y co-ordinate relative to viewport (0-1)</param>
        protected override void OnLeftMouseDownInScene(float relX, float relY)
        {
            GeoPosition pos = geoscapeScene.WindowToGeoPosition(new UiPoint(relX, relY));
            if (pos != null)
            {
                state.OnLeftMouseDownInScene(pos);
            }
        }

        /// <summary>React to user clicking the "Cancel setting new base location" button</summary>
        /// <param name="sender">Button the user clicked</param>
        /// <param name="e">Not used</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope",
           Justification = "FxCop False Positive")]
        private void OnCancelNewBase(object sender, EventArgs e)
        {
            ScreenManager.ScheduleScreen(new GeoscapeScreen());
        }

        #endregion event handlers

        /// <summary>
        /// Confirm user wants to build base here
        /// </summary>
        /// <param name="pos">location of new base</param>
        /// <param name="isFirstBase">is this player's first base?</param>
        private static void ConfirmBasePositionDialog(GeoPosition pos, bool isFirstBase)
        {
            if (Xenocide.GameState.GeoData.Planet.IsPositionOverWater(pos))
            {
                Util.ShowMessageBox(Strings.MSGBOX_CANNOT_BUILD_ON_WATER);
            }
            else
            {
                PlanetRegion region = Xenocide.GameState.GeoData.Planet.GetRegionAtLocation(pos);
                int cost = isFirstBase ? 0 : region.OutpostBuildCost;
                string area = region.ToString();

                if (Xenocide.GameState.GeoData.XCorp.Bank.CanAfford(cost))
                {
                    GumYesNoDialog dlg = GumYesNoDialog.OkCancelDialog(
                        Util.StringFormat(Strings.YESNOMSG_BUILD_BASE_HERE, Util.StringFormat("{0:N0}", cost), area)
                    );

                    // if yes is pressed, do the "base name" dialog
                    dlg.YesAction += delegate ()
                    {
                        if (!isFirstBase)
                        {
                            Xenocide.GameState.GeoData.XCorp.Bank.Debit(cost);
                        }
                        else
                        {
                            // Overmind starts sending missions near location of X-Corp base
                            GeoPosition target = pos.RandomLocationDistantBykm(500);
                            target = Xenocide.GameState.GeoData.Planet.GetClosestLand(target);
                            Xenocide.GameState.GeoData.Overmind.BeginFirstMissions(target);
                        }
                        Xenocide.ScreenManager.QueueDialog(new NameNewBaseDialog(pos, isFirstBase));
                    };

                    Xenocide.ScreenManager.ShowDialog(dlg);
                }
            }
        }

        /// <summary>
        /// Return the Craft (UFO or Aircraft) that is closest to position on the Geoscape.
        /// <remarks>
        /// Craft more than 500km away from position will be ignored.</remarks>
        /// </summary>
        /// <param name="pos">center of search area</param>
        /// <returns>closest craft, or null if none found</returns>
        private static Craft FindClosestCraft(GeoPosition pos)
        {
            double distance = GeoPosition.KilometersToRadians(501);
            Craft nearest = pos.FindClosest(Xenocide.GameState.GeoData.Overmind.Ufos, distance);
            if (null != nearest)
            {
                distance = pos.Distance(nearest.Position);
            }
            foreach (Outpost outpost in Xenocide.GameState.GeoData.Outposts)
            {
                foreach (Craft craft in outpost.Fleet)
                {
                    if (!craft.InBase && (craft.Position.Distance(pos) < distance))
                    {
                        nearest = craft;
                        distance = craft.Position.Distance(pos);
                    }
                }
            }
            return nearest;
        }

        #region fields

        /// <summary>
        /// What mode are we in?
        /// </summary>
        public ScreenState State
        {
            get { return state; }
            set { state = value; }
        }

        /// <summary>The current state's requested cursor (e.g. placement).</summary>
        public override UI.SoftwareCursor.CursorType? RequestedCursor => state?.RequestedCursor;

        /// <summary>
        /// Return the scene field as it's real type (a GeoscapeScene)
        /// </summary>
        private GeoscapeScene geoscapeScene { get { return (GeoscapeScene)Scene; } }

        /// <summary>
        /// What mode are we in?
        /// </summary>
        private ScreenState state;

        /// <summary>
        /// Remember where camera has been, to prevent Geoscape jumping around
        /// </summary>
        private static Vector3 oldCameraPosition = new Vector3(0.0f, 0.0f, 3.5f);


        #endregion fields
    }
}
