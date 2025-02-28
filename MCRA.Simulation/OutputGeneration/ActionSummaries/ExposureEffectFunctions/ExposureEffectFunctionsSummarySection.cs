using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ExposureEffectFunctionsSummarySection : ActionSummarySectionBase {

        public List<ExposureEffectFunctionSummaryRecord> Records { get; set; }

        public void Summarize(List<ExposureEffectFunction> exposureEffectFunctions) {
            Records = exposureEffectFunctions
                .Select(r => {
                    var record = new ExposureEffectFunctionSummaryRecord {
                        SubstanceName = r.Substance.Name,
                        SubstanceCode = r.Substance.Code,
                        Effect = r.Effect.Name,
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
                        ExposureResponesType = r.ExposureResponseType.GetDisplayName(),
                        ExposureResponseSpecification = r.ExposureResponseSpecification.ExpressionString,
                        Baseline = r.Baseline
                    };
                    return record;
                })
                .ToList();
        }
    }
}
