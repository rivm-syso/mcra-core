using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Calculators.IntakeModelling.IntakeModels {
    public class AmountModelCalculationSettings : IIntakeModelCalculationSettings {
        private readonly DietaryExposuresModuleConfig _dietaryConfig;

        public AmountModelCalculationSettings(DietaryExposuresModuleConfig dietaryConfig) {
            _dietaryConfig = dietaryConfig;
        }
        public CovariateModelType CovariateModelType => _dietaryConfig.CovariateModelType;

        public FunctionType FunctionType => IsCovariateModel ? _dietaryConfig.FunctionType : FunctionType.Polynomial;

        public double TestingLevel => IsCovariateModel ? _dietaryConfig.TestingLevel : 0.05;

        public TestingMethodType TestingMethod => IsCovariateModel ? _dietaryConfig.TestingMethod : TestingMethodType.Backward;

        public int MinDegreesOfFreedom => IsCovariateModel ? _dietaryConfig.MinDegreesOfFreedom : 0;

        public int MaxDegreesOfFreedom => IsCovariateModel ? _dietaryConfig.MaxDegreesOfFreedom : 4;

        private bool IsCovariateModel =>
            _dietaryConfig.CovariateModelType != CovariateModelType.Constant &&
            _dietaryConfig.CovariateModelType != CovariateModelType.Cofactor;

    }
}
