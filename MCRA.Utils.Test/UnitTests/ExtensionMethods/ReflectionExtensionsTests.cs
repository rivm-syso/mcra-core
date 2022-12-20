using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MCRA.Utils.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests {

    public sealed class MockRecord {
        [DisplayName("Percentage for upper tail")]
        [DisplayFormat(DataFormatString = @"{0}%")]
        public double Percentage { get; set; }
    }

    [TestClass]
    public class ReflectionExtensionsTests {

        [TestMethod]
        public void TestFormat() {
            var mock = new MockRecord() {
                Percentage = 33.5,
            };
            var mockType= typeof(MockRecord);
            var property = mockType.GetPropertySingleOrDefault("Percentage");
            var formatted = mock.PrintPropertyValue(property);
            Assert.AreEqual("33.5%", formatted);
        }
    }
}
