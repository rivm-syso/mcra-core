using ExCSS;
using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.PbpkModelCalculation {
    public class SubstanceTargetExposureTimeSeries {

        /// <summary>
        /// The chemical substance.
        /// </summary>
        public Compound Substance { get; set; }

        /// <summary>
        /// The (internal) exposure target and unit.
        /// </summary>
        public TargetUnit TargetUnit { get; set; }

        /// <summary>
        /// The exposure time series (amount or concentration per time).
        /// Time unit is in days.
        /// </summary>
        public List<SubstanceTargetExposureTimePoint> Exposures { get; set; }

        /// <summary>
        /// Relative compartment size of the target.
        /// </summary>
        public double RelativeCompartmentWeight { get; set; }

        /// <summary>
        /// The computed peak target exposure = internal dose
        /// </summary>
        public double ComputePeakTargetExposure(
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

        /// <summary>
        /// The computed average target exposure.
        /// </summary>
        public double ComputeSteadyStateTargetExposure(
            int nonStationaryPeriod
        ) {
            if (Exposures?.Count >= 0) {
                return Exposures
                    .Where(r => r.Time >= nonStationaryPeriod)
                    .Average(r => r.Exposure);
            } else {
                return 0;
            }
        }
    }
}
