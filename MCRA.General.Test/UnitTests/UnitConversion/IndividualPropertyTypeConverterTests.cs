using System;
using System.Linq;
using MCRA.Utils.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.General.Test.UnitTests.UnitConversion {

    [TestClass]
    public class IndividualPropertyTypeConverterTests {

        [TestMethod]
        public void IndividualPropertyTypeConverterTests_TestCompleteness() {
            var definition = IndividualPropertyTypeConverter.UnitDefinition;
            var enumValues = Enum.GetValues(typeof(IndividualPropertyType)).Cast<IndividualPropertyType>();
            // Check whether there is a unit definition for each enum value.
            foreach (var value in enumValues) {
                var unitValueDefinition = definition.FromString<IndividualPropertyType>(value.ToString());
                Assert.AreEqual(unitValueDefinition, value);
            }
            // Check whether each unit specified in the definitions matches an enum value.
            foreach (var units in definition.Units) {
                var value = Enum.Parse(typeof(IndividualPropertyType), units.Id);
            }
        }

        /// <summary>
        /// Tests whether each of the specified aliases is correctly parsed.
        /// </summary>
        [TestMethod]
        public void IndividualPropertyTypeConverterTests_TestAliases() {
            var definition = IndividualPropertyTypeConverter.UnitDefinition;
            var aliases = definition.Units.SelectMany(r => r.Aliases, (r, a) => new {
                Unit = Enum.Parse(typeof(IndividualPropertyType), r.Id),
                Alias = a
            });
            // Check whether the parsed unit for each alias matches with the amount unit for which it is supposed to be an alias.
            foreach (var alias in aliases) {
                var parsedUnit = IndividualPropertyTypeConverter.FromString(alias.Alias);
                Assert.AreEqual(alias.Unit, parsedUnit);
            }
        }

        /// <summary>
        /// Tests whether there is a display name and a short display name for each unit.
        /// </summary>
        [TestMethod]
        public void IndividualPropertyTypeConverterTests_TestDisplayNames() {
            var definition = IndividualPropertyTypeConverter.UnitDefinition;
            foreach (var units in definition.Units) {
                var value = (IndividualPropertyType)Enum.Parse(typeof(IndividualPropertyType), units.Id);
                Assert.IsTrue(units.Name.Equals(value.GetDisplayName(), StringComparison.InvariantCultureIgnoreCase));
                Assert.IsTrue(units.ShortName.Equals(value.GetShortDisplayName(), StringComparison.InvariantCultureIgnoreCase));
            }
        }
    }
}
