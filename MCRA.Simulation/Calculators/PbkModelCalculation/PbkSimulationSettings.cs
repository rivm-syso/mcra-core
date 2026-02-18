using MCRA.General;

namespace MCRA.Simulation.Calculators.PbkModelCalculation {
    public class PbkSimulationSettings {
        public int NumberOfSimulatedDays { get; set; } = 50;
        public int LifetimeYears { get; set; } = 50;
        public bool UseRepeatedDailyEvents { get; set; } = true;
        public bool BodyWeightCorrected { get; set; }

        public bool UseParameterVariability { get; set; }

        private int _numberOfOralDosesPerDay = 1;

        public int NumberOfOralDosesPerDay {
            get {
                if (SpecifyEvents) {
                    return SelectedEvents.Length;
                }
                return _numberOfOralDosesPerDay;
            }
            set {
                _numberOfOralDosesPerDay = value;
            }
        }

        public int NumberOfDermalDosesPerDay { get; set; } = 1;
        public int NumberOfInhalationDosesPerDay { get; set; } = 1;
        public int NonStationaryPeriod { get; set; } = 10;
        public bool SpecifyEvents { get; set; }
        public int[] SelectedEvents { get; set; }

        public PbkSimulationMethod PbkSimulationMethod { get; set; }

        public PbkModelOutputResolutionTimeUnit OutputResolutionTimeUnit { get; set; }
        public int OutputResolutionStepSize { get; set; }

        public double PrecisionReverseDoseCalculation { get; set; } = 0.001;

        public bool AllowFallbackSystemic { get; set; }
        public bool AllowUseSurrogateMatrix { get; set; }
        public BiologicalMatrix SurrogateBiologicalMatrix { get; set; } = BiologicalMatrix.Undefined;

        public int GetNumberOfEventsPerDay(ExposureRoute exposureRoute) {
            switch (exposureRoute) {
                case ExposureRoute.Oral:
                    return NumberOfOralDosesPerDay;
                case ExposureRoute.Dermal:
                    return NumberOfDermalDosesPerDay;
                case ExposureRoute.Inhalation:
                    return NumberOfInhalationDosesPerDay;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
