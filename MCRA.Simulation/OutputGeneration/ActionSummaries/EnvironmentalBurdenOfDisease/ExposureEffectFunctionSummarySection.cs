using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ExposureEffectFunctionSummarySection : SummarySection {

        public List<AttributableBodSummaryRecord> Records { get; set; }
        public List<ExposureEffectFunctionSummaryRecord> EefRecords { get; set; }
        public List<ExposureEffectFunction> ExposureEffectFunctions { get; set; }

        public void Summarize(
            List<EnvironmentalBurdenOfDiseaseResultRecord> environmentalBurdenOfDiseases
        ) {
            Records = environmentalBurdenOfDiseases
                .Select(r => new AttributableBodSummaryRecord {
                    ExposureResponseFunctionCode = r.ExposureEffectFunction.Code,
                    Exposure = r.Exposure,
                    Ratio = r.Ratio
                })
                .ToList();

            var exposureEffectFunctions = environmentalBurdenOfDiseases
                .Select(r => r.ExposureEffectFunction)
                .Distinct()
                .ToList();

            ExposureEffectFunctions = exposureEffectFunctions;

            EefRecords = exposureEffectFunctions
                .Select(r => new ExposureEffectFunctionSummaryRecord() {
                    ExposureResponseFunctionCode = r.Code,
                    SubstanceName = r.Substance.Name,
                    SubstanceCode = r.Substance.Code,
                    EffectCode = r.Effect.Code,
                    EffectName = r.Effect.Name,
                    TargetLevel = r.TargetLevel.GetDisplayName(),
                    ExposureRoute = r.ExposureRoute != ExposureRoute.Undefined
                    ? r.ExposureRoute.GetDisplayName()
                    : null,
                    BiologicalMatrix = r.BiologicalMatrix != BiologicalMatrix.Undefined
                    ? r.BiologicalMatrix.GetDisplayName()
                    : null,
                    DoseUnit = r.DoseUnit.GetShortDisplayName(),
                    ExpressionType = r.ExpressionType != ExpressionType.None
                    ? r.ExpressionType.GetDisplayName()
                    : null,
                    EffectMetric = r.EffectMetric.GetDisplayName(),
                    ExposureResponseType = r.ExposureResponseType.GetDisplayName(),
                    ExposureResponseSpecification = r.ExposureResponseSpecification.ExpressionString,
                    Baseline = r.Baseline
                })
                .ToList();
        }
    }
}
