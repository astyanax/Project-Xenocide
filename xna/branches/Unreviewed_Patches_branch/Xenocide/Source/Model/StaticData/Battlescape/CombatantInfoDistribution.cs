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
* @file CombatantInfoDistribution.cs
* @date Created: 2007/12/26
* @author File creator: David Teviotdale
* @author Credits: nil
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Text;
using System.Diagnostics;
using System.Xml;
using System.Xml.XPath;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;


using ProjectXenocide.Utils;
using ProjectXenocide.Model;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Model.Battlescape;
using ProjectXenocide.Model.Battlescape.Combatants;

#endregion

namespace ProjectXenocide.Model.StaticData.Battlescape
{
    /// <summary>
    /// Creates Combatants
    /// </summary>
    public partial class CombatantInfo
    {
        /// <summary>
        /// Describes the range of inital values for a Statistic
        /// </summary>
        private abstract class Distribution
        {
            /// <summary>
            /// Create a value from the distribution
            /// </summary>
            /// <returns>created value</returns>
            public abstract int MakeValue();
        }

        /// <summary>
        /// Statistic always has same value
        /// </summary>
        private class CoreDistribution : Distribution
        {
            /// <summary>
            /// Construct distribution from an xml element
            /// </summary>
            /// <param name="node">xml element</param>
            public CoreDistribution(XPathNavigator node)
            {
                this.core = Util.GetIntAttribute(node, "base");
            }

            /// <summary>
            /// Create a value from the distribution
            /// </summary>
            /// <returns>created value</returns>
            public override int MakeValue()
            {
                return core;
            }

            private int core;
        }

        /// <summary>
        /// Statistic values follow normal distribution
        /// </summary>
        private class NormalDistribution : Distribution
        {
            /// <summary>
            /// Construct distribution from an xml element
            /// </summary>
            /// <param name="node">xml element</param>
            public NormalDistribution(XPathNavigator node)
            {
                this.mean     = Util.GetIntAttribute(node, "mean");
                this.variance = Util.GetIntAttribute(node, "variance");
            }

            /// <summary>
            /// Create a value from the distribution
            /// </summary>
            /// <returns>created value</returns>
            public override int MakeValue()
            {
                // ToDo: implement correctly.
                return mean + variance;
            }

            private int mean;
            private int variance;
        }

        /// <summary>
        /// Statistic values follow uniform distribution
        /// </summary>
        private class UniformDistribution : Distribution
        {
            /// <summary>
            /// Construct distribution from an xml element
            /// </summary>
            /// <param name="node">xml element</param>
            public UniformDistribution(XPathNavigator node)
            {
                this.min = Util.GetIntAttribute(node, "min");
                this.max = Util.GetIntAttribute(node, "max");
            }

            /// <summary>
            /// Create a value from the distribution
            /// </summary>
            /// <returns>created value</returns>
            public override int MakeValue()
            {
                // ToDo: implement correctly.
                return (min + max) / 2;
            }

            private int min;
            private int max;
        }
    }
}
