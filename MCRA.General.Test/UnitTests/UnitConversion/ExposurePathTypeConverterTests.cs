using MCRA.Utils.ExtensionMethods;

namespace MCRA.General.Test.UnitTests.UnitConversion {

    [TestClass]
    public class ExposurePathTypeConverterTests {

        [TestMethod]
        public void ExposurePathTypeConverter_TestCompleteness() {
            var definition = ExposurePathTypeConverter.UnitDefinition;
            var enumValues = Enum.GetValues(typeof(ExposurePathType)).Cast<ExposurePathType>().Where(r => r != ExposurePathType.Undefined);
            // Check whether there is a unit definition for each enum value.
            foreach (var value in enumValues) {
                var unitValueDefinition = definition.FromString<ExposurePathType>(value.ToString());
                Assert.AreEqual(unitValueDefinition, value);
            }
            // Check whether each unit specified in the definitions matches an enum value.
            foreach (var units in definition.Units) {
                var value = Enum.Parse(typeof(ExposurePathType), units.Id);
            }
        }

        /// <summary>
        /// Tests whether each of the specified aliases is correctly parsed.
        /// </summary>
        [TestMethod]
        public void ExposurePathTypeConverter_TestAliases() {
            var definition = ExposurePathTypeConverter.UnitDefinition;
            var aliases = definition.Units.SelectMany(r => r.Aliases, (r, a) => new {
                Unit = Enum.Parse(typeof(ExposurePathType), r.Id),
                Alias = a
            });
            // Check whether the parsed unit for each alias matches with the amount unit for which it is supposed to be an alias.
            foreach (var alias in aliases) {
                var parsedUnit = ExposurePathTypeConverter.FromString(alias.Alias);
                Assert.AreEqual(alias.Unit, parsedUnit);
            }
        }

        /// <summary>
        /// Tests whether there is a display name and a short display name for each unit.
        /// </summary>
        [TestMethod]
        public void ExposurePathTypeConverter_TestDisplayNames() {
            var definition = ExposurePathTypeConverter.UnitDefinition;
            foreach (var units in definition.Units) {
                var value = (ExposurePathType)Enum.Parse(typeof(ExposurePathType), units.Id);
                Assert.IsTrue(units.Name.Equals(value.GetDisplayName(), StringComparison.InvariantCultureIgnoreCase));
                Assert.IsTrue(units.ShortName.Equals(value.GetShortDisplayName(), StringComparison.InvariantCultureIgnoreCase));
            }
        }
    }
}
