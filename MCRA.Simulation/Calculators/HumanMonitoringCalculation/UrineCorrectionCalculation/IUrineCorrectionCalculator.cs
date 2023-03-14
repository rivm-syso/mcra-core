using MCRA.General;
using MCRA.General.UnitDefinitions.Enums;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.UrineCorrectionCalculation {
    public interface IUrineCorrectionCalculator {

        List<HumanMonitoringSampleSubstanceCollection> ComputeResidueCorrection(
           ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections,
           ConcentrationUnit targetUnit,
           string defaultCompartment,
           CompartmentUnitCollector compartmentUnitCollector
        );
    }
}
