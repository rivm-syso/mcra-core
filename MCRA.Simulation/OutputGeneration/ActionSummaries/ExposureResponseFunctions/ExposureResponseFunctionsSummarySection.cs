using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ExposureResponseFunctionsSummarySection : ActionSummarySectionBase {

        public List<ExposureResponseFunctionSummaryRecord> Records { get; set; }

        public void Summarize(List<ExposureResponseFunction> exposureResponseFunctions) {
            Records = exposureResponseFunctions
                .Select(r => {
                    var record = new ExposureResponseFunctionSummaryRecord {
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
                    };
                    return record;
                })
                .ToList();
        }
    }
}
