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
* @file BaseInfoScreen.cs
* @date Created: 2009/10/20
* @author File creator: John Perrin
* @author Credits: none
*/
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

using ProjectXenocide.Utils;
using ProjectXenocide.Model;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.Model.Geoscape.AI;
using ProjectXenocide.UI.Scenes.Common;
#endregion

namespace ProjectXenocide.UI.Scenes.Geoscape
{
    /// <summary>
    /// Used to draw an icon representation of a point of interest on the geoscape
    /// </summary>
    class GeoHud
    {
        #region privates
        SpriteBatch hudSprites;
        MouseState mouseState;
        SpriteFont hudFont;
        String selectedItem = "";
        string TempSelectedItem;
        Texture2D iconTex;
        SpriteMap iconSprites;
        private const float OCCLUDEDISTANCE = 1.2f;
        #endregion

        /// <summary>
        /// This holds a list of all icons so far detected in the current ron on the geoscape
        /// </summary>
        Dictionary<string, IconState> iconStates = new Dictionary<string, IconState>();

        /// <summary>
        /// A list of the different icon typs that can be drawn
        /// </summary>
        public enum HudIconTypes
        {
            XCorpBase = 0,
            AlienBase = 1,
            TerrorSite = 2,
            XCorpCraft = 3,
            UfoFly = 4,
            UfoLand = 5,
            UfoCrash = 6,
            OccludedIcon = 19
        }

        /// <summary>
        /// Load the graphic content of the scene
        /// </summary>
        /// <param name="content">content manager that fetches the content</param>
        /// <param name="device">the display</param>
        public void LoadContent(ContentManager content, GraphicsDevice device)
        {
            hudSprites = new SpriteBatch(device);
            iconTex = content.Load<Texture2D>(@"Content\Textures\Geoscape\IconSpriteMap");
            hudFont = content.Load<SpriteFont>(@"Content\SpriteFont1");
            iconSprites = new SpriteMap(iconTex,4,5);
        }

        /// <summary>
        /// Starts the icon rendering process
        /// </summary>
        public void Begin()
        {
            mouseState = Mouse.GetState();
            TempSelectedItem = string.Empty;
            hudSprites.Begin();
        }

        /// <summary>
        /// finishes the icon rendering process
        /// </summary>
        /// <returns></returns>
        public void End()
        {
            selectedItem = TempSelectedItem;
            hudSprites.End();
        }

        #region Icon Methods

        /// <summary>
        /// Checks to see if icon state for a particular geo object is already being held.  If not
        /// create a new iconstate for it.
        /// </summary>
        /// <param name="iconName"></param>
        /// <returns></returns>
        private IconState GetIconState(string iconName)
        {
            IconState returnVal;
            if (!iconStates.TryGetValue(iconName, out returnVal))
            {
                returnVal = new IconState();
                iconStates.Add(iconName, returnVal);
            }
            return returnVal;
        }

        /// <summary>
        ///  This is the method called by geoscape scene to draw an icon
        /// </summary>
        /// <param name="device"></param>
        /// <param name="objectGeoPosition"></param>
        /// <param name="basicEffect"></param>
        /// <param name="gameTime"></param>
        /// <param name="cameraGeoPosition"></param>
        /// <param name="objectName"></param>
        /// <param name="iconType"></param>
        public void DrawIcon(GraphicsDevice device,
                                IGeoPosition iconGeoPosition, 
                                BasicEffect basicEffect, 
                                GameTime gameTime, 
                                GeoPosition cameraGeoPosition,
                                string objectName,
                                HudIconTypes iconType)
        {
            Vector3 position = iconGeoPosition.Position.Polar;
            Vector2 screenPos = Get2DProjection(position, device, basicEffect);
            IconState myIcon = GetIconState(objectName);
            bool blnHovering = IsHovering(screenPos);

            // if icon is sufficiently far from camera, assume on other side of world
            if (iconGeoPosition.Position.Distance(cameraGeoPosition) <= OCCLUDEDISTANCE)
            {
                if (blnHovering)
                {
                    PlayMouseOver(objectName);
                    TempSelectedItem = objectName;
                }

                myIcon.Next(blnHovering, gameTime);

                DrawHudIcon(iconType, screenPos, myIcon, objectName,device);
            }
            else
            {
                CreateBlip(iconType,screenPos);
            }
        }
        #endregion

