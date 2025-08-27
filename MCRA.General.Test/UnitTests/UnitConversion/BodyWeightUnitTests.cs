namespace MCRA.General.Test.UnitTests.UnitConversion {

    [TestClass]
    public class BodyWeightUnitTests {

        /// <summary>
        /// Tests whether there is a value definition for each enum value of the amount units enum type.
        /// </summary>
        [TestMethod]
        public void BodyWeightUnitConverter_TestCompleteness() {
            var definition = BodyWeightUnitConverter.UnitDefinition;
            var enumValues = Enum.GetValues(typeof(BodyWeightUnit)).Cast<BodyWeightUnit>();
            // Check whether there is a unit definition for each enum value.
            foreach (var value in enumValues) {
                var unitValueDefinition = definition.FromString<BodyWeightUnit>(value.ToString());
                Assert.AreEqual(unitValueDefinition, value);
            }
            // Check whether each unit specified in the definitions matches an enum value.
            foreach (var units in definition.Units) {
                var value = Enum.Parse(typeof(BodyWeightUnit), units.Id);
            }
        }

        /// <summary>
        /// Tests whether each of the specified aliases is correctly parsed.
        /// </summary>
        [TestMethod]
        public void BodyWeightUnitConverter_TestAliases() {
            var definition = BodyWeightUnitConverter.UnitDefinition;
            var aliases = definition.Units.SelectMany(r => r.Aliases, (r, a) => new {
                WeightUnit = Enum.Parse(typeof(BodyWeightUnit), r.Id),
                Alias = a
            });
            // Check whether the parsed unit for each alias matches with the amount unit for which it is supposed to be an alias.
            foreach (var alias in aliases) {
                var parsedUnit = BodyWeightUnitConverter.FromString(alias.Alias);
                Assert.AreEqual(alias.WeightUnit, parsedUnit);
            }
        }
        /// <summary>
        /// Tests whether body weights are rightly converted
        /// </summary>
        public void BodyWeightUnitConverter_TestGetBodyWeightUnitMultiplier() {
            var enumValues = Enum.GetValues(typeof(BodyWeightUnit)).Cast<BodyWeightUnit>();
            foreach (var value1 in enumValues) {
                foreach (var value2 in enumValues) {
                    var multiplier = value1.GetBodyWeightUnitMultiplier(value2);
                    Assert.IsFalse(double.IsNaN(multiplier));
                }
            }
            Assert.AreEqual(1D, BodyWeightUnit.kg.GetBodyWeightUnitMultiplier(BodyWeightUnit.kg), 1e-6);
            Assert.AreEqual(1e-3, BodyWeightUnit.g.GetBodyWeightUnitMultiplier(BodyWeightUnit.kg), 1e-6);
            Assert.AreEqual(1e3, BodyWeightUnit.kg.GetBodyWeightUnitMultiplier(BodyWeightUnit.g), 1e-6);
            Assert.AreEqual(1D, BodyWeightUnit.g.GetBodyWeightUnitMultiplier(BodyWeightUnit.g), 1e-6);
        }
    }
}
