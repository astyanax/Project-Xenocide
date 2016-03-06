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
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using CeGui;
using CeGui.Renderers.Xna;

using ProjectXenocide.Utils;
using Xenocide.Resources;

#endregion

namespace ProjectXenocide.UI.Screens
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
    public abstract class Frame : IDisposable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ceguiId">ID that CeGui will use to refer to this frame</param>
        /// <param name="size">Dimensions of the frame</param>
        protected Frame(string ceguiId, System.Drawing.SizeF size)
        {
            this.ceguiId = ceguiId;
            this.screenManager = Xenocide.ScreenManager;
            this.size = size;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ceguiId">ID that CeGui will use to refer to this frame</param>
        /// <param name="layoutFilename">The .layout filename used to describe this frame</param>
        protected Frame(string ceguiId, string layoutFilename)
        {
            this.ceguiId = ceguiId;
            this.screenManager = Xenocide.ScreenManager;
            this.layoutFilename = layoutFilename;
        }
        
        /// <summary>
        /// Puts the frame onto the display
        /// </summary>
        public virtual void Show()
        {
            if (layoutFilename == null)
            {
                rootWidget = ConstructRootWidget();
                rootWidget.Size = size;
                rootWidget.MinimumSize = size;
            }
            else
            {
                rootWidget = WindowManager.Instance.LoadWindowLayout(layoutFilename, this);
                rootWidget.MinimumSize = rootWidget.Size;
                size = rootWidget.Size;
            }

            //ToDo: Need to set window to have correct parent
            GuiSheet.AddChild(rootWidget);

            rootWidget.MetricsMode = CeGui.MetricsMode.Relative;
            rootWidget.Position = CalculatePosition();

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
                CeGui.WindowManager.Instance.DestroyWindow(rootWidget);
            }
            rootWidget = null;
        }

        /// <summary>
        /// Enable/Disable this frame (and it's child widgets)
        /// </summary>
        /// <param name="enableFrame">true to enable</param>
        public void Enable(bool enableFrame)
        {
            if (enableFrame)
            {
                rootWidget.Enable();
            }
            else
            {
                rootWidget.Disable();
            }
        }

        /// <summary>
        /// populate the fame with the Cegui Widgets
        /// </summary>
        protected abstract void CreateCeguiWidgets();

        /// <summary>
        /// Load the graphic content of objects in the frame
        /// </summary>
        /// <param name="content">content manager that fetches the content</param>
        /// <param name="device">the display</param>
        public virtual void LoadContent(ContentManager content, GraphicsDevice device)
        {
        }

        /// <summary>
        /// Unload the Scene's graphic content
        /// </summary>
        public virtual void UnloadContent()
        {
        }

        /// <summary>
        /// Retreive the Gui Sheet that "hosts" the Form
        /// </summary>
        /// <returns>The Sheet</returns>
        protected virtual CeGui.GuiSheet GuiSheet
        {
            get { return ScreenManager.RootGuiSheet; }
        }

        /// <summary>
        /// Construct the widget that holds all widgets on this window
        /// </summary>
        /// <returns>the widget</returns>
        protected abstract CeGui.Window ConstructRootWidget();

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
            rootWidget.AddChild(widget);

            // ToDo: we're using Position and Size because Top, Left, Width and Height
            // properties are currently broken in CeGui
            widget.MetricsMode = CeGui.MetricsMode.Relative;
            widget.Position = new System.Drawing.PointF(left, top);
            widget.Size = new System.Drawing.SizeF(width, height);
        }

        /// <summary>
        /// Create the specified slider and put it on this frame
        /// </summary>
        /// <param name="resourceName">Name of resource string for label to put on button</param>
        /// <param name="left">Position on frame for slider's left edge</param>
        /// <param name="top">Position on frame for slider's top edge</param>
        /// <param name="width">Width of slider (relative to frame)</param>
        /// <param name="height">Height of slider (relative to frame)</param>
        /// <returns>The slider that has been created</returns>
        public
        CeGui.Widgets.Slider
        AddSlider(
            string resourceName,
            float left, float top, float width, float height)
        {
            string label = XenocideResourceManager.Get(resourceName);
            Debug.Assert((!String.IsNullOrEmpty(label)));
            CeGui.Widgets.Slider slider = GuiBuilder.CreateSlider(ceguiId + '_' + label);
            slider.Text = label;
            AddWidget(slider, left, top, width, height);
            return slider;
        }
        
        /// <summary>
        /// Create the specified checkbox (and text) and put it on this frame
        /// </summary>
        /// <param name="resourceName">Name of resource string for label to put on checkbox</param>
        /// <param name="left">Position on frame for checkbox left edge</param>
        /// <param name="top">Position on frame for checkbox top edge</param>
        /// <param name="width">Width of checkbox (relative to frame)</param>
        /// <param name="height">Height of checkbox (relative to frame)</param>
        /// <returns>The checkbox that has been created</returns>
        public
        CeGui.Widgets.Checkbox
        AddCheckbox(
            string resourceName,
            float left, float top, float width, float height)
        {
            string label = XenocideResourceManager.Get(resourceName);
            Debug.Assert((!String.IsNullOrEmpty(label)));
            CeGui.Widgets.Checkbox checkbox = GuiBuilder.CreateCheckbox(ceguiId + '_' + label);
            checkbox.Text = label;
            AddWidget(checkbox, left, top, width, height);
            return checkbox;
        }

        /// <summary>
        /// Create the specified static text and put it on this frame
        /// </summary>
        /// <remarks>Assumes label will be the text</remarks>
        /// <param name="resourceName">Name of resource string for label to put on</param>
        /// <param name="left">Position on frame for text's left edge</param>
        /// <param name="top">Position on frame for text's top edge</param>
        /// <param name="width">Width of text (relative to frame)</param>
        /// <param name="height">Height of text (relative to frame)</param>
        /// <returns>The text that has been created</returns>
        public
        CeGui.Widgets.StaticText
        AddStaticText(
            string resourceName,
            float left, float top, float width, float height)
        {
            string label = XenocideResourceManager.Get(resourceName);
            Debug.Assert((!String.IsNullOrEmpty(label)));
            CeGui.Widgets.StaticText statictext = GuiBuilder.CreateText(ceguiId + '_' + label);
            statictext.Text = label;
            AddWidget(statictext, left, top, width, height);
            return statictext;
        }

        /// <summary>
        /// Create the specified static text and put it on this frame
        /// </summary>
        /// <remarks>We're going to add the text later</remarks>
        /// <param name="left">Position on frame for text's left edge</param>
        /// <param name="top">Position on frame for text's top edge</param>
        /// <param name="width">Width of text (relative to frame)</param>
        /// <param name="height">Height of text (relative to frame)</param>
        /// <returns>The text that has been created</returns>
        public
        CeGui.Widgets.StaticText
        AddStaticText(
            float left, float top, float width, float height)
        {
            CeGui.Widgets.StaticText statictext = GuiBuilder.CreateText(ceguiId + "_StaticText_" + NextId);
            AddWidget(statictext, left, top, width, height);
            return statictext;
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
            string label = XenocideResourceManager.Get(resourceName);
            Debug.Assert((!String.IsNullOrEmpty(label)));
            CeGui.Widgets.PushButton button = GuiBuilder.CreateButton(ceguiId + '_' + label);
            button.Text = label;
            button.Font = CeGui.FontManager.Instance.GetFont("Xeno");
            AddWidget(button, left, top, width, height);
            button.Clicked += new CeGui.GuiEventHandler(OnPlayButtonSound);
            button.MouseEnters += new MouseEventHandler(OnHoverPlaySound);
            return button;
        }

        /// <summary>
        /// Adds a sound to be played for a button.
        /// </summary>
        /// <param name="btnName">The name of the button</param>
        /// <param name="sound">The sound to play.</param>
        public void AddButtonSound(string btnName, string sound)
        {
            buttonSounds.Add(btnName, sound);
        }

        /// <summary>
        /// Create the specified button and put it on this frame
        /// </summary>
        /// <param name="resourceName">Name of resource string for label to put on button</param>
        /// <param name="left">Position on frame for button's left edge</param>
        /// <param name="top">Position on frame for button's top edge</param>
        /// <param name="width">Width of button (relative to frame)</param>
        /// <param name="height">Height of button (relative to frame)</param>
        /// <param name="sound">Sound to play when button pressed</param>
        /// <returns>The button that has been created</returns>
        public
        CeGui.Widgets.PushButton
        AddButton(
            string resourceName,
            float left, float top, float width, float height,
            string sound)
        {
            CeGui.Widgets.PushButton button = AddButton(resourceName, left, top, width, height);
            buttonSounds.Add(button.Name, sound);
            return button;
        }

        /// <summary>
        /// Create the specified Edit box and put it on this frame
        /// </summary>
        /// <param name="resourceName">Name of edit box</param>
        /// <param name="left">Position on frame for Edit box's left edge</param>
        /// <param name="top">Position on frame for Edit box's top edge</param>
        /// <param name="width">Width of Edit box (relative to frame)</param>
        /// <param name="height">Height of Edit box (relative to frame)</param>
        /// <returns>The Edit box that has been created</returns>
        public
        CeGui.Widgets.EditBox
        AddEditBox(
            string resourceName,
            float left, float top, float width, float height)
        {
            CeGui.Widgets.EditBox editBox = GuiBuilder.CreateEditBox(ceguiId + '_' + resourceName);
            AddWidget(editBox, left, top, width, height);
            return editBox;
        }

        /// <summary>
        /// Create the specified image with button events.
        /// </summary>
        public
        CeGui.Widgets.StaticImage
        AddStaticImageButton(
            string resourceName,
            float left, float top, float width, float height, string imagenormal, string imagepushed, string sound)
        {
            CeGui.Widgets.StaticImage staticImage = GuiBuilder.CreateImage(ceguiId + '_' + resourceName);
            staticImage.MouseClicked += new MouseEventHandler(OnPlayButtonSound);
            staticImage.MouseEnters  += new MouseEventHandler(ImageHoverOver);
            staticImage.MouseLeaves  += new MouseEventHandler(ImageHoverLeave);

            staticImage.SetImage(ImageSetName, imagenormal);

            //set items to dictionary
            buttonSounds.Add(staticImage.Name, sound);
            imageNames.Add(staticImage.Name, imagenormal);
            imageNamesP.Add(staticImage.Name, imagepushed);

            AddWidget(staticImage, left, top, width, height);
            return staticImage;
        }

        /// <summary>
        /// Create the specified MultiColumnList and put it on this frame
        /// </summary>
        /// <param name="left">Position on frame for MCL's left edge</param>
        /// <param name="top">Position on frame for MCL's top edge</param>
        /// <param name="width">Width of MCL (relative to frame)</param>
        /// <param name="height">Height of MCL (relative to frame)</param>
        /// <param name="args">details of columns to add, in form string name, float width</param>
        /// <returns>The MultiColumnList that has been created</returns>
        public
        CeGui.Widgets.MultiColumnList
        AddGrid(
            float left, float top, float width, float height, params Object[] args)
        {
            CeGui.Widgets.MultiColumnList grid = GuiBuilder.CreateGrid(ceguiId + "_multiColumnList_" + NextId);
            AddWidget(grid, left, top, width, height);
            for (int i = 0; i < args.Length; i += 2)
            {
                grid.AddColumn((string)args[i], grid.ColumnCount, (float)args[i + 1]);
            }
            return grid;
        }

        /// <summary>
        /// Play sound when mouse hovers over button
        /// </summary>
        /// <param name="sender">unused</param>
        /// <param name="e">unused</param>
        private void OnHoverPlaySound(object sender, MouseEventArgs e)
        {
            Xenocide.AudioSystem.PlaySound("Menu\\buttonover.ogg");
        }

        /// <summary>
        /// Change to hover image and play sound when hover over image "button"
        /// </summary>
        /// <param name="sender">button</param>
        /// <param name="e">unused</param>
        void ImageHoverOver(object sender, MouseEventArgs e)
        {
            CeGui.Widgets.StaticImage imageWidget = sender as CeGui.Widgets.StaticImage;
            string label = imageWidget.Name;
            imageWidget.SetImage(ImageSetName, imageNamesP[label]);

            //play sound
            Xenocide.AudioSystem.PlaySound("Menu\\buttonover.ogg");
        }

        /// <summary>
        /// Replace image with normal image when end hover
        /// </summary>
        /// <param name="sender">button</param>
        /// <param name="e">unused</param>
        private void ImageHoverLeave(object sender, MouseEventArgs e)
        {
            CeGui.Widgets.StaticImage imageWidget = sender as CeGui.Widgets.StaticImage;
            string label = imageWidget.Name;
            imageWidget.SetImage(ImageSetName, imageNames[label]);
        }

        /// <summary>
        /// Called to tell the Window prior to destruction to tell it to save any state information
        /// needed to reconstruct it's state.
        /// Basically, used to get the Geoscape to store the camera's position 
        /// when we go to a different screen.
        /// </summary>
        public virtual void SaveState()
        {
        }

        /// <summary>React to user clicking on a button by playing it's sound</summary>
        /// <param name="sender">Button the user clicked</param>
        /// <param name="e">Not used</param>
        [GuiEvent()]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        public void OnPlayButtonSound(object sender, CeGui.GuiEventArgs e)
        {
            if (enableButtonSounds)
            {
                // figure out sound to play, if no sound registered, use default
                string sound;
                string label = (sender as CeGui.Window).Name;
                if (!buttonSounds.TryGetValue(label, out sound))
                {
                    sound = DefaultButtonClickSound;
                }
                Xenocide.AudioSystem.PlaySound(sound);
            }
        }

        #region Fields

        /// <summary>
        /// Play sound when button pressed?
        /// </summary>
        public bool EnableButtonSounds { get { return enableButtonSounds; } set { enableButtonSounds = value; } }

        /// <summary>Make window visible/invisible</summary>
        public bool Visible { get { return rootWidget.Visible; } set { rootWidget.Visible = value; } }

        /// <summary>CeGui widget that holds all other widgets in the window</summary>
        protected CeGui.Window  RootWidget { get { return rootWidget; } }

        /// <summary>CeGui widget that holds all other widgets in the window</summary>
        private CeGui.Window rootWidget;

        /// <summary>The identifer (name) that CeGui will use for the root widget</summary>
        /// RK: There is no other ID you may require from a frame anyways, why not call it Id (and use it widely ;) ).
        private string ceguiId;

        /// <summary>
        /// The identifer (name) that CeGui will use for the root widget
        /// </summary>
        protected string CeguiId { get { return ceguiId; } }

        /// <summary>
        /// The ScreenManager
        /// </summary>
        private ScreenManager screenManager;

        /// <summary>
        /// The ScreenManager
        /// </summary>
        protected ScreenManager ScreenManager { get { return screenManager; } }

        /// <summary>
        /// The dimensions of the frame (in relative co-ordinates
        /// </summary>
        private System.Drawing.SizeF size;

        /// <summary>
        /// Helper function to get the GuiBuilder used to create widgets to put on form
        /// </summary>
        protected CeGui.GuiBuilder GuiBuilder { get { return ScreenManager.GuiBuilder; } }

        /// <summary>
        /// Sound to play when a button is pressed. Format is button ID, sound ID
        /// </summary>
        private Dictionary<String, String> buttonSounds = new Dictionary<string, string>();

        /// <summary>
        /// images when imagebutton is pushed
        /// </summary>
        private Dictionary<String, String> imageNamesP = new Dictionary<string, string>();

        /// <summary>
        /// images when imagebutton is not pushed
        /// </summary>
        private Dictionary<String, String> imageNames = new Dictionary<string, string>();

        /// <summary>
        /// Play sound when button pressed?
        /// </summary>
        private bool enableButtonSounds = true;

        /// <summary>
        /// Generate a unique (to this form) component to use in widget names
        /// </summary>
        private string NextId { get { return Util.ToString(++nextId); } }

        /// <summary>
        /// Generate a unique (to this form) component to use in widget names
        /// </summary>
        private int nextId;

        /// <summary>
        /// Filename of the .layout for this frame
        /// </summary>
        private string layoutFilename;

        /// <summary>
        /// Default button click sound to play
        /// </summary>
        public const String DefaultButtonClickSound = "Menu\\buttonclick1_ok.ogg";

        private const String ImageSetName = "TaharezLook";

        #endregion Fields
    }
}
