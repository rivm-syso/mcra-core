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

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            section.SummarizeSetting(SettingsItemType.ExposureRoutes, _configuration.ExposureRoutes);
            if (_configuration.ExposureRoutes.Contains(ExposureRoute.Oral)) {
                section.SummarizeSetting(SettingsItemType.OralAbsorptionFactorForDietaryExposure, _configuration.OralAbsorptionFactorForDietaryExposure);
                section.SummarizeSetting(SettingsItemType.OralAbsorptionFactor, _configuration.OralAbsorptionFactor);
            }
            if (_configuration.ExposureRoutes.Contains(ExposureRoute.Dermal)) {
                section.SummarizeSetting(SettingsItemType.DermalAbsorptionFactor, _configuration.DermalAbsorptionFactor);
            }
            if (_configuration.ExposureRoutes.Contains(ExposureRoute.Inhalation)) {
                section.SummarizeSetting(SettingsItemType.InhalationAbsorptionFactor, _configuration.InhalationAbsorptionFactor);
            }
            return section;
        }
    }
}
