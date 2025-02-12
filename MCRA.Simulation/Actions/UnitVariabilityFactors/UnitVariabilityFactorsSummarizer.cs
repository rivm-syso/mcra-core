﻿using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.UnitVariabilityFactors {
    public enum UnitVariabilityFactorsSections {
        //No sub-sections
    }
    public sealed class UnitVariabilityFactorsSummarizer : ActionResultsSummarizerBase<IUnitVariabilityFactorsActionResult> {

        public override ActionType ActionType => ActionType.UnitVariabilityFactors;

        public override void Summarize(ActionModuleConfig sectionConfig, IUnitVariabilityFactorsActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<UnitVariabilityFactorsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new UnitVariabilityFactorsSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            section.Records = data.UnitVariabilityDictionary.Values
                .SelectMany(r => r.UnitVariabilityFactors, (f, uv) => {
                    return new UnitVariabilityFactorsRecord() {
                        FoodName = uv.Food.Name,
                        FoodCode = uv.Food.Code,
                        CompoundName = uv.Compound?.Name,
                        CompoundCode = uv.Compound?.Code,
                        ProcessingTypeDescription = uv.ProcessingType?.Description,
                        ProcessingTypeCode = uv.ProcessingType?.Code,
                        UnitWeightRac = uv.Food.DefaultUnitWeightRac?.Value ?? double.NaN,
                        UnitVariabilityFactor = uv.Factor ?? double.NaN,
                        UnitsInCompositeSample = (int)uv.UnitsInCompositeSample,
                        CoefficientOfVariation = uv.Coefficient ?? double.NaN
                    };
                })
                .OrderBy(r => r.FoodName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.FoodCode, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.CompoundName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.CompoundCode, StringComparer.OrdinalIgnoreCase)
                .ToList();

            subHeader.SaveSummarySection(section);
        }
    }
}
