using MCRA.Data.Compiled.Objects;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class EefSummaryTableSection : SummarySection {

        public List<EefSummaryRecord> Records { get; set; }

        public void Summarize(List<ExposureEffectFunction> exposureEffectFunctions) {
            Records = exposureEffectFunctions
                .Select(r => {
                    var record = new EefSummaryRecord {
                        SubstanceName = r.Substance.Name,
                        SubstanceCode = r.Substance.Code,
                        Effect = r.Effect.Name,
                        TargetLevel = r.TargetLevel.GetDisplayName(),
                        ExposureRoute = r.ExposureRoute.GetDisplayName(),
                        BiologicalMatrix = r.BiologicalMatrix.GetDisplayName(),
                        DoseUnit = r.DoseUnit.GetShortDisplayName(),
                        ExpressionType = r.ExpressionType.GetDisplayName(),
                        EffectMetric = r.EffectMetric.GetDisplayName(),
                        Expression = r.Expression.ExpressionString
                    };
                    return record;
                })
                .ToList();
        }
    }
}
