using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.SingleValueDietaryExposures {

    public class SingleValueDietaryExposuresSettingsSummarizer : ActionModuleSettingsSummarizer<SingleValueDietaryExposuresModuleConfig> {

        public SingleValueDietaryExposuresSettingsSummarizer(SingleValueDietaryExposuresModuleConfig config) : base(config) {
        }

        public override ActionSettingsSummary Summarize(bool isCompute, ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            section.SummarizeSetting(SettingsItemType.SingleValueDietaryExposureCalculationMethod, _configuration.SingleValueDietaryExposureCalculationMethod);
            if (_configuration.ExposureType == ExposureType.Acute) {
                section.SummarizeSetting("Apply unit variability", _configuration.UseUnitVariability);
            } else {
                section.SummarizeSetting(SettingsItemType.UseUnitVariability, _configuration.UseUnitVariability, isVisible: false);
            }
            return section;
        }
    }
}
