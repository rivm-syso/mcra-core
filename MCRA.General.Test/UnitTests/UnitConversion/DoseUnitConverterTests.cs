using MCRA.Utils.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.General.Test.UnitTests.UnitConversion {

    [TestClass]
    public class DoseUnitConverterTests {

        [TestMethod]
        public void DoseUnitConverter_TestCompleteness() {
            var definition = DoseUnitConverter.UnitDefinition;
            var enumValues = Enum.GetValues(typeof(DoseUnit)).Cast<DoseUnit>();
            // Check whether there is a unit definition for each enum value.
            foreach (var value in enumValues) {
                var unitValueDefinition = definition.FromString<DoseUnit>(value.ToString());
                Assert.AreEqual(unitValueDefinition, value);
            }
            // Check whether each unit specified in the definitions matches an enum value.
            foreach (var units in definition.Units) {
                var value = Enum.Parse(typeof(DoseUnit), units.Id);
            }
        }

        /// <summary>
        /// Tests whether each of the specified aliases is correctly parsed.
        /// </summary>
        [TestMethod]
        public void DoseUnitConverter_TestAliases() {
            var definition = DoseUnitConverter.UnitDefinition;
            var aliases = definition.Units.SelectMany(r => r.Aliases, (r, a) => new {
                Unit = Enum.Parse(typeof(DoseUnit), r.Id),
                Alias = a
            });
            // Check whether the parsed unit for each alias matches with the amount unit for which it is supposed to be an alias.
            foreach (var alias in aliases) {
                var parsedUnit = DoseUnitConverter.FromString(alias.Alias);
                Assert.AreEqual(alias.Unit, parsedUnit);
            }
        }

        /// <summary>
        /// Tests whether there is a display name and a short display name for each unit.
        /// </summary>
        [TestMethod]
        public void DoseUnitConverter_TestDisplayNames() {
            var definition = DoseUnitConverter.UnitDefinition;
            foreach (var units in definition.Units) {
                var value = (DoseUnit)Enum.Parse(typeof(DoseUnit), units.Id);
                Assert.IsTrue(units.Name.Equals(value.GetDisplayName(), StringComparison.InvariantCultureIgnoreCase));
                Assert.IsTrue(units.ShortName.Equals(value.GetShortDisplayName(), StringComparison.InvariantCultureIgnoreCase));
            }
        }

        /// <summary>
        /// Tests for valid substance amount unit extraction for some selected dose units.
        /// </summary>
        [TestMethod]
        public void DoseUnitConverter_TestGetSubstanceAmountUnit() {
            Assert.AreEqual(SubstanceAmountUnit.Picograms, DoseUnit.pgPerDay.GetSubstanceAmountUnit());
            Assert.AreEqual(SubstanceAmountUnit.Micromoles, DoseUnit.umoles.GetSubstanceAmountUnit());
            Assert.AreEqual(SubstanceAmountUnit.Micrograms, DoseUnit.ugPerKgBWPerDay.GetSubstanceAmountUnit());
            Assert.AreEqual(SubstanceAmountUnit.Grams, DoseUnit.gPerDay.GetSubstanceAmountUnit());
            Assert.AreEqual(SubstanceAmountUnit.Femtograms, DoseUnit.fgPerDay.GetSubstanceAmountUnit());
            Assert.AreEqual(SubstanceAmountUnit.Picograms, DoseUnit.pgPerKgBWPerDay.GetSubstanceAmountUnit());
            Assert.AreEqual(SubstanceAmountUnit.Micromoles, DoseUnit.uM.GetSubstanceAmountUnit());
            Assert.AreEqual(SubstanceAmountUnit.Kilograms, DoseUnit.kgPerKg.GetSubstanceAmountUnit());
            Assert.AreEqual(SubstanceAmountUnit.Picograms, DoseUnit.pgPerWeek.GetSubstanceAmountUnit());
            Assert.AreEqual(SubstanceAmountUnit.Micrograms, DoseUnit.ugPerKgBWPerWeek.GetSubstanceAmountUnit());
            Assert.AreEqual(SubstanceAmountUnit.Grams, DoseUnit.gPerWeek.GetSubstanceAmountUnit());
            Assert.AreEqual(SubstanceAmountUnit.Femtograms, DoseUnit.fgPerWeek.GetSubstanceAmountUnit());
            Assert.AreEqual(SubstanceAmountUnit.Picograms, DoseUnit.pgPerKgBWPerWeek.GetSubstanceAmountUnit());
        }

        /// <summary>
        /// Tests whether a substance amount can be obtained for all dose units.
        /// </summary>
        [TestMethod]
        public void DoseUnitConverter_TestGetSubstanceAmountUnitAll() {
            var enumValues = Enum.GetValues(typeof(DoseUnit)).Cast<DoseUnit>().ToList();
            foreach (var value in enumValues) {
                Assert.IsFalse(value.GetSubstanceAmountUnit() == SubstanceAmountUnit.Undefined);
            }
        }

        /// <summary>
        /// Tests for valid concentration mass unit extraction for some selected dose units.
        /// </summary>
        [TestMethod]
        public void DoseUnitConverter_TestGetConcentrationMassUnit() {
            Assert.AreEqual(ConcentrationMassUnit.PerUnit, DoseUnit.pgPerDay.GetConcentrationMassUnit());
            Assert.AreEqual(ConcentrationMassUnit.PerUnit, DoseUnit.umoles.GetConcentrationMassUnit());
            Assert.AreEqual(ConcentrationMassUnit.Kilograms, DoseUnit.ugPerKgBWPerDay.GetConcentrationMassUnit());
            Assert.AreEqual(ConcentrationMassUnit.PerUnit, DoseUnit.gPerDay.GetConcentrationMassUnit());
            Assert.AreEqual(ConcentrationMassUnit.PerUnit, DoseUnit.fgPerDay.GetConcentrationMassUnit());
            Assert.AreEqual(ConcentrationMassUnit.Kilograms, DoseUnit.pgPerKgBWPerDay.GetConcentrationMassUnit());
            Assert.AreEqual(ConcentrationMassUnit.Kilograms, DoseUnit.uM.GetConcentrationMassUnit());
            Assert.AreEqual(ConcentrationMassUnit.Kilograms, DoseUnit.kgPerKg.GetConcentrationMassUnit());
            Assert.AreEqual(ConcentrationMassUnit.Grams, DoseUnit.mgPerGBWPerDay.GetConcentrationMassUnit());
            Assert.AreEqual(ConcentrationMassUnit.Grams, DoseUnit.ugPerGBWPerDay.GetConcentrationMassUnit());
            Assert.AreEqual(ConcentrationMassUnit.PerUnit, DoseUnit.pgPerWeek.GetConcentrationMassUnit());
            Assert.AreEqual(ConcentrationMassUnit.Kilograms, DoseUnit.ugPerKgBWPerWeek.GetConcentrationMassUnit());
            Assert.AreEqual(ConcentrationMassUnit.PerUnit, DoseUnit.gPerWeek.GetConcentrationMassUnit());
            Assert.AreEqual(ConcentrationMassUnit.PerUnit, DoseUnit.fgPerWeek.GetConcentrationMassUnit());
            Assert.AreEqual(ConcentrationMassUnit.Kilograms, DoseUnit.pgPerKgBWPerWeek.GetConcentrationMassUnit());
            Assert.AreEqual(ConcentrationMassUnit.Grams, DoseUnit.mgPerGBWPerWeek.GetConcentrationMassUnit());
            Assert.AreEqual(ConcentrationMassUnit.Grams, DoseUnit.ugPerGBWPerWeek.GetConcentrationMassUnit());
        }

        /// <summary>
        /// Tests whether a concentration mass unit can be obtained for all dose units.
        /// </summary>
        [TestMethod]
        public void DoseUnitConverter_TestGetConcentrationMassUnitAll() {
            var enumValues = Enum.GetValues(typeof(DoseUnit)).Cast<DoseUnit>().ToList();
            foreach (var value in enumValues) {
                Assert.IsFalse(value.GetConcentrationMassUnit() == ConcentrationMassUnit.Undefined);
            }
        }

        /// <summary>
        /// Tests for valid time scale unit extraction for some selected dose units.
        /// </summary>
        [TestMethod]
        public void DoseUnitConverter_TestGetTimeScaleUnit() {
            Assert.AreEqual(TimeScaleUnit.PerDay, DoseUnit.pgPerDay.GetTimeScale());
            Assert.AreEqual(TimeScaleUnit.Unspecified, DoseUnit.umoles.GetTimeScale());
            Assert.AreEqual(TimeScaleUnit.PerDay, DoseUnit.ugPerKgBWPerDay.GetTimeScale());
            Assert.AreEqual(TimeScaleUnit.PerDay, DoseUnit.gPerDay.GetTimeScale());
            Assert.AreEqual(TimeScaleUnit.PerDay, DoseUnit.fgPerDay.GetTimeScale());
            Assert.AreEqual(TimeScaleUnit.PerDay, DoseUnit.pgPerKgBWPerDay.GetTimeScale());
            Assert.AreEqual(TimeScaleUnit.Unspecified, DoseUnit.uM.GetTimeScale());
            Assert.AreEqual(TimeScaleUnit.Unspecified, DoseUnit.kgPerKg.GetTimeScale());
            Assert.AreEqual(TimeScaleUnit.PerDay, DoseUnit.mgPerGBWPerDay.GetTimeScale());
            Assert.AreEqual(TimeScaleUnit.PerDay, DoseUnit.ugPerGBWPerDay.GetTimeScale());
        }

        /// <summary>
        /// Tests whether a concentration mass unit can be obtained for all dose units.
        /// </summary>
        [TestMethod]
        public void DoseUnitConverter_TestGetTimeScaleAll() {
            var enumValues = Enum.GetValues(typeof(DoseUnit)).Cast<DoseUnit>().ToList();
            foreach (var value in enumValues) {
                if (value.ToString().Contains("PerDay", StringComparison.OrdinalIgnoreCase)) {
                    Assert.AreEqual(TimeScaleUnit.PerDay, value.GetTimeScale());
                } else if (value.ToString().Contains("PerWeek", StringComparison.OrdinalIgnoreCase)) {
                    try {
                        Assert.AreEqual(TimeScaleUnit.Unspecified, value.GetTimeScale());
                        // Should throw an exception; if we get here, then fail
                        Assert.Fail();
                    } catch {
                    }
                } else {
                    Assert.AreEqual(TimeScaleUnit.Unspecified, value.GetTimeScale());
                }
            }
        }

        /// <summary>
        /// Test get alignment factor to align doses with a specified unit to
        /// the specified target unit.
        /// </summary>
        [TestMethod]
        public void DoseUnitConverter_TestGetDoseAlignmentFactor() {
            // Dose units used for external exposures
            var targetUnitExternal = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
            Assert.AreEqual(1D, DoseUnit.mgPerKgBWPerDay.GetDoseAlignmentFactor(targetUnitExternal, double.NaN), 1e-10);
            Assert.AreEqual(1/7D, DoseUnit.mgPerKgBWPerWeek.GetDoseAlignmentFactor(targetUnitExternal, double.NaN), 1e-10);
            Assert.AreEqual(1D, DoseUnit.mgPerKg.GetDoseAlignmentFactor(targetUnitExternal, double.NaN), 1e-10);
            Assert.AreEqual(1e-3, DoseUnit.ugPerKgBWPerDay.GetDoseAlignmentFactor(targetUnitExternal, double.NaN), 1e-10);
            Assert.AreEqual(1e-3/7, DoseUnit.ugPerKgBWPerWeek.GetDoseAlignmentFactor(targetUnitExternal, double.NaN), 1e-10);
            Assert.AreEqual(1D, DoseUnit.ugPerGBWPerDay.GetDoseAlignmentFactor(targetUnitExternal, double.NaN), 1e-10);
            Assert.AreEqual(1/7D, DoseUnit.ugPerGBWPerWeek.GetDoseAlignmentFactor(targetUnitExternal, double.NaN), 1e-10);
            Assert.AreEqual(2, DoseUnit.mM.GetDoseAlignmentFactor(targetUnitExternal, 2), 1e-10);
            Assert.AreEqual(2e-3, DoseUnit.uM.GetDoseAlignmentFactor(targetUnitExternal, 2), 1e-10);

            // Dose units for internal exposures
            var targetUnitInternal = new ExposureUnitTriple(SubstanceAmountUnit.Milligrams, ConcentrationMassUnit.Liter);
            Assert.AreEqual(1e+6, DoseUnit.kgPerL.GetDoseAlignmentFactor(targetUnitInternal));
            Assert.AreEqual(1e+3, DoseUnit.gPerL.GetDoseAlignmentFactor(targetUnitInternal));
            Assert.AreEqual(1, DoseUnit.mgPerL.GetDoseAlignmentFactor(targetUnitInternal));
            Assert.AreEqual(1e-3, DoseUnit.ugPerL.GetDoseAlignmentFactor(targetUnitInternal));
            Assert.AreEqual(1e-6, DoseUnit.ngPerL.GetDoseAlignmentFactor(targetUnitInternal));
            Assert.AreEqual(1e-9, DoseUnit.pgPerL.GetDoseAlignmentFactor(targetUnitInternal));

            double concentrationInUrine = 7.4; // ng/L urine
            var convertFromMatrixTargetUnit = new ExposureUnitTriple(
                SubstanceAmountUnit.Nanograms,
                ConcentrationMassUnit.Liter,
                TimeScaleUnit.SteadyState
            );
            var convertToMatrixTargetUnit = new ExposureUnitTriple(
                SubstanceAmountUnit.Micrograms,
                ConcentrationMassUnit.Liter,
                TimeScaleUnit.SteadyState
            );
            double kineticModelConversionFactor = 0.7;
            var convertFromDoseUnit = DoseUnit.ugPerL;      // ug/L urine
            var convertToDoseUnit = DoseUnit.mgPerL;        // mg/L blood

            var totalFactor = kineticModelConversionFactor * convertToDoseUnit.GetDoseAlignmentFactor(convertToMatrixTargetUnit) / convertFromDoseUnit.GetDoseAlignmentFactor(convertFromMatrixTargetUnit);
            double concentrationInBlood = concentrationInUrine * totalFactor;
            Assert.AreEqual(5.18, concentrationInBlood);
        }
    }
}
