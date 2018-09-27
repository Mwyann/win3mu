using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PetaTest;
using PetaJson;

namespace TestCases
{
    [TestFixture]
    public class TestDictionaryUtils
    {
        [Test]
        public void DictionaryPaths()
        {
            var dict = new Dictionary<string, object>();
            dict.SetPath("settings.subSettings.settingA", 23);
            dict.SetPath("settings.subSettings.settingB", 24);

            Assert.IsTrue(dict.ContainsKey("settings"));
            Assert.IsTrue(((IDictionary<string, object>)dict["settings"]).ContainsKey("subSettings"));
            Assert.AreEqual(dict.GetPath<int>("settings.subSettings.settingA"), 23);
            Assert.AreEqual(dict.GetPath<int>("settings.subSettings.settingB"), 24);
            Assert.IsTrue(dict.PathExists("settings.subSettings"));
            Assert.IsTrue(dict.PathExists("settings.subSettings.settingA"));
            Assert.IsFalse(dict.PathExists("missing_in_action"));
        }

        [Test]
        public void DictionaryReparseType()
        {
            // Create and initialize and object then convert it to a dictionary
            var o = new DaObject() { id = 101, Name = "#101" };
            var oDict = Json.Reparse<IDictionary<string, object>>(o);

            // Store that dictionary at a path inside another dictionary
            var dict = new Dictionary<string, object>();
            dict.SetPath("settings.daObject", oDict);

            // Get it back out, but reparse it back into a strongly typed object
            var o2 = dict.GetPath<DaObject>("settings.daObject");
            Assert.AreEqual(o2.id, o.id);
            Assert.AreEqual(o2.Name, o.Name);
        }

        [Test]
        public void ObjectAtPath()
        {
            // Create and initialize and object then convert it to a dictionary
            var o = new DaObject() { id = 101, Name = "#101" };
            var oDict = Json.Reparse<IDictionary<string, object>>(o);

            // Store that dictionary at a path inside another dictionary
            var dict = new Dictionary<string, object>();
            dict.SetPath("settings.daObject", oDict);

            // Get it back as an object (and update dict to hold an actual DaObject
            var o2 = dict.GetObjectAtPath<DaObject>("settings.daObject");

            // Modify it
            o2.id = 102;
            o2.Name = "modified";

            // Save the dictionary and make sure we got the change
            var json = Json.Format(dict);
            Assert.Contains(json, "102");
            Assert.Contains(json, "modified");
        }

        [Test]
        public void NewObjectAtPath()
        {
            // Create a new object at a path
            var dict = new Dictionary<string, object>();
            var o2 = dict.GetObjectAtPath<DaObject>("settings.daObject");

            // Modify it
            o2.id = 103;
            o2.Name = "new guy";

            // Save the dictionary and make sure we got the change
            var json = Json.Format(dict);
            Assert.Contains(json, "103");
            Assert.Contains(json, "new guy");
        }
    }
}
