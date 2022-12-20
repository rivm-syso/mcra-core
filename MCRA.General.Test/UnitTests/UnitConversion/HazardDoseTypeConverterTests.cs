using MCRA.Utils.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace MCRA.General.Test.UnitTests.UnitConversion {

    [TestClass]
    public class HazardDoseTypeConverterTests {

        [TestMethod]
        public void HazardDoseTypeConverterTests_TestCompleteness() {
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
        public void HazardDoseTypeConverterTests_TestAliases() {
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
        public void HazardDoseTypeConverterTests_TestDisplayNames() {
            var definition = PointOfDepartureTypeConverter.UnitDefinition;
            foreach (var units in definition.Units) {
                var value = (PointOfDepartureType)Enum.Parse(typeof(PointOfDepartureType), units.Id);
                Assert.IsTrue(units.Name.Equals(value.GetDisplayName(), StringComparison.InvariantCultureIgnoreCase));
                Assert.IsTrue(units.ShortName.Equals(value.GetShortDisplayName(), StringComparison.InvariantCultureIgnoreCase));
            }
        }

        /// <summary>
        /// Tests conversion of a hazard dose type to its similar potency origin type
        /// counterpart. Checks whether there is a potency origin known for all hazard
        /// dose types.
        /// </summary>
        [TestMethod]
        public void HazardDoseTypeConverterTests_TestToPotencyOrigin() {
            var enumValues = Enum.GetValues(typeof(PointOfDepartureType))
                .Cast<PointOfDepartureType>()
                .Where(r => r != PointOfDepartureType.Unspecified);
            // Check whether there is a potency origin known for all hazard dose types.
            foreach (var value in enumValues) {
                var potencyOrigin = value.ToPotencyOrigin();
                Assert.AreNotEqual(PotencyOrigin.Unknown, potencyOrigin);
            }
        }
    }
}
