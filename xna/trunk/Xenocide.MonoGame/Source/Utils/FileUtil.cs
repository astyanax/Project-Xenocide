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
* @file GameOptions.cs
* @date Created: 2009/10/19
* @author File creator: John Perrin
* @author Credits: none
*/
#endregion
#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
#endregion
namespace Xenocide.Source.Utils
{
    static class FileUtil
    {
        public static bool DoesFileExist(string pathName)
        {
            return File.Exists(pathName);
        }
    }
}
