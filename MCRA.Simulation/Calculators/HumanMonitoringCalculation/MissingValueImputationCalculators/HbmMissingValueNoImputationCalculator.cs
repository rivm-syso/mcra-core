using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.MissingValueImputationCalculators {
    public class HbmMissingValueNoImputationCalculator : IHbmMissingValueImputationCalculator {

        public List<HumanMonitoringSampleSubstanceCollection> ImputeMissingValues(
            ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections,
            double missingValueCutOff,
            IRandom random
        ) {
            return hbmSampleSubstanceCollections.ToList();
        }
    }
}
