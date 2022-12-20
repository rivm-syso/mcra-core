using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;
using System.Linq;

namespace MCRA.Simulation.Actions.Concentrations {

    public sealed class ConcentrationsSettingsSummarizer : ActionSettingsSummarizerBase {

        public override ActionType ActionType => ActionType.Concentrations;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            var cms = project.ConcentrationModelSettings;
            section.SummarizeSetting(SettingsItemType.ConcentrationsTier, cms.ConcentrationsTier);
            section.SummarizeSetting(SettingsItemType.RestrictToModelledFoodSubset, project.SubsetSettings.RestrictToModelledFoodSubset);
            section.SummarizeSetting(SettingsItemType.FilterConcentrationLimitExceedingSamples, cms.FilterConcentrationLimitExceedingSamples);
            if (project.ConcentrationModelSettings.FilterConcentrationLimitExceedingSamples) {
                section.SummarizeSetting(SettingsItemType.ConcentrationLimitFilterFractionExceedanceThreshold, cms.ConcentrationLimitFilterFractionExceedanceThreshold);
            }
            section.SummarizeSetting(SettingsItemType.UseComplexResidueDefinitions, cms.UseComplexResidueDefinitions);
            if (project.ConcentrationModelSettings.UseComplexResidueDefinitions) {
                section.SummarizeSetting(SettingsItemType.SubstanceTranslationAllocationMethod, cms.SubstanceTranslationAllocationMethod);
                section.SummarizeSetting(SettingsItemType.RetainAllAllocatedSubstancesAfterAllocation, cms.RetainAllAllocatedSubstancesAfterAllocation);
                section.SummarizeSetting(SettingsItemType.ConsiderAuthorisationsForSubstanceConversion, cms.ConsiderAuthorisationsForSubstanceConversion);
                if (cms.TryFixDuplicateAllocationInconsistencies) {
                    section.SummarizeSetting(SettingsItemType.TryFixDuplicateAllocationInconsistencies, cms.TryFixDuplicateAllocationInconsistencies);
                }
            }
            section.SummarizeSetting(SettingsItemType.ExtrapolateConcentrations, cms.ExtrapolateConcentrations);
            if (project.ConcentrationModelSettings.ExtrapolateConcentrations) {
                section.SummarizeSetting(SettingsItemType.ThresholdForExtrapolation, cms.ThresholdForExtrapolation);
                section.SummarizeSetting(SettingsItemType.ConsiderMrlForExtrapolations, cms.ConsiderMrlForExtrapolations);
                section.SummarizeSetting(SettingsItemType.ConsiderAuthorisationsForExtrapolations, cms.ConsiderAuthorisationsForExtrapolations);
            }
            section.SummarizeSetting(SettingsItemType.ImputeWaterConcentrations, cms.ImputeWaterConcentrations);
            if (project.ConcentrationModelSettings.ImputeWaterConcentrations) {
                section.SummarizeSetting(SettingsItemType.CodeWater, cms.CodeWater);
                section.SummarizeSetting(SettingsItemType.WaterConcentrationValue, cms.WaterConcentrationValue);
                section.SummarizeSetting(SettingsItemType.RestrictWaterImputationToAuthorisedUses, cms.RestrictWaterImputationToAuthorisedUses);
            }
            section.SummarizeSetting(SettingsItemType.FocalCommodity, project.AssessmentSettings.FocalCommodity);
            if (project.AssessmentSettings.FocalCommodity) {
                section.SummarizeSetting(SettingsItemType.FocalCommodityReplacementMethod, cms.FocalCommodityReplacementMethod);
                if (cms.FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.AppendSamples
                    || cms.FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.ReplaceSamples) {
                    section.SummarizeSetting(SettingsItemType.FocalFoods, string.Join(", ", project.FocalFoods.Select(r => r.CodeFood).Distinct()));
                } else {
                    section.SummarizeSetting(SettingsItemType.UseDeterministicSubstanceConversionsForFocalCommodity, cms.UseDeterministicSubstanceConversionsForFocalCommodity);
                    section.SummarizeSetting(SettingsItemType.FocalFoods, string.Join(", ", project.FocalFoods.Select(r => r.CodeFood).Distinct()));
                    section.SummarizeSetting(SettingsItemType.FocalSubstances, string.Join(", ", project.FocalFoods.Select(r => r.CodeSubstance).Distinct()));
                    if (cms.FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.ReplaceSubstanceConcentrationsByLimitValue
                        || cms.FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.ReplaceSubstances
                    ) {
                        section.SummarizeSetting(SettingsItemType.FocalCommodityScenarioOccurrencePercentage, cms.FocalCommodityScenarioOccurrencePercentage);
                        section.SummarizeSetting(SettingsItemType.FocalCommodityConcentrationAdjustmentFactor, cms.FocalCommodityConcentrationAdjustmentFactor);
                        section.SummarizeSetting(SettingsItemType.UseDeterministicSubstanceConversionsForFocalCommodity, cms.UseDeterministicSubstanceConversionsForFocalCommodity);
                    }
                }

            }

