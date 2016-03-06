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
* @file Frame.cs
* @date Created: 2007/03/04
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using CeGui.Renderers.Xna;

using Xenocide.Resources;

#endregion

namespace Xenocide.UI.Screens
{
    /// <summary>
    /// This is the base class that all screen and dialogs derive from
    /// The frame is responsible for:
    /// 1. Owning the window that holds all the widgets making the screen or dialog
    /// 2. utility functions for creating the widgets
    /// <remarks>Because the CeGui widgets are XNA Components
    /// the Frame class does NOT pump Update() and Draw() to the widgets
    /// themselves.  That's done by the XNA framework</remarks>
    /// </summary>
    public abstract class Frame : DrawableGameComponent, IDisposable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ceguiId">ID that CeGui will use to refer to this frame</param>
        /// <param name="screenManager">the Screen Manager</param>
        /// <param name="size">Dimensions of the frame</param>
        protected Frame(Game game, string ceguiId, System.Drawing.SizeF size)
            : base(game)
        {
            this.ceguiId = ceguiId;            
            this.size = size;

            this.unimplemented = new CeGui.GuiEventHandler(notImplementedYet);
            this.OnGoToGeoscapeScreen = new CeGui.GuiEventHandler(OnShowGeoscapeScreen);

            EnabledChanged += delegate(object sender, EventArgs args)
                {
                    Enable(this.Enabled);
                };
        }

