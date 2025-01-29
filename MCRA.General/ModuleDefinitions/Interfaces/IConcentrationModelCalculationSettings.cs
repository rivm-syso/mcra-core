using MCRA.General.Action.Settings;

namespace MCRA.General.ModuleDefinitions.Interfaces {
    public interface IConcentrationModelCalculationSettings {
        NonDetectsHandlingMethod NonDetectsHandlingMethod { get; }
        ICollection<ConcentrationModelTypeFoodSubstance> ConcentrationModelTypesPerFoodCompound { get; }
        ConcentrationModelType DefaultConcentrationModel { get; }
        double FractionOfLor { get; }
        double FractionOfMrl { get; }
        bool IsFallbackMrl { get; }
        bool CorrelateImputedValueWithSamplePotency { get; }
        bool RestrictLorImputationToAuthorisedUses { get; }
    }
}
