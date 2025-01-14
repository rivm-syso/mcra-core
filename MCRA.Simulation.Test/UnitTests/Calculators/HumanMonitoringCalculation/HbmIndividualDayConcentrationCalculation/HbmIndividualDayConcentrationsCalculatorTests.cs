using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation {
    [TestClass]
    public class HbmIndividualDayConcentrationsCalculatorTests {

        [TestMethod]
        public void HbmIndividualDayConcentrationsCalculator_Test() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = FakeIndividualsGenerator.CreateSimulated(25, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = FakeSubstancesGenerator.Create(6);
            var activeSubstances = substances.Take(3).ToList();

            var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod(BiologicalMatrix.Blood);
            var hbmSampleSubstanceCollection = FakeHbmDataGenerator
                .FakeHbmSampleSubstanceCollections(individualDays, substances, samplingMethod)
                .First();

            var calculator = new HbmIndividualDayConcentrationsCalculator();
            var result = calculator.Calculate(
                hbmSampleSubstanceCollection,
                simulatedIndividualDays: individualDays
                    .Select((r, ix) => new SimulatedIndividualDay(r.SimulatedIndividual) {
                        SimulatedIndividualDayId = r.SimulatedIndividualDayId,
                        Day = r.Day
                    })
                    .ToList(),
                substances: activeSubstances);
            var observedSubstances = result.HbmIndividualDayConcentrations
                .SelectMany(r => r.ConcentrationsBySubstance.Keys)
                .Distinct()
                .ToList();
            CollectionAssert.AreEquivalent(activeSubstances, observedSubstances);
        }
    }
}
