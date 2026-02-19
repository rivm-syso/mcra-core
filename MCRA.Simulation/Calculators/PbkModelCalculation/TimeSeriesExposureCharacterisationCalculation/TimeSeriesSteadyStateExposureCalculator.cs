using MCRA.General;

namespace MCRA.Simulation.Calculators.PbkModelCalculation.TargetExposureFromTimeSeriesCalculation {
    public class TimeSeriesSteadyStateExposureCalculator : TimeSeriesExposureCharacterisationCalculatorBase {

        public bool IsLifetime { get; private set; }
        
        public int NonStationaryPeriod { get; private set; }

        public int LifetimeStationaryPeriod { get; private set; }

        public TimeSeriesSteadyStateExposureCalculator(
            bool isLifetime,
            int nonStationaryPeriod,
            int lifetimeStationaryPeriod
        ) {
            IsLifetime = isLifetime;
            NonStationaryPeriod = nonStationaryPeriod;
            LifetimeStationaryPeriod = lifetimeStationaryPeriod;
        }

        public override double Compute(List<SubstanceTargetExposureTimePoint> exposures) {
            return Compute(exposures, IsLifetime, NonStationaryPeriod, LifetimeStationaryPeriod);
        }

        /// <summary>
        /// The computed average target exposure.
        /// </summary>
        public static double Compute(
            List<SubstanceTargetExposureTimePoint> exposures,
            bool isLifetime,
            int nonStationaryPeriod,
            int lifetimeStationaryPeriod
        ) {
            if (exposures?.Count >= 0) {
                if (!isLifetime) {
                    return ComputeWithNonStationaryPeriod(exposures, nonStationaryPeriod);
                } else {
                    return ComputeForLastNDays(exposures, lifetimeStationaryPeriod);
                }
            } else {
                return 0;
            }
        }

        public static double ComputeWithNonStationaryPeriod(List<SubstanceTargetExposureTimePoint> exposures, int nonStationaryPeriod) {
            return exposures
                .Where(r => r.Time >= nonStationaryPeriod)
                .Average(r => r.Exposure);
        }

        public static double ComputeForLastNDays(List<SubstanceTargetExposureTimePoint> exposures, int numDays) {
            var endTime = exposures.Last().Time;
            var nonStationaryPeriod = (int)endTime - numDays;
            return ComputeWithNonStationaryPeriod(exposures, nonStationaryPeriod);
        }
    }
}
