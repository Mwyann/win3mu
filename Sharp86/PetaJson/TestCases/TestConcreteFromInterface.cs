using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PetaTest;
using PetaJson;
using System.Collections;

namespace TestCases
{
    [TestFixture]
    class TestConcreteFromInterface
    {
        [Test]
        public void TestGenericList()
        {
            var l = new List<int>() { 10, 20, 30 };

            var json = Json.Format(l);

            var l2 = Json.Parse<IList<int>>(json);
            Assert.IsInstanceOf(typeof(List<int>), l2);

            Assert.AreEquivalent(l, l2);
        }

        [Test]
        public void TestGenericDictionary()
        {
            var l = new Dictionary<string,int>() { 
                {"A", 10}, 
                {"B", 20},
                {"C", 30}
            };

            var json = Json.Format(l);

            var l2 = Json.Parse<IDictionary<string,int>>(json);
            Assert.IsInstanceOf(typeof(Dictionary<string,int>), l2);

            Assert.AreEquivalent(l, l2);
        }

        [Test]
        public void TestObjectList()
        {
            var l = new List<int>() { 10, 20, 30 };

            var json = Json.Format(l);

            var l2 = Json.Parse<IList>(json);
            Assert.IsInstanceOf(typeof(List<object>), l2);

            Assert.AreEqual(l.Count, l2.Count);
        }

        [Test]
        public void TestObjectDictionary()
        {
            var l = new Dictionary<string, int>() { 
                {"A", 10}, 
                {"B", 20},
                {"C", 30}
            };

            var json = Json.Format(l);

            var l2 = Json.Parse<IDictionary>(json);
            Assert.IsInstanceOf(typeof(Dictionary<string,object>), l2);
            Assert.AreEqual(l.Count, l2.Count);
        }

    }
}
