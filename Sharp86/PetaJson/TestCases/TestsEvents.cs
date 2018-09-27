using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PetaTest;
using PetaJson;
using System.Reflection;

namespace TestCases
{
    [Json]
    struct StructEvents : IJsonLoaded, IJsonLoading, IJsonLoadField, IJsonWriting, IJsonWritten
    {
        public int IntField;

        [JsonExclude] public bool loading;
        [JsonExclude] public bool loaded;
        [JsonExclude] public bool fieldLoaded;

        void IJsonLoaded.OnJsonLoaded(IJsonReader r)
        {
            loaded = true;
        }

        void IJsonLoading.OnJsonLoading(IJsonReader r)
        {
            loading = true;
        }

        bool IJsonLoadField.OnJsonField(IJsonReader r, string key)
        {
            fieldLoaded = true;
            return false;
        }

        void IJsonWriting.OnJsonWriting(IJsonWriter w)
        {
            w.WriteRaw("/* OnJsonWriting */");
        }

        void IJsonWritten.OnJsonWritten(IJsonWriter w)
        {
            w.WriteRaw("/* OnJsonWritten */");
        }
    }


    [TestFixture]
    [Obfuscation(Exclude = true, ApplyToMembers = true)]
    public class TestsEvents
    {
        [Test]
        public void TestStructLoadEvents()
        {
            var o2 = Json.Parse<StructEvents>("{\"IntField\":23}");
            Assert.IsTrue(o2.loading);
            Assert.IsTrue(o2.loaded);
            Assert.IsTrue(o2.fieldLoaded);
        }

        [Test]
        public void TestStructWriteEvents()
        {
            var o = new StructEvents();
            o.IntField = 23;

            var json = Json.Format(o);
            Assert.Contains(json, "OnJsonWriting");
            Assert.Contains(json, "OnJsonWritten");
        }
    }
}
