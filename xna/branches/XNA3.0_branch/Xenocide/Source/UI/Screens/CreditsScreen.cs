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
* @file CreditsScreen.cs
* @date Created: 2008 / 03 / 09
* @author File creator: Tamás Terpai
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;

#endregion

namespace ProjectXenocide.UI.Screens
{
    /// <summary>A line of text to display</summary>
    class SpriteLine
    {
        public SpriteLine(string givenText, SpriteFont givenFont)
        {
            this.innerText = givenText;
            // center of text
            this.size = new Vector2(
                ((int)(givenFont.MeasureString(this.innerText).X) + 1) / 2, 
                givenFont.LineSpacing / 2);
        }

        public string InnerText
        {
            get { return innerText; }
        }

        public Vector2 Size
        {
            get { return size; }
        }

        private string innerText;
        private Vector2 size;
    }

    /// <summary>
    /// Creates and starts a scrolling credits screen
    /// </summary>
    public class CreditsScreen : ProjectXenocide.UI.Screens.Screen
    {
        /// <summary>
        /// CreditsScreen constructor
        /// </summary>
        public CreditsScreen()
            :base("CreditsScreen")
        {
        }

        /// <summary>
        /// Implement IDisposable
        /// </summary>
        /// <param name="disposing">false when called from a finalizer</param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (spriteBatch != null)
                    {
                        spriteBatch.Dispose();
                        spriteBatch = null;
                    }
                    if (contentManager != null)
                    {
                        contentManager.Dispose();
                        contentManager = null;
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// Load the Scene's graphic content
        /// </summary>
        /// <param name="content">unused</param>
        /// <param name="device">the display</param>
        
        public override void LoadContent(ContentManager content, GraphicsDevice device)
        {
            
            {
                spriteBatch = new SpriteBatch(Xenocide.Instance.GraphicsDevice);
                outputFont = contentManager.Load<SpriteFont>(@"Content\SpriteFont1");
                BuildDisplayStrings(device);
            }
        }

        /// <summary>add the buttons to the screen</summary>
        protected override void CreateCeguiWidgets()
        {
        }

        /// <summary>Build the strings to show to user</summary>
        /// <param name="device">the display</param>
        private void BuildDisplayStrings(GraphicsDevice device)
        {
            List<SpriteLine> preContent = new List<SpriteLine>();
            int width = GetDisplayAreaWidth(device);
            string[] givenLines = LoadCredits();
            foreach (string currentLine in givenLines)
            {
                StringBuilder buildingLine = new StringBuilder();
                StringBuilder currentWord = new StringBuilder();
                foreach (Char currentChar in currentLine.ToCharArray())
                {
                    currentWord.Append(currentChar);
                    buildingLine.Append(currentChar);
                    if (outputFont.MeasureString(buildingLine.ToString()).X > width)
                    {
                        buildingLine.Remove(buildingLine.Length - currentWord.Length, currentWord.Length);
                        preContent.Add(new SpriteLine(buildingLine.ToString(), outputFont));
                        buildingLine = new StringBuilder();
                        buildingLine.Append(currentWord);
                        currentWord = new StringBuilder();
                    }
                    if (currentChar==' ')
                    {
                        currentWord = new StringBuilder();
                    }
                }
                preContent.Add(new SpriteLine(buildingLine.ToString(), outputFont));
            }
            preContent.Add(new SpriteLine(String.Empty,outputFont));
            lines = preContent.ToArray();
            this.reset(device);
        }

        /// <summary>
        /// Overridden Screen.Draw method
        /// </summary>
        /// <param name="gameTime">Current time</param>
        /// <param name="device">Where to draw the screen</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification="Will throw if gameTime is null")]
        public override void Draw(GameTime gameTime, GraphicsDevice device)
        {
            offset -= gameTime.ElapsedRealTime.Milliseconds*scrollSpeed*outputFont.LineSpacing/1000;
            if (offset < - outputFont.LineSpacing)
            {
                index++;
                offset += outputFont.LineSpacing;
                if (index == lines.Length) this.reset(device);
            }

            spriteBatch.Begin();

            int i=index;
            Vector2 textPosition = new Vector2(GetDisplayAreaWidth(device) / 2, offset);
            while ((i < lines.Length) && (textPosition.Y < device.Viewport.Height))
            {
                textPosition.Y += lines[i].Size.Y;
                spriteBatch.DrawString(
                    outputFont,
                    lines[i].InnerText,
                    textPosition,
                    Color.Wheat,
                    0,
                    lines[i].Size,
                    1.0f,
                    SpriteEffects.None,
                    0.5f
                    );
                textPosition.Y += lines[i].Size.Y;
                i++;
            }

            spriteBatch.End();
        }

        /// <summary>Look for esc key pressed to end screen</summary>
        /// <param name="gameTime">unused</param>
        public override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                ScreenManager.ScheduleScreen(new StartScreen());
            }
        }

        private void reset(GraphicsDevice graphicsDevice)
        {
            offset = graphicsDevice.Viewport.Height;
            index = 0;
        }

        /// <summary>Get the credits text to show</summary>
        /// <returns>text to show</returns>
        private static string[] LoadCredits()
        {
            string creditsFileName = StorageContainer.TitleLocation + "/Content/DataFiles/credits.txt";
            return File.ReadAllLines(creditsFileName);
        }

        /// <summary>Width of area available for showing credits</summary>
        /// <param name="device">the display</param>
        /// <returns>width, in pixels</returns>
        private static int GetDisplayAreaWidth(GraphicsDevice device)
        {
            return (int)(device.Viewport.Width * 0.74f);
        }


        /// <summary>Lines of text per second</summary>
        private const float scrollSpeed=2.0f;

        /// <summary>Needed so that we can load the sprite font</summary>
        private ContentManager contentManager = new ContentManager(Xenocide.Instance.Services);

        /// <summary>Font used</summary>
        private SpriteFont outputFont;

        /// <summary>Used to draw the text</summary>
        private SpriteBatch spriteBatch;

        private SpriteLine [] lines;
        private float offset;
        private int index;
    }
}
