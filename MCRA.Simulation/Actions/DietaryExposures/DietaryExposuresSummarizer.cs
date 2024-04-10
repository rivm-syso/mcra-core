using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.ExposureLevelsCalculation;
using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.Calculators.IntakeModelling.ModelThenAddIntakeModelCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.OutputGeneration.ActionSummaries.DietaryExposures;
using MCRA.Simulation.OutputGeneration.Generic.Diagnostics;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.Actions.DietaryExposures {

    public enum DietaryExposuresSections {
        DailyIntakeDistributionSection,
        DetailsSection,
        ExposuresByFoodSection,
        ExposuresBySubstanceSection,
        CoExposuresSection,
        ExposuresByFoodAndSubstanceSection,
        ExposuresByProcessedFoodAndSubstanceSection,
        ExposureDistributionsBySubstanceSection,
        ObservedIndividualMeansByModelledFoodSection,
        McrCoExposureSection,
        DrilldownSection,
        DiagnosticsSection
    }

    public sealed class DietaryExposuresSummarizer : ActionResultsSummarizerBase<DietaryExposuresActionResult> {

        public override ActionType ActionType => ActionType.DietaryExposures;

        private readonly CompositeProgressState _progressState;

        public DietaryExposuresSummarizer(CompositeProgressState progressState = null) {
            _progressState = progressState;
        }

        public override void Summarize(
            ProjectDto project,
            DietaryExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var outputSettings = new ModuleOutputSectionsManager<DietaryExposuresSections>(project, ActionType);
            var substances = data.ActiveSubstances;

            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var outputSummary = new DietaryExposuresSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(outputSummary, ActionType.GetDisplayName(), order);
            subHeader.Units = collectUnits(project, data);
            subHeader.SaveSummarySection(outputSummary);

            int subOrder = 0;

            var referenceSubstance = data.ActiveSubstances.Count == 1 ? data.ActiveSubstances.First() : data.ReferenceSubstance;
            // Summarize model assisted usual intakes distribution (BBN, LNN0 or MTA)
            if (result.DietaryModelAssistedIntakes != null) {
                summarizeModelAssistedUsualIntakesDistribution(project, result, data, subHeader, subOrder++);
            }

            // Summarize ISUF usual intakes distribution
            if (project.IntakeModelSettings.IntakeModelType == IntakeModelType.ISUF) {
                summarizeIsufUsualIntakesDistribution(project, result, referenceSubstance, subHeader, subOrder++);
            }

            var subHeaderDetails = subHeader.AddEmptySubSectionHeader("Details", order, getSectionLabel(DietaryExposuresSections.DetailsSection));
            subHeaderDetails.SaveSummarySection(outputSummary);

            // Summarize OIM
            if (result.DietaryObservedIndividualMeans != null) {
                if (project.IntakeModelSettings.IntakeModelType != IntakeModelType.OIM || project.IntakeModelSettings.FirstModelThenAdd) {
                    summarizeOimDistribution(project, result, referenceSubstance, subHeaderDetails, subOrder++);
                } else {
                    summarizeOimDistribution(project, result, referenceSubstance, subHeader, subOrder++);
                }
            }

            // Model-then-add OIMs by food
            if (project.IntakeModelSettings.FirstModelThenAdd && (result.DietaryIndividualDayIntakes?.Any() ?? false)
                && outputSettings.ShouldSummarize(DietaryExposuresSections.ExposuresByFoodSection)
            ) {
                summarizeMtaDistributionsByModelledFood(project, result, data, subHeaderDetails, subOrder++);
            }

            // Model-then-add models by category
            if (result.IntakeModel is CompositeIntakeModel) {
                summarizeMtaModelsPerCategory(result, subHeaderDetails, subOrder++);
            }

            // Summarize estimates/diagnostics
            if (result.IntakeModel != null) {
                summarizeEstimatesAndDiagnostics(project, result, data, subHeaderDetails, subOrder++);
            }

            //Summarize marginals BBN, LNN0 and LNN
            if (result.DietaryModelBasedIntakeResults != null) {
                if (project.IntakeModelSettings.IntakeModelType == IntakeModelType.LNN && result.DietaryModelAssistedIntakes == null) {
                    summarizeModelBasedUsualIntakesDistributionMta(project, result, subHeader, referenceSubstance, subOrder++);
                } else {
                    summarizeModelBasedUsualIntakesDistributionMta(project, result, subHeaderDetails, referenceSubstance, subOrder++);
                }
            }

            // Summarize conditional distribution
            if (result.DietaryConditionalUsualIntakeResults != null) {
                summarizeConditionalUsualIntakeDistribution(project, result, data, subHeaderDetails, subOrder++);
            }

            // Daily intakes distribution
            if (result.DietaryIndividualDayIntakes != null
                && ((data.CorrectedRelativePotencyFactors?.Any() ?? false) || (substances?.Count == 1))
                && outputSettings.ShouldSummarize(DietaryExposuresSections.DailyIntakeDistributionSection)
            ) {
                if (result.DietaryObservedIndividualMeans != null) {
                    summarizeDailyIntakesDistribution(project, result, data, subHeaderDetails, subOrder++);
                } else {
                    summarizeDailyIntakesDistribution(project, result, data, subHeader, subOrder++);
                }
            }

            // Diagnostics
            if (project.DietaryIntakeCalculationSettings.VariabilityDiagnosticsAnalysis && result.DietaryIndividualDayIntakes != null
                && ((data.CorrectedRelativePotencyFactors?.Any() ?? false) || (substances?.Count == 1))
                && outputSettings.ShouldSummarize(DietaryExposuresSections.DiagnosticsSection)
            ) {
                summarizeDiagnostics(project, result, data, subHeader, subOrder++);
            }

            // Exposures by food summary contributions and pie
            if (result.DietaryIndividualDayIntakes != null
                && ((data.CorrectedRelativePotencyFactors?.Any() ?? false) || (substances?.Count == 1))
                && outputSettings.ShouldSummarize(DietaryExposuresSections.ExposuresByFoodSection)
            ) {
                summarizeDietaryExposuresByFood(project, result, data, subHeaderDetails, subOrder++);
            }

            // Exposures by substance summary contributions, pie and boxplot
            if (result.DietaryIndividualDayIntakes != null
                && substances.Count > 1
                && outputSettings.ShouldSummarize(DietaryExposuresSections.ExposuresBySubstanceSection)
            ) {
                if (data.CorrectedRelativePotencyFactors != null) {
                    summarizeExposuresBySubstance(project, result, data, subHeaderDetails, subOrder++);
                } else {
                    summarizeExposuresBySubstance(project, result, data, subHeader, subOrder++);
                }
            }

            // Co-exposures
            if (result.DietaryIndividualDayIntakes != null
                && substances.Count > 1
                && outputSettings.ShouldSummarize(DietaryExposuresSections.CoExposuresSection)
            ) {
                if (data.CorrectedRelativePotencyFactors != null) {
                    summarizeCoExposures(project, result, data, subHeaderDetails, subOrder++);
                } else {
                    summarizeCoExposures(project, result, data, subHeader, subOrder++);
                }
            }

            // MCR co-exposures
            if (project.MixtureSelectionSettings.IsMcrAnalysis
                && result.DietaryIndividualDayIntakes != null
                && substances.Count > 1
                && result.ExposureMatrix != null
                && outputSettings.ShouldSummarize(DietaryExposuresSections.McrCoExposureSection)
            ) {
                summarizeMaximumCumulativeRatio(project, result, data, subHeaderDetails, subOrder++);
            }

            // Exposure distributions by substance
            if (result.DietaryIndividualDayIntakes != null
                && substances.Count > 1
                && outputSettings.ShouldSummarize(DietaryExposuresSections.ExposureDistributionsBySubstanceSection)
            ) {
                if (data.CorrectedRelativePotencyFactors != null) {
                    summarizeExposureDistributionsBySubstance(project, result, data, subHeaderDetails, subOrder++);
                } else {
                    summarizeExposureDistributionsBySubstance(project, result, data, subHeader, subOrder++);
                }
            }

            // Exposures by food and substance
            if (result.DietaryIndividualDayIntakes != null
                && substances.Count > 1
                && outputSettings.ShouldSummarize(DietaryExposuresSections.ExposuresByFoodAndSubstanceSection)
            ) {
                summarizeExposuresByFoodAndSubstance(project, result, data, subHeaderDetails, subOrder++);
            }

            // Exposures by processed food and substance
            if (result.DietaryIndividualDayIntakes != null
                && ((data.CorrectedRelativePotencyFactors?.Any() ?? false) || (substances?.Count == 1))
                && project.ConcentrationModelSettings.IsProcessing
                && project.DietaryIntakeCalculationSettings.DietaryExposuresDetailsLevel == DietaryExposuresDetailsLevel.Full
                && (result.DietaryIndividualDayIntakes?.Any(r => r.DetailedIntakesPerFood?.Any() ?? false) ?? false)
                && outputSettings.ShouldSummarize(DietaryExposuresSections.ExposuresByProcessedFoodAndSubstanceSection)
            ) {
                summarizeExposuresByProcessedFoodAndSubstance(project, result, data, subHeaderDetails, subOrder++);
            }

            // TDS Reduction to limit scenario
            if (result?.TdsReductionFactors?.Any() ?? false) {
                summarizePotentialReductions(project, data, subHeaderDetails, subOrder++);
            }

            // Drilldown
            if (project.OutputDetailSettings.IsDetailedOutput
                && !project.OutputDetailSettings.SkipPrivacySensitiveOutputs
                && ((data.CorrectedRelativePotencyFactors?.Any() ?? false) || (substances?.Count == 1))
                && outputSettings.ShouldSummarize(DietaryExposuresSections.DrilldownSection)
            ) {
                summarizeIndividualDrilldown(project, result, data, subHeaderDetails, subOrder++);
            }
        }

        public void SummarizeUncertain(
            ProjectDto project,
            DietaryExposuresActionResult result,
            ActionData data,
            SectionHeader header
        ) {
            var outputSettings = new ModuleOutputSectionsManager<DietaryExposuresSections>(project, ActionType);
            var subHeader = header.GetSubSectionHeader<DietaryExposuresSummarySection>();
            if (subHeader == null) {
                return;
            }
            var outputSummary = (ActionSummarySectionBase)subHeader.GetSummarySection();
            var substances = data.ActiveSubstances;

            // Daily intake distribution
            if (result.DietaryIndividualDayIntakes != null
                && ((data.CorrectedRelativePotencyFactors?.Any() ?? false) || (substances?.Count == 1))
                && outputSettings.ShouldSummarize(DietaryExposuresSections.DailyIntakeDistributionSection)
            ) {
                subHeader = header.GetSubSectionHeader<DietaryIntakeDistributionSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as DietaryIntakeDistributionSection;
                    var rpfs = data.CorrectedRelativePotencyFactors ?? substances.ToDictionary(r => r, r => 1D);
                    var membershipProbabilities = data.MembershipProbabilities ?? substances.ToDictionary(r => r, r => 1D);
                    section.SummarizeUncertainty(
                        subHeader,
                        result.DietaryIndividualDayIntakes,
                        rpfs,
                        membershipProbabilities,
                        project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                        project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                        project.SubsetSettings.IsPerPerson
                    );
                    subHeader.SaveSummarySection(section);
                }
            }

            // Diagnostics
            if (project.DietaryIntakeCalculationSettings.VariabilityDiagnosticsAnalysis && result.DietaryIndividualDayIntakes != null
                && ((data.CorrectedRelativePotencyFactors?.Any() ?? false) || (substances?.Count == 1))
                && outputSettings.ShouldSummarize(DietaryExposuresSections.DiagnosticsSection)
            ) {
                subHeader = header.GetSubSectionHeader<DiagnosticsSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as DiagnosticsSection;
                    if (result.DietaryObservedIndividualMeans != null) {
                        var dietaryObservedIndividualMeans = result.DietaryObservedIndividualMeans.Shuffle(new McraRandomGenerator());
                        var intakes = dietaryObservedIndividualMeans.Select(c => c.DietaryIntakePerMassUnit).ToList();
                        var weights = dietaryObservedIndividualMeans.Select(c => c.IndividualSamplingWeight).ToList();
                        section.SummarizeUncertainty(intakes, weights, project.OutputDetailSettings.SelectedPercentiles);
                    } else {
                        var rpfs = data.CorrectedRelativePotencyFactors ?? substances.ToDictionary(r => r, r => 1D);
                        var intakes = result.DietaryIndividualDayIntakes
                            .Select(c => c.TotalExposurePerMassUnit(rpfs, data.MembershipProbabilities, project.SubsetSettings.IsPerPerson))
                            .ToList();
                        var weights = result.DietaryIndividualDayIntakes.Select(c => c.IndividualSamplingWeight).ToList();
                        section.SummarizeUncertainty(intakes, weights, project.OutputDetailSettings.SelectedPercentiles);
                    }
                    subHeader.SaveSummarySection(section);
                }
            }

            if (project.AssessmentSettings.ExposureType == ExposureType.Chronic) {
                // OIM
                if (result.DietaryObservedIndividualMeans != null) {
                    subHeader = header.GetSubSectionHeader<ChronicDietarySection>();
                    if (subHeader != null) {
                        var section = subHeader.GetSummarySection() as ChronicDietarySection;
                        section.ProgressState = _progressState;
                        section.SummarizeUncertainty(
                            subHeader,
                            result.DietaryObservedIndividualMeans,
                            project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                            project.UncertaintyAnalysisSettings.UncertaintyUpperBound);
                        subHeader.SaveSummarySection(section);
                    }
                }

                // Model-assisted
                if (result.DietaryModelAssistedIntakes != null && data.DesiredIntakeModelType != IntakeModelType.LNN) {
                    subHeader = header.GetSubSectionHeader<ChronicModelAssistedSection>();
                    if (subHeader != null) {
                        var section = subHeader.GetSummarySection() as ChronicModelAssistedSection;
                        section.ProgressState = _progressState;
                        section.SummarizeUncertainty(
                            subHeader,
                            result.DietaryModelAssistedIntakes,
                            project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                            project.UncertaintyAnalysisSettings.UncertaintyUpperBound
                        );
                        subHeader.SaveSummarySection(section);
                    }
                }

                // Conditional
                if (result.DietaryConditionalUsualIntakeResults != null) {
                    subHeader = header.GetSubSectionHeader<ChronicConditionalSection>();
                    if (subHeader != null) {
                        var section = subHeader.GetSummarySection() as ChronicConditionalSection;
                        section.ProgressState = _progressState;
                        section.SummarizeUncertainty(
                            subHeader,
                            result.DietaryConditionalUsualIntakeResults,
                            project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                            project.UncertaintyAnalysisSettings.UncertaintyUpperBound
                        );
                        subHeader.SaveSummarySection(section);
                    }
                }

                // Model-based
                if (result.DietaryModelBasedIntakeResults != null) {
                    var marginals = result.DietaryModelBasedIntakeResults.SelectMany(c => c.ModelBasedIntakes).ToList();
                    subHeader = header.GetSubSectionHeader<ChronicModelBasedSection>();
                    if (subHeader != null) {
                        var section = subHeader.GetSummarySection() as ChronicModelBasedSection;
                        section.ProgressState = _progressState;
                        section.SummarizeUncertainty(
                            subHeader,
                            marginals,
                            project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                            project.UncertaintyAnalysisSettings.UncertaintyUpperBound
                        );
                        subHeader.SaveSummarySection(section);
                    }
                }

                // ISUF
                if (project.IntakeModelSettings.IntakeModelType == IntakeModelType.ISUF && !project.IntakeModelSettings.FirstModelThenAdd) {
                    var intakeModel = result.IntakeModel as ISUFModel;
                    subHeader = header.GetSubSectionHeader<ChronicModelBasedSection>();
                    if (subHeader != null) {
                        var section = subHeader.GetSummarySection() as ChronicModelBasedSection;
                        section.ProgressState = _progressState;
                        section.SummarizeUncertainty(
                            subHeader,
                            intakeModel,
                            project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                            project.UncertaintyAnalysisSettings.UncertaintyUpperBound
                        );
                        subHeader.SaveSummarySection(section);
                    }
                }

                // Model-then-add OIMs per category
                if (result.IntakeModel is CompositeIntakeModel) {
                    subHeader = header.GetSubSectionHeader<UsualIntakeDistributionPerCategorySection>();
                    if (subHeader != null) {
                        var section = subHeader.GetSummarySection() as UsualIntakeDistributionPerCategorySection;
                        section.SummarizeUncertainty(
                            section.UsualIntakeDistributionPerCategoryModelSections,
                            result.IntakeModel as CompositeIntakeModel,
                            project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                            project.UncertaintyAnalysisSettings.UncertaintyUpperBound
                        );
                        subHeader.SaveSummarySection(section);
                    }
                }
            }

            // Contributions by food (as-eaten and as-measured)
            if ((data.CorrectedRelativePotencyFactors?.Any() ?? false) || substances?.Count == 1) {
                subHeader = header.GetSubSectionHeader<ExposureByFoodSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as ExposureByFoodSection;
                    section.SummarizeUncertain(
                        header,
                        data.AllFoods,
                        result.DietaryIndividualDayIntakes,
                        substances,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        project.AssessmentSettings.ExposureType,
                        project.OutputDetailSettings.PercentageForUpperTail,
                        project.SubsetSettings.IsPerPerson
                    );
                }
            }

            subHeader = header.GetSubSectionHeader<SubstancesOverviewSection>();
            if (subHeader != null) {
                var section = subHeader.GetSummarySection() as SubstancesOverviewSection;
                section.SummarizeUncertain(
                    subHeader,
                    substances,
                    result.DietaryIndividualDayIntakes,
                    project.AssessmentSettings.ExposureType,
                    project.SubsetSettings.IsPerPerson,
                    project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                    project.UncertaintyAnalysisSettings.UncertaintyUpperBound
                );
                subHeader.SaveSummarySection(section);
            }

            // Contributions by substance
            if (substances.Count > 1 && (data.CorrectedRelativePotencyFactors?.Any() ?? false)
                && outputSettings.ShouldSummarize(DietaryExposuresSections.ExposuresBySubstanceSection)
            ) {
                summarizeExposuresBySubstanceUncertain(project, result, data, header);
            }

            // Exposures by food and substance
            if (substances.Count > 1 && (data.CorrectedRelativePotencyFactors?.Any() ?? false)
                && outputSettings.ShouldSummarize(DietaryExposuresSections.ExposuresByFoodAndSubstanceSection)
            ) {
                summarizeExposuresByFoodAndSubstanceUncertain(project, result, data, header);
            }

            // Exposures by processed food and substance
            if (result.DietaryIndividualDayIntakes != null
                && project.ConcentrationModelSettings.IsProcessing
                && (substances.Count > 1 && (data.CorrectedRelativePotencyFactors?.Any() ?? false))
                && outputSettings.ShouldSummarize(DietaryExposuresSections.ExposuresByFoodAndSubstanceSection)
            ) {
                summarizedExposuresByProcessedFoodAndSubstanceUncertain(project, result, data, header);
            }
        }

        private static List<ActionSummaryUnitRecord> collectUnits(ProjectDto project, ActionData data) {
            var result = new List<ActionSummaryUnitRecord> {
                new ActionSummaryUnitRecord("ExposureType", project.AssessmentSettings.ExposureType.GetDisplayName()),
                new ActionSummaryUnitRecord("IntakeUnit", data.DietaryExposureUnit.GetShortDisplayName(TargetUnit.DisplayOption.AppendBiologicalMatrix)),
                new ActionSummaryUnitRecord("PerPersonIntakeUnit", $"{data.DietaryExposureUnit.SubstanceAmountUnit.GetShortDisplayName()}/day")
            };
            if (project.AssessmentSettings.ExposureType == ExposureType.Chronic) {
                result.Add(new ActionSummaryUnitRecord("IndividualDayUnit", "individuals"));
            } else {
                result.Add(new ActionSummaryUnitRecord("IndividualDayUnit", "individual days"));
            }
            result.Add(new ActionSummaryUnitRecord("BodyWeightUnit", data.BodyWeightUnit.GetDisplayName()));
            result.Add(new ActionSummaryUnitRecord("LowerPercentage", $"p{project.OutputDetailSettings.LowerPercentage}"));
            result.Add(new ActionSummaryUnitRecord("UpperPercentage", $"p{project.OutputDetailSettings.UpperPercentage}"));
            result.Add(new ActionSummaryUnitRecord("ConcentrationUnit", data.ConcentrationUnit.GetShortDisplayName()));
            result.Add(new ActionSummaryUnitRecord("ConsumptionUnit", data.ConsumptionUnit.GetShortDisplayName()));
            result.Add(new ActionSummaryUnitRecord("LowerBound", $"p{project.UncertaintyAnalysisSettings.UncertaintyLowerBound}"));
            result.Add(new ActionSummaryUnitRecord("UpperBound", $"p{project.UncertaintyAnalysisSettings.UncertaintyUpperBound}"));
            return result;
        }

        private void summarizeExposuresByProcessedFoodAndSubstance(
            ProjectDto project,
            DietaryExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            if (result.DietaryIndividualDayIntakes?.Any(r => r.DetailedIntakesPerFood?.Any() ?? false) ?? false) {
                var section = new ExposureByFoodSubstanceProcessingTypeSection() {
                    ProgressState = _progressState,
                    SectionLabel = getSectionLabel(DietaryExposuresSections.ExposuresByProcessedFoodAndSubstanceSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Exposures by processed food and substance",
                    order
                );
                subHeader.SaveSummarySection(section);
                var substances = data.ActiveSubstances;

                // Total section
                var totalSection = new TotalDistributionFoodAsMeasuredSubstanceProcessingTypeSection();
                var subSubHeader = subHeader.AddSubSectionHeaderFor(totalSection, "Total distribution", order++);
                totalSection.Summarize(
                    result.DietaryIndividualDayIntakes,
                    data.CorrectedRelativePotencyFactors ?? substances.ToDictionary(r => r, r => 1D),
                    data.MembershipProbabilities ?? substances.ToDictionary(r => r, r => 1D),
                    substances,
                    data.ModelledFoods,
                    data.ProcessingTypes,
                    project.AssessmentSettings.ExposureType,
                    project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                    project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                    project.SubsetSettings.IsPerPerson
                );
                subSubHeader.SaveSummarySection(totalSection);

                // Upper section
                var upperSection = new UpperDistributionModelledFoodSubstanceProcessingTypeSection();
                subSubHeader = subHeader.AddSubSectionHeaderFor(upperSection, "Upper distribution", order++);
                upperSection.Summarize(
                    result.DietaryIndividualDayIntakes,
                    data.CorrectedRelativePotencyFactors ?? substances.ToDictionary(r => r, r => 1D),
                    data.MembershipProbabilities ?? substances.ToDictionary(r => r, r => 1D),
                    substances,
                    data.ModelledFoods,
                    data.ProcessingTypes,
                    project.AssessmentSettings.ExposureType,
                    project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                    project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                    project.OutputDetailSettings.PercentageForUpperTail,
                    project.SubsetSettings.IsPerPerson
                );
                subSubHeader.SaveSummarySection(upperSection);
            }
        }

        private void summarizeExposuresByFoodAndSubstance(
            ProjectDto project,
            DietaryExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var section = new ExposureByFoodCompoundSection() {
                ProgressState = _progressState,
                SectionLabel = getSectionLabel(DietaryExposuresSections.ExposuresByFoodAndSubstanceSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Exposures by food and substance",
                order
            );
            section.Summarize(
                subHeader,
                result.DietaryIndividualDayIntakes,
                data.CorrectedRelativePotencyFactors,
                data.MembershipProbabilities,
                data.ModelledFoods,
                data.ActiveSubstances,
                project.AssessmentSettings.ExposureType,
                project.OutputDetailSettings.LowerPercentage,
                project.OutputDetailSettings.UpperPercentage,
                project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                project.OutputDetailSettings.PercentageForUpperTail,
                project.SubsetSettings.IsPerPerson
            );
            subHeader.SaveSummarySection(section);
        }

        private void summarizeExposureDistributionsBySubstance(
            ProjectDto project,
            DietaryExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var substancesOverViewSection = new SubstancesOverviewSection() {
                SectionLabel = getSectionLabel(DietaryExposuresSections.ExposureDistributionsBySubstanceSection)
            };
            var subSubHeader = header.AddSubSectionHeaderFor(
                substancesOverViewSection,
                "Exposure distributions by substance",
                order
            );
            substancesOverViewSection.Summarize(
                subSubHeader,
                result.DietaryIndividualDayIntakes,
                data.ActiveSubstances,
                null,
                project.AssessmentSettings.ExposureType,
                project.OutputDetailSettings.SelectedPercentiles,
                project.SubsetSettings.IsPerPerson,
                project.OutputDetailSettings.ExposureMethod,
                project.OutputDetailSettings.ExposureLevels
            );
            subSubHeader.SaveSummarySection(substancesOverViewSection);
        }

        private void summarizeMaximumCumulativeRatio(
            ProjectDto project,
            DietaryExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            if (result.DriverSubstances != null) {
                var mcrSection = new MaximumCumulativeRatioSection() {
                    SectionLabel = getSectionLabel(DietaryExposuresSections.McrCoExposureSection)
                };
                var mcrHeader = header.AddSubSectionHeaderFor(
                    mcrSection,
                    "MCR co-exposure",
                    order++
                );
                mcrSection.Summarize(
                    result.DriverSubstances,
                    data.DietaryExposureUnit,
                    project.MixtureSelectionSettings.McrExposureApproachType,
                    project.OutputDetailSettings.MaximumCumulativeRatioCutOff,
                    project.OutputDetailSettings.MaximumCumulativeRatioPercentiles,
                    project.MixtureSelectionSettings.TotalExposureCutOff,
                    project.OutputDetailSettings.MaximumCumulativeRatioMinimumPercentage,
                    project.OutputDetailSettings.SkipPrivacySensitiveOutputs
                );

                mcrSection.Summarize(
                    result.ExposureMatrix,
                    project.OutputDetailSettings.MaximumCumulativeRatioPercentiles,
                    project.OutputDetailSettings.MaximumCumulativeRatioMinimumPercentage
                );
                mcrHeader.SaveSummarySection(mcrSection);
            }
        }

        private void summarizeDietaryExposuresByFood(
            ProjectDto project,
            DietaryExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var section = new ExposureByFoodSection() {
                ProgressState = _progressState,
                SectionLabel = getSectionLabel(DietaryExposuresSections.ExposuresByFoodSection)
            };
            var sub2Header = header.AddSubSectionHeaderFor(
                section,
                "Exposures by food",
                order
            );
            section.Summarize(
                sub2Header,
                result.DietaryIndividualDayIntakes,
                data.AllFoods,
                data.FoodsAsEaten,
                data.ModelledFoods,
                data.ActiveSubstances,
                data.CorrectedRelativePotencyFactors,
                data.MembershipProbabilities,
                project.AssessmentSettings.TotalDietStudy,
                project.ConversionSettings.UseReadAcrossFoodTranslations,
                project.AssessmentSettings.ExposureType,
                project.OutputDetailSettings.LowerPercentage,
                project.OutputDetailSettings.UpperPercentage,
                project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                project.OutputDetailSettings.PercentageForUpperTail,
                project.SubsetSettings.IsPerPerson
            );
            sub2Header.SaveSummarySection(section);
        }

        /// <summary>
        /// Pie and summary table
        /// </summary>
        /// <param name="project"></param>
        /// <param name="result"></param>
        /// <param name="data"></param>
        /// <param name="header"></param>
        /// <param name="order"></param>
        private void summarizeExposuresBySubstance(
            ProjectDto project,
            DietaryExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var section = new ExposureByCompoundSection() {
                ProgressState = _progressState,
                SectionLabel = getSectionLabel(DietaryExposuresSections.ExposuresBySubstanceSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Exposures by substance",
                order
            );
            section.Summarize(
                subHeader,
                result.DietaryIndividualDayIntakes,
                result.ExposurePerCompoundRecords,
                data.CorrectedRelativePotencyFactors,
                data.MembershipProbabilities,
                data.ActiveSubstances,
                project.AssessmentSettings.ExposureType,
                project.OutputDetailSettings.LowerPercentage,
                project.OutputDetailSettings.UpperPercentage,
                project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                project.OutputDetailSettings.PercentageForUpperTail,
                project.SubsetSettings.IsPerPerson
            );
            subHeader.SaveSummarySection(section);
        }


        private void summarizeCoExposures(
            ProjectDto project,
            DietaryExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var section = new ExposureByCompoundSection() {
                ProgressState = _progressState,
                SectionLabel = getSectionLabel(DietaryExposuresSections.CoExposuresSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Co-exposures",
                order
            );
            section.Summarize(
                subHeader,
                result.DietaryIndividualDayIntakes,
                data.CorrectedRelativePotencyFactors,
                data.MembershipProbabilities,
                data.ActiveSubstances,
                project.AssessmentSettings.ExposureType,
                project.OutputDetailSettings.PercentageForUpperTail,
                project.SubsetSettings.IsPerPerson
            );
            subHeader.SaveSummarySection(section);
        }

        private void summarizeDiagnostics(
                ProjectDto project,
                DietaryExposuresActionResult result,
                ActionData data,
                SectionHeader header,
                int order
            ) {
            var section = new DiagnosticsSection();
            var subHeader = header.AddSubSectionHeaderFor(section, "Variability diagnostics", order);
            var substances = data.ActiveSubstances;
            if (result.DietaryObservedIndividualMeans != null) {
                var dietaryObservedIndividualMeans = result.DietaryObservedIndividualMeans.Shuffle(new McraRandomGenerator());
                var intakes = dietaryObservedIndividualMeans.Select(c => c.DietaryIntakePerMassUnit).ToList();
                var weights = dietaryObservedIndividualMeans.Select(c => c.IndividualSamplingWeight).ToList();
                section.Summarize(
                    intakes,
                    weights,
                    project.OutputDetailSettings.SelectedPercentiles,
                    intakes.Count,
                    project.UncertaintyAnalysisSettings.NumberOfResampleCycles
                );
            } else {
                var rpfs = data.CorrectedRelativePotencyFactors ?? substances.ToDictionary(r => r, r => 1D);
                var intakes = result.DietaryIndividualDayIntakes
                    .Select(c => c.TotalExposurePerMassUnit(rpfs, data.MembershipProbabilities, project.SubsetSettings.IsPerPerson))
                    .ToList();
                var weights = result.DietaryIndividualDayIntakes.Select(c => c.IndividualSamplingWeight).ToList();
                section.Summarize(
                    intakes,
                    weights,
                    project.OutputDetailSettings.SelectedPercentiles,
                    project.UncertaintyAnalysisSettings.NumberOfIterationsPerResampledSet,
                    project.UncertaintyAnalysisSettings.NumberOfResampleCycles
                );
            }
            subHeader.SaveSummarySection(section);
        }

        /// <summary>
        /// Daily intakes distribution
        /// </summary>
        /// <param name="project"></param>
        /// <param name="result"></param>
        /// <param name="data"></param>
        /// <param name="header"></param>
        /// <param name="order"></param>
        private void summarizeDailyIntakesDistribution(
            ProjectDto project,
            DietaryExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var substances = data.ActiveSubstances;
            if (result.DietaryIndividualDayIntakes != null
                && ((data.CorrectedRelativePotencyFactors?.Any() ?? false) || (substances?.Count == 1))
            ) {
                var section = new DietaryIntakeDistributionSection() { ProgressState = _progressState };
                var subHeader = header.AddSubSectionHeaderFor(section, "Distribution (daily intakes)", order);
                var rpfs = data.CorrectedRelativePotencyFactors ?? substances.ToDictionary(r => r, r => 1D);
                var intakes = result.DietaryIndividualDayIntakes
                    .Select(c => c.TotalExposurePerMassUnit(rpfs, data.MembershipProbabilities, project.SubsetSettings.IsPerPerson))
                    .ToList();
                var exposureLevels = ExposureLevelsCalculator.GetExposureLevels(
                    intakes,
                    project.OutputDetailSettings.ExposureMethod,
                    project.OutputDetailSettings.ExposureLevels);
                section.Summarize(
                    subHeader,
                    result.DietaryIndividualDayIntakes,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    data.ActiveSubstances.Count == 1 ? data.ActiveSubstances.First() : data.ReferenceSubstance,
                    exposureLevels,
                    project.OutputDetailSettings.SelectedPercentiles,
                    project.OutputDetailSettings.PercentageForUpperTail,
                    project.SubsetSettings.IsPerPerson,
                    project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                    project.UncertaintyAnalysisSettings.UncertaintyUpperBound
                );
                subHeader.SaveSummarySection(section);

                if (project.AssessmentSettings.ExposureType == ExposureType.Chronic) {
                    var frequencyAmountSummarySection = new FrequencyAmountSummarySection() { ProgressState = _progressState };
                    var sub2Header = subHeader.AddSubSectionHeaderFor(frequencyAmountSummarySection, "Frequency, amounts", 6);
                    frequencyAmountSummarySection.Summarize(
                        result.DietaryIndividualDayIntakes,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        project.SubsetSettings.IsPerPerson
                    );
                    sub2Header.SaveSummarySection(frequencyAmountSummarySection);
                }
            }
        }

        private void summarizeConditionalUsualIntakeDistribution(
            ProjectDto project,
            DietaryExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            if (result.DietaryConditionalUsualIntakeResults != null) {
                var section = new ChronicConditionalSection() { ProgressState = _progressState };
                var subHeader = header.AddSubSectionHeaderFor(section, "Distribution (conditional)", order);
                section.Summarize(
                    subHeader,
                    data.Cofactor,
                    data.Covariable,
                    data.ActiveSubstances.Count == 1 ? data.ActiveSubstances.First() : data.ReferenceSubstance,
                    result.DietaryConditionalUsualIntakeResults,
                    project.OutputDetailSettings.SelectedPercentiles,
                    project.OutputDetailSettings.ExposureMethod,
                    project.OutputDetailSettings.ExposureLevels);
                subHeader.SaveSummarySection(section);
            }
        }

        /// <summary>
        /// Summarize estimates/diagnostics
        /// </summary>
        /// <param name="project"></param>
        /// <param name="result"></param>
        /// <param name="data"></param>
        /// <param name="header"></param>
        /// <param name="order"></param>
        private void summarizeEstimatesAndDiagnostics(
            ProjectDto project,
            DietaryExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            if (result.IntakeModel != null && project.IntakeModelSettings.FirstModelThenAdd) {
                var section = new ModelThenAddIntakeModelsSection() { ProgressState = _progressState };
                var subHeader = header.AddSubSectionHeaderFor(section, "Estimates, diagnostics (MTA)", order);
                section.SummarizeModels(subHeader, result.IntakeModel as CompositeIntakeModel, data);
                subHeader.SaveSummarySection(section);
            } else if (result.IntakeModel != null && !project.IntakeModelSettings.FirstModelThenAdd) {
                if (project.IntakeModelSettings.IntakeModelType == IntakeModelType.BBN || project.IntakeModelSettings.IntakeModelType == IntakeModelType.LNN0) {
                    var intakeModel = result.IntakeModel;
                    var section = new ChronicIntakeEstimatesSection() { ProgressState = _progressState };
                    var subHeader = header.AddSubSectionHeaderFor(section, "Estimates, diagnostics", order);
                    if (intakeModel is BBNModel) {
                        section.SummarizeModel(subHeader, data, intakeModel as BBNModel);
                    }
                    if (intakeModel is LNN0Model) {
                        section.SummarizeModel(subHeader, data, intakeModel as LNN0Model);
                    }
                    subHeader.SaveSummarySection(section);
                } else if (project.IntakeModelSettings.IntakeModelType == IntakeModelType.ISUF) {
                    var section = new ISUFModelResultsSection() { ProgressState = _progressState };
                    var subHeader = header.AddSubSectionHeaderFor(section, "Estimates, diagnostics", order);
                    section.SummarizeModel(result.IntakeModel as ISUFModel);
                    subHeader.SaveSummarySection(section);
                } else if (project.IntakeModelSettings.IntakeModelType == IntakeModelType.LNN) {
                    var section = new ChronicIntakeInitialEstimatesSection() { ProgressState = _progressState };
                    var subHeader = header.AddSubSectionHeaderFor(section, "Estimates, diagnostics", order);
                    section.SummarizeModels(subHeader, result.IntakeModel as LNNModel);
                    subHeader.SaveSummarySection(section);
                }
            }
        }

        /// <summary>
        /// Summarize BBN, LNN0 or MTA
        /// </summary>
        /// <param name="project"></param>
        /// <param name="result"></param>
        /// <param name="data"></param>
        /// <param name="header"></param>
        /// <param name="order"></param>
        private void summarizeModelAssistedUsualIntakesDistribution(
            ProjectDto project,
            DietaryExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            if ((project.AssessmentSettings.ExposureType == ExposureType.Chronic
                && (project.IntakeModelSettings.IntakeModelType == IntakeModelType.BBN || project.IntakeModelSettings.IntakeModelType == IntakeModelType.LNN0
                || data.DesiredIntakeModelType == IntakeModelType.LNN0))
                || (project.AssessmentSettings.ExposureType == ExposureType.Chronic && project.IntakeModelSettings.FirstModelThenAdd)
            ) {
                var section = new ChronicModelAssistedSection() { ProgressState = _progressState };
                var subHeader = header.AddSubSectionHeaderFor(section, "Distribution (model assisted)", order);
                section.Summarize(
                    subHeader,
                    result.DietaryModelAssistedIntakes,
                    data.ActiveSubstances.Count == 1 ? data.ActiveSubstances.First() : data.ReferenceSubstance,
                    project.OutputDetailSettings.ExposureMethod,
                    project.OutputDetailSettings.SelectedPercentiles,
                    project.OutputDetailSettings.ExposureLevels,
                    project.OutputDetailSettings.PercentageForUpperTail
                );
                subHeader.SaveSummarySection(section);
            }
        }

        /// <summary>
        /// Summarize ISUF
        /// </summary>
        /// <param name="project"></param>
        /// <param name="result"></param>
        /// <param name="header"></param>
        /// <param name="order"></param>
        private void summarizeIsufUsualIntakesDistribution(
            ProjectDto project,
            DietaryExposuresActionResult result,
            Compound referenceSubstance,
            SectionHeader header,
            int order
        ) {
            if (project.IntakeModelSettings.IntakeModelType == IntakeModelType.ISUF
                && result.IntakeModel != null
                && !project.IntakeModelSettings.FirstModelThenAdd
            ) {
                var section = new ChronicModelBasedSection() { ProgressState = _progressState };
                var subHeader = header.AddSubSectionHeaderFor(section, "Distribution (ISUF)", order);
                var model = result.IntakeModel as ISUFModel;
                var exposureLevels = ExposureLevelsCalculator.GetExposureLevels(
                    model.UsualIntakeResult.UsualIntakes.Select(c => c.UsualIntake).ToList(),
                    project.OutputDetailSettings.ExposureMethod,
                    project.OutputDetailSettings.ExposureLevels);
                section.Summarize(
                    subHeader,
                    model,
                    referenceSubstance,
                    project.OutputDetailSettings.SelectedPercentiles,
                    exposureLevels
                //result.ExternalReferenceDose
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private void summarizeOimDistribution(
            ProjectDto project,
            DietaryExposuresActionResult result,
            Compound referenceSubstance,
            SectionHeader header,
            int order
        ) {
            if (result.DietaryObservedIndividualMeans != null) {
                var section = new ChronicDietarySection() { ProgressState = _progressState };
                var subHeader = header.AddSubSectionHeaderFor(section, "Distribution (OIM)", order);
                section.Summarize(
                    subHeader,
                    result.DietaryObservedIndividualMeans,
                    project.OutputDetailSettings.ExposureMethod,
                    referenceSubstance,
                    project.OutputDetailSettings.ExposureLevels,
                    project.OutputDetailSettings.SelectedPercentiles,
                    project.OutputDetailSettings.PercentageForUpperTail,
                    project.SubsetSettings.IsPerPerson
                );
                subHeader.SaveSummarySection(section);
            }
        }

        /// <summary>
        /// Model-then-add OIMs by food
        /// </summary>
        /// <param name="project"></param>
        /// <param name="result"></param>
        /// <param name="data"></param>
        /// <param name="header"></param>
        /// <param name="subOrder"></param>
        private void summarizeMtaDistributionsByModelledFood(
            ProjectDto project,
            DietaryExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int subOrder
        ) {
            if (project.IntakeModelSettings.FirstModelThenAdd && (result.DietaryIndividualDayIntakes?.Any() ?? false)) {
                var section = new UsualIntakeDistributionPerFoodAsMeasuredSection() {
                    ProgressState = _progressState,
                    SectionLabel = getSectionLabel(DietaryExposuresSections.ObservedIndividualMeansByModelledFoodSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Per modelled food (MTA)",
                    subOrder++
                );
                section.Summarize(
                    result.DietaryIndividualDayIntakes,
                    data.ActiveSubstances,
                    data.ModelledFoods,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    project.SubsetSettings.IsPerPerson
                );
                // Store OIMs by modelled food
                DataSectionHelper.CreateXmlDataSection(
                    "ObservedIndividualMeansByFoodAsMeased",
                    section,
                    section,
                    subHeader.TitlePath,
                    subHeader.SectionId.ToString()
                );
                subHeader.SaveSummarySection(section);
            }
        }

        /// <summary>
        /// Model-then-add models by category
        /// </summary>
        /// <param name="result"></param>
        /// <param name="header"></param>
        /// <param name="order"></param>
        private void summarizeMtaModelsPerCategory(
            DietaryExposuresActionResult result,
            SectionHeader header,
            int order
        ) {
            if (result.IntakeModel is CompositeIntakeModel) {
                var section = new UsualIntakeDistributionPerCategorySection() { ProgressState = _progressState };
                var subHeader = header.AddSubSectionHeaderFor(section, "Per category (MTA)", order);
                section.Summarize(result.IntakeModel as CompositeIntakeModel);
                subHeader.SaveSummarySection(section);
            }
        }

        private void summarizeModelBasedUsualIntakesDistributionMta(
            ProjectDto project,
            DietaryExposuresActionResult result,
            SectionHeader header,
            Compound referenceSubstance,
            int order
        ) {
            if (result.DietaryModelBasedIntakeResults != null) {
                var mtaString = project.IntakeModelSettings.FirstModelThenAdd ? "MTA " : string.Empty;
                var modelBasedIntakes = result.DietaryModelBasedIntakeResults.SelectMany(c => c.ModelBasedIntakes).ToList();
                var section = new ChronicModelBasedSection() { ProgressState = _progressState };
                var subHeader = header.AddSubSectionHeaderFor(section, $"Distribution {mtaString}(model based)", order);
                var selectedPercentiles = project.OutputDetailSettings.SelectedPercentiles;
                var exposureLevels = ExposureLevelsCalculator.GetExposureLevels(
                    modelBasedIntakes,
                    project.OutputDetailSettings.ExposureMethod,
                    project.OutputDetailSettings.ExposureLevels);
                section.Summarize(
                    subHeader,
                    modelBasedIntakes,
                    referenceSubstance,
                    selectedPercentiles,
                    exposureLevels
                );
                subHeader.SaveSummarySection(section);
            }
        }

        /// <summary>
        /// Reduction factors for scenario analysis
        /// </summary>
        /// <param name="project"></param>
        /// <param name="data"></param>
        /// <param name="header"></param>
        /// <param name="order"></param>
        private void summarizePotentialReductions(
            ProjectDto project,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var subHeader = header.AddEmptySubSectionHeader("TDS reductions", order);
            int subOrder = 0;
            if (data.TdsReductionFactors != null) {
                var section = new TdsReductionFactorsSection();
                var sub2Header = subHeader.AddSubSectionHeaderFor(section, "Reduction factors scenario analysis", subOrder++);
                section.Summarize(data.TdsReductionFactors);
                sub2Header.SaveSummarySection(section);
            }

            if (data.ConcentrationDistributions?.Any() ?? false) {
                var section = new TdsPotentialReductionFactorsSection();
                var sub2Header = subHeader.AddSubSectionHeaderFor(section, "Potential reductions", subOrder++);
                section.Summarize(
                    data.ConcentrationDistributions,
                    project.SelectedScenarioAnalysisFoods.ToList(),
                    data.ConcentrationUnit
                );
                sub2Header.SaveSummarySection(section);
            }
        }

        /// <summary>
        /// Drilldown
        /// </summary>
        /// <param name="project"></param>
        /// <param name="result"></param>
        /// <param name="data"></param>
        /// <param name="header"></param>
        /// <param name="order"></param>
        private void summarizeIndividualDrilldown(
            ProjectDto project,
            DietaryExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            if (result.IndividualModelAssistedIntakes != null) {
                // BBN, LNN0
                var section = new DietaryChronicDrilldownSection() {
                    ProgressState = _progressState,
                    SectionLabel = getSectionLabel(DietaryExposuresSections.DrilldownSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Drilldown individuals",
                    order
                );
                section.Summarize(
                    result.IndividualModelAssistedIntakes,
                    result.DietaryObservedIndividualMeans,
                    result.DietaryIndividualDayIntakes,
                    data.Cofactor,
                    data.Covariable,
                    data.ActiveSubstances,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    data.ActiveSubstances.Count == 1 ? data.ActiveSubstances.First() : data.ReferenceSubstance,
                    project.ConcentrationModelSettings.IsProcessing,
                    project.AssessmentSettings.Cumulative,
                    project.OutputDetailSettings.PercentageForDrilldown,
                    project.SubsetSettings.IsPerPerson
                );
                subHeader.SaveSummarySection(section);
            } else if (result.DietaryObservedIndividualMeans != null) {
                // OIM, LNN, ISUF
                var section = new DietaryChronicDrilldownSection() {
                    ProgressState = _progressState,
                    SectionLabel = getSectionLabel(DietaryExposuresSections.DrilldownSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Drilldown individuals",
                    order
                );
                section.Summarize(
                    result.DietaryObservedIndividualMeans,
                    result.DietaryIndividualDayIntakes,
                    data.Cofactor,
                    data.Covariable,
                    data.ActiveSubstances,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    data.ActiveSubstances.Count == 1 ? data.ActiveSubstances.First() : data.ReferenceSubstance,
                    project.ConcentrationModelSettings.IsProcessing,
                    project.AssessmentSettings.Cumulative,
                    project.OutputDetailSettings.PercentageForDrilldown,
                    project.SubsetSettings.IsPerPerson
                );
                subHeader.SaveSummarySection(section);
            } else if (result.DietaryIndividualDayIntakes != null) {
                // Acute
                var section = new DietaryAcuteDrillDownSection() {
                    ProgressState = _progressState,
                    SectionLabel = getSectionLabel(DietaryExposuresSections.DrilldownSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Drilldown individual-days",
                    order
                );
                section.Summarize(
                    result.DietaryIndividualDayIntakes,
                    data.ActiveSubstances,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    data.ActiveSubstances.Count == 1 ? data.ActiveSubstances.First() : data.ReferenceSubstance,
                    data.UnitVariabilityDictionary,
                    project.ConcentrationModelSettings.IsProcessing,
                    project.UnitVariabilitySettings.UseUnitVariability,
                    project.AssessmentSettings.Cumulative,
                    project.OutputDetailSettings.PercentageForDrilldown,
                    project.SubsetSettings.IsPerPerson
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private void summarizedExposuresByProcessedFoodAndSubstanceUncertain(
            ProjectDto project,
            DietaryExposuresActionResult result,
            ActionData data,
            SectionHeader header
        ) {

            if (result.DietaryIndividualDayIntakes?.Any(r => r.DetailedIntakesPerFood?.Any() ?? false) ?? false) {
                var subHeader = header.GetSubSectionHeader<ExposureByFoodSubstanceProcessingTypeSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as ExposureByFoodSubstanceProcessingTypeSection;

                    // Total section
                    var subSubHeader = subHeader.GetSubSectionHeader<TotalDistributionFoodAsMeasuredSubstanceProcessingTypeSection>();
                    if (subSubHeader != null) {
                        var totalSection = subSubHeader.GetSummarySection() as TotalDistributionFoodAsMeasuredSubstanceProcessingTypeSection;
                        totalSection.SummarizeUncertainty(
                            result.DietaryIndividualDayIntakes,
                            data.CorrectedRelativePotencyFactors ?? data.ActiveSubstances.ToDictionary(r => r, r => 1D),
                            data.MembershipProbabilities ?? data.ActiveSubstances.ToDictionary(r => r, r => 1D),
                            data.ActiveSubstances,
                            project.AssessmentSettings.ExposureType,
                            project.SubsetSettings.IsPerPerson
                        );
                        subSubHeader.SaveSummarySection(totalSection);
                    }

                    // Upper section
                    subSubHeader = subHeader.GetSubSectionHeader<UpperDistributionModelledFoodSubstanceProcessingTypeSection>();
                    if (subSubHeader != null) {
                        var upperSection = subSubHeader.GetSummarySection() as UpperDistributionModelledFoodSubstanceProcessingTypeSection;
                        upperSection.SummarizeUncertainty(
                            result.DietaryIndividualDayIntakes,
                            data.CorrectedRelativePotencyFactors ?? data.ActiveSubstances.ToDictionary(r => r, r => 1D),
                            data.MembershipProbabilities ?? data.ActiveSubstances.ToDictionary(r => r, r => 1D),
                            data.ActiveSubstances,
                            project.AssessmentSettings.ExposureType,
                            project.OutputDetailSettings.PercentageForUpperTail,
                            project.SubsetSettings.IsPerPerson
                        );
                        subSubHeader.SaveSummarySection(upperSection);
                    }
                }
            }
        }

        private void summarizeExposuresByFoodAndSubstanceUncertain(
            ProjectDto project,
            DietaryExposuresActionResult result,
            ActionData data,
            SectionHeader header
        ) {
            // Total
            if (result.DietaryIndividualDayIntakes != null) {
                var subHeader = header.GetSubSectionHeader<TotalDistributionFoodCompoundSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as TotalDistributionFoodCompoundSection;
                    section.SummarizeUncertainty(
                        result.DietaryIndividualDayIntakes,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        data.ActiveSubstances,
                        project.AssessmentSettings.ExposureType,
                        project.SubsetSettings.IsPerPerson
                    );
                    subHeader.SaveSummarySection(section);
                }
            }

            // Upper
            if (result.DietaryIndividualDayIntakes != null) {
                var subHeader = header.GetSubSectionHeader<UpperDistributionFoodCompoundSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as UpperDistributionFoodCompoundSection;
                    section.SummarizeUncertainty(
                        result.DietaryIndividualDayIntakes,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        data.ActiveSubstances,
                        project.AssessmentSettings.ExposureType,
                        project.OutputDetailSettings.PercentageForUpperTail,
                        project.SubsetSettings.IsPerPerson
                    );
                    subHeader.SaveSummarySection(section);
                }
            }
        }

        private void summarizeExposuresBySubstanceUncertain(
            ProjectDto project,
            DietaryExposuresActionResult result,
            ActionData data,
            SectionHeader header
        ) {

            // Total
            if (result.DietaryIndividualDayIntakes != null) {
                var subHeader = header.GetSubSectionHeader<TotalDistributionCompoundSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as TotalDistributionCompoundSection;
                    section.SummarizeUncertainty(
                        result.DietaryIndividualDayIntakes,
                        data.ActiveSubstances,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        project.AssessmentSettings.ExposureType,
                        project.SubsetSettings.IsPerPerson

                    );
                    subHeader.SaveSummarySection(section);
                }
            }

            // Upper
            if (result.DietaryIndividualDayIntakes != null) {
                var subHeader = header.GetSubSectionHeader<UpperDistributionCompoundSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as UpperDistributionCompoundSection;
                    section.SummarizeUncertainty(
                        result.DietaryIndividualDayIntakes,
                        data.ActiveSubstances,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        project.AssessmentSettings.ExposureType,
                        project.OutputDetailSettings.PercentageForUpperTail,
                        project.SubsetSettings.IsPerPerson
                    );
                    subHeader.SaveSummarySection(section);
                }
            }
        }
    }
}
