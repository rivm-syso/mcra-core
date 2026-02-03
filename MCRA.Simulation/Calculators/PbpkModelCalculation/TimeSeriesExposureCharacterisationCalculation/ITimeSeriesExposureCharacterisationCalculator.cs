using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation.Calculators.PbpkModelCalculation.TargetExposureFromTimeSeriesCalculation {

    /// <summary>
    /// Interface for classes to compute substance target exposure estimates
    /// from PBK simulation output timeseries. For example, compute (chronic)
    /// steady-state long-term average concentrations or (acute) daily peak
    /// concentrations.
    /// </summary>
    public interface ITimeSeriesExposureCharacterisationCalculator {
        Dictionary<ExposureTarget, Dictionary<Compound, ISubstanceTargetExposure>> ComputeSubstanceTargetExposures(
            PbkSimulationOutput simulationOutput
        );
        double Compute(List<SubstanceTargetExposureTimePoint> Exposures);
    }
}
