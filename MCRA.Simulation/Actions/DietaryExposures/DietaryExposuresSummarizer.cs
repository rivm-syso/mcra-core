using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.ExposureLevelsCalculation;
using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.Calculators.IntakeModelling.ModelThenAddIntakeModelCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.OutputGeneration.ActionSummaries.DietaryExposures;
using MCRA.Simulation.OutputGeneration.Generic.Diagnostics;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

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

    public sealed class DietaryExposuresSummarizer : ActionModuleResultsSummarizer<DietaryExposuresModuleConfig, DietaryExposuresActionResult> {

        public override ActionType ActionType => ActionType.DietaryExposures;

        private readonly CompositeProgressState _progressState;

        public DietaryExposuresSummarizer(
            DietaryExposuresModuleConfig config,
            CompositeProgressState progressState = null
        ) : base(config) {
            _progressState = progressState;
        }

        public override void Summarize(
            ActionModuleConfig settingsConfig,
            DietaryExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var outputSettings = new ModuleOutputSectionsManager<DietaryExposuresSections>(settingsConfig, ActionType);
            var substances = data.ActiveSubstances;

            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var outputSummary = new DietaryExposuresSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(outputSummary, ActionType.GetDisplayName(), order);
            subHeader.Units = collectUnits(data);
            subHeader.SaveSummarySection(outputSummary);

            int subOrder = 0;

            var referenceSubstance = data.ActiveSubstances.Count == 1 ? data.ActiveSubstances.First() : data.ReferenceSubstance;
            // Summarize model assisted usual intakes distribution (BBN, LNN0 or MTA)
            if (result.DietaryModelAssistedIntakes != null) {
                summarizeModelAssistedUsualIntakesDistribution(result, data, subHeader, subOrder++);
            }

            // Summarize ISUF usual intakes distribution
            if (_configuration.IntakeModelType == IntakeModelType.ISUF) {
                summarizeIsufUsualIntakesDistribution(result, referenceSubstance, subHeader, subOrder++);
            }

            var subHeaderDetails = subHeader.AddEmptySubSectionHeader("Details", order, getSectionLabel(DietaryExposuresSections.DetailsSection));
            subHeaderDetails.SaveSummarySection(outputSummary);

            // Summarize OIM
            if (result.DietaryObservedIndividualMeans != null) {
                if (_configuration.IntakeModelType != IntakeModelType.OIM || _configuration.IntakeFirstModelThenAdd) {
                    summarizeOimDistribution(result, referenceSubstance, subHeaderDetails, subOrder++);
                } else {
                    summarizeOimDistribution(result, referenceSubstance, subHeader, subOrder++);
                }
            }

            // Model-then-add OIMs by food
            if (_configuration.IntakeFirstModelThenAdd && (result.DietaryIndividualDayIntakes?.Count > 0)
                && outputSettings.ShouldSummarize(DietaryExposuresSections.ExposuresByFoodSection)
            ) {
                summarizeMtaDistributionsByModelledFood(result, data, subHeaderDetails, subOrder++);
            }

            // Model-then-add models by category
            if (result.IntakeModel is CompositeIntakeModel) {
                summarizeMtaModelsPerCategory(result, subHeaderDetails, subOrder++);
            }

            // Summarize estimates/diagnostics
            if (result.IntakeModel != null) {
                summarizeEstimatesAndDiagnostics(result, data, subHeaderDetails, subOrder++);
            }

            //Summarize marginals BBN, LNN0 and LNN
            if (result.DietaryModelBasedIntakeResults != null) {
                if (_configuration.IntakeModelType == IntakeModelType.LNN && result.DietaryModelAssistedIntakes == null) {
                    summarizeModelBasedUsualIntakesDistributionMta(result, subHeader, referenceSubstance, subOrder++);
                } else {
                    summarizeModelBasedUsualIntakesDistributionMta(result, subHeaderDetails, referenceSubstance, subOrder++);
                }
            }

            // Summarize conditional distribution
            if (result.DietaryConditionalUsualIntakeResults != null) {
                summarizeConditionalUsualIntakeDistribution(result, data, subHeaderDetails, subOrder++);
            }

            // Daily intakes distribution
            if (result.DietaryIndividualDayIntakes != null
                && ((data.CorrectedRelativePotencyFactors?.Count > 0) || (substances?.Count == 1))
                && outputSettings.ShouldSummarize(DietaryExposuresSections.DailyIntakeDistributionSection)
            ) {
                if (result.DietaryObservedIndividualMeans != null) {
                    summarizeDailyIntakesDistribution(result, data, subHeaderDetails, subOrder++);
                } else {
                    summarizeDailyIntakesDistribution(result, data, subHeader, subOrder++);
                }
            }

            // Diagnostics
            if (_configuration.VariabilityDiagnosticsAnalysis && result.DietaryIndividualDayIntakes != null
                && ((data.CorrectedRelativePotencyFactors?.Count > 0) || (substances?.Count == 1))
                && outputSettings.ShouldSummarize(DietaryExposuresSections.DiagnosticsSection)
            ) {
                summarizeDiagnostics(result, data, subHeader, subOrder++);
            }

            // Exposures by food summary contributions and pie
            if (result.DietaryIndividualDayIntakes != null
                && ((data.CorrectedRelativePotencyFactors?.Count > 0) || (substances?.Count == 1))
                && outputSettings.ShouldSummarize(DietaryExposuresSections.ExposuresByFoodSection)
            ) {
                summarizeDietaryExposuresByFood(result, data, subHeaderDetails, subOrder++);
            }

            // Exposures by substance summary contributions, pie and boxplot
            if (result.DietaryIndividualDayIntakes != null
                && substances.Count > 1
                && outputSettings.ShouldSummarize(DietaryExposuresSections.ExposuresBySubstanceSection)
            ) {
                if (data.CorrectedRelativePotencyFactors != null) {
                    summarizeExposuresBySubstance(result, data, subHeaderDetails, subOrder++);
                } else {
                    summarizeExposuresBySubstance(result, data, subHeader, subOrder++);
                }
            }

            // Co-exposures
            if (result.DietaryIndividualDayIntakes != null
                && substances.Count > 1
                && outputSettings.ShouldSummarize(DietaryExposuresSections.CoExposuresSection)
            ) {
                if (data.CorrectedRelativePotencyFactors != null) {
                    summarizeCoExposures(result, data, subHeaderDetails, subOrder++);
                } else {
                    summarizeCoExposures(result, data, subHeader, subOrder++);
                }
            }

            // MCR co-exposures
            if (_configuration.McrAnalysis
                && result.DietaryIndividualDayIntakes != null
                && substances.Count > 1
                && result.ExposureMatrix != null
                && outputSettings.ShouldSummarize(DietaryExposuresSections.McrCoExposureSection)
            ) {
                summarizeMaximumCumulativeRatio(result, data, subHeaderDetails, subOrder++);
            }

            // Exposure distributions by substance
            if (result.DietaryIndividualDayIntakes != null
                && substances.Count > 1
                && outputSettings.ShouldSummarize(DietaryExposuresSections.ExposureDistributionsBySubstanceSection)
            ) {
                if (data.CorrectedRelativePotencyFactors != null) {
                    summarizeExposureDistributionsBySubstance(result, data, subHeaderDetails, subOrder++);
                } else {
                    summarizeExposureDistributionsBySubstance(result, data, subHeader, subOrder++);
                }
            }

            // Exposures by food and substance
            if (result.DietaryIndividualDayIntakes != null
                && substances.Count > 1
                && outputSettings.ShouldSummarize(DietaryExposuresSections.ExposuresByFoodAndSubstanceSection)
            ) {
                summarizeExposuresByFoodAndSubstance(result, data, subHeaderDetails, subOrder++);
            }

            // Exposures by processed food and substance
            if (result.DietaryIndividualDayIntakes != null
                && ((data.CorrectedRelativePotencyFactors?.Count > 0) || (substances?.Count == 1))
                && _configuration.IsProcessing
                && _configuration.DietaryExposuresDetailsLevel == DietaryExposuresDetailsLevel.Full
                && (result.DietaryIndividualDayIntakes?.Any(r => r.DetailedIntakesPerFood?.Any() ?? false) ?? false)
                && outputSettings.ShouldSummarize(DietaryExposuresSections.ExposuresByProcessedFoodAndSubstanceSection)
            ) {
                summarizeExposuresByProcessedFoodAndSubstance(result, data, subHeaderDetails, subOrder++);
            }

            // TDS Reduction to limit scenario
            if (result?.TdsReductionFactors?.Count > 0) {
                summarizePotentialReductions(data, subHeaderDetails, subOrder++);
            }

            // Drilldown
            if (_configuration.IsDetailedOutput
                && !_configuration.SkipPrivacySensitiveOutputs
                && ((data.CorrectedRelativePotencyFactors?.Count > 0) || (substances?.Count == 1))
                && outputSettings.ShouldSummarize(DietaryExposuresSections.DrilldownSection)
            ) {
                summarizeIndividualDrilldown(result, data, subHeaderDetails, subOrder++);
            }
        }

        public void SummarizeUncertain(
            ActionModuleConfig outputConfig,
            DietaryExposuresActionResult result,
            ActionData data,
            SectionHeader header
        ) {
            var outputSettings = new ModuleOutputSectionsManager<DietaryExposuresSections>(outputConfig, ActionType);
            var subHeader = header.GetSubSectionHeader<DietaryExposuresSummarySection>();
            if (subHeader == null) {
                return;
            }
            var outputSummary = (ActionSummarySectionBase)subHeader.GetSummarySection();
            var substances = data.ActiveSubstances;

            // Daily intake distribution
            if (result.DietaryIndividualDayIntakes != null
                && ((data.CorrectedRelativePotencyFactors?.Count > 0) || (substances?.Count == 1))
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
                        _configuration.UncertaintyLowerBound,
                        _configuration.UncertaintyUpperBound,
                        _configuration.IsPerPerson
                    );
                    subHeader.SaveSummarySection(section);
                }
            }

            // Diagnostics
            if (_configuration.VariabilityDiagnosticsAnalysis && result.DietaryIndividualDayIntakes != null
                && ((data.CorrectedRelativePotencyFactors?.Count > 0) || (substances?.Count == 1))
                && outputSettings.ShouldSummarize(DietaryExposuresSections.DiagnosticsSection)
            ) {
                subHeader = header.GetSubSectionHeader<DiagnosticsSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as DiagnosticsSection;
                    if (result.DietaryObservedIndividualMeans != null) {
                        var dietaryObservedIndividualMeans = result.DietaryObservedIndividualMeans.Shuffle(new McraRandomGenerator());
                        var intakes = dietaryObservedIndividualMeans.Select(c => c.DietaryIntakePerMassUnit).ToList();
                        var weights = dietaryObservedIndividualMeans.Select(c => c.SimulatedIndividual.SamplingWeight).ToList();
                        section.SummarizeUncertainty(intakes, weights, _configuration.SelectedPercentiles.ToArray());
                    } else {
                        var rpfs = data.CorrectedRelativePotencyFactors ?? substances.ToDictionary(r => r, r => 1D);
                        var intakes = result.DietaryIndividualDayIntakes
                            .Select(c => c.TotalExposurePerMassUnit(rpfs, data.MembershipProbabilities, _configuration.IsPerPerson))
                            .ToList();
                        var weights = result.DietaryIndividualDayIntakes.Select(c => c.SimulatedIndividual.SamplingWeight).ToList();
                        section.SummarizeUncertainty(intakes, weights, _configuration.SelectedPercentiles.ToArray());
                    }
                    subHeader.SaveSummarySection(section);
                }
            }

            if (_configuration.ExposureType == ExposureType.Chronic) {
                // OIM
                if (result.DietaryObservedIndividualMeans != null) {
                    subHeader = header.GetSubSectionHeader<ChronicDietarySection>();
                    if (subHeader != null) {
                        var section = subHeader.GetSummarySection() as ChronicDietarySection;
                        section.ProgressState = _progressState;
                        section.SummarizeUncertainty(
                            subHeader,
                            result.DietaryObservedIndividualMeans,
                            _configuration.UncertaintyLowerBound,
                            _configuration.UncertaintyUpperBound);
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
                            _configuration.UncertaintyLowerBound,
                            _configuration.UncertaintyUpperBound
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
                            _configuration.UncertaintyLowerBound,
                            _configuration.UncertaintyUpperBound
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
                            _configuration.UncertaintyLowerBound,
                            _configuration.UncertaintyUpperBound
                        );
                        subHeader.SaveSummarySection(section);
                    }
                }

                // ISUF
                if (_configuration.IntakeModelType == IntakeModelType.ISUF && !_configuration.IntakeFirstModelThenAdd) {
                    var intakeModel = result.IntakeModel as ISUFModel;
                    subHeader = header.GetSubSectionHeader<ChronicModelBasedSection>();
                    if (subHeader != null) {
                        var section = subHeader.GetSummarySection() as ChronicModelBasedSection;
                        section.ProgressState = _progressState;
                        section.SummarizeUncertainty(
                            subHeader,
                            intakeModel,
                            _configuration.UncertaintyLowerBound,
                            _configuration.UncertaintyUpperBound
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
                            _configuration.UncertaintyLowerBound,
                            _configuration.UncertaintyUpperBound
                        );
                        subHeader.SaveSummarySection(section);
                    }
                }
            }

            // Contributions by food (as-eaten and as-measured)
            if ((data.CorrectedRelativePotencyFactors?.Count > 0) || substances?.Count == 1) {
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
                        _configuration.ExposureType,
                        _configuration.VariabilityUpperTailPercentage,
                        _configuration.IsPerPerson
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
                    _configuration.ExposureType,
                    _configuration.IsPerPerson,
                    _configuration.UncertaintyLowerBound,
                    _configuration.UncertaintyUpperBound
                );
                subHeader.SaveSummarySection(section);
            }

            // Contributions by substance
            if (substances.Count > 1
                && outputSettings.ShouldSummarize(DietaryExposuresSections.ExposuresBySubstanceSection)
            ) {
                summarizeExposuresBySubstanceUncertain(result, data, header);
            }

            // Exposures by food and substance
            if (substances.Count > 1 && (data.CorrectedRelativePotencyFactors?.Count > 0)
                && outputSettings.ShouldSummarize(DietaryExposuresSections.ExposuresByFoodAndSubstanceSection)
            ) {
                summarizeExposuresByFoodAndSubstanceUncertain(result, data, header);
            }

            // Exposures by processed food and substance
            if (result.DietaryIndividualDayIntakes != null
                && _configuration.IsProcessing
                && (substances.Count > 1 && (data.CorrectedRelativePotencyFactors?.Count > 0))
                && outputSettings.ShouldSummarize(DietaryExposuresSections.ExposuresByFoodAndSubstanceSection)
            ) {
                summarizedExposuresByProcessedFoodAndSubstanceUncertain(result, data, header);
            }
        }

        private List<ActionSummaryUnitRecord> collectUnits(ActionData data) {
            var result = new List<ActionSummaryUnitRecord> {
                new("ExposureType", _configuration.ExposureType.GetDisplayName()),
                new("IntakeUnit", data.DietaryExposureUnit.GetShortDisplayName(TargetUnit.DisplayOption.AppendBiologicalMatrix)),
                new("PerPersonIntakeUnit", $"{data.DietaryExposureUnit.SubstanceAmountUnit.GetShortDisplayName()}/day")
            };
            if (_configuration.ExposureType == ExposureType.Chronic) {
                result.Add(new ActionSummaryUnitRecord("IndividualDayUnit", "individuals"));
            } else {
                result.Add(new ActionSummaryUnitRecord("IndividualDayUnit", "individual days"));
            }
            result.Add(new ActionSummaryUnitRecord("BodyWeightUnit", data.BodyWeightUnit.GetDisplayName()));
            result.Add(new ActionSummaryUnitRecord("LowerPercentage", $"p{_configuration.VariabilityLowerPercentage}"));
            result.Add(new ActionSummaryUnitRecord("UpperPercentage", $"p{_configuration.VariabilityUpperPercentage}"));
            result.Add(new ActionSummaryUnitRecord("ConcentrationUnit", data.ConcentrationUnit.GetShortDisplayName()));
            result.Add(new ActionSummaryUnitRecord("ConsumptionUnit", data.ConsumptionUnit.GetShortDisplayName()));
            result.Add(new ActionSummaryUnitRecord("LowerBound", $"p{_configuration.UncertaintyLowerBound}"));
            result.Add(new ActionSummaryUnitRecord("UpperBound", $"p{_configuration.UncertaintyUpperBound}"));
            return result;
        }

        private void summarizeExposuresByProcessedFoodAndSubstance(
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
                    _configuration.ExposureType,
                    _configuration.UncertaintyLowerBound,
                    _configuration.UncertaintyUpperBound,
                    _configuration.IsPerPerson
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
                    _configuration.ExposureType,
                    _configuration.UncertaintyLowerBound,
                    _configuration.UncertaintyUpperBound,
                    _configuration.VariabilityUpperTailPercentage,
                    _configuration.IsPerPerson
                );
                subSubHeader.SaveSummarySection(upperSection);
            }
        }

        private void summarizeExposuresByFoodAndSubstance(
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
                _configuration.ExposureType,
                _configuration.VariabilityLowerPercentage,
                _configuration.VariabilityUpperPercentage,
                _configuration.UncertaintyLowerBound,
                _configuration.UncertaintyUpperBound,
                _configuration.VariabilityUpperTailPercentage,
                _configuration.IsPerPerson
            );
            subHeader.SaveSummarySection(section);
        }

        private void summarizeExposureDistributionsBySubstance(
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
                _configuration.ExposureType,
                _configuration.SelectedPercentiles.ToArray(),
                _configuration.IsPerPerson,
                _configuration.ExposureMethod,
                _configuration.ExposureLevels.ToArray()
            );
            subSubHeader.SaveSummarySection(substancesOverViewSection);
        }

        private void summarizeMaximumCumulativeRatio(
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
                    _configuration.McrExposureApproachType,
                    _configuration.McrPlotRatioCutOff,
                    _configuration.McrPlotPercentiles.ToArray(),
                    _configuration.McrCalculationTotalExposureCutOff,
                    _configuration.McrPlotMinimumPercentage,
                    _configuration.SkipPrivacySensitiveOutputs
                );

                mcrSection.Summarize(
                    result.ExposureMatrix,
                    _configuration.McrPlotPercentiles.ToArray(),
                    _configuration.McrPlotMinimumPercentage
                );
                mcrHeader.SaveSummarySection(mcrSection);
            }
        }

        private void summarizeDietaryExposuresByFood(
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
                _configuration.TotalDietStudy,
                _configuration.UseReadAcrossFoodTranslations,
                _configuration.ExposureType,
                _configuration.VariabilityLowerPercentage,
                _configuration.VariabilityUpperPercentage,
                _configuration.UncertaintyLowerBound,
                _configuration.UncertaintyUpperBound,
                _configuration.VariabilityUpperTailPercentage,
                _configuration.IsPerPerson
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
                _configuration.ExposureType,
                _configuration.VariabilityLowerPercentage,
                _configuration.VariabilityUpperPercentage,
                _configuration.UncertaintyLowerBound,
                _configuration.UncertaintyUpperBound,
                _configuration.VariabilityUpperTailPercentage,
                _configuration.IsPerPerson
            );
            subHeader.SaveSummarySection(section);
        }


        private void summarizeCoExposures(
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
                _configuration.ExposureType,
                _configuration.VariabilityUpperTailPercentage,
                _configuration.IsPerPerson
            );
            subHeader.SaveSummarySection(section);
        }

        private void summarizeDiagnostics(
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
                var weights = dietaryObservedIndividualMeans.Select(c => c.SimulatedIndividual.SamplingWeight).ToList();
                section.Summarize(
                    intakes,
                    weights,
                    _configuration.SelectedPercentiles.ToArray(),
                    intakes.Count,
                    _configuration.UncertaintyAnalysisCycles
                );
            } else {
                var rpfs = data.CorrectedRelativePotencyFactors ?? substances.ToDictionary(r => r, r => 1D);
                var intakes = result.DietaryIndividualDayIntakes
                    .Select(c => c.TotalExposurePerMassUnit(rpfs, data.MembershipProbabilities, _configuration.IsPerPerson))
                    .ToList();
                var weights = result.DietaryIndividualDayIntakes.Select(c => c.SimulatedIndividual.SamplingWeight).ToList();
                section.Summarize(
                    intakes,
                    weights,
                    _configuration.SelectedPercentiles.ToArray(),
                    _configuration.UncertaintyIterationsPerResampledSet,
                    _configuration.UncertaintyAnalysisCycles
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
            DietaryExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var substances = data.ActiveSubstances;
            if (result.DietaryIndividualDayIntakes != null
                && ((data.CorrectedRelativePotencyFactors?.Count > 0) || (substances?.Count == 1))
            ) {
                var section = new DietaryIntakeDistributionSection() { ProgressState = _progressState };
                var subHeader = header.AddSubSectionHeaderFor(section, "Distribution (daily intakes)", order);
                var rpfs = data.CorrectedRelativePotencyFactors ?? substances.ToDictionary(r => r, r => 1D);
                var intakes = result.DietaryIndividualDayIntakes
                    .Select(c => c.TotalExposurePerMassUnit(rpfs, data.MembershipProbabilities, _configuration.IsPerPerson))
                    .ToList();
                var exposureLevels = ExposureLevelsCalculator.GetExposureLevels(
                    intakes,
                    _configuration.ExposureMethod,
                    _configuration.ExposureLevels.ToArray());
                section.Summarize(
                    subHeader,
                    result.DietaryIndividualDayIntakes,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    data.ActiveSubstances.Count == 1 ? data.ActiveSubstances.First() : data.ReferenceSubstance,
                    exposureLevels,
                    _configuration.SelectedPercentiles.ToArray(),
                    _configuration.VariabilityUpperTailPercentage,
                    _configuration.IsPerPerson,
                    _configuration.UncertaintyLowerBound,
                    _configuration.UncertaintyUpperBound
                );
                subHeader.SaveSummarySection(section);

                if (_configuration.ExposureType == ExposureType.Chronic) {
                    var frequencyAmountSummarySection = new FrequencyAmountSummarySection() { ProgressState = _progressState };
                    var sub2Header = subHeader.AddSubSectionHeaderFor(frequencyAmountSummarySection, "Frequency, amounts", 6);
                    frequencyAmountSummarySection.Summarize(
                        result.DietaryIndividualDayIntakes,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        _configuration.IsPerPerson
                    );
                    sub2Header.SaveSummarySection(frequencyAmountSummarySection);
                }
            }
        }

        private void summarizeConditionalUsualIntakeDistribution(
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
                    _configuration.SelectedPercentiles.ToArray(),
                    _configuration.ExposureMethod,
                    _configuration.ExposureLevels.ToArray());
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
            DietaryExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            if (result.IntakeModel != null && _configuration.IntakeFirstModelThenAdd) {
                var section = new ModelThenAddIntakeModelsSection() { ProgressState = _progressState };
                var subHeader = header.AddSubSectionHeaderFor(section, "Estimates, diagnostics (MTA)", order);
                section.SummarizeModels(subHeader, result.IntakeModel as CompositeIntakeModel, data);
                subHeader.SaveSummarySection(section);
            } else if (result.IntakeModel != null && !_configuration.IntakeFirstModelThenAdd) {
                if (_configuration.IntakeModelType == IntakeModelType.BBN || _configuration.IntakeModelType == IntakeModelType.LNN0) {
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
                } else if (_configuration.IntakeModelType == IntakeModelType.ISUF) {
                    var section = new ISUFModelResultsSection() { ProgressState = _progressState };
                    var subHeader = header.AddSubSectionHeaderFor(section, "Estimates, diagnostics", order);
                    section.SummarizeModel(result.IntakeModel as ISUFModel);
                    subHeader.SaveSummarySection(section);
                } else if (_configuration.IntakeModelType == IntakeModelType.LNN) {
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
            DietaryExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            if ((_configuration.ExposureType == ExposureType.Chronic
                && (_configuration.IntakeModelType == IntakeModelType.BBN || _configuration.IntakeModelType == IntakeModelType.LNN0
                || data.DesiredIntakeModelType == IntakeModelType.LNN0))
                || (_configuration.ExposureType == ExposureType.Chronic && _configuration.IntakeFirstModelThenAdd)
            ) {
                var section = new ChronicModelAssistedSection() { ProgressState = _progressState };
                var subHeader = header.AddSubSectionHeaderFor(section, "Distribution (model assisted)", order);
                section.Summarize(
                    subHeader,
                    result.DietaryModelAssistedIntakes,
                    data.ActiveSubstances.Count == 1 ? data.ActiveSubstances.First() : data.ReferenceSubstance,
                    _configuration.ExposureMethod,
                    _configuration.SelectedPercentiles.ToArray(),
                    _configuration.ExposureLevels.ToArray(),
                    _configuration.VariabilityUpperTailPercentage
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
            DietaryExposuresActionResult result,
            Compound referenceSubstance,
            SectionHeader header,
            int order
        ) {
            if (_configuration.IntakeModelType == IntakeModelType.ISUF
                && result.IntakeModel != null
                && !_configuration.IntakeFirstModelThenAdd
            ) {
                var section = new ChronicModelBasedSection() { ProgressState = _progressState };
                var subHeader = header.AddSubSectionHeaderFor(section, "Distribution (ISUF)", order);
                var model = result.IntakeModel as ISUFModel;
                var exposureLevels = ExposureLevelsCalculator.GetExposureLevels(
                    model.UsualIntakeResult.UsualIntakes.Select(c => c.UsualIntake).ToList(),
                    _configuration.ExposureMethod,
                    _configuration.ExposureLevels.ToArray());
                section.Summarize(
                    subHeader,
                    model,
                    referenceSubstance,
                    _configuration.SelectedPercentiles.ToArray(),
                    exposureLevels
                //result.ExternalReferenceDose
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private void summarizeOimDistribution(
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
                    _configuration.ExposureMethod,
                    referenceSubstance,
                    _configuration.ExposureLevels.ToArray(),
                    _configuration.SelectedPercentiles.ToArray(),
                    _configuration.VariabilityUpperTailPercentage,
                    _configuration.IsPerPerson
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
            DietaryExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int subOrder
        ) {
            if (_configuration.IntakeFirstModelThenAdd && (result.DietaryIndividualDayIntakes?.Count > 0)) {
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
                    _configuration.IsPerPerson
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
            DietaryExposuresActionResult result,
            SectionHeader header,
            Compound referenceSubstance,
            int order
        ) {
            if (result.DietaryModelBasedIntakeResults != null) {
                var mtaString = _configuration.IntakeFirstModelThenAdd ? "MTA " : string.Empty;
                var modelBasedIntakes = result.DietaryModelBasedIntakeResults.SelectMany(c => c.ModelBasedIntakes).ToList();
                var section = new ChronicModelBasedSection() { ProgressState = _progressState };
                var subHeader = header.AddSubSectionHeaderFor(section, $"Distribution {mtaString}(model based)", order);
                var selectedPercentiles = _configuration.SelectedPercentiles.ToArray();
                var exposureLevels = ExposureLevelsCalculator.GetExposureLevels(
                    modelBasedIntakes,
                    _configuration.ExposureMethod,
                    _configuration.ExposureLevels.ToArray());
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

            if (data.ConcentrationDistributions?.Count > 0) {
                var section = new TdsPotentialReductionFactorsSection();
                var sub2Header = subHeader.AddSubSectionHeaderFor(section, "Potential reductions", subOrder++);
                section.Summarize(
                    data.ConcentrationDistributions,
                    _configuration.ScenarioAnalysisFoods.ToList(),
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
            DietaryExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            if (result.IndividualModelAssistedIntakes != null) {
                // BBN, LNN0
                var section = new DietaryChronicDrillDownSection() {
                    ProgressState = _progressState,
                    SectionLabel = getSectionLabel(DietaryExposuresSections.DrilldownSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Drilldown individuals",
                    order
                );
                section.Summarize(
                    subHeader,
                    result.IndividualModelAssistedIntakes,
                    result.DietaryObservedIndividualMeans,
                    result.DietaryIndividualDayIntakes,
                    data.Cofactor,
                    data.Covariable,
                    data.ActiveSubstances,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    data.ActiveSubstances.Count == 1 ? data.ActiveSubstances.First() : data.ReferenceSubstance,
                    _configuration.IsProcessing,
                    _configuration.Cumulative,
                    _configuration.VariabilityDrilldownPercentage,
                    _configuration.IsPerPerson
                );
                subHeader.SaveSummarySection(section);
            } else if (result.DietaryObservedIndividualMeans != null) {
                // OIM, LNN, ISUF
                var section = new DietaryChronicDrillDownSection() {
                    ProgressState = _progressState,
                    SectionLabel = getSectionLabel(DietaryExposuresSections.DrilldownSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Drilldown individuals",
                    order
                );
                section.Summarize(
                    subHeader,
                    result.DietaryObservedIndividualMeans,
                    result.DietaryIndividualDayIntakes,
                    data.Cofactor,
                    data.Covariable,
                    data.ActiveSubstances,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    data.ActiveSubstances.Count == 1 ? data.ActiveSubstances.First() : data.ReferenceSubstance,
                    _configuration.IsProcessing,
                    _configuration.Cumulative,
                    _configuration.VariabilityDrilldownPercentage,
                    _configuration.IsPerPerson
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
                    subHeader,
                    result.DietaryIndividualDayIntakes,
                    data.ActiveSubstances,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    data.ActiveSubstances.Count == 1 ? data.ActiveSubstances.First() : data.ReferenceSubstance,
                    data.UnitVariabilityDictionary,
                    _configuration.IsProcessing,
                    _configuration.UseUnitVariability,
                    _configuration.Cumulative,
                    _configuration.VariabilityDrilldownPercentage,
                    _configuration.IsPerPerson
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private void summarizedExposuresByProcessedFoodAndSubstanceUncertain(
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
                            _configuration.ExposureType,
                            _configuration.IsPerPerson
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
                            _configuration.ExposureType,
                            _configuration.VariabilityUpperTailPercentage,
                            _configuration.IsPerPerson
                        );
                        subSubHeader.SaveSummarySection(upperSection);
                    }
                }
            }
        }

        private void summarizeExposuresByFoodAndSubstanceUncertain(
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
                        _configuration.ExposureType,
                        _configuration.IsPerPerson
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
                        _configuration.ExposureType,
                        _configuration.VariabilityUpperTailPercentage,
                        _configuration.IsPerPerson
                    );
                    subHeader.SaveSummarySection(section);
                }
            }
        }

        private void summarizeExposuresBySubstanceUncertain(
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
                        _configuration.ExposureType,
                        _configuration.IsPerPerson

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
                        _configuration.ExposureType,
                        _configuration.VariabilityUpperTailPercentage,
                        _configuration.IsPerPerson
                    );
                    subHeader.SaveSummarySection(section);
                }
            }
        }
    }
}
