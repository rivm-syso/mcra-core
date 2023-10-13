using MCRA.General;
using MCRA.General.Action.Settings;

namespace MCRA.Simulation.Calculators.ConcentrationModelCalculation {
    public interface IConcentrationModelCalculationSettings {
        NonDetectsHandlingMethod NonDetectsHandlingMethod { get; }
        ICollection<ConcentrationModelTypeFoodSubstance> ConcentrationModelTypesPerFoodCompound { get; }
        ConcentrationModelType DefaultConcentrationModel { get; }
        double FractionOfLOR { get; }
        double FractionOfMrl { get; }
        bool IsFallbackMrl { get; }
        bool CorrelateImputedValueWithSamplePotency { get; }
    }
}
