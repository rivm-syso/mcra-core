using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.General.SettingsDefinitions;
using MCRA.Simulation.Action;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.Individuals {

    public sealed class IndividualsSettingsSummarizer : ActionModuleSettingsSummarizer<IndividualsModuleConfig> {
        public IndividualsSettingsSummarizer(IndividualsModuleConfig config) : base(config) {
        }
        public override ActionType ActionType => ActionType.Individuals;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());

            if (project.AirExposuresSettings.AirExposuresIndividualGenerationMethod == AirExposuresIndividualGenerationMethod.Simulate
                    || project.DustExposuresSettings.DustExposuresIndividualGenerationMethod == DustExposuresIndividualGenerationMethod.Simulate
                    || project.SoilExposuresSettings.SoilExposuresIndividualGenerationMethod == SoilExposuresIndividualGenerationMethod.Simulate
            ) {
                section.SummarizeSetting(SettingsItemType.NumberOfSimulatedIndividuals, _configuration.NumberOfSimulatedIndividuals);
            }
            return section;
        }
    }
}
