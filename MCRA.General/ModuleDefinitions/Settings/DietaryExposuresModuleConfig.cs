using MCRA.General.ModuleDefinitions.Interfaces;

namespace MCRA.General.ModuleDefinitions.Settings {
    public partial class DietaryExposuresModuleConfig {
        public IIntakeModelCalculationSettings GetFrequencyModelCalculationSettings() =>
            new IntakeModelCalculationSettings(
                FrequencyModelCovariateModelType,
                FrequencyModelFunctionType,
                FrequencyModelTestingLevel,
                FrequencyModelTestingMethod,
                FrequencyModelMinDegreesOfFreedom,
                FrequencyModelMaxDegreesOfFreedom
            );

        public IIntakeModelCalculationSettings GetAmountModelCalculationSettings() =>
            new IntakeModelCalculationSettings(
                AmountModelCovariateModelType,
                AmountModelFunctionType,
                AmountModelTestingLevel,
                AmountModelTestingMethod,
                AmountModelMinDegreesOfFreedom,
                AmountModelMaxDegreesOfFreedom
            );

        public IISUFModelCalculationSettings GetISUFModelCalculationSettings() =>
            new ISUFModelCalculationSettings(
                IsufModelGridPrecision,
                IsufModelNumberOfIterations,
                IsufModelSplineFit
            );
    }
}
