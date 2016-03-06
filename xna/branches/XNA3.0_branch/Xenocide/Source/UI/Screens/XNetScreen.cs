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
* @file XNetScreen.cs
* @date Created: 2007/01/21
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Text;

using CeGui;

using ProjectXenocide.UI.Scenes.XNet;

using ProjectXenocide.Model.StaticData;
using ProjectXenocide.Utils;

#endregion

namespace ProjectXenocide.UI.Screens
{
    /// <summary>
    /// Displays the Encyclopedia of research and info
    /// </summary>
    class XNetScreen : PolarScreen
    {

        /// <summary>
        /// constructor (obviously)
        /// </summary>
        public XNetScreen()
            : base("XNetScreen", @"Content\Textures\UI\XnetScreenBackground.png")
        {
            Scene = new XNetScene();
            Xenocide.AudioSystem.PlayRandomMusic("XNet");
        }

        #region Create the CeGui widgets

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        protected override void CreateCeguiWidgets()
        {
            SetView(0.005f, 0.0733f, 0.5038f, 0.4417f);

            // Fake the list of items with a list box
            InitEntriesTree();

            // CeGui# doesn't allow scrolling static text, so fake with list box
            textWindow = GuiBuilder.CreateListBox(CeguiId + "_text");
            AddWidget(textWindow, 0.002f, 0.52f, 0.541f, 0.475f);

            // and provide a close button
            closeButton = AddButton("BUTTON_CLOSE", 0.7475f, 0.9400f, 0.2275f, 0.04125f);
            closeButton.Clicked += new CeGui.GuiEventHandler(OnCloseButton);
        }

        /// <summary>
        /// Create the "tree" of available xnet entries
        /// <remarks>Currently we're faking it with a list box</remarks>
        /// </summary>
        private void InitEntriesTree()
        {
            // create the list box
            entriesTree = GuiBuilder.CreateListBox("entriesTree");
            AddWidget(entriesTree, 0.606f, 0.073f, 0.366f, 0.8600f);

            Dictionary<string, int> catNames = new Dictionary<string, int>();
            foreach (XNetEntry e in Xenocide.StaticTables.XNetEntryList)
            {
                if (!catNames.ContainsKey(e.Category) && EntryAvailableToPlayer(e))
                {
                    catNames.Add(e.Category, 0);
                    categories.Add(new Category(e.Category, categories, entriesTree));
                }
            }

            entriesTree.SelectionChanged += new WindowEventHandler(OnEntrySelected);
        }

        private CeGui.Widgets.Listbox    entriesTree;
        private CeGui.Widgets.PushButton closeButton;
        private CeGui.Widgets.Listbox    textWindow;

        #endregion Create the CeGui widgets

        #region event handlers

