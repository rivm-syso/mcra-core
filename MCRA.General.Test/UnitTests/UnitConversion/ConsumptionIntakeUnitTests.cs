namespace MCRA.General.Test.UnitTests.UnitConversion {

    [TestClass]
    public class ConsumptionIntakeUnitTests {

        /// <summary>
        /// Tests whether there is a value definition for each enum value of the amount units enum type.
        /// </summary>
        [TestMethod]
        public void ConsumptionIntakeUnitConverter_TestCompleteness() {
            var definition = ConsumptionIntakeUnitConverter.UnitDefinition;
            var enumValues = Enum.GetValues(typeof(ConsumptionIntakeUnit)).Cast<ConsumptionIntakeUnit>();
            // Check whether there is a unit definition for each enum value.
            foreach (var value in enumValues) {
                var unitValueDefinition = definition.FromString<ConsumptionIntakeUnit>(value.ToString());
                Assert.AreEqual(unitValueDefinition, value);
            }
            // Check whether each unit specified in the definitions matches an enum value.
            foreach (var units in definition.Units) {
                var value = Enum.Parse(typeof(ConsumptionIntakeUnit), units.Id);
            }
        }

        /// <summary>
        /// Tests whether each of the specified aliases is correctly parsed.
        /// </summary>
        [TestMethod]
        public void ConsumptionIntakeUnitConverter_TestAliases() {
            var definition = ConsumptionIntakeUnitConverter.UnitDefinition;
            var aliases = definition.Units.SelectMany(r => r.Aliases, (r, a) => new {
                ConsumptionIntakeUnit = Enum.Parse(typeof(ConsumptionIntakeUnit), r.Id),
                Alias = a
            });
            // Check whether the parsed unit for each alias matches with the amount unit for which it is supposed to be an alias.
            foreach (var alias in aliases) {
                var parsedUnit = ConsumptionIntakeUnitConverter.FromString(alias.Alias);
                Assert.AreEqual(alias.ConsumptionIntakeUnit, parsedUnit);
            }
        }

        /// <summary>
        /// Tests whether the is-per-person method is available for all consumption intake units.
        /// </summary>
        [TestMethod]
        public void ConsumptionIntakeUnitConverter_TestIsPerPerson() {
            var enumValues = Enum.GetValues(typeof(ConsumptionIntakeUnit)).Cast<ConsumptionIntakeUnit>();
            foreach (var value in enumValues) {
                var isPerPerson = value.IsPerPerson();
            }
            Assert.IsTrue(ConsumptionIntakeUnit.gPerDay.IsPerPerson());
            Assert.IsFalse(ConsumptionIntakeUnit.gPerKgBWPerDay.IsPerPerson());
        }

        /// <summary>
        /// Tests whether the get consumption (amount) unit method is available for all consumption intake units.
        /// </summary>
        [TestMethod]
        public void ConsumptionIntakeUnitConverter_TestGetConsumptionUnit() {
            var enumValues = Enum.GetValues(typeof(ConsumptionIntakeUnit)).Cast<ConsumptionIntakeUnit>();
            foreach (var value in enumValues) {
                var consumptionUnit = value.GetConsumptionUnit();
            }
            Assert.AreEqual(ConsumptionUnit.g, ConsumptionIntakeUnit.gPerDay.GetConsumptionUnit());
            Assert.AreEqual(ConsumptionUnit.g, ConsumptionIntakeUnit.gPerKgBWPerDay.GetConsumptionUnit());
        }

        /// <summary>
        /// Tests whether the get consumption (amount) unit method is available for all consumption intake units.
        /// </summary>
        [TestMethod]
        public void ConsumptionIntakeUnitConverter_TestGetBodyWeightUnit() {
            var enumValues = Enum.GetValues(typeof(ConsumptionIntakeUnit)).Cast<ConsumptionIntakeUnit>();
            foreach (var value in enumValues) {
                var consumptionUnit = value.GetBodyWeightUnit();
            }
            Assert.AreEqual(BodyWeightUnit.kg, ConsumptionIntakeUnit.gPerDay.GetBodyWeightUnit());
            Assert.AreEqual(BodyWeightUnit.kg, ConsumptionIntakeUnit.gPerKgBWPerDay.GetBodyWeightUnit());
        }

        /// <summary>
        /// Tests whether the get consumption (amount) unit method is available for all consumption intake units.
        /// </summary>
        [TestMethod]
        public void ConsumptionIntakeUnitConverter_TestGetTargetUnitConversionFactor() {
            var units = Enum.GetValues(typeof(ConsumptionIntakeUnit)).Cast<ConsumptionIntakeUnit>();
            foreach (var value in units) {
                foreach (var target in units) {
                    var consumptionUnit = value.GetTargetUnitConversionFactor(target, 70);
                }
            }
            Assert.AreEqual(50D, ConsumptionIntakeUnit.gPerKgBWPerDay.GetTargetUnitConversionFactor(ConsumptionIntakeUnit.gPerDay, 50D));
            Assert.AreEqual(0.02, ConsumptionIntakeUnit.gPerDay.GetTargetUnitConversionFactor(ConsumptionIntakeUnit.gPerKgBWPerDay, 50D));
            Assert.AreEqual(1D, ConsumptionIntakeUnit.gPerDay.GetTargetUnitConversionFactor(ConsumptionIntakeUnit.gPerDay, 70D));
            Assert.AreEqual(1D, ConsumptionIntakeUnit.gPerKgBWPerDay.GetTargetUnitConversionFactor(ConsumptionIntakeUnit.gPerKgBWPerDay, 70D));
        }
    }
}
