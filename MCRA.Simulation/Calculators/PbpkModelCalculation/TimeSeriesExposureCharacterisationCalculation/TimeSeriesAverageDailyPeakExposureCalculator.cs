using MCRA.General;

namespace MCRA.Simulation.Calculators.PbpkModelCalculation.TargetExposureFromTimeSeriesCalculation {
    public class TimeSeriesAverageDailyPeakExposureCalculator : TimeSeriesExposureCharacterisationCalculatorBase {

        public int NonStationaryPeriod { get; private set; }

        public TimeSeriesAverageDailyPeakExposureCalculator(int nonStationaryPeriod) {
            NonStationaryPeriod = nonStationaryPeriod;
        }

        public override double Compute(List<SubstanceTargetExposureTimePoint> exposures) {
            return Compute(exposures, NonStationaryPeriod);
        }

        /// <summary>
        /// The computed peak target exposure = internal dose
        /// </summary>
        public static double Compute(
            List<SubstanceTargetExposureTimePoint> Exposures,
            int nonStationaryPeriod
        ) {
            if (Exposures?.Count >= 0) {
                var stationaryTargetExposures = Exposures
                    .Where(r => r.Time >= nonStationaryPeriod)
                    .ToList();

                var n = stationaryTargetExposures.Max(r => r.Time) - nonStationaryPeriod;
                if (n < 1) {
                    n = 1;
                }
                var peaks = new List<double>();
                var timeOffsetStart = nonStationaryPeriod;
                for (var i = 0; i < n; i++) {
                    var timeOffSetStop = (nonStationaryPeriod + i + 1);
                    var max = stationaryTargetExposures
                        .Where(r => r.Time >= timeOffsetStart && r.Time < timeOffSetStop)
                        .Select(r => r.Exposure)
                        .Max();
                    peaks.Add(max);
                    timeOffsetStart = timeOffSetStop;
                }
                return peaks.Average();
            } else {
                return 0;
            }
        }
    }
}
