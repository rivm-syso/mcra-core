﻿using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.Substances {
    public enum SubstancesSections {
        //No sub-sections
    }
    public class SubstancesSummarizer : ActionResultsSummarizerBase<ISubstancesActionResult> {

        public override ActionType ActionType => ActionType.Substances;

        public override void Summarize(ActionModuleConfig sectionConfig, ISubstancesActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<SubstancesSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new SubstancesSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            section.Summarize(data.AllCompounds);
            subHeader.SaveSummarySection(section);
        }
    }
}
