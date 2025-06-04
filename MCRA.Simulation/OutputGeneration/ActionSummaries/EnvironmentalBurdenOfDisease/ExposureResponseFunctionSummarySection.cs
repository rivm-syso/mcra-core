using MCRA.General;
using MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation;
using MCRA.Simulation.Calculators.ExposureResponseFunctions;
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
            ICollection<IExposureResponseFunctionModel> exposureResponseFunctionModels,
            List<EnvironmentalBurdenOfDiseaseResultRecord> environmentalBurdenOfDiseases
        ) {
            var erfSummaryRecords = new List<ErfSummaryRecord>();
            foreach (var exposureResponseFunctionModel in exposureResponseFunctionModels) {
                var erf = exposureResponseFunctionModel.ExposureResponseFunction;
                var ebdRecords = environmentalBurdenOfDiseases
                    .Where(r => r.ExposureResponseFunction == erf)
                    .ToList();
                var ebdBinRecords = ebdRecords
                    .SelectMany(r => r.EnvironmentalBurdenOfDiseaseResultBinRecords);
                var targetUnit = erf.TargetUnit;
                var doseUnitAlignmentFactor = ebdBinRecords.First()
                    .ExposureResponseResultRecord.ErfDoseUnitAlignmentFactor;
                var dataPoints = ebdBinRecords
                    .Select(r => new ExposureResponseDataPoint() {
                        Exposure = r.Exposure,
                        ResponseValue = r.ResponseValue
                    })
                    .ToList();
                var maxExposure = ebdBinRecords.Max(r => r.Exposure);
                var exposureResponseGridDataPoints = new UncertainDataPointCollection<double>();
                var XValues = GriddingFunctions.Arange(
                    erf.CounterfactualValue,
                    maxExposure,
                    n: 1000);
                var YValues = XValues
                    .Select(r => exposureResponseFunctionModel.Compute(r * doseUnitAlignmentFactor));
                exposureResponseGridDataPoints.XValues = XValues;
                exposureResponseGridDataPoints.ReferenceValues = YValues;

                var record = new ErfSummaryRecord() {
                    ErfCode = erf.Code,
                    SubstanceName = erf.Substance.Name,
                    SubstanceCode = erf.Substance.Code,
                    EffectCode = erf.Effect.Code,
                    EffectName = erf.Effect.Name,
                    TargetLevel = erf.TargetLevel.GetDisplayName(),
                    ExposureRoute = erf.ExposureRoute != ExposureRoute.Undefined
                        ? erf.ExposureRoute.GetDisplayName()
                        : null,
                    BiologicalMatrix = erf.BiologicalMatrix != BiologicalMatrix.Undefined
                        ? erf.BiologicalMatrix.GetDisplayName()
                        : null,
                    TargetUnit = targetUnit.GetShortDisplayName(),
                    ExpressionType = erf.ExpressionType != ExpressionType.None
                        ? erf.ExpressionType.GetDisplayName()
                        : null,
                    EffectMetric = erf.EffectMetric.GetDisplayName(),
                    ExposureResponseType = erf.ExposureResponseType.GetDisplayName(),
                    ExposureResponseSpecification = erf.ExposureResponseSpecification.ExpressionString,
                    ExposureResponseSpecificationLower = erf.ExposureResponseSpecificationLower.ExpressionString.Length != 0
                        ? erf.ExposureResponseSpecificationLower.ExpressionString
                        : null,
                    ExposureResponseSpecificationUpper = erf.ExposureResponseSpecificationUpper.ExpressionString.Length != 0
                        ? erf.ExposureResponseSpecificationUpper.ExpressionString
                        : null,
                    ErfDoseUnit = erf.DoseUnit.GetShortDisplayName(),
                    ErfDoseAlignmentFactor = doseUnitAlignmentFactor,
                    CounterfactualValue = erf.CounterfactualValue,
                    HasSubgroups = erf.HasErfSubGroups,
                    ExposureResponseDataPoints = dataPoints,
                    ExposureResponseGridDataPoints = exposureResponseGridDataPoints
                };
                erfSummaryRecords.Add(record);
            }
            ErfSummaryRecords = erfSummaryRecords;
        }

        public void SummarizeUncertainty(
           ICollection<IExposureResponseFunctionModel> exposureResponseFunctionModels,
           List<EnvironmentalBurdenOfDiseaseResultRecord> environmentalBurdenOfDiseases,
           double lowerBound,
           double upperBound
        ) {
            foreach (var exposureResponseFunctionModel in exposureResponseFunctionModels) {
                var erf = exposureResponseFunctionModel.ExposureResponseFunction;
                var doseUnitAlignmentFactor = environmentalBurdenOfDiseases
                    .Where(r => r.ExposureResponseFunction == erf)
                    .SelectMany(r => r.EnvironmentalBurdenOfDiseaseResultBinRecords)
                    .First()
                    .ExposureResponseResultRecord.ErfDoseUnitAlignmentFactor;
                var XValues = ErfSummaryRecords
                    .Single(r => r.ErfCode == erf.Code)
                    .ExposureResponseGridDataPoints.XValues;
                var YValues = XValues
                    .Select(r => exposureResponseFunctionModel.Compute(r * doseUnitAlignmentFactor));
                ErfSummaryRecords
                    .Single(r => r.ErfCode == erf.Code)
                    .ExposureResponseGridDataPoints.AddUncertaintyValues(YValues);
            }
            UncertaintyLowerLimit = lowerBound;
            UncertaintyUpperLimit = upperBound;
        }
    }
}