            section.SummarizeSetting(SettingsItemType.SampleSubsetSelection, project.SubsetSettings.SampleSubsetSelection);
            if (project.SubsetSettings.SampleSubsetSelection) {
                if ((project.LocationSubsetDefinition?.AlignSubsetWithPopulation ?? false)
                    || (project.LocationSubsetDefinition?.LocationSubset?.Any() ?? false)
                ) {
                    section.SummarizeSetting(SettingsItemType.AlignSampleLocationSubsetWithPopulation, project.LocationSubsetDefinition.AlignSubsetWithPopulation, isVisible: project.LocationSubsetDefinition.AlignSubsetWithPopulation);
                    if (!project.LocationSubsetDefinition.AlignSubsetWithPopulation) {
                        section.SummarizeSetting(SettingsItemType.FilterSamplesByLocation, string.Join(", ", project.LocationSubsetDefinition.LocationSubset.Distinct()));
                    }
                    section.SummarizeSetting(SettingsItemType.IncludeMissingLocationRecords, project.LocationSubsetDefinition.IncludeMissingValueRecords);
                }
                if ((project.PeriodSubsetDefinition?.AlignSampleDateSubsetWithPopulation ?? false)
                    || (project.PeriodSubsetDefinition?.YearsSubset?.Any() ?? false)
                ) {
                    section.SummarizeSetting(SettingsItemType.AlignSampleDateSubsetWithPopulation, project.PeriodSubsetDefinition.AlignSampleDateSubsetWithPopulation, isVisible: project.PeriodSubsetDefinition.AlignSampleDateSubsetWithPopulation);
                    if (!project.PeriodSubsetDefinition.AlignSampleDateSubsetWithPopulation) {
                        section.SummarizeSetting(SettingsItemType.FilterSamplesByYear, string.Join(", ", project.PeriodSubsetDefinition.YearsSubset.Distinct()));
                    }
                }
                if ((project.PeriodSubsetDefinition?.AlignSampleSeasonSubsetWithPopulation ?? false) 
                    || (project.PeriodSubsetDefinition?.MonthsSubset?.Any() ?? false)
                ) {
                    section.SummarizeSetting(SettingsItemType.AlignSampleSeasonSubsetWithPopulation, project.PeriodSubsetDefinition.AlignSampleSeasonSubsetWithPopulation, isVisible: project.PeriodSubsetDefinition.AlignSampleSeasonSubsetWithPopulation);
                    if (!project.PeriodSubsetDefinition.AlignSampleSeasonSubsetWithPopulation) {
                        section.SummarizeSetting(SettingsItemType.FilterSamplesByMonth, string.Join(", ", project.PeriodSubsetDefinition.MonthsSubset.Distinct()));
                    }
                }

                if ((project.PeriodSubsetDefinition?.YearsSubset?.Any() ?? false)
                    || (project.PeriodSubsetDefinition?.AlignSampleDateSubsetWithPopulation ?? false)
                    || (project.PeriodSubsetDefinition?.MonthsSubset?.Any() ?? false)
                    || (project.PeriodSubsetDefinition?.AlignSampleSeasonSubsetWithPopulation ?? false)
                ) {
                    section.SummarizeSetting(SettingsItemType.IncludeMissingDateValueRecords, project.PeriodSubsetDefinition.IncludeMissingValueRecords);
                }

                if (project.SamplesSubsetDefinitions?.Any() ?? false) {
                    foreach (var subset in project.SamplesSubsetDefinitions) {
                        if (subset.AlignSubsetWithPopulation) {
                            section.SummarizeSetting($"Sample subset on {subset.PropertyName}", "Aligned with population");
                        } else {
                            section.SummarizeSetting($"Sample subset on {subset.PropertyName}", string.Join(", ", subset.KeyWords.Distinct()));
                        }
                        section.SummarizeSetting($"Include records with unspecified {subset.PropertyName}", subset.IncludeMissingValueRecords);
                    }
                }
            }

            return section;
        }
    }
}