        #region private methods
        /// </summary>
        /// <param name="geoposition">position on globe (in polar radians)</param>
        /// <returns>World matrix used by Draw</returns>
        private static Vector2 Get2DProjection(Vector3 geoObject, GraphicsDevice device, BasicEffect effect)
        {
            Matrix World;

            geoObject.Z = 1.0f;

            World = Matrix.CreateRotationX(-geoObject.Y)
                * Matrix.CreateRotationY(geoObject.X)
                * Matrix.CreateTranslation(GeoPosition.PolarToCartesian(geoObject));

            Vector3 projectedPosition = device.Viewport.Project(Vector3.Zero, effect.Projection, effect.View, World);
            return new Vector2(projectedPosition.X, projectedPosition.Y);
        }

        /// <summary>
        /// Internal call to draw an icon.
        /// </summary>
        /// <param name="iconType"></param>
        /// <param name="position"></param>
        /// <param name="icon"></param>
        /// <param name="text"></param>
        private void DrawHudIcon(HudIconTypes iconType, Vector2 position, IconState iconState, string text, GraphicsDevice device)
        {
            if (0 < iconState.Scale)
            {
                SpriteItem outer = iconSprites[7];
                outer.Draw(hudSprites, position, new Color(1.0f, 1.0f, 1.0f, 1.0f), iconState.Rotation, new Vector2((outer.Width / 2), (outer.Height / 2)), iconState.Scale, SpriteEffects.None, 0.0F);
            }
            SpriteItem inner = iconSprites[(int)iconType];
            inner.Draw(hudSprites, position, new Color(1.0f, 1.0f, 1.0f, 1.0f), 0.0f, new Vector2((inner.Width / 2), (inner.Height / 2)), 1.0F, SpriteEffects.None, 0.0F);

            // Get text to display on the icon
            string iconText = iconState.GetTextToDisplay(text);

            if (iconText != string.Empty)
            {
                // Get the size of the text
                Vector2 textSize = hudFont.MeasureString(iconText);

                // Text position set to right hand side of icon, cast to int to prevent 'fuzzy' text
                Vector2 absTextPosition = position;
                absTextPosition.X = (int)(absTextPosition.X += 30);
                absTextPosition.Y = (int)(absTextPosition.Y - (textSize.Y / 2));

                // Dont let the text exceed right hand side of screen.
                if ((hudFont.MeasureString(text).X + position.X + 50) >= device.Viewport.Width)
                {
                    absTextPosition.X = device.Viewport.Width - hudFont.MeasureString(text).X - 30;
                    absTextPosition.Y += 30;
                }

                // Set the recectangles that border,and leftborder the text.
                Rectangle background = new Rectangle(Convert.ToInt32(absTextPosition.X) - 5, Convert.ToInt32(absTextPosition.Y), Convert.ToInt32(textSize.X) + 10, Convert.ToInt32(textSize.Y));

                // Draw these rectangles
                iconSprites[(int)iconState.FlashFrameToDraw].Draw(hudSprites, background, new Color(Color.White, 0.8f));
                hudSprites.DrawString(hudFont, iconText, absTextPosition, Color.White);
            }
        }

        /// <summary>
        ///  Create a blip used to reprensent the object when it moves out of view on the geoscape
        /// </summary>
        /// <param name="position"></param>
        /// <param name="color"></param>
        private void CreateBlip(HudIconTypes iconType,Vector2 position)
        {
            Color oCol = Color.White;
            if (iconType <= HudIconTypes.UfoCrash)
            {
                oCol = obscuredIconColors[(int)iconType];
            }
            SpriteItem inner = iconSprites[(int)HudIconTypes.OccludedIcon];
            inner.Draw(hudSprites, position, oCol, 0.0f, new Vector2((inner.Width / 2), (inner.Height / 2)), 1.0F, SpriteEffects.None, 0.0F);
        }

