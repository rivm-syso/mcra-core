using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.General.SettingsDefinitions;
using MCRA.Simulation.Action;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.OccupationalExposures {

    public sealed class OccupationalExposuresSettingsSummarizer : ActionModuleSettingsSummarizer<OccupationalExposuresModuleConfig> {
        public OccupationalExposuresSettingsSummarizer(OccupationalExposuresModuleConfig config) : base(config) {
        }

        public override ActionType ActionType => ActionType.OccupationalExposures;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());

            section.SummarizeSetting(
                SettingsItemType.SelectedExposureRoutes,
                string.Join(", ", _configuration.SelectedExposureRoutes),
                _configuration.SelectedExposureRoutes.Count > 0
            );

            section.SummarizeSetting(SettingsItemType.ExposureApproachMethod, _configuration.ExposureApproachMethod);
            if (_configuration.ExposureApproachMethod == ExposureApproachMethod.Modelled) {
                section.SummarizeSetting(SettingsItemType.OccupationalExposureModelType, _configuration.OccupationalExposureModelType);
                if (_configuration.OccupationalExposureModelType == OccupationalExposureModelType.Percentile) {
                    section.SummarizeSetting(SettingsItemType.SelectedPercentage, _configuration.SelectedPercentage);
                }
            }

            section.SummarizeSetting(SettingsItemType.ComputeExternalOccupationalDoses, _configuration.ComputeExternalOccupationalDoses);
            if (_configuration.ComputeExternalOccupationalDoses) {
                section.SummarizeSetting(SettingsItemType.OccupationalExposuresCalculationMethod, _configuration.OccupationalExposuresCalculationMethod);
                if (_configuration.OccupationalExposuresCalculationMethod == OccupationalExposuresCalculationMethod.SingleIndividual) {
                    if (_configuration.SelectedExposureRoutes.Contains(ExposureRoute.Inhalation)) {
                        section.SummarizeSetting(SettingsItemType.VentilatoryFlowRate, _configuration.VentilatoryFlowRate);
                    }
                    if (_configuration.SelectedExposureRoutes.Contains(ExposureRoute.Dermal)) {
                        section.SummarizeSetting(SettingsItemType.FractionSkinExposed, _configuration.FractionSkinExposed);
                    }
                }
            }

            return section;
        }
    }
}
