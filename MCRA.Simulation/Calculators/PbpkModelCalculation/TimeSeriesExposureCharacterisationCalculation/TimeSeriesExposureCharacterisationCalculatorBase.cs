using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation.Calculators.PbpkModelCalculation.TargetExposureFromTimeSeriesCalculation {
    public abstract class TimeSeriesExposureCharacterisationCalculatorBase : ITimeSeriesExposureCharacterisationCalculator {

        public Dictionary<ExposureTarget, Dictionary<Compound, ISubstanceTargetExposure>> ComputeSubstanceTargetExposures(
            PbkSimulationOutput simulationOutput
        ) {
            var result = simulationOutput.SubstanceTargetLevelTimeSeries
                .Select(r => {
                    return new SubstanceTargetExposurePattern() {
                        TimeSeries = r,
                        Exposure = Compute(r.Exposures)
                    };
                })
                .GroupBy(r => r.Target)
                .ToDictionary(
                    g => g.Key,
                    g => g
                        .ToDictionary(
                            r => r.Substance,
                            r => r as ISubstanceTargetExposure
                        )
                );
            return result;
        }

        public abstract double Compute(List<SubstanceTargetExposureTimePoint> exposures);
    }
}
