using MCRA.Utils.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.General.Test.UnitTests.UnitConversion {

    [TestClass]
    public class BenchmarkResponseTypeConverterTests {

        [TestMethod]
        public void BenchmarkResponseTypeConverterTests_TestCompleteness() {
            var definition = BenchmarkResponseTypeConverter.UnitDefinition;
            var enumValues = Enum.GetValues(typeof(BenchmarkResponseType))
                .Cast<BenchmarkResponseType>()
                .Where(r => r != BenchmarkResponseType.Undefined);
            // Check whether there is a unit definition for each enum value.
            foreach (var value in enumValues) {
                var unitValueDefinition = definition.FromString<BenchmarkResponseType>(value.ToString());
                Assert.AreEqual(unitValueDefinition, value);
            }
            // Check whether each unit specified in the definitions matches an enum value.
            foreach (var units in definition.Units) {
                var value = Enum.Parse(typeof(BenchmarkResponseType), units.Id);
            }
            var defaultValue = BenchmarkResponseTypeConverter.FromString("");
            Assert.AreEqual(BenchmarkResponseType.Factor, defaultValue);
        }

        /// <summary>
        /// Tests whether each of the specified aliases is correctly parsed.
        /// </summary>
        [TestMethod]
        public void BenchmarkResponseTypeConverterTests_TestAliases() {
            var definition = BenchmarkResponseTypeConverter.UnitDefinition;
            var aliases = definition.Units.SelectMany(r => r.Aliases, (r, a) => new {
                Unit = Enum.Parse(typeof(BenchmarkResponseType), r.Id),
                Alias = a
            });
            // Check whether the parsed unit for each alias matches with the amount unit for which it is supposed to be an alias.
            foreach (var alias in aliases) {
                var parsedUnit = BenchmarkResponseTypeConverter.FromString(alias.Alias);
                Assert.AreEqual(alias.Unit, parsedUnit);
            }
        }

        /// <summary>
        /// Tests whether there is a display name and a short display name for each unit.
        /// </summary>
        [TestMethod]
        public void BenchmarkResponseTypeConverterTests_TestDisplayNames() {
            var definition = BenchmarkResponseTypeConverter.UnitDefinition;
            foreach (var units in definition.Units) {
                var value = (BenchmarkResponseType)Enum.Parse(typeof(BenchmarkResponseType), units.Id);
                Assert.IsTrue(units.Name.Equals(value.GetDisplayName(), StringComparison.InvariantCultureIgnoreCase));
                Assert.IsTrue(units.ShortName.Equals(value.GetShortDisplayName(), StringComparison.InvariantCultureIgnoreCase));
            }
        }
    }
}
