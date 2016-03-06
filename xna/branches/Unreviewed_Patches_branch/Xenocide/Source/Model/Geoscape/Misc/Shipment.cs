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
* @file Shipment.cs
* @date Created: 2007/09/23
* @author File creator: David Teviotdale
* @author Credits: none
*/

#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;


using ProjectXenocide.Model.StaticData;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.Geoscape.GeoEvents;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.Model.Geoscape
{
    /// <summary>
    /// A collection of items being sent to an X-Corp outpost
    /// Items may be a transfer, salvage from mission or a purchase
    /// </summary>
    [Serializable]
    public class Shipment
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="destination">Where the shipment is going</param>
        /// <param name="eta">How long shipment is expected to take to arive at its destination</param>
        public Shipment(Outpost destination, TimeSpan eta)
        {
            this.destination = destination;
            this.eta         = Xenocide.GameState.GeoData.GeoTime.Time + eta;
        }

        /// <summary>
        /// Called when Shipment arrives at it's destination
        /// </summary>
        public void OnShipmentArrive()
        {
            // put items into outpost
            foreach(Item item in items)
            {
                destination.Inventory.ClearReservation(item.ItemInfo);
                destination.Inventory.Add(item, false);
            }

            // tell outpost the package has arrived
            destination.Inventory.OnShipmentArrive(this);

            // tell player the package has arrived
            MessageBoxGeoEvent.Queue(Strings.MSGBOX_SHIPMENT_ARRIVED, destination.Name);
        }

        /// <summary>
        /// Called if Outpost this shipment was heading for has been destroyed
        /// </summary>
        public void OnOutpostDestroyed()
        {
            // Shipment is basically destroying itself.
            // ToDo: Might redirect to other bases that have sufficent spare space
            Xenocide.GameState.GeoData.GeoTime.Scheduler.Remove(appointment);
        }

        /// <summary>
        /// Add an item to the shipment
        /// </summary>
        /// <param name="item">item to add</param>
        public void Add(Item item)
        {
            items.Add(item);

            // reserve space for item when it arrives at destination
            destination.Inventory.ReserveSpace(item.ItemInfo);

            // update manifest
            if (item.ItemInfo.IsUnique)
            {
                // unique items always get their own line
                manifest.Add(new ManifestLine(item.Name));
            }
            else
            {
                // stackable item, if already present, add to quantity, else new line
                ManifestLine line = Find(item.Name);
                if (null == line)
                {
                    manifest.Add(new ManifestLine(item.Name));
                }
                else
                {
                    ++line.Quantity;
                }
            }
        }

        /// <summary>
        /// Add a set of items to the shipment
        /// </summary>
        /// <param name="lines">items to add</param>
        public void Add(IEnumerable<ItemLine> lines)
        {
            foreach (ItemLine line in lines)
            {
                for (int i = 0; i < line.Quantity; ++i)
                {
                    Add(line.Construct());
                }
            }
        }

        /// <summary>
        /// Start shipment on its way to destination
        /// </summary>
        public void Ship()
        {
            // if the shipment is empty, nothing to do
            if (0 < items.Count)
            {
                TimeSpan span = eta - Xenocide.GameState.GeoData.GeoTime.Time;
                appointment = Xenocide.GameState.GeoData.GeoTime.MakeAppointment(span, OnShipmentArrive);
                destination.Inventory.Add(this);
            }
        }

        /// <summary>
        /// Figure out how long it will take shipment to arrive
        /// </summary>
        /// <returns></returns>
        public static TimeSpan CalcEta()
        {
            // all shipments take 48 hours to arrive
            // ToDo: replace this with some other mechanism.
            return new TimeSpan(48, 0, 0);
        }

        /// <summary>
        /// Return manifest line for an item, or null if there isn't one
        /// </summary>
        /// <param name="label">label value of manifest line to find</param>
        /// <returns>manifest line, or null</returns>
        private ManifestLine Find(string label)
        {
            return manifest.Find(delegate(ManifestLine line) { return line.Label == label; });
        }

        #region Fields

        /// <summary>
        /// list of items in shipment
        /// </summary>
        public IEnumerable<Item> Items
        {
            get { return items; }
        }

        /// <summary>
        /// Time left until the shipment is expected to arive at its destination
        /// </summary>
        public TimeSpan Eta { get { return (eta - Xenocide.GameState.GeoData.GeoTime.Time); } }

        /// <summary>
        /// Summary of items in shipment, for display to user
        /// </summary>
        public IList<ManifestLine> Manifest { get { return manifest; } }

        /// <summary>
        /// The actual items in the shipment
        /// </summary>
        private List<Item> items = new List<Item>();

        /// <summary>
        /// Where the shipment is going
        /// </summary>
        private Outpost destination;

        /// <summary>
        /// When the shipment is expected to arive at its destination
        /// </summary>
        private DateTime eta;

        /// <summary>
        /// Summary of items in shipment, for display to user
        /// </summary>
        private List<ManifestLine> manifest = new List<ManifestLine>();

        /// <summary>
        /// Appointment for when this shipment is expected to arrive
        /// </summary>
        private Appointment appointment;

        /// <summary>
        /// A line entry in the Manifest
        /// </summary>
        [Serializable]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible",
            Justification="We can handle nested classes")]
        public class ManifestLine
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="label">Name of item</param>
            public ManifestLine(string label)
            {
                this.label    = label;
                this.quantity = 1;
            }

            #region Fields

            /// <summary>
            /// Name of item
            /// </summary>
            public string Label { get { return label; } }

            /// <summary>
            /// Quantity of item
            /// </summary>
            public int Quantity { get { return quantity; } set { quantity = value; } }

            /// <summary>
            /// Name of item
            /// </summary>
            private string label;

            /// <summary>
            /// Quantity of item
            /// </summary>
            private int quantity;

            #endregion Fields
        }

        #endregion Fields
    }
}
