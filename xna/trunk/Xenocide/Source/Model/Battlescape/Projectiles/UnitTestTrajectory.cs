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
* @file UnitTestTrajectory.cs
* @date Created: 2008/02/24
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;


using ProjectXenocide.Utils;

#endregion

namespace ProjectXenocide.Model.Battlescape
{
    /// <summary>The path taken by an object that has been fired or thrown.
    /// e.g. a missile, bullet, plasma burst, thrown object, etc.
    /// </summary>
    public partial class Trajectory
    {
        #region UnitTests

        /// <summary>
        /// Run set of tests
        /// </summary>
        [Conditional("DEBUG")]
        public static void RunTests()
        {
            Trajectory t = new Trajectory(new Vector3(), new Vector3(0, -3, 0), 1);
            Debug.Assert(t.velocity == new Vector3(0, -1, 0));
            Debug.Assert(t.Update(1.5f));
            Debug.Assert(t.current == new Vector3(0, -1.5f, 0));
            Debug.Assert(!t.Update(1.5f));
            Debug.Assert(t.current == new Vector3(0, -3, 0));
        }

        #endregion UnitTests
    }
}
