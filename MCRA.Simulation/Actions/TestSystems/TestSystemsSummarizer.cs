﻿using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.TestSystems {
    public enum TestSystemsSections {
        //No sub-sections
    }
    public sealed class TestSystemsSummarizer : ActionResultsSummarizerBase<ITestSystemsActionResult> {

        public override ActionType ActionType => ActionType.TestSystems;

        public override void Summarize(ActionModuleConfig sectionConfig, ITestSystemsActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<TestSystemsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new TestSystemsSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            section.Records = data.TestSystems
                .Select(c => new TestSystemsSummaryRecord() {
                    CodeSystem = c.Code,
                    Name = c.Name,
                    Description = c.Description,
                    ExposureRouteType = c.ExposureRoute.GetDisplayName(),
                    GuidelineStudy = c.GuidelineStudy,
                    Organ = c.Organ,
                    Species = c.Species,
                    Reference = c.Reference,
                    Strain = c.Strain,
                    TestSystemType = c.TestSystemType.GetDisplayName(),
                })
                .ToList();
            subHeader.SaveSummarySection(section);
        }
    }
}
