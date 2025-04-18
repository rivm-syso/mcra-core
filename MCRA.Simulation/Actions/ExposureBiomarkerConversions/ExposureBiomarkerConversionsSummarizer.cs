﻿using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.ExposureBiomarkerConversions {

    public enum ExposureBiomarkerConversionsSections {
        //No sub-sections
    }

    public class ExposureBiomarkerConversionsSummarizer : ActionResultsSummarizerBase<IExposureBiomarkerConversionsActionResult> {

        public override ActionType ActionType => ActionType.ExposureBiomarkerConversions;

        public override void Summarize(
            ActionModuleConfig outputDetails,
            IExposureBiomarkerConversionsActionResult actionResult,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var outputSettings = new ModuleOutputSectionsManager<ExposureBiomarkerConversionsSections>(outputDetails, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            if (data.ExposureBiomarkerConversions.Any()) {
                var section = new ExposureBiomarkerConversionsSummarySection() {
                    SectionLabel = ActionType.ToString()
                };
                var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
                section.Summarize(data.ExposureBiomarkerConversions);
                subHeader.SaveSummarySection(section);
            }
        }
    }
}
