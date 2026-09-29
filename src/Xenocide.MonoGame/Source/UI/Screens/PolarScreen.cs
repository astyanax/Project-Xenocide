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
using Microsoft.Xna.Framework.Input;

using MonoGameGum;

using ProjectXenocide.UI.Scenes.Common;
using ProjectXenocide.Utils;

#endregion

namespace ProjectXenocide.UI.Screens
{
    /// <summary>
    /// Screen base class for 3D scenes with polar (spherical) camera control.
    /// Manages viewport rendering and mouse-based camera rotation/zoom.
    /// </summary>
    /// <remarks>
    /// ARCHITECTURE: Adds a PolarScene for 3D rendering and handles mouse input
    /// for camera manipulation. Subclasses set the Scene property and override
    /// OnLeftMouseDownInScene() for click interactions.
    /// 
    /// INPUT HANDLING: Right-drag rotates camera, scroll wheel zooms, left-click
    /// dispatches to OnLeftMouseDownInScene() with normalized coordinates.
    /// </remarks>
    public abstract class PolarScreen : GumScreen
    {
        protected PolarScreen(string ceguiId)
            : base(ceguiId)
        {
        }

        protected PolarScreen(string ceguiId, String backgroundFilename)
            : base(ceguiId, backgroundFilename)
        {
        }

        public void SetView(float left, float top, float width, float height)
        {
            _viewportRect = new UiRect(left, top, left + width, top + height);
        }

        public override void LoadContent(ContentManager content, GraphicsDevice device)
        {
            scene.LoadContent(content, device);
        }

        public override void Draw(GameTime gameTime, GraphicsDevice device)
        {
            base.Draw(gameTime, device);
            scene.Draw(gameTime, device, _viewportRect);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            EnsureTargetCamera();
            HandleMouseInput();
            ApplyCameraSmoothing(gameTime);
        }

        /// <summary>
        /// Rotate the camera (used by the on-screen camera arrow buttons).
        /// </summary>
        public void RotateBy(float longitude, float latitude)
        {
            EnsureTargetCamera();
            targetCamera.X += longitude;
            targetCamera.Y += latitude;
            WrapAndClampTarget();
        }

        /// <summary>
        /// Zoom the camera (used by the on-screen zoom buttons).
        /// </summary>
        public void ZoomBy(float notches)
        {
            EnsureTargetCamera();
            targetCamera.Z = ClampZoom(targetCamera.Z + (notches * ZoomStep * CameraSettings.ZoomSensitivity));
        }

        /// <summary>
        /// Snap the smoothing target to the scene's current camera.  Call after
        /// setting <see cref="PolarScene.CameraPosition"/> directly.
        /// </summary>
        public void SnapCameraTarget()
        {
            targetCamera = scene.CameraPosition;
            _cameraTargetInitialized = true;
        }

        private void HandleMouseInput()
        {
            var mouse = Mouse.GetState();
            var device = Xenocide.Instance.GraphicsDevice;

            int vpX = (int)(device.Viewport.Width * _viewportRect.Left);
            int vpY = (int)(device.Viewport.Height * _viewportRect.Top);
            int vpW = (int)(device.Viewport.Width * _viewportRect.Width);
            int vpH = (int)(device.Viewport.Height * _viewportRect.Height);

            bool inViewport = mouse.X >= vpX && mouse.X <= vpX + vpW
                           && mouse.Y >= vpY && mouse.Y <= vpY + vpH;

            // Right-drag rotates the globe. Always track the previous position so
            // dragging out of (and back into) the viewport doesn't jump.
            if (_prevRightDown && mouse.RightButton == ButtonState.Pressed)
            {
                float deltaX = mouse.X - _prevMouseX;
                float deltaY = mouse.Y - _prevMouseY;
                float rotateSpeed = (0.005f + 0.004f * scene.CameraHeight) * CameraSettings.RotateSensitivity;
                targetCamera.X += deltaX * rotateSpeed;
                targetCamera.Y += (CameraSettings.InvertY ? -deltaY : deltaY) * rotateSpeed;
                WrapAndClampTarget();
            }
            _prevMouseX = mouse.X;
            _prevMouseY = mouse.Y;

            if (mouse.LeftButton == ButtonState.Pressed && !_prevLeftDown)
            {
                // Don't turn a click on a Gum HUD control (button, list, ...) into a
                // scene action such as a move order or a waypoint.
                bool overGumControl = GumService.Default.Cursor?.FrameworkElementOver != null;
                if (inViewport && !overGumControl)
                {
                    float relX = (mouse.X - vpX) / (float)vpW;
                    float relY = (mouse.Y - vpY) / (float)vpH;
                    OnLeftMouseDownInScene(relX, relY);
                }
            }

            int wheelDelta = mouse.ScrollWheelValue - _prevScrollValue;
            if (inViewport && wheelDelta != 0)
            {
                ZoomAtCursor(wheelDelta / 120f, mouse, vpX, vpY, vpW, vpH);
            }
            _prevScrollValue = mouse.ScrollWheelValue;

            _prevLeftDown = mouse.LeftButton == ButtonState.Pressed;
            _prevRightDown = mouse.RightButton == ButtonState.Pressed;
        }

