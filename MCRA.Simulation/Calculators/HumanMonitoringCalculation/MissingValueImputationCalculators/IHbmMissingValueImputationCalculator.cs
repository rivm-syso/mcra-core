using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.MissingValueImputationCalculators {
    public interface IHbmMissingValueImputationCalculator {

        List<HumanMonitoringSampleSubstanceCollection> ImputeMissingValues(
           ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections,
           double missingValueCutOff,
           IRandom random
        );
    }
}
