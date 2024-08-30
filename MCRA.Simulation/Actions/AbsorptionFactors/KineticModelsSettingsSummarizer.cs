using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.KineticModels {

    public class KineticModelsSettingsSummarizer : ActionModuleSettingsSummarizer<KineticModelsModuleConfig> {

        public KineticModelsSettingsSummarizer(KineticModelsModuleConfig config) : base(config) {
        }

        public override ActionSettingsSummary Summarize(bool isCompute, ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            var isAggregate = _configuration.Aggregate;
            section.SummarizeSetting(SettingsItemType.OralAbsorptionFactorForDietaryExposure, _configuration.OralAbsorptionFactorForDietaryExposure);
            if (isAggregate) {
                section.SummarizeSetting(SettingsItemType.OralAbsorptionFactor, _configuration.OralAbsorptionFactor);
                section.SummarizeSetting(SettingsItemType.DermalAbsorptionFactor, _configuration.DermalAbsorptionFactor);
                section.SummarizeSetting(SettingsItemType.InhalationAbsorptionFactor, _configuration.InhalationAbsorptionFactor);
            }
            return section;
        }
    }
}
