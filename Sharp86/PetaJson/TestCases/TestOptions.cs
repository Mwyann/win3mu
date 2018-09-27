using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PetaTest;
using PetaJson;
using System.Reflection;

namespace TestCases
{
    [TestFixture]
    [Obfuscation(Exclude = true, ApplyToMembers = true)]
    public class TestOptions
    {
        [Test]
        public void TestWhitespace()
        {
            var o = new { x = 10, y = 20 };

            var json = Json.Format(o, JsonOptions.WriteWhitespace);
            Assert.Contains(json, "\n");
            Assert.Contains(json, "\t");
            Assert.Contains(json, ": ");

            json = Json.Format(o, JsonOptions.DontWriteWhitespace);
            Assert.DoesNotContain(json, "\n");
            Assert.DoesNotContain(json, "\t");
            Assert.DoesNotContain(json, ": ");
        }
        
        [Test]
        public void TestStrictComments()
        {
            var jsonWithCComment = "/* This is a comment*/ 23";
            var jsonWithCppComment = "// This is a comment\n 23";

            // Nonstrict parser allows it
            var val = Json.Parse<int>(jsonWithCComment, JsonOptions.NonStrictParser);
            Assert.AreEqual(val, 23);
            val = Json.Parse<int>(jsonWithCppComment, JsonOptions.NonStrictParser);
            Assert.AreEqual(val, 23);

            // Strict parser
            Assert.Throws<JsonParseException>(() => Json.Parse<int>(jsonWithCComment, JsonOptions.StrictParser));
            Assert.Throws<JsonParseException>(() => Json.Parse<int>(jsonWithCppComment, JsonOptions.StrictParser));
        }

        [Test]
        public void TestStrictTrailingCommas()
        {
            var arrayWithTrailingComma = "[1,2,]";
            var dictWithTrailingComma = "{\"a\":1,\"b\":2,}";

            // Nonstrict parser allows it
            var array = Json.Parse<int[]>(arrayWithTrailingComma, JsonOptions.NonStrictParser);
            Assert.AreEqual(array.Length, 2);
            var dict = Json.Parse<IDictionary<string, object>>(dictWithTrailingComma, JsonOptions.NonStrictParser);
            Assert.AreEqual(dict.Count, 2);

            // Strict parser
            Assert.Throws<JsonParseException>(() => Json.Parse<int>(arrayWithTrailingComma, JsonOptions.StrictParser));
            Assert.Throws<JsonParseException>(() => Json.Parse<int>(dictWithTrailingComma, JsonOptions.StrictParser));
        }

        [Test]
        public void TestStrictIdentifierKeys()
        {
            var data = "{a:1,b:2}";

            var dict = Json.Parse<IDictionary<string, object>>(data, JsonOptions.NonStrictParser);
            Assert.AreEqual(dict.Count, 2);
            Assert.Contains(dict.Keys, "a");
            Assert.Contains(dict.Keys, "b");

            Assert.Throws<JsonParseException>(() => Json.Parse<Dictionary<string, object>>(data, JsonOptions.StrictParser));
        }
    }
}
