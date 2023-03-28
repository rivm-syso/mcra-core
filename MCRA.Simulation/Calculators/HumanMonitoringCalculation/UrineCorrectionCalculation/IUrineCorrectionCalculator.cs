using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.UrineCorrectionCalculation {
    public interface IUrineCorrectionCalculator {

        List<HumanMonitoringSampleSubstanceCollection> ComputeResidueCorrection(
           ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections,
           ConcentrationUnit targetUnit,
           TimeScaleUnit timeScaleUnit,
           Dictionary<TargetUnit, HashSet<Compound>> substanceTargetUnits
        );
    }
}
