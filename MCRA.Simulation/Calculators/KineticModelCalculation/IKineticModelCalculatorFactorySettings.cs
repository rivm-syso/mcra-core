namespace MCRA.Simulation.Calculators.KineticModelCalculation {
    public interface IKineticModelCalculatorFactorySettings {
        string CodeModel { get; }
        string CodeSubstance { get; }
        string CodeCompartment { get; }
        int NumberOfDays { get; }
        int NumberOfDosesPerDay { get; }
        int NumberOfDosesPerDayNonDietaryOral { get; }
        int NumberOfDosesPerDayNonDietaryDermal { get; }
        int NumberOfDosesPerDayNonDietaryInhalation { get; }
        bool UseParameterVariability { get; }
        bool SpecifyEvents { get; }
        string SelectedEvents { get; }
    }
}