        /// <summary>
        /// Zoom by a number of notches, pulling the point under the cursor toward
        /// the centre (configurable) so zooming keeps the area of interest in view.
        /// </summary>
        private void ZoomAtCursor(float notches, MouseState mouse, int vpX, int vpY, int vpW, int vpH)
        {
            float oldZ = targetCamera.Z;
            float newZ = ClampZoom(oldZ + (notches * ZoomStep * CameraSettings.ZoomSensitivity));
            float zoomDelta = newZ - oldZ;

            if (CameraSettings.ZoomToCursor && (zoomDelta != 0f))
            {
                float offX = Math.Clamp((mouse.X - (vpX + (vpW * 0.5f))) / (vpW * 0.5f), -1f, 1f);
                float offY = Math.Clamp((mouse.Y - (vpY + (vpH * 0.5f))) / (vpH * 0.5f), -1f, 1f);
                targetCamera.X -= offX * zoomDelta * 0.35f;
                targetCamera.Y += (CameraSettings.InvertY ? -offY : offY) * zoomDelta * 0.35f;
                WrapAndClampTarget();
            }

            targetCamera.Z = newZ;
        }

        private void ApplyCameraSmoothing(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (dt <= 0f)
            {
                return;
            }

            float factor = 1f - (float)Math.Exp(-12f * dt);
            Vector3 current = scene.CameraPosition;
            Vector3 next = Vector3.Lerp(current, targetCamera, factor);
            next.Z = ClampZoom(next.Z);
            scene.CameraPosition = next;
        }

        private void EnsureTargetCamera()
        {
            if (!_cameraTargetInitialized)
            {
                targetCamera = scene.CameraPosition;
                _cameraTargetInitialized = true;
            }
        }

        private void WrapAndClampTarget()
        {
            const float latLimit = MathHelper.Pi * 0.5f * 85f / 90f;
            while (targetCamera.X > MathHelper.Pi)
            {
                targetCamera.X -= MathHelper.TwoPi;
            }
            while (targetCamera.X < -MathHelper.Pi)
            {
                targetCamera.X += MathHelper.TwoPi;
            }
            targetCamera.Y = MathHelper.Clamp(targetCamera.Y, -latLimit, latLimit);
        }

        private float ClampZoom(float z) => Math.Clamp(z, scene.MinZoom, scene.MaxZoom);

        protected virtual void OnLeftMouseDownInScene(float relX, float relY)
        {
        }

        #region fields

        protected PolarScene Scene { get { return scene; } set { scene = value; } }

        /// <summary>
        /// The viewport rectangle (normalized 0-1 coordinates) that the 3D scene
        /// renders into and mouse input is constrained to. Screens can set this
        /// property to override the default viewport (e.g., when using ScreenLayout
        /// with SplitViewport mode).
        /// </summary>
        public UiRect ViewportRect
        {
            get => _viewportRect;
            set => _viewportRect = value;
        }

        /// <summary>Zoom amount per wheel notch / button press, in world units.</summary>
        private const float ZoomStep = 0.15f;

        private PolarScene scene;
        private UiRect _viewportRect;
        private Vector3 targetCamera;
        private bool _cameraTargetInitialized;
        private bool _prevLeftDown;
        private bool _prevRightDown;
        private int _prevMouseX;
        private int _prevMouseY;
        private int _prevScrollValue;

        #endregion fields
    }
}
