using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.Concentrations {
    public enum ConcentrationsSections {
        SampleOriginsSection,
        AnalyticalMethodsSection,
        SamplesByFoodSubstanceSection,
        SamplesByFoodActiveSubstanceSection,
        SamplesByPropertySection,
        ConcentrationLimitExceedancesSection,
        DataGapsAndExtrapolationSection,
        FocalCommodityConcentrationScenario,
    }

    public sealed class ConcentrationsSummarizer : ActionResultsSummarizerBase<IConcentrationsActionResult> {

        public override ActionType ActionType => ActionType.Concentrations;

        public override void Summarize(ProjectDto project, IConcentrationsActionResult result, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<ConcentrationsSections>(project, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section1 = new ConcentrationDataSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            section1.Summarize(data.MeasuredSubstanceSampleCollections, data.AllCompounds);
            var subHeader = header.AddSubSectionHeaderFor(section1, ActionType.GetDisplayName(), order);
            subHeader.Units = collectUnits(project, data);

            var subOrder = 0;
            if (project.AssessmentSettings.FocalCommodity
                && outputSettings.ShouldSummarize(ConcentrationsSections.FocalCommodityConcentrationScenario)) {
                summarizeFocalCommodityConcentrationScenarios(project, data, subHeader, subOrder++);
            }

            summarizeSamplesByProperty(project, data, outputSettings, subHeader, subOrder++);

            if (data.FoodSamples.SelectMany(s => s).Any(r => r.SampleAnalyses.Any(sa => sa.AnalyticalMethod != null)) && outputSettings.ShouldSummarize(ConcentrationsSections.AnalyticalMethodsSection)) {
                summarizeAnalyticalMethods(project, data, subHeader, subOrder++);
            }

            if (project.ConcentrationModelSettings.FilterConcentrationLimitExceedingSamples
                && (data.MaximumConcentrationLimits?.Any() ?? false)
                && outputSettings.ShouldSummarize(ConcentrationsSections.ConcentrationLimitExceedancesSection)) {
                summarizeConcentrationLimitExceedances(project, data, subHeader, subOrder++);
            }

            if (data.MeasuredSubstanceSampleCollections?.Any(r => r.SampleCompoundRecords.Any()) ?? false && outputSettings.ShouldSummarize(ConcentrationsSections.SamplesByFoodSubstanceSection)) {
                summarizeSamplesByFoodSubstance(project, data, subHeader, subOrder++);
            }

            if (data.ExtrapolationCandidates?.Any() ?? false && outputSettings.ShouldSummarize(ConcentrationsSections.DataGapsAndExtrapolationSection)) {
                summarizeDataGapAndExtrapolations(project, data, subHeader, subOrder++);
            }

            if (data.MeasuredSubstanceSampleCollections != data.ActiveSubstanceSampleCollections && outputSettings.ShouldSummarize(ConcentrationsSections.SamplesByFoodActiveSubstanceSection)) {
                summarizeSamplesByFoodActiveSubstance(project, data, subHeader, subOrder++);
            }
            subHeader.SaveSummarySection(section1);
        }

        private static List<ActionSummaryUnitRecord> collectUnits(ProjectDto project, ActionData data) {
            var result = new List<ActionSummaryUnitRecord>();
            result.Add(new ActionSummaryUnitRecord("ConcentrationUnit", data.ConcentrationUnit.GetShortDisplayName()));
            result.Add(new ActionSummaryUnitRecord("LowerPercentage", $"p{project.OutputDetailSettings.LowerPercentage}"));
            result.Add(new ActionSummaryUnitRecord("UpperPercentage", $"p{project.OutputDetailSettings.UpperPercentage}"));
            return result;
        }

        private void summarizeConcentrationLimitExceedances(ProjectDto project, ActionData data, SectionHeader header, int order) {
            var subHeader = header.AddEmptySubSectionHeader(
                "Concentration limit exceedances",
                order,
                getSectionLabel(ConcentrationsSections.ConcentrationLimitExceedancesSection)
             );
            var subOrder = 0;
            if (data.MaximumConcentrationLimits != null) {
                var exceedancesSection = new ConcentrationLimitExceedancesDataSection();
                var exceedancesSectionHeader = subHeader.AddSubSectionHeaderFor(exceedancesSection, "Concentration limit exceedances by food and substance", subOrder++);
                exceedancesSection.Summarize(data.MaximumConcentrationLimits.Values, data.FoodSamples, data.ConcentrationUnit, project.ConcentrationModelSettings.ConcentrationLimitFilterFractionExceedanceThreshold);
                exceedancesSectionHeader.SaveSummarySection(exceedancesSection);
                if (data.AllCompounds.Count > 1 && exceedancesSection.Records.Any()) {
                    var exceedancesByFoodSection = new ConcentrationLimitExceedancesByFoodDataSection();
                    var exceedancesByFoodSectionHeader = subHeader.AddSubSectionHeaderFor(exceedancesByFoodSection, "Concentration limit exceedances by food", subOrder++);
                    exceedancesByFoodSection.Summarize(data.MaximumConcentrationLimits.Values, data.FoodSamples, project.ConcentrationModelSettings.ConcentrationLimitFilterFractionExceedanceThreshold);
                    exceedancesByFoodSectionHeader.SaveSummarySection(exceedancesByFoodSection);
                }
            }
        }

        private void summarizeAnalyticalMethods(ProjectDto project, ActionData data, SectionHeader header, int order) {
            var section = new AnalyticalMethodsSummarySection() {
                SectionLabel = getSectionLabel(ConcentrationsSections.AnalyticalMethodsSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Analytical methods",
                order++
            );
            section.Summarize(data.FoodSamples.SelectMany(s => s).ToList(), data.MeasuredSubstances);
            subHeader.SaveSummarySection(section);
        }

        private void summarizeFocalCommodityConcentrationScenarios(ProjectDto project, ActionData data, SectionHeader header, int order) {
            if ((data.FocalCommodityCombinations?.Any() ?? false)) {

                if (project.ConcentrationModelSettings.FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.ReplaceSubstanceConcentrationsByLimitValue
                    && (data.MaximumConcentrationLimits?.Any() ?? false)
                ) {
                    var section = new FocalCommodityConcentrationScenarioSection() {
                        SectionLabel = getSectionLabel(ConcentrationsSections.FocalCommodityConcentrationScenario)
                    };
                    var subHeader = header.AddSubSectionHeaderFor(
                        section,
                        "Focal commodity concentration scenario",
                        order
                    );
                    section.SummarizeConcentrationLimits(project, data.FocalCommodityCombinations, data.MaximumConcentrationLimits);
                    subHeader.SaveSummarySection(section);
                } else if (project.ConcentrationModelSettings.FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.ReplaceSubstances
                     && (data.FocalCommoditySamples?.Any() ?? false)) {
                    var section = new FocalCommodityConcentrationScenarioSection() {
                        SectionLabel = getSectionLabel(ConcentrationsSections.FocalCommodityConcentrationScenario)
                    };
                    var subHeader = header.AddSubSectionHeaderFor(
                        section,
                        "Focal commodity concentration scenario",
                        order
                    );
                    section.SummarizeReplaceSubstances(
                        project,
                        data.FocalCommodityCombinations,
                        data.FocalCommoditySubstanceSampleCollections,
                        data.ConcentrationUnit
                    );
                    subHeader.SaveSummarySection(section);
                }
            }
        }

        private int summarizeSamplesByProperty(ProjectDto project, ActionData data, ModuleOutputSectionsManager<ConcentrationsSections> outputSettings, SectionHeader header, int order) {
            if (project.SubsetSettings.SampleSubsetSelection) {
                var subHeader = header.AddEmptySubSectionHeader("Samples by property", order, ConcentrationsSections.SamplesByPropertySection.ToString());
                var subOrder = 1;
                summarizeSampleOrigin(project, data, subHeader, subOrder);
                if (data.FoodSamples.SelectMany(s => s.Select(r => r.DateSampling?.Year ?? -1)).Distinct().Count() > 1
                    || project.PeriodSubsetDefinition?.YearsSubset != null
                ) {
                    Func<FoodSample, string> propertyExtractor = s => s.DateSampling?.Year.ToString();
                    summarizeSampleProperties(data, "year", propertyExtractor, subHeader, subOrder++);
                }

                if (data.FoodSamples.SelectMany(s => s.Select(r => r.DateSampling?.Month ?? -1)).Distinct().Count() > 1
                    || project.PeriodSubsetDefinition?.MonthsSubset != null
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
                    summarizeSampleOrigin(project, data, header, order);
                }
            }

            return order;
        }

        private void summarizeSampleOrigin(ProjectDto project, ActionData data, SectionHeader header, int order) {
            var section = new SampleOriginDataSection() {
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

        private void summarizeSamplesByFoodActiveSubstance(ProjectDto project, ActionData data, SectionHeader header, int order) {
            var section = new SamplesByFoodSubstanceSection() {
                SectionLabel = getSectionLabel(ConcentrationsSections.SamplesByFoodActiveSubstanceSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Samples by food and active substance",
                order
            );
            section.Summarize(
                data.ActiveSubstanceSampleCollections,
                project.ConcentrationModelSettings.UseDeterministicSubstanceConversionsForFocalCommodity
                    ? data.FocalCommodityCombinations : null,
                project.OutputDetailSettings.LowerPercentage,
                project.OutputDetailSettings.UpperPercentage
            );
            subHeader.SaveSummarySection(section);
        }

        private void summarizeSamplesByFoodSubstance(ProjectDto project, ActionData data, SectionHeader header, int order) {
            var section = new SamplesByFoodSubstanceSection() {
                SectionLabel = getSectionLabel(ConcentrationsSections.SamplesByFoodSubstanceSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Samples by food and substance",
                order
            );
            section.Summarize(
                data.MeasuredSubstanceSampleCollections,
                !project.ConcentrationModelSettings.UseDeterministicSubstanceConversionsForFocalCommodity
                    ? data.FocalCommodityCombinations : null,
                project.OutputDetailSettings.LowerPercentage,
                project.OutputDetailSettings.UpperPercentage
            );
            subHeader.SaveSummarySection(section);
        }
        private void summarizeDataGapAndExtrapolations(ProjectDto project, ActionData data, SectionHeader header, int order) {
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
