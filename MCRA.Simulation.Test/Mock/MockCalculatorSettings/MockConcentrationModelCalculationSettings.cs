using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation;

namespace MCRA.Simulation.Test.Mock.MockCalculatorSettings {
    public class MockConcentrationModelCalculationSettings : IConcentrationModelCalculationSettings {
        public NonDetectsHandlingMethod NonDetectsHandlingMethod { get; set; }
        public ICollection<ConcentrationModelTypeFoodSubstance> ConcentrationModelTypesPerFoodCompound { get; set; }
        public ConcentrationModelType DefaultConcentrationModel { get; set; }
        public double FractionOfLor { get; set; }
        public double FractionOfMrl { get; set; }
        public bool IsFallbackMrl { get; set; }
        public bool CorrelateImputedValueWithSamplePotency { get; set; }
        public bool RestrictLorImputationToAuthorisedUses { get; set; }
    }
}
