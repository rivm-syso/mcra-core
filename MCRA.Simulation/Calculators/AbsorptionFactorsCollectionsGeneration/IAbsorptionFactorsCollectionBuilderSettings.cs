namespace MCRA.Simulation.Calculators.KineticConversionCalculation.AbsorptionFactorsCollectionsGeneration {
    public interface IAbsorptionFactorsCollectionBuilderSettings {
        double DefaultFactorDietary { get; }
        double DefaultFactorDermalNonDietary { get; }
        double DefaultFactorOralNonDietary { get; }
        double DefaultFactorInhalationNonDietary { get; }
    }
}
