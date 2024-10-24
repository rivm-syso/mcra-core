using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MCRA.Utils.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests {

    public enum TestEnum {
        [Display(Name = "Undefined")]
        Undefined = -1,
        UnnamedType,
        [Display(Name = "Named")]
        NamedType
    }

    public sealed class MockRecord {
        [DisplayName("Percentage for upper tail")]
        [DisplayFormat(DataFormatString = @"{0}%")]
        public double Percentage { get; set; }

        public List<double> Percentiles { get; set; }

        public IEnumerable<int> Indices { get; set; }

        public string[] Names { get; set; }

        public List<TestEnum> Enums { get; set; }
    }

    [TestClass]
    public class ReflectionExtensionsTests {

        [TestMethod]
        public void ReflectionExtensions_TestFormat_Percentage() {
            var mock = new MockRecord() {
                Percentage = 33.5
            };
            var mockType = typeof(MockRecord);
            var property = mockType.GetPropertySingleOrDefault("Percentage");

            var formatted = mock.PrintPropertyValue(property);
            Assert.AreEqual("33.5%", formatted);

            formatted = mock.Percentage.PrintValue();
            Assert.AreEqual("33.5", formatted);
        }

        [TestMethod]
        public void ReflectionExtensions_TestFormat_Percentages() {
            var mock = new MockRecord() {
                Percentiles = [95, 97.5, 99.9]
            };
            var mockType = typeof(MockRecord);

            var property = mockType.GetPropertySingleOrDefault("Percentiles");
            var formatted = mock.PrintPropertyValue(property);
            Assert.AreEqual("95, 97.5, 99.9", formatted);

            formatted = mock.Percentiles.PrintValue();
            Assert.AreEqual("95, 97.5, 99.9", formatted);
        }

        [TestMethod]
        public void ReflectionExtensions_TestFormat_IntegerArray() {
            var mock = new MockRecord() {
                Indices = [1, 22, 333]
            };
            var mockType = typeof(MockRecord);

            var property = mockType.GetPropertySingleOrDefault("Indices");
            var formatted = mock.PrintPropertyValue(property);
            Assert.AreEqual("1, 22, 333", formatted);

            formatted = mock.Indices.PrintValue();
            Assert.AreEqual("1, 22, 333", formatted);
        }

        [TestMethod]
        public void ReflectionExtensions_TestFormat_StringArray() {
            var mock = new MockRecord() {
                Names = ["AB", "cde", "fGhI"]
            };
            var mockType = typeof(MockRecord);

            var property = mockType.GetPropertySingleOrDefault("Names");
            var formatted = mock.PrintPropertyValue(property);
            Assert.AreEqual("AB, cde, fGhI", formatted);
            formatted = mock.Names.PrintValue();
            Assert.AreEqual("AB, cde, fGhI", formatted);
        }

        [TestMethod]
        public void ReflectionExtensions_TestFormat_EnumArray() {
            var mock = new MockRecord() {
                Enums = [TestEnum.NamedType, TestEnum.NamedType, TestEnum.UnnamedType, TestEnum.Undefined]
            };
            var mockType = typeof(MockRecord);

            var property = mockType.GetPropertySingleOrDefault("Enums");
            var formatted = mock.PrintPropertyValue(property);
            Assert.AreEqual("Named, Named, UnnamedType, Undefined", formatted);
            formatted = mock.Enums.PrintValue();
            Assert.AreEqual("Named, Named, UnnamedType, Undefined", formatted);
        }

        [TestMethod]
        public void ReflectionExtensions_TestFormat_EmptyEnumArray() {
            var mock = new MockRecord() {
                Enums = []
            };
            var mockType = typeof(MockRecord);

            var property = mockType.GetPropertySingleOrDefault("Enums");
            var formatted = mock.PrintPropertyValue(property);
            Assert.AreEqual(string.Empty, formatted);
            formatted = mock.Enums.PrintValue();
            Assert.AreEqual(string.Empty, formatted);
        }
    }
}
