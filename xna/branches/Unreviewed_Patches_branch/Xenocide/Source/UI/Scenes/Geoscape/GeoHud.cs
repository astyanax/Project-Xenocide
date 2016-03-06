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
        string tipText = string.Empty;
    
        Texture2D iconTex;
        SpriteMap iconSprites;
        Texture2D textBackGround;
        Texture2D textBorder;

        private const float OCCLUDEDISTANCE = 1.2f;
        #endregion

        /// <summary>
        /// This holds a list of all icons so far detected in the current ron on the geoscape
        /// </summary>
        Dictionary<string, IconState> iconStates = new Dictionary<string, IconState>();

        /// <summary>
        /// A list of the different icon typs that can be drawn
        /// </summary>
        public enum hudIconTypes
        {
            XCorpBase = 0,
            XCorpCraft = 3,
            UfoFly = 4,
            UfoLand = 5,
            UfoCrash= 6,
            TerrorSite = 2,
            AlienBase = 1
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
            textBackGround = new Texture2D(device, 1, 1, 1, TextureUsage.None, SurfaceFormat.Color);
            textBorder = new Texture2D(device, 1, 1, 1, TextureUsage.None, SurfaceFormat.Color);
            
            // create a semi-transparent background texture for the textbackgorund.
            Color[] rgba = new Color[1];
            rgba[0] = new Color(Color.DarkCyan , 0.5f);
            textBackGround.SetData<Color>(rgba);

            // create a slighly-transparent border texture for the textbackgorund.
            rgba = new Color[1];
            rgba[0] = new Color(Color.Cyan, 0.9f);
            textBorder.SetData<Color>(rgba);
        }

        /// <summary>
        /// Starts the icon rendering process
        /// </summary>
        public void Begin()
        {
            mouseState = Mouse.GetState();
            TempSelectedItem = string.Empty;
            tipText = string.Empty;
            hudSprites.Begin();
        }

        /// <summary>
        /// finishes the icon rendering process
        /// </summary>
        /// <returns></returns>
        public string End()
        {
            selectedItem = TempSelectedItem;
            hudSprites.End();
            return tipText;
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
       
            if (iconStates.ContainsKey(iconName))
            {
                returnVal = iconStates[iconName];
                
            }
            else
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
        /// <param name="position"></param>
        /// <param name="basicEffect"></param>
        /// <param name="gameTime"></param>
        /// <param name="distanceToCamera"></param>
        /// <param name="objectName"></param>
        /// <param name="iconType"></param>
        public void DrawIcon(GraphicsDevice device,
                                Vector3 position, 
                                BasicEffect basicEffect, 
                                GameTime gameTime, 
                                float distanceToCamera,
                                string objectName,
                                hudIconTypes iconType)
        {
            Vector2 screenPos = Get2DProjection(position, device, basicEffect);
            IconState myIcon = GetIconState(objectName);
            bool blnHovering = Hovering(screenPos);
            bool blnOcclude = distanceToCamera >= OCCLUDEDISTANCE;
            
            if (!blnOcclude)
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
        private void DrawHudIcon(hudIconTypes iconType, Vector2 position, IconState icon, string text, GraphicsDevice device)
        {
            SpriteItem inner = iconSprites[(int)iconType];
            SpriteItem outer = iconSprites[7];
            inner.Draw(hudSprites, position, new Color(1.0f, 1.0f, 1.0f, 1.0f), 0.0f, new Vector2((inner.Width / 2), (inner.Height / 2)), 1.0F, SpriteEffects.None, 0.0F);
            outer.Draw(hudSprites, position, new Color(1.0f, 1.0f, 1.0f, 1.0f), icon.Rotation, new Vector2((outer.Width / 2), (outer.Height / 2)), icon.Scale, SpriteEffects.None, 0.0F);

            // Get text to display on the icon
            string iconText = icon.GetTextToDisplay(text);

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
                hudSprites.Draw(textBackGround, background, new Color(Color.White, 0.5f));
                iconSprites[(int)icon.FlashFrameToDraw].Draw(hudSprites, background, new Color(Color.White, 0.5f));
                //hudSprites.Draw(textBorder, leftBorder, Color.White);

                // Draws a 'border' around the text.  Alternative is sprite text font.
             //   absTextPosition.X -= 1;
              //  hudSprites.DrawString(hudFont, iconText, absTextPosition, Color.Black);
              //  absTextPosition.X += 2;
              //  hudSprites.DrawString(hudFont, iconText, absTextPosition, Color.Black);
              //  absTextPosition.X -= 1;
               // absTextPosition.Y -= 1;
               // hudSprites.DrawString(hudFont, iconText, absTextPosition, Color.Black);
               // absTextPosition.Y += 2;
               // hudSprites.DrawString(hudFont, iconText, absTextPosition, Color.Black);
               // absTextPosition.Y -= 1;
                // And now the text
                hudSprites.DrawString(hudFont, iconText, absTextPosition, Color.White);
            } 
        }

        /// <summary>
        ///  Create a blip used to reprensent the object when it moves out of view on the geoscape
        /// </summary>
        /// <param name="position"></param>
        /// <param name="color"></param>
        private void CreateBlip(hudIconTypes iconType,Vector2 position)
        {
            Color oCol = Color.White;
            switch (iconType)
            {
                case hudIconTypes.AlienBase:
                    oCol =     new Color(Color.Pink,0.7f);
                    break;
                case hudIconTypes.XCorpBase:
                    oCol = new Color(Color.Blue, 0.7f);
                    break;
                case hudIconTypes.XCorpCraft:
                    oCol = new Color(Color.Green, 0.7f);
                    break;
                case hudIconTypes.TerrorSite:
                    oCol = new Color(Color.Orange, 0.7f);
                    break;
                case hudIconTypes.UfoCrash:
                    oCol = new Color(Color.White, 0.7f);
                    break;
                case hudIconTypes.UfoFly:
                    oCol = new Color(Color.Red, 0.7f);
                    break;
                case hudIconTypes.UfoLand:
                    oCol = new Color(Color.Yellow, 0.7f);
                    break;
            }
            SpriteItem inner = iconSprites[(int)hudIconTypes.XCorpCraft ];
            inner.Draw(hudSprites, position, oCol, 0.0f, new Vector2((inner.Width / 2), (inner.Height / 2)), 1.0F, SpriteEffects.None, 0.0F);
        }

        /// <summary>
        /// Determines if the mouse point is psoitioned over a given position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private bool Hovering(Vector2 position)
        {
            return mouseState.X >= position.X - 16 &&
            mouseState.X <= position.X + 16 &&
            mouseState.Y >= position.Y - 16 &&
            mouseState.Y <= position.Y + 16 &&
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
        #endregion

        #region IconStateClass
        /// <summary>
        /// IconState class, hold persistent information for each geoobject that relates to the current
        /// state of it's on screen icon.
        /// </summary>
        public class IconState
        {
            
            private const float TEXTDRAWSPEED = 10.0f;
            private const float MINICONSCALE = 0.0f;
            private const float MAXICONSCALE = 1.0f;
            private const float SCALEUPRATE = 0.10f;
            private const float SCALEDOWNRATE = 0.05f;
            private const float OUTERROTATIONSPEED = 0.025f;
            private const float FULLCIRCLE = 360.0f;
            private const int FLASHANIMSTARTSEED = 1000;
            private const int FLASHANIMSTARTTHRESHOLD = 975;
            private const int FLASHSTARTFRAME = 8000;
            private const int FLASHENDFRAME = 18000;
            private const int FLASHSPEED = 800;
            private const int FLASHFRAMEINTERVAL = 1000;
            private const int FLASHCHECKINTERVAL = 500;


            public string GetTextToDisplay(string name)
            {
                string textToDraw = string.Empty;

                // only do this if we have some text to draw.
                if( ticks > 0)
                {
                    // Get the text we will be drawing
                    if (ticks > name.Length)
                        textToDraw = name.Substring(0, name.Length).ToUpper();
                    else
                        textToDraw = name.Substring(0, ticks).ToUpper();
                }

                if (oldText == textToDraw)
                    changed = false;
                else
                    changed = true;
         
                oldText = textToDraw;
                return textToDraw;
            }

            // called to update the state of the current icon based on screen input
            public void Next(bool blnRover, GameTime time)
            {
              
                // if hovered and not at full scale, increment scale
                if (blnRover &&
                    Scale < MAXICONSCALE)
                    scale += SCALEUPRATE;
                else

                // if not hovered and not at minimum scale, decrement scale
                if (!blnRover &&
                    Scale > MINICONSCALE)
                    scale -= SCALEDOWNRATE;
                
                // If Hovered
                if (blnRover)
                {
                    //update rotation
                    rotation -= OUTERROTATIONSPEED;
                    if (rotation <= -FULLCIRCLE)
                        rotation += FULLCIRCLE;

                    // if we werent hovered in the previous frame, stame time since hovered.
                    if (!hovered)
                    {
                        timeCreated = time.TotalRealTime;
                        hovered = true;
                    }
                    // set number of 'tick' (arbitarty time interval) since hovered.  This will become
                    // how many characters of the name string to display
                    ticks = (int)((time.TotalRealTime.TotalMilliseconds - timeCreated.TotalMilliseconds) / TEXTDRAWSPEED);

                    // update the status of the flash animation
                    UpdateFlash(time);
                }
                else
                {
                    // hovered off, reset values
                    hovered = false;
                    ticks = 0;
                }
            }

            /// <summary>
            /// Sets the status of the text animation wipe.
            /// </summary>
            private void UpdateFlash(GameTime time)
            {

                if (!inFlash)
                {
                    Random rnd = new Random(DateTime.Now.Millisecond);
                    int decide = rnd.Next(FLASHANIMSTARTSEED);

                    if (decide > FLASHANIMSTARTTHRESHOLD)
                        inFlash = true;
                }
                else
                {
                    flashFrameToDraw += FLASHSPEED;

                    if (flashFrameToDraw >= FLASHENDFRAME)
                    {
                        flashFrameToDraw = FLASHSTARTFRAME;
                        inFlash = false;
                    }
                }
            }
            #region Fields
            /// <summary>
            /// Icon current rotation
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
            private bool hovered;

            /// <summary>
            /// USed to remember previously rendered text
            /// </summary>
            private string oldText;

            /// <summary>
            /// Has the text changed in this pass?
            /// </summary>
            private bool changed;
            public bool Changed
            {
                get { return changed; }
            }


            private int flashFrameToDraw = FLASHSTARTFRAME;
            /// <summary>
            /// Current frame index of the text area animation effect
            /// </summary>
            public int FlashFrameToDraw
            {
                get { return Convert.ToInt32(flashFrameToDraw / FLASHFRAMEINTERVAL); }
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