        /// <summary>
        /// Determines if the mouse point is psoitioned over a given position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private bool IsHovering(Vector2 position)
        {
            float deltaX = mouseState.X - position.X;
            float deltaY = mouseState.Y - position.Y;
            return (deltaX * deltaX + deltaY * deltaY < 256.0) && 
            TempSelectedItem == String.Empty;
        }

        /// <summary>
        /// If selected object wasnt selected in the last pass, play the mouse over soundeffect.
        /// </summary>
        /// <param name="objectName"></param>
        private void PlayMouseOver(string objectName)
        {
            if (selectedItem != objectName)
            {
                Xenocide.AudioSystem.PlaySound("Menu\\buttonover.ogg");
            }
        }

        /// <summary>
        /// Sound to play if the rendered icon text has changed.  Typewrite effect?
        /// </summary>
        /// <param name="objectName"></param>
        private void PlayTextChanged()
        {
           Xenocide.AudioSystem.PlaySound("Menu\\buttonover.ogg");
        }

        /// <summary>
        /// Colours to use for icons that are on far side of Earth
        /// </summary>
        private Color[] obscuredIconColors = new Color[]
        {
            new Color(Color.Blue,0.7f),
            new Color(Color.Pink, 0.7f),
            new Color(Color.Orange, 0.7f),
            new Color(Color.Yellow, 0.7f),
            new Color(Color.Red, 0.7f),
            new Color(Color.Green , 0.7f),
            new Color(Color.White, 0.7f)
        };

        #endregion

        #region IconStateClass
        /// <summary>
        /// IconState class, hold persistent information for each geoobject that relates to the current
        /// state of it's on screen icon.
        /// </summary>
        public class IconState
        {
            private const float fluxCapacitor = 15.0f;

            /// <summary>
            /// Time in seconds between hover over and full focus icon size
            /// </summary>
            private const float scaleUpRate = 200.0f;

            // maximum size of icon
            private const float maxIconScale = 1.0f;

            //outer ring rotation speed
            private const float outerRotationSpeed = 0.025f;
            // max rotation value
            private const float fullCircle = 360.0f;

            // text area flash animation control values
            private const int flashAnimStartChance = 30;
            private const int flashStartFrame = 8;
            private const int flashEndFrame = 18;
            private const float flashSpeed = 300.0f;
            private const float flashCheckInterval = 200.0f;

            /// <summary>
            /// returns time since icon was hovered over
            /// </summary>
            /// <param name="gameTime"></param>
            /// <returns></returns>
            private double msSinceMouseOver(GameTime gameTime)
            {
                return gameTime.TotalRealTime.TotalMilliseconds - timeCreated.TotalMilliseconds; 
            }

            public string GetTextToDisplay(string name)
            {
                string textToDraw = string.Empty;

                // only do this if we have some text to draw.
                if( ticks > 0)
                {
                    // Get the text we will be drawing
                    textToDraw = name.Substring(0, Math.Min(name.Length, ticks)).ToUpper();
                }

                oldText = textToDraw;
                return textToDraw;
            }

            private void UpdateHoverStatus(GameTime time, bool blnHovered)
            {
                if (blnHovered && !previouslyHovered)
                { // newly entered
                    timeCreated = time.TotalRealTime;
                    previouslyHovered = true;
                }
                else if (!blnHovered && previouslyHovered)
                { // newly left
                    // no longer previously hovered
                    previouslyHovered = false ;
                    // no ticks since hover
                    ticks = 0;
                }
            }

