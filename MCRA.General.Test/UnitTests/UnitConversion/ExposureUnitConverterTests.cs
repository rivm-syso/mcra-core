using MCRA.Utils.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

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
        public void ExposureUnitConverter_GetConcentrationUnitMultiplier() {
            var eps = 1e-6;
            Assert.AreEqual(ExposureUnitExtensions.GetConcentrationUnitMultiplier(ExposureUnit.gPerKgBWPerDay, ConcentrationUnit.gPerKg), 1e0, eps);
            Assert.AreEqual(ExposureUnitExtensions.GetConcentrationUnitMultiplier(ExposureUnit.mgPerKgBWPerDay, ConcentrationUnit.gPerKg), 1e-3, eps);
            Assert.AreEqual(ExposureUnitExtensions.GetConcentrationUnitMultiplier(ExposureUnit.ugPerKgBWPerDay, ConcentrationUnit.gPerKg), 1e-6, eps);
            Assert.AreEqual(ExposureUnitExtensions.GetConcentrationUnitMultiplier(ExposureUnit.ngPerKgBWPerDay, ConcentrationUnit.gPerKg), 1e-9, eps);
            Assert.AreEqual(ExposureUnitExtensions.GetConcentrationUnitMultiplier(ExposureUnit.pgPerKgBWPerDay, ConcentrationUnit.gPerKg), 1e-12, eps);

            Assert.AreEqual(ExposureUnitExtensions.GetConcentrationUnitMultiplier(ExposureUnit.gPerGBWPerDay, ConcentrationUnit.gPerKg), 1e0, eps);
            Assert.AreEqual(ExposureUnitExtensions.GetConcentrationUnitMultiplier(ExposureUnit.mgPerGBWPerDay, ConcentrationUnit.gPerKg), 1e-3, eps);
            Assert.AreEqual(ExposureUnitExtensions.GetConcentrationUnitMultiplier(ExposureUnit.ugPerGBWPerDay, ConcentrationUnit.gPerKg), 1e-6, eps);
            Assert.AreEqual(ExposureUnitExtensions.GetConcentrationUnitMultiplier(ExposureUnit.ngPerGBWPerDay, ConcentrationUnit.gPerKg), 1e-9, eps);
            Assert.AreEqual(ExposureUnitExtensions.GetConcentrationUnitMultiplier(ExposureUnit.pgPerGBWPerDay, ConcentrationUnit.gPerKg), 1e-12, eps);

            Assert.AreEqual(ExposureUnitExtensions.GetConcentrationUnitMultiplier(ExposureUnit.kgPerDay, ConcentrationUnit.gPerKg), 1e3, eps);
            Assert.AreEqual(ExposureUnitExtensions.GetConcentrationUnitMultiplier(ExposureUnit.gPerDay, ConcentrationUnit.gPerKg), 1e0, eps);
            Assert.AreEqual(ExposureUnitExtensions.GetConcentrationUnitMultiplier(ExposureUnit.mgPerDay, ConcentrationUnit.gPerKg), 1e-3, eps);
            Assert.AreEqual(ExposureUnitExtensions.GetConcentrationUnitMultiplier(ExposureUnit.ugPerDay, ConcentrationUnit.gPerKg), 1e-6, eps);
            Assert.AreEqual(ExposureUnitExtensions.GetConcentrationUnitMultiplier(ExposureUnit.ngPerDay, ConcentrationUnit.gPerKg), 1e-9, eps);
            Assert.AreEqual(ExposureUnitExtensions.GetConcentrationUnitMultiplier(ExposureUnit.pgPerDay, ConcentrationUnit.gPerKg), 1e-12, eps);

            Assert.AreEqual(ExposureUnitExtensions.GetConcentrationUnitMultiplier(ExposureUnit.gPerKg, ConcentrationUnit.gPerKg), 1e0, eps);
            Assert.AreEqual(ExposureUnitExtensions.GetConcentrationUnitMultiplier(ExposureUnit.mgPerKg, ConcentrationUnit.gPerKg), 1e-3, eps);
            Assert.AreEqual(ExposureUnitExtensions.GetConcentrationUnitMultiplier(ExposureUnit.ugPerKg, ConcentrationUnit.gPerKg), 1e-6, eps);
            Assert.AreEqual(ExposureUnitExtensions.GetConcentrationUnitMultiplier(ExposureUnit.ngPerKg, ConcentrationUnit.gPerKg), 1e-9, eps);
            Assert.AreEqual(ExposureUnitExtensions.GetConcentrationUnitMultiplier(ExposureUnit.pgPerKg, ConcentrationUnit.gPerKg), 1e-12, eps);

            Assert.AreEqual(ExposureUnitExtensions.GetConcentrationUnitMultiplier(ExposureUnit.g, ConcentrationUnit.gPerKg), 1e0, eps);
            Assert.AreEqual(ExposureUnitExtensions.GetConcentrationUnitMultiplier(ExposureUnit.mg, ConcentrationUnit.gPerKg), 1e-3, eps);
            Assert.AreEqual(ExposureUnitExtensions.GetConcentrationUnitMultiplier(ExposureUnit.ug, ConcentrationUnit.gPerKg), 1e-6, eps);
            Assert.AreEqual(ExposureUnitExtensions.GetConcentrationUnitMultiplier(ExposureUnit.ng, ConcentrationUnit.gPerKg), 1e-9, eps);
            Assert.AreEqual(ExposureUnitExtensions.GetConcentrationUnitMultiplier(ExposureUnit.pg, ConcentrationUnit.gPerKg), 1e-12, eps);

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
        /// Tests for valid concentration mass unit extraction for some selected intake units.
        /// </summary>
        [TestMethod]
        public void ExposureUnitConverter_TestGetConcentrationMassUnit() {
            Assert.AreEqual(ConcentrationMassUnit.Grams, ExposureUnit.pgPerGBWPerDay.GetConcentrationMassUnit());
            Assert.AreEqual(ConcentrationMassUnit.Kilograms, ExposureUnit.pgPerKgBWPerDay.GetConcentrationMassUnit());
            Assert.AreEqual(ConcentrationMassUnit.Kilograms, ExposureUnit.ugPerKgBWPerDay.GetConcentrationMassUnit());
            Assert.AreEqual(ConcentrationMassUnit.Grams, ExposureUnit.gPerGBWPerDay.GetConcentrationMassUnit());
            Assert.AreEqual(ConcentrationMassUnit.Kilograms, ExposureUnit.fgPerKgBWPerDay.GetConcentrationMassUnit());
            Assert.AreEqual(ConcentrationMassUnit.Kilograms, ExposureUnit.pgPerKgBWPerDay.GetConcentrationMassUnit());
            Assert.AreEqual(ConcentrationMassUnit.Kilograms, ExposureUnit.ugPerKgBWPerDay.GetConcentrationMassUnit());
            Assert.AreEqual(ConcentrationMassUnit.Grams, ExposureUnit.mgPerGBWPerDay.GetConcentrationMassUnit());
            Assert.AreEqual(ConcentrationMassUnit.Grams, ExposureUnit.ugPerGBWPerDay.GetConcentrationMassUnit());
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
        public void ExposureUnitConverter_GetExposureUnitMultiplier() {
            var eps = 1e-10;
            Assert.AreEqual(1D, ExposureUnit.gPerKgBWPerDay.GetExposureUnitMultiplier(new TargetUnit(ExposureUnit.gPerKgBWPerDay), 70), eps);
            Assert.AreEqual(1e-3, ExposureUnit.mgPerKgBWPerDay.GetExposureUnitMultiplier(new TargetUnit(ExposureUnit.gPerKgBWPerDay), 70), eps);
            Assert.AreEqual(1e-6, ExposureUnit.ugPerKgBWPerDay.GetExposureUnitMultiplier(new TargetUnit(ExposureUnit.gPerKgBWPerDay), 70), eps);
            Assert.AreEqual(1e-9, ExposureUnit.ngPerKgBWPerDay.GetExposureUnitMultiplier(new TargetUnit(ExposureUnit.gPerKgBWPerDay), 70), eps);
            Assert.AreEqual(1e-12, ExposureUnit.pgPerKgBWPerDay.GetExposureUnitMultiplier(new TargetUnit(ExposureUnit.gPerKgBWPerDay), 70), eps);

            Assert.AreEqual(1e3, ExposureUnit.gPerGBWPerDay.GetExposureUnitMultiplier(new TargetUnit(ExposureUnit.gPerKgBWPerDay), 70), eps);
            Assert.AreEqual(1D, ExposureUnit.mgPerGBWPerDay.GetExposureUnitMultiplier(new TargetUnit(ExposureUnit.gPerKgBWPerDay), 70), eps);
            Assert.AreEqual(1e-3, ExposureUnit.ugPerGBWPerDay.GetExposureUnitMultiplier(new TargetUnit(ExposureUnit.gPerKgBWPerDay), 70), eps);
            Assert.AreEqual(1e-6, ExposureUnit.ngPerGBWPerDay.GetExposureUnitMultiplier(new TargetUnit(ExposureUnit.gPerKgBWPerDay), 70), eps);
            Assert.AreEqual(1e-9, ExposureUnit.pgPerGBWPerDay.GetExposureUnitMultiplier(new TargetUnit(ExposureUnit.gPerKgBWPerDay), 70), eps);

            Assert.AreEqual(1D / 70D, ExposureUnit.gPerDay.GetExposureUnitMultiplier(new TargetUnit(ExposureUnit.gPerKgBWPerDay), 70), eps);
            Assert.AreEqual(1e-3 / 70D, ExposureUnit.mgPerDay.GetExposureUnitMultiplier(new TargetUnit(ExposureUnit.gPerKgBWPerDay), 70), eps);
            Assert.AreEqual(1e-6 / 70D, ExposureUnit.ugPerDay.GetExposureUnitMultiplier(new TargetUnit(ExposureUnit.gPerKgBWPerDay), 70), eps);
            Assert.AreEqual(1e-9 / 70D, ExposureUnit.ngPerDay.GetExposureUnitMultiplier(new TargetUnit(ExposureUnit.gPerKgBWPerDay), 70), eps);
            Assert.AreEqual(1e-12 / 70D, ExposureUnit.pgPerDay.GetExposureUnitMultiplier(new TargetUnit(ExposureUnit.gPerKgBWPerDay), 70), eps);

            var enumValues = Enum.GetValues(typeof(ExposureUnit)).Cast<ExposureUnit>();
            var targetUnitValues = Enum.GetValues(typeof(ExposureUnit)).Cast<ExposureUnit>();
            foreach (var enumValue in enumValues) {
                foreach (var target in targetUnitValues) {
                    var factor = ExposureUnitExtensions.GetExposureUnitMultiplier(enumValue, new TargetUnit(target), 70);
                    Assert.IsFalse(double.IsNaN(factor));
                }
            }
        }
    }
}