        public override void Initialize()
        {
            this.screenManager = (IScreenManager)Game.Services.GetService(typeof(IScreenManager));
            base.Initialize();
            Show();
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
        
        /// <summary>
        /// Puts the frame onto the display
        /// </summary>
        private void Show()
        {
            // create the frame and put it on the display
            frameWidget = GuiBuilder.CreateFrameWindow(ceguiId);

            //ToDo: Need to set window to have correct parent
            GuiSheet.AddChild(frameWidget);

            frameWidget.Text = ceguiId;
            frameWidget.MetricsMode = CeGui.MetricsMode.Relative;
            frameWidget.Position = CalculatePosition();
            frameWidget.Size = size;
            frameWidget.MinimumSize = size;
            
            // now add the other items to the frame
            CreateCeguiWidgets();
        }

        /// <summary>
        /// Determine where to position frame so that it's in center of display
        /// </summary>
        /// <returns>co-ordinates for top left corner of frame</returns>
        System.Drawing.PointF CalculatePosition()
        {
            return new System.Drawing.PointF
            (
                (1.0f - size.Width) / 2.0f,
                (1.0f - size.Height) / 2.0f
            );
        }

        /// <summary>
        /// Implement IDisposable
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Removes the frame from the display
        /// Also, completely destroys all the Cegui widgets associated
        /// with the frame
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                CeGui.WindowManager.Instance.DestroyWindow(frameWidget);
            }
            frameWidget = null;
        }

        /// <summary>
        /// Enable/Disable this frame (and it's child widgets)
        /// </summary>
        /// <param name="enableFrame">true to enable</param>
        private void Enable(bool enableFrame)
        {
            if (enableFrame)
            {
                frameWidget.Enable();
            }
            else
            {
                frameWidget.Disable();
            }
        }

        /// <summary>
        /// populate the fame with the Cegui Widgets
        /// </summary>
        protected abstract void CreateCeguiWidgets();

        /// <summary>
        /// Retreive the Gui Sheet that "hosts" the Form
        /// </summary>
        /// <returns>The Sheet</returns>
        /// RK:  BTW use a property named Sheet instead.
        /// DT: Except the Gui Sheet ISN'T owned by the frame.
        protected virtual CeGui.GuiSheet GuiSheet
        {
            get { return ScreenManager.RootGuiSheet; }
        }

        /// <summary>
        /// Put a wigdet at the specified co-ordinates (and size) on this frame
        /// </summary>
        /// <param name="widget">The widget to place</param>
        /// <param name="left">Position on frame for widget's left edge</param>
        /// <param name="top">Position on frame for widget's top edge</param>
        /// <param name="width">Width of widget (relative to frame)</param>
        /// <param name="height">Height of widget (relative to frame)</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if widget == null")]
        protected void AddWidget(CeGui.Window widget, float left, float top, float width, float height)
        {
            frameWidget.AddChild(widget);

            // ToDo: we're using Position and Size because Top, Left, Width and Height
            // properties are currently broken in CeGui
            widget.MetricsMode = CeGui.MetricsMode.Relative;
            widget.Position = new System.Drawing.PointF(left, top);
            widget.Size = new System.Drawing.SizeF(width, height);
        }

        /// <summary>
        /// Create the specified button and put it on this frame
        /// </summary>
        /// <param name="resourceName">Name of resource string for label to put on button</param>
        /// <param name="left">Position on frame for button's left edge</param>
        /// <param name="top">Position on frame for button's top edge</param>
        /// <param name="width">Width of button (relative to frame)</param>
        /// <param name="height">Height of button (relative to frame)</param>
        /// <returns>The button that has been created</returns>
        public
        CeGui.Widgets.PushButton
        AddButton(
            string resourceName,
            float left, float top, float width, float height)
        {
            string label = Strings.ResourceManager.GetString(resourceName);
            Debug.Assert((!String.IsNullOrEmpty(label)));
            CeGui.Widgets.PushButton button = GuiBuilder.CreateButton(ceguiId + '_' + label);
            button.Text = label;
            AddWidget(button, left, top, width, height);
            return button;
        }

        /// <summary>CeGui widget that is the Frame window holding all other widgets in the window</summary>
        protected CeGui.Widgets.FrameWindow FrameWidget { get { return frameWidget; } }

        /// <summary>CeGui widget that is the Frame window holding all other widgets in the window</summary>
        private CeGui.Widgets.FrameWindow frameWidget;

        /// <summary>The identifer (name) that CeGui will use for this frame</summary>
        /// RK: There is no other ID you may require from a frame anyways, why not call it Id (and use it widely ;) ).
        private string ceguiId;

        /// <summary>
        /// The identifer (name) that CeGui will use for this frame
        /// </summary>
        protected string CeguiId { get { return ceguiId; } }

        /// <summary>
        /// The ScreenManager
        /// </summary>
        private IScreenManager screenManager;

        /// <summary>
        /// The ScreenManager
        /// </summary>
        protected IScreenManager ScreenManager { get { return screenManager; } }

        /// <summary>
        /// The dimensions of the frame (in relative co-ordinates
        /// </summary>
        private System.Drawing.SizeF size;

        /// <summary>
        /// Helper function to get the GuiBuilder used to create widgets to put on form
        /// </summary>
        protected CeGui.GuiBuilder GuiBuilder { get { return ScreenManager.GuiBuilder; } }
        
        #region Commonly used event handlers

        /// <summary>
        /// Default "on clicked" handler for buttons to use while development in progress
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields",
            Justification = "ToDo: stub function while construction in progress, will remove later")]
        protected CeGui.GuiEventHandler unimplemented;

        /// <summary>Default delegate, when we don't have a handler for a button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions",
            Justification = "ToDo: stub function while construction in progress, will remove later")]
        private void notImplementedYet(object sender, CeGui.GuiEventArgs e)
        {
            System.Windows.Forms.MessageBox.Show(Strings.EXCEPTION_NOT_IMPLEMENTED);
        }

        /// <summary>
        /// Set Geoscape screen as main screen
        /// </summary>
        protected CeGui.GuiEventHandler OnGoToGeoscapeScreen;

        /// <summary>Replace this screen on display with the Geoscape Screen</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        /// RK: Definitely not the place for this functionality ;)
        private void OnShowGeoscapeScreen(object sender, CeGui.GuiEventArgs e)
        {
            screenManager.ScheduleScreen(new GeoscapeScreen(Game));
        }

        #endregion
    }
}
