using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ExposureResponseFunctionSummarySection : SummarySection {

        public List<ErfSummaryRecord> ErfSummaryRecords { get; set; }

        public void Summarize(
            List<ExposureResponseFunction> exposureResponseFunctions,
            List<EnvironmentalBurdenOfDiseaseResultRecord> environmentalBurdenOfDiseases
        ) {

            var erfSummaryRecords = new List<ErfSummaryRecord>();
            foreach (var erf in exposureResponseFunctions) {
                var ebdRecords = environmentalBurdenOfDiseases
                    .Where(r => r.ExposureResponseFunction == erf)
                    .ToList();
                var dataPoints = ebdRecords
                    .Select(r => new ExposureResponseDataPoint() {
                        Exposure = r.Exposure,
                        ResponseValue = r.ResponseValue
                    })
                    .ToList();
                var targetUnit = ebdRecords.First().TargetUnit;
                var doseUnitAlignmentFactor = ebdRecords.First()
                    .ExposureResponseResultRecord.ErfDoseUnitAlignmentFactor;
                var record = new ErfSummaryRecord() {
                    ExposureResponseFunction = erf,
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
                    ErfDoseUnit = erf.DoseUnit.GetShortDisplayName(),
                    ErfDoseAlignmentFactor = doseUnitAlignmentFactor,
                    Baseline = erf.Baseline,
                    HasSubgroups = erf.HasErfSubGroups(),
                    ExposureResponseDataPoints = dataPoints
                };
                erfSummaryRecords.Add(record);
            }

            ErfSummaryRecords = erfSummaryRecords;
        }
    }
}
