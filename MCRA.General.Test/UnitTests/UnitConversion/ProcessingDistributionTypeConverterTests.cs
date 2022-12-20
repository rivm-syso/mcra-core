using System;
using System.Linq;
using MCRA.Utils.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.General.Test.UnitTests.UnitConversion {

    [TestClass]
    public class ProcessingDistributionTypeConverterTests {

        [TestMethod]
        public void ProcessingDistributionTypeConverterTests_TestCompleteness() {
            var definition = ProcessingDistributionTypeConverter.UnitDefinition;
            var enumValues = Enum.GetValues(typeof(ProcessingDistributionType)).Cast<ProcessingDistributionType>();
            // Check whether there is a unit definition for each enum value.
            foreach (var value in enumValues) {
                var unitValueDefinition = definition.FromString<ProcessingDistributionType>(value.ToString());
                Assert.AreEqual(unitValueDefinition, value);
            }
            // Check whether each unit specified in the definitions matches an enum value.
            foreach (var units in definition.Units) {
                var value = Enum.Parse(typeof(ProcessingDistributionType), units.Id);
            }
        }

        /// <summary>
        /// Tests whether each of the specified aliases is correctly parsed.
        /// </summary>
        [TestMethod]
        public void ProcessingDistributionTypeConverterTests_TestAliases() {
            var definition = ProcessingDistributionTypeConverter.UnitDefinition;
            var aliases = definition.Units.SelectMany(r => r.Aliases, (r, a) => new {
                Unit = Enum.Parse(typeof(ProcessingDistributionType), r.Id),
                Alias = a
            });
            // Check whether the parsed unit for each alias matches with the amount unit for which it is supposed to be an alias.
            foreach (var alias in aliases) {
                var parsedUnit = ProcessingDistributionTypeConverter.FromString(alias.Alias, ProcessingDistributionType.LogisticNormal);
                Assert.AreEqual(alias.Unit, parsedUnit);
            }

            // Check default fallback
            var defaultType = ProcessingDistributionTypeConverter.FromString(string.Empty, ProcessingDistributionType.LogisticNormal);
            Assert.AreEqual(ProcessingDistributionType.LogisticNormal, defaultType);
        }

        /// <summary>
        /// Tests whether there is a display name and a short display name for each unit.
        /// </summary>
        [TestMethod]
        public void ProcessingDistributionTypeConverterTests_TestDisplayNames() {
            var definition = ProcessingDistributionTypeConverter.UnitDefinition;
            foreach (var units in definition.Units) {
                var value = (ProcessingDistributionType)Enum.Parse(typeof(ProcessingDistributionType), units.Id);
                Assert.IsTrue(units.Name.Equals(value.GetDisplayName(), StringComparison.InvariantCultureIgnoreCase));
                Assert.IsTrue(units.ShortName.Equals(value.GetShortDisplayName(), StringComparison.InvariantCultureIgnoreCase));
            }
        }
    }
}
