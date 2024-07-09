using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation;

namespace MCRA.Simulation.Actions.ConcentrationModels {

    public sealed class ConcentrationModelsModuleSettings :
        IConcentrationModelCalculationSettings 
    {

        private readonly ConcentrationModelsModuleConfig _configuration;

        public ConcentrationModelsModuleSettings(ConcentrationModelsModuleConfig config) {
            _configuration = config;
        }

        public bool TotalDietStudy => _configuration.TotalDietStudy;

        public bool IsSampleBased => _configuration.IsSampleBased;

        public bool IsMultipleSubstances => _configuration.MultipleSubstances;

        public bool Cumulative => _configuration.Cumulative;

        // Concentration models

        public SettingsTemplateType ConcentrationModelChoice => _configuration.ConcentrationModelChoice;

        public ConcentrationModelType DefaultConcentrationModel => _configuration.DefaultConcentrationModel;

        public ICollection<ConcentrationModelTypeFoodSubstance> ConcentrationModelTypesPerFoodCompound => _configuration.ConcentrationModelTypesFoodSubstance;

        public NonDetectsHandlingMethod NonDetectsHandlingMethod => _configuration.NonDetectsHandlingMethod;

        public double FractionOfLor => _configuration.FractionOfLor;

        public bool RestrictLorImputationToAuthorisedUses => _configuration.RestrictLorImputationToAuthorisedUses;

        public double FractionOfMrl => _configuration.FractionOfMrl;

        public bool IsFallbackMrl => _configuration.IsFallbackMrl;

        public bool CorrelateImputedValueWithSamplePotency => _configuration.CorrelateImputedValueWithSamplePotency;

        public bool ImputeMissingValues => _configuration.ImputeMissingValues;

        // Uncertainty settings

        public bool ResampleConcentrations => _configuration.ResampleConcentrations;

        public bool IsParametric => _configuration.IsParametric;
    }
}
