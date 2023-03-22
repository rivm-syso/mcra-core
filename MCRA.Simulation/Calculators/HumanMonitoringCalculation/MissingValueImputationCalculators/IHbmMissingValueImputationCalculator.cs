using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.MissingValueImputationCalculators {
    public interface IHbmMissingValueImputationCalculator {

        List<HumanMonitoringSampleSubstanceCollection> ImputeMissingValues(
           ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections,
           double missingValueCutOff,
           int randomSeed
        );
    }
}
