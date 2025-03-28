﻿using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.FocalFoodConcentrations {
    public enum FocalFoodConcentrationsSections {
        SamplesByFoodAndSubstanceSection
    }
    public sealed class FocalFoodConcentrationsSummarizer : ActionModuleResultsSummarizer<FocalFoodConcentrationsModuleConfig, IFocalFoodConcentrationsActionResult> {

        public FocalFoodConcentrationsSummarizer(FocalFoodConcentrationsModuleConfig config) : base(config) {
        }

        public override void Summarize(ActionModuleConfig sectionConfig, IFocalFoodConcentrationsActionResult result, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<FocalFoodConcentrationsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new ConcentrationDataSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            section.Summarize(data.FocalCommoditySubstanceSampleCollections, data.AllCompounds);
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            subHeader.Units = collectUnits(data);

            if ((data.FocalCommoditySubstanceSampleCollections?.Any(r => r.SampleCompoundRecords.Any()) ?? false)
               && outputSettings.ShouldSummarize(FocalFoodConcentrationsSections.SamplesByFoodAndSubstanceSection)
            ) {
                summarizeSamplesByFoodAndSubstance(data, subHeader, order++);
            }
            subHeader.SaveSummarySection(section);
        }

        private List<ActionSummaryUnitRecord> collectUnits(ActionData data) {
            var result = new List<ActionSummaryUnitRecord> {
                new("ConcentrationUnit", data.ConcentrationUnit.GetShortDisplayName()),
                new("LowerPercentage", $"p{_configuration.VariabilityLowerPercentage}"),
                new("UpperPercentage", $"p{_configuration.VariabilityUpperPercentage}")
            };
            return result;
        }

        private void summarizeSamplesByFoodAndSubstance(ActionData data, SectionHeader header, int order) {
            var section = new SamplesByFoodSubstanceSection() {
                SectionLabel = getSectionLabel(FocalFoodConcentrationsSections.SamplesByFoodAndSubstanceSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Samples by food and substance",
                order
            );
            section.Summarize(
                data.FocalCommoditySubstanceSampleCollections,
                null,
                _configuration.VariabilityLowerPercentage,
                _configuration.VariabilityUpperPercentage
            );
            subHeader.SaveSummarySection(section);
        }
    }
}
