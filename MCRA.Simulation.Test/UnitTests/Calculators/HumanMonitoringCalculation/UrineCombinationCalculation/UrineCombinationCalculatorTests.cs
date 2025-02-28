using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.UrineCombinationCalculation;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.HumanMonitoringCalculation.UrineCombinationCalculation {
    [TestClass]
    public class UrineCombinationCalculatorTests {

        /// <summary>
        /// Test scenario:
        /// - urine, hair, blood
        /// - different sampling types for urine (spot, 24h, morning)
        /// - creatine standardised and non-standardised
        /// </summary>
        [TestMethod]
        public void UrineCombinationCalculator_DifferentUrineSamplingTypes_ShouldYieldOneCollectionPerExpressionType() {
            // Arrange
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = FakeIndividualsGenerator.Create(3, 1, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = FakeSubstancesGenerator.Create(2, null, null);
            var samplingMethods = new List<HumanMonitoringSamplingMethod> {
                FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod(BiologicalMatrix.Hair),
                FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod(BiologicalMatrix.Urine, "Spot"),
                FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod(BiologicalMatrix.Blood),
                FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod(BiologicalMatrix.Urine, "24h"),
                FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod(BiologicalMatrix.Urine, "Morning"),
            };

            var targetUnitUrineCreat = new TargetUnit(
               new ExposureTarget(BiologicalMatrix.Urine, ExpressionType.Creatinine),
               new ExposureUnitTriple(SubstanceAmountUnit.Micrograms, ConcentrationMassUnit.Grams, TimeScaleUnit.Unspecified)
            );
            var hbmIndividualDayCollections = FakeHbmIndividualDayConcentrationsGenerator
                .Create(individualDays, substances, samplingMethods, random);
            hbmIndividualDayCollections.Add(FakeHbmIndividualDayConcentrationsGenerator
                .Create(individualDays, substances, samplingMethods[1], targetUnitUrineCreat, random));
            hbmIndividualDayCollections.Add(FakeHbmIndividualDayConcentrationsGenerator
                .Create(individualDays, substances, samplingMethods[3], targetUnitUrineCreat, random));
            hbmIndividualDayCollections.Add(FakeHbmIndividualDayConcentrationsGenerator
                .Create(individualDays, substances, samplingMethods[4], targetUnitUrineCreat, random));

            // Act
            var result = UrineCombinationCalculator.Combine(hbmIndividualDayCollections, individualDays);

            // Assert
            Assert.AreEqual(2, result.Count(r => r.Target.BiologicalMatrix.IsUrine()),
                "Expected two combined urine collections, one for creatinine standardised and one for non-standardised");
            Assert.AreEqual(individualDays.Count, result
                .SelectMany(r => r.HbmIndividualDayConcentrations
                .Select(h => (h.Individual, h.Day)))
                .Distinct()
                .Count());
            var combinedSamplingMethods = result
                .Where(r => r.Target.BiologicalMatrix.IsUrine())
                .SelectMany(r => r.HbmIndividualDayConcentrations)
                .SelectMany(h => h.ConcentrationsBySubstance.Values)
                .Select(c => (c.Substance, c.SourceSamplingMethods))
                .ToList();
            Assert.IsTrue(combinedSamplingMethods.All(c => c.SourceSamplingMethods.Count == 3));

            double getExposureCmp0(HbmIndividualDayCollection collection) {
                return collection
                    .HbmIndividualDayConcentrations
                    .FirstOrDefault(h => h.SimulatedIndividualId == 0 && h.Day == "0")
                    .ConcentrationsBySubstance[substances[0]]
                    .Exposure;
            }
            void AssertCombinedConcentrations(
                HbmIndividualDayCollection collectionSpot,
                HbmIndividualDayCollection collection24h,
                HbmIndividualDayCollection collectionMorning,
                HbmIndividualDayCollection collectionUrineOut
            ) {

                var exposureInSpot = getExposureCmp0(collectionSpot);
                var exposureIn24h = getExposureCmp0(collection24h);
                var exposureInMorning = getExposureCmp0(collectionMorning);
                var exposureOut = getExposureCmp0(collectionUrineOut);

                var residueOutExpected = (new [] { exposureInSpot, exposureIn24h, exposureInMorning }).Average();

                Assert.AreEqual(residueOutExpected, exposureOut, 0.001);
            }

            AssertCombinedConcentrations(
                hbmIndividualDayCollections[1],
                hbmIndividualDayCollections[3],
                hbmIndividualDayCollections[4],
                result.FirstOrDefault(r => r.Target.BiologicalMatrix.IsUrine()
                    && r.Target.ExpressionType == ExpressionType.None)
            );
            AssertCombinedConcentrations(
                hbmIndividualDayCollections[5],
                hbmIndividualDayCollections[6],
                hbmIndividualDayCollections[7],
                result.FirstOrDefault(r => r.Target.BiologicalMatrix.IsUrine()
                    && r.Target.ExpressionType == ExpressionType.Creatinine)
            );
        }

        /// <summary>
        /// Null test: urine combination calculator should do nothing when there are no multiple sampling methods
        /// for urine, it should return the original collection objects.
        /// </summary>
        [TestMethod]
        public void UrineCombinationCalculator_NoMultipleUrineSamplingMethods_ShouldYieldSameInputObjects() {
            // Arrange
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = FakeIndividualsGenerator.Create(3, 1, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = FakeSubstancesGenerator.Create(2, null, null);
            var samplingMethods = new List<HumanMonitoringSamplingMethod> {
                FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod(BiologicalMatrix.Hair),
                FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod(BiologicalMatrix.Blood),
                FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod(BiologicalMatrix.Urine),
            };
            var hbmIndividualDayCollections = FakeHbmIndividualDayConcentrationsGenerator
               .Create(individualDays, substances, samplingMethods, random);

            // Act
            var result = UrineCombinationCalculator.Combine(hbmIndividualDayCollections, individualDays);

            // Assert
            Assert.AreEqual(hbmIndividualDayCollections.Count, result.Count,
                "Expected same number of collections out as collections in");
            Assert.AreEqual(individualDays.Count, result
                .SelectMany(r => r.HbmIndividualDayConcentrations
                .Select(h => (h.Individual, h.Day)))
                .Distinct()
                .Count());

            foreach (var samlingMethod in samplingMethods) {
                var matrix = samlingMethod.BiologicalMatrix;
                Assert.AreSame(
                    hbmIndividualDayCollections.First(c => c.Target.BiologicalMatrix == matrix),
                    result.First(c => c.Target.BiologicalMatrix == matrix));
            }
        }
    }
}
