namespace MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation {
    public class PbkSimulationSettings {
        public int NumberOfSimulatedDays { get; set; } = 50;
        public bool UseRepeatedDailyEvents { get; set; } = true;

        public bool UseParameterVariability { get; set; }

        private int _numberOfDosesPerDay = 1;

        public int NumberOfDosesPerDay {
            get {
                if (SpecifyEvents) {
                    return SelectedEvents.Length;
                }
                return _numberOfDosesPerDay;
            }
            set {
                _numberOfDosesPerDay = value;
            }
        }

        public int NumberOfDosesPerDayNonDietaryOral { get; set; } = 1;
        public int NumberOfDosesPerDayNonDietaryDermal { get; set; } = 1;
        public int NumberOfDosesPerDayNonDietaryInhalation { get; set; } = 1;
        public int NonStationaryPeriod { get; set; } = 10;
        public bool SpecifyEvents { get; set; }
        public int[] SelectedEvents { get; set; }

        public double PrecisionReverseDoseCalculation { get; set; } = 0.001;

    }
}
