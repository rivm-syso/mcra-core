using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.General.SettingsDefinitions;
using MCRA.Simulation.Action;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.Concentrations {

    public sealed class ConcentrationsSettingsSummarizer : ActionModuleSettingsSummarizer<ConcentrationsModuleConfig> {
        public ConcentrationsSettingsSummarizer(ConcentrationsModuleConfig config) : base(config) {
        }

        public override ActionSettingsSummary Summarize(ProjectDto proj) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());

            section.SummarizeSetting(SettingsItemType.SelectedTier, _configuration.SelectedTier);
            section.SummarizeSetting(SettingsItemType.RestrictToModelledFoodSubset, _configuration.RestrictToModelledFoodSubset);
            section.SummarizeSetting(SettingsItemType.FilterConcentrationLimitExceedingSamples, _configuration.FilterConcentrationLimitExceedingSamples);
            if (_configuration.FilterConcentrationLimitExceedingSamples) {
                section.SummarizeSetting(SettingsItemType.ConcentrationLimitFilterFractionExceedanceThreshold, _configuration.ConcentrationLimitFilterFractionExceedanceThreshold);
            }
            section.SummarizeSetting(SettingsItemType.UseComplexResidueDefinitions, _configuration.UseComplexResidueDefinitions);
            if (_configuration.UseComplexResidueDefinitions) {
                section.SummarizeSetting(SettingsItemType.SubstanceTranslationAllocationMethod, _configuration.SubstanceTranslationAllocationMethod);
                section.SummarizeSetting(SettingsItemType.RetainAllAllocatedSubstancesAfterAllocation, _configuration.RetainAllAllocatedSubstancesAfterAllocation);
                section.SummarizeSetting(SettingsItemType.ConsiderAuthorisationsForSubstanceConversion, _configuration.ConsiderAuthorisationsForSubstanceConversion);
                if (_configuration.TryFixDuplicateAllocationInconsistencies) {
                    section.SummarizeSetting(SettingsItemType.TryFixDuplicateAllocationInconsistencies, _configuration.TryFixDuplicateAllocationInconsistencies);
                }
            }
            section.SummarizeSetting(SettingsItemType.ExtrapolateConcentrations, _configuration.ExtrapolateConcentrations);
            if (_configuration.ExtrapolateConcentrations) {
                section.SummarizeSetting(SettingsItemType.ThresholdForExtrapolation, _configuration.ThresholdForExtrapolation);
                section.SummarizeSetting(SettingsItemType.ConsiderMrlForExtrapolations, _configuration.ConsiderMrlForExtrapolations);
                section.SummarizeSetting(SettingsItemType.ConsiderAuthorisationsForExtrapolations, _configuration.ConsiderAuthorisationsForExtrapolations);
            }
            section.SummarizeSetting(SettingsItemType.ImputeWaterConcentrations, _configuration.ImputeWaterConcentrations);
            if (_configuration.ImputeWaterConcentrations) {
                section.SummarizeSetting(SettingsItemType.CodeWater, _configuration.CodeWater);
                section.SummarizeSetting(SettingsItemType.WaterConcentrationValue, _configuration.WaterConcentrationValue);
                section.SummarizeSetting(SettingsItemType.RestrictWaterImputationToAuthorisedUses, _configuration.RestrictWaterImputationToAuthorisedUses);
                section.SummarizeSetting(SettingsItemType.RestrictWaterImputationToApprovedSubstances, _configuration.RestrictWaterImputationToApprovedSubstances);
            }
            section.SummarizeSetting(SettingsItemType.FocalCommodity, _configuration.FocalCommodity);
            if (_configuration.FocalCommodity) {
                section.SummarizeSetting(SettingsItemType.FocalCommodityReplacementMethod, _configuration.FocalCommodityReplacementMethod);
                if (_configuration.FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.AppendSamples
                    || _configuration.FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.ReplaceSamples) {
                    section.SummarizeSetting(SettingsItemType.FocalFoods, string.Join(", ", _configuration.FocalFoods.Select(r => r.CodeFood).Distinct()));
                } else {
                    if (_configuration.FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.ReplaceSubstanceConcentrationsByProposedLimitValue) { 
                        section.SummarizeSetting(SettingsItemType.FocalCommodityProposedConcentrationLimit, _configuration.FocalCommodityProposedConcentrationLimit);
                    }
                    section.SummarizeSetting(SettingsItemType.UseDeterministicSubstanceConversionsForFocalCommodity, _configuration.UseDeterministicSubstanceConversionsForFocalCommodity);
                    section.SummarizeSetting(SettingsItemType.FocalFoods, string.Join(", ", _configuration.FocalFoods.Select(r => r.CodeFood).Distinct()));
                    section.SummarizeSetting(SettingsItemType.FocalSubstances, string.Join(", ", _configuration.FocalFoods.Select(r => r.CodeSubstance).Distinct()));
                    if (_configuration.FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.ReplaceSubstanceConcentrationsByLimitValue
                        || _configuration.FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.ReplaceSubstanceConcentrationsByProposedLimitValue
                        || _configuration.FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.ReplaceSubstances
                    ) {
                        section.SummarizeSetting(SettingsItemType.FocalCommodityScenarioOccurrencePercentage, _configuration.FocalCommodityScenarioOccurrencePercentage);
                        section.SummarizeSetting(SettingsItemType.FocalCommodityConcentrationAdjustmentFactor, _configuration.FocalCommodityConcentrationAdjustmentFactor);
                        section.SummarizeSetting(SettingsItemType.UseDeterministicSubstanceConversionsForFocalCommodity, _configuration.UseDeterministicSubstanceConversionsForFocalCommodity);
                    }
                    section.SummarizeSetting(SettingsItemType.FocalCommodityIncludeProcessedDerivatives, _configuration.FocalCommodityIncludeProcessedDerivatives);
                }
                section.SummarizeSetting(SettingsItemType.FilterProcessedFocalCommoditySamples, _configuration.FilterProcessedFocalCommoditySamples);
            }

            section.SummarizeSetting(SettingsItemType.SampleSubsetSelection, _configuration.SampleSubsetSelection);
            if (_configuration.SampleSubsetSelection) {
                if ((_configuration.LocationSubsetDefinition?.AlignSubsetWithPopulation ?? false)
                    || (_configuration.LocationSubsetDefinition?.LocationSubset?.Count > 0)
                ) {
                    section.SummarizeSetting(SettingsItemType.AlignSampleLocationSubsetWithPopulation, _configuration.LocationSubsetDefinition.AlignSubsetWithPopulation, isVisible: _configuration.LocationSubsetDefinition.AlignSubsetWithPopulation);
                    if (!_configuration.LocationSubsetDefinition.AlignSubsetWithPopulation) {
                        section.SummarizeSetting(SettingsItemType.FilterSamplesByLocation, string.Join(", ", _configuration.LocationSubsetDefinition.LocationSubset.Distinct()));
                    }
                    section.SummarizeSetting(SettingsItemType.IncludeMissingLocationRecords, _configuration.LocationSubsetDefinition.IncludeMissingValueRecords);
                }
                if ((_configuration.PeriodSubsetDefinition?.AlignSampleDateSubsetWithPopulation ?? false)
                    || (_configuration.PeriodSubsetDefinition?.YearsSubset?.Count > 0)
                ) {
                    section.SummarizeSetting(SettingsItemType.AlignSampleDateSubsetWithPopulation, _configuration.PeriodSubsetDefinition.AlignSampleDateSubsetWithPopulation, isVisible: _configuration.PeriodSubsetDefinition.AlignSampleDateSubsetWithPopulation);
                    if (!_configuration.PeriodSubsetDefinition.AlignSampleDateSubsetWithPopulation) {
                        section.SummarizeSetting(SettingsItemType.FilterSamplesByYear, string.Join(", ", _configuration.PeriodSubsetDefinition.YearsSubset.Distinct()));
                    }
                }
                if ((_configuration.PeriodSubsetDefinition?.AlignSampleSeasonSubsetWithPopulation ?? false)
                    || (_configuration.PeriodSubsetDefinition?.MonthsSubset?.Count > 0)
                ) {
                    section.SummarizeSetting(SettingsItemType.AlignSampleSeasonSubsetWithPopulation, _configuration.PeriodSubsetDefinition.AlignSampleSeasonSubsetWithPopulation, isVisible: _configuration.PeriodSubsetDefinition.AlignSampleSeasonSubsetWithPopulation);
                    if (!_configuration.PeriodSubsetDefinition.AlignSampleSeasonSubsetWithPopulation) {
                        section.SummarizeSetting(SettingsItemType.FilterSamplesByMonth, string.Join(", ", _configuration.PeriodSubsetDefinition.MonthsSubset.Distinct()));
                    }
                }

                if ((_configuration.PeriodSubsetDefinition?.YearsSubset?.Count > 0)
                    || (_configuration.PeriodSubsetDefinition?.AlignSampleDateSubsetWithPopulation ?? false)
                    || (_configuration.PeriodSubsetDefinition?.MonthsSubset?.Count > 0)
                    || (_configuration.PeriodSubsetDefinition?.AlignSampleSeasonSubsetWithPopulation ?? false)
                ) {
                    section.SummarizeSetting(SettingsItemType.IncludeMissingDateValueRecords, _configuration.PeriodSubsetDefinition.IncludeMissingValueRecords);
                }

                if (_configuration.SamplesSubsetDefinitions?.Count > 0) {
                    foreach (var subset in _configuration.SamplesSubsetDefinitions) {
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
