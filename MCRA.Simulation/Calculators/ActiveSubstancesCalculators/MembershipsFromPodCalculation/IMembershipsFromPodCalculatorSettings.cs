namespace MCRA.Simulation.Calculators.ActiveSubstancesCalculators.MembershipsFromPodCalculation {
    public interface IMembershipsFromPodCalculatorSettings {
        bool RestrictToAvailableHazardCharacterisations { get; }
        bool RestrictToAvailableHazardDoses { get; }
    }
}
