using MCRA.General;
using MCRA.General.Action.Settings;

namespace MCRA.Simulation.Calculators.IntakeModelling.IntakeModels {
    public class AmountModelCalculationSettings : IIntakeModelCalculationSettings {
        private readonly AmountModelSettings _amountModelSettings;

        public AmountModelCalculationSettings(AmountModelSettings amountModelSettings) {
            _amountModelSettings = amountModelSettings;
        }
        public CovariateModelType CovariateModelType => _amountModelSettings.CovariateModelType;

        public FunctionType FunctionType => _isCovariateModel ? _amountModelSettings.FunctionType : FunctionType.Polynomial;

        public double TestingLevel => _isCovariateModel ? _amountModelSettings.TestingLevel : 0.05;

        public TestingMethodType TestingMethod => _isCovariateModel ? _amountModelSettings.TestingMethod : TestingMethodType.Backward;

        public int MinDegreesOfFreedom => _isCovariateModel ? _amountModelSettings.MinDegreesOfFreedom : 0;

        public int MaxDegreesOfFreedom => _isCovariateModel ? _amountModelSettings.MaxDegreesOfFreedom : 4;

        private bool _isCovariateModel => _amountModelSettings.CovariateModelType != CovariateModelType.Constant
            && _amountModelSettings.CovariateModelType != CovariateModelType.Cofactor;

    }
}
