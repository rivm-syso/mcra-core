using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class CoExposureTotalDistributionSubstanceSection : CoExposureDistributionSubstanceSectionBase {
        public void SummarizeCoExposure<T>(
            ICollection<T> aggregateExposures,
            ICollection<Compound> substances,
            TargetUnit targetUnit
        ) where T : AggregateIndividualExposureBase {
            summarize(aggregateExposures, substances, targetUnit);
        }
    }
}
