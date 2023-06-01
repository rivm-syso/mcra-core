using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.General.Test.UnitTests.UnitConversion {

    [TestClass]
    public class TargetUnitTests {

        [TestMethod]
        public void TargetUnit_TestConstructor() {
            var substanceAmount = SubstanceAmountUnit.Grams;
            var timeUnit = TimeScaleUnit.PerDay;
            var massUnit = ConcentrationMassUnit.Kilograms;
            var biologicalMatrix = BiologicalMatrix.WholeBody;
            var targetUnit = new TargetUnit(substanceAmount, massUnit, timeUnit, biologicalMatrix);
            Assert.AreEqual(substanceAmount, targetUnit.SubstanceAmountUnit);
            Assert.AreEqual(biologicalMatrix, targetUnit.BiologicalMatrix);
            Assert.AreEqual(timeUnit, targetUnit.TimeScaleUnit);
            Assert.AreEqual(massUnit, targetUnit.ConcentrationMassUnit);
        }

        [TestMethod]
        public void TargetUnit_TestFromDoseUnit() {
            Assert.AreEqual(SubstanceAmountUnit.Femtograms, TargetUnit.FromDoseUnit(DoseUnit.fgPerDay, BiologicalMatrix.WholeBody).SubstanceAmountUnit);
            Assert.AreEqual(ConcentrationMassUnit.PerUnit, TargetUnit.FromDoseUnit(DoseUnit.fgPerDay, BiologicalMatrix.WholeBody).ConcentrationMassUnit);
            Assert.AreEqual(ConcentrationMassUnit.Kilograms, TargetUnit.FromDoseUnit(DoseUnit.fgPerKgBWPerDay, BiologicalMatrix.WholeBody).ConcentrationMassUnit);
            var doseUnits = Enum.GetValues(typeof(DoseUnit)).Cast<DoseUnit>().ToList();
            foreach (var doseUnit in doseUnits) {
                try {
                    var target = TargetUnit.FromDoseUnit(doseUnit, BiologicalMatrix.WholeBody);
                    Assert.IsNotNull(target);
                } catch {
                    // Expect failure for per-week dose units (not supported yet)
                    Assert.IsTrue(doseUnit.ToString().Contains("PerWeek", StringComparison.OrdinalIgnoreCase));
                }
            }
        }

        [TestMethod]
        public void TargetUnit_TestFromIntakeUnit() {
            Assert.AreEqual(SubstanceAmountUnit.Picograms, new TargetUnit(ExposureUnit.pgPerKgBWPerDay, BiologicalMatrix.WholeBody).SubstanceAmountUnit);
            Assert.AreEqual(SubstanceAmountUnit.Milligrams, new TargetUnit(ExposureUnit.mgPerKgBWPerDay, BiologicalMatrix.WholeBody).SubstanceAmountUnit);
            Assert.AreEqual(ConcentrationMassUnit.Kilograms, new TargetUnit(ExposureUnit.pgPerKgBWPerDay, BiologicalMatrix.WholeBody).ConcentrationMassUnit);
            Assert.AreEqual(TimeScaleUnit.PerDay, new TargetUnit(ExposureUnit.pgPerKgBWPerDay, BiologicalMatrix.WholeBody).TimeScaleUnit);
            Assert.AreEqual(TimeScaleUnit.PerDay, new TargetUnit(ExposureUnit.ugPerGBWPerDay, BiologicalMatrix.WholeBody).TimeScaleUnit);
            var intakeUnits = Enum.GetValues(typeof(ExposureUnit)).Cast<ExposureUnit>().ToList();
            foreach (var intakeUnit in intakeUnits) {
                var target = new TargetUnit(intakeUnit, BiologicalMatrix.WholeBody);
                Assert.IsNotNull(target);
            }
        }

        [TestMethod]
        public void TargetUnit_TestGetShortDisplayName() {
            var substanceAmountUnits = Enum.GetValues(typeof(SubstanceAmountUnit)).Cast<SubstanceAmountUnit>().ToList();
            var concentrationMassUnits = Enum.GetValues(typeof(ConcentrationMassUnit)).Cast<ConcentrationMassUnit>().ToList();
            var timeScaleUnits = Enum.GetValues(typeof(TimeScaleUnit)).Cast<TimeScaleUnit>().ToList();
            foreach (var substanceAmountUnit in substanceAmountUnits) {
                foreach (var concentrationMassUnit in concentrationMassUnits) {
                    foreach (var timeScaleUnit in timeScaleUnits) {
                        var target = new TargetUnit(substanceAmountUnit, concentrationMassUnit, timeScaleUnit, BiologicalMatrix.WholeBody);
                        Assert.IsTrue(!string.IsNullOrEmpty(target.GetShortDisplayName(TargetUnit.DisplayOption.AppendBiologicalMatrix)));
                    }
                }
            }
        }

        /// <summary>
        /// Test completeness of method to create target unit from consumption intake unit and concentration unit.
        /// </summary>
        [TestMethod]
        public void IntakeUnitConverter_TestCreateSingleValueDietaryExposureUnitCompleteness() {
            var concentrationUnits = Enum.GetValues(typeof(ConsumptionIntakeUnit)).Cast<ConsumptionIntakeUnit>().ToList();
            var consumptionIntakeUnits = Enum.GetValues(typeof(ConcentrationUnit)).Cast<ConcentrationUnit>().ToList();
            var bodyWeightUnits = Enum.GetValues(typeof(BodyWeightUnit)).Cast<BodyWeightUnit>().ToList();
            foreach (var concentrationUnit in concentrationUnits) {
                foreach (var consumptionIntakeUnit in consumptionIntakeUnits) {
                    foreach (var bodyWeightUnit in bodyWeightUnits) {
                        var target = TargetUnit.CreateSingleValueDietaryExposureUnit(concentrationUnit, consumptionIntakeUnit, bodyWeightUnit, true);
                        Assert.IsTrue(!string.IsNullOrEmpty(target.GetShortDisplayName(TargetUnit.DisplayOption.AppendBiologicalMatrix)));
                        target = TargetUnit.CreateSingleValueDietaryExposureUnit(concentrationUnit, consumptionIntakeUnit, bodyWeightUnit, false);
                        Assert.IsTrue(!string.IsNullOrEmpty(target.GetShortDisplayName(TargetUnit.DisplayOption.AppendBiologicalMatrix)));
                    }
                }
            }
        }

        /// <summary>
        /// Test method to create target unit from consumption intake unit and concentration unit.
        /// </summary>
        [TestMethod]
        public void IntakeUnitConverter_TestCreateSingleValueDietaryExposureUnit() {
            Assert.AreEqual(
                (new TargetUnit(ExposureUnit.ugPerDay)).GetShortDisplayName(),
                TargetUnit.CreateSingleValueDietaryExposureUnit(ConsumptionIntakeUnit.gPerDay, ConcentrationUnit.mgPerKg, BodyWeightUnit.kg, true).GetShortDisplayName()
            );
            Assert.AreEqual(
                (new TargetUnit(ExposureUnit.ugPerKgBWPerDay)).GetShortDisplayName(),
                TargetUnit.CreateSingleValueDietaryExposureUnit(ConsumptionIntakeUnit.gPerDay, ConcentrationUnit.mgPerKg, BodyWeightUnit.kg, false).GetShortDisplayName()
            );
            Assert.AreEqual(
                (new TargetUnit(ExposureUnit.ngPerKgBWPerDay)).GetShortDisplayName(),
                TargetUnit.CreateSingleValueDietaryExposureUnit(ConsumptionIntakeUnit.gPerKgBWPerDay, ConcentrationUnit.ugPerKg, BodyWeightUnit.kg, false).GetShortDisplayName()
            );
            Assert.AreEqual(
                (new TargetUnit(ExposureUnit.pgPerKgBWPerDay)).GetShortDisplayName(),
                TargetUnit.CreateSingleValueDietaryExposureUnit(ConsumptionIntakeUnit.gPerKgBWPerDay, ConcentrationUnit.ngPerKg, BodyWeightUnit.kg, false).GetShortDisplayName()
            );
        }

        /// <summary>
        /// Test intake unit calculation based on consumption, concentration, and bodyweight unit.
        /// </summary>
        [TestMethod]
        public void IntakeUnitConverter_GetIntakeUnitTest1() {
            Assert.AreEqual(TargetUnit.CreateDietaryExposureUnit(ConsumptionUnit.g, ConcentrationUnit.mgPerKg, BodyWeightUnit.g, false).GetShortDisplayName(), new TargetUnit(ExposureUnit.ugPerGBWPerDay).GetShortDisplayName());
            Assert.AreEqual(TargetUnit.CreateDietaryExposureUnit(ConsumptionUnit.g, ConcentrationUnit.mgPerKg, BodyWeightUnit.kg, false).GetShortDisplayName(), new TargetUnit(ExposureUnit.ugPerKgBWPerDay).GetShortDisplayName());
            Assert.AreEqual(TargetUnit.CreateDietaryExposureUnit(ConsumptionUnit.kg, ConcentrationUnit.mgPerKg, BodyWeightUnit.g, false).GetShortDisplayName(), new TargetUnit(ExposureUnit.mgPerGBWPerDay).GetShortDisplayName());
            Assert.AreEqual(TargetUnit.CreateDietaryExposureUnit(ConsumptionUnit.kg, ConcentrationUnit.mgPerKg, BodyWeightUnit.kg, false).GetShortDisplayName(), new TargetUnit(ExposureUnit.mgPerKgBWPerDay).GetShortDisplayName());

            Assert.AreEqual(TargetUnit.CreateDietaryExposureUnit(ConsumptionUnit.g, ConcentrationUnit.ugPerKg, BodyWeightUnit.g, false).GetShortDisplayName(), new TargetUnit(ExposureUnit.ngPerGBWPerDay).GetShortDisplayName());
            Assert.AreEqual(TargetUnit.CreateDietaryExposureUnit(ConsumptionUnit.g, ConcentrationUnit.ugPerKg, BodyWeightUnit.kg, false).GetShortDisplayName(), new TargetUnit(ExposureUnit.ngPerKgBWPerDay).GetShortDisplayName());
            Assert.AreEqual(TargetUnit.CreateDietaryExposureUnit(ConsumptionUnit.kg, ConcentrationUnit.ugPerKg, BodyWeightUnit.g, false).GetShortDisplayName(), new TargetUnit(ExposureUnit.ugPerGBWPerDay).GetShortDisplayName());
            Assert.AreEqual(TargetUnit.CreateDietaryExposureUnit(ConsumptionUnit.kg, ConcentrationUnit.ugPerKg, BodyWeightUnit.kg, false).GetShortDisplayName(), new TargetUnit(ExposureUnit.ugPerKgBWPerDay).GetShortDisplayName());
        }
    }
}
