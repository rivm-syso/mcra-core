using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Calculators.IntakeModelling.IntakeModels {
    public class FrequencyModelCalculationSettings : IIntakeModelCalculationSettings {
        private readonly DietaryExposuresModuleConfig _dietaryConfig;

        public FrequencyModelCalculationSettings(DietaryExposuresModuleConfig dietaryConfig) {
            _dietaryConfig = dietaryConfig;
        }
        public CovariateModelType CovariateModelType => _dietaryConfig.FrequencyModelCovariateModelType;

        public FunctionType FunctionType => IsCovariateModel ? _dietaryConfig.FrequencyModelFunctionType : FunctionType.Polynomial;

        public double TestingLevel => IsCovariateModel ? _dietaryConfig.FrequencyModelTestingLevel : 0.05;

        public TestingMethodType TestingMethod => IsCovariateModel ? _dietaryConfig.FrequencyModelTestingMethod : TestingMethodType.Backward;

        public int MinDegreesOfFreedom => IsCovariateModel ? _dietaryConfig.FrequencyModelMinDegreesOfFreedom : 0;

        public int MaxDegreesOfFreedom => IsCovariateModel ? _dietaryConfig.FrequencyModelMaxDegreesOfFreedom : 4;

        private bool IsCovariateModel =>
            _dietaryConfig.FrequencyModelCovariateModelType != CovariateModelType.Constant &&
            _dietaryConfig.FrequencyModelCovariateModelType != CovariateModelType.Cofactor;
    }
}
