using Microsoft.VisualStudio.TestTools.UnitTesting;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.General.Test.UnitTests.UnitConversion {

    [TestClass]
    public class TestSystemTypeConverterTests {

        [TestMethod]
        public void TestSystemTypeConverterTests_TestCompleteness() {
            var definition = TestSystemTypeConverter.UnitDefinition;
            var enumValues = Enum.GetValues(typeof(TestSystemType)).Cast<TestSystemType>();
            // Check whether there is a unit definition for each enum value.
            foreach (var value in enumValues.Where(v => v != TestSystemType.Undefined)) {
                var unitValueDefinition = definition.FromString<TestSystemType>(value.ToString());
                Assert.AreEqual(unitValueDefinition, value);
            }
            // Check whether each unit specified in the definitions matches an enum value.
            foreach (var units in definition.Units) {
                var value = Enum.Parse(typeof(TestSystemType), units.Id);
            }
        }

        /// <summary>
        /// Tests whether each of the specified aliases is correctly parsed.
        /// </summary>
        [TestMethod]
        public void TestSystemTypeConverterTests_TestAliases() {
            var definition = TestSystemTypeConverter.UnitDefinition;
            var aliases = definition.Units.SelectMany(r => r.Aliases, (r, a) => new {
                Unit = Enum.Parse(typeof(TestSystemType), r.Id),
                Alias = a
            });
            // Check whether the parsed unit for each alias matches with the amount unit for which it is supposed to be an alias.
            foreach (var alias in aliases) {
                var parsedUnit = TestSystemTypeConverter.FromString(alias.Alias);
                Assert.AreEqual(alias.Unit, parsedUnit);
            }
        }

        /// <summary>
        /// Tests whether there is a display name and a short display name for each unit.
        /// </summary>
        [TestMethod]
        public void TestSystemTypeConverterTests_TestDisplayNames() {
            var definition = TestSystemTypeConverter.UnitDefinition;
            foreach (var units in definition.Units) {
                var value = (TestSystemType)Enum.Parse(typeof(TestSystemType), units.Id);
                Assert.IsTrue(units.Name.Equals(value.GetDisplayName(), StringComparison.InvariantCultureIgnoreCase));
                Assert.IsTrue(units.ShortName.Equals(value.GetShortDisplayName(), StringComparison.InvariantCultureIgnoreCase));
            }
        }
    }
}
