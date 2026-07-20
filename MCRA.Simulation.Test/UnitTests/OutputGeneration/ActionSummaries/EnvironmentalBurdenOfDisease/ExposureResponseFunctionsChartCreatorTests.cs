using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExposureResponseFunctionModels;
using MCRA.Simulation.Calculators.ExposureResponseFunctions;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.EnvironmentalBurdenOfDisease {

    [TestClass]
    public class ExposureResponseFunctionChartCreatorTests : ChartCreatorTestBase {

        [TestMethod]
        [DataRow(ExposureResponseType.Constant, "2.0", "1.0", "3.0", 2, 10)]
        [DataRow(ExposureResponseType.PerUnit, "2.0", "1.0", "3.0", 2, 10)]
        [DataRow(ExposureResponseType.PerDoubling, "2.0", "1.8", "2.2", 2, 10)]
        [DataRow(ExposureResponseType.Function, "x", "0.9 * x", "1.1 * x", 2, 10)]
        public void ExposureResponseFunctionChartCreator_TestCreate(
            ExposureResponseType erfType,
            string erfString,
            string erfStringLower,
            string erfStringUpper,
            double counterfactualValue,
            double maxExposure
        ) {
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Blood);
            var erf = new ExposureResponseFunction() {
                ExposureTarget = targetUnit.Target,
                EffectMetric = EffectMetric.OddsRatio,
                ExposureUnit = targetUnit.ExposureUnit,
                ExposureResponseSpecification = new NCalc.Expression(erfString),
                ExposureResponseSpecificationLower = new NCalc.Expression(erfStringLower),
                ExposureResponseSpecificationUpper = new NCalc.Expression(erfStringUpper),
                ERFUncertaintyDistribution = ExposureResponseSpecificationDistributionType.Uniform,
                ExposureResponseType = erfType,
                CounterFactualValue = counterfactualValue
            };

            var erfModel = ExposureResponseModelBuilder.Create(erf);
            var erfSummaryRecord = CreateFakeErfSummaryRecord(
                erfModel,
                maxExposure: maxExposure,
                numUncertaintyValues: 100,
                seed: 1
            );

            var section = new ExposureResponseFunctionSummarySection();
            var chartCreator = new ExposureResponseFunctionChartCreator(
                erfSummaryRecord,
                section.SectionId,
                5.0,
                95.0
            );

            RenderChart(chartCreator, $"TestCreateDirect_{erfType}");
        }

        private static ErfSummaryRecord CreateFakeErfSummaryRecord(
            IExposureResponseModel erfModel,
            double maxExposure,
            int numUncertaintyValues,
            int seed
        ) {
            var random = new McraRandomGenerator(seed);
            var counterfactualValue = erfModel.GetCounterFactualValue();
            var doseUnitAlignmentFactor = 1D;
            var useErfBins = erfModel.HasErfSubGroups;

            var numScatterPoints = 9;
            var xScatter = Enumerable.Range(0, numScatterPoints)
                .Select(i => counterfactualValue + (maxExposure - counterfactualValue) * i / (numScatterPoints - 1))
                .ToList();
            var yScatter = xScatter
                .Select(x => erfModel.Compute(x * doseUnitAlignmentFactor, useErfBins))
                .ToList();

            var exposureResponseDataPoints = xScatter
                .Zip(yScatter, (x, y) => new ExposureResponseDataPoint() {
                    Exposure = x,
                    ResponseValue = y
                })
                .ToList();

            var numGridPoints = 100;
            var xGrid = GriddingFunctions.Arange(
               erfModel.GetCounterFactualValue(),
               maxExposure,
               numGridPoints
            );
            var yGrid = xGrid
                .Select(x => erfModel.Compute(x * doseUnitAlignmentFactor, useErfBins))
                .ToList();

            var gridDataPoints = new UncertainDataPointCollection<double> {
                XValues = xGrid,
                ReferenceValues = yGrid
            };

            if (numUncertaintyValues > 0) {
                for (var j = 0; j < numUncertaintyValues; j++) {
                    erfModel.ResampleExposureResponseFunction(random, useErfBins);
                    var yUnc = xGrid
                        .Select(x => erfModel.Compute(x * doseUnitAlignmentFactor, useErfBins))
                        .ToList();
                    gridDataPoints.AddUncertaintyValues(yUnc);
                }
            }

            return new ErfSummaryRecord() {
                TargetUnit = erfModel.TargetUnit.GetShortDisplayName(),
                CounterfactualValue = counterfactualValue,
                ExposureResponseDataPoints = exposureResponseDataPoints,
                ExposureResponseGridDataPoints = gridDataPoints
            };
        }
    }
}