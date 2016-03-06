using System;
using System.Collections.Generic;
using System.Text;
using Xenocide.Model.StaticData;

namespace Xenocide.Model.Geoscape
{
    /// <summary>
    /// Class responsible for holding information about which XNET-Topics are visible to the player
    /// </summary>
    [Serializable]
    public class XNetState
    {
        /// <summary>
        /// Construct a new XNETState with all entries invisible.
        /// </summary>
        public XNetState()
        {
            entries = new Dictionary<string, bool>();   
        }

        /// <summary>
        /// Returns whether a particular XNET-Entry is visible to the player
        /// </summary>
        /// <param name="id">The symbolic id of the entry to check</param>
        /// <returns>true if the entry is visible to the player</returns>
        public bool this[string id]
        {
            get
            {
                return entries.ContainsKey(id);
            }
        }

        /// <summary>
        /// Make an XNET-Entry visible to the player. If it's already visible, do nothing.
        /// </summary>
        /// <param name="id">The id of the entry to make visible</param>
        public void ActivateEntry(string id)
        {
            if(!entries.ContainsKey(id))
                entries.Add(id, true);
        }

        private Dictionary<string, bool> entries;
    }
}
