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
* @file XenoError.cs
* @date Created: 2007/05/07
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using System.Xml.XPath;
using System.Threading;

#endregion


namespace ProjectXenocide
{
    /// <summary>
    /// A collection of error codes that may be returned from fuctions
    /// that can have multiple error returns
    /// </summary>
    public enum XenoError
    {
        /// <summary>
        /// The "No error" error code
        /// </summary>
        None,

        /// <summary>
        /// Can't put facility here, it won't be (completely) inside base's floorplan
        /// </summary>
        PositionNotInBase,

        /// <summary>
        /// There's already a facility occupying this postion
        /// </summary>
        PositionAlreadyOccupied,

        /// <summary>
        /// Can only build a facility next to an already built facility
        /// </summary>
        CellHasNoNeighbours,

        /// <summary>
        /// You can't delete a facility that is in use
        /// </summary>
        FacilityIsInUse,

        /// <summary>
        /// Deleting this facility will prevent access to the access lift (for some facilities)
        /// </summary>
        DeleteWillSplitBase,
    }
}
