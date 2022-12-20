namespace MCRA.Simulation.Calculators.KineticModelCalculation {
    public interface IKineticModelCalculatorFactorySettings {
        string CodeModel { get; }
        string CodeSubstance { get; }
        string CodeCompartment { get; }
        int NumberOfIndividuals { get; }
        int NumberOfDays { get; }
        int NumberOfDosesPerDay { get; }
        int NumberOfDosesPerDayNonDietaryOral { get; }
        int NumberOfDosesPerDayNonDietaryDermal { get; }
        int NumberOfDosesPerDayNonDietaryInhalation { get; }
        int NonStationaryPeriod { get; }
        bool UseParameterVariability { get; }
    }
}
