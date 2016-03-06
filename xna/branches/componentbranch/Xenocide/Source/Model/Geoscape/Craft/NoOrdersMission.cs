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
* @file NoOrdersMission.cs
* @date Created: 2007/02/21
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace Xenocide.Model.Geoscape.Craft
{
    /// <summary>
    /// The "mission" of a craft that's sitting in a base with no orders
    /// </summary>
    [Serializable]
    public class NoOrdersMission : Mission
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="craft">craft that "owns" this mission</param>
        public NoOrdersMission(Craft craft)
            :
            base(craft)
        {
            SetState(new InBaseState(this));
        }
    }
}
