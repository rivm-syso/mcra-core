using MCRA.General;
using MCRA.General.Action.Settings;

namespace MCRA.Simulation.Calculators.IntakeModelling.IntakeModels {
    public class FrequencyModelCalculationSettings : IIntakeModelCalculationSettings {
        private readonly FrequencyModelSettings _frequencyModelSettings;

        public FrequencyModelCalculationSettings(FrequencyModelSettings frequencyModelSettings) {
            _frequencyModelSettings = frequencyModelSettings;
        }
        public CovariateModelType CovariateModelType => _frequencyModelSettings.CovariateModelType;

        public FunctionType FunctionType => _isCovariateModel ? _frequencyModelSettings.FunctionType : FunctionType.Polynomial;

        public double TestingLevel => _isCovariateModel ? _frequencyModelSettings.TestingLevel : 0.05;

        public TestingMethodType TestingMethod => _isCovariateModel ? _frequencyModelSettings.TestingMethod : TestingMethodType.Backward;

        public int MinDegreesOfFreedom => _isCovariateModel ? _frequencyModelSettings.MinDegreesOfFreedom : 0;

        public int MaxDegreesOfFreedom => _isCovariateModel ? _frequencyModelSettings.MaxDegreesOfFreedom : 4;

        private bool _isCovariateModel => _frequencyModelSettings.CovariateModelType != CovariateModelType.Constant
            && _frequencyModelSettings.CovariateModelType != CovariateModelType.Cofactor;
    }
}
