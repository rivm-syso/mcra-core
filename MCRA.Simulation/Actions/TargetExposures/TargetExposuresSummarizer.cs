using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.ExposureLevelsCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.KineticConversionCalculation;
using MCRA.Simulation.Calculators.PbpkModelCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.TargetExposures {

    public enum TargetExposuresSections {
        DetailsSection,
        ExposuresBySubstanceSection,
        ExposuresByRouteSection,
        ExposuresByRouteSubstanceSection,
        ExposuresBySourceSection,
        ExposuresBySourceSubstanceSection,
        ExposuresBySourceRouteSection,
        ExposuresBySourceRouteSubstanceSection,
        ExternalExposuresByRouteSection,
        ExternalExposuresBySourceSection,
        ExternalExposuresBySourceRouteSection,
        ExternalExposuresDistributionsBySourceSection,
        KineticConversionFactorsSection,
        McrCoExposureSection
    }
    public class TargetExposuresSummarizer : ActionModuleResultsSummarizer<TargetExposuresModuleConfig, TargetExposuresActionResult> {

        public TargetExposuresSummarizer(TargetExposuresModuleConfig config) : base(config) {
        }

        public override void Summarize(
            ActionModuleConfig settingsConfig,
            TargetExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var outputSettings = new ModuleOutputSectionsManager<TargetExposuresSections>(settingsConfig, ActionType);

            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var outputSummary = new TargetExposuresSummarySection() {
                SectionLabel = getSectionLabel(ActionType)
            };
            var subHeader = header.AddSubSectionHeaderFor(outputSummary, ActionType.GetDisplayName(), order);
            subHeader.Units = collectUnits(data);
            subHeader.SaveSummarySection(outputSummary);
            int subOrder = 0;

            // Toc: Exposures (daily intakes) with subtoc graph total, graph upper, percentiles, percentages
            if (result.AggregateIndividualDayExposures != null
                && (data.ActiveSubstances.Count == 1 || data.CorrectedRelativePotencyFactors != null)
            ) {
                summarizeDailyExposures(
                    result,
                    data,
                    subHeader,
                    subOrder
                );
            }

            // Toc: Distribution with subtoc graph total, graph upper, percentiles, percentages (for single substance)
            if (result.AggregateIndividualExposures != null
                && (data.ActiveSubstances.Count == 1 || data.CorrectedRelativePotencyFactors != null)
            ) {
                summarizeExposureDistribution(
                    result,
                    data,
                    subHeader,
                    subOrder
                );
            }

            // Toc: Exposures by route with subtoc Exposures total contribution and Contributions, total, upper (for single substance)
            if (_configuration.ExposureSources.Count > 1
                && (data.ActiveSubstances.Count == 1 || data.CorrectedRelativePotencyFactors != null)
                && outputSettings.ShouldSummarize(TargetExposuresSections.ExposuresByRouteSection)
            ) {
                summarizeExposuresByRoute(
                    result,
                    data,
                    subHeader,
                    subOrder++
                );
            }

            // Toc: Exposures by substance
            if (data.ActiveSubstances.Count > 1
                && (result.AggregateIndividualExposures != null || result.AggregateIndividualDayExposures != null)
                && outputSettings.ShouldSummarize(TargetExposuresSections.ExposuresBySubstanceSection)
            ) {
                summarizeExposureBySubstance(
                    result,
                    data,
                    subHeader,
                    subOrder++
                 );
            }

            // Toc: Exposures by route and substance
            if (_configuration.ExposureSources.Count > 1
                && data.ActiveSubstances.Count > 1
                && (result.AggregateIndividualExposures != null || result.AggregateIndividualDayExposures != null)
                && outputSettings.ShouldSummarize(TargetExposuresSections.ExposuresByRouteSubstanceSection)
            ) {
                summarizeExposureByRouteSubstance(
                    result,
                    data,
                    subHeader,
                    subOrder++
               );
            }

            // Toc: Exposures by source
            if (result.ExternalExposureCollections.Count > 0
                && (data.ActiveSubstances.Count == 1 || data.CorrectedRelativePotencyFactors?.Count > 0)
                && outputSettings.ShouldSummarize(TargetExposuresSections.ExposuresBySourceSection)
            ) {
                summarizeExposureBySource(
                    result,
                    data,
                    subHeader,
                    subOrder++
                );
            }

            // Toc: Exposures by source and substance
            if (result.ExternalExposureCollections.Count > 0
                && data.ActiveSubstances.Count > 1
                && data.CorrectedRelativePotencyFactors?.Count > 0
                && outputSettings.ShouldSummarize(TargetExposuresSections.ExposuresBySourceSubstanceSection)
            ) {
                summarizeExposureBySourceSubstance(
                    result,
                    data,
                    subHeader,
                    subOrder++
                );
            }

            // Toc: Exposures by source and route
            if (result.ExternalExposureCollections.Count > 0
                && (data.ActiveSubstances.Count == 1 || data.CorrectedRelativePotencyFactors?.Count > 0)
                && outputSettings.ShouldSummarize(TargetExposuresSections.ExposuresBySourceRouteSection)
            ) {
                summarizeExposureBySourceRoute(
                    result,
                    data,
                    subHeader,
                    subOrder++
                );
            }

            // Toc: Exposures by source and route and substance
            if (result.ExternalExposureCollections.Count > 0
                && data.ActiveSubstances.Count > 1
                && data.CorrectedRelativePotencyFactors?.Count > 0
                && outputSettings.ShouldSummarize(TargetExposuresSections.ExposuresBySourceRouteSubstanceSection)
            ) {
                summarizeExposureBySourceRouteSubstance(
                    result,
                    data,
                    subHeader,
                    subOrder++
                );
            }

            // Toc: external exposures
            summarizeExternalExposures(subHeader, result, data, outputSettings, subOrder);

            SectionHeader subHeaderKineticConversion = null;

            // Kinetic conversion factors
            if (_configuration.ExposureSources.Count > 1
                && result.KineticConversionFactors != null
                && outputSettings.ShouldSummarize(TargetExposuresSections.KineticConversionFactorsSection)
            ) {
                subHeaderKineticConversion ??= subHeader.AddEmptySubSectionHeader("Kinetic conversion models", subOrder);
                summarizeKineticConversionFactors(
                    result,
                    subHeaderKineticConversion,
                    subOrder++
               );
            }

            // Kinetic models
            if (result.KineticModelCalculators?.Values
                .Any(r => r is PbkKineticConversionCalculator) ?? false
            ) {
                subHeaderKineticConversion ??= subHeader.AddEmptySubSectionHeader("Kinetic conversion models", subOrder);
                summarizePbkModelSimulationResults(
                    result,
                    data,
                    subHeaderKineticConversion,
                    subOrder++
                );
            }

            // MCR co-exposures
            if (_configuration.McrAnalysis
                && result.ExposureMatrix != null
                && outputSettings.ShouldSummarize(TargetExposuresSections.McrCoExposureSection)
            ) {
                summarizeMCRSection(
                    result,
                    subHeader,
                    subOrder++
                );
            }
            subHeader.SaveSummarySection(outputSummary);
        }

        private void summarizeExternalExposures(
            SectionHeader header,
            TargetExposuresActionResult result,
            ActionData data,
            ModuleOutputSectionsManager<TargetExposuresSections> outputSettings,
            int order
        ) {
            var subHeader = header
                .AddEmptySubSectionHeader(
                    "External exposures",
                    order
                );

            // Section displays exposures in exposure units.
            // For summarizing exposure percentiles and percentages the generic component is used.
            // This component uses intake units as unit in its summaries.
            // Set the value of intake unit to exposure unit for this section only.
            var subHeaderIntakeUnit = new ActionSummaryUnitRecord("IntakeUnit", data.ExternalExposureUnit.GetShortDisplayName());
            var subHeaderUnits = collectUnits(data)
                .Where(r => r.Type != "IntakeUnit")
                .Append(subHeaderIntakeUnit)
                .ToList();
            subHeader.Units = subHeaderUnits;

            var subOrder = 1;
            // Toc: Exposures by source with subtoc Exposures total contribution and Contributions, total, upper (for single substance)
            if (result.ExternalExposureCollections.Count > 0
                && (data.ActiveSubstances.Count == 1 || data.CorrectedRelativePotencyFactors?.Count > 0)
                && outputSettings.ShouldSummarize(TargetExposuresSections.ExternalExposuresBySourceSection)
            ) {
                summarizeExternalExposureBySource(
                    result,
                    data,
                    subHeader,
                    subOrder++
                );
            }

            // Toc: Exposures by route with subtoc Exposures total contribution and Contributions, total, upper (for single substance)
            if (result.ExternalExposureCollections.Count > 0
                && (data.ActiveSubstances.Count == 1 || data.CorrectedRelativePotencyFactors?.Count > 0)
                && outputSettings.ShouldSummarize(TargetExposuresSections.ExternalExposuresByRouteSection)
            ) {
                summarizeExternalExposureByRoute(
                    result,
                    data,
                    subHeader,
                    subOrder
                );
            }

            // Toc: Exposures by source and route with subtoc Exposures total contribution and Contributions, total, upper (for single substance)
            if (result.ExternalExposureCollections.Count > 0
                && (data.ActiveSubstances.Count == 1 || data.CorrectedRelativePotencyFactors?.Count > 0)
                && outputSettings.ShouldSummarize(TargetExposuresSections.ExternalExposuresBySourceRouteSection)
            ) {
                summarizeExternalExposureBySourceRoute(
                    result,
                    data,
                    subHeader,
                    subOrder
                );
            }
            // Toc: Exposures by source sections, Dust, Soil distributions
            if (result.ExternalExposureCollections.Count > 0
                && outputSettings.ShouldSummarize(TargetExposuresSections.ExternalExposuresDistributionsBySourceSection)
            ) {
                summarizeExternalExposureDistributionsBySource(
                    subHeader,
                    result,
                    data,
                    subOrder++
                );
            }
        }

        private void summarizeExternalExposureDistributionsBySource(
            SectionHeader header,
            TargetExposuresActionResult result,
            ActionData data,
            int order
        ) {
            var subHeader = header.AddEmptySubSectionHeader(
                "Exposure distributions by source",
                order,
                TargetExposuresSections.ExternalExposuresDistributionsBySourceSection.ToString()
            );

            foreach (var externalExposureCollection in result.ExternalExposureCollections) {
                var subOrder = 1;
                var sub2Header = subHeader.AddEmptySubSectionHeader(
                    $"{externalExposureCollection.ExposureSource.GetShortDisplayName()}",
                    order++,
                    $"{TargetExposuresSections.ExternalExposuresDistributionsBySourceSection}-{externalExposureCollection.ExposureSource}"
                );

                // Exposures by source
                if (data.ActiveSubstances.Count == 1 || data.CorrectedRelativePotencyFactors != null) {
                    summarizeExternalSourceExposure(
                        sub2Header,
                        externalExposureCollection,
                        data,
                        subOrder++
                    );
                }

                // Exposures by source by route
                if (data.ActiveSubstances.Count == 1 || data.CorrectedRelativePotencyFactors != null) {
                    summarizeExternalSourceExposureByRoute(
                        sub2Header,
                        externalExposureCollection,
                        data,
                        subOrder++
                    );
                }

                // Exposures by source by route and substance
                summarizeExternalSourceExposureByRouteSubstance(
                    sub2Header,
                    externalExposureCollection,
                    data,
                    subOrder++
                );
            }
        }

        private void summarizeMCRSection(
            TargetExposuresActionResult result,
            SectionHeader subHeaderDetails,
            int subOrder
        ) {
            var section = new MaximumCumulativeRatioSection() {
                SectionLabel = getSectionLabel(TargetExposuresSections.McrCoExposureSection)
            };
            var subSubHeader = subHeaderDetails.AddSubSectionHeaderFor(section, "MCR co-exposure", subOrder++);
            section.Summarize(
                result.DriverSubstances,
                result.TargetExposureUnit,
                _configuration.McrExposureApproachType,
                _configuration.McrPlotRatioCutOff,
                [.. _configuration.McrPlotPercentiles],
                _configuration.McrCalculationTotalExposureCutOff,
                _configuration.McrPlotMinimumPercentage,
                _configuration.SkipPrivacySensitiveOutputs
            );

            section.Summarize(
                result.ExposureMatrix,
                [.. _configuration.McrPlotPercentiles],
                _configuration.McrPlotMinimumPercentage
            );
            subSubHeader.SaveSummarySection(section);
        }

        private void summarizeExposureDistribution(
            TargetExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int subOrder
        ) {
            var subHeader = header.AddEmptySubSectionHeader($"Distribution", subOrder++);

            {
                var section = new InternalDistributionTotalSection();
                var sub2Header = subHeader.AddSubSectionHeaderFor(section, $"Graph total", 1);
                section.Summarize(
                    result.AggregateIndividualExposures,
                    data.ActiveSubstances,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    result.KineticConversionFactors,
                    data.ExposureRoutes,
                    data.ExternalExposureUnit,
                    data.TargetExposureUnit
                );
                sub2Header.SaveSummarySection(section);
            }

            {
                var section = new InternalChronicDistributionUpperSection();
                var sub2Header = subHeader.AddSubSectionHeaderFor(section, $"Graph upper tail", 2);
                section.Summarize(
                    result.AggregateIndividualExposures,
                    data.ActiveSubstances,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    result.KineticConversionFactors,
                    data.ExposureRoutes,
                    data.ExternalExposureUnit,
                    data.TargetExposureUnit,
                    _configuration.VariabilityUpperTailPercentage
                );
                sub2Header.SaveSummarySection(section);
            }

            if (data.ActiveSubstances.Count == 1 || data.CorrectedRelativePotencyFactors != null) {
                var relativePotencyFactors = data.CorrectedRelativePotencyFactors
                    ?? data.ActiveSubstances.ToDictionary(r => r, r => 1D);
                var membershipProbabilities = data.MembershipProbabilities
                    ?? data.ActiveSubstances.ToDictionary(r => r, r => 1D);
                var exposures = result.AggregateIndividualExposures
                    .Select(c => c.GetTotalExposureAtTarget(
                            data.TargetExposureUnit.Target,
                            relativePotencyFactors,
                            membershipProbabilities
                        )
                    ).ToList();
                var samplingWeights = result.AggregateIndividualExposures
                    .Select(c => c.SimulatedIndividual.SamplingWeight)
                    .ToList();
                {
                    var section = new IntakePercentileSection();
                    var sub2Header = subHeader.AddSubSectionHeaderFor(section, "Percentiles", 3);
                    section.Summarize(
                        exposures,
                        samplingWeights,
                        data.ReferenceSubstance,
                        [.. _configuration.SelectedPercentiles]
                    );
                    sub2Header.SaveSummarySection(section);
                }

                {
                    var section = new IntakePercentageSection();
                    var sub2Header = subHeader.AddSubSectionHeaderFor(section, "Percentages", 4);
                    var exposureLevels = ExposureLevelsCalculator.GetExposureLevels(
                        exposures,
                        _configuration.ExposureMethod,
                        [.. _configuration.ExposureLevels]
                    );
                    section.Summarize(
                        exposures,
                        samplingWeights,
                        data.ReferenceSubstance,
                        exposureLevels
                    );
                    sub2Header.SaveSummarySection(section);
                }
            }

            if (_configuration.StoreIndividualDayIntakes
                && !_configuration.SkipPrivacySensitiveOutputs
            ) {
                var individualDaysection = new IndividualSubstanceExposureSection();
                var sub2Header = subHeader.AddSubSectionHeaderFor(individualDaysection, "Simulated individual exposures", 10);
                individualDaysection
                    .Summarize(
                        result.AggregateIndividualExposures,
                        data.ActiveSubstances,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        result.KineticConversionFactors,
                        result.TargetExposureUnit,
                        data.ExternalExposureUnit,
                        data.ReferenceSubstance ?? data.ActiveSubstances.First(),
                        true
                    );
                sub2Header.SaveSummarySection(individualDaysection);
            }
        }
        private void summarizeDailyExposures(
            TargetExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int subOrder
        ) {
            var subHeader = header.AddEmptySubSectionHeader("Exposures (daily intakes)", subOrder++);
            if (data.ActiveSubstances.Count == 1 || data.CorrectedRelativePotencyFactors != null) {
                var relativePotencyFactors = data.CorrectedRelativePotencyFactors ?? data.ActiveSubstances.ToDictionary(r => r, r => 1D);
                var membershipProbabilities = data.MembershipProbabilities ?? data.ActiveSubstances.ToDictionary(r => r, r => 1D);
                {
                    var section = new InternalUntransformedExposureDistributionSection();
                    var sub2Header = subHeader.AddSubSectionHeaderFor(section, "Graph untransformed", 1);
                    section.Summarize(
                        result.AggregateIndividualDayExposures,
                        relativePotencyFactors,
                        membershipProbabilities,
                        result.TargetExposureUnit.Target
                    );
                    sub2Header.SaveSummarySection(section);
                }

                var coExposureIntakes = result.AggregateIndividualDayExposures
                    .AsParallel()
                    .Where(idi => idi.IsCoExposure())
                    .Select(c => c.SimulatedIndividualDayId)
                    .ToList();
                {
                    var section = new InternalDistributionTotalSection();
                    var sub2Header = subHeader.AddSubSectionHeaderFor(section, "Graph total", 2);
                    section.Summarize(
                        coExposureIntakes,
                        result.AggregateIndividualDayExposures,
                        relativePotencyFactors,
                        membershipProbabilities,
                        result.KineticConversionFactors,
                        data.ExposureRoutes,
                        result.ExternalExposureUnit,
                        result.TargetExposureUnit,
                        GriddingFunctions.GetPlotPercentages(),
                        _configuration.UncertaintyLowerBound,
                        _configuration.UncertaintyUpperBound
                    );
                    sub2Header.SaveSummarySection(section);
                }

                {
                    var section = new InternalAcuteDistributionUpperSection();
                    var sub2Header = subHeader.AddSubSectionHeaderFor(section, "Graph upper tail", 3);
                    section.Summarize(
                        coExposureIntakes,
                        result.AggregateIndividualDayExposures,
                        relativePotencyFactors,
                        membershipProbabilities,
                            result.KineticConversionFactors,
                        data.ExposureRoutes,
                        result.ExternalExposureUnit,
                        result.TargetExposureUnit,
                        _configuration.VariabilityUpperTailPercentage,
                        _configuration.UncertaintyLowerBound,
                        _configuration.UncertaintyUpperBound
                    );
                    sub2Header.SaveSummarySection(section);
                }

                var exposures = result.AggregateIndividualDayExposures
                    .Select(c => c.GetTotalExposureAtTarget(
                        result.TargetExposureUnit.Target,
                        relativePotencyFactors,
                        membershipProbabilities
                    ))
                    .ToList();
                var samplingWeights = result.AggregateIndividualDayExposures
                    .Select(c => c.SimulatedIndividual.SamplingWeight)
                    .ToList();

                {
                    var section = new IntakePercentileSection();
                    var sub2Header = subHeader.AddSubSectionHeaderFor(section, "Percentiles", 3);
                    section.Summarize(
                        exposures,
                        samplingWeights,
                        data.ReferenceSubstance,
                        [.. _configuration.SelectedPercentiles]
                    );
                    sub2Header.SaveSummarySection(section);
                }

                {
                    var section = new IntakePercentageSection();
                    var sub2Header = subHeader.AddSubSectionHeaderFor(section, "Percentages", 4);
                    var exposureLevels = ExposureLevelsCalculator.GetExposureLevels(
                        exposures,
                        _configuration.ExposureMethod,
                        [.. _configuration.ExposureLevels]
                    );
                    section.Summarize(
                        exposures,
                        samplingWeights,
                        data.ReferenceSubstance,
                        exposureLevels
                    );
                    sub2Header.SaveSummarySection(section);
                }

                if (_configuration.StoreIndividualDayIntakes
                    && !_configuration.SkipPrivacySensitiveOutputs
                ) {
                    var individualDaySection = new IndividualDaySubstanceExposureSection();
                    var sub2Header = subHeader.AddSubSectionHeaderFor(individualDaySection, "Simulated individual day exposures", 10);
                    individualDaySection.Summarize(
                        result.AggregateIndividualDayExposures,
                        data.ActiveSubstances,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        result.KineticConversionFactors,
                        result.TargetExposureUnit,
                        result.ExternalExposureUnit,
                        data.ReferenceSubstance ?? data.ActiveSubstances.First(),
                        true
                    );
                    sub2Header.SaveSummarySection(individualDaySection);
                }
            }
        }

        /// <summary>
        /// Uncertainty
        /// </summary>
        public void SummarizeUncertain(
            SectionHeader header,
            TargetExposuresActionResult actionResult,
            ActionData data
        ) {
            SectionHeader subHeader, subHeader1, subSubHeader;
            subHeader = header.GetSubSectionHeader<TargetExposuresSummarySection>();

            var relativePotencyFactors = data.CorrectedRelativePotencyFactors;
            var membershipProbabilities = data.MembershipProbabilities;
            var kineticConversionFactors = actionResult.KineticConversionFactors;
            var activeSubstances = data.ActiveSubstances;
            var kineticModelInstances = data.KineticModelInstances;
            var routes = data.ExposureRoutes;
            var nonDietaryExposureRoutes = data.NonDietaryExposureRoutes;
            var exposureType = _configuration.ExposureType;
            var uncertaintyLowerBound = _configuration.UncertaintyLowerBound;
            var uncertaintyUpperBound = _configuration.UncertaintyUpperBound;
            var percentageForUpperTail = _configuration.VariabilityUpperTailPercentage;
            var isPerPerson = _configuration.IsPerPerson;
            var isAggregate = _configuration.ExposureSources.Count > 1;

            var outputSummary = subHeader?.GetSummarySection() as TargetExposuresSummarySection;
            if (outputSummary == null) {
                return;
            }
            var aggregateExposures = actionResult.AggregateIndividualExposures != null
                ? data.AggregateIndividualExposures
                : data.AggregateIndividualDayExposures.Cast<AggregateIndividualExposure>().ToList();
            if (activeSubstances.Count == 1 || relativePotencyFactors != null) {
                if (exposureType == ExposureType.Acute) {
                    relativePotencyFactors = relativePotencyFactors
                        ?? activeSubstances.ToDictionary(r => r, r => 1D);
                    membershipProbabilities = membershipProbabilities
                        ?? activeSubstances.ToDictionary(r => r, r => 1D);

                    var exposures = aggregateExposures
                        .Select(c => c
                            .GetTotalExposureAtTarget(
                                data.TargetExposureUnit.Target,
                                relativePotencyFactors,
                                membershipProbabilities
                            )
                        ).ToList();
                    var samplingWeights = aggregateExposures.Select(c => c.SimulatedIndividual.SamplingWeight).ToList();

                    //Percentiles
                    var sub2Header = subHeader.GetSubSectionHeader<IntakePercentileSection>();
                    if (sub2Header != null) {
                        var section = sub2Header.GetSummarySection() as IntakePercentileSection;
                        section.SummarizeUncertainty(
                            exposures,
                            samplingWeights,
                            uncertaintyLowerBound,
                            uncertaintyUpperBound
                        );
                        sub2Header.SaveSummarySection(section);
                    }

                    //Percentages
                    sub2Header = subHeader.GetSubSectionHeader<IntakePercentageSection>();
                    if (sub2Header != null) {
                        var section = sub2Header.GetSummarySection() as IntakePercentageSection;
                        section.SummarizeUncertainty(
                            exposures,
                            samplingWeights,
                            uncertaintyLowerBound,
                            uncertaintyUpperBound
                        );
                        sub2Header.SaveSummarySection(section);
                    }

                    //Percentiles for a grid of values
                    sub2Header = subHeader.GetSubSectionHeader<InternalDistributionTotalSection>();
                    if (sub2Header != null) {
                        var section = sub2Header.GetSummarySection() as InternalDistributionTotalSection;
                        section.SummarizeUncertainty(
                            exposures,
                            samplingWeights,
                            uncertaintyLowerBound,
                            uncertaintyUpperBound
                         );
                        sub2Header.SaveSummarySection(section);
                    }
                } else {
                    relativePotencyFactors = relativePotencyFactors
                        ?? activeSubstances.ToDictionary(r => r, r => 1D);
                    membershipProbabilities = membershipProbabilities
                        ?? activeSubstances.ToDictionary(r => r, r => 1D);

                    var exposures = aggregateExposures
                        .Select(c => c
                            .GetTotalExposureAtTarget(
                                data.TargetExposureUnit.Target,
                                relativePotencyFactors,
                                membershipProbabilities
                            )
                        ).ToList();
                    var samplingWeights = aggregateExposures.Select(c => c.SimulatedIndividual.SamplingWeight).ToList();

                    //Percentiles
                    var sub2Header = subHeader.GetSubSectionHeader<IntakePercentileSection>();
                    if (sub2Header != null) {
                        var section = sub2Header.GetSummarySection() as IntakePercentileSection;
                        section.SummarizeUncertainty(
                            exposures,
                            samplingWeights,
                            uncertaintyLowerBound,
                            uncertaintyUpperBound
                        );
                        sub2Header.SaveSummarySection(section);
                    }

                    //Percentages
                    sub2Header = subHeader.GetSubSectionHeader<IntakePercentageSection>();
                    if (sub2Header != null) {
                        var section = sub2Header.GetSummarySection() as IntakePercentageSection;
                        section.SummarizeUncertainty(
                            exposures,
                            samplingWeights,
                            uncertaintyLowerBound,
                            uncertaintyUpperBound
                        );
                        sub2Header.SaveSummarySection(section);
                    }

                    //Percentiles for a grid of values
                    sub2Header = subHeader.GetSubSectionHeader<InternalDistributionTotalSection>();
                    if (sub2Header != null) {
                        var section = sub2Header.GetSummarySection() as InternalDistributionTotalSection;
                        section.SummarizeUncertainty(
                            exposures,
                            samplingWeights,
                            uncertaintyLowerBound,
                            uncertaintyUpperBound
                            );
                        sub2Header.SaveSummarySection(section);
                    }
                }
            }

            if (actionResult.KineticModelCalculators?.Values.Any(r => r is PbkKineticConversionCalculator) ?? false) {
                foreach (var substance in activeSubstances) {
                    if (actionResult.KineticModelCalculators[substance] is PbkKineticConversionCalculator) {
                        var sectionTitle = data.ActiveSubstances.Count > 1
                            ? $"PBK model {substance.Name}"
                            : "PBK model";
                        subHeader = header
                            .GetSubSectionHeaderFromTitleString<PbkModelSimulationResultsSection>(sectionTitle)?
                            .GetSubSectionHeader<InternalVersusExternalExposuresSection>();
                        if (subHeader != null) {
                            var section = subHeader.GetSummarySection() as InternalVersusExternalExposuresSection;
                            var kineticModelInstance = kineticModelInstances.Single(c => c.IsHumanModel && c.Substances.Contains(substance));
                            section.SummarizeUncertainty(
                                actionResult.KineticConversionFactors,
                                substance,
                                routes
                            );
                            subHeader.SaveSummarySection(section);
                        }
                    }
                }
            }

            // Exposures by substance
            if (isAggregate) {
                subHeader = header.GetSubSectionHeader<TargetExposuresSummarySection>();
                if (subHeader != null) {
                    subSubHeader = subHeader.GetSubSectionHeader<ContributionByRouteSubstanceTotalSection>();
                    if (subSubHeader != null) {
                        var section = subSubHeader.GetSummarySection() as ContributionByRouteSubstanceTotalSection;
                        section.SummarizeUncertainty(
                            actionResult.AggregateIndividualExposures,
                            actionResult.AggregateIndividualDayExposures,
                            relativePotencyFactors,
                            membershipProbabilities,
                            actionResult.KineticConversionFactors,
                            activeSubstances,
                            actionResult.ExternalExposureUnit
                        );
                        subSubHeader.SaveSummarySection(section);
                    }
                    subSubHeader = subHeader.GetSubSectionHeader<ContributionByRouteSubstanceUpperSection>();
                    if (subSubHeader != null) {
                        var section = subSubHeader.GetSummarySection() as ContributionByRouteSubstanceUpperSection;
                        section.SummarizeUncertainty(
                            actionResult.AggregateIndividualExposures,
                            actionResult.AggregateIndividualDayExposures,
                            relativePotencyFactors,
                            membershipProbabilities,
                            actionResult.KineticConversionFactors,
                            activeSubstances,
                            actionResult.ExternalExposureUnit,
                            actionResult.TargetExposureUnit,
                            percentageForUpperTail
                        );
                        subSubHeader.SaveSummarySection(section);
                    }
                }
            }

            subHeader = header.GetSubSectionHeader<KineticConversionFactorSection>();
            if (subHeader != null) {
                var section = subHeader.GetSummarySection() as KineticConversionFactorSection;
                section.SummarizeUncertainty(
                    actionResult.KineticConversionFactors
                );
            }

            if (activeSubstances.Count > 1) {
                subHeader = header.GetSubSectionHeader<TargetExposuresSummarySection>();
                if (subHeader != null) {
                    subHeader1 = subHeader.GetSubSectionHeader<ContributionBySubstanceTotalSection>();
                    if (subHeader1 != null) {
                        var section = subHeader1.GetSummarySection() as ContributionBySubstanceTotalSection;
                        section.SummarizeUncertainty(
                            actionResult.AggregateIndividualExposures,
                            actionResult.AggregateIndividualDayExposures,
                            relativePotencyFactors,
                            membershipProbabilities,
                            actionResult.KineticConversionFactors,
                            activeSubstances,
                            actionResult.ExternalExposureUnit
                        );
                        subHeader1.SaveSummarySection(section);
                    }
                    subHeader1 = subHeader.GetSubSectionHeader<ContributionBySubstanceUpperSection>();
                    if (subHeader1 != null) {
                        var section = subHeader1.GetSummarySection() as ContributionBySubstanceUpperSection;
                        section.SummarizeUncertainty(
                            actionResult.AggregateIndividualExposures,
                            actionResult.AggregateIndividualDayExposures,
                            relativePotencyFactors,
                            membershipProbabilities,
                            actionResult.KineticConversionFactors,
                            activeSubstances,
                            actionResult.ExternalExposureUnit,
                            actionResult.TargetExposureUnit,
                            percentageForUpperTail
                        );
                        subHeader1.SaveSummarySection(section);
                    }
                }
            }
            header.SaveSummarySection(outputSummary);

            // Toc: Exposures by route
            if (isAggregate) {
                subHeader = header.GetSubSectionHeader<TargetExposuresSummarySection>();
                if (subHeader != null) {
                    subSubHeader = subHeader.GetSubSectionHeader<ContributionByRouteTotalSection>();
                    if (subSubHeader != null) {
                        var section = subSubHeader.GetSummarySection() as ContributionByRouteTotalSection;
                        section.SummarizeUncertainty(
                            actionResult.AggregateIndividualExposures,
                            actionResult.AggregateIndividualDayExposures,
                            activeSubstances,
                            relativePotencyFactors,
                            membershipProbabilities,
                            actionResult.KineticConversionFactors,
                            routes,
                            actionResult.ExternalExposureUnit
                        );
                        subSubHeader.SaveSummarySection(section);
                    }
                    subSubHeader = subHeader.GetSubSectionHeader<ContributionByRouteUpperSection>();
                    if (subSubHeader != null) {
                        var section = subSubHeader.GetSummarySection() as ContributionByRouteUpperSection;
                        section.SummarizeUncertainty(
                            actionResult.AggregateIndividualExposures,
                            actionResult.AggregateIndividualDayExposures,
                            activeSubstances,
                            relativePotencyFactors,
                            membershipProbabilities,
                            actionResult.KineticConversionFactors,
                            actionResult.ExternalExposureUnit,
                            actionResult.TargetExposureUnit,
                            percentageForUpperTail
                        );
                        subSubHeader.SaveSummarySection(section);
                    }
                }
            }

            // Toc: External exposures by source
            subHeader = header.GetSubSectionHeader<TargetExposuresSummarySection>();
            if (subHeader != null) {
                subSubHeader = subHeader.GetSubSectionHeader<ExternalContributionBySourceTotalSection>();
                if (subSubHeader != null) {
                    var section = subSubHeader.GetSummarySection() as ExternalContributionBySourceTotalSection;
                    section.SummarizeUncertainty(
                        actionResult.ExternalIndividualExposures,
                        data.ActiveSubstances,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        actionResult.ExternalExposureUnit,
                        _configuration.IsPerPerson
                    );
                    subSubHeader.SaveSummarySection(section);
                }
                subSubHeader = subHeader.GetSubSectionHeader<ExternalContributionBySourceUpperSection>();
                if (subSubHeader != null) {
                    var section = subSubHeader.GetSummarySection() as ExternalContributionBySourceUpperSection;
                    section.SummarizeUncertainty(
                        actionResult.ExternalIndividualExposures,
                        data.ActiveSubstances,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        percentageForUpperTail,
                        _configuration.IsPerPerson
                    );
                    subSubHeader.SaveSummarySection(section);
                }
            }

            // Toc: External exposures by route
            subHeader = header.GetSubSectionHeader<TargetExposuresSummarySection>();
            if (subHeader != null) {
                subSubHeader = subHeader.GetSubSectionHeader<ExternalContributionByRouteTotalSection>();
                if (subSubHeader != null) {
                    var section = subSubHeader.GetSummarySection() as ExternalContributionByRouteTotalSection;
                    section.SummarizeUncertainty(
                        actionResult.ExternalIndividualExposures,
                        data.ActiveSubstances,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        actionResult.ExternalExposureUnit,
                        _configuration.IsPerPerson
                    );
                    subSubHeader.SaveSummarySection(section);
                }
                subSubHeader = subHeader.GetSubSectionHeader<ExternalContributionByRouteUpperSection>();
                if (subSubHeader != null) {
                    var section = subSubHeader.GetSummarySection() as ExternalContributionByRouteUpperSection;
                    section.SummarizeUncertainty(
                        actionResult.ExternalIndividualExposures,
                        data.ActiveSubstances,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        percentageForUpperTail,
                        actionResult.ExternalExposureUnit,
                        _configuration.IsPerPerson
                    );
                    subSubHeader.SaveSummarySection(section);
                }
            }

            // Toc: External exposures by source and route
            subHeader = header.GetSubSectionHeader<TargetExposuresSummarySection>();
            if (subHeader != null) {
                subSubHeader = subHeader.GetSubSectionHeader<ExternalContributionBySourceRouteTotalSection>();
                if (subSubHeader != null) {
                    var section = subSubHeader.GetSummarySection() as ExternalContributionBySourceRouteTotalSection;
                    section.SummarizeUncertainty(
                        actionResult.ExternalIndividualExposures,
                        data.ActiveSubstances,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        actionResult.ExternalExposureUnit,
                        _configuration.IsPerPerson
                    );
                    subSubHeader.SaveSummarySection(section);
                }
                subSubHeader = subHeader.GetSubSectionHeader<ExternalContributionBySourceRouteUpperSection>();
                if (subSubHeader != null) {
                    var section = subSubHeader.GetSummarySection() as ExternalContributionBySourceRouteUpperSection;
                    section.SummarizeUncertainty(
                        actionResult.ExternalIndividualExposures,
                        data.ActiveSubstances,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        percentageForUpperTail,
                        actionResult.ExternalExposureUnit,
                        _configuration.IsPerPerson
                    );
                    subSubHeader.SaveSummarySection(section);
                }
            }

            // Toc: Internal exposures by source
            subHeader = header.GetSubSectionHeader<TargetExposuresSummarySection>();
            if (subHeader != null) {
                subSubHeader = subHeader.GetSubSectionHeader<ContributionBySourceTotalSection>();
                if (subSubHeader != null) {
                    var section = subSubHeader.GetSummarySection() as ContributionBySourceTotalSection;
                    section.SummarizeUncertainty(
                        actionResult.ExternalIndividualExposures,
                        data.ActiveSubstances,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        actionResult.KineticConversionFactors,
                        actionResult.ExternalExposureUnit,
                        _configuration.IsPerPerson
                    );
                    subSubHeader.SaveSummarySection(section);
                }
                subSubHeader = subHeader.GetSubSectionHeader<ContributionBySourceUpperSection>();
                if (subSubHeader != null) {
                    var section = subSubHeader.GetSummarySection() as ContributionBySourceUpperSection;
                    section.SummarizeUncertainty(
                        actionResult.ExternalIndividualExposures,
                        data.ActiveSubstances,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        actionResult.KineticConversionFactors,
                        percentageForUpperTail,
                        actionResult.ExternalExposureUnit,
                        _configuration.IsPerPerson
                    );
                    subSubHeader.SaveSummarySection(section);
                }
            }

            // Toc: Internal exposures by source and route
            subHeader = header.GetSubSectionHeader<TargetExposuresSummarySection>();
            if (subHeader != null) {
                subSubHeader = subHeader.GetSubSectionHeader<ContributionBySourceRouteTotalSection>();
                if (subSubHeader != null) {
                    var section = subSubHeader.GetSummarySection() as ContributionBySourceRouteTotalSection;
                    section.SummarizeUncertainty(
                        actionResult.ExternalIndividualExposures,
                        data.ActiveSubstances,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        actionResult.KineticConversionFactors,
                        actionResult.ExternalExposureUnit,
                        _configuration.IsPerPerson
                    );
                    subSubHeader.SaveSummarySection(section);
                }
                subSubHeader = subHeader.GetSubSectionHeader<ContributionBySourceRouteUpperSection>();
                if (subSubHeader != null) {
                    var section = subSubHeader.GetSummarySection() as ContributionBySourceRouteUpperSection;
                    section.SummarizeUncertainty(
                        actionResult.ExternalIndividualExposures,
                        data.ActiveSubstances,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        actionResult.KineticConversionFactors,
                        percentageForUpperTail,
                        actionResult.ExternalExposureUnit,
                        _configuration.IsPerPerson
                    );
                    subSubHeader.SaveSummarySection(section);
                }
            }

            // Toc: Internal exposures by source and substance
            subHeader = header.GetSubSectionHeader<TargetExposuresSummarySection>();
            if (subHeader != null) {
                subSubHeader = subHeader.GetSubSectionHeader<ContributionBySourceSubstanceTotalSection>();
                if (subSubHeader != null) {
                    var section = subSubHeader.GetSummarySection() as ContributionBySourceSubstanceTotalSection;
                    section.SummarizeUncertainty(
                        actionResult.ExternalIndividualExposures,
                        data.ActiveSubstances,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        actionResult.KineticConversionFactors,
                        actionResult.ExternalExposureUnit,
                        _configuration.IsPerPerson
                    );
                    subSubHeader.SaveSummarySection(section);
                }
                subSubHeader = subHeader.GetSubSectionHeader<ContributionBySourceSubstanceUpperSection>();
                if (subSubHeader != null) {
                    var section = subSubHeader.GetSummarySection() as ContributionBySourceSubstanceUpperSection;
                    section.SummarizeUncertainty(
                        actionResult.ExternalIndividualExposures,
                        data.ActiveSubstances,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        actionResult.KineticConversionFactors,
                        percentageForUpperTail,
                        actionResult.ExternalExposureUnit,
                        _configuration.IsPerPerson
                    );
                    subSubHeader.SaveSummarySection(section);
                }
            }
            // Toc: Internal exposures by source, route and substance
            subHeader = header.GetSubSectionHeader<TargetExposuresSummarySection>();
            if (subHeader != null) {
                subSubHeader = subHeader.GetSubSectionHeader<ContributionBySourceRouteSubstanceTotalSection>();
                if (subSubHeader != null) {
                    var section = subSubHeader.GetSummarySection() as ContributionBySourceRouteSubstanceTotalSection;
                    section.SummarizeUncertainty(
                        actionResult.ExternalIndividualExposures,
                        data.ActiveSubstances,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        actionResult.KineticConversionFactors,
                        actionResult.ExternalExposureUnit,
                        _configuration.IsPerPerson
                    );
                    subSubHeader.SaveSummarySection(section);
                }
                subSubHeader = subHeader.GetSubSectionHeader<ContributionBySourceRouteSubstanceUpperSection>();
                if (subSubHeader != null) {
                    var section = subSubHeader.GetSummarySection() as ContributionBySourceRouteSubstanceUpperSection;
                    section.SummarizeUncertainty(
                        actionResult.ExternalIndividualExposures,
                        data.ActiveSubstances,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        actionResult.KineticConversionFactors,
                        percentageForUpperTail,
                        actionResult.ExternalExposureUnit,
                        _configuration.IsPerPerson
                    );
                    subSubHeader.SaveSummarySection(section);
                }
            }


            // Toc: External exposure distributions by source
            if (isAggregate) {
                foreach (var externalExposureCollection in actionResult.ExternalExposureCollections) {
                    subHeader = header.GetSubSectionHeaderBySectionLabel(
                        $"{TargetExposuresSections.ExternalExposuresDistributionsBySourceSection}-{externalExposureCollection.ExposureSource}"
                    );
                    summarizeExternalSourceExposureUncertain(
                        subHeader,
                        externalExposureCollection,
                        data
                    );
                }
            }
        }

        /// <summary>
        /// Summarize exposure by substance.
        /// </summary>
        private void summarizeExposureBySubstance(
            TargetExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var headSection = new TargetExposuresSummarySection() {
                SectionLabel = getSectionLabel(TargetExposuresSections.ExposuresBySubstanceSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                headSection,
                "Exposures by substance",
                order++
            );
            var subOrder = order;
            {
                {
                    var section = new ExposureBySubstanceSection();
                    var sub2Header = subHeader.AddSubSectionHeaderFor(section, "Exposure distribution", subOrder++);
                    section.Summarize(
                        result.AggregateIndividualExposures,
                        result.AggregateIndividualDayExposures,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        result.KineticConversionFactors,
                        data.ActiveSubstances,
                        _configuration.VariabilityLowerPercentage,
                        _configuration.VariabilityUpperPercentage,
                        data.TargetExposureUnit,
                        result.ExternalExposureUnit,
                        _configuration.SkipPrivacySensitiveOutputs
                    );
                    sub2Header.SaveSummarySection(section);

                }

                if (data.CorrectedRelativePotencyFactors != null) {
                    var sub2Header = subHeader.AddEmptySubSectionHeader($"Contributions", subOrder++);
                    {
                        var section = new ContributionBySubstanceTotalSection();
                        var sub3Header = sub2Header.AddSubSectionHeaderFor(section, "Contributions to total distribution", subOrder++);
                        section.Summarize(
                            result.AggregateIndividualExposures,
                            result.AggregateIndividualDayExposures,
                            data.ActiveSubstances,
                            data.CorrectedRelativePotencyFactors,
                            data.MembershipProbabilities,
                            result.KineticConversionFactors,
                            _configuration.UncertaintyLowerBound,
                            _configuration.UncertaintyUpperBound,
                            result.ExternalExposureUnit,
                            result.TargetExposureUnit
                        );
                        sub3Header.SaveSummarySection(section);
                    }
                    {
                        var section = new ContributionBySubstanceUpperSection();
                        var sub3Header = sub2Header.AddSubSectionHeaderFor(section, "Contributions to upper distribution", subOrder++);
                        section.Summarize(
                            result.AggregateIndividualExposures,
                            result.AggregateIndividualDayExposures,
                            data.ActiveSubstances,
                            data.CorrectedRelativePotencyFactors,
                            data.MembershipProbabilities,
                            result.KineticConversionFactors,
                            _configuration.VariabilityUpperTailPercentage,
                            _configuration.UncertaintyLowerBound,
                            _configuration.UncertaintyUpperBound,
                            result.ExternalExposureUnit,
                            result.TargetExposureUnit
                       );
                        sub3Header.SaveSummarySection(section);
                    }

                }
            }
            {
                var sub2Header = subHeader.AddEmptySubSectionHeader($"Co-exposure", subOrder++);
                if (_configuration.ExposureSources.Count > 1) {
                    var section = new CoExposureTotalDistributionSubstanceSection();
                    var sub3Header = sub2Header.AddSubSectionHeaderFor(section, "Co-exposure total distribution", subOrder++);
                    section.Summarize(
                        result.AggregateIndividualExposures,
                        result.AggregateIndividualDayExposures,
                        data.ActiveSubstances,
                        result.TargetExposureUnit
                    );
                    sub3Header.SaveSummarySection(section);
                }
                if (_configuration.ExposureSources.Count > 1 &&
                    data.CorrectedRelativePotencyFactors != null) {
                    var section = new CoExposureUpperDistributionSubstanceSection();
                    var sub3Header = sub2Header.AddSubSectionHeaderFor(section, "Co-exposure upper tail", subOrder++);
                    section.Summarize(
                        result.AggregateIndividualExposures,
                        result.AggregateIndividualDayExposures,
                        data.ActiveSubstances,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        result.KineticConversionFactors,
                        _configuration.VariabilityUpperTailPercentage,
                        result.ExternalExposureUnit,
                        result.TargetExposureUnit
                    );
                    sub3Header.SaveSummarySection(section);
                }
            }
            if (_configuration.StoreIndividualDayIntakes
                && !_configuration.SkipPrivacySensitiveOutputs
            ) {

                if (result.AggregateIndividualExposures != null) {
                    var section = new IndividualSubstanceExposureSection();
                    var sub2Header = subHeader.AddSubSectionHeaderFor(section, "Individual exposures by substance", subOrder++);
                    section.Summarize(
                        result.AggregateIndividualExposures,
                        data.ActiveSubstances,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        result.KineticConversionFactors,
                        result.TargetExposureUnit,
                        result.ExternalExposureUnit
                    );
                    sub2Header.SaveSummarySection(section);
                } else {
                    var section = new IndividualDaySubstanceExposureSection();
                    var sub2Header = subHeader.AddSubSectionHeaderFor(section, "Individual day exposures by substance", subOrder++);
                    section.Summarize(
                        result.AggregateIndividualDayExposures,
                        data.ActiveSubstances,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        result.KineticConversionFactors,
                        result.TargetExposureUnit,
                        result.ExternalExposureUnit
                    );
                    sub2Header.SaveSummarySection(section);
                }
            }
            subHeader.SaveSummarySection(headSection);
        }

        /// <summary>
        /// Summarize kinetic conversion factors
        /// </summary>
        private void summarizeKineticConversionFactors(
            TargetExposuresActionResult result,
            SectionHeader header,
            int order
        ) {
            var section = new KineticConversionFactorSection() {
                SectionLabel = getSectionLabel(TargetExposuresSections.KineticConversionFactorsSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(section, "Kinetic conversion factors", order++);
            section.Summarize(
                result.KineticConversionFactors,
                _configuration.UncertaintyLowerBound,
                _configuration.UncertaintyUpperBound

            );
            subHeader.SaveSummarySection(section);
        }

        /// <summary>
        /// Summarize contributions by route substance
        /// </summary>
        private void summarizeExposureByRouteSubstance(
            TargetExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var subHeader = header
                .AddEmptySubSectionHeader(
                    "Exposures by route and substance",
                    order++,
                    TargetExposuresSections.ExposuresByRouteSubstanceSection.ToString()
                );
            {
                var section = new ExposureByRouteSubstanceSection();
                var sub2Header = subHeader.AddSubSectionHeaderFor(section, "Exposure distribution", order++);
                section.Summarize(
                    result.AggregateIndividualExposures,
                    result.AggregateIndividualDayExposures,
                    data.ActiveSubstances,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    result.KineticConversionFactors,
                    _configuration.VariabilityLowerPercentage,
                    _configuration.VariabilityUpperPercentage,
                    result.TargetExposureUnit,
                    result.ExternalExposureUnit,
                    _configuration.SkipPrivacySensitiveOutputs
                );
                sub2Header.SaveSummarySection(section);
            }

            {
                if (data.CorrectedRelativePotencyFactors != null) {
                    var sub2Header = subHeader.AddEmptySubSectionHeader($"Contributions", order++);
                    {
                        var section = new ContributionByRouteSubstanceTotalSection();
                        var sub3Header = sub2Header.AddSubSectionHeaderFor(section, "Contributions to total distribution", order++);
                        section.Summarize(
                            result.AggregateIndividualExposures,
                            result.AggregateIndividualDayExposures,
                            data.ActiveSubstances,
                            data.CorrectedRelativePotencyFactors,
                            data.MembershipProbabilities,
                            result.KineticConversionFactors,
                            _configuration.UncertaintyLowerBound,
                            _configuration.UncertaintyUpperBound,
                            result.ExternalExposureUnit
                        );
                        sub3Header.SaveSummarySection(section);
                    }
                    {
                        var section = new ContributionByRouteSubstanceUpperSection();
                        var sub3Header = sub2Header.AddSubSectionHeaderFor(section, "Contributions to upper distribution", order++);
                        section.Summarize(
                            result.AggregateIndividualExposures,
                            result.AggregateIndividualDayExposures,
                            data.ActiveSubstances,
                            data.CorrectedRelativePotencyFactors,
                            data.MembershipProbabilities,
                            result.KineticConversionFactors,
                            _configuration.VariabilityUpperTailPercentage,
                            _configuration.UncertaintyLowerBound,
                            _configuration.UncertaintyUpperBound,
                            result.TargetExposureUnit,
                            result.ExternalExposureUnit
                       );
                        sub3Header.SaveSummarySection(section);
                    }
                }
            }
        }

        /// <summary>
        /// Summarize PBK model simulation results.
        /// </summary>
        private void summarizePbkModelSimulationResults(
            TargetExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var substances = data.ActiveSubstances;

            var allTargetExposures = _configuration.ExposureType == ExposureType.Acute
                ? result.AggregateIndividualDayExposures
                    .Cast<AggregateIndividualExposure>()
                    .ToList()
                : [.. result.AggregateIndividualExposures];

            var drilldownIndividualIds = PbkModelTimeCourseSection
                .GetDrilldownIndividualIds(
                    allTargetExposures,
                    data.ActiveSubstances,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    _configuration.VariabilityDrilldownPercentage,
                    result.TargetExposureUnit
                );

            var selectedTargetExposures = allTargetExposures
                .Where(c => drilldownIndividualIds.Contains(c.SimulatedIndividual.Id))
                .ToList();

            if (substances.Count == 1) {
                // Single substance
                var substance = substances.First();
                if (result.KineticModelCalculators[substance] is PbkKineticConversionCalculator) {
                    summarizePbkModelSimulationResults(
                        result,
                        data,
                        data.KineticModelInstances.Single(c => c.IsHumanModel && c.Substances.Contains(substance)),
                        substance,
                        allTargetExposures,
                        selectedTargetExposures,
                        "PBK model",
                        header,
                        order
                    );
                }
            } else {
                // Multiple substance
                var subHeader = header.AddEmptySubSectionHeader("PBK models", order++);
                var subOrder = 0;

                var pbkModelCalculators = result.KineticModelCalculators.Values
                    .Where(r => r is PbkKineticConversionCalculator)
                    .Cast<PbkKineticConversionCalculator>()
                    .ToList();

                foreach (var calculator in pbkModelCalculators) {
                    summarizePbkModelSimulationResults(
                        result,
                        data,
                        calculator.KineticModelInstance,
                        calculator.Substance,
                        allTargetExposures,
                        selectedTargetExposures,
                        $"PBK model {calculator.Substance.Name}",
                        subHeader,
                        subOrder++
                    );
                }
            }
        }

        private void summarizePbkModelSimulationResults(
            TargetExposuresActionResult actionResult,
            ActionData data,
            KineticModelInstance kineticModelInstance,
            Compound substance,
            ICollection<AggregateIndividualExposure> allTargetExposures,
            ICollection<AggregateIndividualExposure> selectedTargetExposures,
            string subTitle,
            SectionHeader header,
            int order
        ) {
            var section = new PbkModelSimulationResultsSection();
            var subHeader = header.AddSubSectionHeaderFor(section, subTitle, order);
            section.Summarize(
                substance,
                kineticModelInstance,
                data.ExposureRoutes,
                [data.TargetExposureUnit]
            );
            subHeader.SaveSummarySection(section);

            var subOrder = 0;
            summarizeInternalVersusExternalExposures(
                actionResult,
                data.ExposureRoutes,
                substance,
                allTargetExposures,
                [data.TargetExposureUnit],
                subHeader,
                subOrder++
            );

            if (selectedTargetExposures != null) {
                summarizePbkModelTimeCourse(
                    data.ExposureRoutes,
                    kineticModelInstance,
                    substance,
                    selectedTargetExposures,
                    [data.TargetExposureUnit],
                    actionResult.ExternalExposureUnit,
                    subHeader,
                    subOrder++
                );
            }
        }

        private void summarizeInternalVersusExternalExposures(
            TargetExposuresActionResult actionResult,
            ICollection<ExposureRoute> routes,
            Compound substance,
            ICollection<AggregateIndividualExposure> allTargetExposures,
            List<TargetUnit> targetUnits,
            SectionHeader header,
            int order
        ) {
            var section = new InternalVersusExternalExposuresSection();
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Internal versus external exposures",
                order
            );
            section.Summarize(
                substance,
                routes,
                allTargetExposures,
                actionResult.KineticConversionFactors,
                targetUnits,
                _configuration.ExposureType,
                actionResult.ExternalExposureUnit,
                _configuration.UncertaintyLowerBound,
                _configuration.UncertaintyUpperBound
            );
            subHeader.SaveSummarySection(section);
        }

        private void summarizePbkModelTimeCourse(
            ICollection<ExposureRoute> routes,
            KineticModelInstance kineticModelInstance,
            Compound substance,
            ICollection<AggregateIndividualExposure> selectedTargetExposures,
            ICollection<TargetUnit> targetUnits,
            ExposureUnitTriple externalExposureUnit,
            SectionHeader header,
            int subOrder
        ) {
            var section = new PbkModelTimeCourseSection();
            var subHeader = header.AddSubSectionHeaderFor(
                section: section,
                title: $"Individual drilldown PBK model {substance.Name}",
                order: subOrder++
            );
            section.Summarize(
                selectedTargetExposures,
                routes,
                substance,
                targetUnits,
                externalExposureUnit,
                _configuration.NonStationaryPeriodInDays
            );
            subHeader.SaveSummarySection(section);
        }

        /// <summary>
        /// Summarize exposure by route.
        /// </summary>
        private void summarizeExposuresByRoute(
            TargetExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var subHeader = header
                .AddEmptySubSectionHeader(
                    "Exposures by route",
                    order++,
                    TargetExposuresSections.ExposuresByRouteSection.ToString()
                );
            {
                var section = new ExposureByRouteSection();
                var sub2Header = subHeader.AddSubSectionHeaderFor(section, "Exposure distribution", 1);
                section.Summarize(
                    result.AggregateIndividualExposures,
                    result.AggregateIndividualDayExposures,
                    data.ActiveSubstances,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    result.KineticConversionFactors,
                    _configuration.VariabilityLowerPercentage,
                    _configuration.VariabilityUpperPercentage,
                    result.TargetExposureUnit,
                    result.ExternalExposureUnit,
                    _configuration.SkipPrivacySensitiveOutputs
                );
                sub2Header.SaveSummarySection(section);
            }
            {
                var sub2Header = subHeader.AddEmptySubSectionHeader($"Contributions", 2);
                {
                    var section = new ContributionByRouteTotalSection();
                    var sub3Header = sub2Header.AddSubSectionHeaderFor(section, "Contributions to total distribution", 1);
                    section.Summarize(
                        result.AggregateIndividualExposures,
                        result.AggregateIndividualDayExposures,
                        data.ActiveSubstances,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        result.KineticConversionFactors,
                        result.ExternalExposureUnit
,
                        _configuration.UncertaintyLowerBound,
                        _configuration.UncertaintyUpperBound);
                    sub3Header.SaveSummarySection(section);
                }
                {
                    var section = new ContributionByRouteUpperSection();
                    var sub3Header = sub2Header.AddSubSectionHeaderFor(section, "Contributions to upper distribution", 2);
                    section.Summarize(
                        result.AggregateIndividualExposures,
                        result.AggregateIndividualDayExposures,
                        data.ActiveSubstances,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        result.KineticConversionFactors,
                        _configuration.VariabilityUpperTailPercentage,
                        _configuration.UncertaintyLowerBound,
                        _configuration.UncertaintyUpperBound,
                        result.TargetExposureUnit,
                        result.ExternalExposureUnit
                    );
                    sub3Header.SaveSummarySection(section);
                }
            }
        }

        /// <summary>
        /// Summarize exposures by source
        /// </summary>
        private void summarizeExternalExposureBySource(
            TargetExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var subHeader = header
                .AddEmptySubSectionHeader(
                    "Exposures by source",
                    order++,
                    TargetExposuresSections.ExternalExposuresBySourceSection.ToString()
                );
            {
                var section = new ExternalExposureBySourceSection();
                var sub2Header = subHeader.AddSubSectionHeaderFor(section, "Exposure distribution", 1);
                section.Summarize(
                    result.ExternalIndividualExposures,
                    data.ActiveSubstances,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    _configuration.VariabilityLowerPercentage,
                    _configuration.VariabilityUpperPercentage,
                    result.ExternalExposureUnit,
                    _configuration.IsPerPerson,
                    _configuration.SkipPrivacySensitiveOutputs
                );
                sub2Header.SaveSummarySection(section);
            }
            {
                var sub2Header = subHeader.AddEmptySubSectionHeader($"Contributions", 2);
                {
                    var section = new ExternalContributionBySourceTotalSection();
                    var sub3Header = sub2Header.AddSubSectionHeaderFor(section, "Contributions to total distribution", 1);
                    section.Summarize(
                        result.ExternalIndividualExposures,
                        data.ActiveSubstances,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        _configuration.UncertaintyLowerBound,
                        _configuration.UncertaintyUpperBound,
                        result.ExternalExposureUnit,
                        _configuration.IsPerPerson

                    );
                    sub3Header.SaveSummarySection(section);
                }
                {
                    var section = new ExternalContributionBySourceUpperSection();
                    var sub3Header = sub2Header.AddSubSectionHeaderFor(section, "Contributions to upper distribution", 2);
                    section.Summarize(
                        result.ExternalIndividualExposures,
                        data.ActiveSubstances,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        _configuration.VariabilityUpperTailPercentage,
                        _configuration.UncertaintyLowerBound,
                        _configuration.UncertaintyUpperBound,
                        result.ExternalExposureUnit,
                        _configuration.IsPerPerson
                    );
                    sub3Header.SaveSummarySection(section);
                }
            }
        }

        /// <summary>
        /// Summarize exposures by source and route
        /// </summary>
        private void summarizeExternalExposureBySourceRoute(
            TargetExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var subHeader = header
                .AddEmptySubSectionHeader(
                    "Exposures by source and route",
                    order++,
                    TargetExposuresSections.ExternalExposuresBySourceRouteSection.ToString()
                );
            {
                var section = new ExternalExposureBySourceRouteSection();
                var sub2Header = subHeader.AddSubSectionHeaderFor(section, "Exposure distribution", 1);
                section.Summarize(
                    result.ExternalIndividualExposures,
                    data.ActiveSubstances,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    data.ExposureRoutes,
                    _configuration.VariabilityLowerPercentage,
                    _configuration.VariabilityUpperPercentage,
                    result.ExternalExposureUnit,
                    _configuration.IsPerPerson,
                    _configuration.SkipPrivacySensitiveOutputs
                );
                sub2Header.SaveSummarySection(section);
            }
            {
                var sub2Header = subHeader.AddEmptySubSectionHeader($"Contributions", 2);
                {
                    var section = new ExternalContributionBySourceRouteTotalSection();
                    var sub3Header = sub2Header.AddSubSectionHeaderFor(section, "Contributions to total distribution", 1);
                    section.Summarize(
                        result.ExternalIndividualExposures,
                        data.ActiveSubstances,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        _configuration.UncertaintyLowerBound,
                        _configuration.UncertaintyUpperBound,
                        result.ExternalExposureUnit,
                        _configuration.IsPerPerson
                    );
                    sub3Header.SaveSummarySection(section);
                }
                {
                    var section = new ExternalContributionBySourceRouteUpperSection();
                    var sub3Header = sub2Header.AddSubSectionHeaderFor(section, "Contributions to upper distribution", 2);
                    section.Summarize(
                        result.ExternalIndividualExposures,
                        data.ActiveSubstances,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        _configuration.VariabilityUpperTailPercentage,
                        _configuration.UncertaintyLowerBound,
                        _configuration.UncertaintyUpperBound,
                        result.ExternalExposureUnit,
                        _configuration.IsPerPerson
                    );
                    sub3Header.SaveSummarySection(section);
                }
            }
        }

        /// <summary>
        /// Summarize exposures route
        /// </summary>
        private void summarizeExternalExposureByRoute(
            TargetExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var subHeader = header
                .AddEmptySubSectionHeader(
                    "Exposures by route",
                    order++,
                    TargetExposuresSections.ExternalExposuresByRouteSection.ToString()
                );
            {
                var section = new ExternalExposureByRouteSection();
                var sub2Header = subHeader.AddSubSectionHeaderFor(section, "Exposure distribution", 1);
                section.Summarize(
                    result.ExternalIndividualExposures,
                    data.ActiveSubstances,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    data.ExposureRoutes,
                    _configuration.VariabilityLowerPercentage,
                    _configuration.VariabilityUpperPercentage,
                    result.ExternalExposureUnit,
                    _configuration.IsPerPerson,
                    _configuration.SkipPrivacySensitiveOutputs
                );
                sub2Header.SaveSummarySection(section);
            }
            {
                var sub2Header = subHeader.AddEmptySubSectionHeader($"Contributions", 2);
                {
                    var section = new ExternalContributionByRouteTotalSection();
                    var sub3Header = sub2Header.AddSubSectionHeaderFor(section, "Contributions to total distribution", 1);
                    section.Summarize(
                        result.ExternalIndividualExposures,
                        data.ActiveSubstances,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        _configuration.UncertaintyLowerBound,
                        _configuration.UncertaintyUpperBound,
                        result.ExternalExposureUnit,
                        _configuration.IsPerPerson
                    );
                    sub3Header.SaveSummarySection(section);
                }
                {
                    var section = new ExternalContributionByRouteUpperSection();
                    var sub3Header = sub2Header.AddSubSectionHeaderFor(section, "Contributions to upper distribution", 2);
                    section.Summarize(
                        result.ExternalIndividualExposures,
                        data.ActiveSubstances,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        _configuration.VariabilityUpperTailPercentage,
                        _configuration.UncertaintyLowerBound,
                        _configuration.UncertaintyUpperBound,
                        result.ExternalExposureUnit,
                        _configuration.IsPerPerson
                    );
                    sub3Header.SaveSummarySection(section);
                }
            }
        }


        /// <summary>
        /// Summarize exposures by source
        /// </summary>
        private void summarizeExposureBySource(
            TargetExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var subHeader = header
                .AddEmptySubSectionHeader(
                    "Exposures by source",
                    order++,
                    TargetExposuresSections.ExposuresBySourceSection.ToString()
                );
            {

                var section = new ExposureBySourceSection();
                var sub2Header = subHeader.AddSubSectionHeaderFor(section, "Exposure distribution", 1);
                section.Summarize(
                    result.ExternalIndividualExposures,
                    data.ActiveSubstances,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    result.KineticConversionFactors,
                    _configuration.VariabilityLowerPercentage,
                    _configuration.VariabilityUpperPercentage,
                    result.TargetExposureUnit,
                    result.ExternalExposureUnit,
                    _configuration.IsPerPerson,
                    _configuration.SkipPrivacySensitiveOutputs
                );
                sub2Header.SaveSummarySection(section);
            }
            {
                var sub2Header = subHeader.AddEmptySubSectionHeader($"Contributions", 2);
                {
                    var section = new ContributionBySourceTotalSection();
                    var sub3Header = sub2Header.AddSubSectionHeaderFor(section, "Contributions to total distribution", 1);
                    section.Summarize(
                        result.ExternalIndividualExposures,
                        data.ActiveSubstances,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        result.KineticConversionFactors,
                        _configuration.UncertaintyLowerBound,
                        _configuration.UncertaintyUpperBound,
                        result.ExternalExposureUnit,
                        _configuration.IsPerPerson
                    );
                    sub3Header.SaveSummarySection(section);
                }
                {
                    var section = new ContributionBySourceUpperSection();
                    var sub3Header = sub2Header.AddSubSectionHeaderFor(section, "Contributions to upper distribution", 2);
                    section.Summarize(
                        result.ExternalIndividualExposures,
                        data.ActiveSubstances,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        result.KineticConversionFactors,
                        _configuration.VariabilityUpperTailPercentage,
                        _configuration.UncertaintyLowerBound,
                        _configuration.UncertaintyUpperBound,
                        result.ExternalExposureUnit,
                        _configuration.IsPerPerson
                    );
                    sub3Header.SaveSummarySection(section);
                }
            }
        }


        /// <summary>
        /// Summarize exposures by source
        /// </summary>
        private void summarizeExposureBySourceSubstance(
            TargetExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var subHeader = header
                .AddEmptySubSectionHeader(
                    "Exposures by source and substance",
                    order++,
                    TargetExposuresSections.ExposuresBySourceSubstanceSection.ToString()
                );
            {
                var section = new ExposureBySourceSubstanceSection();
                var sub2Header = subHeader.AddSubSectionHeaderFor(section, "Exposure distribution", 1);
                section.Summarize(
                    result.ExternalIndividualExposures,
                    data.ActiveSubstances,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    result.KineticConversionFactors,
                    _configuration.VariabilityLowerPercentage,
                    _configuration.VariabilityUpperPercentage,
                    result.TargetExposureUnit,
                    result.ExternalExposureUnit,
                    _configuration.IsPerPerson,
                    _configuration.SkipPrivacySensitiveOutputs
                );
                sub2Header.SaveSummarySection(section);
            }
            {
                var sub2Header = subHeader.AddEmptySubSectionHeader($"Contributions", 2);
                {
                    var section = new ContributionBySourceSubstanceTotalSection();
                    var sub3Header = sub2Header.AddSubSectionHeaderFor(section, "Contributions to total distribution", 1);
                    section.Summarize(
                        result.ExternalIndividualExposures,
                        data.ActiveSubstances,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        result.KineticConversionFactors,
                        _configuration.UncertaintyLowerBound,
                        _configuration.UncertaintyUpperBound,
                        result.ExternalExposureUnit,
                        _configuration.IsPerPerson
                    );
                    sub3Header.SaveSummarySection(section);
                }
                {
                    var section = new ContributionBySourceSubstanceUpperSection();
                    var sub3Header = sub2Header.AddSubSectionHeaderFor(section, "Contributions to upper distribution", 2);
                    section.Summarize(
                        result.ExternalIndividualExposures,
                        data.ActiveSubstances,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        result.KineticConversionFactors,
                        _configuration.VariabilityUpperTailPercentage,
                        _configuration.UncertaintyLowerBound,
                        _configuration.UncertaintyUpperBound,
                        result.ExternalExposureUnit,
                        _configuration.IsPerPerson
                    );
                    sub3Header.SaveSummarySection(section);
                }
            }
        }

        /// <summary>
        /// Summarize exposures by source and route
        /// </summary>
        private void summarizeExposureBySourceRoute(
            TargetExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var subHeader = header
                .AddEmptySubSectionHeader(
                    "Exposures by source and route",
                    order++,
                    TargetExposuresSections.ExposuresBySourceRouteSection.ToString()
                );
            {

                var section = new ExposureBySourceRouteSection();
                var sub2Header = subHeader.AddSubSectionHeaderFor(section, "Exposure distribution", 1);
                section.Summarize(
                    result.ExternalIndividualExposures,
                    data.ActiveSubstances,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    result.KineticConversionFactors,
                    _configuration.VariabilityLowerPercentage,
                    _configuration.VariabilityUpperPercentage,
                    result.TargetExposureUnit,
                    result.ExternalExposureUnit,
                    _configuration.IsPerPerson,
                    _configuration.SkipPrivacySensitiveOutputs
                );
                sub2Header.SaveSummarySection(section);
            }
            {
                var sub2Header = subHeader.AddEmptySubSectionHeader($"Contributions", 2);
                {
                    var section = new ContributionBySourceRouteTotalSection();
                    var sub3Header = sub2Header.AddSubSectionHeaderFor(section, "Contributions to total distribution", 1);
                    section.Summarize(
                        result.ExternalIndividualExposures,
                        data.ActiveSubstances,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        result.KineticConversionFactors,
                        _configuration.UncertaintyLowerBound,
                        _configuration.UncertaintyUpperBound,
                        result.ExternalExposureUnit,
                        _configuration.IsPerPerson
                    );
                    sub3Header.SaveSummarySection(section);
                }
                {
                    var section = new ContributionBySourceRouteUpperSection();
                    var sub3Header = sub2Header.AddSubSectionHeaderFor(section, "Contributions to upper distribution", 2);
                    section.Summarize(
                        result.ExternalIndividualExposures,
                        data.ActiveSubstances,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        result.KineticConversionFactors,
                        _configuration.VariabilityUpperTailPercentage,
                        _configuration.UncertaintyLowerBound,
                        _configuration.UncertaintyUpperBound,
                        result.ExternalExposureUnit,
                        _configuration.IsPerPerson
                    );
                    sub3Header.SaveSummarySection(section);
                }
            }
        }

        private void summarizeExposureBySourceRouteSubstance(
           TargetExposuresActionResult result,
           ActionData data,
           SectionHeader header,
           int order
       ) {
            var subHeader = header
                .AddEmptySubSectionHeader(
                    "Exposures by source, route and substance",
                    order++,
                    TargetExposuresSections.ExposuresBySourceRouteSubstanceSection.ToString()
                );
            {

                var section = new ExposureBySourceRouteSubstanceSection();
                var sub2Header = subHeader.AddSubSectionHeaderFor(section, "Exposure distribution", 1);
                section.Summarize(
                    result.ExternalIndividualExposures,
                    data.ActiveSubstances,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    result.KineticConversionFactors,
                    _configuration.VariabilityLowerPercentage,
                    _configuration.VariabilityUpperPercentage,
                    result.TargetExposureUnit,
                    result.ExternalExposureUnit,
                    _configuration.IsPerPerson,
                    _configuration.SkipPrivacySensitiveOutputs
                );
                sub2Header.SaveSummarySection(section);
            }
            {
                var sub2Header = subHeader.AddEmptySubSectionHeader($"Contributions", 2);
                {
                    var section = new ContributionBySourceRouteSubstanceTotalSection();
                    var sub3Header = sub2Header.AddSubSectionHeaderFor(section, "Contributions to total distribution", 1);
                    section.Summarize(
                        result.ExternalIndividualExposures,
                        data.ActiveSubstances,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        result.KineticConversionFactors,
                        _configuration.UncertaintyLowerBound,
                        _configuration.UncertaintyUpperBound,
                        result.ExternalExposureUnit,
                        _configuration.IsPerPerson
                    );
                    sub3Header.SaveSummarySection(section);
                }
                {
                    var section = new ContributionBySourceRouteSubstanceUpperSection();
                    var sub3Header = sub2Header.AddSubSectionHeaderFor(section, "Contributions to upper distribution", 2);
                    section.Summarize(
                        result.ExternalIndividualExposures,
                        data.ActiveSubstances,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        result.KineticConversionFactors,
                        _configuration.VariabilityUpperTailPercentage,
                        _configuration.UncertaintyLowerBound,
                        _configuration.UncertaintyUpperBound,
                        result.ExternalExposureUnit,
                        _configuration.IsPerPerson
                    );
                    sub3Header.SaveSummarySection(section);
                }
            }
        }
        public void summarizeExternalSourceExposure(
            SectionHeader header,
            ExternalExposureCollection externalExposureCollection,
            ActionData data,
            int order
        ) {
            var section = new ExternalExposureDistributionSection() {
                SectionLabel = getSectionLabel(TargetExposuresSections.ExternalExposuresBySourceSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                $"{externalExposureCollection.ExposureSource.GetShortDisplayName()} distribution",
                order
            );
            subHeader.SaveSummarySection(section);

            var externalIndividualDayExposures = externalExposureCollection.ExternalIndividualDayExposures;
            var relativePotencyFactors = data.CorrectedRelativePotencyFactors
                ?? data.ActiveSubstances.ToDictionary(r => r, r => 1D);

            var coExposures = externalIndividualDayExposures
                .AsParallel()
                .Select(idi => (
                    idi.SimulatedIndividualDayId,
                    IntakesPerCompound: idi.GetExposuresBySubstance().Count(g => g.Amount > 0)
                ))
                .Where(ipc => ipc.IntakesPerCompound > 1)
                .Select(c => c.SimulatedIndividualDayId)
                .ToHashSet();
            var totalExposureDistributionSection = new ExternalTotalExposureDistributionSection();
            var subSubHeader = subHeader.AddSubSectionHeaderFor(totalExposureDistributionSection, "Graph total", 1);
            totalExposureDistributionSection.Summarize(
                coExposures,
                externalIndividualDayExposures,
                relativePotencyFactors,
                data.MembershipProbabilities,
                GriddingFunctions.GetPlotPercentages(),
                _configuration.IsPerPerson,
                _configuration.UncertaintyLowerBound,
                _configuration.UncertaintyUpperBound
            );
            subSubHeader.SaveSummarySection(totalExposureDistributionSection);

            var upperExposureDistributionSection = new ExternalUpperExposureDistributionSection();
            subSubHeader = subHeader.AddSubSectionHeaderFor(upperExposureDistributionSection, "Graph upper tail", 2);
            upperExposureDistributionSection.Summarize(
                coExposures,
                externalIndividualDayExposures,
                relativePotencyFactors,
                data.MembershipProbabilities,
                _configuration.VariabilityUpperTailPercentage,
                _configuration.IsPerPerson,
                _configuration.UncertaintyLowerBound,
                _configuration.UncertaintyUpperBound
           );
            subSubHeader.SaveSummarySection(upperExposureDistributionSection);

            var samplingWeights = externalIndividualDayExposures
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .ToList();

            var externalExposures = externalIndividualDayExposures
                .Select(c => c.GetExposure(
                    relativePotencyFactors,
                    data.MembershipProbabilities,
                    _configuration.IsPerPerson)
                )
                .ToList();

            var exposureLevels = ExposureLevelsCalculator.GetExposureLevels(
                externalExposures,
                _configuration.ExposureMethod,
                [.. _configuration.ExposureLevels]);

            var percentileSection = new IntakePercentileSection();
            subSubHeader = subHeader.AddSubSectionHeaderFor(percentileSection, "Percentiles", 3);
            percentileSection.Summarize(externalExposures, samplingWeights, data.ReferenceSubstance, [.. _configuration.SelectedPercentiles]);
            subSubHeader.SaveSummarySection(percentileSection);

            var percentageSection = new IntakePercentageSection();
            subSubHeader = subHeader.AddSubSectionHeaderFor(percentageSection, "Percentages", 4);
            percentageSection.Summarize(externalExposures, samplingWeights, data.ReferenceSubstance, exposureLevels);
            subSubHeader.SaveSummarySection(percentageSection);
        }

        public void summarizeExternalSourceExposureByRoute(
            SectionHeader header,
            ExternalExposureCollection externalExposureCollection,
            ActionData data,
            int order
        ) {
            var subHeader = header.AddEmptySubSectionHeader("Exposures by route", order);
            var totalSection = new ExternalExposureTotalDistributionRouteSection();
            var subSubHeader = subHeader.AddSubSectionHeaderFor(totalSection, "Contributions to total distribution", 1);
            totalSection.Summarize(
                externalExposureCollection,
                data.CorrectedRelativePotencyFactors,
                data.MembershipProbabilities,
                data.ActiveSubstances,
                _configuration.ExposureType,
                _configuration.VariabilityLowerPercentage,
                _configuration.VariabilityUpperPercentage,
                _configuration.UncertaintyLowerBound,
                _configuration.UncertaintyUpperBound,
                _configuration.IsPerPerson
            );
            subSubHeader.SaveSummarySection(totalSection);

            var upperSection = new ExternalExposureUpperDistributionRouteSection();
            subSubHeader = subHeader.AddSubSectionHeaderFor(upperSection, "Contributions to upper distribution", 2);
            upperSection.Summarize(
                externalExposureCollection,
                data.CorrectedRelativePotencyFactors,
                data.MembershipProbabilities,
                data.ActiveSubstances,
                _configuration.ExposureType,
                _configuration.VariabilityUpperTailPercentage,
                _configuration.VariabilityLowerPercentage,
                _configuration.VariabilityUpperPercentage,
                _configuration.UncertaintyLowerBound,
                _configuration.UncertaintyUpperBound,
                _configuration.IsPerPerson
            );
            subSubHeader.SaveSummarySection(upperSection);
        }

        public void summarizeExternalSourceExposureByRouteSubstance(
            SectionHeader header,
            ExternalExposureCollection externalExposureCollection,
            ActionData data,
            int order
        ) {
            if (data.ActiveSubstances.Count > 1) {
                var subHeader = header.AddEmptySubSectionHeader("Exposures by route and substance", order);

                var totalSection = new ExternalExposureTotalDistributionRouteSubstanceSection();
                var subSubHeader = subHeader.AddSubSectionHeaderFor(totalSection, "Contributions to total distribution", 1);
                totalSection.Summarize(
                    data.ActiveSubstances,
                    externalExposureCollection,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    _configuration.ExposureType,
                    _configuration.VariabilityLowerPercentage,
                    _configuration.VariabilityUpperPercentage,
                    _configuration.UncertaintyLowerBound,
                    _configuration.UncertaintyUpperBound,
                    _configuration.IsPerPerson
                );
                subSubHeader.SaveSummarySection(totalSection);

                var upperSection = new ExternalExposureUpperDistributionRouteSubstanceSection();
                subSubHeader = subHeader.AddSubSectionHeaderFor(upperSection, "Contributions to upper distribution", 2);
                upperSection.Summarize(
                    data.ActiveSubstances,
                    externalExposureCollection,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    _configuration.ExposureType,
                    _configuration.VariabilityLowerPercentage,
                    _configuration.VariabilityUpperPercentage,
                    _configuration.VariabilityUpperTailPercentage,
                    _configuration.UncertaintyLowerBound,
                    _configuration.UncertaintyUpperBound,
                    _configuration.IsPerPerson
                );
                subSubHeader.SaveSummarySection(upperSection);
            }
        }

        public void summarizeExternalSourceExposureUncertain(
            SectionHeader header,
            ExternalExposureCollection externalExposureCollection,
            ActionData data
        ) {
            var relativePotencyFactors = data.CorrectedRelativePotencyFactors
                ?? data.ActiveSubstances.ToDictionary(r => r, r => 1D);
            var membershipProbabilities = data.MembershipProbabilities
                ?? data.ActiveSubstances.ToDictionary(r => r, r => 1D);

            var externalIndividualDayExposures = externalExposureCollection.ExternalIndividualDayExposures;
            var externalExposures = externalIndividualDayExposures
                .Select(c => c.GetExposure(
                    relativePotencyFactors,
                    membershipProbabilities,
                    _configuration.IsPerPerson
                ))
                .ToList();

            var weights = externalIndividualDayExposures.Select(c => c.SimulatedIndividual.SamplingWeight).ToList();
            var subHeader = header.GetSubSectionHeader<ExternalTotalExposureDistributionSection>();
            if (subHeader != null) {
                var section = subHeader.GetSummarySection() as ExternalTotalExposureDistributionSection;
                section.SummarizeUncertainty(
                    externalIndividualDayExposures,
                    relativePotencyFactors,
                    membershipProbabilities,
                    _configuration.IsPerPerson
                );
            }

            subHeader = header.GetSubSectionHeader<IntakePercentileSection>();
            if (subHeader != null) {
                var section = subHeader.GetSummarySection() as IntakePercentileSection;
                section.SummarizeUncertainty(externalExposures, weights, _configuration.UncertaintyLowerBound, _configuration.UncertaintyUpperBound);
            }

            subHeader = header.GetSubSectionHeader<IntakePercentageSection>();
            if (subHeader != null) {
                var section = subHeader.GetSummarySection() as IntakePercentageSection;
                section.SummarizeUncertainty(externalExposures, weights, _configuration.UncertaintyLowerBound, _configuration.UncertaintyUpperBound);
            }
        }

        private List<ActionSummaryUnitRecord> collectUnits(ActionData data) {
            var printOption = _configuration.TargetDoseLevelType == TargetLevelType.External
                ? TargetUnit.DisplayOption.AppendBiologicalMatrix
                : TargetUnit.DisplayOption.UnitOnly;
            var individualDayUnit = _configuration.ExposureType == ExposureType.Chronic
                ? "individuals"
                : "individual days";
            var actionSummaryTargetUnitRecord = new ActionSummaryUnitRecord(
                data.TargetExposureUnit.Target.Code,
                data.TargetExposureUnit.Target.ExpressionType == ExpressionType.SpecificGravity
                    ? data.TargetExposureUnit.GetShortDisplayName()
                    : data.TargetExposureUnit.GetShortDisplayName(TargetUnit.DisplayOption.AppendExpressionType)
            );
            var bodyWeightUnit = data.ExternalExposureUnit.IsPerBodyWeight
                ? data.ExternalExposureUnit.ConcentrationMassUnit.GetShortDisplayName()
                : BodyWeightUnit.kg.GetShortDisplayName();
            var result = new List<ActionSummaryUnitRecord> {
                new("IntakeUnit", data.TargetExposureUnit.GetShortDisplayName(printOption)),
                new("TargetAmountUnit", data.TargetExposureUnit.SubstanceAmountUnit.GetShortDisplayName()),
                new("BodyWeightUnit", bodyWeightUnit),
                new("TargetExposureUnit", data.TargetExposureUnit.GetShortDisplayName()),
                new("ExternalExposureUnit", data.ExternalExposureUnit.GetShortDisplayName()),
                new("IndividualDayUnit", individualDayUnit),
                new("LowerPercentage", $"p{_configuration.VariabilityLowerPercentage}"),
                new("UpperPercentage", $"p{_configuration.VariabilityUpperPercentage}"),
                new("LowerBound", $"p{_configuration.UncertaintyLowerBound}"),
                new("UpperBound", $"p{_configuration.UncertaintyUpperBound}"),
                new("AgeUnit", "years"),
                actionSummaryTargetUnitRecord
            };
            return result;
        }
    }
}

