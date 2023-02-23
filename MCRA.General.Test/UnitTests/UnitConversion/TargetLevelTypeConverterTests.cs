using Microsoft.VisualStudio.TestTools.UnitTesting;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.General.Test.UnitTests.UnitConversion {

    [TestClass]
    public class TargetLevelTypeConverterTests {

        [TestMethod]
        public void TargetLevelTypeConverterTests_TestCompleteness() {
            var definition = TargetLevelTypeConverter.UnitDefinition;
            var enumValues = Enum.GetValues(typeof(TargetLevelType)).Cast<TargetLevelType>();
            // Check whether there is a unit definition for each enum value.
            foreach (var value in enumValues) {
                var unitValueDefinition = definition.FromString<TargetLevelType>(value.ToString());
                Assert.AreEqual(unitValueDefinition, value);
            }
            // Check whether each unit specified in the definitions matches an enum value.
            foreach (var units in definition.Units) {
                var value = Enum.Parse(typeof(TargetLevelType), units.Id);
            }
        }

        /// <summary>
        /// Tests whether each of the specified aliases is correctly parsed.
        /// </summary>
        [TestMethod]
        public void TargetLevelTypeConverterTests_TestAliases() {
            var definition = TargetLevelTypeConverter.UnitDefinition;
            var aliases = definition.Units.SelectMany(r => r.Aliases, (r, a) => new {
                Unit = Enum.Parse(typeof(TargetLevelType), r.Id),
                Alias = a
            });
            // Check whether the parsed unit for each alias matches with the amount unit for which it is supposed to be an alias.
            foreach (var alias in aliases) {
                var parsedUnit = TargetLevelTypeConverter.FromString(alias.Alias);
                Assert.AreEqual(alias.Unit, parsedUnit);
            }
        }

        /// <summary>
        /// Tests whether there is a display name and a short display name for each unit.
        /// </summary>
        [TestMethod]
        public void TargetLevelTypeConverterTests_TestDisplayNames() {
            var definition = TargetLevelTypeConverter.UnitDefinition;
            foreach (var units in definition.Units) {
                var value = (TargetLevelType)Enum.Parse(typeof(TargetLevelType), units.Id);
                Assert.IsTrue(units.Name.Equals(value.GetDisplayName(), StringComparison.InvariantCultureIgnoreCase));
                Assert.IsTrue(units.ShortName.Equals(value.GetShortDisplayName(), StringComparison.InvariantCultureIgnoreCase));
            }
        }
    }
}
