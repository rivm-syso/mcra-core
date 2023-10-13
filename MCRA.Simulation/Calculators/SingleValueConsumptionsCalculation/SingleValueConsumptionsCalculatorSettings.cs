using MCRA.General;
using MCRA.General.Action.Settings;

namespace MCRA.Simulation.Calculators.SingleValueConsumptionsCalculation {
    public sealed class SingleValueConsumptionsCalculatorSettings : ISingleValueConsumptionsCalculatorSettings {

        private readonly SubsetSettings _subsetSettings;
        private readonly AssessmentSettings _assessmentSettings;
        private readonly ConcentrationModelSettings _concentrationModelSettings;
        public SingleValueConsumptionsCalculatorSettings(
            SubsetSettings subsetSettings,
            AssessmentSettings assessmentSettings,
            ConcentrationModelSettings concentrationModelSettings
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
