using MCRA.General;
using MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation;
using MCRA.Utils;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ExposureResponseFunctionSummarySection : SummarySection {
        public override bool SaveTemporaryData => true;
        public double UncertaintyLowerLimit { get; set; } = 2.5;
        public double UncertaintyUpperLimit { get; set; } = 97.5;

        public List<ErfSummaryRecord> ErfSummaryRecords { get; set; }

        public void Summarize(
            ICollection<ExposureResponseResult> exposureResponseResults
        ) {
            var erfSummaryRecords = new List<ErfSummaryRecord>();
            foreach (var exposureResponseResult in exposureResponseResults) {
                var erf = exposureResponseResult.ExposureResponseFunctionModel;
                var targetUnit = erf.TargetUnit;
                var doseUnitAlignmentFactor = exposureResponseResult.ErfDoseUnitAlignmentFactor;
                var dataPoints = exposureResponseResult
                    .ExposureResponseResultRecords
                    .Select(r => new ExposureResponseDataPoint() {
                        Exposure = r.ExposureLevel,
                        ResponseValue = r.PercentileSpecificRisk
                    })
                    .ToList();
                var maxExposure = dataPoints.Max(r => r.Exposure);
                var exposureResponseGridDataPoints = new UncertainDataPointCollection<double>();
                var XValues = GriddingFunctions.Arange(
                    erf.GetCounterFactualValue(),
                    maxExposure,
                    n: 1000
                );
                var YValues = XValues
                    .Select(r => exposureResponseResult.ExposureResponseFunctionModel
                        .Compute(r * doseUnitAlignmentFactor, false)
                    );
                exposureResponseGridDataPoints.XValues = XValues;
                exposureResponseGridDataPoints.ReferenceValues = YValues;

                var record = new ErfSummaryRecord() {
                    ErfCode = erf.Code,
                    SubstanceName = erf.Substance.Name,
                    SubstanceCode = erf.Substance.Code,
                    EffectCode = erf.Effect.Code,
                    EffectName = erf.Effect.Name,
                    TargetLevel = erf.TargetUnit.TargetLevelType.GetDisplayName(),
                    ExposureRoute = erf.TargetUnit.ExposureRoute != ExposureRoute.Undefined
                        ? erf.TargetUnit.ExposureRoute.GetDisplayName()
                        : null,
                    BiologicalMatrix = erf.TargetUnit.BiologicalMatrix != BiologicalMatrix.Undefined
                        ? erf.TargetUnit.BiologicalMatrix.GetDisplayName()
                        : null,
                    TargetUnit = targetUnit.GetShortDisplayName(),
                    ExpressionType = erf.TargetUnit.ExpressionType != ExpressionType.None
                        ? erf.TargetUnit.ExpressionType.GetDisplayName()
                        : null,
                    EffectMetric = erf.EffectMetric.GetDisplayName(),
                    ExposureResponseType = erf.ExposureResponseType.GetDisplayName(),
                    ExposureResponseSpecification = erf.ExposureResponseFunction.ExposureResponseSpecification.ExpressionString,
                    ErSpecificationUncertaintyType = erf.ExposureResponseFunction.ERFUncertaintyDistribution != ExposureResponseSpecificationDistributionType.Constant
                        ? erf.ExposureResponseFunction.ERFUncertaintyDistribution.GetShortDisplayName()
                        : null,
                    ErSpecificationUncLower = erf.ExposureResponseFunction.ExposureResponseSpecificationLower?.ExpressionString.Length > 0
                        ? erf.ExposureResponseFunction.ExposureResponseSpecificationLower.ExpressionString
                        : null,
                    ErSpecificationUncUpper = erf.ExposureResponseFunction.ExposureResponseSpecificationUpper?.ExpressionString.Length > 0
                        ? erf.ExposureResponseFunction.ExposureResponseSpecificationUpper.ExpressionString
                        : null,
                    ErfDoseUnit = erf.TargetUnit.ExposureUnit.GetShortDisplayName(),
                    ErfDoseAlignmentFactor = doseUnitAlignmentFactor,
                    CounterfactualValue = erf.GetCounterFactualValue(),
                    HasSubgroups = erf.HasErfSubGroups,
                    ExposureResponseDataPoints = dataPoints,
                    ExposureResponseGridDataPoints = exposureResponseGridDataPoints
                };
                erfSummaryRecords.Add(record);
            }
            ErfSummaryRecords = erfSummaryRecords;
        }

        public void SummarizeUncertainty(
            ICollection<ExposureResponseResult> exposureResponseResults,
            double lowerBound,
            double upperBound
        ) {
            foreach (var resultRecord in exposureResponseResults) {
                var doseUnitAlignmentFactor = resultRecord.ErfDoseUnitAlignmentFactor;
                var XValues = ErfSummaryRecords
                    .Single(r => r.ErfCode == resultRecord.ExposureResponseFunctionModel.Code)
                    .ExposureResponseGridDataPoints.XValues;
                var YValues = XValues
                    .Select(r => resultRecord.ExposureResponseFunctionModel
                        .Compute(r * doseUnitAlignmentFactor, false)
                    );
                ErfSummaryRecords
                    .Single(r => r.ErfCode == resultRecord.ExposureResponseFunctionModel.Code)
                    .ExposureResponseGridDataPoints.AddUncertaintyValues(YValues);
            }
            UncertaintyLowerLimit = lowerBound;
            UncertaintyUpperLimit = upperBound;
        }
    }
}
