using MCRA.Utils.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace MCRA.General.Test.UnitTests.UnitConversion {

    [TestClass]
    public class BiologicalOrganisationTypeConverterTests {

        [TestMethod]
        public void BiologicalOrganisationTypeConverterTests_TestCompleteness() {
            var definition = BiologicalOrganisationTypeConverter.UnitDefinition;
            var enumValues = Enum.GetValues(typeof(BiologicalOrganisationType))
                .Cast<BiologicalOrganisationType>()
                .Where(r => r != BiologicalOrganisationType.Unspecified);
            // Check whether there is a unit definition for each enum value.
            foreach (var value in enumValues) {
                var unitValueDefinition = definition.FromString<BiologicalOrganisationType>(value.ToString());
                Assert.AreEqual(unitValueDefinition, value);
            }
            // Check whether each unit specified in the definitions matches an enum value.
            foreach (var units in definition.Units) {
                var value = Enum.Parse(typeof(BiologicalOrganisationType), units.Id);
            }
        }

        /// <summary>
        /// Tests whether each of the specified aliases is correctly parsed.
        /// </summary>
        [TestMethod]
        public void BiologicalOrganisationTypeConverterTests_TestAliases() {
            var definition = BiologicalOrganisationTypeConverter.UnitDefinition;
            var aliases = definition.Units.SelectMany(r => r.Aliases, (r, a) => new {
                Unit = Enum.Parse(typeof(BiologicalOrganisationType), r.Id),
                Alias = a
            });
            // Check whether the parsed unit for each alias matches with the amount unit for which it is supposed to be an alias.
            foreach (var alias in aliases) {
                var parsedUnit = BiologicalOrganisationTypeConverter.FromString(alias.Alias);
                Assert.AreEqual(alias.Unit, parsedUnit);
            }
        }

        /// <summary>
        /// Tests whether there is a display name and a short display name for each unit.
        /// </summary>
        [TestMethod]
        public void BiologicalOrganisationTypeConverterTests_TestDisplayNames() {
            var definition = BiologicalOrganisationTypeConverter.UnitDefinition;
            foreach (var units in definition.Units) {
                var value = (BiologicalOrganisationType)Enum.Parse(typeof(BiologicalOrganisationType), units.Id);
                Assert.IsTrue(units.Name.Equals(value.GetDisplayName(), StringComparison.InvariantCultureIgnoreCase));
                Assert.IsTrue(units.ShortName.Equals(value.GetShortDisplayName(), StringComparison.InvariantCultureIgnoreCase));
            }
        }
    }
}
