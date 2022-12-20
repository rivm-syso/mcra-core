namespace MCRA.Simulation.Calculators.ActiveSubstancesCalculators.MembershipsFromInSilicoCalculation {
    public interface IMembershipsFromInSilicoCalculatorSettings {
        bool UseQsarModels { get; }
        bool UseMolecularDockingModels { get; }
    }
}
