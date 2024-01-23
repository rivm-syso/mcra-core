using MCRA.Utils.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.General.Test.UnitTests.UnitConversion {

    [TestClass]
    public class ExposureRouteConverterTests {

        [TestMethod]
        public void ExposureRouteConverter_TestCompleteness() {
            var definition = ExposureRouteConverter.UnitDefinition;
            var enumValues = Enum.GetValues(typeof(ExposureRoute)).Cast<ExposureRoute>().Where(r => r != ExposureRoute.Undefined);
            // Check whether there is a unit definition for each enum value.
            foreach (var value in enumValues) {
                var unitValueDefinition = definition.FromString<ExposureRoute>(value.ToString());
                Assert.AreEqual(unitValueDefinition, value);
            }
            // Check whether each unit specified in the definitions matches an enum value.
            foreach (var units in definition.Units) {
                var value = Enum.Parse(typeof(ExposureRoute), units.Id);
            }
        }

        /// <summary>
        /// Tests whether each of the specified aliases is correctly parsed.
        /// </summary>
        [TestMethod]
        public void ExposureRouteConverter_TestAliases() {
            var definition = ExposureRouteConverter.UnitDefinition;
            var aliases = definition.Units.SelectMany(r => r.Aliases, (r, a) => new {
                Unit = Enum.Parse(typeof(ExposureRoute), r.Id),
                Alias = a
            });
            // Check whether the parsed unit for each alias matches with the amount unit for which it is supposed to be an alias.
            foreach (var alias in aliases) {
                var parsedUnit = ExposureRouteConverter.FromString(alias.Alias);
                Assert.AreEqual(alias.Unit, parsedUnit);
            }
        }

        /// <summary>
        /// Tests whether there is a display name and a short display name for each unit.
        /// </summary>
        [TestMethod]
        public void ExposureRouteConverter_TestDisplayNames() {
            var definition = ExposureRouteConverter.UnitDefinition;
            foreach (var units in definition.Units) {
                var value = (ExposureRoute)Enum.Parse(typeof(ExposureRoute), units.Id);
                Assert.IsTrue(units.Name.Equals(value.GetDisplayName(), StringComparison.InvariantCultureIgnoreCase));
                Assert.IsTrue(units.ShortName.Equals(value.GetShortDisplayName(), StringComparison.InvariantCultureIgnoreCase));
            }
        }

        /// <summary>
        /// Tests the from string method for different strings.
        /// </summary>
        [TestMethod]
        [DataRow("AtTarget", ExposureRoute.Undefined)]
        [DataRow("attarget", ExposureRoute.Undefined)]
        [DataRow("Dietary", ExposureRoute.Oral)]
        [DataRow("dietary", ExposureRoute.Oral)]
        [DataRow("Oral", ExposureRoute.Oral)]
        [DataRow("", ExposureRoute.Undefined)]
        public void ExposureRouteConverter_TestFromString(string str, ExposureRoute expected) {
            var result = ExposureRouteConverter.FromString(str);
            Assert.AreEqual(expected, result);
        }
    }
}
