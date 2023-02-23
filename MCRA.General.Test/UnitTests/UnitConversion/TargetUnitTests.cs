using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.General.Test.UnitTests.UnitConversion {

    [TestClass]
    public class TargetUnitTests {

        [TestMethod]
        public void TargetUnit_TestConstructor() {
            var substanceAmount = SubstanceAmountUnit.Grams;
            var timeUnit = TimeScaleUnit.PerDay;
            var massUnit = ConcentrationMassUnit.Kilograms;
            var compartment = "BW";
            var targetUnit = new TargetUnit(substanceAmount, massUnit, compartment, timeUnit);
            Assert.AreEqual(substanceAmount, targetUnit.SubstanceAmount);
            Assert.AreEqual(compartment, targetUnit.Compartment);
            Assert.AreEqual(timeUnit, targetUnit.TimeScaleUnit);
            Assert.AreEqual(massUnit, targetUnit.ConcentrationMassUnit);
        }

        [TestMethod]
        public void TargetUnit_TestFromDoseUnit() {
            Assert.AreEqual(SubstanceAmountUnit.Femtograms, TargetUnit.FromDoseUnit(DoseUnit.fgPerDay, "BW").SubstanceAmount);
            Assert.AreEqual(ConcentrationMassUnit.PerUnit, TargetUnit.FromDoseUnit(DoseUnit.fgPerDay, "BW").ConcentrationMassUnit);
            Assert.AreEqual(ConcentrationMassUnit.Kilograms, TargetUnit.FromDoseUnit(DoseUnit.fgPerKgBWPerDay, "BW").ConcentrationMassUnit);
            var doseUnits = Enum.GetValues(typeof(DoseUnit)).Cast<DoseUnit>().ToList();
            foreach (var doseUnit in doseUnits) {
                try {
                    var target = TargetUnit.FromDoseUnit(doseUnit, "BW");
                    Assert.IsNotNull(target);
                } catch {
                    // Expect failure for per-week dose units (not supported yet)
                    Assert.IsTrue(doseUnit.ToString().Contains("PerWeek", StringComparison.OrdinalIgnoreCase));
                }
            }
        }

        [TestMethod]
        public void TargetUnit_TestFromIntakeUnit() {
            Assert.AreEqual(SubstanceAmountUnit.Picograms, new TargetUnit(ExposureUnit.pgPerKgBWPerDay, "BW").SubstanceAmount);
            Assert.AreEqual(SubstanceAmountUnit.Milligrams, new TargetUnit(ExposureUnit.mgPerKgBWPerDay, "BW").SubstanceAmount);
            Assert.AreEqual(ConcentrationMassUnit.Kilograms, new TargetUnit(ExposureUnit.pgPerKgBWPerDay, "BW").ConcentrationMassUnit);
            Assert.AreEqual(TimeScaleUnit.PerDay, new TargetUnit(ExposureUnit.pgPerKgBWPerDay, "BW").TimeScaleUnit);
            Assert.AreEqual(TimeScaleUnit.PerDay, new TargetUnit(ExposureUnit.ugPerGBWPerDay, "BW").TimeScaleUnit);
            var intakeUnits = Enum.GetValues(typeof(ExposureUnit)).Cast<ExposureUnit>().ToList();
            foreach (var intakeUnit in intakeUnits) {
                var target = new TargetUnit(intakeUnit, "BW");
                Assert.IsNotNull(target);
            }
        }

        [TestMethod]
        public void TargetUnit_TestGetShortDisplayName() {
            var substanceAmountUnits = Enum.GetValues(typeof(SubstanceAmountUnit)).Cast<SubstanceAmountUnit>().ToList();
            var concentrationMassUnits = Enum.GetValues(typeof(ConcentrationMassUnit)).Cast<ConcentrationMassUnit>().ToList();
            var timeUnits = Enum.GetValues(typeof(TimeScaleUnit)).Cast<TimeScaleUnit>().ToList();
            foreach (var substanceAmountUnit in substanceAmountUnits) {
                foreach (var concentrationMassUnit in concentrationMassUnits) {
                    foreach (var timeUnit in timeUnits) {
                        var target = new TargetUnit(substanceAmountUnit, concentrationMassUnit, "bw", timeUnit);
                        Assert.IsTrue(!string.IsNullOrEmpty(target.GetShortDisplayName(true)));
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
                        Assert.IsTrue(!string.IsNullOrEmpty(target.GetShortDisplayName(true)));
                        target = TargetUnit.CreateSingleValueDietaryExposureUnit(concentrationUnit, consumptionIntakeUnit, bodyWeightUnit, false);
                        Assert.IsTrue(!string.IsNullOrEmpty(target.GetShortDisplayName(true)));
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
                (new TargetUnit(ExposureUnit.ugPerDay)).GetShortDisplayName(false),
                TargetUnit.CreateSingleValueDietaryExposureUnit(ConsumptionIntakeUnit.gPerDay, ConcentrationUnit.mgPerKg, BodyWeightUnit.kg, true).GetShortDisplayName(false)
            );
            Assert.AreEqual(
                (new TargetUnit(ExposureUnit.ugPerKgBWPerDay)).GetShortDisplayName(false),
                TargetUnit.CreateSingleValueDietaryExposureUnit(ConsumptionIntakeUnit.gPerDay, ConcentrationUnit.mgPerKg, BodyWeightUnit.kg, false).GetShortDisplayName(false)
            );
            Assert.AreEqual(
                (new TargetUnit(ExposureUnit.ngPerKgBWPerDay)).GetShortDisplayName(false),
                TargetUnit.CreateSingleValueDietaryExposureUnit(ConsumptionIntakeUnit.gPerKgBWPerDay, ConcentrationUnit.ugPerKg, BodyWeightUnit.kg, false).GetShortDisplayName(false)
            );
            Assert.AreEqual(
                (new TargetUnit(ExposureUnit.pgPerKgBWPerDay)).GetShortDisplayName(false),
                TargetUnit.CreateSingleValueDietaryExposureUnit(ConsumptionIntakeUnit.gPerKgBWPerDay, ConcentrationUnit.ngPerKg, BodyWeightUnit.kg, false).GetShortDisplayName(false)
            );
        }

        /// <summary>
        /// Test intake unit calculation based on consumption, concentration, and bodyweight unit.
        /// </summary>
        [TestMethod]
        public void IntakeUnitConverter_GetIntakeUnitTest1() {
            Assert.AreEqual(TargetUnit.CreateDietaryExposureUnit(ConsumptionUnit.g, ConcentrationUnit.mgPerKg, BodyWeightUnit.g, false).GetShortDisplayName(false), new TargetUnit(ExposureUnit.ugPerGBWPerDay).GetShortDisplayName(false));
            Assert.AreEqual(TargetUnit.CreateDietaryExposureUnit(ConsumptionUnit.g, ConcentrationUnit.mgPerKg, BodyWeightUnit.kg, false).GetShortDisplayName(false), new TargetUnit(ExposureUnit.ugPerKgBWPerDay).GetShortDisplayName(false));
            Assert.AreEqual(TargetUnit.CreateDietaryExposureUnit(ConsumptionUnit.kg, ConcentrationUnit.mgPerKg, BodyWeightUnit.g, false).GetShortDisplayName(false), new TargetUnit(ExposureUnit.mgPerGBWPerDay).GetShortDisplayName(false));
            Assert.AreEqual(TargetUnit.CreateDietaryExposureUnit(ConsumptionUnit.kg, ConcentrationUnit.mgPerKg, BodyWeightUnit.kg, false).GetShortDisplayName(false), new TargetUnit(ExposureUnit.mgPerKgBWPerDay).GetShortDisplayName(false));

            Assert.AreEqual(TargetUnit.CreateDietaryExposureUnit(ConsumptionUnit.g, ConcentrationUnit.ugPerKg, BodyWeightUnit.g, false).GetShortDisplayName(false), new TargetUnit(ExposureUnit.ngPerGBWPerDay).GetShortDisplayName(false));
            Assert.AreEqual(TargetUnit.CreateDietaryExposureUnit(ConsumptionUnit.g, ConcentrationUnit.ugPerKg, BodyWeightUnit.kg, false).GetShortDisplayName(false), new TargetUnit(ExposureUnit.ngPerKgBWPerDay).GetShortDisplayName(false));
            Assert.AreEqual(TargetUnit.CreateDietaryExposureUnit(ConsumptionUnit.kg, ConcentrationUnit.ugPerKg, BodyWeightUnit.g, false).GetShortDisplayName(false), new TargetUnit(ExposureUnit.ugPerGBWPerDay).GetShortDisplayName(false));
            Assert.AreEqual(TargetUnit.CreateDietaryExposureUnit(ConsumptionUnit.kg, ConcentrationUnit.ugPerKg, BodyWeightUnit.kg, false).GetShortDisplayName(false), new TargetUnit(ExposureUnit.ugPerKgBWPerDay).GetShortDisplayName(false));
        }
    }
}
