using MCRA.Utils.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.General.Test.UnitTests.UnitConversion {

    [TestClass]
    public class ExposureUnitConverterTests {

        [TestMethod]
        public void ExposureUnitConverter_TestCompleteness() {
            var definition = ExternalExposureUnitConverter.UnitDefinition;
            var enumValues = Enum.GetValues(typeof(ExternalExposureUnit)).Cast<ExternalExposureUnit>();
            // Check whether there is a unit definition for each enum value.
            foreach (var value in enumValues) {
                var unitValueDefinition = definition.FromString<ExternalExposureUnit>(value.ToString());
                Assert.AreEqual(unitValueDefinition, value);
            }
            // Check whether each unit specified in the definitions matches an enum value.
            foreach (var units in definition.Units) {
                var value = Enum.Parse(typeof(ExternalExposureUnit), units.Id);
            }
        }

        /// <summary>
        /// Tests whether each of the specified aliases is correctly parsed.
        /// </summary>
        [TestMethod]
        public void ExposureUnitConverter_TestAliases() {
            var definition = ExternalExposureUnitConverter.UnitDefinition;
            var aliases = definition.Units.SelectMany(r => r.Aliases, (r, a) => new {
                Unit = Enum.Parse(typeof(ExternalExposureUnit), r.Id),
                Alias = a
            });
            // Check whether the parsed unit for each alias matches with the amount unit for which it is supposed to be an alias.
            foreach (var alias in aliases) {
                var parsedUnit = ExternalExposureUnitConverter.FromString(alias.Alias);
                Assert.AreEqual(alias.Unit, parsedUnit);
            }
        }

        /// <summary>
        /// Tests whether there is a display name and a short display name for each unit.
        /// </summary>
        [TestMethod]
        public void ExposureUnitConverter_TestDisplayNames() {
            var definition = ExternalExposureUnitConverter.UnitDefinition;
            foreach (var units in definition.Units) {
                var value = (ExternalExposureUnit)Enum.Parse(typeof(ExternalExposureUnit), units.Id);
                Assert.IsTrue(units.Name.Equals(value.GetDisplayName(), StringComparison.InvariantCultureIgnoreCase));
                Assert.IsTrue(units.ShortName.Equals(value.GetShortDisplayName(), StringComparison.InvariantCultureIgnoreCase));
            }
        }

        [TestMethod]
        [DataRow(ExternalExposureUnit.gPerKgBWPerDay, ConcentrationUnit.gPerKg, 1e0)]
        [DataRow(ExternalExposureUnit.mgPerKgBWPerDay, ConcentrationUnit.gPerKg, 1e-3)]
        [DataRow(ExternalExposureUnit.ugPerKgBWPerDay, ConcentrationUnit.gPerKg, 1e-6)]
        [DataRow(ExternalExposureUnit.ngPerKgBWPerDay, ConcentrationUnit.gPerKg, 1e-9)]
        [DataRow(ExternalExposureUnit.pgPerKgBWPerDay, ConcentrationUnit.gPerKg, 1e-12)]
        [DataRow(ExternalExposureUnit.gPerGBWPerDay, ConcentrationUnit.gPerKg, 1e0)]
        [DataRow(ExternalExposureUnit.mgPerGBWPerDay, ConcentrationUnit.gPerKg, 1e-3)]
        [DataRow(ExternalExposureUnit.ugPerGBWPerDay, ConcentrationUnit.gPerKg, 1e-6)]
        [DataRow(ExternalExposureUnit.ngPerGBWPerDay, ConcentrationUnit.gPerKg, 1e-9)]
        [DataRow(ExternalExposureUnit.pgPerGBWPerDay, ConcentrationUnit.gPerKg, 1e-12)]
        [DataRow(ExternalExposureUnit.kgPerDay, ConcentrationUnit.gPerKg, 1e3)]
        [DataRow(ExternalExposureUnit.gPerDay, ConcentrationUnit.gPerKg, 1e0)]
        [DataRow(ExternalExposureUnit.mgPerDay, ConcentrationUnit.gPerKg, 1e-3)]
        [DataRow(ExternalExposureUnit.ugPerDay, ConcentrationUnit.gPerKg, 1e-6)]
        [DataRow(ExternalExposureUnit.ngPerDay, ConcentrationUnit.gPerKg, 1e-9)]
        [DataRow(ExternalExposureUnit.pgPerDay, ConcentrationUnit.gPerKg, 1e-12)]
        [DataRow(ExternalExposureUnit.gPerKg, ConcentrationUnit.gPerKg, 1e0)]
        [DataRow(ExternalExposureUnit.mgPerKg, ConcentrationUnit.gPerKg, 1e-3)]
        [DataRow(ExternalExposureUnit.ugPerKg, ConcentrationUnit.gPerKg, 1e-6)]
        [DataRow(ExternalExposureUnit.ngPerKg, ConcentrationUnit.gPerKg, 1e-9)]
        [DataRow(ExternalExposureUnit.pgPerKg, ConcentrationUnit.gPerKg, 1e-12)]
        public void ExposureUnitConverter_GetConcentrationUnitMultiplier_TestScnario(
            ExternalExposureUnit exposureUnit,
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
            var enumValues = Enum.GetValues(typeof(ExternalExposureUnit)).Cast<ExternalExposureUnit>();
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
            Assert.AreEqual(SubstanceAmountUnit.Picograms, ExternalExposureUnit.pgPerGBWPerDay.GetSubstanceAmountUnit());
            Assert.AreEqual(SubstanceAmountUnit.Picograms, ExternalExposureUnit.pgPerKgBWPerDay.GetSubstanceAmountUnit());
            Assert.AreEqual(SubstanceAmountUnit.Micrograms, ExternalExposureUnit.ugPerKgBWPerDay.GetSubstanceAmountUnit());
            Assert.AreEqual(SubstanceAmountUnit.Grams, ExternalExposureUnit.gPerGBWPerDay.GetSubstanceAmountUnit());
            Assert.AreEqual(SubstanceAmountUnit.Femtograms, ExternalExposureUnit.fgPerGBWPerDay.GetSubstanceAmountUnit());
            Assert.AreEqual(SubstanceAmountUnit.Femtograms, ExternalExposureUnit.fgPerKgBWPerDay.GetSubstanceAmountUnit());
            Assert.AreEqual(SubstanceAmountUnit.Micrograms, ExternalExposureUnit.ugPerGBWPerDay.GetSubstanceAmountUnit());
            Assert.AreEqual(SubstanceAmountUnit.Micrograms, ExternalExposureUnit.ugPerKgBWPerDay.GetSubstanceAmountUnit());
        }

        /// <summary>
        /// Tests whether a substance amount can be obtained for all intake units.
        /// </summary>
        [TestMethod]
        public void ExposureUnitConverter_TestGetSubstanceAmountUnitAll() {
            var enumValues = Enum.GetValues(typeof(ExternalExposureUnit)).Cast<ExternalExposureUnit>().ToList();
            foreach (var value in enumValues) {
                Assert.IsFalse(value.GetSubstanceAmountUnit() == SubstanceAmountUnit.Undefined);
            }
        }

        /// <summary>
        /// Tests for valid concentration mass unit extraction for some selected
        /// exposure units.
        /// </summary>
        [TestMethod]
        [DataRow(ExternalExposureUnit.pgPerGBWPerDay, ConcentrationMassUnit.Grams)]
        [DataRow(ExternalExposureUnit.pgPerKgBWPerDay, ConcentrationMassUnit.Kilograms)]
        [DataRow(ExternalExposureUnit.ugPerKgBWPerDay, ConcentrationMassUnit.Kilograms)]
        [DataRow(ExternalExposureUnit.gPerGBWPerDay, ConcentrationMassUnit.Grams)]
        [DataRow(ExternalExposureUnit.fgPerKgBWPerDay, ConcentrationMassUnit.Kilograms)]
        [DataRow(ExternalExposureUnit.pgPerKgBWPerDay, ConcentrationMassUnit.Kilograms)]
        [DataRow(ExternalExposureUnit.ugPerKgBWPerDay, ConcentrationMassUnit.Kilograms)]
        [DataRow(ExternalExposureUnit.mgPerGBWPerDay, ConcentrationMassUnit.Grams)]
        [DataRow(ExternalExposureUnit.ugPerGBWPerDay, ConcentrationMassUnit.Grams)]
        public void ExposureUnitConverter_TestGetConcentrationMassUnit(
            ExternalExposureUnit exposureUnit,
            ConcentrationMassUnit expected
        ) {
            Assert.AreEqual(expected, exposureUnit.GetConcentrationMassUnit());
        }

        /// <summary>
        /// Tests whether a concentration mass unit can be obtained for all intake units.
        /// </summary>
        [TestMethod]
        public void ExposureUnitConverter_TestGetConcentrationMassUnitAll() {
            var enumValues = Enum.GetValues(typeof(ExternalExposureUnit)).Cast<ExternalExposureUnit>().ToList();
            foreach (var value in enumValues) {
                Assert.IsFalse(value.GetConcentrationMassUnit() == ConcentrationMassUnit.Undefined);
            }
        }

        /// <summary>
        /// Tests for valid time scale unit extraction for some selected intake units.
        /// </summary>
        [TestMethod]
        public void ExposureUnitConverter_TestGetTimeScaleUnit() {
            Assert.AreEqual(TimeScaleUnit.PerDay, ExternalExposureUnit.fgPerGBWPerDay.GetTimeScale());
            Assert.AreEqual(TimeScaleUnit.PerDay, ExternalExposureUnit.fgPerKgBWPerDay.GetTimeScale());
            Assert.AreEqual(TimeScaleUnit.PerDay, ExternalExposureUnit.gPerGBWPerDay.GetTimeScale());
            Assert.AreEqual(TimeScaleUnit.PerDay, ExternalExposureUnit.pgPerKgBWPerDay.GetTimeScale());
            Assert.AreEqual(TimeScaleUnit.PerDay, ExternalExposureUnit.mgPerKgBWPerDay.GetTimeScale());
            Assert.AreEqual(TimeScaleUnit.PerDay, ExternalExposureUnit.mgPerGBWPerDay.GetTimeScale());
            Assert.AreEqual(TimeScaleUnit.PerDay, ExternalExposureUnit.ugPerKgBWPerDay.GetTimeScale());
            Assert.AreEqual(TimeScaleUnit.PerDay, ExternalExposureUnit.ugPerGBWPerDay.GetTimeScale());
        }

        [TestMethod]
        [DataRow(1D, ExternalExposureUnit.gPerKgBWPerDay, ExternalExposureUnit.gPerKgBWPerDay)]
        [DataRow(1e-3, ExternalExposureUnit.mgPerKgBWPerDay, ExternalExposureUnit.gPerKgBWPerDay)]
        [DataRow(1e-6, ExternalExposureUnit.ugPerKgBWPerDay, ExternalExposureUnit.gPerKgBWPerDay)]
        [DataRow(1e-9, ExternalExposureUnit.ngPerKgBWPerDay, ExternalExposureUnit.gPerKgBWPerDay)]
        [DataRow(1e-12, ExternalExposureUnit.pgPerKgBWPerDay, ExternalExposureUnit.gPerKgBWPerDay)]
        [DataRow(1e3, ExternalExposureUnit.gPerGBWPerDay, ExternalExposureUnit.gPerKgBWPerDay)]
        [DataRow(1D, ExternalExposureUnit.mgPerGBWPerDay, ExternalExposureUnit.gPerKgBWPerDay)]
        [DataRow(1e-3, ExternalExposureUnit.ugPerGBWPerDay, ExternalExposureUnit.gPerKgBWPerDay)]
        [DataRow(1e-6, ExternalExposureUnit.ngPerGBWPerDay, ExternalExposureUnit.gPerKgBWPerDay)]
        [DataRow(1e-9, ExternalExposureUnit.pgPerGBWPerDay, ExternalExposureUnit.gPerKgBWPerDay)]
        [DataRow(1D / 70D, ExternalExposureUnit.gPerDay, ExternalExposureUnit.gPerKgBWPerDay)]
        [DataRow(1e-3 / 70D, ExternalExposureUnit.mgPerDay, ExternalExposureUnit.gPerKgBWPerDay)]
        [DataRow(1e-6 / 70D, ExternalExposureUnit.ugPerDay, ExternalExposureUnit.gPerKgBWPerDay)]
        [DataRow(1e-9 / 70D, ExternalExposureUnit.ngPerDay, ExternalExposureUnit.gPerKgBWPerDay)]
        [DataRow(1e-12 / 70D, ExternalExposureUnit.pgPerDay, ExternalExposureUnit.gPerKgBWPerDay)]
        public void ExposureUnitConverter_TestGetExposureUnitMultiplier(
            double expected,
            ExternalExposureUnit sourceExposureUnit,
            ExternalExposureUnit targetExposureUnit
        ) {
            var val = sourceExposureUnit
                .GetExposureUnitMultiplier(ExposureUnitTriple.FromExposureUnit(targetExposureUnit), 70);
            Assert.AreEqual(expected, val, 1e-10);
        }

        [TestMethod]
        public void ExposureUnitConverter_TestGetExposureUnitMultiplierAll() {
            var enumValues = Enum.GetValues(typeof(ExternalExposureUnit)).Cast<ExternalExposureUnit>();
            var exposureUnitValues = Enum.GetValues(typeof(ExternalExposureUnit)).Cast<ExternalExposureUnit>();
            foreach (var enumValue in enumValues) {
                foreach (var exposureUnit in exposureUnitValues) {
                    var factor = ExposureUnitExtensions.GetExposureUnitMultiplier(enumValue, ExposureUnitTriple.FromExposureUnit(exposureUnit), 70);
                    Assert.IsFalse(double.IsNaN(factor));
                }
            }
        }
    }
}
