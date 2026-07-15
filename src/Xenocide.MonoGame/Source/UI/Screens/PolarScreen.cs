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

using ProjectXenocide.UI.Scenes.Common;
using ProjectXenocide.Utils;

#endregion

namespace ProjectXenocide.UI.Screens
{
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
            HandleMouseInput();
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

            if (inViewport)
            {
                if (_prevRightDown && mouse.RightButton == ButtonState.Pressed)
                {
                    float deltaX = mouse.X - _prevMouseX;
                    float deltaY = mouse.Y - _prevMouseY;
                    float rotateSpeed = 0.005f + 0.004f * scene.CameraHeight;
                    scene.RotateCamera(deltaX * rotateSpeed, deltaY * -rotateSpeed);
                }
                _prevMouseX = mouse.X;
                _prevMouseY = mouse.Y;
            }

            if (mouse.LeftButton == ButtonState.Pressed && !_prevLeftDown)
            {
                if (inViewport)
                {
                    float relX = (mouse.X - vpX) / (float)vpW;
                    float relY = (mouse.Y - vpY) / (float)vpH;
                    OnLeftMouseDownInScene(relX, relY);
                }
            }

            int wheelDelta = mouse.ScrollWheelValue - _prevScrollValue;
            if (inViewport && wheelDelta != 0)
            {
                float zoomSpeed = 0.005f;
                scene.ZoomCamera(zoomSpeed * wheelDelta);
            }
            _prevScrollValue = mouse.ScrollWheelValue;

            _prevLeftDown = mouse.LeftButton == ButtonState.Pressed;
            _prevRightDown = mouse.RightButton == ButtonState.Pressed;
        }

        protected virtual void OnLeftMouseDownInScene(float relX, float relY)
        {
        }

        #region fields

        protected PolarScene Scene { get { return scene; } set { scene = value; } }

        private PolarScene scene;
        private UiRect _viewportRect;
        private bool _prevLeftDown;
        private bool _prevRightDown;
        private int _prevMouseX;
        private int _prevMouseY;
        private int _prevScrollValue;

        #endregion fields
    }
}
