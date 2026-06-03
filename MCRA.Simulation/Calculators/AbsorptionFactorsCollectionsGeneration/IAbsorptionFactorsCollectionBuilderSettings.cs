namespace MCRA.Simulation.Calculators.KineticConversionCalculation.AbsorptionFactorsCollectionsGeneration {
    public interface IAbsorptionFactorsCollectionBuilderSettings {
        double DefaultFactorOral { get; }
        double DefaultFactorDermal { get; }
        double DefaultFactorInhalation { get; }
    }
}
