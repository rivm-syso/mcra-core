using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MCRA.Utils.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests {

    public sealed class MockRecord {
        [DisplayName("Percentage for upper tail")]
        [DisplayFormat(DataFormatString = @"{0}%")]
        public double Percentage { get; set; }

        public List<double> Percentiles { get; set; }

        public IEnumerable<int> Indices { get; set; }

        public string[] Names { get; set; }
    }

    [TestClass]
    public class ReflectionExtensionsTests {

        [TestMethod]
        public void TestFormat() {
            var mock = new MockRecord() {
                Percentage = 33.5,
                Percentiles = [95, 97.5, 99.9],
                Indices = [1, 22, 333],
                Names = ["AB", "cde", "fGhI"]
            };
            var mockType = typeof(MockRecord);
            var property = mockType.GetPropertySingleOrDefault("Percentage");
            var formatted = mock.PrintPropertyValue(property);
            Assert.AreEqual("33.5%", formatted);
            formatted = mock.Percentage.PrintValue();
            Assert.AreEqual("33.5", formatted);

            property = mockType.GetPropertySingleOrDefault("Percentiles");
            formatted = mock.PrintPropertyValue(property);
            Assert.AreEqual("95, 97.5, 99.9", formatted);
            formatted = mock.Percentiles.PrintValue();
            Assert.AreEqual("95, 97.5, 99.9", formatted);


            property = mockType.GetPropertySingleOrDefault("Indices");
            formatted = mock.PrintPropertyValue(property);
            Assert.AreEqual("1, 22, 333", formatted);
            formatted = mock.Indices.PrintValue();
            Assert.AreEqual("1, 22, 333", formatted);

            property = mockType.GetPropertySingleOrDefault("Names");
            formatted = mock.PrintPropertyValue(property);
            Assert.AreEqual("AB, cde, fGhI", formatted);
            formatted = mock.Names.PrintValue();
            Assert.AreEqual("AB, cde, fGhI", formatted);
        }

    }
}
