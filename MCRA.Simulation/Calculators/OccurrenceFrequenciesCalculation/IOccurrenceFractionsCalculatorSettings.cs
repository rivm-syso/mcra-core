namespace MCRA.Simulation.Calculators.OccurrenceFrequenciesCalculation {
    public interface IOccurrenceFractionsCalculatorSettings {
        bool SetMissingAgriculturalUseAsUnauthorized { get; }
        bool UseAgriculturalUsePercentage { get; }
    }
}
