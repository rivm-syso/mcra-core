using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.ExposureLevelsCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation.DesolvePbkModelCalculators;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation.SbmlModelCalculation;
using MCRA.Simulation.Calculators.NonDietaryIntakeCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.TargetExposures {

    public enum TargetExposuresSections {
        DetailsSection,
        ExposuresByRouteSection,
        ExposuresBySubstanceSection,
        ExposuresByRouteSubstanceSection,
        ExposuresBySourceSection,
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
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(outputSummary, ActionType.GetDisplayName(), order);
            subHeader.Units = collectUnits(data);
            subHeader.SaveSummarySection(outputSummary);
            int subOrder = 0;

            // Summarize acute
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

            // Summarize chronic
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

            SectionHeader subHeaderDetails = null;
            // Exposures by route
            if (_configuration.ExposureSources.Count > 1
                && (data.ActiveSubstances.Count == 1 || data.CorrectedRelativePotencyFactors != null)
                && outputSettings.ShouldSummarize(TargetExposuresSections.ExposuresByRouteSection)
            ) {
                subHeaderDetails = subHeaderDetails ?? subHeader.AddEmptySubSectionHeader("Details", subOrder);
                summarizeExposureByRoute(
                    result,
                    data,
                    subHeaderDetails,
                    subOrder++
                );
            }
            // Exposures by substance
            if (data.ActiveSubstances.Count > 1
                && (result.AggregateIndividualExposures != null || result.AggregateIndividualDayExposures != null)
                && outputSettings.ShouldSummarize(TargetExposuresSections.ExposuresBySubstanceSection)
            ) {
                subHeaderDetails = subHeaderDetails ?? subHeader.AddEmptySubSectionHeader("Details", subOrder);
                summarizeExposureBySubstance(
                    result,
                    data,
                    subHeaderDetails,
                    subOrder++
                 );
            }
            // Exposures by route and substance
            if (_configuration.ExposureSources.Count > 1
                && data.ActiveSubstances.Count > 1
                && (result.AggregateIndividualExposures != null || result.AggregateIndividualDayExposures != null)
                && outputSettings.ShouldSummarize(TargetExposuresSections.ExposuresByRouteSubstanceSection)
            ) {
                subHeaderDetails = subHeaderDetails ?? subHeader.AddEmptySubSectionHeader("Details", subOrder);
                summarizeExposureByRouteSubstance(
                    result,
                    data,
                    subHeaderDetails,
                    subOrder++
               );
            }

            // Kinetic conversion factors
            if (_configuration.ExposureSources.Count > 1
                && result.KineticConversionFactors != null
                && outputSettings.ShouldSummarize(TargetExposuresSections.KineticConversionFactorsSection)
            ) {
                subHeaderDetails = subHeaderDetails ?? subHeader.AddEmptySubSectionHeader("Details", subOrder);
                summarizeKineticConversionFactors(
                    result,
                    data,
                    subHeaderDetails,
                    subOrder++
               );
            }

            // TODO: Reason for second criterion?
            if (result.ExternalExposureCollections.Count > 0 &&
                data.CorrectedRelativePotencyFactors != null &&
                outputSettings.ShouldSummarize(TargetExposuresSections.ExposuresBySourceSection)
            ) {
                subHeaderDetails = subHeaderDetails ?? subHeader.AddEmptySubSectionHeader("Details", subOrder);
                summarizeExposureBySource(
                    result,
                    data,
                    subHeaderDetails,
                    subOrder++
               );
            }

            if (result.KineticModelCalculators?.Values.Any(r => r is DesolvePbkModelCalculator || r is SbmlPbkModelCalculator) ?? false) {
                subHeaderDetails = subHeaderDetails ?? subHeader.AddEmptySubSectionHeader("Details", subOrder);
                summarizeKineticModellingResults(
                    result,
                    data,
                    subHeaderDetails,
                    subOrder++
                );
            }

            if (_configuration.McrAnalysis
                && result.ExposureMatrix != null
                && outputSettings.ShouldSummarize(TargetExposuresSections.McrCoExposureSection)
            ) {
                subHeaderDetails = subHeaderDetails ?? subHeader.AddEmptySubSectionHeader("Details", subOrder);
                summarizeMCRSection(
                    result,
                    subHeaderDetails,
                    subOrder++
                );
            }
            subHeader.SaveSummarySection(outputSummary);
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
            var section = new ChronicAggregateSection();
            var subHeader = header.AddSubSectionHeaderFor(section, $"Exposure distribution", subOrder++);
            section.Summarize(
                subHeader,
                result.AggregateIndividualExposures,
                data.ActiveSubstances,
                data.CorrectedRelativePotencyFactors,
                data.MembershipProbabilities,
                result.KineticConversionFactors,
                data.ExposureRoutes,
                data.ExternalExposureUnit,
                data.TargetExposureUnit,
                data.ReferenceSubstance,
                _configuration.ExposureMethod,
                [.. _configuration.ExposureLevels],
                [.. _configuration.SelectedPercentiles],
                _configuration.VariabilityUpperTailPercentage,
                _configuration.ExposureSources.Count > 1
            );
            subHeader.SaveSummarySection(section);

            if (_configuration.StoreIndividualDayIntakes
                && !_configuration.SkipPrivacySensitiveOutputs
            ) {
                var individualDaysection = new IndividualCompoundIntakeSection();
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
            var section = new AggregateIntakeDistributionSection();
            var subHeader = header.AddSubSectionHeaderFor(section, "Exposures (daily intakes)", subOrder);
            section.Summarize(
                subHeader,
                result.AggregateIndividualDayExposures,
                data.ActiveSubstances,
                data.CorrectedRelativePotencyFactors,
                data.MembershipProbabilities,
                result.KineticConversionFactors,
                data.ExposureRoutes,
                result.ExternalExposureUnit,
                result.TargetExposureUnit,
                data.ReferenceSubstance,
                _configuration.ExposureMethod,
                _configuration.ExposureLevels.ToArray(),
                _configuration.SelectedPercentiles.ToArray(),
                _configuration.VariabilityUpperTailPercentage,
                _configuration.UncertaintyLowerBound,
                _configuration.UncertaintyUpperBound
            );
            subHeader.SaveSummarySection(section);

            if (_configuration.StoreIndividualDayIntakes
                && !_configuration.SkipPrivacySensitiveOutputs
            ) {
                var individualDaySection = new IndividualDayCompoundIntakeSection();
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
            var exposureRoutes = data.ExposureRoutes;
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
                    subHeader = header.GetSubSectionHeader<AggregateIntakeDistributionSection>();
                    if (subHeader != null) {
                        var section = subHeader.GetSummarySection() as AggregateIntakeDistributionSection;
                        section.SummarizeUncertainty(
                            subHeader,
                            aggregateExposures,
                            activeSubstances,
                            relativePotencyFactors,
                            membershipProbabilities,
                            actionResult.TargetExposureUnit,
                            uncertaintyLowerBound,
                            uncertaintyUpperBound
                        );
                        subHeader.SaveSummarySection(section);
                    }
                } else {
                    subHeader = header.GetSubSectionHeader<ChronicAggregateSection>();
                    if (subHeader != null) {
                        var section = subHeader.GetSummarySection() as ChronicAggregateSection;
                        //TODO this is not very nice
                        section.SummarizeUncertainty(
                            subHeader,
                            aggregateExposures,
                            activeSubstances,
                            relativePotencyFactors,
                            membershipProbabilities,
                            actionResult.TargetExposureUnit,
                            uncertaintyLowerBound,
                            uncertaintyUpperBound
                        );
                        subHeader.SaveSummarySection(section);
                    }
                }
                if (subHeader != null) {

                }
            }

            if (actionResult.KineticModelCalculators?.Values.Any(r => r is DesolvePbkModelCalculator) ?? false) {
                foreach (var substance in activeSubstances) {
                    if (actionResult.KineticModelCalculators[substance] is DesolvePbkModelCalculator) {
                        subHeader = header.GetSubSectionHeaderFromTitleString<InternalVersusExternalExposuresSection>(substance.Name);
                        if (subHeader != null) {
                            var section = subHeader.GetSummarySection() as InternalVersusExternalExposuresSection;
                            var kineticModelInstance = kineticModelInstances.Single(c => c.IsHumanModel && c.Substances.Contains(substance));
                            section.SummarizeUncertainty(
                                actionResult.KineticConversionFactors,
                                substance,
                                exposureRoutes
                            );
                            subHeader.SaveSummarySection(section);
                        }
                    } else {
                        // TODO: summarize linear aggregation results
                    }
                }
            }

            // Exposures by route
            if (isAggregate) {
                subHeader = header.GetSubSectionHeader<TargetExposuresSummarySection>();
                if (subHeader != null) {
                    subSubHeader = subHeader.GetSubSectionHeader<TotalDistributionAggregateRouteSection>();
                    if (subSubHeader != null) {
                        var section = subSubHeader.GetSummarySection() as TotalDistributionAggregateRouteSection;
                        section.SummarizeUncertainty(
                            actionResult.AggregateIndividualExposures,
                            actionResult.AggregateIndividualDayExposures,
                            activeSubstances,
                            relativePotencyFactors,
                            membershipProbabilities,
                            actionResult.KineticConversionFactors,
                            exposureRoutes,
                            actionResult.ExternalExposureUnit
                        );
                        subSubHeader.SaveSummarySection(section);
                    }
                    subSubHeader = subHeader.GetSubSectionHeader<UpperDistributionAggregateRouteSection>();
                    if (subSubHeader != null) {
                        var section = subSubHeader.GetSummarySection() as UpperDistributionAggregateRouteSection;
                        section.SummarizeUncertainty(
                            actionResult.AggregateIndividualExposures,
                            actionResult.AggregateIndividualDayExposures,
                            activeSubstances,
                            relativePotencyFactors,
                            membershipProbabilities,
                            actionResult.KineticConversionFactors,
                            exposureRoutes,
                            actionResult.ExternalExposureUnit,
                            actionResult.TargetExposureUnit,
                            percentageForUpperTail
                        );
                        subSubHeader.SaveSummarySection(section);
                    }
                }
            }
            // Exposures by substance
            if (isAggregate) {
                subHeader = header.GetSubSectionHeader<TargetExposuresSummarySection>();
                if (subHeader != null) {
                    subSubHeader = subHeader.GetSubSectionHeader<TotalDistributionRouteCompoundSection>();
                    if (subSubHeader != null) {
                        var section = subSubHeader.GetSummarySection() as TotalDistributionRouteCompoundSection;
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
                    subSubHeader = subHeader.GetSubSectionHeader<UpperDistributionRouteCompoundSection>();
                    if (subSubHeader != null) {
                        var section = subSubHeader.GetSummarySection() as UpperDistributionRouteCompoundSection;
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

            if (isAggregate) {
                subHeader = header.GetSubSectionHeader<NonDietaryIntakeDistributionSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as NonDietaryIntakeDistributionSection;
                    section.SummarizeUncertainty(
                        subHeader,
                        actionResult.NonDietaryIndividualDayIntakes,
                        relativePotencyFactors,
                        membershipProbabilities,
                        uncertaintyLowerBound,
                        uncertaintyUpperBound,
                        isPerPerson
                    );
                    subHeader.SaveSummarySection(section);
                }
            }

            //Nondietary route
            if (isAggregate) {
                subHeader = header.GetSubSectionHeader<NonDietaryTotalDistributionRouteSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as NonDietaryTotalDistributionRouteSection;
                    section.SummarizeUncertainty(
                        actionResult.NonDietaryIndividualDayIntakes,
                        relativePotencyFactors,
                        membershipProbabilities,
                        nonDietaryExposureRoutes,
                        exposureType,
                        isPerPerson
                    );
                    subHeader.SaveSummarySection(section);
                }

                subHeader = header.GetSubSectionHeader<NonDietaryUpperDistributionRouteSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as NonDietaryUpperDistributionRouteSection;
                    section.SummarizeUncertainty(
                        actionResult.NonDietaryIndividualDayIntakes,
                        relativePotencyFactors,
                        membershipProbabilities,
                        nonDietaryExposureRoutes,
                        exposureType,
                        percentageForUpperTail,
                        isPerPerson
                    );
                    subHeader.SaveSummarySection(section);
                }
            }

            // Non-dietary route x substance
            if (isAggregate) {
                subHeader = header.GetSubSectionHeader<NonDietaryTotalDistributionRouteCompoundSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as NonDietaryTotalDistributionRouteCompoundSection;
                    section.SummarizeUncertainty(
                        activeSubstances,
                        actionResult.NonDietaryIndividualDayIntakes,
                        relativePotencyFactors,
                        membershipProbabilities,
                        nonDietaryExposureRoutes,
                        exposureType,
                        isPerPerson
                    );
                    subHeader.SaveSummarySection(section);
                }
                subHeader = header.GetSubSectionHeader<NonDietaryUpperDistributionRouteCompoundSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as NonDietaryUpperDistributionRouteCompoundSection;
                    section.SummarizeUncertainty(
                        activeSubstances,
                        actionResult.NonDietaryIndividualDayIntakes,
                        relativePotencyFactors,
                        membershipProbabilities,
                        nonDietaryExposureRoutes,
                        exposureType,
                        percentageForUpperTail,
                        isPerPerson
                    );
                    subHeader.SaveSummarySection(section);
                }
            }
            if (activeSubstances.Count > 1) {
                subHeader = header.GetSubSectionHeader<TargetExposuresSummarySection>();
                if (subHeader != null) {
                    subHeader1 = subHeader.GetSubSectionHeader<TotalDistributionCompoundSection>();
                    if (subHeader1 != null) {
                        var section = subHeader1.GetSummarySection() as TotalDistributionCompoundSection;
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
                    subHeader1 = subHeader.GetSubSectionHeader<UpperDistributionCompoundSection>();
                    if (subHeader1 != null) {
                        var section = subHeader1.GetSummarySection() as UpperDistributionCompoundSection;
                        section.SummarizeUncertainty(
                            actionResult.AggregateIndividualExposures,
                            actionResult.AggregateIndividualDayExposures,
                            relativePotencyFactors,
                            membershipProbabilities,
                            actionResult.KineticConversionFactors,
                            activeSubstances,
                            percentageForUpperTail,
                            actionResult.ExternalExposureUnit,
                            actionResult.TargetExposureUnit
                        );
                        subHeader1.SaveSummarySection(section);
                    }
                }
            }
            header.SaveSummarySection(outputSummary);
        }

        /// <summary>
        /// Summarize exposure by substance
        /// </summary>
        /// <param name="project"></param>
        /// <param name="result"></param>
        /// <param name="data"></param>
        /// <param name="header"></param>
        /// <param name="order"></param>
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
            var subOrder = order + 1;

            var totalSection = new TotalDistributionCompoundSection();
            var sub2Header = subHeader.AddSubSectionHeaderFor(totalSection, "Total distribution", subOrder++);
            totalSection.Summarize(
                result.AggregateIndividualExposures,
                result.AggregateIndividualDayExposures,
                data.CorrectedRelativePotencyFactors,
                data.MembershipProbabilities,
                result.KineticConversionFactors,
                data.ActiveSubstances,
                _configuration.VariabilityLowerPercentage,
                _configuration.VariabilityUpperPercentage,
                _configuration.UncertaintyLowerBound,
                _configuration.UncertaintyUpperBound,
                result.ExternalExposureUnit,
                data.TargetExposureUnit
            );
            sub2Header.SaveSummarySection(totalSection);

            var boxPlotSection = new TargetExposuresBySubstanceSection();
            sub2Header = subHeader.AddSubSectionHeaderFor(boxPlotSection, "Total distribution (boxplots)", subOrder++);
            boxPlotSection.Summarize(
                result.AggregateIndividualExposures,
                result.AggregateIndividualDayExposures,
                result.KineticConversionFactors,
                data.ActiveSubstances,
                totalSection.Records.Select(c => c.CompoundCode).ToList(),
                result.ExternalExposureUnit,
                data.TargetExposureUnit
            );
            sub2Header.SaveSummarySection(boxPlotSection);

            if (data.CorrectedRelativePotencyFactors != null) {
                //No upper because upper tail is based on RPF corrected exposures, so can not be determined
                var upperSection = new UpperDistributionCompoundSection();
                sub2Header = subHeader.AddSubSectionHeaderFor(upperSection, "Upper tail distribution", subOrder++);
                upperSection.Summarize(
                    result.AggregateIndividualExposures,
                    result.AggregateIndividualDayExposures,
                    data.ActiveSubstances,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    result.KineticConversionFactors,
                    _configuration.VariabilityUpperTailPercentage,
                    _configuration.VariabilityLowerPercentage,
                    _configuration.VariabilityUpperPercentage,
                    _configuration.UncertaintyLowerBound,
                    _configuration.UncertaintyUpperBound,
                    result.ExternalExposureUnit,
                    result.TargetExposureUnit
                );
                sub2Header.SaveSummarySection(upperSection);
            }
            if (_configuration.ExposureSources.Count > 1) {
                var section = new CoExposureTotalDistributionSection();
                sub2Header = subHeader.AddSubSectionHeaderFor(section, "Co-exposure total distribution", subOrder++);
                section.Summarize(
                    result.AggregateIndividualExposures,
                    result.AggregateIndividualDayExposures,
                    data.ActiveSubstances,
                    result.TargetExposureUnit
                );
                sub2Header.SaveSummarySection(section);
            }
            if (_configuration.ExposureSources.Count > 1 &&
                data.CorrectedRelativePotencyFactors != null) {
                var section = new CoExposureUpperDistributionSection();
                sub2Header = subHeader.AddSubSectionHeaderFor(section, "Co-exposure upper tail", subOrder++);
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
                sub2Header.SaveSummarySection(section);
            }

            if (_configuration.StoreIndividualDayIntakes
                && !_configuration.SkipPrivacySensitiveOutputs
            ) {
                if (result.AggregateIndividualExposures != null) {
                    var section = new IndividualCompoundIntakeSection();
                    sub2Header = subHeader.AddSubSectionHeaderFor(section, "Individual exposures by substance", subOrder++);
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
                    var section = new IndividualDayCompoundIntakeSection();
                    sub2Header = subHeader.AddSubSectionHeaderFor(section, "Individual day exposures by substance", subOrder++);
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
        /// Summarize ExposureByRouteCompound
        /// </summary>
        /// <param name="result"></param>
        /// <param name="data"></param>
        /// <param name="header"></param>
        /// <param name="order"></param>
        private void summarizeKineticConversionFactors(
            TargetExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var section = new KineticConversionFactorSection() {
                SectionLabel = getSectionLabel(TargetExposuresSections.KineticConversionFactorsSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(section, "Kinetic conversion factors by route and substance", order++);
            section.Summarize(
                result.KineticConversionFactors,
                _configuration.UncertaintyLowerBound,
                _configuration.UncertaintyUpperBound

            );
            subHeader.SaveSummarySection(section);
        }

        /// <summary>
        /// Summarize ExposureByRouteSubstance
        /// </summary>
        /// <param name="result"></param>
        /// <param name="data"></param>
        /// <param name="header"></param>
        /// <param name="order"></param>
        private void summarizeExposureByRouteSubstance(
            TargetExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var headSection = new ExposureByRouteCompoundSection() {
                SectionLabel = getSectionLabel(TargetExposuresSections.ExposuresByRouteSubstanceSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(headSection, "Exposures by route and substance", order++);
            {
                var section = new TotalDistributionRouteCompoundSection();
                var sub2Header = subHeader.AddSubSectionHeaderFor(section, "Total distribution", order++);
                section.Summarize(
                    result.AggregateIndividualExposures,
                    result.AggregateIndividualDayExposures,
                    data.ActiveSubstances,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    result.KineticConversionFactors,
                    _configuration.VariabilityLowerPercentage,
                    _configuration.VariabilityUpperPercentage,
                    _configuration.UncertaintyLowerBound,
                    _configuration.UncertaintyUpperBound,
                    result.ExternalExposureUnit
                );
                sub2Header.SaveSummarySection(section);
            }

            if (data.ActiveSubstances.Count == 1
                || (data.MembershipProbabilities != null && data.CorrectedRelativePotencyFactors != null)
            ) {
                var section = new UpperDistributionRouteCompoundSection();
                var sub2Header = subHeader.AddSubSectionHeaderFor(section, "Upper tail distribution", order++);
                section.Summarize(
                    result.AggregateIndividualExposures,
                    result.AggregateIndividualDayExposures,
                    data.ActiveSubstances,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    result.KineticConversionFactors,
                    _configuration.VariabilityLowerPercentage,
                    _configuration.VariabilityUpperPercentage,
                    _configuration.VariabilityUpperTailPercentage,
                    _configuration.UncertaintyLowerBound,
                    _configuration.UncertaintyUpperBound,
                    result.TargetExposureUnit,
                    result.ExternalExposureUnit
               );
                sub2Header.SaveSummarySection(section);
            }
            subHeader.SaveSummarySection(headSection);
        }

        /// <summary>
        /// Summarize KineticModellingResults
        /// </summary>
        private void summarizeKineticModellingResults(
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

            var drilldownIndividualIds = KineticModelTimeCourseSection
                 .GetDrilldownIndividualIds(
                    allTargetExposures,
                    data.ActiveSubstances,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    _configuration.VariabilityDrilldownPercentage,
                    result.TargetExposureUnit
                );

            var selectedTargetExposures = drilldownIndividualIds
                .Select(r => allTargetExposures.First(c => c.SimulatedIndividualId == r))
                .ToList();

            if (substances.Count == 1) {
                // Single compound
                var substance = substances.First();
                if (result.KineticModelCalculators[substance] is PbkModelCalculatorBase) {
                    summarizeCompoundKineticModel(
                        result,
                        data.ExposureRoutes,
                        data.KineticModelInstances.Single(c => c.IsHumanModel && c.Substances.Contains(substance)),
                        data.TargetExposureUnit,
                        substance,
                        allTargetExposures,
                        selectedTargetExposures,
                        "Kinetic model",
                        _configuration.UncertaintyLowerBound,
                        _configuration.UncertaintyUpperBound,
                        _configuration.ExposureType,
                        header,
                        order
                    );
                }
            } else {
                // Multiple compounds
                var subHeader = header.AddEmptySubSectionHeader("Kinetic models", order++);
                var subOrder = 0;
                var pbkModelSubstances = new List<Compound>();
                foreach (var calculator in result.KineticModelCalculators.Values) {
                    var instance = (calculator is PbkModelCalculatorBase pbkCalculator)
                        ? pbkCalculator.KineticModelInstance : null;
                    foreach (var outputSubstance in calculator.OutputSubstances) {
                        summarizeCompoundKineticModel(
                            result,
                            data.ExposureRoutes,
                            instance,
                            data.TargetExposureUnit,
                            outputSubstance,
                            allTargetExposures,
                            selectedTargetExposures,
                            $"PBPK model {outputSubstance.Name}",
                            _configuration.UncertaintyLowerBound,
                            _configuration.UncertaintyUpperBound,
                            _configuration.ExposureType,
                            subHeader,
                            subOrder++
                        );
                        pbkModelSubstances.Add(outputSubstance);
                    }
                }
                var linearModelCompounds = substances.Except(pbkModelSubstances).ToList();

                if (linearModelCompounds.Any()) {
                    var section = new LinearModelSection();
                    var subHeader2 = subHeader.AddSubSectionHeaderFor(section, $"Absorption factor models", subOrder++);
                    section.Summarize(linearModelCompounds);
                    subHeader2.SaveSummarySection(section);
                }
            }
        }

        private static void summarizeCompoundKineticModel(
            TargetExposuresActionResult actionResult,
            ICollection<ExposurePathType> exposureRoutes,
            KineticModelInstance kineticModelInstance,
            TargetUnit targetUnit,
            Compound substance,
            ICollection<AggregateIndividualExposure> allTargetExposures,
            ICollection<AggregateIndividualExposure> selectedTargetExposures,
            string subTitle,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            ExposureType exposureType,
            SectionHeader header,
            int order
        ) {
            var targetUnits = new List<TargetUnit>() { targetUnit };
            if (kineticModelInstance != null) {
                var section = new KineticModelSection();
                var subHeader = header.AddSubSectionHeaderFor(section, subTitle, order);
                section.Summarize(
                    substance,
                    kineticModelInstance,
                    exposureRoutes,
                    targetUnits
                );
                subHeader.SaveSummarySection(section);

                var subOrder = 0;
                summarizeInternalVersusExternalExposures(
                    actionResult,
                    exposureRoutes,
                    substance,
                    allTargetExposures,
                    uncertaintyLowerBound,
                    uncertaintyUpperBound,
                    exposureType,
                    targetUnits,
                    subHeader,
                    subOrder++
                );

                if (selectedTargetExposures != null) {
                    summarizeKineticModelTimeCourse(
                        exposureRoutes,
                        kineticModelInstance,
                        substance,
                        selectedTargetExposures,
                        exposureType,
                        targetUnits,
                        actionResult.ExternalExposureUnit,
                        subHeader,
                        subOrder++
                    );
                }
            }
        }

        private static void summarizeInternalVersusExternalExposures(
            TargetExposuresActionResult actionResult,
            ICollection<ExposurePathType> exposureRoutes,
            Compound substance,
            ICollection<AggregateIndividualExposure> allTargetExposures,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            ExposureType exposureType,
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
                exposureRoutes,
                allTargetExposures,
                actionResult.KineticConversionFactors,
                targetUnits,
                exposureType,
                actionResult.ExternalExposureUnit,
                uncertaintyLowerBound,
                uncertaintyUpperBound
            );
            subHeader.SaveSummarySection(section);
        }

        /// <summary>
        /// Summarize KineticModelTimeCourse
        /// </summary>
        private static void summarizeKineticModelTimeCourse(
            ICollection<ExposurePathType> exposureRoutes,
            KineticModelInstance kineticModelInstance,
            Compound substance,
            ICollection<AggregateIndividualExposure> selectedTargetExposures,
            ExposureType exposureType,
            ICollection<TargetUnit> targetUnits,
            ExposureUnitTriple externalExposureUnit,
            SectionHeader header,
            int subOrder
        ) {
            var section = new KineticModelTimeCourseSection();
            var subHeader = header.AddSubSectionHeaderFor(
                section: section,
                title: $"Individual drilldown PBPK model {substance.Name}",
                order: subOrder++
            );
            section.Summarize(
                selectedTargetExposures,
                exposureRoutes.Select(r => r.GetExposureRoute()).ToList(),
                substance,
                kineticModelInstance,
                targetUnits,
                externalExposureUnit,
                exposureType
            );
            subHeader.SaveSummarySection(section);
        }

        /// <summary>
        /// Summarize exposure by route
        /// </summary>
        /// <param name="result"></param>
        /// <param name="data"></param>
        /// <param name="header"></param>
        /// <param name="order"></param>
        private void summarizeExposureByRoute(
            TargetExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var headSection = new ExposureByRouteSection() {
                SectionLabel = getSectionLabel(TargetExposuresSections.ExposuresByRouteSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(headSection, "Exposures by route", order++);
            {
                var section = new TotalDistributionAggregateRouteSection();
                var sub2Header = subHeader.AddSubSectionHeaderFor(section, "Total distribution", 1);
                section.Summarize(
                    result.AggregateIndividualExposures,
                    result.AggregateIndividualDayExposures,
                    data.ExposureRoutes,
                    data.ActiveSubstances,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    result.KineticConversionFactors,
                    _configuration.VariabilityLowerPercentage,
                    _configuration.VariabilityUpperPercentage,
                    _configuration.UncertaintyLowerBound,
                    _configuration.UncertaintyUpperBound,
                    result.TargetExposureUnit,
                    result.ExternalExposureUnit
                );
                sub2Header.SaveSummarySection(section);
            }

            {
                var section = new UpperDistributionAggregateRouteSection();
                var sub2Header = subHeader.AddSubSectionHeaderFor(section, "Upper tail distribution", 2);
                section.Summarize(
                    result.AggregateIndividualExposures,
                    result.AggregateIndividualDayExposures,
                    data.ActiveSubstances,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    result.KineticConversionFactors,
                    data.ExposureRoutes,
                    _configuration.VariabilityLowerPercentage,
                    _configuration.VariabilityUpperPercentage,
                    _configuration.VariabilityUpperTailPercentage,
                    _configuration.UncertaintyLowerBound,
                    _configuration.UncertaintyUpperBound,
                    result.TargetExposureUnit,
                    result.ExternalExposureUnit
                );
                sub2Header.SaveSummarySection(section);
            }
            subHeader.SaveSummarySection(headSection);
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
            foreach (var externalExposureCollection in result.ExternalExposureCollections) {
                var subHeader = header.AddEmptySubSectionHeader($"{externalExposureCollection.ExposureSource.GetShortDisplayName()}", order++);

                // Exposures by source
                summarizeExternalSourceExposure(
                    subHeader,
                    externalExposureCollection,
                    data,
                    ref order
                );

                // Exposures by source by route
                summarizeExternalSourceExposureByRoute(
                    subHeader,
                    externalExposureCollection,
                    data,
                    ref order
                );

                // Exposures by source by route and substance
                summarizeExternalSourceExposureByRouteSubstance(
                    subHeader,
                    externalExposureCollection,
                    data,
                    ref order
                );
            }
        }

        public void summarizeExternalSourceExposure(
            SectionHeader header,
            ExternalExposureCollection externalExposureCollection,
            ActionData data,
            ref int order
        ) {
            var section = new ExternalExposureDistributionSection() {
                SectionLabel = getSectionLabel(TargetExposuresSections.ExposuresBySourceSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                $"{externalExposureCollection.ExposureSource.GetShortDisplayName()} distribution (daily exposure)",
                order++
            );
            subHeader.SaveSummarySection(section);

            var externalIndividualDayExposures = externalExposureCollection.ExternalIndividualDayExposures;

            var externalExposures = externalIndividualDayExposures
                .Select(c => c.GetTotalExternalExposure(
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    _configuration.IsPerPerson)
                )
                .ToList();

            var exposureLevels = ExposureLevelsCalculator.GetExposureLevels(
                externalExposures,
                _configuration.ExposureMethod,
                [.. _configuration.ExposureLevels]);

            var coExposures = externalIndividualDayExposures
                .AsParallel()
                .Select(idi => (
                    idi.SimulatedIndividualDayId,
                    IntakesPerCompound: idi.GetTotalExposurePerCompound().Count(g => g.Amount > 0)
                ))
                .Where(ipc => ipc.IntakesPerCompound > 1)
                .Select(c => c.SimulatedIndividualDayId)
                .ToHashSet();
            var totalExposureDistributionSection = new ExternalTotalExposureDistributionSection();
            var subSubHeader = subHeader.AddSubSectionHeaderFor(totalExposureDistributionSection, "Graph total", 1);
            totalExposureDistributionSection.Summarize(
                coExposures,
                externalIndividualDayExposures,
                data.CorrectedRelativePotencyFactors,
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
                data.CorrectedRelativePotencyFactors,
                data.MembershipProbabilities,
                _configuration.VariabilityUpperPercentage,
                _configuration.IsPerPerson,
                _configuration.UncertaintyLowerBound,
                _configuration.UncertaintyUpperBound
           );
            subSubHeader.SaveSummarySection(upperExposureDistributionSection);

            var weights = externalIndividualDayExposures
                .Select(c => c.IndividualSamplingWeight)
                .ToList();

            var percentileSection = new IntakePercentileSection();
            subSubHeader = subHeader.AddSubSectionHeaderFor(percentileSection, "Percentiles", 3);
            percentileSection.Summarize(externalExposures, weights, data.ReferenceSubstance, [.. _configuration.SelectedPercentiles]);
            subSubHeader.SaveSummarySection(percentileSection);

            var percentageSection = new IntakePercentageSection();
            subSubHeader = subHeader.AddSubSectionHeaderFor(percentageSection, "Percentages", 4);
            percentageSection.Summarize(externalExposures, weights, data.ReferenceSubstance, exposureLevels);
            subSubHeader.SaveSummarySection(percentageSection);
        }

        public void summarizeExternalSourceExposureByRoute(
            SectionHeader header,
            ExternalExposureCollection externalExposureCollection,
            ActionData data,
            ref int order
        ) {
            var subHeader = header.AddEmptySubSectionHeader("Exposures by route", order++);

            var totalSection = new ExternalExposureTotalDistributionRouteSection();
            var subSubHeader = subHeader.AddSubSectionHeaderFor(totalSection, "Total distribution", 1);
            totalSection.Summarize(
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

            var upperSection = new ExternalExposureUpperDistributionRouteSection();
            subSubHeader = subHeader.AddSubSectionHeaderFor(upperSection, "Upper tail distribution", 2);
            upperSection.Summarize(
                externalExposureCollection,
                data.CorrectedRelativePotencyFactors,
                data.MembershipProbabilities,
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
            ref int order
        ) {
            if (data.ActiveSubstances.Count > 1) {
                var subHeader = header.AddEmptySubSectionHeader("Exposures by route and substance", order++);

                var totalSection = new ExternalExposureTotalDistributionRouteSubstanceSection();
                var subSubHeader = subHeader.AddSubSectionHeaderFor(totalSection, "Total distribution", 1);
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
                subSubHeader = subHeader.AddSubSectionHeaderFor(upperSection, "Upper tail distribution", 2);
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

        private List<ActionSummaryUnitRecord> collectUnits(ActionData data) {
            var printOption = _configuration.TargetDoseLevelType == TargetLevelType.External ? TargetUnit.DisplayOption.AppendBiologicalMatrix : TargetUnit.DisplayOption.UnitOnly;
            var individualDayUnit = _configuration.ExposureType == ExposureType.Chronic
                ? "individuals"
                : "individual days";
            var actionSummaryTargetUnitRecord = new ActionSummaryUnitRecord(
                data.TargetExposureUnit.Target.Code,
                data.TargetExposureUnit.Target.ExpressionType == ExpressionType.SpecificGravity
                    ? data.TargetExposureUnit.GetShortDisplayName()
                    : data.TargetExposureUnit.GetShortDisplayName(TargetUnit.DisplayOption.AppendExpressionType)
            );
            var result = new List<ActionSummaryUnitRecord> {
                new("IntakeUnit", data.TargetExposureUnit.GetShortDisplayName(printOption)),
                new("TargetAmountUnit", data.TargetExposureUnit.SubstanceAmountUnit.GetShortDisplayName()),
                new("TargetExposureUnit", data.TargetExposureUnit.GetShortDisplayName()),
                new("BodyWeightUnit", data.BodyWeightUnit.GetShortDisplayName()),
                new("ExternalExposureUnit", data.ExternalExposureUnit.GetShortDisplayName()),
                new("IndividualDayUnit", individualDayUnit),
                new("LowerPercentage", $"p{_configuration.VariabilityLowerPercentage}"),
                new("UpperPercentage", $"p{_configuration.VariabilityUpperPercentage}"),
                new("ConcentrationUnit", data.ConcentrationUnit.GetShortDisplayName()),
                new("ConsumptionUnit", data.ConsumptionUnit.GetShortDisplayName()),
                new("LowerBound", $"p{_configuration.UncertaintyLowerBound}"),
                new("UpperBound", $"p{_configuration.UncertaintyUpperBound}"),
                actionSummaryTargetUnitRecord
            };
            return result;
        }
    }
}

