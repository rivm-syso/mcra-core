using MCRA.General;
using MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation;
using MCRA.Simulation.Calculators.ExposureResponseFunctions;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ExposureResponseFunctionSummarySection : SummarySection {

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
                var targetUnit = ebdRecords.First().TargetUnit;
                var doseUnitAlignmentFactor = ebdRecords.First()
                    .ExposureResponseResultRecord.ErfDoseUnitAlignmentFactor;
                var dataPoints = ebdRecords
                    .Select(r => new ExposureResponseDataPoint() {
                        Exposure = r.Exposure,
                        ResponseValue = r.ResponseValue
                    })
                    .ToList();
                var maxExposure = ebdRecords.Max(r => r.Exposure);
                var functionDataPoints = new List<ExposureResponseDataPoint>();
                for (double x = erf.Baseline; x <= maxExposure; x += 0.001 * erf.Baseline) {
                    var functionDataPoint = new ExposureResponseDataPoint() {
                        Exposure = x,
                        ResponseValue = exposureResponseFunctionModel.Compute(x * doseUnitAlignmentFactor)
                    };
                    functionDataPoints.Add(functionDataPoint);
                }
                ;
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
                    Baseline = erf.Baseline,
                    HasSubgroups = erf.HasErfSubGroups,
                    ExposureResponseDataPoints = dataPoints,
                    ExposureResponseChartDataPoints = functionDataPoints
                };
                erfSummaryRecords.Add(record);
            }

            ErfSummaryRecords = erfSummaryRecords;
        }
    }
}
