namespace MCRA.Simulation.Calculators.ActiveSubstancesCalculators.MembershipsFromPodCalculation {
    public interface IMembershipsFromPodCalculatorSettings {
        bool RestrictToAvailableRpfs { get; }
        bool RestrictToAvailableHazardCharacterisations { get; }
        bool RestrictToAvailableHazardDoses { get; }
    }
}
