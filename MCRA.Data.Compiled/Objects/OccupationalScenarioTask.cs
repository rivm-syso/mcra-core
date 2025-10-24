using MCRA.General;
using MCRA.Simulation.Calculators.OccupationalExposureCalculation;

namespace MCRA.Data.Compiled.Objects {
    public sealed class OccupationalScenarioTask {
        public OccupationalTask OccupationalTask { get; set; }

        public double Duration { get; set; }

        public RPEType RpeType { get; set; }

        public double Frequency { get; set; }

        public FrequencyResolutionType FrequencyResolution { get; set; }

        public OccupationalTaskDeterminants Determinants() {
            return new OccupationalTaskDeterminants() {
                RPEType = RpeType,
            };
        }
    }
}
