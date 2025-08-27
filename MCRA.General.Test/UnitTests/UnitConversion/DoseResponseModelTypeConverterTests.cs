using MCRA.Utils.ExtensionMethods;
using System.Globalization;

namespace MCRA.General.Test.UnitTests.UnitConversion {

    [TestClass]
    public class DoseResponseModelTypeConverterTests {

        [TestMethod]
        public void DoseResponseModelTypeConverterTests_TestCompleteness() {
            var definition = DoseResponseModelTypeConverter.UnitDefinition;
            var enumValues = Enum.GetValues(typeof(DoseResponseModelType))
                .Cast<DoseResponseModelType>()
                .Where(r => r != DoseResponseModelType.Unknown);
            // Check whether there is a unit definition for each enum value.
            foreach (var value in enumValues) {
                var unitValueDefinition = definition.FromString<DoseResponseModelType>(value.ToString());
                Assert.AreEqual(unitValueDefinition, value);
            }
            // Check whether each unit specified in the definitions matches an enum value.
            foreach (var units in definition.Units) {
                var value = Enum.Parse(typeof(DoseResponseModelType), units.Id);
            }
        }

        /// <summary>
        /// Tests whether each of the specified aliases is correctly parsed.
        /// </summary>
        [TestMethod]
        public void DoseResponseModelTypeConverterTests_TestAliases() {
            var definition = DoseResponseModelTypeConverter.UnitDefinition;
            var aliases = definition.Units.SelectMany(r => r.Aliases, (r, a) => new {
                Unit = Enum.Parse(typeof(DoseResponseModelType), r.Id),
                Alias = a
            });
            // Check whether the parsed unit for each alias matches with the amount unit for which it is supposed to be an alias.
            foreach (var alias in aliases) {
                var parsedUnit = DoseResponseModelTypeConverter.FromString(alias.Alias);
                Assert.AreEqual(alias.Unit, parsedUnit);
            }
        }

        /// <summary>
        /// Tests whether there is a display name and a short display name for each unit.
        /// </summary>
        [TestMethod]
        public void DoseResponseModelTypeConverterTests_TestDisplayNames() {
            var definition = DoseResponseModelTypeConverter.UnitDefinition;
            foreach (var units in definition.Units) {
                var value = (DoseResponseModelType)Enum.Parse(typeof(DoseResponseModelType), units.Id);
                Assert.AreEqual(units.Name, value.GetDisplayName(), true, CultureInfo.InvariantCulture);
                Assert.AreEqual(units.ShortName, value.GetShortDisplayName(), true, CultureInfo.InvariantCulture);
            }
        }
    }
}
