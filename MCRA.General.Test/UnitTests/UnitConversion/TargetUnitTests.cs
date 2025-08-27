namespace MCRA.General.Test.UnitTests.UnitConversion {

    [TestClass]
    public class TargetUnitTests {

        [TestMethod]
        public void TargetUnit_TestConstructor() {
            var substanceAmount = SubstanceAmountUnit.Grams;
            var timeUnit = TimeScaleUnit.PerDay;
            var massUnit = ConcentrationMassUnit.Kilograms;
            var biologicalMatrix = BiologicalMatrix.WholeBody;
            var targetUnit = new TargetUnit(
                new ExposureTarget(biologicalMatrix),
                substanceAmount,
                massUnit,
                timeUnit
            );
            Assert.AreEqual(substanceAmount, targetUnit.SubstanceAmountUnit);
            Assert.AreEqual(biologicalMatrix, targetUnit.BiologicalMatrix);
            Assert.AreEqual(timeUnit, targetUnit.TimeScaleUnit);
            Assert.AreEqual(massUnit, targetUnit.ConcentrationMassUnit);
        }

        [TestMethod]
        public void TargetUnit_TestGetShortDisplayName() {
            var substanceAmountUnits = Enum.GetValues(typeof(SubstanceAmountUnit)).Cast<SubstanceAmountUnit>().ToList();
            var concentrationMassUnits = Enum.GetValues(typeof(ConcentrationMassUnit)).Cast<ConcentrationMassUnit>().ToList();
            var timeScaleUnits = Enum.GetValues(typeof(TimeScaleUnit)).Cast<TimeScaleUnit>().ToList();
            foreach (var substanceAmountUnit in substanceAmountUnits) {
                foreach (var concentrationMassUnit in concentrationMassUnits) {
                    foreach (var timeScaleUnit in timeScaleUnits) {
                        var target = new TargetUnit(
                            new ExposureTarget(BiologicalMatrix.WholeBody),
                            substanceAmountUnit,
                            concentrationMassUnit,
                            timeScaleUnit
                        );
                        Assert.IsTrue(
                            !string.IsNullOrEmpty(target.GetShortDisplayName(TargetUnit.DisplayOption.AppendBiologicalMatrix))
                        );
                    }
                }
            }
        }

        /// <summary>
        /// Test method to create target unit from consumption intake unit and concentration unit.
        /// </summary>
        [TestMethod]
        [DataRow(ConsumptionIntakeUnit.gPerDay, ConcentrationUnit.mgPerKg, BodyWeightUnit.kg, true, ExternalExposureUnit.ugPerDay)]
        [DataRow(ConsumptionIntakeUnit.gPerDay, ConcentrationUnit.mgPerKg, BodyWeightUnit.kg, false, ExternalExposureUnit.ugPerKgBWPerDay)]
        [DataRow(ConsumptionIntakeUnit.gPerKgBWPerDay, ConcentrationUnit.ugPerKg, BodyWeightUnit.kg, false, ExternalExposureUnit.ngPerKgBWPerDay)]
        [DataRow(ConsumptionIntakeUnit.gPerKgBWPerDay, ConcentrationUnit.ngPerKg, BodyWeightUnit.kg, false, ExternalExposureUnit.pgPerKgBWPerDay)]
        public void TargetUnit_TestCreateSingleValueDietaryExposureUnit(
            ConsumptionIntakeUnit consumptionIntakeUnit,
            ConcentrationUnit concentrationUnit,
            BodyWeightUnit bodyWeightUnit,
            bool isPerPerson,
            ExternalExposureUnit expected
        ) {
            Assert.AreEqual(
                TargetUnit
                    .FromExternalExposureUnit(expected)
                    .GetShortDisplayName(),
                TargetUnit
                    .CreateSingleValueDietaryExposureUnit(consumptionIntakeUnit, concentrationUnit, bodyWeightUnit, isPerPerson)
                    .GetShortDisplayName()
            );
        }

        /// <summary>
        /// Test completeness of method to create target unit from consumption intake unit and concentration unit.
        /// </summary>
        [TestMethod]
        public void TargetUnit_TestCreateSingleValueDietaryExposureUnitCompleteness() {
            var concentrationUnits = Enum.GetValues(typeof(ConsumptionIntakeUnit)).Cast<ConsumptionIntakeUnit>().ToList();
            var consumptionIntakeUnits = Enum.GetValues(typeof(ConcentrationUnit)).Cast<ConcentrationUnit>().ToList();
            var bodyWeightUnits = Enum.GetValues(typeof(BodyWeightUnit)).Cast<BodyWeightUnit>().ToList();
            foreach (var concentrationUnit in concentrationUnits) {
                foreach (var consumptionIntakeUnit in consumptionIntakeUnits) {
                    foreach (var bodyWeightUnit in bodyWeightUnits) {
                        var target = TargetUnit.CreateSingleValueDietaryExposureUnit(concentrationUnit, consumptionIntakeUnit, bodyWeightUnit, true);
                        Assert.IsTrue(!string.IsNullOrEmpty(target.GetShortDisplayName()));
                        target = TargetUnit.CreateSingleValueDietaryExposureUnit(concentrationUnit, consumptionIntakeUnit, bodyWeightUnit, false);
                        Assert.IsTrue(!string.IsNullOrEmpty(target.GetShortDisplayName()));
                    }
                }
            }
        }

        [TestMethod]
        [DataRow(ExternalExposureUnit.mgPerKgBWPerDay, "mg/kg bw/day")]
        [DataRow(ExternalExposureUnit.mgPerDay, "mg/day")]
        public void TargetUnit_TestGetDisplayName_ExternalUnits(ExternalExposureUnit exposureUnit, string expected) {
            var target = TargetUnit.FromExternalExposureUnit(exposureUnit);
            Assert.AreEqual(expected, target.GetShortDisplayName());
        }

        [TestMethod]
        [DataRow(DoseUnit.mgPerKg, BiologicalMatrix.Blood, ExpressionType.Lipids, "mg/kg lipids")]
        [DataRow(DoseUnit.mgPerL, BiologicalMatrix.Blood, ExpressionType.None, "mg/L")]
        public void TargetUnit_TestGetDisplayName_InternalUnits(
            DoseUnit doseUnit,
            BiologicalMatrix biologicalMatrix,
            ExpressionType expressionType,
            string expected
        ) {
            var target = TargetUnit.FromInternalDoseUnit(doseUnit, biologicalMatrix, expressionType);
            Assert.AreEqual(expected, target.GetShortDisplayName(TargetUnit.DisplayOption.AppendExpressionType));
        }
    }
}
