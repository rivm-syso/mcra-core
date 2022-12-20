namespace MCRA.Simulation.Calculators.KineticModelCalculation.AbsorptionFactorsGeneration {
    public interface IAbsorptionFactorsCollectionBuilderSettings {
        double DefaultFactorDietary { get; }
        double DefaultFactorDermalNonDietary { get; }
        double DefaultFactorOralNonDietary { get; }
        double DefaultFactorInhalationNonDietary { get; }
    }
}
