namespace MCRA.General.Test.UnitTests.UnitConversion {

    [TestClass]
    public class ConsumptionUnitTests {

        /// <summary>
        /// Tests whether there is a value definition for each enum value of the amount units enum type.
        /// </summary>
        [TestMethod]
        public void ConsumptionUnitConverter_TestCompleteness() {
            var definition = ConsumptionUnitConverter.UnitDefinition;
            var enumValues = Enum.GetValues(typeof(ConsumptionUnit)).Cast<ConsumptionUnit>();
            // Check whether there is a unit definition for each enum value.
            foreach (var value in enumValues) {
                var unitValueDefinition = definition.FromString<ConsumptionUnit>(value.ToString());
                Assert.AreEqual(unitValueDefinition, value);
            }
            // Check whether each unit specified in the definitions matches an enum value.
            foreach (var units in definition.Units) {
                var value = Enum.Parse(typeof(ConsumptionUnit), units.Id);
            }
        }

        /// <summary>
        /// Tests whether each of the specified aliases is correctly parsed.
        /// </summary>
        [TestMethod]
        public void ConsumptionUnitConverter_TestAliases() {
            var definition = ConsumptionUnitConverter.UnitDefinition;
            var aliases = definition.Units.SelectMany(r => r.Aliases, (r, a) => new {
                ConsumptionUnit = Enum.Parse(typeof(ConsumptionUnit), r.Id),
                Alias = a
            });
            // Check whether the parsed unit for each alias matches with the amount unit for which it is supposed to be an alias.
            foreach (var alias in aliases) {
                var parsedUnit = ConsumptionUnitConverter.FromString(alias.Alias);
                Assert.AreEqual(alias.ConsumptionUnit, parsedUnit);
            }
        }
    }
}
