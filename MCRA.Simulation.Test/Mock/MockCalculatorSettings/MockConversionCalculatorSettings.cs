using MCRA.Simulation.Calculators.FoodConversionCalculation;

namespace MCRA.Simulation.Test.Mock.MockCalculatorSettings {
    public sealed class MockConversionCalculatorSettings : IConversionCalculatorSettings {
        public bool UseProcessing { get; set; }

        public bool UseComposition { get; set; }

        public bool UseReadAcrossFoodTranslations { get; set; }

        public bool UseMarketShares { get; set; }

        public bool UseSubTypes { get; set; }

        public bool UseSuperTypes { get; set; }

        public bool UseDefaultProcessingFactor { get; set; }

        public bool UseWorstCaseValues { get; set; }

        public bool FoodIncludeNonDetects { get; set; }

        public bool CompoundIncludeNonDetects { get; set; }

        public bool CompoundIncludeNoMeasurements { get; set; }

        public bool UseTdsCompositions { get; set; }

        public bool SubstanceIndependent { get; set; }
    }
}
