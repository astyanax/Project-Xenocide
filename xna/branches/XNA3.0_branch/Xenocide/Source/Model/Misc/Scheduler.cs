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
* @file Scheduler.cs
* @date Created: 2007/08/06
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using CeGui;

#endregion

namespace ProjectXenocide.Model
{
    /// <summary>
    /// Keeps track of the scheduled events, and fires them at the correct time
    /// </summary>
    [Serializable]
    public class Scheduler
    {
        /// <summary>
        /// Add an appointment to the list of appointments
        /// </summary>
        /// <param name="appointment">to add</param>
        public void Add(Appointment appointment)
        {
            // they're kept in soonest first order
            // because list is maintained in order, insertion sort is most efficient
            LinkedListNode<Appointment> node = appointments.First;
            while (null != node)
            {
                if (appointment.Occurs < node.Value.Occurs)
                {
                    appointments.AddBefore(node, appointment);
                    return;
                }
                node = node.Next;
            }
            appointments.AddLast(appointment);
        }

        /// <summary>
        /// Remove an appointment from the list before it was scheduled to occur
        /// </summary>
        /// <param name="appointment">to remove</param>
        public void Remove(Appointment appointment)
        {
            appointments.Remove(appointment);
        }

        /// <summary>
        /// Tell scheduler that a certain amount of GameTime has passed
        /// </summary>
        /// <param name="now">what the time currently is</param>
        public void Update(DateTime now)
        {
            LinkedListNode<Appointment> node = appointments.First;
            while ((null != node) && (node.Value.Occurs <= now))
            {
                appointments.Remove(node);
                node.Value.Process();
                node = appointments.First;
            }
        }

        #region Fields

        /// <summary>
        /// The events
        /// </summary>
        private LinkedList<Appointment> appointments = new LinkedList<Appointment>();

        #endregion Fields

        #region Unit Tests

        /// <summary>
        /// Stub class for testing
        /// </summary>
        private class TestAppointment : Appointment
        {
            public TestAppointment(DateTime occurs) : base(occurs) { }
            /// <summary>
            /// 
            /// </summary>
            public override void Process() {}
        }

        /// <summary>
        /// Run set of tests
        /// </summary>
        [Conditional("DEBUG")]
        public static void RunTests()
        {
            TestAddRemove();
        }

        /// <summary>
        /// Try to add and remove appointments
        /// </summary>
        [Conditional("DEBUG")]
        public static void TestAddRemove()
        {
            Scheduler       s   = new Scheduler();
            TestAppointment a1  = new TestAppointment(new DateTime(2020,1,1));
            TestAppointment a2  = new TestAppointment(new DateTime(2019,1,1));
            TestAppointment a3  = new TestAppointment(new DateTime(2021,1,1));
            DateTime        now = DateTime.Now;

            s.Add(a1);
            s.Add(a2);
            s.Add(a3);
            Debug.Assert(3 == s.appointments.Count);
            Debug.Assert(s.appointments.First.Value == a2);
            Debug.Assert(s.appointments.Last.Value  == a3);

            s.Remove(a3);
            Debug.Assert(2 == s.appointments.Count);
            Debug.Assert(s.appointments.Last.Value  == a1);

            // replace a3
            s.Add(a3);
            Debug.Assert(3 == s.appointments.Count);
            Debug.Assert(s.appointments.First.Value == a2);
            Debug.Assert(s.appointments.Last.Value == a3);

            // Update, with none firing
            s.Update(now);
            Debug.Assert(3 == s.appointments.Count);
            Debug.Assert(s.appointments.First.Value == a2);
            Debug.Assert(s.appointments.Last.Value == a3);

            // Update, fire just the first
            s.Update(new DateTime(2019, 1, 2));
            Debug.Assert(2 == s.appointments.Count);
            Debug.Assert(s.appointments.First.Value == a1);
            Debug.Assert(s.appointments.Last.Value == a3);

            // Update, fire all
            s.Update(new DateTime(2021, 1, 2));
            Debug.Assert(0 == s.appointments.Count);
        }

        #endregion Unit Tests
    }
}
