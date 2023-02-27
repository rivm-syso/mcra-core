using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.ActiveSubstances {

    public sealed class ActiveSubstancesSettingsSummarizer : ActionSettingsSummarizerBase {

        public override ActionType ActionType => ActionType.ActiveSubstances;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            var settings = project.EffectSettings;
            var isCompute = project.CalculationActionTypes.Contains(ActionType);

            var restrictToHazardData = settings.RestrictToAvailableHazardDoses
                || settings.RestrictToAvailableHazardCharacterisations;
            var computeFromDockingOrQsar =
                isCompute && (settings.UseMolecularDockingModels || settings.UseQsarModels);

            section.SummarizeSetting(SettingsItemType.FilterByAvailableHazardCharacterisation, settings.RestrictToAvailableHazardCharacterisations);
            section.SummarizeSetting(SettingsItemType.FilterByAvailableHazardDose, settings.RestrictToAvailableHazardDoses);

            if (isCompute) {
                section.SummarizeSetting(SettingsItemType.UseQsarModels, settings.UseQsarModels);
                section.SummarizeSetting(SettingsItemType.UseMolecularDockingModels, settings.UseMolecularDockingModels);
                if (computeFromDockingOrQsar) {
                    section.SummarizeSetting(SettingsItemType.AssessmentGroupMembershipCalculationMethod, settings.AssessmentGroupMembershipCalculationMethod);
                    if (restrictToHazardData) {
                        section.SummarizeSetting(SettingsItemType.CombinationMethodMembershipInfoAndPodPresence, settings.CombinationMethodMembershipInfoAndPodPresence);
                    }
                }
            } else {
                section.SummarizeSetting(SettingsItemType.FilterByCertainAssessmentGroupMembership, settings.RestrictToCertainMembership, isVisible: false);
                if (restrictToHazardData) {
                    section.SummarizeSetting(SettingsItemType.CombinationMethodMembershipInfoAndPodPresence, settings.CombinationMethodMembershipInfoAndPodPresence);
                }
            }

            if (!isCompute || computeFromDockingOrQsar) {
                section.SummarizeSetting(SettingsItemType.UseProbabilisticMemberships, settings.UseProbabilisticMemberships);
                if (settings.UseProbabilisticMemberships) {
                    section.SummarizeSetting(SettingsItemType.PriorMembershipProbability, settings.PriorMembershipProbability);
                    if (isCompute) {
                        section.SummarizeSetting(SettingsItemType.FilterByCertainAssessmentGroupMembership, settings.RestrictToCertainMembership, isVisible: false);
                    }
                } else {
                    if (computeFromDockingOrQsar && !restrictToHazardData) {
                        section.SummarizeSetting(SettingsItemType.IncludeSubstancesWithUnknowMemberships, settings.IncludeSubstancesWithUnknowMemberships);
                    } else if (!isCompute) {
                        section.SummarizeSetting(SettingsItemType.IncludeSubstancesWithUnknowMemberships, settings.IncludeSubstancesWithUnknowMemberships);
                    }
                }
            }

            return section;
        }
    }
}
