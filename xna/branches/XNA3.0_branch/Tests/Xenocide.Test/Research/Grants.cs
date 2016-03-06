using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace ProjectXenocide.Research
{
    [TestFixture]
    public class GrantsTest
    {
        public class SuccessGrant : ITechGrant
        {
            public bool Granted = false;

            public void Grant()
            {
                this.Granted = true;
            } 
        }

        [Test]
        public void AllOfGrantSingle()
        {
            IList<ITechGrant> list = new List<ITechGrant>();
            list.Add(new SuccessGrant());

            AllOfGrant grant = new AllOfGrant(list);
            grant.Grant();

            foreach (SuccessGrant techGrant in list)
                Assert.IsTrue(techGrant.Granted);
        }

        [Test]
        public void AllOfGrantMultiple()
        {
            IList<ITechGrant> list = new List<ITechGrant>();
            list.Add(new SuccessGrant());
            list.Add(new SuccessGrant());
            list.Add(new SuccessGrant());

            AllOfGrant grant = new AllOfGrant(list);
            grant.Grant();

            foreach (SuccessGrant techGrant in list)
                Assert.IsTrue(techGrant.Granted);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AllOfGrantNullCreation()
        {
            AllOfGrant grant = new AllOfGrant(null);
        }

        [Test]
        public void AllOfGrantEmptyCreation()
        {
            AllOfGrant grant = new AllOfGrant(new List<ITechGrant>());
            grant.Grant();
        }





        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void OneOfGrantNullCreation()
        {
            OneOfGrant grant = new OneOfGrant(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void OneOfGrantEmptyCreation()
        {
            OneOfGrant grant = new OneOfGrant(new List<ITechGrant>());
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void OneOfGrantSingle()
        {
            IList<ITechGrant> list = new List<ITechGrant>();
            list.Add(new SuccessGrant());

            OneOfGrant grant = new OneOfGrant(list);
        }

        [Test]
        public void OneOfGrantMultiple()
        {
            IList<ITechGrant> list = new List<ITechGrant>();
            list.Add(new SuccessGrant());
            list.Add(new SuccessGrant());
            list.Add(new SuccessGrant());
            list.Add(new SuccessGrant());

            OneOfGrant grant = new OneOfGrant(list);
            grant.Grant();

            int granted = 0;
            foreach (SuccessGrant techGrant in list)
                if (techGrant.Granted)
                    granted++;

            Assert.AreEqual(1, granted);
        }
    }
}
