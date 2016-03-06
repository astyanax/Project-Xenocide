using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace Xenocide.Research
{
    [TestFixture]
    public class PrerequisitesTest
    {
        public class UnlockedTechPrerequisite : ITechPrerequisite
        {
            public bool Evaluate()
            {
                return true;
            }
        }

        public class LockedTechPrerequisite : ITechPrerequisite
        {
            public bool Evaluate()
            {
                return false;
            }
        }

        [Test]
        public void AllOfPrerequisiteSingle()
        {
            IList<ITechPrerequisite> unlockedList = new List<ITechPrerequisite>();
            unlockedList.Add(new UnlockedTechPrerequisite());

            AllOfPrerequisite unlockedPrerequisite = new AllOfPrerequisite(unlockedList);
            Assert.IsTrue(unlockedPrerequisite.Evaluate());


            IList<ITechPrerequisite> lockedList = new List<ITechPrerequisite>();
            lockedList.Add(new LockedTechPrerequisite());

            AllOfPrerequisite lockedPrerequisite = new AllOfPrerequisite(lockedList);
            Assert.IsFalse(lockedPrerequisite.Evaluate());
        }


        [Test]
        public void AllOfPrerequisiteMultiple()
        {
            IList<ITechPrerequisite> unlockedList = new List<ITechPrerequisite>();
            unlockedList.Add(new UnlockedTechPrerequisite());
            unlockedList.Add(new UnlockedTechPrerequisite());
            unlockedList.Add(new UnlockedTechPrerequisite());

            AllOfPrerequisite unlockedPrerequisite = new AllOfPrerequisite(unlockedList);
            Assert.IsTrue(unlockedPrerequisite.Evaluate());


            IList<ITechPrerequisite> lockedList = new List<ITechPrerequisite>();
            lockedList.Add(new LockedTechPrerequisite());
            lockedList.Add(new LockedTechPrerequisite());
            lockedList.Add(new LockedTechPrerequisite());

            AllOfPrerequisite lockedPrerequisite = new AllOfPrerequisite(lockedList);
            Assert.IsFalse(lockedPrerequisite.Evaluate());


            IList<ITechPrerequisite> mixedList = new List<ITechPrerequisite>();
            mixedList.Add(new UnlockedTechPrerequisite());
            mixedList.Add(new LockedTechPrerequisite());
            mixedList.Add(new UnlockedTechPrerequisite());

            AllOfPrerequisite mixedPrerequisite = new AllOfPrerequisite(mixedList);
            Assert.IsFalse(mixedPrerequisite.Evaluate());
        }



        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void AllOfPrerequisiteConstructionFailure()
        {
            IList<ITechPrerequisite> unlockedList = new List<ITechPrerequisite>();
            AllOfPrerequisite unlockedPrerequisite = new AllOfPrerequisite(unlockedList);
        }


        [Test]
        public void AnyOfPrerequisiteMultiple()
        {
            IList<ITechPrerequisite> unlockedList = new List<ITechPrerequisite>();
            unlockedList.Add(new UnlockedTechPrerequisite());
            unlockedList.Add(new UnlockedTechPrerequisite());
            unlockedList.Add(new UnlockedTechPrerequisite());

            AnyOfPrerequisite unlockedPrerequisite = new AnyOfPrerequisite(unlockedList);
            Assert.IsTrue(unlockedPrerequisite.Evaluate());


            IList<ITechPrerequisite> lockedList = new List<ITechPrerequisite>();
            lockedList.Add(new LockedTechPrerequisite());
            lockedList.Add(new LockedTechPrerequisite());
            lockedList.Add(new LockedTechPrerequisite());

            AnyOfPrerequisite lockedPrerequisite = new AnyOfPrerequisite(lockedList);
            Assert.IsFalse(lockedPrerequisite.Evaluate());


            IList<ITechPrerequisite> mixedList = new List<ITechPrerequisite>();
            mixedList.Add(new LockedTechPrerequisite());
            mixedList.Add(new LockedTechPrerequisite());
            mixedList.Add(new UnlockedTechPrerequisite());

            AnyOfPrerequisite mixedPrerequisite = new AnyOfPrerequisite(mixedList);
            Assert.IsTrue(mixedPrerequisite.Evaluate());
        }


        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void AnyOfPrerequisiteConstructionFailure()
        {
            IList<ITechPrerequisite> unlockedList = new List<ITechPrerequisite>();
            unlockedList.Add(new UnlockedTechPrerequisite());

            AnyOfPrerequisite unlockedPrerequisite = new AnyOfPrerequisite(unlockedList);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AnyOfPrerequisiteConstructionNullFailure()
        {
            AllOfPrerequisite unlockedPrerequisite = new AllOfPrerequisite(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void AnyOfPrerequisiteConstructionZeroFailure()
        {
            IList<ITechPrerequisite> unlockedList = new List<ITechPrerequisite>();
            AllOfPrerequisite unlockedPrerequisite = new AllOfPrerequisite(unlockedList);
        }

        [Test]
        public void InitialTechPrerequisite()
        {
            ITechPrerequisite initialTech = new InitialTechPrerequisite();
            Assert.IsTrue(initialTech.Evaluate());
        }
    }
}
