using MCRA.General;

namespace MCRA.Simulation.Calculators.PbpkModelCalculation.TargetExposureFromTimeSeriesCalculation {
    public class TimeSeriesSteadyStateExposureCalculator : TimeSeriesExposureCharacterisationCalculatorBase {

        public int NonStationaryPeriod { get; private set; }

        public TimeSeriesSteadyStateExposureCalculator(int nonStationaryPeriod) {
            NonStationaryPeriod = nonStationaryPeriod;
        }

        public override double Compute(List<SubstanceTargetExposureTimePoint> exposures) {
            return Compute(exposures, NonStationaryPeriod);
        }

        /// <summary>
        /// The computed average target exposure.
        /// </summary>
        public static double Compute(
            List<SubstanceTargetExposureTimePoint> exposures,
            int nonStationaryPeriod
        ) {
            if (exposures?.Count >= 0) {
                return exposures
                    .Where(r => r.Time >= nonStationaryPeriod)
                    .Average(r => r.Exposure);
            } else {
                return 0;
            }
        }
    }
}