            // called to update the state of the current icon based on screen input
            public void Next(bool blnMouseOver, GameTime time)
            {
                UpdateHoverStatus(time, blnMouseOver);
                UpdateOuterRing(time, blnMouseOver);
                UpdateFlash(time, blnMouseOver);

                if (blnMouseOver)
                    UpdateTextDisplay(time);
            }

            /// <summary>
            /// How many characters to draw of object name
            /// </summary>
            /// <param name="time"></param>
            private void UpdateTextDisplay(GameTime time)
            {
                // set number of 'tick' (arbitarty time interval) since hovered.  This will become
                // how many characters of the name string to display
                ticks = (int)(msSinceMouseOver(time) / fluxCapacitor);
            }


            /// <summary>
            /// updates the size and roation of the icons outer ring
            /// </summary>
            /// <param name="time"></param>
            /// <param name="blnHovered"></param>
            private void UpdateOuterRing(GameTime time, bool blnHovered)
            {
                if (blnHovered)
                {
                    outerRingSize += (float)time.ElapsedRealTime.TotalMilliseconds;
                }
                else
                {
                    outerRingSize -= (float)time.ElapsedRealTime.TotalMilliseconds;
                }

                outerRingSize = MathHelper.Clamp(outerRingSize, 0, scaleUpRate);

                scale = (outerRingSize / scaleUpRate) * maxIconScale;

                rotation -= outerRotationSpeed;
                rotation = MathHelper.Clamp(rotation, -fullCircle, 0.0f);
            }

            /// <summary>
            /// Sets the status of the text animation wipe.
            /// </summary>
            private void UpdateFlash(GameTime time, bool blnHovered)
            {
                if (!blnHovered)
                    return;

                flashCheckTime += (float)time.ElapsedRealTime.TotalMilliseconds;

                if (flashCheckTime > flashCheckInterval)
                {
                    flashCheckTime = 0;
                    if(Xenocide.Rng.RollDice(flashAnimStartChance))
                    {
                        inFlash = true;
                        flashAnimTime = 0;
                    }
                }

                if(inFlash)
                {
                    flashAnimTime += (float)time.ElapsedRealTime.TotalMilliseconds;
                    flashFrameToDraw = (int)((flashAnimTime/flashSpeed)*10) + 8;

                    if (flashFrameToDraw >= flashEndFrame )
                    {
                        flashFrameToDraw = flashStartFrame;
                        inFlash = false;
                    }
                }
            }
            #region Fields
            /// <summary>
            /// Outer ring current rotation
            /// </summary>
            float rotation = 0.0f;
            public float Rotation
            {
                get { return rotation; }
            }

            float scale = 0.0f;
            /// <summary>
            /// Icon current scale
            /// </summary>
            public float Scale
            {
                get { return scale; }
            }

            /// <summary>
            /// internal value that holds time since hover over occured.
            /// </summary>
            private TimeSpan timeCreated;

            /// <summary>
            /// Size of the icon outer ring
            /// </summary>
            private float outerRingSize;

            /// <summary>
            /// Time since last flash animation check
            /// </summary>
            private float flashCheckTime = 0;

            /// <summary>
            /// Time since being of last animation check
            /// </summary>
            private float flashAnimTime = 0;

            private int ticks = 0;
            /// <summary>
            /// Number of ticks since icon started
            /// </summary>
            public int Ticks
            {
                get { return Convert.ToInt32(ticks); }
            }

            /// <summary>
            /// Use to remember the hover status
            /// </summary>
            private bool previouslyHovered;

            /// <summary>
            /// USed to remember previously rendered text
            /// </summary>
            private string oldText;

            private int flashFrameToDraw = 8;
            /// <summary>
            /// Current frame index of the text area animation effect
            /// </summary>
            public int FlashFrameToDraw
            {
                get { return flashFrameToDraw; }
            }

            /// <summary>
            /// Are we in the midst of a run of the text area animation effect
            /// </summary>
            private bool inFlash = false;


            #endregion
        } 
        #endregion
    }
}
