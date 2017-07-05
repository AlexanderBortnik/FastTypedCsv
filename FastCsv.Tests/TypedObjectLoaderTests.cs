using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;

namespace FastCsv.Tests
{
    [TestFixture]
    class TypedObjectLoaderTests
    {
        private class TestData
        {
            public int A { get; set; }
            public string B { get; set; }
            public DateTime? C { get; set; }
            public long? D { get; set; }
            public double? E { get; set; }
            public DateTime F { get; set; }

            protected bool Equals(TestData other)
            {
                return A == other.A && string.Equals(B, other.B) && C.Equals(other.C) && D == other.D && E.Equals(other.E) && F.Equals(other.F);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((TestData) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = A;
                    hashCode = (hashCode*397) ^ (B != null ? B.GetHashCode() : 0);
                    hashCode = (hashCode*397) ^ C.GetHashCode();
                    hashCode = (hashCode*397) ^ D.GetHashCode();
                    hashCode = (hashCode*397) ^ E.GetHashCode();
                    hashCode = (hashCode*397) ^ F.GetHashCode();
                    return hashCode;
                }
            }
        }

        [Test]
        public void PropertiesDeserializationTest()
        {
            var loader = TypedObjectLoader.Create<TestData>();
            var loadedObject = loader(new Dictionary<string, string>
            {
                {"A", "22" },
                {"B", "This is it." },
                {"C", "2017-05-09 12:13:45" },
                {"D", "666" },
                {"E", "124.55555" },
                { "F",  "2017-04-20 22:33:44"}
            });

            Assert.That(loadedObject, Is.EqualTo(new TestData
            {
                A = 22,
                B = "This is it.",
                C = DateTime.Parse("2017-05-09 12:13:45", CultureInfo.InvariantCulture),
                D = 666,
                E = 124.55555,
                F = DateTime.Parse("2017-04-20 22:33:44", CultureInfo.InvariantCulture)
            }));
        }

        [Test]
        public void EmptyPropertiesDeserializationTest()
        {
            var loader = TypedObjectLoader.Create<TestData>();

            var loadedObject = loader(new Dictionary<string, string>
            {
                {"A", "" },
                {"B", "" },
                {"C", "" },
                {"D", "" },
                {"E", "" },
                {"F", "" }
            });

            Assert.That(loadedObject, Is.EqualTo(new TestData
            {
                A = 0,
                B = "",
                C = null,
                D = null,
                E = null,
                F = new DateTime()
            }));
        }
    }
}
