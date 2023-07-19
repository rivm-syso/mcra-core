using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.AOPNetworks {
    public enum AOPNetworkSections {
        //This one is not realy needed
        AdverseOutcomePathwayNetworkSection,
        SelectedEffectsSection,
        EffectRelationshipsSection
    }
    public sealed class AOPNetworksSummarizer : ActionResultsSummarizerBase<IAOPNetworksCalculationActionResult> {

        public override ActionType ActionType => ActionType.AOPNetworks;

        public override void Summarize(ProjectDto project, IAOPNetworksCalculationActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<AOPNetworkSections>(project, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }

            var section = new AopNetworkSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            section.Summarize(data.AdverseOutcomePathwayNetwork, data.RelevantEffects);
            subHeader.SaveSummarySection(section);
            var subOrder = 1;

            if (outputSettings.ShouldSummarize(AOPNetworkSections.SelectedEffectsSection)) {
                summarizeAopNetworkEffects(
                    data.RelevantEffects,
                    data.SelectedEffect?.Code,
                    subHeader,
                    subOrder++
                );
            }
            if (outputSettings.ShouldSummarize(AOPNetworkSections.EffectRelationshipsSection)) {
                createEffectRelationsSection(
                    data.AdverseOutcomePathwayNetwork,
                    data.RelevantEffects,
                    subHeader,
                    subOrder++
                );
            }
        }

        /// <summary>
        /// Selected effects
        /// </summary>
        /// <param name="relevantEffects"></param>
        /// <param name="selectedEffectCode"></param>
        /// <param name="header"></param>
        /// <param name="order"></param>
        private void summarizeAopNetworkEffects(
            ICollection<Effect> relevantEffects,
            string selectedEffectCode,
            SectionHeader header,
            int order
        ) {
            var section = new EffectsSummarySection() {
                SectionLabel = getSectionLabel(AOPNetworkSections.SelectedEffectsSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(section, "Selected effects", order);
            section.Summarize(relevantEffects, selectedEffectCode);
            subHeader.SaveSummarySection(section);
        }

        /// <summary>
        /// Effect relationships
        /// </summary>
        /// <param name="adverseOutcomePathwayNetwork"></param>
        /// <param name="relevantEffects"></param>
        /// <param name="header"></param>
        /// <param name="order"></param>
        private void createEffectRelationsSection(
                AdverseOutcomePathwayNetwork adverseOutcomePathwayNetwork,
                ICollection<Effect> relevantEffects,
                SectionHeader header,
                int order
            ) {
            var section = new EffectRelationshipsSummarySection() {
                SectionLabel = getSectionLabel(AOPNetworkSections.EffectRelationshipsSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(section, "Effect relationships", order);
            section.Summarize(adverseOutcomePathwayNetwork, relevantEffects);
            subHeader.SaveSummarySection(section);
        }
    }
}
