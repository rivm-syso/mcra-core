using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;
using MCRA.Simulation.Units;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.UrineCorrectionCalculation {
    public interface IUrineCorrectionCalculator {

        List<HumanMonitoringSampleSubstanceCollection> ComputeResidueCorrection(
           ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections,
           ConcentrationUnit targetUnit
        );
    }
}
