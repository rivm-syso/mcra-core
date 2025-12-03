using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.BodIndicatorModels;
using MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation;
using MCRA.Simulation.Calculators.ExposureResponseFunctionModels;
using MCRA.Simulation.Calculators.ExposureResponseFunctionModels.CounterFactualValueModels;
using MCRA.Simulation.Calculators.ExposureResponseFunctionModels.ExposureResponseSpecificationModels;
using MCRA.Simulation.Calculators.ExposureResponseFunctions;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Calculators.EnvironmentalBurdenOfDiseaseCalculation {
    [TestClass]
    public class EnvironmentalBurdenOfDiseaseTests {
        [TestMethod]
        public void EnvironmentalBurdenOfDiseaseTests_TestConversionsCompute() {
            var exposureResponseCalculator = new ExposureResponseCalculator(
                ExposureGroupingMethod.CustomBins,
                [5, 10, 25, 50, 75, 90, 95],
                WithinBinExposureRepresentationMethod.AverageInBin
            );
            var substances = FakeSubstancesGenerator.Create(1);
            var erf = new ExposureResponseFunction() {
                EffectMetric = EffectMetric.OddsRatio,
                ExposureTarget = new ExposureTarget(),
                Substance = substances.First(),
                ExposureUnit = new ExposureUnitTriple(SubstanceAmountUnit.Milligrams, ConcentrationMassUnit.Liter),
                ExposureResponseType = ExposureResponseType.PerDoubling,
                CounterFactualValue = 2,
                ExposureResponseSpecification = new NCalc.Expression("10"),

            };
            var seed = 1;
            var n = 100;
            var random = new McraRandomGenerator(seed);
            var individuals = FakeIndividualsGenerator.Create(n, 2, random, useSamplingWeights: false);
            var targetExposures = FakeTargetExposuresGenerator.MockIndividualExposures(
                individuals,
                substances,
                random,
                [.. Enumerable.Repeat(3d, n)],
                [.. Enumerable.Repeat(.3, n)]
            );

            var exposures = new Dictionary<ExposureTarget, (List<ITargetIndividualExposure> Exposures, TargetUnit Unit)> {
                [new ExposureTarget()] = ([.. targetExposures], new TargetUnit() { ExposureUnit = new ExposureUnitTriple(SubstanceAmountUnit.Milligrams, ConcentrationMassUnit.Liter) })
            };
            var exposureResponseFunctionModels = new List<ExposureResponseFunction>() { erf }
                .Select(ExposureResponseModelBuilder.Create)
                .Cast<IExposureResponseModel>();
            var exposureResponseResults = exposureResponseCalculator.ComputeFromTargetIndividualExposures(
                exposures,
                [.. exposureResponseFunctionModels],
                seed
            );

            var calculator = new EnvironmentalBurdenOfDiseaseCalculator(
                BodApproach.TopDown,
                EnvironmentalBodStandardisationMethod.PER10K
            );

            var conversion = new BodIndicatorConversion() {
                FromIndicator = BodIndicator.DALY,
                ToIndicator = BodIndicator.Deaths,
                Value = 2
            };
            var bod = new DerivedBurdenOfDisease() {
                BodIndicator = BodIndicator.DALY,
                Conversions = [conversion],
                BodUncertaintyDistribution = BodIndicatorDistributionType.Constant,
                Value = 2,
            };

            var bodIndicatorModel = BodIndicatorValueCalculatorFactory.Create(bod);

            var results = calculator.Compute(
                exposureResponseResults.First(),
                bodIndicatorModel,
                new Population() { Size = 10000 }
            );

            //Set Bod conversion factor to 20
            var totalBod1 = results.EnvironmentalBurdenOfDiseaseResultBinRecords.First().TotalBod;
            bod.Value = 20;
            bodIndicatorModel = BodIndicatorValueCalculatorFactory.Create(bod);
            results = calculator.Compute(
               exposureResponseResults.First(),
               bodIndicatorModel,
               new Population() { Size = 10000 }
           );
            var totalBod2 = results.EnvironmentalBurdenOfDiseaseResultBinRecords.First().TotalBod;

            Assert.AreEqual(10 * totalBod1, totalBod2);
        }
    }
}
