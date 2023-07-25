using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;
using MCRA.Simulation.Units;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.BloodCorrectionCalculation {
    public interface IBloodCorrectionCalculator {
        List<HumanMonitoringSampleSubstanceCollection> ComputeTotalLipidCorrection(
            ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections,
            ConcentrationUnit targetUnit,
            TimeScaleUnit timeScaleUnit,
            TargetUnitsModel substanceTargetUnits
        );
    }
}
