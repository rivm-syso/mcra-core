using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.Concentrations {
    public enum ConcentrationsSections {
        SampleOriginsSection,
        SamplesByFoodSummarySection,
        SamplesByFoodSubstanceSection,
        SamplesByFoodActiveSubstanceSection,
        SamplesByPropertySection,
        ConcentrationLimitExceedancesSection,
        DataGapsAndExtrapolationSection,
        FocalCommodityConcentrationScenario,
    }

    public sealed class ConcentrationsSummarizer : ActionModuleResultsSummarizer<ConcentrationsModuleConfig, IConcentrationsActionResult> {
        public ConcentrationsSummarizer(ConcentrationsModuleConfig config): base(config) {
        }

        public override void Summarize(
            ActionModuleConfig sectionConfig,
            IConcentrationsActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var outputManager = new ModuleOutputSectionsManager<ConcentrationsSections>(sectionConfig, ActionType);
            if (!outputManager.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new ConcentrationDataSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            section.Summarize(data.MeasuredSubstanceSampleCollections.Values, data.AllCompounds);
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            subHeader.Units = collectUnits(data);

            var subOrder = 0;
            if (_configuration.FocalCommodity
                && outputManager.ShouldSummarize(ConcentrationsSections.FocalCommodityConcentrationScenario)) {
                summarizeFocalCommodityConcentrationScenarios(data, subHeader, subOrder++);
            }

            if (data.FoodSamples != null
                && outputManager.ShouldSummarize(ConcentrationsSections.SamplesByFoodSummarySection)
            ) {
                summarizeSamplesByFood(data, subHeader, subOrder++);
            }

            if (outputManager.ShouldSummarize(ConcentrationsSections.SamplesByPropertySection)) {
                summarizeSamplesByProperty(data, outputManager, subHeader, subOrder++);
            }

            if ((data.MeasuredSubstanceSampleCollections?.Values.Any(r => r.SampleCompoundRecords.Any()) ?? false)
                && outputManager.ShouldSummarize(ConcentrationsSections.SamplesByFoodSubstanceSection)
            ) {
                summarizeSamplesByFoodSubstance(data, subHeader, subOrder++);
            }

            if (_configuration.FilterConcentrationLimitExceedingSamples
                && (data.MaximumConcentrationLimits?.Count > 0)
                && outputManager.ShouldSummarize(ConcentrationsSections.ConcentrationLimitExceedancesSection)) {
                summarizeConcentrationLimitExceedances(data, subHeader, subOrder++);
            }

            if ((data.ExtrapolationCandidates?.Count > 0)
                && outputManager.ShouldSummarize(ConcentrationsSections.DataGapsAndExtrapolationSection)
            ) {
                summarizeDataGapAndExtrapolations(data, subHeader, subOrder++);
            }

            if (data.MeasuredSubstanceSampleCollections != data.ActiveSubstanceSampleCollections && outputManager.ShouldSummarize(ConcentrationsSections.SamplesByFoodActiveSubstanceSection)) {
                summarizeSamplesByFoodActiveSubstance(data, subHeader, subOrder++);
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

        private void summarizeConcentrationLimitExceedances(ActionData data, SectionHeader header, int order) {
            var subOrder = 0;
            var subHeader = header.AddEmptySubSectionHeader(
                "Concentration limit exceedances",
                order,
                getSectionLabel(ConcentrationsSections.ConcentrationLimitExceedancesSection)
            );
            var exceedancesByFoodSection = new ConcentrationLimitExceedancesByFoodDataSection();
            var exceedancesByFoodSectionHeader = subHeader.AddSubSectionHeaderFor(exceedancesByFoodSection, "Concentration limit exceedances by food", subOrder++);
            exceedancesByFoodSection.Summarize(data.MaximumConcentrationLimits.Values, data.FoodSamples, _configuration.ConcentrationLimitFilterFractionExceedanceThreshold);
            exceedancesByFoodSectionHeader.SaveSummarySection(exceedancesByFoodSection);
            if (data.AllCompounds.Count > 1 && exceedancesByFoodSection.Records.Count != 0) {
                var exceedancesByFoodSubstanceSection = new ConcentrationLimitExceedancesDataSection();
                var exceedancesSectionHeader = subHeader.AddSubSectionHeaderFor(exceedancesByFoodSubstanceSection, "Concentration limit exceedances by food and substance", subOrder++);
                exceedancesByFoodSubstanceSection.Summarize(data.MaximumConcentrationLimits.Values, data.FoodSamples, data.ConcentrationUnit, _configuration.ConcentrationLimitFilterFractionExceedanceThreshold);
                exceedancesSectionHeader.SaveSummarySection(exceedancesByFoodSubstanceSection);
            }
        }

        private void summarizeFocalCommodityConcentrationScenarios(ActionData data, SectionHeader header, int order) {
            if ((data.FocalCommodityCombinations?.Count > 0)) {

                if (_configuration.FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.ReplaceSubstanceConcentrationsByLimitValue
                    && (data.MaximumConcentrationLimits?.Count > 0)
                ) {
                    var section = new FocalCommodityConcentrationScenarioSection() {
                        SectionLabel = getSectionLabel(ConcentrationsSections.FocalCommodityConcentrationScenario)
                    };
                    var subHeader = header.AddSubSectionHeaderFor(
                        section,
                        "Focal commodity concentration scenario",
                        order
                    );
                    section.SummarizeConcentrationLimits(_configuration, data.FocalCommodityCombinations, data.MaximumConcentrationLimits);
                    subHeader.SaveSummarySection(section);
                } else if (_configuration.FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.ReplaceSubstances
                     && (data.FocalCommoditySamples?.Count > 0)) {
                    var section = new FocalCommodityConcentrationScenarioSection() {
                        SectionLabel = getSectionLabel(ConcentrationsSections.FocalCommodityConcentrationScenario)
                    };
                    var subHeader = header.AddSubSectionHeaderFor(
                        section,
                        "Focal commodity concentration scenario",
                        order
                    );
                    section.SummarizeReplaceSubstances(
                        _configuration,
                        data.FocalCommodityCombinations,
                        data.FocalCommoditySubstanceSampleCollections,
                        data.ConcentrationUnit
                    );
                    subHeader.SaveSummarySection(section);
                }
            }
        }

        private int summarizeSamplesByProperty(ActionData data, ModuleOutputSectionsManager<ConcentrationsSections> outputSettings, SectionHeader header, int order) {
            if (_configuration.SampleSubsetSelection) {
                var subHeader = header.AddEmptySubSectionHeader("Samples by property", order, ConcentrationsSections.SamplesByPropertySection.ToString());
                var subOrder = 1;
                summarizeSampleOrigin(data, subHeader, subOrder);
                if (data.FoodSamples.SelectMany(s => s.Select(r => r.DateSampling?.Year ?? -1)).Distinct().Count() > 1
                    || _configuration.PeriodSubsetDefinition?.YearsSubset != null
                ) {
                    Func<FoodSample, string> propertyExtractor = s => s.DateSampling?.Year.ToString();
                    summarizeSampleProperties(data, "year", propertyExtractor, subHeader, subOrder++);
                }

                if (data.FoodSamples.SelectMany(s => s.Select(r => r.DateSampling?.Month ?? -1)).Distinct().Count() > 1
                    || _configuration.PeriodSubsetDefinition?.MonthsSubset != null
                ) {
                    Func<FoodSample, string> propertyExtractor = s => s.DateSampling?.Month.ToString();
                    summarizeSampleProperties(data, "month", propertyExtractor, subHeader, subOrder++);
                }

                if (data.FoodSamples?.Any(r => r.Any(s => !string.IsNullOrEmpty(s.Region))) ?? false) {
                    Func<FoodSample, string> propertyExtractor = s => s.Region;
                    summarizeSampleProperties(data, "region", propertyExtractor, subHeader, subOrder++);
                }
                if (data.FoodSamples?.Any(r => r.Any(s => !string.IsNullOrEmpty(s.ProductionMethod))) ?? false) {
                    Func<FoodSample, string> propertyExtractor = s => s.ProductionMethod;
                    summarizeSampleProperties(data, "production method", propertyExtractor, subHeader, subOrder++);
                }
            } else {
                if (data.SampleOriginInfos != null && outputSettings.ShouldSummarize(ConcentrationsSections.SampleOriginsSection)) {
                    summarizeSampleOrigin(data, header, order);
                }
            }

            return order;
        }

        private void summarizeSamplesByFood(ActionData data, SectionHeader header, int order) {
            var section = new SamplesByFoodSummarySection() {
                SectionLabel = getSectionLabel(ConcentrationsSections.SamplesByFoodSummarySection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Samples by food",
                order
            );
            section.Summarize(data.FoodSamples);
            subHeader.SaveSummarySection(section);
        }

        private void summarizeSampleOrigin(ActionData data, SectionHeader header, int order) {
            var section = new SampleOriginsSummarySection() {
                SectionLabel = getSectionLabel(ConcentrationsSections.SampleOriginsSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Sample origins",
                order
            );
            section.Summarize(data.SampleOriginInfos.SelectMany(r => r.Value).ToList());
            subHeader.SaveSummarySection(section);
        }

        private void summarizeSampleProperties(ActionData data, string displayName, Func<FoodSample, string> propertyValueExtractor, SectionHeader header, int order) {
            var section = new SamplePropertyDataSection() {
                SectionLabel = getSectionLabel(ConcentrationsSections.SampleOriginsSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                $"Samples by {displayName}",
                order
            );
            section.Summarize(data.FoodSamples, displayName, propertyValueExtractor);
            subHeader.SaveSummarySection(section);
        }

        private void summarizeSamplesByFoodActiveSubstance(ActionData data, SectionHeader header, int order) {
            var section = new SamplesByFoodSubstanceSection() {
                SectionLabel = getSectionLabel(ConcentrationsSections.SamplesByFoodActiveSubstanceSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Samples by food and active substance",
                order
            );
            section.Summarize(
                data.ActiveSubstanceSampleCollections?.Values,
                _configuration.UseDeterministicSubstanceConversionsForFocalCommodity
                    ? data.FocalCommodityCombinations : null,
                _configuration.VariabilityLowerPercentage,
                _configuration.VariabilityUpperPercentage
            );
            subHeader.SaveSummarySection(section);
        }

        private void summarizeSamplesByFoodSubstance(ActionData data, SectionHeader header, int order) {
            var section = new SamplesByFoodSubstanceSection() {
                SectionLabel = getSectionLabel(ConcentrationsSections.SamplesByFoodSubstanceSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Samples by food and substance",
                order
            );
            section.Summarize(
                data.MeasuredSubstanceSampleCollections.Values,
                !_configuration.UseDeterministicSubstanceConversionsForFocalCommodity
                    ? data.FocalCommodityCombinations : null,
                _configuration.VariabilityLowerPercentage,
                _configuration.VariabilityUpperPercentage
            );
            subHeader.SaveSummarySection(section);
        }
        private void summarizeDataGapAndExtrapolations(ActionData data, SectionHeader header, int order) {
            var section = new ConcentrationExtrapolationsSummarySection() {
                SectionLabel = getSectionLabel(ConcentrationsSections.DataGapsAndExtrapolationSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Data gaps and extrapolation",
                order
            );
            section.Summarize(data.ExtrapolationCandidates);
            subHeader.SaveSummarySection(section);
        }
    }
}
