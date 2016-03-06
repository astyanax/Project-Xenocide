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
* @file Screen.cs
* @date Created: 2007/01/20
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

#endregion

namespace Xenocide.UI.Screens
{
    /// <summary>
    /// This is the base class that all screens derive from
    /// A "screen" is a window that fills the entire display
    /// The screen is responsible for:
    /// 1. Creating the CeGeui widgets on the display
    /// 2. Handing events from the widgets
    /// 3. Managing the 3D scene (if there is one) on the screen
    /// 4. Pumping Update() and Draw() to scene and game's model
    /// <remarks>Because the CeGui widgets are XNA Components
    /// the Screen class does NOT pump Update() and Draw() to the widgets
    /// themselves.  That's done by the XNA framework</remarks>
    /// </summary>
    public abstract class Screen : Frame
    {
        /// <summary>
        /// Constructor (obviously)
        /// </summary>
        /// <param name="ceguiId">CeGui's identifer for this screen</param>
        /// <param name="screenManager">The screen manager</param>
        protected Screen(Game game, string ceguiId)
            :
            base(game, ceguiId, new System.Drawing.SizeF(1.0f, 1.0f))
        {
        }
    }
}
