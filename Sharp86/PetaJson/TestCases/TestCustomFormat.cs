using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PetaTest;
using PetaJson;
using System.IO;
using System.Globalization;
using System.Reflection;

namespace TestCases
{
    struct PointSimple
    {
        public int X;
        public int Y;

        private string FormatJson()
        {
            return string.Format("{0},{1}", X, Y);
        }

        private static PointSimple ParseJson(string literal)
        {
            var parts = literal.Split(',');
            if (parts.Length == 2)
            {
                return new PointSimple()
                {
                    X = int.Parse(parts[0], CultureInfo.InvariantCulture),
                    Y = int.Parse(parts[0], CultureInfo.InvariantCulture),
                };
            }
            throw new InvalidDataException("Invalid point");
        }
    }
    struct PointComplex
    {
        public int X;
        public int Y;

        private void FormatJson(IJsonWriter writer)
        {
            writer.WriteStringLiteral(string.Format("{0},{1}", X, Y));
        }

        private static PointComplex ParseJson(IJsonReader r)
        {
            if (r.GetLiteralKind() == LiteralKind.String)
            {
                var parts = ((string)r.GetLiteralString()).Split(',');
                if (parts.Length == 2)
                {
                    var pt = new PointComplex()
                    {
                        X = int.Parse(parts[0], CultureInfo.InvariantCulture),
                        Y = int.Parse(parts[0], CultureInfo.InvariantCulture),
                    };
                    r.NextToken();
                    return pt;
                }
            }
            throw new InvalidDataException("Invalid point");
        }

    }

    [TestFixture]
    [Obfuscation(Exclude = true, ApplyToMembers = true)]
    public class TestCustomFormat
    {
        [Test]
        public void TestSimple()
        {
            var p = new PointSimple() { X = 10, Y = 20 };

            var json = Json.Format(p);

            Assert.AreEqual(json, "\"10,20\"");

            var p2 = Json.Parse<PointSimple>(json);

            Assert.Equals(p.X, p2.X);
            Assert.Equals(p.Y, p2.Y);
        }

        [Test]
        public void TestSimpleExceptionPassed()
        {
            Assert.Throws<JsonParseException>(() => Json.Parse<PointSimple>("\"10,20,30\""));
        }

        [Test]
        public void TestComplex()
        {
            var p = new PointComplex() { X = 10, Y = 20 };

            var json = Json.Format(p);

            Assert.AreEqual(json, "\"10,20\"");

            var p2 = Json.Parse<PointComplex>(json);

            Assert.Equals(p.X, p2.X);
            Assert.Equals(p.Y, p2.Y);
        }

        [Test]
        public void TestComplexExceptionPassed()
        {
            Assert.Throws<JsonParseException>(() => Json.Parse<PointComplex>("\"10,20,30\""));
        }
    }
}
