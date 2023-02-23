using MCRA.Utils.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.General.Test.UnitTests.UnitConversion {

    [TestClass]
    public class InternalModelTypeConverterTests {

        [TestMethod]
        public void InternalModelTypeConverterTests_TestCompleteness() {
            var definition = InternalModelTypeConverter.UnitDefinition;
            var enumValues = Enum.GetValues(typeof(InternalModelType)).Cast<InternalModelType>();
            // Check whether there is a unit definition for each enum value.
            foreach (var value in enumValues) {
                var unitValueDefinition = definition.FromString<InternalModelType>(value.ToString());
                Assert.AreEqual(unitValueDefinition, value);
            }
            // Check whether each unit specified in the definitions matches an enum value.
            foreach (var units in definition.Units) {
                var value = Enum.Parse(typeof(InternalModelType), units.Id);
            }
        }

        /// <summary>
        /// Tests whether each of the specified aliases is correctly parsed.
        /// </summary>
        [TestMethod]
        public void InternalModelTypeConverterTests_TestAliases() {
            var definition = InternalModelTypeConverter.UnitDefinition;
            var aliases = definition.Units.SelectMany(r => r.Aliases, (r, a) => new {
                Unit = Enum.Parse(typeof(InternalModelType), r.Id),
                Alias = a
            });
            // Check whether the parsed unit for each alias matches with the amount unit for which it is supposed to be an alias.
            foreach (var alias in aliases) {
                var parsedUnit = InternalModelTypeConverter.FromString(alias.Alias);
                Assert.AreEqual(alias.Unit, parsedUnit);
            }
        }

        /// <summary>
        /// Tests whether there is a display name and a short display name for each unit.
        /// </summary>
        [TestMethod]
        public void InternalModelTypeConverterTests_TestDisplayNames() {
            var definition = InternalModelTypeConverter.UnitDefinition;
            foreach (var units in definition.Units) {
                var value = (InternalModelType)Enum.Parse(typeof(InternalModelType), units.Id);
                Assert.IsTrue(units.Name.Equals(value.GetDisplayName(), StringComparison.InvariantCultureIgnoreCase));
                Assert.IsTrue(units.ShortName.Equals(value.GetShortDisplayName(), StringComparison.InvariantCultureIgnoreCase));
            }
        }
    }
}
