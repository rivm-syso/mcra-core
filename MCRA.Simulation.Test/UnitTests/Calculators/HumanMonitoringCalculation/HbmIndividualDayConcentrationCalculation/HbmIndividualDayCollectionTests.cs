using MCRA.General;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation {
    [TestClass]
    public class HbmIndividualDayCollectionTests {

        [TestMethod]
        public void HbmIndividualDayCollection_Clone_ShouldReturnNewObjectInstance() {
            // Arrange
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(3);
            var individuals = FakeIndividualsGenerator.Create(1, 1, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod();
            var targetUnit = TargetUnit
                .FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Urine, ExpressionType.Creatinine);

            var originalndividualDayCollection = FakeHbmIndividualDayConcentrationsGenerator
                .Create(individualDays, substances, samplingMethod, targetUnit, random);
            var originalIndividualDayConcentration = originalndividualDayCollection.HbmIndividualDayConcentrations.First();
            var originalIndividualDay = "Original";
            originalIndividualDayConcentration.Day = originalIndividualDay;

            // Act
            var clonedIndividualDayCollection = originalndividualDayCollection.Clone();
            var clonedIndividualDayConcentration = clonedIndividualDayCollection.HbmIndividualDayConcentrations.First();
            var clonedIndividualDay = "Clone";
            clonedIndividualDayConcentration.Day = clonedIndividualDay;

            // Assert
            Assert.IsFalse(ReferenceEquals(clonedIndividualDayCollection, originalndividualDayCollection));
            Assert.IsFalse(ReferenceEquals(clonedIndividualDayConcentration, originalndividualDayCollection));
            Assert.AreEqual(originalIndividualDay, originalIndividualDayConcentration.Day);
            Assert.AreEqual(clonedIndividualDay, clonedIndividualDayConcentration.Day);
            Assert.AreNotEqual(originalIndividualDayConcentration.Day, clonedIndividualDayConcentration.Day);
        }
    }
}
