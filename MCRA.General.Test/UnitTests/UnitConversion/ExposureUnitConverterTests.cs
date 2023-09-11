using MCRA.Utils.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.General.Test.UnitTests.UnitConversion {

    [TestClass]
    public class ExposureUnitConverterTests {

        [TestMethod]
        public void ExposureUnitConverter_TestCompleteness() {
            var definition = ExposureUnitConverter.UnitDefinition;
            var enumValues = Enum.GetValues(typeof(ExposureUnit)).Cast<ExposureUnit>();
            // Check whether there is a unit definition for each enum value.
            foreach (var value in enumValues) {
                var unitValueDefinition = definition.FromString<ExposureUnit>(value.ToString());
                Assert.AreEqual(unitValueDefinition, value);
            }
            // Check whether each unit specified in the definitions matches an enum value.
            foreach (var units in definition.Units) {
                var value = Enum.Parse(typeof(ExposureUnit), units.Id);
            }
        }

        /// <summary>
        /// Tests whether each of the specified aliases is correctly parsed.
        /// </summary>
        [TestMethod]
        public void ExposureUnitConverter_TestAliases() {
            var definition = ExposureUnitConverter.UnitDefinition;
            var aliases = definition.Units.SelectMany(r => r.Aliases, (r, a) => new {
                Unit = Enum.Parse(typeof(ExposureUnit), r.Id),
                Alias = a
            });
            // Check whether the parsed unit for each alias matches with the amount unit for which it is supposed to be an alias.
            foreach (var alias in aliases) {
                var parsedUnit = ExposureUnitConverter.FromString(alias.Alias);
                Assert.AreEqual(alias.Unit, parsedUnit);
            }
        }

        /// <summary>
        /// Tests whether there is a display name and a short display name for each unit.
        /// </summary>
        [TestMethod]
        public void ExposureUnitConverter_TestDisplayNames() {
            var definition = ExposureUnitConverter.UnitDefinition;
            foreach (var units in definition.Units) {
                var value = (ExposureUnit)Enum.Parse(typeof(ExposureUnit), units.Id);
                Assert.IsTrue(units.Name.Equals(value.GetDisplayName(), StringComparison.InvariantCultureIgnoreCase));
                Assert.IsTrue(units.ShortName.Equals(value.GetShortDisplayName(), StringComparison.InvariantCultureIgnoreCase));
            }
        }

        [TestMethod]
        [DataRow(ExposureUnit.gPerKgBWPerDay, ConcentrationUnit.gPerKg, 1e0)]
        [DataRow(ExposureUnit.mgPerKgBWPerDay, ConcentrationUnit.gPerKg, 1e-3)]
        [DataRow(ExposureUnit.ugPerKgBWPerDay, ConcentrationUnit.gPerKg, 1e-6)]
        [DataRow(ExposureUnit.ngPerKgBWPerDay, ConcentrationUnit.gPerKg, 1e-9)]
        [DataRow(ExposureUnit.pgPerKgBWPerDay, ConcentrationUnit.gPerKg, 1e-12)]
        [DataRow(ExposureUnit.gPerGBWPerDay, ConcentrationUnit.gPerKg, 1e0)]
        [DataRow(ExposureUnit.mgPerGBWPerDay, ConcentrationUnit.gPerKg, 1e-3)]
        [DataRow(ExposureUnit.ugPerGBWPerDay, ConcentrationUnit.gPerKg, 1e-6)]
        [DataRow(ExposureUnit.ngPerGBWPerDay, ConcentrationUnit.gPerKg, 1e-9)]
        [DataRow(ExposureUnit.pgPerGBWPerDay, ConcentrationUnit.gPerKg, 1e-12)]
        [DataRow(ExposureUnit.kgPerDay, ConcentrationUnit.gPerKg, 1e3)]
        [DataRow(ExposureUnit.gPerDay, ConcentrationUnit.gPerKg, 1e0)]
        [DataRow(ExposureUnit.mgPerDay, ConcentrationUnit.gPerKg, 1e-3)]
        [DataRow(ExposureUnit.ugPerDay, ConcentrationUnit.gPerKg, 1e-6)]
        [DataRow(ExposureUnit.ngPerDay, ConcentrationUnit.gPerKg, 1e-9)]
        [DataRow(ExposureUnit.pgPerDay, ConcentrationUnit.gPerKg, 1e-12)]
        [DataRow(ExposureUnit.gPerKg, ConcentrationUnit.gPerKg, 1e0)]
        [DataRow(ExposureUnit.mgPerKg, ConcentrationUnit.gPerKg, 1e-3)]
        [DataRow(ExposureUnit.ugPerKg, ConcentrationUnit.gPerKg, 1e-6)]
        [DataRow(ExposureUnit.ngPerKg, ConcentrationUnit.gPerKg, 1e-9)]
        [DataRow(ExposureUnit.pgPerKg, ConcentrationUnit.gPerKg, 1e-12)]
        [DataRow(ExposureUnit.g, ConcentrationUnit.gPerKg, 1e0)]
        [DataRow(ExposureUnit.mg, ConcentrationUnit.gPerKg, 1e-3)]
        [DataRow(ExposureUnit.ug, ConcentrationUnit.gPerKg, 1e-6)]
        [DataRow(ExposureUnit.ng, ConcentrationUnit.gPerKg, 1e-9)]
        [DataRow(ExposureUnit.pg, ConcentrationUnit.gPerKg, 1e-12)]
        public void ExposureUnitConverter_GetConcentrationUnitMultiplier_TestScnario(
            ExposureUnit exposureUnit, 
            ConcentrationUnit concentrationUnit,
            double expected
        ) {
            var eps = 1e-6;
            Assert.AreEqual(ExposureUnitExtensions.GetConcentrationUnitMultiplier(exposureUnit, concentrationUnit), expected, eps);
        }

        /// <summary>
        /// Tests for successful get concentration unit multiplier method execution for
        /// all combinatins of exposure unit and concentration unit.
        /// </summary>
        [TestMethod]
        public void ExposureUnitConverter_GetConcentrationUnitMultiplier_TestCompleteness() {
            var enumValues = Enum.GetValues(typeof(ExposureUnit)).Cast<ExposureUnit>();
            var targetUnitValues = Enum.GetValues(typeof(ConcentrationUnit)).Cast<ConcentrationUnit>();
            foreach (var enumValue in enumValues) {
                foreach (var target in targetUnitValues) {
                    var factor = ExposureUnitExtensions.GetConcentrationUnitMultiplier(enumValue, target);
                    Assert.IsFalse(double.IsNaN(factor));
                }
            }
        }

        /// <summary>
        /// Tests for valid substance amount unit extraction for some selected intake units.
        /// </summary>
        [TestMethod]
        public void ExposureUnitConverter_TestGetSubstanceAmountUnit() {
            Assert.AreEqual(SubstanceAmountUnit.Picograms, ExposureUnit.pgPerGBWPerDay.GetSubstanceAmountUnit());
            Assert.AreEqual(SubstanceAmountUnit.Picograms, ExposureUnit.pgPerKgBWPerDay.GetSubstanceAmountUnit());
            Assert.AreEqual(SubstanceAmountUnit.Micrograms, ExposureUnit.ugPerKgBWPerDay.GetSubstanceAmountUnit());
            Assert.AreEqual(SubstanceAmountUnit.Grams, ExposureUnit.gPerGBWPerDay.GetSubstanceAmountUnit());
            Assert.AreEqual(SubstanceAmountUnit.Femtograms, ExposureUnit.fgPerGBWPerDay.GetSubstanceAmountUnit());
            Assert.AreEqual(SubstanceAmountUnit.Femtograms, ExposureUnit.fgPerKgBWPerDay.GetSubstanceAmountUnit());
            Assert.AreEqual(SubstanceAmountUnit.Micrograms, ExposureUnit.ugPerGBWPerDay.GetSubstanceAmountUnit());
            Assert.AreEqual(SubstanceAmountUnit.Micrograms, ExposureUnit.ugPerKgBWPerDay.GetSubstanceAmountUnit());
        }

        /// <summary>
        /// Tests whether a substance amount can be obtained for all intake units.
        /// </summary>
        [TestMethod]
        public void ExposureUnitConverter_TestGetSubstanceAmountUnitAll() {
            var enumValues = Enum.GetValues(typeof(ExposureUnit)).Cast<ExposureUnit>().ToList();
            foreach (var value in enumValues) {
                Assert.IsFalse(value.GetSubstanceAmountUnit() == SubstanceAmountUnit.Undefined);
            }
        }

        /// <summary>
        /// Tests for valid concentration mass unit extraction for some selected
        /// exposure units.
        /// </summary>
        [TestMethod]
        [DataRow(ExposureUnit.pgPerGBWPerDay, ConcentrationMassUnit.Grams)]
        [DataRow(ExposureUnit.pgPerKgBWPerDay, ConcentrationMassUnit.Kilograms)]
        [DataRow(ExposureUnit.ugPerKgBWPerDay, ConcentrationMassUnit.Kilograms)]
        [DataRow(ExposureUnit.gPerGBWPerDay, ConcentrationMassUnit.Grams)]
        [DataRow(ExposureUnit.fgPerKgBWPerDay, ConcentrationMassUnit.Kilograms)]
        [DataRow(ExposureUnit.pgPerKgBWPerDay, ConcentrationMassUnit.Kilograms)]
        [DataRow(ExposureUnit.ugPerKgBWPerDay, ConcentrationMassUnit.Kilograms)]
        [DataRow(ExposureUnit.mgPerGBWPerDay, ConcentrationMassUnit.Grams)]
        [DataRow(ExposureUnit.ugPerGBWPerDay, ConcentrationMassUnit.Grams)]
        public void ExposureUnitConverter_TestGetConcentrationMassUnit(
            ExposureUnit exposureUnit,
            ConcentrationMassUnit expected
        ) {
            Assert.AreEqual(expected, exposureUnit.GetConcentrationMassUnit());
        }

        /// <summary>
        /// Tests whether a concentration mass unit can be obtained for all intake units.
        /// </summary>
        [TestMethod]
        public void ExposureUnitConverter_TestGetConcentrationMassUnitAll() {
            var enumValues = Enum.GetValues(typeof(ExposureUnit)).Cast<ExposureUnit>().ToList();
            foreach (var value in enumValues) {
                Assert.IsFalse(value.GetConcentrationMassUnit() == ConcentrationMassUnit.Undefined);
            }
        }

        /// <summary>
        /// Tests for valid time scale unit extraction for some selected intake units.
        /// </summary>
        [TestMethod]
        public void ExposureUnitConverter_TestGetTimeScaleUnit() {
            Assert.AreEqual(TimeScaleUnit.PerDay, ExposureUnit.fgPerGBWPerDay.GetTimeScale());
            Assert.AreEqual(TimeScaleUnit.PerDay, ExposureUnit.fgPerKgBWPerDay.GetTimeScale());
            Assert.AreEqual(TimeScaleUnit.PerDay, ExposureUnit.gPerGBWPerDay.GetTimeScale());
            Assert.AreEqual(TimeScaleUnit.PerDay, ExposureUnit.pgPerKgBWPerDay.GetTimeScale());
            Assert.AreEqual(TimeScaleUnit.PerDay, ExposureUnit.mgPerKgBWPerDay.GetTimeScale());
            Assert.AreEqual(TimeScaleUnit.PerDay, ExposureUnit.mgPerGBWPerDay.GetTimeScale());
            Assert.AreEqual(TimeScaleUnit.PerDay, ExposureUnit.ugPerKgBWPerDay.GetTimeScale());
            Assert.AreEqual(TimeScaleUnit.PerDay, ExposureUnit.ugPerGBWPerDay.GetTimeScale());
        }

        [TestMethod]
        [DataRow(1D, ExposureUnit.gPerKgBWPerDay, ExposureUnit.gPerKgBWPerDay)]
        [DataRow(1e-3, ExposureUnit.mgPerKgBWPerDay, ExposureUnit.gPerKgBWPerDay)]
        [DataRow(1e-6, ExposureUnit.ugPerKgBWPerDay, ExposureUnit.gPerKgBWPerDay)]
        [DataRow(1e-9, ExposureUnit.ngPerKgBWPerDay, ExposureUnit.gPerKgBWPerDay)]
        [DataRow(1e-12, ExposureUnit.pgPerKgBWPerDay, ExposureUnit.gPerKgBWPerDay)]
        [DataRow(1e3, ExposureUnit.gPerGBWPerDay, ExposureUnit.gPerKgBWPerDay)]
        [DataRow(1D, ExposureUnit.mgPerGBWPerDay, ExposureUnit.gPerKgBWPerDay)]
        [DataRow(1e-3, ExposureUnit.ugPerGBWPerDay, ExposureUnit.gPerKgBWPerDay)]
        [DataRow(1e-6, ExposureUnit.ngPerGBWPerDay, ExposureUnit.gPerKgBWPerDay)]
        [DataRow(1e-9, ExposureUnit.pgPerGBWPerDay, ExposureUnit.gPerKgBWPerDay)]
        [DataRow(1D / 70D, ExposureUnit.gPerDay, ExposureUnit.gPerKgBWPerDay)]
        [DataRow(1e-3 / 70D, ExposureUnit.mgPerDay, ExposureUnit.gPerKgBWPerDay)]
        [DataRow(1e-6 / 70D, ExposureUnit.ugPerDay, ExposureUnit.gPerKgBWPerDay)]
        [DataRow(1e-9 / 70D, ExposureUnit.ngPerDay, ExposureUnit.gPerKgBWPerDay)]
        [DataRow(1e-12 / 70D, ExposureUnit.pgPerDay, ExposureUnit.gPerKgBWPerDay)]
        public void ExposureUnitConverter_TestGetExposureUnitMultiplier(
            double expected,
            ExposureUnit sourceExposureUnit,
            ExposureUnit targetExposureUnit
        ) {
            var val = sourceExposureUnit
                .GetExposureUnitMultiplier(ExposureUnitTriple.FromExposureUnit(targetExposureUnit), 70);
            Assert.AreEqual(expected, val, 1e-10);
        }

        [TestMethod]
        public void ExposureUnitConverter_TestGetExposureUnitMultiplierAll() {
            var enumValues = Enum.GetValues(typeof(ExposureUnit)).Cast<ExposureUnit>();
            var exposureUnitValues = Enum.GetValues(typeof(ExposureUnit)).Cast<ExposureUnit>();
            foreach (var enumValue in enumValues) {
                foreach (var exposureUnit in exposureUnitValues) {
                    var factor = ExposureUnitExtensions.GetExposureUnitMultiplier(enumValue, ExposureUnitTriple.FromExposureUnit(exposureUnit), 70);
                    Assert.IsFalse(double.IsNaN(factor));
                }
            }
        }
    }
}
