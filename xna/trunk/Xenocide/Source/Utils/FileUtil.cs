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
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Storage;
using System.IO;
using Microsoft.Xna.Framework;

#endregion

namespace Xenocide.Source.Utils
{
    /// <summary>
    /// Provides some common methods for persisting game data
    /// </summary>
    static class FileUtil
    {
        /// <summary>
        /// Get the container (directory) holding the requested file
        /// </summary>
        /// <param name="pathName">Full path to requested file</param>
        /// <returns>the container</returns>
        public static StorageContainer GetContainer(string pathName)
        {
            // this bit is dummy on windows
            string directory = Path.GetDirectoryName(pathName);
            IAsyncResult result = Guide.BeginShowStorageDeviceSelector(PlayerIndex.One, null, null);
            StorageDevice device = Guide.EndShowStorageDeviceSelector(result);

            // Now open container(directory)
            return device.OpenContainer(directory);
        }

        /// <summary>
        /// Get correct pathname to use to get a file
        /// Allow for path being different on an X-Box
        /// </summary>
        /// <param name="container">container holding the file</param>
        /// <param name="pathName">full path</param>
        /// <returns>correct path</returns>
        public static string TruePathName(StorageContainer container, string pathName)
        {
            return Path.Combine(container.Path, Path.GetFileName(pathName));
        }

        /// <summary>
        /// Will check to see if a file exists.
        /// </summary>
        /// <param name="pathName">File Name to check for</param>
        /// <returns></returns>
        public static bool DoesFileExist(string pathName)
        {
            using (StorageContainer container = GetContainer(pathName))
            {
                return File.Exists(TruePathName(container, pathName));
            }
        }
    }
}
