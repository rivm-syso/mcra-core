using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Actions.ActiveSubstances {

    public enum ActiveSubstancesSections {
        AvailableAssessmentGroupMembershipModelsSection,
        AssessmentGroupMembershipModelCorrelationsSection,
        AllAOPEffectsMembershipModelsSection
    }
    public sealed class ActiveSubstancesSummarizer : ActionResultsSummarizerBase<ActiveSubstancesActionResult> {

        public override ActionType ActionType => ActionType.ActiveSubstances;

        public override void Summarize(ProjectDto project, ActiveSubstancesActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<ActiveSubstancesSections>(project, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }

            var section = new ActiveSubstancesSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            section.Summarize(data.MembershipProbabilities, data.AllCompounds.ToHashSet(), data.SelectedEffect);
            subHeader.SaveSummarySection(section);

            var subOrder = 0;
            subHeader.AddSubSectionHeaderFor(section.ActiveSubstancesTableSection, "Substance memberships table", subOrder++);

            if (data.AvailableActiveSubstanceModels.Any() && outputSettings.ShouldSummarize(ActiveSubstancesSections.AvailableAssessmentGroupMembershipModelsSection)) {
                summarizeAvailableActiveSubstanceModels(
                    data.AvailableActiveSubstanceModels,
                    data.AllCompounds,
                    subHeader,
                    subOrder++
                );
                if (data.AvailableActiveSubstanceModels.Count > 1 && outputSettings.ShouldSummarize(ActiveSubstancesSections.AssessmentGroupMembershipModelCorrelationsSection)) {
                    summarizeAvailableActiveSubstanceModelCorrelations(
                        data.AvailableActiveSubstanceModels,
                        data.AllCompounds,
                        subHeader,
                        subOrder++
                    );
                }
            }
            if (actionResult?.AopNetworkEffectsActiveSubstanceModels?.Any() ?? false && outputSettings.ShouldSummarize(ActiveSubstancesSections.AllAOPEffectsMembershipModelsSection)) {
                summarizeAopEffectsActiveSubstanceModels(
                    data.AllCompounds,
                    actionResult,
                    subHeader,
                    subOrder++
                );
            }
        }


        private void summarizeAvailableActiveSubstanceModels(
                ICollection<ActiveSubstanceModel> availableActiveSubstanceModels,
                ICollection<Compound> allCompounds,
                SectionHeader header,
                int order
            ) {
            var section = new ActiveSubstancesSummarySection() {
                SectionLabel = getSectionLabel(ActiveSubstancesSections.AvailableAssessmentGroupMembershipModelsSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(section, "Available assessment group membership models", order);
            section.Summarize(availableActiveSubstanceModels.ToList(), allCompounds.ToHashSet());
            subHeader.SaveSummarySection(section);
        }


        private void summarizeAvailableActiveSubstanceModelCorrelations(
                ICollection<ActiveSubstanceModel> availableActiveSubstanceModels,
                ICollection<Compound> allCompounds,
                SectionHeader header,
                int order
            ) {
            var section = new ActiveSubstanceModelCorrelationsSection() {
                SectionLabel = getSectionLabel(ActiveSubstancesSections.AssessmentGroupMembershipModelCorrelationsSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(section, "Assessment group membership model correlations", order);
            section.Summarize(availableActiveSubstanceModels.ToList(), allCompounds.ToHashSet());
            subHeader.SaveSummarySection(section);
        }

        private void summarizeAopEffectsActiveSubstanceModels(
                ICollection<Compound> allCompounds,
                ActiveSubstancesActionResult result,
                SectionHeader header,
                int order
            ) {
            var section = new ActiveSubstancesSummarySection() {
                SectionLabel = getSectionLabel(ActiveSubstancesSections.AllAOPEffectsMembershipModelsSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(section, "All AOP effects membership models", order);
            section.Summarize(result.AopNetworkEffectsActiveSubstanceModels.ToList(), allCompounds.ToHashSet());
            subHeader.SaveSummarySection(section);
        }
    }
}
