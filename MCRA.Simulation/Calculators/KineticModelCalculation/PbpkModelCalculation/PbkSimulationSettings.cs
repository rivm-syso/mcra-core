using MCRA.General;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation {
    public class PbkSimulationSettings {
        public int NumberOfSimulatedDays { get; set; } = 50;
        public bool UseRepeatedDailyEvents { get; set; } = true;

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

        public double PrecisionReverseDoseCalculation { get; set; } = 0.001;

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
