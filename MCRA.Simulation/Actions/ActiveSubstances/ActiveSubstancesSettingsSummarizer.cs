using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.General.SettingsDefinitions;
using MCRA.Simulation.Action;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.ActiveSubstances {

    public sealed class ActiveSubstancesSettingsSummarizer : ActionModuleSettingsSummarizer<ActiveSubstancesModuleConfig> {
        public ActiveSubstancesSettingsSummarizer(ActiveSubstancesModuleConfig config): base(config) {
        }

        public override ActionType ActionType => ActionType.ActiveSubstances;

        public override ActionSettingsSummary Summarize(ProjectDto project = null) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());

            var restrictToHazardData = _configuration.FilterByAvailableHazardDose
                || _configuration.FilterByAvailableHazardCharacterisation;
            var computeFromDockingOrQsar =
                _configuration.IsCompute && (_configuration.UseMolecularDockingModels || _configuration.UseQsarModels);

            section.SummarizeSetting(SettingsItemType.FilterByAvailableHazardCharacterisation, _configuration.FilterByAvailableHazardCharacterisation);
            section.SummarizeSetting(SettingsItemType.FilterByAvailableHazardDose, _configuration.FilterByAvailableHazardDose);

            if (_configuration.IsCompute) {
                section.SummarizeSetting(SettingsItemType.UseQsarModels, _configuration.UseQsarModels);
                section.SummarizeSetting(SettingsItemType.UseMolecularDockingModels, _configuration.UseMolecularDockingModels);
                if (computeFromDockingOrQsar) {
                    section.SummarizeSetting(SettingsItemType.AssessmentGroupMembershipCalculationMethod, _configuration.AssessmentGroupMembershipCalculationMethod);
                    if (restrictToHazardData) {
                        section.SummarizeSetting(SettingsItemType.CombinationMethodMembershipInfoAndPodPresence, _configuration.CombinationMethodMembershipInfoAndPodPresence);
                    }
                }
            } else {
                section.SummarizeSetting(SettingsItemType.FilterByCertainAssessmentGroupMembership, _configuration.FilterByCertainAssessmentGroupMembership, isVisible: false);
                if (restrictToHazardData) {
                    section.SummarizeSetting(SettingsItemType.CombinationMethodMembershipInfoAndPodPresence, _configuration.CombinationMethodMembershipInfoAndPodPresence);
                }
            }

            if (!_configuration.IsCompute || computeFromDockingOrQsar) {
                section.SummarizeSetting(SettingsItemType.UseProbabilisticMemberships, _configuration.UseProbabilisticMemberships);
                if (_configuration.UseProbabilisticMemberships) {
                    section.SummarizeSetting(SettingsItemType.PriorMembershipProbability, _configuration.PriorMembershipProbability);
                    if (_configuration.IsCompute) {
                        section.SummarizeSetting(SettingsItemType.FilterByCertainAssessmentGroupMembership, _configuration.FilterByCertainAssessmentGroupMembership, isVisible: false);
                    }
                } else {
                    if (computeFromDockingOrQsar && !restrictToHazardData) {
                        section.SummarizeSetting(SettingsItemType.IncludeSubstancesWithUnknowMemberships, _configuration.IncludeSubstancesWithUnknowMemberships);
                    } else if (!_configuration.IsCompute) {
                        section.SummarizeSetting(SettingsItemType.IncludeSubstancesWithUnknowMemberships, _configuration.IncludeSubstancesWithUnknowMemberships);
                    }
                }
            }

            return section;
        }
    }
}
