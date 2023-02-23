using MCRA.Utils.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.General.Test.UnitTests.UnitConversion {

    [TestClass]
    public class ConcentrationValueTypeConverterTests {

        [TestMethod]
        public void ConcentrationValueTypeConverterTests_TestCompleteness() {
            var definition = ConcentrationValueTypeConverter.UnitDefinition;
            var enumValues = Enum.GetValues(typeof(ConcentrationValueType))
                .Cast<ConcentrationValueType>()
                .Where(r => r != ConcentrationValueType.Undefined);
            // Check whether there is a unit definition for each enum value.
            foreach (var value in enumValues) {
                var unitValueDefinition = definition.FromString<ConcentrationValueType>(value.ToString());
                Assert.AreEqual(unitValueDefinition, value);
            }
            // Check whether each unit specified in the definitions matches an enum value.
            foreach (var units in definition.Units) {
                var value = Enum.Parse(typeof(ConcentrationValueType), units.Id);
            }
        }

        /// <summary>
        /// Tests whether each of the specified aliases is correctly parsed.
        /// </summary>
        [TestMethod]
        public void ConcentrationValueTypeConverterTests_TestAliases() {
            var definition = ConcentrationValueTypeConverter.UnitDefinition;
            var aliases = definition.Units.SelectMany(r => r.Aliases, (r, a) => new {
                Unit = Enum.Parse(typeof(ConcentrationValueType), r.Id),
                Alias = a
            });
            // Check whether the parsed unit for each alias matches with the amount unit for which it is supposed to be an alias.
            foreach (var alias in aliases) {
                var parsedUnit = ConcentrationValueTypeConverter.FromString(alias.Alias);
                Assert.AreEqual(alias.Unit, parsedUnit);
            }
        }

        /// <summary>
        /// Tests whether there is a display name and a short display name for each unit.
        /// </summary>
        [TestMethod]
        public void ConcentrationValueTypeConverterTests_TestDisplayNames() {
            var definition = ConcentrationValueTypeConverter.UnitDefinition;
            foreach (var units in definition.Units) {
                var value = (ConcentrationValueType)Enum.Parse(typeof(ConcentrationValueType), units.Id);
                Assert.AreEqual(units.Name, value.GetDisplayName(), true);
                Assert.AreEqual(units.ShortName, value.GetShortDisplayName(), true);
            }
        }
    }
}
