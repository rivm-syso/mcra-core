using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class CoExposureTotalDistributionSubstanceSection : CoExposureDistributionSubstanceSectionBase {
        public void SummarizeCoExposure(
            ICollection<AggregateIndividualExposure> aggregateExposures,
            ICollection<Compound> substances,
            TargetUnit targetUnit
        ) {
            summarize(aggregateExposures, substances, targetUnit);
        }
    }
}
