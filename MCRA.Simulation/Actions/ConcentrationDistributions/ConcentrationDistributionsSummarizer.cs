﻿using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.ConcentrationDistributions {
    public enum ConcentrationDistributionsSections {
        //No sub-sections
    }
    public sealed class ConcentrationDistributionsSummarizer : ActionResultsSummarizerBase<IConcentrationDistributionsActionResult> {

        public override ActionType ActionType => ActionType.ConcentrationDistributions;

        public override void Summarize(ActionModuleConfig sectionConfig, IConcentrationDistributionsActionResult result, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<ConcentrationDistributionsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new ConcentrationDistributionsSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            section.Summarize(data.ConcentrationDistributions);
            subHeader.SaveSummarySection(section);
        }
    }
}
