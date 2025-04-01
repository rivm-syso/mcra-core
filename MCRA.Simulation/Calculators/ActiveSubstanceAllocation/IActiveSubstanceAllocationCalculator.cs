using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.ActiveSubstanceAllocation {

    public interface IActiveSubstanceAllocationCalculator {
        List<SampleCompoundCollection> Allocate(
            ICollection<SampleCompoundCollection> sampleCompoundCollections,
            HashSet<Compound> activeSubstances,
            IRandom generator,
            CompositeProgressState progressState = null
        );
    }
}
