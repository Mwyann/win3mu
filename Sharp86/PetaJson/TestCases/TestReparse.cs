using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PetaTest;
using PetaJson;

namespace TestCases
{
    class DaObject
    {
        [Json] public long id;
        [Json] public string Name;
    }

    [TestFixture]
    public class TestReparse
    {
        void Compare(DaObject a, DaObject b)
        {
            Assert.AreEqual(a.id, b.id);
            Assert.AreEqual(a.Name, b.Name);
        }

        [Test]
        public void Clone()
        {
            var a = new DaObject() { id = 101, Name = "#101" };
            var b = Json.Clone(a);
            Compare(a, b);
        }

        [Test]
        public void Reparse()
        {
            var a = new DaObject() { id = 101, Name = "#101" };
            var dict = Json.Reparse<IDictionary<string, object>>(a);

            Assert.AreEqual(dict["id"], 101UL);
            Assert.AreEqual(dict["name"], "#101");

            var b = Json.Reparse<DaObject>(dict);

            Compare(a, b);
        }
    }
}
