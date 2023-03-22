using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.MissingValueImputationCalculators {
    public class HbmMissingValueNoImputationCalculator : IHbmMissingValueImputationCalculator {

        public List<HumanMonitoringSampleSubstanceCollection> ImputeMissingValues(
            ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections,
            double missingValueCutOff,
            int randomSeed
        ) {
            return hbmSampleSubstanceCollections.ToList();
        }
    }
}
