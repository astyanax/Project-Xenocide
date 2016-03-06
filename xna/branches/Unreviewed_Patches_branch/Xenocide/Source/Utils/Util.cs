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
* @author File creator: David Teviotdale
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
using System.IO;
using System.Globalization;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Storage;

using ProjectXenocide.Model;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.UI.Screens;
using ProjectXenocide.UI.Dialogs;

using CeGui;

using Vector3 = Microsoft.Xna.Framework.Vector3;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.Utils
{

	/// <summary>
	/// Provides some common methods for persisting game data
	/// </summary>
	public static class GameFiles
	{
		/// <summary>
		/// Get the container (directory) holding the saved files
		/// </summary>
		/// <returns>the container</returns>
		private static StorageContainer GetContainer(string saveDirectory)
		{
			// this bit is dummy on windows
			IAsyncResult result = Guide.BeginShowStorageDeviceSelector(PlayerIndex.One, null, null);
			StorageDevice device = Guide.EndShowStorageDeviceSelector(result);

			// Now open container(directory)
			return device.OpenContainer(saveDirectory);
		}

		/// <summary>
		/// Will check to see if a file exists.
		/// </summary>
		/// <param name="locationToCheck">Folder to check for file</param>
		/// <param name="fileToCheck">File Name to check for</param>
		/// <returns></returns>
		public static bool DoesFileExist(string locationToCheck, string fileToCheck)
		{
			using (StorageContainer container = GetContainer(locationToCheck))
			{
				return File.Exists(Path.Combine(container.Path, fileToCheck));
			}
		}

		/// <summary>
		/// This method will seralize and save a class of type T.  
		/// </summary>
		/// <typeparam name="T">Parameter Type</typeparam>
		/// <param name="objectToSave">Typed parameter, this will be saved.</param>
		/// <param name="locationToSave">Folder to save to</param>
		/// <param name="fileToSave">File to Save to</param>
		public static void Save<T>(T objectToSave, string locationToSave, string fileToSave)
		{
			using (StorageContainer container = GetContainer(locationToSave))
			{
				string filename = Path.Combine(container.Path, fileToSave);
				using (FileStream stream = File.Create(filename))
				{
					BinaryFormatter formatter = new BinaryFormatter();
					formatter.Serialize(stream, objectToSave);
				}
			}
		}

		/// <summary>
		/// Loads an file of type T from a specified file.
		/// </summary>
		/// <typeparam name="T">Type of object that will be returned.</typeparam>
		/// <param name="loadLocation">folder to load from</param>
		/// <param name="loadFile">file to load from</param>
		/// <returns></returns>
		public static T Load<T>(string loadLocation, string loadFile)
		{
			using (StorageContainer container = GetContainer(loadLocation))
			{
				using (FileStream stream = File.Open(Path.Combine(container.Path, loadFile), FileMode.Open))
				{
					BinaryFormatter formatter = new BinaryFormatter();
					return (T)formatter.Deserialize(stream);
				}
			}
		}
	}


    /// <summary>
    /// Used to pick an element at random from a set, where
    /// the different elements may have different selection probabilities
    /// </summary>
    public interface IOdds
    {
        /// <summary>
        /// Relative odds that this element is selected
        /// </summary>
        int Odds { get; }
    }

    /// <summary>Simple general purpose container</summary>
    public class Pair<T1, T2>
    {
        /// <summary>Ctor</summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        public Pair(T1 first, T2 second)
        {
            this.first = first;
            this.second = second;
        }
        #region fields
        /// <summary></summary>
        public T1 First { get { return first; } set { first = value; } }

        /// <summary></summary>
        public T2 Second { get { return second; } set { second = value; } }

        private T1 first;
        private T2 second;
        #endregion fields
    }

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
                Xenocide.GameState.GeoData.GeoTime.ToString() + " " +
                StringFormat(format, args)
            );
        }

        /// <summary>
        /// Utility function, to stop Code Analysis complaining about ToString lacking IFormatProvider
        /// </summary>
        /// <param name="i">integer to obtain string representation of</param>
        /// <returns>the string</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if obj == null")]
        public static string ToString(int i)
        {
            return i.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Format currency value to display to user
        /// </summary>
        /// <param name="dollars">value </param>
        /// <returns>string to display to user</returns>
        public static string FormatCurrency(int dollars)
        {
            return Util.StringFormat(Strings.FORMAT_CURRENCY, dollars);
        }

        /// <summary>
        /// Utility function, to stop Code Analysis complaining about String.Format lacking IFormatProvider
        /// </summary>
        /// <param name="format">formatting string</param>
        /// <param name="args">values to inject into formatting string</param>
        /// <returns>expanded string</returns>
        public static string StringFormat(string format, params Object[] args)
        {
            return String.Format(Thread.CurrentThread.CurrentCulture, format, args);
        }

        /// <summary>
        /// Load a string from the resources, with error checking
        /// </summary>
        /// <param name="resourceName">name of string to load</param>
        /// <returns>the loaded string</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
          Justification = "will throw if resourceName == null")]
        public static string LoadString(string resourceName)
        {
            // need to replace any hyphens in the name with underscores
            resourceName = resourceName.Replace('-', '_');

            string temp = XenocideResourceManager.Get(resourceName);
            Debug.Assert(!String.IsNullOrEmpty(temp));
            return temp;
        }

        /// <summary>
        /// Version of LoadString that will return the resourceName if can't find a resource with the name
        /// </summary>
        /// <param name="resourceName">name of string to load</param>
        /// <returns>the loaded string, or resourceName if not able to load</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
          Justification = "will throw if resourceName == null")]
        public static string SafeLoadString(string resourceName)
        {
            // need to replace any hyphens in the name with underscores
            resourceName = resourceName.Replace('-', '_');

            string temp = XenocideResourceManager.Get(resourceName);
            if (String.IsNullOrEmpty(temp))
            {
                temp = resourceName;
            }
            return temp;
        }

        /// <summary>
        /// Check if this XML element contains a specified attribute
        /// </summary>
        /// <param name="element">Navigator pointing at element to get attribute from</param>
        /// <param name="attributeName">name of attribute to check for</param>
        /// <returns>true if element found</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
          Justification = "will throw if element == null")]
        public static bool AttributePresent(XPathNavigator element, string attributeName)
        {
            bool found = element.MoveToAttribute(attributeName, String.Empty);

            // Need to reset the navigator after calling MoveToAttribute
            if (found)
            {
                element.MoveToParent();
            }
            return found;
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if element == null")]
        public static int GetIntAttribute(XPathNavigator element, string attributeName)
        {
            return XmlConvert.ToInt32(GetStringAttribute(element, attributeName));
        }

        /// <summary>
        /// Retrive a string valued attribute from an XML element
        /// </summary>
        /// <param name="element">XML element holding the attribute</param>
        /// <param name="attributeName">attribute of the element</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if element == null")]
        public static string GetStringAttribute(XPathNavigator element, string attributeName)
        {
            MoveToAttribute(element, attributeName);
            string value = element.Value;

            // Need to reset the navigator after calling MoveToAttribute
            element.MoveToParent();
            return value;
        }

        /// <summary>
        /// Retrive a boolean valued attribute from an XML element
        /// </summary>
        /// <param name="element">XML element holding the attribute</param>
        /// <param name="attributeName">attribute of the element</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if element == null")]
        public static bool GetBoolAttribute(XPathNavigator element, string attributeName)
        {
            return XmlConvert.ToBoolean(GetStringAttribute(element, attributeName));
        }

        /// <summary>
        /// Retrive a floating point double valued attribute from an XML element
        /// </summary>
        /// <param name="element">XML element holding the attribute</param>
        /// <param name="attributeName">attribute of the element</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if element == null")]
        public static double GetDoubleAttribute(XPathNavigator element, string attributeName)
        {
            return XmlConvert.ToDouble(GetStringAttribute(element, attributeName));
        }

        /// <summary>
        /// Retrive a float valued attribute from an XML element
        /// </summary>
        /// <param name="element">XML element holding the attribute</param>
        /// <param name="attributeName">attribute of the element</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if element == null")]
        public static float GetFloatAttribute(XPathNavigator element, string attributeName)
        {
            return (float)GetDoubleAttribute(element, attributeName);
        }

        /// <summary>
        /// Retrive an attribute holding a degrees value
        /// </summary>
        /// <param name="element">XML element holding the attribute</param>
        /// <param name="attributeName">attribute holding the degrees</param>
        /// <returns>degress, converted to radians</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if element == null")]
        public static float GetDegreesAttribute(XPathNavigator element, string attributeName)
        {
            return MathHelper.ToRadians(Util.GetFloatAttribute(element, attributeName));
        }

        /// <summary>
        /// Retrive a color keyy element from an XML element
        /// </summary>
        /// <param name="element">XML element holding the ColorKey child element</param>
        /// <param name="manager">Namespace used in planet.xml</param>
        /// <returns>the color, as a packed ARGB</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
             Justification = "will throw if element == null")]
        public static uint GetColorKey(XPathNavigator element, XmlNamespaceManager manager)
        {
            XPathNavigator colorKeyAttrib = element.SelectSingleNode("p:colorKey", manager);
            uint pixel = 0xFF000000;
            pixel += ((uint)GetIntAttribute(colorKeyAttrib, "R") << 16);
            pixel += ((uint)GetIntAttribute(colorKeyAttrib, "G") << 8);
            pixel += (uint)GetIntAttribute(colorKeyAttrib, "B");
            return pixel;
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
                case XenoError.None: 
                    return Strings.XENOERROR_NONE;
                case XenoError.PositionNotInBase: 
                    return Strings.XENOERROR_POSITION_NOT_IN_BASE;
                case XenoError.PositionAlreadyOccupied: 
                    return Strings.XENOERROR_POSITION_ALREADY_OCCUPIED;
                case XenoError.CellHasNoNeighbours: 
                    return Strings.XENOERROR_CELL_HAS_NO_NEIGHBOURS;
                case XenoError.FacilityIsInUse: 
                    return Strings.XENOERROR_FACILITY_IS_IN_USE;
                case XenoError.DeleteWillSplitBase: 
                    return Strings.XENOERROR_DELETE_WILL_SPLIT_BASE;
                default:
                    Debug.Assert(false);
                    return "";
            }
        }

        /// <summary>
        /// Format a message and put up as a message box
        /// </summary>
        /// <param name="format">The text to show in the message box</param>
        /// <param name="args">Arguments to put into the string</param>
        public static void ShowMessageBox(string format, params Object[] args)
        {
            Xenocide.ScreenManager.ShowDialog(new MessageBoxDialog(Util.StringFormat(format, args)));
        }

        /// <summary>
        /// Construct a validating XPathNavigator for the given XML file
        /// </summary>
        /// <param name="filename">full filename of XML file</param>
        /// <param name="xmlns">the namespace used in the XML file</param>
        /// <returns>the XPathNavigator</returns>
        public static XPathNavigator MakeValidatingXPathNavigator(string filename, string xmlns)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.Schemas.Add(xmlns, Path.ChangeExtension(filename, ".xsd"));
            settings.ValidationType = ValidationType.Schema;
            return (new XPathDocument(XmlReader.Create(filename, settings))).CreateNavigator();
        }

        /// <summary>
        /// Return an interator that returns a filtered subset of a collection
        /// </summary>
        /// <param name="collection">the collection to filter</param>
        /// <param name="filter">predicate that defines the filtering critera</param>
        /// <returns>the iterator</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
           Justification = "FxCop bug, reporting false positive")]
        public static IEnumerable<T> FilterColection<T>(IEnumerable<T> collection, Predicate<T> filter)
        {
            foreach (T t in collection)
            {
                if (filter(t))
                {
                    yield return t;
                }
            }
        }

        /// <summary>
        /// Find number of items in a sequence
        /// </summary>
        /// <remarks>Sequence is consumed by operation</remarks>
        /// <typeparam name="T">anything</typeparam>
        /// <param name="sequence">the sequence</param>
        /// <returns>number of items</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
           Justification = "FxCop bug, reporting false positive")]
        public static int SequenceLength<T>(IEnumerable<T> sequence)
        {
            int count = 0;
            IEnumerator<T> itr = sequence.GetEnumerator();
            while (itr.MoveNext())
            {
                ++count;
            }
            return count;
        }

        /// <summary>
        /// Add a cell holding a number to a MultiColumnList
        /// </summary>
        /// <param name="grid">the multicolumn list</param>
        /// <param name="column">to put the element in (zero based)</param>
        /// <param name="row">to put the element in (zero based)</param>
        /// <param name="value">integer to show in the element</param>
        public static void AddNumericElementToGrid<T>(CeGui.Widgets.MultiColumnList grid, int column, int row, T value)
        {
            AddStringElementToGrid(grid, column, row, StringFormat("{0}", value));
        }

        /// <summary>
        /// Add a cell holding a string to a MultiColumnList
        /// </summary>
        /// <param name="grid">the multicolumn list</param>
        /// <param name="column">to put the element in (zero based)</param>
        /// <param name="row">to put the element in (zero based)</param>
        /// <param name="text">text show in the element</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if grid == null")]
        public static void AddStringElementToGrid(CeGui.Widgets.MultiColumnList grid, int column, int row, string text)
        {
            grid.SetGridItem(column, row, CreateListboxItem(text));
        }

        /// <summary>
        /// Return value rounded to nearest 1000
        /// </summary>
        /// <param name="v">value to round</param>
        /// <returns>rounded value</returns>
        public static int RoundTo1000(double v)
        {
            return (int)(Math.Round(v / 1000)) * 1000;
        }

        /// <summary>
        /// Return the ratio of the two values as a percent
        /// </summary>
        /// <param name="numerator">the value</param>
        /// <param name="denominator">value that indicates 100%</param>
        /// <returns></returns>
        public static int ToPercent(double numerator, double denominator)
        {
            if (numerator < 0)
            {
                return 0;
            }
            if (denominator < numerator)
            {
                return 100;
            }
            return (int)Math.Round(numerator * 100.0 / denominator);
        }

        /// <summary>
        /// Round a float to nearest integer
        /// </summary>
        /// <param name="f">float to round</param>
        /// <returns>rounded value</returns>
        public static int Round(float f)
        {
            Debug.Assert(0.0f <= f);
            return (int)Math.Truncate(f + 0.5f);
        }

        /// <summary>
        /// Convert a string representation of an enumeration into it's enumerated value
        /// </summary>
        /// <typeparam name="T">The enumeration type</typeparam>
        /// <param name="s">string representation of the value</param>
        /// <returns>enumerated value</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
           Justification = "FxCop bug, reporting false positive")]
        public static T ParseEnum<T>(string s)
        {
            return (T)Enum.Parse(typeof(T), s, true);
        }

        /// <summary>
        /// Used to pick an element at random from a set, where
        /// the different elements may have different selection probabilities
        /// </summary>
        public static T SelectRandom<T>(ICollection<T> elements) where T : IOdds
        {
            // first, find total options.
            int totalOdds = 0;
            foreach (T element in elements)
            {
                totalOdds += element.Odds;
            }
            Debug.Assert(0 < totalOdds);

            // now pick at random
            int choice = Xenocide.Rng.Next(totalOdds) + 1;
            foreach (T element in elements)
            {
                choice -= element.Odds;
                if (choice <= 0)
                {
                    return element;
                }
            }

            // If get here, something went wrong
            Debug.Assert(false);
            return default(T);
        }

        /// <summary>
        /// Is the right mouse button down?
        /// </summary>
        /// <returns>true if button is down</returns>
        public static bool IsRightMouseButtonDown()
        {
            return (Mouse.GetState().RightButton == ButtonState.Pressed);
        }

        /// <summary>
        /// Compute bounding sphere for a model
        /// </summary>
        /// <param name="model">to compute bounding sphere for</param>
        /// <returns>the sphere</returns>
        public static BoundingSphere CalcBoundingSphere(Microsoft.Xna.Framework.Graphics.Model model)
        {
            // Copy any parent transforms
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            // compute bounding sphere that holds all the meshes in the model
            BoundingSphere sphere = new BoundingSphere();
            foreach (ModelMesh mesh in model.Meshes)
            {
                // appply mesh's bone transforms to bounding sphere
                float radius = mesh.BoundingSphere.Radius;
                Vector3 scale = new Vector3(radius, radius, radius);
                scale = Vector3.TransformNormal(scale, transforms[mesh.ParentBone.Index]);
                BoundingSphere subsphere = new BoundingSphere(
                    Vector3.Transform(mesh.BoundingSphere.Center, transforms[mesh.ParentBone.Index]),
                    Math.Max(Math.Abs(scale.X), Math.Max(Math.Abs(scale.Y), Math.Abs(scale.Z))));

                // now combine each mesh's bounding sphere
                if (0.0f == sphere.Radius)
                {
                    // first mesh
                    sphere = subsphere;
                }
                else
                {
                    sphere = BoundingSphere.CreateMerged(sphere, subsphere);
                }
            }
            return sphere;
        }

        /// <summary>Debugging test, assert two float values are same value</summary>
        /// <param name="v1">value to compare</param>
        /// <param name="v2">other value to compare it to</param>
        [Conditional("DEBUG")]
        public static void DebugTestFloatValuesSame(double v1, double v2)
        {
            Debug.Assert(MathHelper.Distance((float)v1, (float)v2) < 0.000001f);
        }

        /// <summary>Compute Axis Aligned Bounding Box for a model</summary>
        /// <param name="model">to compute AABB for</param>
        /// <returns>the AABB</returns>
        public static BoundingBox CalcBoundingBox(Microsoft.Xna.Framework.Graphics.Model model)
        {
            // Copy any parent transforms
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            // compute bounding box that holds all the meshes in the model
            BoundingBox box = new BoundingBox();
            bool firstMesh = true;
            foreach (ModelMesh mesh in model.Meshes)
            {
                // get limits of this mesh
                Vector3[] extents = CalcMeshExtents(mesh);

                // scale by bone transform
                Vector3.Transform(extents, ref transforms[mesh.ParentBone.Index], extents);

                // and merge with the boxes so far
                BoundingBox meshBox = new BoundingBox(extents[0], extents[1]);
                if (firstMesh)
                {
                    box = meshBox;
                    firstMesh = false;
                }
                else
                {
                    box = BoundingBox.CreateMerged(box, meshBox);
                }
            }
            return box;
        }

        /// <summary>Compute Axis Aligned Bounding Box for a model mesh</summary>
        /// <param name="mesh">to compute AABB for</param>
        /// <returns>min [0] and max [1] extents of the mesh</returns>
        public static Vector3[] CalcMeshExtents(ModelMesh mesh)
        {
            // get offset to position element in each vertex
            // assumes all mesh parts in mesh use same vertex layout
            int positionOffset = -1;
            foreach (VertexElement element in mesh.MeshParts[0].VertexDeclaration.GetVertexElements())
            {
                if (element.VertexElementUsage == VertexElementUsage.Position)
                {
                    positionOffset = element.Offset;
                    break;
                }
            }
            Debug.Assert(0 <= positionOffset);
            int stride = mesh.MeshParts[0].VertexStride;

            // calc number of vertices in mesh
            int vertices = mesh.VertexBuffer.SizeInBytes / stride;
            Debug.Assert((vertices * stride) == mesh.VertexBuffer.SizeInBytes);

            // load vertex info where it can be read.
            byte[] buffer = new byte[mesh.VertexBuffer.SizeInBytes];
            mesh.VertexBuffer.GetData<byte>(buffer);

            // scan the vertices, recording the maximum and minimum extents
            Vector3[] extents =
            {
                new Vector3( float.MaxValue,  float.MaxValue,  float.MaxValue),
                new Vector3(-float.MaxValue, -float.MaxValue, -float.MaxValue),
            };
            for (int i = 0; i < vertices; ++i)
            {
                float x = BitConverter.ToSingle(buffer, (i * stride) + positionOffset);
                extents[0].X = Math.Min(x, extents[0].X);
                extents[1].X = Math.Max(x, extents[1].X);
                float y = BitConverter.ToSingle(buffer, (i * stride) + positionOffset + sizeof(float));
                extents[0].Y = Math.Min(y, extents[0].Y);
                extents[1].Y = Math.Max(y, extents[1].Y);
                float z = BitConverter.ToSingle(buffer, (i * stride) + positionOffset + (2 * sizeof(float)));
                extents[0].Z = Math.Min(z, extents[0].Z);
                extents[1].Z = Math.Max(z, extents[1].Z);
            }
            return extents;
        }

        /// <summary>
        /// What shader version is supported?
        /// </summary>
        /// <param name="caps">capabilities of device to check</param>
        /// <returns>0 if can't tell, 1 if 1.x, 2 if 2.x, 3 if 3.0</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "Will throw if device is null")]
        public static int GetShaderVersion(GraphicsDeviceCapabilities caps)
        {
            ShaderProfile pixel = caps.MaxPixelShaderProfile;
            ShaderProfile vertex = caps.MaxVertexShaderProfile;
            if (pixel == ShaderProfile.Unknown)
            {
                return 0;
            }
            if ((pixel < ShaderProfile.PS_2_0) || (vertex < ShaderProfile.VS_2_0))
            {
                return 1;
            }
            if ((pixel < ShaderProfile.PS_3_0) || (vertex < ShaderProfile.VS_3_0))
            {
                return 2;
            }
            else
                return 3;
        }

        /// <summary>
        /// Linefeed sequence
        /// </summary>
        public const String Linefeed = "\r\n";
    }
}
