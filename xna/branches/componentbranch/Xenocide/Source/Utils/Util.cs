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
* @file Util.cs
* @date Created: 2007/03/12
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

using Xenocide.Resources;
using Xenocide.Model;
using Xenocide.Model.Geoscape;

using CeGui;

#endregion

namespace Xenocide.Utils
{
    /// <summary>
    /// Assorted utility functions
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces",
        Justification = "we're not going to be using System.Web.Util")]
    public static class Util
    {
        /// <summary>
        /// Slightly enhanced Debug.WriteLine(), will timestamp line with game's GeoTime
        /// </summary>
        /// <param name="format">formatting string</param>
        /// <param name="args">values to inject into formatting string</param>
        [Conditional("DEBUG")]
        public static void GeoTimeDebugWriteLine(string format, params Object[] args)
        {
            Debug.WriteLine(
                "No GeoTime " /*Xenocide.GameState.GeoData.GeoTime.ToString()*/ + " " +
                StringFormat(format, args)
            );
        }

        /// <summary>
        /// Utility function, to stop Code Analysis complaining about String.Format lacking IFormatProvider
        /// </summary>
        /// <param name="format">formatting string</param>
        /// <param name="args">values to inject into formatting string</param>
        public static string StringFormat(string format, params Object[] args)
        {
            return String.Format(Thread.CurrentThread.CurrentCulture, format, args);
        }

        /// <summary>
        /// Load a string from the resources, with error checking
        /// </summary>
        /// <param name="resourceName">name of string to load</param>
        /// <returns>the loaded string</returns>
        public static string LoadString(string resourceName)
        {
            string temp = Strings.ResourceManager.GetString(resourceName);
            Debug.Assert(!String.IsNullOrEmpty(temp));
            return temp;
        }

        public static string LoadString(string resourceName, string fallback)
        {
            string temp = Strings.ResourceManager.GetString(resourceName);
            if (String.IsNullOrEmpty(temp))
                temp = fallback;
            
            return temp;
        }
        
        /// <summary>
        /// Advance XPathNavigator to specified attribute throwing exception if attribute not there
        /// </summary>
        /// <param name="element">Navigator pointing at element to get attribute from</param>
        /// <param name="attributeName">name of attribute to advance to</param>
        private static void MoveToAttribute(XPathNavigator element, string attributeName)
        {
            if (!element.MoveToAttribute(attributeName, String.Empty))
            {
                throw new XmlException(StringFormat(Strings.EXCEPTION_XML_ATTRIBUTE_NOT_FOUND, attributeName));
            }
        }
        
        /// <summary>
        /// Retrive an integer valued attribute from an XML element
        /// </summary>
        /// <param name="element">XML element holding the attribute</param>
        /// <param name="attributeName">attribute of the element</param>
        /// <param name="value">The value of the element</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if element == null")]
        public static void GetAttribute(XPathNavigator element, string attributeName, ref int value)
        {
            MoveToAttribute(element, attributeName);
            value = element.ValueAsInt;

            // Need to reset the navigator after calling MoveToAttribute
            element.MoveToParent();
        }

        /// <summary>
        /// Retrive a string valued attribute from an XML element
        /// </summary>
        /// <param name="element">XML element holding the attribute</param>
        /// <param name="attributeName">attribute of the element</param>
        /// <param name="value">The value of the element</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if element == null")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference",
           Justification = "I'm trying to make the interface consistent, by overloading function on value's type")]
        public static void GetAttribute(XPathNavigator element, string attributeName, ref string value)
        {
            MoveToAttribute(element, attributeName);
            value = element.Value;

            // Need to reset the navigator after calling MoveToAttribute
            element.MoveToParent();
        }

        /// <summary>
        /// Retrive a boolean valued attribute from an XML element
        /// </summary>
        /// <param name="element">XML element holding the attribute</param>
        /// <param name="attributeName">attribute of the element</param>
        /// <param name="value">The value of the element</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if element == null")]
        public static void GetAttribute(XPathNavigator element, string attributeName, ref bool value)
        {
            MoveToAttribute(element, attributeName);
            value = element.ValueAsBoolean;

            // Need to reset the navigator after calling MoveToAttribute
            element.MoveToParent();
        }

        /// <summary>
        /// Retrive a floating point double valued attribute from an XML element
        /// </summary>
        /// <param name="element">XML element holding the attribute</param>
        /// <param name="attributeName">attribute of the element</param>
        /// <param name="value">The value of the element</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if element == null")]
        public static void GetAttribute(XPathNavigator element, string attributeName, ref double value)
        {
            MoveToAttribute(element, attributeName);
            value = element.ValueAsDouble;

            // Need to reset the navigator after calling MoveToAttribute
            element.MoveToParent();
        }

        /// <summary>
        /// Retrive a float valued attribute from an XML element
        /// </summary>
        /// <param name="element">XML element holding the attribute</param>
        /// <param name="attributeName">attribute of the element</param>
        /// <param name="value">The value of the element</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if element == null")]
        public static void GetAttribute(XPathNavigator element, string attributeName, ref float value)
        {
            double temp = 0.0;
            GetAttribute(element, attributeName, ref temp);
            value = (float)temp;
        }

        /// <summary>
        /// Add a column to the supplied MultiColumnList
        /// </summary>
        public static ListboxTextItem CreateListboxItem(String text)
        {
            ListboxTextItem item = new ListboxTextItem(text);
            item.SetSelectionBrushImage("TaharezLook", "MultiListSelectionBrush");
            return item;
        }

        /// <summary>
        /// Get error message to show player coresponding to the XenoError enumeration
        /// </summary>
        /// <param name="xenoError">Error code</param>
        /// <returns>Textual represention of error</returns>
        public static String GetErrorMessage(XenoError xenoError)
        {
            switch (xenoError)
            {
                case XenoError.None:                     return Strings.XENOERROR_NONE;
                case XenoError.PositionNotInBase:        return Strings.XENOERROR_POSITION_NOT_IN_BASE;
                case XenoError.PositionAlreadyOccupied:  return Strings.XENOERROR_POSITION_ALREADY_OCCUPIED;
                case XenoError.CellHasNoNeighbours:      return Strings.XENOERROR_CELL_HAS_NO_NEIGHBOURS;
                case XenoError.FacilityIsInUse:          return Strings.XENOERROR_FACILITY_IS_IN_USE;
                case XenoError.DeleteWillSplitBase:      return Strings.XENOERROR_DELETE_WILL_SPLIT_BASE;

                default:
                    Debug.Assert(false);
                    return "";
            }
        }
    }
}
