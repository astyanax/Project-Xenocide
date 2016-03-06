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
* @file Dialog.cs
* @date Created: 2007/03/04
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

using CeGui.Renderers.Xna;
using ProjectXenocide.UI.Screens;
using ProjectXenocide.Utils;

#endregion

namespace ProjectXenocide.UI.Dialogs
{
    /// <summary>
    /// This is the base class that all dialogs derive from
    /// A "dialog" is a window that overlays a screen
    /// The dialog is responsible for:
    /// 1. Creating the CeGeui widgets on the dialog
    /// 2. Handing events from the widgets on the dialog
    /// </summary>
    public abstract class Dialog : Frame
    {
        /// <summary>
        /// Constructor (obviously)
        /// </summary>
        /// <param name="size">Size of dialog (relative to screen)</param>
        protected Dialog(System.Drawing.SizeF size)
            :
            base(MakeDialogId(), size)
        {
        }

        /// <summary>
        /// Constructor (obviously)
        /// </summary>
        /// <param name="size">Size of dialog (relative to screen)</param>
        /// <param name="title">The title displayed on the dialog</param>
        protected Dialog(System.Drawing.SizeF size, string title)
            :
            this(size)
        {
            this.title = title;
        }

        /// <summary>
        /// Constructor (obviously)
        /// </summary>
        /// <param name="layoutFilename">The .layout filename that decribes this dialog</param>
        protected Dialog(string layoutFilename)
            : base(MakeDialogId(), layoutFilename)
        {
        }

        /// <summary>
        /// Each dialog needs a unique identifier.
        /// </summary>
        /// <returns>A unique string</returns>
        private static string MakeDialogId()
        {
            // OK, this isn't thread safe, and could theoretically overflow
            // but as we're not threading, and I can't see one session
            // reaching 2 billion dialogs, so I don't think that's going to 
            // be a problem
            return Util.StringFormat("Dialog_{0}", ++dialogIdCounter);
        }

        /// <summary>
        /// Construct the widget that will be the parent for all widgets on this dialog
        /// </summary>
        /// <returns>the widget</returns>
        protected override CeGui.Window ConstructRootWidget()
        {
            CeGui.Widgets.FrameWindow frameWidget = GuiBuilder.CreateFrameWindow(CeguiId);
            if (title != null)
            {
                frameWidget.Text = title;
            }
            else
            {
                frameWidget.Text = CeguiId;
            }
            frameWidget.AlwaysOnTop = true;
            return frameWidget;
        }

        #region Fields

        /// <summary>
        /// delegate that is hooked up to yes or no button
        /// </summary>
        public delegate void ButtonAction();

        private static int dialogIdCounter;

        private string title;

        #endregion
    }
}
