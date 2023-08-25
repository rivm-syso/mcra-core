using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmBiologicalMatrixConcentrationConversion;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation {
    [TestClass]
    public class HbmIndividualDayConcentrationsCalculatorTests {
        [TestMethod]
        public void HbmIndividualDayConcentrationsCalculator_Test() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = MockSubstancesGenerator.Create(6);
            var activeSubstances = substances.Take(3).ToList();
            var targetUnitsModel = MockTargetUnitsModelGenerator.Create(substances);
            var biologicalMatrix = BiologicalMatrix.Blood;
            var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod(biologicalMatrix);
            var hbmSampleSubstanceCollections = FakeHbmDataGenerator
                .FakeHbmSampleSubstanceCollections(individualDays, substances, samplingMethod);

            var calculator = new HbmMainIndividualDayConcentrationsCalculator(biologicalMatrix);
            var result = calculator.Calculate(
                hbmSampleSubstanceCollections: hbmSampleSubstanceCollections,
                individualDays: individualDays
                    .Select(r => new IndividualDay() {
                        Individual = r.Individual,
                        IdDay = r.Day
                    })
                    .ToList(),
                substances: activeSubstances,
                targetUnits: targetUnitsModel.SubstanceTargetUnits.Keys
            );
            var observedSubstances = result.First().HbmIndividualDayConcentrationCollections
                .SelectMany(r => r.ConcentrationsBySubstance.Keys)
                .Distinct()
                .ToList();
            CollectionAssert.AreEquivalent(activeSubstances, observedSubstances);
        }
    }
}
