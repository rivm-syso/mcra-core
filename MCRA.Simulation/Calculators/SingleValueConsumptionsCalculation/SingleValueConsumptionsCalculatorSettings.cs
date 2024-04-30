using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Calculators.SingleValueConsumptionsCalculation {
    public sealed class SingleValueConsumptionsCalculatorSettings : ISingleValueConsumptionsCalculatorSettings {
        SingleValueConsumptionsModuleConfig _configuration;
        public SingleValueConsumptionsCalculatorSettings(SingleValueConsumptionsModuleConfig config) {
            _configuration = config;
        }
        public bool UseSamplingWeights => !_configuration.IsDefaultSamplingWeight;

        public bool IsConsumersOnly => _configuration.ModelledFoodsConsumerDaysOnly;

        public ExposureType ExposureType => _configuration.ExposureType;

        public bool IsProcessing => _configuration.IsProcessing;

        public bool UseBodyWeightStandardisedConsumptionDistribution => _configuration.UseBodyWeightStandardisedConsumptionDistribution;
    }
}
