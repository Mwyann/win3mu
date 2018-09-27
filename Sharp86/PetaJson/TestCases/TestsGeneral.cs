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
    public class TestsGeneral
	{
		[Test]
		public void Format_Null()
		{
			Assert.AreEqual(Json.Format(null), "null");
		}

		[Test]
		public void Format_Boolean()
		{
			Assert.AreEqual(Json.Format(true), "true");
			Assert.AreEqual(Json.Format(false), "false");
		}

		[Test]
		public void Format_String()
		{
			Assert.AreEqual(Json.Format("Hello World"), "\"Hello World\"");
            Assert.AreEqual(Json.Format(" \" \\ / \b \f \n \r \t \0 \u1234"), "\" \\\" \\\\ \\/ \\b \\f \\n \\r \\t \\u0000 \u1234\"");
		}

		[Test]
		public void Format_Numbers()
		{
			Assert.AreEqual(Json.Format(123), "123");
			Assert.AreEqual(Json.Format(-123), "-123");
			Assert.AreEqual(Json.Format(123.0), "123");
			Assert.AreEqual(Json.Format(123.4), "123.4");
			Assert.AreEqual(Json.Format(-123.4), "-123.4");
			Assert.AreEqual(Json.Format(-123.45E-67), "-1.2345E-65");
			Assert.AreEqual(Json.Format(123U), "123");
			Assert.AreEqual(Json.Format(0xFF), "255");
			Assert.AreEqual(Json.Format(0xFFU), "255");
			Assert.AreEqual(Json.Format(0xFFFFFFFFFFFFFFFFL), "18446744073709551615");
		}

		[Test]
		public void Format_Empty_Array()
		{
			Assert.AreEqual(Json.Format(new int[] { }), "[]");
		}

		[Test]
		public void Format_Simple_Array()
		{
			Assert.AreEqual(Json.Format(new int[] { 1, 2, 3 }), "[\n\t1,\n\t2,\n\t3\n]");
		}


		[Test]
		public void Format_Empty_Dictionary()
		{
			Assert.AreEqual(Json.Format(new Dictionary<int, int>() { }), "{}");
		}

		[Test]
		public void Format_Simple_Dictionary()
		{
			var result = Json.Format(new Dictionary<string, int>() { {"Apples", 1}, {"Pears", 2} , {"Bananas", 3 } });
			Assert.AreEqual(result, "{\n\t\"Apples\": 1,\n\t\"Pears\": 2,\n\t\"Bananas\": 3\n}");
		}

		[Test]
		public void Format_Date()
		{
			Assert.AreEqual(Json.Format(new DateTime(2011, 1, 1, 0, 0, 0, DateTimeKind.Utc)), "1293840000000");
		}

		[Test]
		public void Format_Poco()
		{
			var result = Json.Format(new { Apples=1, Pears=2, Bananas=3});
			Assert.AreEqual(result, "{\n\t\"apples\": 1,\n\t\"pears\": 2,\n\t\"bananas\": 3\n}");
		}

		[Test]
		public void Parse_Null()
		{
			Assert.IsNull(Json.Parse<object>("null"));
		}

		[Test]
		public void Parse_Boolean()
		{
			Assert.IsTrue(Json.Parse<bool>("true"));
			Assert.IsFalse(Json.Parse<bool>("false"));
		}

		[Test]
		public void Parse_String()
		{
			var s = Json.Parse<string>("\"Hello\\r\\n\\t\\u0000 World\"");
			Assert.AreEqual((string)s, "Hello\r\n\t\0 World");
		}

		[Test]
		public void Parse_Numbers()
		{
			Assert.AreEqual(Json.Parse<int>("0"), 0);
			Assert.AreEqual(Json.Parse<int>("123"), 123);
			Assert.AreEqual(Json.Parse<double>("123.45"), 123.45);

			Assert.AreEqual(Json.Parse<double>("123e45"), 123e45);
			Assert.AreEqual(Json.Parse<double>("123.0e45"), 123.0e45);
			Assert.AreEqual(Json.Parse<double>("123e+45"), 123e45);
			Assert.AreEqual(Json.Parse<double>("123.0e+45"), 123.0e45);
			Assert.AreEqual(Json.Parse<double>("123e-45"), 123e-45);
			Assert.AreEqual(Json.Parse<double>("123.0e-45"), 123.0e-45);

			Assert.AreEqual(Json.Parse<double>("123E45"), 123E45);
			Assert.AreEqual(Json.Parse<double>("-123e45"), -123e45);
		}

		[Test]
		public void Parse_Empty_Array()
		{
			var d = Json.Parse<object[]>("[]");
			Assert.AllItemsAreEqual(d as object[], new object[] { });
        }

		[Test]
		public void Parse_simple_Array()
		{
			var d = Json.Parse<int[]>("[1,2,3]");
			Assert.AllItemsAreEqual(d, new int[] { 1, 2, 3} );
        }

		[Test]
		public void Parse_Date()
		{
			var d1 = new DateTime(2011, 1, 1, 10, 10, 10, DateTimeKind.Utc);
			var d2 = Json.Parse<DateTime>(Json.Format(d1));
			Assert.AreEqual(d1, d2);
		}

		[Test]
		public void DynamicTest()
		{
			var d = Json.Parse<IDictionary<string, object>>("{\"apples\":1, \"pears\":2, \"bananas\":3}") ;

			Assert.AreEquivalent(d.Keys, new string[] { "apples", "pears", "bananas" });
			Assert.AreEquivalent(d.Values, new object[] { 1UL, 2UL, 3UL });
		}

		[Test]
		public void Invalid_Numbers()
		{
			Assert.Throws<JsonParseException>(() => Json.Parse<object>("123ee0"));
            Assert.Throws<JsonParseException>(() => Json.Parse<object>("+123"));
            Assert.Throws<JsonParseException>(() => Json.Parse<object>("--123"));
            Assert.Throws<JsonParseException>(() => Json.Parse<object>("--123..0"));
            Assert.Throws<JsonParseException>(() => Json.Parse<object>("--123ex"));
            Assert.Throws<JsonParseException>(() => Json.Parse<object>("123x"));
            Assert.Throws<JsonParseException>(() => Json.Parse<object>("0x123"));
        }
		[Test]
		public void Invalid_Trailing_Characters()
		{
			Assert.Throws<JsonParseException>(()=> Json.Parse<object>("\"Hello\" , 123"));
		}

		[Test]
		public void Invalid_Identifier()
		{
			Assert.Throws<JsonParseException>(() => Json.Parse<object>("identifier"));
        }

		[Test]
		public void Invalid_Character()
		{
			Assert.Throws<JsonParseException>(() => Json.Parse<object>("~"));
        }

		[Test]
		public void Invalid_StringEscape()
		{
			Assert.Throws<JsonParseException>(() => Json.Parse<object>("\"\\q\""));
        }

        [Test]
        public void ErrorLocation()
        {
            var strJson="{\r\n \r\n \n\r \r \n \t   \"key:\": zzz";
            try
            {
                Json.Parse<object>(strJson);
            }
            catch (JsonParseException x)
            {
                Assert.AreEqual(x.Position.Line, 5);
                Assert.AreEqual(x.Position.Offset, 13);
            }
        }
    }
}
