using MCRA.Utils.TestReporting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Xml {

    [TestClass]
    public class XmlSerializationTests {

        [TestMethod]
        public void ToXmlTest1() {
            var p = createFakePerson();
            var s = p.ToXml();
            Assert.IsFalse(string.IsNullOrEmpty(s));
            var p2 = XmlSerialization.FromXml<Person>(s);
            Assert.AreEqual(p, p2);
        }

        [TestMethod]
        public void ToAndFromCompressedXmlTest1() {
            var p = createFakePerson();
            var s = p.ToCompressedXml();
            Assert.IsNotNull(s);
            Assert.IsTrue(s.Length > 0);
            var p2 = XmlSerialization.FromCompressedXml<Person>(s);
            Assert.AreEqual(p, p2);
        }

        private static Person createFakePerson() {
            return new Person {
                BirthDate = new DateTime(1970, 1, 1),
                Height = 1.95,
                IsFemale = false,
                Name = TestUtils.GetRandomString(8),
                Number = 13980,
                Partner = new Person {
                    BirthDate = new DateTime(1974, 1, 1),
                    Height = 1.6,
                    IsFemale = true,
                    Name = TestUtils.GetRandomString(8),
                    Number = 18098
                },
                Children = new[] {
                    new Person {
                        BirthDate = new DateTime(2001, 1, 1),
                        Height = 1.65,
                        IsFemale = true,
                        Name = TestUtils.GetRandomString(8),
                        Number = 3541654 },
                    new Person {
                        BirthDate = new DateTime(2003, 1, 1),
                        Height = 1.81,
                        IsFemale = false,
                        Name = TestUtils.GetRandomString(8),
                        Number = 5165165 }
                }
            };
        }
    }

    /// <summary>
    /// Test class for xml serialization
    /// </summary>
    public class Person {
        public int Number { get; set; }
        public string Name { get; set; }
        public bool IsFemale { get; set; }
        public double Height { get; set; }
        public DateTime BirthDate { get; set; }
        public Person Partner { get; set; }
        public Person[] Children { get; set; }

        public override bool Equals(object obj) {
            var p = obj as Person;
            if (p != null) {
                return
                    Number.Equals(p.Number) &&
                    Name.Equals(p.Name, StringComparison.InvariantCultureIgnoreCase) &&
                    IsFemale.Equals(p.IsFemale) &&
                    Height.Equals(p.Height) &&
                    BirthDate.Equals(p.BirthDate) &&
                    (Partner == null && p.Partner == null) ||
                    (Partner != null && p.Partner != null &&
                        Partner.Equals(p.Partner)) &&
                    (Children == null && p.Children == null) ||
                    (Children != null && p.Children != null &&
                        Children.Length == p.Children.Length &&
                        Children.Select((c, i)=>c.Equals(p.Children[i]))
                        .All(t => t)
                    );
            }
            return false;
        }

        public override int GetHashCode() => base.GetHashCode();
    }
}
