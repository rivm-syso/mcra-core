using MCRA.General;
using MCRA.General.UnitDefinitions.Enums;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.BloodCorrectionCalculation {
    public interface IBloodCorrectionCalculator {

        List<HumanMonitoringSampleSubstanceCollection> ComputeTotalLipidCorrection ( 
            ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections,
            ConcentrationUnit targetUnit,
            BiologicalMatrix defaultCompartment,
            CompartmentUnitCollector compartmentUnitCollector
        );
    }
}
