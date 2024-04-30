using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Calculators.AdjustmentFactorCalculation {
    public sealed class AdjustmentFactorModelFactorySettings : IAdjustmentFactorModelFactorySettings {
        private readonly SingleValueRisksModuleConfig _configuration;
        private bool _isExposure;
        public AdjustmentFactorModelFactorySettings(SingleValueRisksModuleConfig config, bool isExposure) {
            _configuration = config;
            _isExposure = isExposure;
        }

        public AdjustmentFactorDistributionMethod AdjustmentFactorDistributionMethod => _isExposure ? _configuration.ExposureAdjustmentFactorDistributionMethod : _configuration.HazardAdjustmentFactorDistributionMethod;
        public double ParameterA => _isExposure ? _configuration.ExposureParameterA : _configuration.HazardParameterA;
        public double ParameterB => _isExposure ? _configuration.ExposureParameterB : _configuration.HazardParameterB;
        public double ParameterC => _isExposure ? _configuration.ExposureParameterC : _configuration.HazardParameterC;
        public double ParameterD => _isExposure ? _configuration.ExposureParameterD : _configuration.HazardParameterD;
    }
}