        /// <summary>User has clicked the close button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope",
           Justification = "FxCop False Positive")]
        private void OnCloseButton(object sender, CeGui.GuiEventArgs e)
        {
            ScreenManager.ScheduleScreen(new GeoscapeScreen());
        }

        /// <summary>User has selected an X-Net entry to display</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnEntrySelected(object sender, WindowEventArgs e)
        {
            CeGui.Widgets.ListboxItem item = entriesTree.GetFirstSelectedItem();
            if (item != null)
            {
                Xenocide.AudioSystem.PlaySound("Menu\\buttonclick2_changesetting.ogg");
                if (Category.categoryIndexOffset <= item.ID)
                {
                    // user clicked on a category (so expand or collapse it)
                    categories[item.ID - Category.categoryIndexOffset].Toggle();

                    //unselect header, to avoid having to double click to close topic
                    item.Selected = false;
                }
                else
                {
                    // user clicked an entry, so show its text and model
                    XNetEntry entry = Xenocide.StaticTables.XNetEntryList[item.ID];
                    PopulateEntryText(entry);

                    // and set the 3D model to show
                    xnetScene.SetModel(entry.Graphic.Model, entry.Graphic.InitialRotation);
                }
            }
        }

        #endregion event handlers

        /// <summary>
        /// Returns true if player is allowed to view this entry
        /// </summary>
        /// <param name="e">entry to examine</param>
        /// <returns>true if player is allowed to view entry</returns>
        private static bool EntryAvailableToPlayer(XNetEntry e) 
        {
            return Xenocide.StaticTables.StartSettings.Cheats.ShowAllXNetEntries ||
                Xenocide.GameState.GeoData.XCorp.TechManager.IsAvailable(e.Id);
        }

        #region Functions to put the entry's text into the "text box"

        /// <summary>
        /// Fill the X-Net entry "text box" with the text from the specified entry
        /// </summary>
        /// <param name="entry">entry to get text from</param>
        private void PopulateEntryText(XNetEntry entry)
        {
            // clear the current text
            textWindow.ResetList();
            AddToEntryText(entry.ShortEntry);
            if (0 < entry.Stats.Count)
            {
                AddToEntryText(String.Empty);
                AddToEntryText(entry.Stats);
            }
            AddToEntryText(String.Empty);
            AddToEntryText(entry.Body);
            AddToEntryText(String.Empty);
            AddToEntryText(entry.Fluff);
        }

        /// <summary>
        /// Append these strings to the end of the entry's text
        /// </summary>
        /// <param name="strings">strings to append</param>
        private void AddToEntryText(StringCollection strings)
        {
            foreach (string s in strings)
            {
                AddToEntryText(s);
            }
        }

        /// <summary>
        /// Add this text as a new line to the entry's "text box"
        /// </summary>
        /// <param name="text">text to add</param>
        private void AddToEntryText(string text)
        {
            string line = "";
            string textLeft = FixupSpecialChars(text);
            do
            {
                line = ExtractLine(ref textLeft);
                CeGui.ListboxTextItem item = new CeGui.ListboxTextItem(line);
                item.SetSelectionBrushImage("TaharezLook", "MultiListSelectionBrush");
                textWindow.AddItem(item);
            }
            while (0 < textLeft.Length);
        }

        /// <summary>
        /// Extract enough text from string to fill one line of Text box
        /// </summary>
        /// <param name="value">String to get text from.  (Text is removed from value)</param>
        /// <returns>Enough text to fill a line</returns>
        private static string ExtractLine(ref string value)
        {
            const int MaxChars = 52;
            int length = value.Length;
            string temp = "";
            if (MaxChars < length)
            {
                // find a space (to avoid splitting a word between lines)
                length = MaxChars;
                while ((0 < length) && (' ' != value[length - 1]))
                {
                    --length;
                }

                if (length == 0)
                {
                    length = MaxChars + 1;
                }

                temp = value.Substring(0, length - 1);
                value = value.Substring(length, value.Length - length);
            }
            else
            {
                temp = value;
                value = "";
            }
            return temp;
        }

        /// <summary>
        /// XNA fonts currently only work with ASCII chars, but the xnet.xml is generated from
        /// RTF, so has special open and close quotes and apostrophies.
        /// This function replaces them with their ASCII equivelents
        /// </summary>
        /// <param name="text">String that may contain special characters</param>
        /// <returns>String with special characters replaced</returns>
        private static string FixupSpecialChars(string text)
        {
            text = text.Replace((char)8220, '\"');
            text = text.Replace((char)8221, '\"');
            text = text.Replace((char)8217, '\'');
            return text;
        }

        #endregion Functions to put the entry's text into the "text box"

        /// <summary>
        /// Move camera due to mouse move
        /// </summary>
        /// <param name="e">details of the mouse move</param>
        protected override void RotateSceneUsingMouse(CeGui.MouseEventArgs e)
        {
            // Calculate pointer position (in pixels)
            // relative to upper left corner of SceneWindow
            float x = e.Position.X - SceneWindow.AbsoluteX;
            float y = e.Position.Y - SceneWindow.AbsoluteY;

            float halfWidth = SceneWindow.AbsoluteWidth / 2.0f;
            float halfHeight = SceneWindow.AbsoluteHeight / 2.0f;

            // Calculate how percentually close the pointer is to the edges of SceneWindow,
            // 0.0f if pointer is in center, 1.0f if pointer is on the edge
            float factorFromCenterX = Math.Abs((x - halfWidth) / halfWidth);
            float factorFromCenterY = Math.Abs((y - halfHeight) / halfHeight);

            // Calculate roll amount caused by horizontal movement.
            // The closer the pointer is to an edge the more roll.
            float rollCausedByHorizontalMove = -e.MoveDelta.X * factorFromCenterY;

            // Calculate roll amount caused by vertical movement.
            float rollCausedByVerticalMove = e.MoveDelta.Y * factorFromCenterX;

            // Correct roll direction
            if (y > halfHeight)
                rollCausedByHorizontalMove = -rollCausedByHorizontalMove;
            if (x > halfWidth)
                rollCausedByVerticalMove = -rollCausedByVerticalMove;
            
            // Apply rotations
            float rotationSpeed = -0.0045f * (Scene.CameraHeight + 1.0f);

            Scene.RotateCamera(rotationSpeed * e.MoveDelta.X * (1 - factorFromCenterY),
                               rotationSpeed * e.MoveDelta.Y * (1 - factorFromCenterX));

            float totalRoll = rollCausedByHorizontalMove + rollCausedByVerticalMove;
            xnetScene.RollCamera(rotationSpeed * totalRoll);
        }

        /// <summary>
        /// Manage the categories in the tree
        /// </summary>
        private class Category
        {
            /// <summary>
            /// Constructor (duh)
            /// </summary>
            /// <param name="name">Name of the category</param>
            /// <param name="categories">List of all the categories so far</param>
            /// <param name="entriesTree">List box (tree) to put the category into</param>
            public Category(string name, List<Category> categories, CeGui.Widgets.Listbox entriesTree)
            {
                this.name        = name;
                this.categories  = categories;
                this.entriesTree = entriesTree;
                index            = categories.Count;
                categoryItem     = CreateCategoryMenuItem(name, index + categoryIndexOffset);
                entriesTree.AddItem(categoryItem);
            }

            /// <summary>
            /// Toggle this category between the expanded and colapsed states in the tree (list)
            /// </summary>
            public void Toggle()
            {
                if (isExpanded)
                {
                    Collapse();
                }
                else
                {
                    Expand();
                }
            }

            /// <summary>
            /// Populate tree (list) with the X-Net entries in this category
            /// </summary>
            private void Expand()
            {
                if (!isExpanded)
                {
                    isExpanded = true;
                    int i = 0;
                    foreach(XNetEntry e in Xenocide.StaticTables.XNetEntryList)
                    {
                        if (e.Category == name && XNetScreen.EntryAvailableToPlayer(e))
                        {
                            ShowEntry(e, i);
                        }
                        ++i;
                    }
                }
            }

            /// <summary>
            /// Put the specified entry on the tree (list) under this context
            /// </summary>
            /// <param name="entry">entry to show</param>
            /// <param name="id">id to assign entry (it's the index to entries list)</param>
            private void ShowEntry(XNetEntry entry, int id)
            {
                CeGui.ListboxTextItem item = CreateEntryMenuItem(entry.Name, id);
                // if this is last category, must add the entries to bottom of list
                if (index == (categories.Count - 1))
                {
                    entriesTree.AddItem(item);
                }
                else
                {
                    entriesTree.InsertItem(item, categories[index + 1].categoryItem);
                }
                childItems.Add(item);
            }

            /// <summary>
            /// Remove from the tree (list) the X-Net entries in this category
            /// </summary>
            private void Collapse()
            {
                isExpanded = false;
                foreach (CeGui.ListboxTextItem item in childItems)
                {
                    entriesTree.RemoveItem(item);
                }
                childItems.Clear();
            }

            /// <summary>
            /// Create an item that will be a type (topic) heading in the menu
            /// </summary>
            /// <param name="categoryName">Text to show in menu</param>
            /// <param name="id">index to categories</param>
            /// <returns></returns>
            private static CeGui.ListboxTextItem CreateCategoryMenuItem(string categoryName, int id)
            {
                return CreateListboxItem("", categoryName, id);
            }

            /// <summary>
            /// Create an item that will be a XNet entry in the menu
            /// </summary>
            /// <param name="entryName">Text to show in the menu</param>
            /// <param name="id">index to X-Net Entries</param>
            /// <returns></returns>
            private static CeGui.ListboxTextItem CreateEntryMenuItem(string entryName, int id)
            {
                return CreateListboxItem("  ", entryName, id);
            }

            /// <summary>
            /// Create an item to go into the menu of available X-Net entries
            /// </summary>
            /// <param name="padding">indetaion of item, to mimic a tree</param>
            /// <param name="itemText">Text to show in menu</param>
            /// <param name="id">index to XNetEntries</param>
            private static CeGui.ListboxTextItem CreateListboxItem(string padding, string itemText, int id)
            {
                CeGui.ListboxTextItem item = Util.CreateListboxItem(padding + itemText);
                item.ID = id;
                return item;
            }

            /// <summary>
            /// Put IDs for categories in separate range to entries
            /// </summary>
            public const int categoryIndexOffset = 100000;

            /// <summary>
            /// Index for this category in categories
            /// </summary>
            private int index;

            /// <summary>
            /// Name this category appears as in the tree
            /// </summary>
            private string name;

            /// <summary>
            /// The item in the list box representing this category
            /// </summary>
            private CeGui.ListboxTextItem categoryItem;

            /// <summary>
            /// List of all the categories
            /// </summary>
            private List<Category> categories;

            /// <summary>
            /// The list box (pretending to be a tree) that shows the available X-Net entries
            /// </summary>
            private CeGui.Widgets.Listbox entriesTree;

            /// <summary>
            /// Has this category been expanded to show all it's child entries?
            /// </summary>
            private bool isExpanded;

            private List<CeGui.ListboxTextItem> childItems = new List<CeGui.ListboxTextItem>();
        }

        private List<Category> categories = new List<Category>();

        /// <summary>
        /// Return the scene field as its real type (a XNetScene)
        /// </summary>
        private XNetScene xnetScene { get { return (XNetScene)Scene; } }
    }
}