using MCRA.General;
using MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation;
using MCRA.Simulation.Calculators.ExposureResponseFunctions;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.EnvironmentalBurdenOfDisease {

    [TestClass]
    public class EnvironmentalBurdenOfDiseaseSectionTests : SectionTestBase {

        /// <summary>
        /// Summarize
        /// </summary>
        [TestMethod]
        public void EnvironmentalBurdenOfDisease_Test1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var effects = FakeEffectsGenerator.Create(2);
            var substances = FakeSubstancesGenerator.Create(3);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Blood);
            var exposureResponseFunctionModels = FakeExposureResponseFunctionsGenerator
                .FakeExposureResponseFunctionModel(
                    substances,
                    effects,
                    targetUnit.Target,
                    targetUnit.ExposureUnit,
                    random.Next()
                );

            var ebdResultRecords = generateFakeEbdResultRecords(
                exposureResponseFunctionModels,
                random
            );

            var section = new EnvironmentalBurdenOfDiseaseSummarySection();

            section.Summarize(ebdResultRecords, true);
            Assert.HasCount(6, section.Records);
        }

        private static List<EnvironmentalBurdenOfDiseaseResultRecord> generateFakeEbdResultRecords(
            List<IExposureResponseModel> exposureResponseFunctionModels,
            McraRandomGenerator random
        ) {
            var individuals = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var substances = exposureResponseFunctionModels.Select(r => r.Substance).Distinct().ToList();
            var effects = exposureResponseFunctionModels.Select(r => r.Effect).Distinct().ToList();
            var targetUnit = exposureResponseFunctionModels.Select(r => r.TargetUnit).Distinct().First();

            var populationCharacteristicTypes = exposureResponseFunctionModels
                .Select(r => r.PopulationCharacteristic)
                .Distinct()
                .ToList();
            var population = FakePopulationsGenerator
                .Create(1, populationCharacteristicTypes, seed: 1).First();
            var burdenOfDiseases = effects.Select(r => FakeBurdenOfDiseasesGenerator.Create(r, population)).ToList();
            var bodIndicatorModels = FakeBurdenOfDiseasesGenerator
                .FakeBodIndicatorValueModel(burdenOfDiseases, population);

            var hbmIndividualConcentrations = FakeHbmDataGenerator
                .FakeHbmIndividualConcentrations(individuals, substances);
            var hbmIndividualCollections = new Dictionary<ExposureTarget, (List<ITargetIndividualExposure> Exposures, TargetUnit Unit)> {
                { targetUnit.Target, (hbmIndividualConcentrations.Cast<ITargetIndividualExposure>().ToList(), targetUnit) }
            };

            var erCalculator = new ExposureResponseCalculator(
                ExposureGroupingMethod.CustomBins,
                [5.0, 50.0, 95.0],
                WithinBinExposureRepresentationMethod.AverageInBin
            );
            var exposureResponseResults = erCalculator.ComputeFromTargetIndividualExposures(
                hbmIndividualCollections,
                exposureResponseFunctionModels,
                1
             );

            var ebdCalculator = new EnvironmentalBurdenOfDiseaseCalculator(
                BodApproach.TopDown,
                EnvironmentalBodStandardisationMethod.PER1M,
                true
            );
            var ebdResultRecords = ebdCalculator.Compute(bodIndicatorModels, exposureResponseResults);

            return ebdResultRecords;
        }
    }
}


