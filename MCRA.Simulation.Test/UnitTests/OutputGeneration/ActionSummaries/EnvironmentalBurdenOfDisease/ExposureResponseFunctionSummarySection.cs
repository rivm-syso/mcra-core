using CommandLine;
using MCRA.General;
using MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation;
using MCRA.Simulation.Calculators.ExposureResponseFunctionModels;
using MCRA.Simulation.Calculators.ExposureResponseFunctions;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.EnvironmentalBurdenOfDisease {

    [TestClass]
    public class ExposureResponseFunctionSummarySectionTests : SectionTestBase {

        /// <summary>
        /// Summarize
        /// </summary>
        [TestMethod]
        [DataRow(ExposureResponseType.Constant, "2.0", "1.0", "3.0")]
        [DataRow(ExposureResponseType.PerUnit, "2.0", "1.0", "3.0")]
        [DataRow(ExposureResponseType.PerDoubling, "2.0", "1.0", "3.0")]
        [DataRow(ExposureResponseType.Function, "x", "0.9 * x", "1.1 * x")]
        public void ExposureResponseFunctionSummarySection_TestSummarize(
            ExposureResponseType erfType,
            string erfString,
            string erfStringLower,
            string erfStringUpper
        ) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var effect = FakeEffectsGenerator.Create(1).First();
            var substance = FakeSubstancesGenerator.Create(1).First();
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Blood);
            var exposureResponseFunction = FakeExposureResponseFunctionsGenerator
                .FakeExposureResponseFunction(
                    $"ERF_{effect.Code}_{substance.Code}_{erfType.GetShortDisplayName()}",
                    substance,
                    effect,
                    targetUnit.Target,
                    targetUnit.ExposureUnit,
                    erfType,
                    erfString,
                    erfStringLower,
                    erfStringUpper,
                    EffectMetric.OddsRatio,
                    2,
                    random
                );
            var exposureResponseFunctionModels = ExposureResponseModelBuilder
                .Create(exposureResponseFunction)
                .Cast<IExposureResponseModel>();

            var hbmIndividualConcentrations = FakeHbmDataGenerator
                .MockHumanMonitoringIndividualConcentrations(individuals, [substance]);
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
                [exposureResponseFunctionModels],
                1
             );

            var section = new ExposureResponseFunctionSummarySection();

            section.Summarize(exposureResponseResults, ExposureGroupingMethod.CustomBins);

            Assert.HasCount(1, section.ErfSummaryRecords);
        }
    }
}


