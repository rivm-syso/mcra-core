using MCRA.Utils.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace MCRA.General.Test.UnitTests.UnitConversion {

    [TestClass]
    public class ConcentrationLimitValueTypeConverterTests {

        [TestMethod]
        public void ConcentrationLimitValueTypeConverterTests_TestCompleteness() {
            var definition = ConcentrationLimitValueTypeConverter.UnitDefinition;
            var enumValues = Enum.GetValues(typeof(ConcentrationLimitValueType))
                .Cast<ConcentrationLimitValueType>()
                .Where(r => r != ConcentrationLimitValueType.Undefined);
            // Check whether there is a unit definition for each enum value.
            foreach (var value in enumValues) {
                var unitValueDefinition = definition.FromString<ConcentrationLimitValueType>(value.ToString());
                Assert.AreEqual(unitValueDefinition, value);
            }
            // Check whether each unit specified in the definitions matches an enum value.
            foreach (var units in definition.Units) {
                var value = Enum.Parse(typeof(ConcentrationLimitValueType), units.Id);
            }
        }

        /// <summary>
        /// Tests whether each of the specified aliases is correctly parsed.
        /// </summary>
        [TestMethod]
        public void ConcentrationLimitValueTypeConverterTests_TestAliases() {
            var definition = ConcentrationLimitValueTypeConverter.UnitDefinition;
            var aliases = definition.Units.SelectMany(r => r.Aliases, (r, a) => new {
                Unit = Enum.Parse(typeof(ConcentrationLimitValueType), r.Id),
                Alias = a
            });
            // Check whether the parsed unit for each alias matches with the amount unit for which it is supposed to be an alias.
            foreach (var alias in aliases) {
                var parsedUnit = ConcentrationLimitValueTypeConverter.FromString(alias.Alias);
                Assert.AreEqual(alias.Unit, parsedUnit);
            }
        }

        /// <summary>
        /// Tests whether there is a display name and a short display name for each unit.
        /// </summary>
        [TestMethod]
        public void ConcentrationLimitValueTypeConverterTests_TestDisplayNames() {
            var definition = ConcentrationLimitValueTypeConverter.UnitDefinition;
            foreach (var units in definition.Units) {
                var value = (ConcentrationLimitValueType)Enum.Parse(typeof(ConcentrationLimitValueType), units.Id);
                Assert.IsTrue(units.Name.Equals(value.GetDisplayName(), StringComparison.InvariantCultureIgnoreCase));
                Assert.IsTrue(units.ShortName.Equals(value.GetShortDisplayName(), StringComparison.InvariantCultureIgnoreCase));
            }
        }
    }
}
