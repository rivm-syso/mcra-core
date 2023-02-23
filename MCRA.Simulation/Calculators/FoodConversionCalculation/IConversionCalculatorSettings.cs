namespace MCRA.Simulation.Calculators.FoodConversionCalculation {
    public interface IConversionCalculatorSettings {
        bool UseProcessing { get; } 

        bool UseComposition { get; }

        bool UseReadAcrossFoodTranslations { get; }

        bool UseMarketShares { get; }

        bool UseSubTypes { get; }

        bool UseSuperTypes { get; }

        bool UseDefaultProcessingFactor { get; }

        bool UseWorstCaseValues { get; }

        bool FoodIncludeNonDetects { get; }

        bool CompoundIncludeNonDetects { get; }

        bool CompoundIncludeNoMeasurements { get; }

        bool UseTdsCompositions { get; }

        bool SubstanceIndependent { get; }
    }
}
