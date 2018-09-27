using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PetaTest;
using PetaJson;
using System.Reflection;

namespace TestCases
{
    [Obfuscation(Exclude = true, ApplyToMembers = true)]
    class ModelNotDecorated
    {
        public string Field1;
        public string Field2;
        public string Prop1 { get; set; }
        public string Prop2 { get; set; }
    }

    [Obfuscation(Exclude = true, ApplyToMembers = true)]
    class ModelInclude
    {
        [Json] public string Field1;
        public string Field2;
        [Json] public string Prop1 { get; set; }
        public string Prop2 { get; set; }
    }

    [Obfuscation(Exclude = true, ApplyToMembers = true)]
    class ModelExclude
    {
        public string Field1;
        public string Field2;
        public string Prop1 { get; set; }
        public string Prop2 { get; set; }

        [JsonExclude]
        public string Field3;

        [JsonExclude]
        public string Prop3 { get; set; }
    }

    [Obfuscation(Exclude = true, ApplyToMembers = true)]
    class ModelRenamedMembers
    {
        [Json("Field1")] public string Field1;
        public string Field2;
        [Json("Prop1")] public string Prop1 { get; set; }
        public string Prop2 { get; set; }
    }

    [Json]
    [Obfuscation(Exclude = true, ApplyToMembers = true)]
    class InstanceObject
    {
        public int IntVal1;

        [JsonExclude] public int IntVal2;

    }

    [Json]
    [Obfuscation(Exclude = true, ApplyToMembers = true)]
    class ModelKeepInstance
    {
        [Json(KeepInstance=true)]
        public InstanceObject InstObj;
    }

    [Json]
    [Obfuscation(Exclude = true, ApplyToMembers = true)]
    class ModelWithInstance
    {
        [Json]
        public InstanceObject InstObj;
    }

    [Json]
    [Obfuscation(Exclude = true, ApplyToMembers = true)]
    struct ModelStruct
    {
        public int IntField;
        public int IntProp { get; set; }
    }

    [TestFixture]
    [Obfuscation(Exclude = true, ApplyToMembers = true)]
    public class TestsReflection
    {
        [Test]
        public void ExcludeAttribute()
        {
            var m = new ModelExclude()
            {
                Field1 = "f1",
                Field2 = "f2",
                Field3 = "f3", 
                Prop1 = "p1",
                Prop2 = "p2",
                Prop3 = "p3",
            };

            var json = Json.Format(m);

            Assert.Contains(json, "field1");
            Assert.Contains(json, "field2");
            Assert.DoesNotContain(json, "field3");
            Assert.Contains(json, "prop1");
            Assert.Contains(json, "prop2");
            Assert.DoesNotContain(json, "prop3");
        }

        [Test]
        public void NonDecorated()
        {
            var m = new ModelNotDecorated()
            {
                Field1 = "f1",
                Field2 = "f2",
                Prop1 = "p1",
                Prop2 = "p2",
            };

            var json = Json.Format(m);

            Assert.Contains(json, "field1");
            Assert.Contains(json, "field2");
            Assert.Contains(json, "prop1");
            Assert.Contains(json, "prop2");
        }

        [Test]
        public void Include()
        {
            var m = new ModelInclude()
            {
                Field1 = "f1",
                Field2 = "f2",
                Prop1 = "p1",
                Prop2 = "p2",
            };

            var json = Json.Format(m);

            Assert.Contains(json, "field1");
            Assert.DoesNotContain(json, "field2");
            Assert.Contains(json, "prop1");
            Assert.DoesNotContain(json, "prop2");
        }

        [Test]
        public void RenamedMembers()
        {
            var m = new ModelRenamedMembers()
            {
                Field1 = "f1",
                Field2 = "f2",
                Prop1 = "p1",
                Prop2 = "p2",
            };

            var json = Json.Format(m);

            Assert.Contains(json, "Field1");
            Assert.DoesNotContain(json, "field2");
            Assert.Contains(json, "Prop1");
            Assert.DoesNotContain(json, "prop2");
        }

        [Test]
        public void KeepInstanceTest1()
        {
            // Create model and save it
            var ki = new ModelKeepInstance();
            ki.InstObj = new InstanceObject();
            ki.InstObj.IntVal1 = 1;
            ki.InstObj.IntVal2 = 2;
            var json = Json.Format(ki);

            // Update the kept instance object
            ki.InstObj.IntVal1 = 11;
            ki.InstObj.IntVal2 = 12;

            // Reload
            var oldInst = ki.InstObj;
            Json.ParseInto(json, ki);

            // Check object instance kept
            Assert.AreSame(oldInst, ki.InstObj);

            // Check json properties updated, others not
            Assert.AreEqual(ki.InstObj.IntVal1, 1);
            Assert.AreEqual(ki.InstObj.IntVal2, 12);
        }

        [Test]
        public void KeepInstanceTest2()
        {
            // Create model and save it
            var ki = new ModelKeepInstance();
            ki.InstObj = new InstanceObject();
            ki.InstObj.IntVal1 = 1;
            ki.InstObj.IntVal2 = 2;
            var json = Json.Format(ki);

            // Update the kept instance object
            ki.InstObj = null;

            // Reload
            Json.ParseInto(json, ki);

            // Check object instance kept
            Assert.IsNotNull(ki.InstObj);

            // Check json properties updated, others not
            Assert.AreEqual(ki.InstObj.IntVal1, 1);
            Assert.AreEqual(ki.InstObj.IntVal2, 0);
        }

        [Test]
        public void StructTest()
        {
            var o = new ModelStruct();
            o.IntField = 23;
            o.IntProp = 24;

            var json = Json.Format(o);
            Assert.Contains(json, "23");
            Assert.Contains(json, "24");

            var o2 = Json.Parse<ModelStruct>(json);
            Assert.AreEqual(o2.IntField, 23);
            Assert.AreEqual(o2.IntProp, 24);

            // Test parseInto on a value type not supported
            var o3 = new ModelStruct();
            Assert.Throws<InvalidOperationException>(() => Json.ParseInto(json, o3));
        }

        [Test]
        public void NullClassMember()
        {
            var m = new ModelWithInstance();
            var json = Json.Format(m);

            Assert.Contains(json, "null");

            m.InstObj = new InstanceObject();

            Json.ParseInto(json, m);
            Assert.IsNull(m.InstObj);
        }

        [Test]
        public void NullClass()
        {
            // Save null
            var json = Json.Format(null);
            Assert.AreEqual(json, "null");

            // Load null
            var m = Json.Parse<ModelWithInstance>("null");
            Assert.IsNull(m);

            // Should fail to parse null into an existing instance
            m = new ModelWithInstance();
            Assert.Throws<JsonParseException>(() => Json.ParseInto("null", m));
        }
    }
}
