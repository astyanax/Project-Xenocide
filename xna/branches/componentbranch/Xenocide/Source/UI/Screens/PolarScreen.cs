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
* @file PolarScreen.cs
* @date Created: 2007/04/01
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Xenocide.UI.Scenes.Common;

#endregion

namespace Xenocide.UI.Screens
{
    /// <summary>
    /// Base class for a screen that shows a 3D model with a camera that moves 
    /// in polar co-ordinates.  I.e. scene has single 3D model, with camera
    /// that "revolves" around the model.  e.g. Geoscape, XNet, Human Base
    /// </summary>
    public abstract class PolarScreen : Screen
    {
        /// <summary>
        /// Constructor (obviously)
        /// </summary>
        /// <param name="ceguiId">CeGui's identifer for this screen</param>
        /// <param name="screenManager">The screen manager</param>
        protected PolarScreen(Game game, string ceguiId)
            : base(game, ceguiId)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            if (scene != null)
            {
                Game.Components.Add(scene);
                scene.SceneWindow = SceneWindow.Rect;
            }
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Game.Components.Remove(scene);
                scene.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Set up window indicating where the 3D scene is
        /// </summary>
        /// <param name="left">Position on screen for view's left edge</param>
        /// <param name="top">Position on screen for view's top edge</param>
        /// <param name="width">Width of view (relative to screen)</param>
        /// <param name="height">Height of view (relative to screen)</param>
        public void SetView(float left, float top, float width, float height)
        {
            sceneWindow = GuiBuilder.CreateImage(CeguiId + "_viewport");
            AddWidget(sceneWindow, left, top, width, height);

            // mouse activity on the "Scene" window
            sceneWindow.MouseMove += new CeGui.MouseEventHandler(OnMouseMoveInScene);
            sceneWindow.MouseButtonsDown += new CeGui.MouseEventHandler(OnMouseDownInScene);
            sceneWindow.MouseWheel += new CeGui.MouseEventHandler(OnMouseWheelInScene);
        }

        #region event handlers
        /// <summary>React to user moving the mouse wheel in 3D scene</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Mouse information</param>
        private void OnMouseWheelInScene(object sender, CeGui.MouseEventArgs e)
        {
            float zoomSpeed = 0.005f;
            scene.ZoomCamera(zoomSpeed * e.WheelDelta);
        }

        /// <summary>React to user moving the mouse in the 3D scene</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Mouse information</param>
        private void OnMouseMoveInScene(object sender, CeGui.MouseEventArgs e)
        {
            // if we're in "rotate the scene mode", rotate it
            if (mouseRotatesScene)
            {
                float rotateSpeed = 0.005f + 0.02f * scene.CameraHeight;
                scene.RotateCamera(e.MoveDelta.X * rotateSpeed, e.MoveDelta.Y * rotateSpeed);
            }
        }

        /// <summary>React to user clicking mouse in the 3D scene</summary>
        /// <param name="sender">CeGui widget sending the event</param>
        /// <param name="e">Mouse information</param>
        private void OnMouseDownInScene(object sender, CeGui.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                // right clicking the mouse toggles "rotate the scene" mode
                mouseRotatesScene = !mouseRotatesScene;
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                // let derived class handle it
                OnLeftMouseDownInScene(e);
            }
        }

        /// <summary>React to user clicking left mouse button in the 3D scene</summary>
        /// <param name="e">Mouse information</param>
        protected virtual void OnLeftMouseDownInScene(CeGui.MouseEventArgs e)
        {
            // default behaviour is to do nothing
        }

        #endregion event handlers

        #region fields

        /// <summary>
        /// CeGui widget that shows the 3D scene
        /// <remarks>Actually, at moment, indicates where to draw the 3D scene</remarks>
        /// </summary>
        protected CeGui.Widgets.StaticImage SceneWindow { get { return sceneWindow; } set { sceneWindow = value; } }

        /// <summary>
        /// The 3D view shown on the screen
        /// </summary>
        protected PolarScene Scene { get { return scene; } set { scene = value; } }

        /// <summary>
        /// CeGui widget that shows the 3D scene
        /// <remarks>Actually, at moment, indicates where to draw the 3D scene</remarks>
        /// </summary>
        private CeGui.Widgets.StaticImage sceneWindow;

        /// <summary>
        /// The 3D view shown on the screen
        /// </summary>
        private PolarScene scene;

        /// <summary>
        /// Do movements of mouse rotate the scene?
        /// </summary>
        private bool mouseRotatesScene;

        #endregion fields
    }
}
