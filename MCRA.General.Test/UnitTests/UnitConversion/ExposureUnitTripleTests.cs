﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.General.Test.UnitTests.UnitConversion {

    [TestClass]
    public class ExposureUnitTripleTests {

        [TestMethod]
        public void ExposureUnitTriple_TestConstructor() {
            var substanceAmount = SubstanceAmountUnit.Grams;
            var timeUnit = TimeScaleUnit.PerDay;
            var massUnit = ConcentrationMassUnit.Kilograms;
            var targetUnit = new ExposureUnitTriple(
                substanceAmount,
                massUnit,
                timeUnit
            );
            Assert.AreEqual(substanceAmount, targetUnit.SubstanceAmountUnit);
            Assert.AreEqual(timeUnit, targetUnit.TimeScaleUnit);
            Assert.AreEqual(massUnit, targetUnit.ConcentrationMassUnit);
        }

        [TestMethod]
        public void ExposureUnitTriple_TestFromDoseUnit() {
            Assert.AreEqual(SubstanceAmountUnit.Femtograms, ExposureUnitTriple.FromDoseUnit(DoseUnit.fgPerDay).SubstanceAmountUnit);
            Assert.AreEqual(ConcentrationMassUnit.PerUnit, ExposureUnitTriple.FromDoseUnit(DoseUnit.fgPerDay).ConcentrationMassUnit);
            Assert.AreEqual(ConcentrationMassUnit.Kilograms, ExposureUnitTriple.FromDoseUnit(DoseUnit.fgPerKgBWPerDay).ConcentrationMassUnit);
            var doseUnits = Enum.GetValues(typeof(DoseUnit)).Cast<DoseUnit>().ToList();
            foreach (var doseUnit in doseUnits) {
                try {
                    var target = ExposureUnitTriple.FromDoseUnit(doseUnit);
                    Assert.IsNotNull(target);
                } catch {
                    // Expect failure for per-week dose units (not supported yet)
                    Assert.IsTrue(doseUnit.ToString().Contains("PerWeek", StringComparison.OrdinalIgnoreCase));
                }
            }
        }

        [TestMethod]
        public void ExposureUnitTriple_TestFromExposureUnit() {
            Assert.AreEqual(SubstanceAmountUnit.Picograms, ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.pgPerKgBWPerDay).SubstanceAmountUnit);
            Assert.AreEqual(SubstanceAmountUnit.Milligrams, ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay).SubstanceAmountUnit);
            Assert.AreEqual(ConcentrationMassUnit.Kilograms, ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.pgPerKgBWPerDay).ConcentrationMassUnit);
            Assert.AreEqual(TimeScaleUnit.PerDay, ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.pgPerKgBWPerDay).TimeScaleUnit);
            Assert.AreEqual(TimeScaleUnit.PerDay, ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerGBWPerDay).TimeScaleUnit);
            var intakeUnits = Enum.GetValues(typeof(ExternalExposureUnit)).Cast<ExternalExposureUnit>().ToList();
            foreach (var intakeUnit in intakeUnits) {
                var target = ExposureUnitTriple.FromExposureUnit(intakeUnit);
                Assert.IsNotNull(target);
            }
        }

        [TestMethod]
        public void ExposureUnitTriple_TestGetShortDisplayName() {
            var substanceAmountUnits = Enum.GetValues(typeof(SubstanceAmountUnit)).Cast<SubstanceAmountUnit>().ToList();
            var concentrationMassUnits = Enum.GetValues(typeof(ConcentrationMassUnit)).Cast<ConcentrationMassUnit>().ToList();
            var timeScaleUnits = Enum.GetValues(typeof(TimeScaleUnit)).Cast<TimeScaleUnit>().ToList();
            foreach (var substanceAmountUnit in substanceAmountUnits) {
                foreach (var concentrationMassUnit in concentrationMassUnits) {
                    foreach (var timeScaleUnit in timeScaleUnits) {
                        var target = new ExposureUnitTriple(substanceAmountUnit, concentrationMassUnit, timeScaleUnit);
                        Assert.IsTrue(!string.IsNullOrEmpty(target.GetShortDisplayName()));
                    }
                }
            }
        }

        /// <summary>
        /// Test creation of an exposure unit based on consumption, concentration and bodyweight unit
        /// for different scenario and asserts whether the generated unit is as expected.
        /// </summary>
        [TestMethod]
        [DataRow(ConsumptionUnit.g, ConcentrationUnit.mgPerKg, BodyWeightUnit.g, ExternalExposureUnit.ugPerGBWPerDay)]
        [DataRow(ConsumptionUnit.g, ConcentrationUnit.mgPerKg, BodyWeightUnit.kg, ExternalExposureUnit.ugPerKgBWPerDay)]
        [DataRow(ConsumptionUnit.kg, ConcentrationUnit.mgPerKg, BodyWeightUnit.g, ExternalExposureUnit.mgPerGBWPerDay)]
        [DataRow(ConsumptionUnit.kg, ConcentrationUnit.mgPerKg, BodyWeightUnit.kg, ExternalExposureUnit.mgPerKgBWPerDay)]
        [DataRow(ConsumptionUnit.g, ConcentrationUnit.ugPerKg, BodyWeightUnit.g, ExternalExposureUnit.ngPerGBWPerDay)]
        [DataRow(ConsumptionUnit.g, ConcentrationUnit.ugPerKg, BodyWeightUnit.kg, ExternalExposureUnit.ngPerKgBWPerDay)]
        [DataRow(ConsumptionUnit.kg, ConcentrationUnit.ugPerKg, BodyWeightUnit.g, ExternalExposureUnit.ugPerGBWPerDay)]
        [DataRow(ConsumptionUnit.kg, ConcentrationUnit.ugPerKg, BodyWeightUnit.kg, ExternalExposureUnit.ugPerKgBWPerDay)]
        public void ExposureUnitTriple_CreateDietaryExposureUnit_TestScenarios(
            ConsumptionUnit consumptionUnit,
            ConcentrationUnit concentrationUnit,
            BodyWeightUnit bodyWeightUnit,
            ExternalExposureUnit expected
        ) {
            Assert.AreEqual(
                ExposureUnitTriple.CreateDietaryExposureUnit(consumptionUnit, concentrationUnit, bodyWeightUnit, false).GetShortDisplayName(),
                ExposureUnitTriple.FromExposureUnit(expected).GetShortDisplayName()
            );
        }
    }
}
