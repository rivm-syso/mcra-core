using MCRA.Utils.ExtensionMethods;

namespace MCRA.General.Test.UnitTests.UnitConversion {

    [TestClass]
    public class PointOfDepartureTypeConverterTests {

        [TestMethod]
        public void PointOfDepartureTypeConverterTests_TestCompleteness() {
            var definition = PointOfDepartureTypeConverter.UnitDefinition;
            var enumValues = Enum.GetValues(typeof(PointOfDepartureType))
                .Cast<PointOfDepartureType>()
                .Where(r => r != PointOfDepartureType.Unspecified);
            // Check whether there is a unit definition for each enum value.
            foreach (var value in enumValues) {
                var unitValueDefinition = definition.FromString<PointOfDepartureType>(value.ToString());
                Assert.AreEqual(unitValueDefinition, value);
            }
            // Check whether each unit specified in the definitions matches an enum value.
            foreach (var units in definition.Units) {
                var value = Enum.Parse(typeof(PointOfDepartureType), units.Id);
            }
        }

        /// <summary>
        /// Tests whether each of the specified aliases is correctly parsed.
        /// </summary>
        [TestMethod]
        public void PointOfDepartureTypeConverterTests_TestAliases() {
            var definition = PointOfDepartureTypeConverter.UnitDefinition;
            var aliases = definition.Units.SelectMany(r => r.Aliases, (r, a) => new {
                Unit = Enum.Parse(typeof(PointOfDepartureType), r.Id),
                Alias = a
            });
            // Check whether the parsed unit for each alias matches with the amount unit for which it is supposed to be an alias.
            foreach (var alias in aliases) {
                var parsedUnit = PointOfDepartureTypeConverter.FromString(alias.Alias);
                Assert.AreEqual(alias.Unit, parsedUnit);
            }
        }

        /// <summary>
        /// Tests whether there is a display name and a short display name for each unit.
        /// </summary>
        [TestMethod]
        public void PointOfDepartureTypeConverterTests_TestDisplayNames() {
            var definition = PointOfDepartureTypeConverter.UnitDefinition;
            foreach (var units in definition.Units) {
                var value = (PointOfDepartureType)Enum.Parse(typeof(PointOfDepartureType), units.Id);
                Assert.IsTrue(units.Name.Equals(value.GetDisplayName(), StringComparison.InvariantCultureIgnoreCase));
                Assert.IsTrue(units.ShortName.Equals(value.GetShortDisplayName(), StringComparison.InvariantCultureIgnoreCase));
            }
        }
    }
}
