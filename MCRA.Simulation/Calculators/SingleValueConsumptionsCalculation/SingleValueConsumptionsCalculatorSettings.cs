using MCRA.General;
using MCRA.General.Action.Settings;

namespace MCRA.Simulation.Calculators.SingleValueConsumptionsCalculation {
    public sealed class SingleValueConsumptionsCalculatorSettings : ISingleValueConsumptionsCalculatorSettings {

        private readonly SubsetSettingsDto _subsetSettings;
        private readonly AssessmentSettingsDto _assessmentSettings;
        private readonly ConcentrationModelSettingsDto _concentrationModelSettings;
        public SingleValueConsumptionsCalculatorSettings(
            SubsetSettingsDto subsetSettings, 
            AssessmentSettingsDto assessmentSettings,
            ConcentrationModelSettingsDto concentrationModelSettings
         ) {
            _subsetSettings = subsetSettings;
            _assessmentSettings = assessmentSettings;
            _concentrationModelSettings = concentrationModelSettings;
        }
        public bool UseSamplingWeights => !_subsetSettings.IsDefaultSamplingWeight;

        public bool IsConsumersOnly => _subsetSettings.ModelledFoodsConsumerDaysOnly;

        public ExposureType ExposureType => _assessmentSettings.ExposureType;

        public bool IsProcessing => _concentrationModelSettings.IsProcessing;

        public bool UseBodyWeightStandardisedConsumptionDistribution => _subsetSettings.UseBodyWeightStandardisedConsumptionDistribution;
    }
}
