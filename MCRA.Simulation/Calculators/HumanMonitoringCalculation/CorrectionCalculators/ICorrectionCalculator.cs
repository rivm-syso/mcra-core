using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.CorrectionCalculators {
    public interface ICorrectionCalculator {

        List<HumanMonitoringSampleSubstanceCollection> ComputeResidueCorrection(
           ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections
        );
    }
}
