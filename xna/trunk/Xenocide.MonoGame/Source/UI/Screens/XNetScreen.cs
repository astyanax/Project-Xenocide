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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;

using Gum.Forms;
using Gum.Forms.Controls;

using ProjectXenocide.Assets;
using ProjectXenocide.Model.StaticData;
using ProjectXenocide.UI.Scenes.XNet;
using ProjectXenocide.Utils;

using Xenocide.Resources;

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
            : base("XNetScreen", @"Content/Textures/UI/XnetScreenBackground.png")
        {
            Scene = new XNetScene();
            if (Xenocide.AudioSystem != null)
                Xenocide.AudioSystem.PlayRandomMusic();
        }

        #region Create the Gum controls

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        protected override void CreateGumControls()
        {
            SetView(0.005f, 0.0733f, 0.5038f, 0.4417f);

            if (GumRoot != null)
            {
                WireButton("closeButton", OnCloseButton);
                InitEntriesTree();
                entriesTree.Visual.X = 20;
                entriesTree.Visual.Y = 80;
                entriesTree.Visual.Width = 300;
                entriesTree.Visual.Height = 400;

                textWindow = new ListBox();
                textWindow.Visual.X = 340;
                textWindow.Visual.Y = 80;
                textWindow.Visual.Width = 300;
                textWindow.Visual.Height = 400;
                AddChild(textWindow);
                return;
            }

            // Fake the list of items with a list box
            InitEntriesTree();

            // Gum's ListBox provides native scrolling for long text entries
            textWindow = new ListBox();
            RootContainer.AddChild(textWindow);

            // and provide a close button
            closeButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_CLOSE") };
            RootContainer.AddChild(closeButton);
            closeButton.Click += OnCloseButton;
        }

        /// <summary>
        /// Create the "tree" of available xnet entries
        /// <remarks>Currently we're faking it with a list box</remarks>
        /// </summary>
        private void InitEntriesTree()
        {
            // create the list box
            entriesTree = new ListBox();
            AddChild(entriesTree);

            Dictionary<string, int> catNames = new Dictionary<string, int>();
            foreach (XNetEntry e in Xenocide.StaticTables.XNetEntryList)
            {
                if (!catNames.ContainsKey(e.Category) && EntryAvailableToPlayer(e))
                {
                    catNames.Add(e.Category, 0);
                    categories.Add(new Category(e.Category, categories, entriesTree, entryItemIds));
                }
            }

            entriesTree.SelectionChanged += (s, a) => OnEntrySelected(s, EventArgs.Empty);
        }

        private ListBox entriesTree;
        private Button closeButton;
        private ListBox textWindow;

        private List<int> entryItemIds = new List<int>();

        #endregion Create the Gum controls

        public override bool HandleEscape()
        {
            if (Xenocide.DebugTesting)
            {
                Xenocide.DebugTesting = false;
                ScreenManager.ScheduleScreen(new StartScreen());
            }
            else
            {
                ScreenManager.ScheduleScreen(new GeoscapeScreen());
            }
            return true;
        }

        #region event handlers

        /// <summary>User has clicked the close button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope",
           Justification = "FxCop False Positive")]
        private void OnCloseButton(object sender, EventArgs e)
        {
            if (Xenocide.DebugTesting)
            {
                Xenocide.DebugTesting = false;
                ScreenManager.ScheduleScreen(new StartScreen());
            }
            else
            {
                ScreenManager.ScheduleScreen(new GeoscapeScreen());
            }
        }

        /// <summary>User has selected an X-Net entry to display</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnEntrySelected(object sender, EventArgs e)
        {
            int index = entriesTree.SelectedIndex;
            if (index >= 0 && index < entryItemIds.Count)
            {
                int id = entryItemIds[index];
                Xenocide.AudioSystem.PlaySound(SoundId.ButtonClick2);
                if (Category.categoryIndexOffset <= id)
                {
                    // user clicked on a category (so expand or collapse it)
                    categories[id - Category.categoryIndexOffset].Toggle();

                    //unselect header, to avoid having to double click to close topic
                    entriesTree.SelectedIndex = -1;
                }
                else
                {
                    // user clicked an entry, so show its text and model
                    XNetEntry entry = Xenocide.StaticTables.XNetEntryList[id];
                    using (Profile.Time("XNet.PopulateEntryText"))
                        PopulateEntryText(entry);
                    using (Profile.Time("XNet.SetModel: " + entry.Graphic.Model))
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
            textWindow.Items.Clear();
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
        /// Add this text as a new line to the entry's "text box".
        /// Previously used CeGui-era manual word-wrapping (ExtractLine at 52 chars) which
        /// called Items.Add for every wrapped line — hundreds of individual Gum layout
        /// updates per entry. Now adds the entire text as a single item; Gum's ListBox
        /// handles word-wrapping natively within each item's visual bounds.
        /// </summary>
        /// <param name="text">text to add</param>
        private void AddToEntryText(string text)
        {
            string processed = FixupSpecialChars(text);
            if (!string.IsNullOrEmpty(processed))
                textWindow.Items.Add(processed);
        }

        /// <summary>
        /// Legacy CeGui word-wrapping: splits text at ~52 characters per visual line.
        /// No longer used in production; retained for reference since it handles
        /// word-boundary splitting that native Gum wrapping may approach differently.
        /// </summary>
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

                temp = value[..(length - 1)];
                value = value[length..];
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
            /// <param name="itemIds">Parallel list tracking IDs for each item in the list box</param>
            public Category(string name, List<Category> categories, ListBox entriesTree, List<int> itemIds)
            {
                this.name = name;
                this.categories = categories;
                this.entriesTree = entriesTree;
                this.itemIds = itemIds;
                index = categories.Count;

                entriesTree.Items.Add(name);
                itemIds.Add(index + categoryIndexOffset);
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
                    foreach (XNetEntry e in Xenocide.StaticTables.XNetEntryList)
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
                string itemText = "  " + entry.Name;
                // if this is last category, must add the entries to bottom of list
                if (index == (categories.Count - 1))
                {
                    entriesTree.Items.Add(itemText);
                    itemIds.Add(id);
                }
                else
                {
                    int insertPos = FindNextCategoryPosition();
                    entriesTree.Items.Insert(insertPos, itemText);
                    itemIds.Insert(insertPos, id);
                }
                childItems.Add(itemText);
            }

            /// <summary>
            /// Find the position of the next category in the list box
            /// </summary>
            private int FindNextCategoryPosition()
            {
                Category nextCat = categories[index + 1];
                for (int i = 0; i < entriesTree.Items.Count; i++)
                {
                    if ((string)entriesTree.Items[i] == nextCat.name)
                        return i;
                }
                return entriesTree.Items.Count;
            }

            /// <summary>
            /// Remove from the tree (list) the X-Net entries in this category
            /// </summary>
            private void Collapse()
            {
                isExpanded = false;
                foreach (string itemText in childItems)
                {
                    int pos = -1;
                    for (int i = 0; i < entriesTree.Items.Count; i++)
                    {
                        if ((string)entriesTree.Items[i] == itemText)
                        {
                            pos = i;
                            break;
                        }
                    }
                    if (pos >= 0)
                    {
                        entriesTree.Items.RemoveAt(pos);
                        itemIds.RemoveAt(pos);
                    }
                }
                childItems.Clear();
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
            /// List of all the categories
            /// </summary>
            private List<Category> categories;

            /// <summary>
            /// The list box (pretending to be a tree) that shows the available X-Net entries
            /// </summary>
            private ListBox entriesTree;

            /// <summary>
            /// Parallel list tracking the ID for each item in the entriesTree
            /// </summary>
            private List<int> itemIds;

            /// <summary>
            /// Has this category been expanded to show all it's child entries?
            /// </summary>
            private bool isExpanded;

            private List<string> childItems = new List<string>();
        }

        private List<Category> categories = new List<Category>();

        /// <summary>
        /// Return the scene field as its real type (a XNetScene)
        /// </summary>
        private XNetScene xnetScene { get { return (XNetScene)Scene; } }
    }
}
