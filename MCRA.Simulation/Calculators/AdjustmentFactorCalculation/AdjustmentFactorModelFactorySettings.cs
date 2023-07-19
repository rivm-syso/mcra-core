using MCRA.General;
using MCRA.General.Action.Settings;

namespace MCRA.Simulation.Calculators.AdjustmentFactorCalculation {
    public sealed class AdjustmentFactorModelFactorySettings : IAdjustmentFactorModelFactorySettings {
        private readonly EffectModelSettingsDto _effectModelSettings;
        private bool _isExposure;
        public AdjustmentFactorModelFactorySettings(EffectModelSettingsDto effectModelSettings,  bool isExposure) {
            _effectModelSettings = effectModelSettings;
            _isExposure = isExposure;
        }

        public AdjustmentFactorDistributionMethod AdjustmentFactorDistributionMethod => _isExposure ? _effectModelSettings.ExposureAdjustmentFactorDistributionMethod : _effectModelSettings.HazardAdjustmentFactorDistributionMethod;
        public double ParameterA => _isExposure ? _effectModelSettings.ExposureParameterA : _effectModelSettings.HazardParameterA;
        public double ParameterB => _isExposure ? _effectModelSettings.ExposureParameterB : _effectModelSettings.HazardParameterB;
        public double ParameterC => _isExposure ? _effectModelSettings.ExposureParameterC : _effectModelSettings.HazardParameterC;
        public double ParameterD => _isExposure ? _effectModelSettings.ExposureParameterD : _effectModelSettings.HazardParameterD;
    }
}
