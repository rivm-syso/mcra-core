using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Calculators.IntakeModelling.IntakeModels {
    public class AmountModelCalculationSettings : IIntakeModelCalculationSettings {
        private readonly DietaryExposuresModuleConfig _dietaryConfig;

        public AmountModelCalculationSettings(DietaryExposuresModuleConfig dietaryConfig) {
            _dietaryConfig = dietaryConfig;
        }
        public CovariateModelType CovariateModelType => _dietaryConfig.AmountModelCovariateModelType;

        public FunctionType FunctionType => IsCovariateModel ? _dietaryConfig.AmountModelFunctionType : FunctionType.Polynomial;

        public double TestingLevel => IsCovariateModel ? _dietaryConfig.AmountModelTestingLevel : 0.05;

        public TestingMethodType TestingMethod => IsCovariateModel ? _dietaryConfig.AmountModelTestingMethod : TestingMethodType.Backward;

        public int MinDegreesOfFreedom => IsCovariateModel ? _dietaryConfig.AmountModelMinDegreesOfFreedom : 0;

        public int MaxDegreesOfFreedom => IsCovariateModel ? _dietaryConfig.AmountModelMaxDegreesOfFreedom : 4;

        private bool IsCovariateModel =>
            _dietaryConfig.AmountModelCovariateModelType != CovariateModelType.Constant &&
            _dietaryConfig.AmountModelCovariateModelType != CovariateModelType.Cofactor;

    }
}
