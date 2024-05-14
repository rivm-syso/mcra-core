using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.ExposureLevelsCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.DesolvePbkModelCalculators;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.SbmlModelCalculation;
using MCRA.Simulation.Calculators.NonDietaryIntakeCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.TargetExposures {

    public enum TargetExposuresSections {
        DetailsSection,
        ExposuresByRouteSection,
        ExposuresBySubstanceSection,
        ExposuresByRouteSubstanceSection,
        KineticConversionFactorsSection,
        McrCoExposureSection
    }
    public class TargetExposuresSummarizer : ActionResultsSummarizerBase<TargetExposuresActionResult> {
        public override ActionType ActionType => ActionType.TargetExposures;
        public override void Summarize(
            ProjectDto project,
            TargetExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var outputSettings = new ModuleOutputSectionsManager<TargetExposuresSections>(project, ActionType);

            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var outputSummary = new TargetExposuresSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(outputSummary, ActionType.GetDisplayName(), order);
            subHeader.Units = collectUnits(project, data);
            subHeader.SaveSummarySection(outputSummary);
            int subOrder = 0;

            // Summarize acute
            if (result.AggregateIndividualDayExposures != null
                && (data.ActiveSubstances.Count == 1 || data.CorrectedRelativePotencyFactors != null)
            ) {
                summarizeDailyExposures(
                    project,
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
                    project,
                    result,
                    data,
                    subHeader,
                    subOrder
                );
            }

            SectionHeader subHeaderDetails = null;
            //Exposures by route
            if (project.AssessmentSettings.Aggregate
                && (data.ActiveSubstances.Count == 1 || data.CorrectedRelativePotencyFactors != null)
                && outputSettings.ShouldSummarize(TargetExposuresSections.ExposuresByRouteSection)
            ) {
                subHeaderDetails = subHeaderDetails ?? subHeader.AddEmptySubSectionHeader("Details", subOrder);
                summarizeExposureByRoute(
                    project,
                    result,
                    data,
                    subHeaderDetails,
                    subOrder++
                );
            }
            //Exposures by substance
            if (data.ActiveSubstances.Count > 1
                && (result.AggregateIndividualExposures != null || result.AggregateIndividualDayExposures != null)
                && outputSettings.ShouldSummarize(TargetExposuresSections.ExposuresBySubstanceSection)
            ) {
                subHeaderDetails = subHeaderDetails ?? subHeader.AddEmptySubSectionHeader("Details", subOrder);
                summarizeExposureBySubstance(
                    project,
                    result,
                    data,
                    subHeaderDetails,
                    subOrder++
                 );
            }
            //Exposures by route and substance
            if (project.AssessmentSettings.Aggregate
                && data.ActiveSubstances.Count > 1
                && (result.AggregateIndividualExposures != null || result.AggregateIndividualDayExposures != null)
                && outputSettings.ShouldSummarize(TargetExposuresSections.ExposuresByRouteSubstanceSection)
            ) {
                subHeaderDetails = subHeaderDetails ?? subHeader.AddEmptySubSectionHeader("Details", subOrder);
                summarizeExposureByRouteSubstance(
                    project,
                    result,
                    data,
                    subHeaderDetails,
                    subOrder++
               );
            }

            //Kinetic conversion factors
            if (project.AssessmentSettings.Aggregate
                && result.KineticConversionFactors != null
                && outputSettings.ShouldSummarize(TargetExposuresSections.KineticConversionFactorsSection)
            ) {
                subHeaderDetails = subHeaderDetails ?? subHeader.AddEmptySubSectionHeader("Details", subOrder);
                summarizeKineticConversionFactors(
                    project,
                    result,
                    data,
                    subHeaderDetails,
                    subOrder++
               );
            }

            if (project.AssessmentSettings.Aggregate && data.CorrectedRelativePotencyFactors != null) {
                subHeaderDetails = subHeaderDetails ?? subHeader.AddEmptySubSectionHeader("Details", subOrder);
                var section = new NonDietaryExposureSection();
                section.CollectUnits(collectUnits(project, data), data);
                var sub2Header = subHeaderDetails.AddSubSectionHeaderFor(section, "Non-dietary exposures", subOrder++);
                summarizeNonDietaryExposures(
                    project,
                    result,
                    data,
                    sub2Header,
                    subOrder++
                );
                sub2Header.SaveSummarySection(section);
            }

            if (result.KineticModelCalculators?.Values.Any(r => r is DesolvePbkModelCalculator || r is SbmlPbkModelCalculator) ?? false) {
                subHeaderDetails = subHeaderDetails ?? subHeader.AddEmptySubSectionHeader("Details", subOrder);
                summarizeKineticModellingResults(
                    project,
                    result,
                    data,
                    subHeaderDetails,
                    subOrder++
                );
            }

            if (project.EffectSettings.AnalyseMcr
                && result.ExposureMatrix != null
                && outputSettings.ShouldSummarize(TargetExposuresSections.McrCoExposureSection)
            ) {
                subHeaderDetails = subHeaderDetails ?? subHeader.AddEmptySubSectionHeader("Details", subOrder);
                summarizeMCRSection(
                    project,
                    result,
                    subHeaderDetails,
                    subOrder++
                );
            }
            subHeader.SaveSummarySection(outputSummary);
        }

        private void summarizeMCRSection(
            ProjectDto project,
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
                project.EffectSettings.ExposureApproachType,
                project.OutputDetailSettings.MaximumCumulativeRatioCutOff,
                project.OutputDetailSettings.MaximumCumulativeRatioPercentiles,
                project.MixtureSelectionSettings.TotalExposureCutOff,
                project.OutputDetailSettings.MaximumCumulativeRatioMinimumPercentage,
                project.OutputDetailSettings.SkipPrivacySensitiveOutputs
            );

            section.Summarize(
                result.ExposureMatrix,
                project.OutputDetailSettings.MaximumCumulativeRatioPercentiles,
                project.OutputDetailSettings.MaximumCumulativeRatioMinimumPercentage
            );
            subSubHeader.SaveSummarySection(section);
        }

        private static void summarizeExposureDistribution(
            ProjectDto project,
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
                project.OutputDetailSettings.ExposureMethod,
                project.OutputDetailSettings.ExposureLevels,
                project.OutputDetailSettings.SelectedPercentiles,
                project.OutputDetailSettings.PercentageForUpperTail,
                project.AssessmentSettings.Aggregate
            );
            subHeader.SaveSummarySection(section);

            if (project.OutputDetailSettings.StoreIndividualDayIntakes
                && !project.OutputDetailSettings.SkipPrivacySensitiveOutputs
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

        private static void summarizeDailyExposures(
            ProjectDto project,
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
                project.OutputDetailSettings.ExposureMethod,
                project.OutputDetailSettings.ExposureLevels,
                project.OutputDetailSettings.SelectedPercentiles,
                project.OutputDetailSettings.PercentageForUpperTail,
                project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                project.UncertaintyAnalysisSettings.UncertaintyUpperBound
            );
            subHeader.SaveSummarySection(section);

            if (project.OutputDetailSettings.StoreIndividualDayIntakes
                && !project.OutputDetailSettings.SkipPrivacySensitiveOutputs
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
            ProjectDto project,
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
            var exposureType = project.AssessmentSettings.ExposureType;
            var uncertaintyLowerBound = project.UncertaintyAnalysisSettings.UncertaintyLowerBound;
            var uncertaintyUpperBound = project.UncertaintyAnalysisSettings.UncertaintyUpperBound;
            var percentageForUpperTail = project.OutputDetailSettings.PercentageForUpperTail;
            var isPerPerson = project.SubsetSettings.IsPerPerson;
            var isAggregate = project.AssessmentSettings.Aggregate;

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
            if (activeSubstances.Count > 1 && relativePotencyFactors != null) {
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
            ProjectDto project,
            TargetExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var headSection = new ExposureByCompoundSection() {
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
                project.OutputDetailSettings.LowerPercentage,
                project.OutputDetailSettings.UpperPercentage,
                project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                result.ExternalExposureUnit
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
                result.ExternalExposureUnit
            );
            sub2Header.SaveSummarySection(boxPlotSection);

            if (data.CorrectedRelativePotencyFactors != null) {
                var upperSection = new UpperDistributionCompoundSection();
                sub2Header = subHeader.AddSubSectionHeaderFor(upperSection, "Upper tail distribution", subOrder++);
                upperSection.Summarize(
                    result.AggregateIndividualExposures,
                    result.AggregateIndividualDayExposures,
                    data.ActiveSubstances,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    result.KineticConversionFactors,
                    project.OutputDetailSettings.PercentageForUpperTail,
                    project.OutputDetailSettings.LowerPercentage,
                    project.OutputDetailSettings.UpperPercentage,
                    project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                    project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                    result.ExternalExposureUnit,
                    result.TargetExposureUnit
                );
                sub2Header.SaveSummarySection(upperSection);
            }
            if (project.AssessmentSettings.Aggregate) {
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
            if (project.AssessmentSettings.Aggregate && data.CorrectedRelativePotencyFactors != null) {
                var section = new CoExposureUpperDistributionSection();
                sub2Header = subHeader.AddSubSectionHeaderFor(section, "Co-exposure upper tail", subOrder++);
                section.Summarize(
                    result.AggregateIndividualExposures,
                    result.AggregateIndividualDayExposures,
                    data.ActiveSubstances,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    result.KineticConversionFactors,
                    project.OutputDetailSettings.PercentageForUpperTail,
                    result.ExternalExposureUnit,
                    result.TargetExposureUnit
                );
                sub2Header.SaveSummarySection(section);
            }

            if (project.OutputDetailSettings.StoreIndividualDayIntakes
                && !project.OutputDetailSettings.SkipPrivacySensitiveOutputs
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
        /// <param name="project"></param>
        /// <param name="result"></param>
        /// <param name="data"></param>
        /// <param name="header"></param>
        /// <param name="order"></param>
        private void summarizeKineticConversionFactors(
            ProjectDto project,
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
                project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                project.UncertaintyAnalysisSettings.UncertaintyUpperBound

            );
            subHeader.SaveSummarySection(section);
        }

        /// <summary>
        /// Summarize ExposureByRouteCompound
        /// </summary>
        /// <param name="project"></param>
        /// <param name="result"></param>
        /// <param name="data"></param>
        /// <param name="header"></param>
        /// <param name="order"></param>
        private void summarizeExposureByRouteSubstance(
        ProjectDto project,
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
                    project.OutputDetailSettings.LowerPercentage,
                    project.OutputDetailSettings.UpperPercentage,
                    project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                    project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
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
                    project.OutputDetailSettings.LowerPercentage,
                    project.OutputDetailSettings.UpperPercentage,
                    project.OutputDetailSettings.PercentageForUpperTail,
                    project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                    project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
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
        private static void summarizeKineticModellingResults(
            ProjectDto project,
            TargetExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var substances = data.ActiveSubstances;

            var allTargetExposures = project.AssessmentSettings.ExposureType == ExposureType.Acute
                ? result.AggregateIndividualDayExposures
                    .Cast<AggregateIndividualExposure>()
                    .ToList()
                : result.AggregateIndividualExposures
                    .ToList();

            var drilldownIndividualIds = KineticModelTimeCourseSection
                 .GetDrilldownIndividualIds(
                    allTargetExposures,
                    data.ActiveSubstances,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    project.OutputDetailSettings.PercentageForDrilldown,
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
                        project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                        project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                        project.AssessmentSettings.ExposureType,
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
                            project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                            project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                            project.AssessmentSettings.ExposureType,
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
        /// Summarize NonDietaryExposures
        /// </summary>
        private static void summarizeNonDietaryExposures(
            ProjectDto project,
            TargetExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            if (result.NonDietaryIndividualDayIntakes != null) {
                var section = new NonDietaryIntakeDistributionSection();
                var sub2Header = header.AddSubSectionHeaderFor(section, "Non-dietary distribution (daily intakes)", order++);
                var intakes = result.NonDietaryIndividualDayIntakes
                    .Select(c => c.ExternalTotalNonDietaryIntakePerMassUnit(
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        project.SubsetSettings.IsPerPerson
                    )).ToList();
                var exposureLevels = ExposureLevelsCalculator.GetExposureLevels(
                    intakes,
                    project.OutputDetailSettings.ExposureMethod,
                    project.OutputDetailSettings.ExposureLevels
                );
                section.Summarize(
                    sub2Header,
                    result.NonDietaryIndividualDayIntakes,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    data.ReferenceSubstance,
                    project.OutputDetailSettings.SelectedPercentiles,
                    exposureLevels,
                    project.OutputDetailSettings.PercentageForUpperTail,
                    project.SubsetSettings.IsPerPerson,
                    project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                    project.UncertaintyAnalysisSettings.UncertaintyUpperBound
                );
                sub2Header.SaveSummarySection(section);
            }
            //Exposures by route
            if (result.NonDietaryIndividualDayIntakes != null) {
                var subHeader = header.AddEmptySubSectionHeader("Exposures by route", order++);
                if (result.NonDietaryIndividualDayIntakes != null) {
                    var section = new NonDietaryTotalDistributionRouteSection();
                    var sub2Header = subHeader.AddSubSectionHeaderFor(section, "Total distribution", 1);
                    section.Summarize(
                        result.NonDietaryIndividualDayIntakes,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        data.NonDietaryExposureRoutes,
                        project.AssessmentSettings.ExposureType,
                        project.OutputDetailSettings.LowerPercentage,
                        project.OutputDetailSettings.UpperPercentage,
                        project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                        project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                        project.SubsetSettings.IsPerPerson
                    );
                    sub2Header.SaveSummarySection(section);
                }

                if (result.NonDietaryIndividualDayIntakes != null) {
                    var section = new NonDietaryUpperDistributionRouteSection();
                    var sub2Header = subHeader.AddSubSectionHeaderFor(section, "Upper tail distribution", 2);
                    section.Summarize(
                        result.NonDietaryIndividualDayIntakes,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        data.NonDietaryExposureRoutes,
                        project.AssessmentSettings.ExposureType,
                        project.OutputDetailSettings.PercentageForUpperTail,
                        project.OutputDetailSettings.LowerPercentage,
                        project.OutputDetailSettings.UpperPercentage,
                        project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                        project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                        project.SubsetSettings.IsPerPerson
                    );
                    sub2Header.SaveSummarySection(section);
                }
            }

            //Exposures by route and substance
            if (result.NonDietaryIndividualDayIntakes != null && data.ActiveSubstances.Count > 1) {
                var subHeader = header.AddEmptySubSectionHeader("Exposures by route and substance", order++);
                if (result.NonDietaryIndividualDayIntakes != null) {
                    var section = new NonDietaryTotalDistributionRouteCompoundSection();
                    var sub2Header = subHeader.AddSubSectionHeaderFor(section, "Total distribution", 1);
                    section.Summarize(
                        data.ActiveSubstances,
                        result.NonDietaryIndividualDayIntakes,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        data.NonDietaryExposureRoutes,
                        project.AssessmentSettings.ExposureType,
                        project.OutputDetailSettings.LowerPercentage,
                        project.OutputDetailSettings.UpperPercentage,
                        project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                        project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                        project.SubsetSettings.IsPerPerson
                    );
                    sub2Header.SaveSummarySection(section);
                }
                if (result.NonDietaryIndividualDayIntakes != null) {
                    var section = new NonDietaryUpperDistributionRouteCompoundSection();
                    var sub2Header = subHeader.AddSubSectionHeaderFor(section, "Upper tail distribution", 2);
                    section.Summarize(
                        data.ActiveSubstances,
                        result.NonDietaryIndividualDayIntakes,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        data.NonDietaryExposureRoutes,
                        project.AssessmentSettings.ExposureType,
                        project.OutputDetailSettings.LowerPercentage,
                        project.OutputDetailSettings.UpperPercentage,
                        project.OutputDetailSettings.PercentageForUpperTail,
                        project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                        project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                        project.SubsetSettings.IsPerPerson
                    );
                    sub2Header.SaveSummarySection(section);
                }
            }

            if (project.OutputDetailSettings.IsDetailedOutput
                && !project.OutputDetailSettings.SkipPrivacySensitiveOutputs
            ) {
                summarizeNonDietaryDrillDown(
                    data.NonDietaryExposureRoutes,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    data.ReferenceSubstance,
                    result.NonDietaryIndividualDayIntakes,
                    result.KineticConversionFactors,
                    project.OutputDetailSettings.PercentageForDrilldown,
                    project.OutputDetailSettings.IsDetailedOutput,
                    project.SubsetSettings.IsPerPerson,
                    header,
                    order++
                );
            }
        }

        /// <summary>
        /// Summarize exposure by route
        /// </summary>
        /// <param name="project"></param>
        /// <param name="result"></param>
        /// <param name="data"></param>
        /// <param name="header"></param>
        /// <param name="order"></param>
        private void summarizeExposureByRoute(
            ProjectDto project,
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
                    project.OutputDetailSettings.LowerPercentage,
                    project.OutputDetailSettings.UpperPercentage,
                    project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                    project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
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
                    project.OutputDetailSettings.LowerPercentage,
                    project.OutputDetailSettings.UpperPercentage,
                    project.OutputDetailSettings.PercentageForUpperTail,
                    project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                    project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                    result.TargetExposureUnit,
                    result.ExternalExposureUnit
                );
                sub2Header.SaveSummarySection(section);
            }
            subHeader.SaveSummarySection(headSection);
        }

        private static void summarizeNonDietaryDrillDown(
                ICollection<ExposurePathType> nonDietaryExposureRoutes,
                IDictionary<Compound, double> relativePotencyFactors,
                IDictionary<Compound, double> membershipProbabilities,
                Compound reference,
                ICollection<NonDietaryIndividualDayIntake> nonDietaryIndividualDayIntakes,
                IDictionary<(ExposurePathType, Compound), double> kineticConversionFactors,
                double percentageForDrilldown,
                bool isDetailedOutput,
                bool isPerPerson,
                SectionHeader header,
                int order
            ) {
            if (isDetailedOutput) {
                var section = new NonDietaryDrillDownSection();
                var subHeader = header.AddSubSectionHeaderFor(section, "Drilldown individuals", order);
                var ids = section.GetDrillDownRecords(
                    percentageForDrilldown,
                    nonDietaryIndividualDayIntakes,
                    relativePotencyFactors,
                    membershipProbabilities,
                    kineticConversionFactors,
                    isPerPerson
                 );
                section.Summarize(
                    nonDietaryIndividualDayIntakes,
                    ids,
                    nonDietaryExposureRoutes,
                    relativePotencyFactors,
                    membershipProbabilities,
                    kineticConversionFactors,
                    reference,
                    isPerPerson
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private static List<ActionSummaryUnitRecord> collectUnits(ProjectDto project, ActionData data) {
            var result = new List<ActionSummaryUnitRecord>();
            var printOption = project.EffectSettings.TargetDoseLevelType == TargetLevelType.External ? TargetUnit.DisplayOption.AppendBiologicalMatrix : TargetUnit.DisplayOption.UnitOnly;
            result.Add(new ActionSummaryUnitRecord("IntakeUnit", data.TargetExposureUnit.GetShortDisplayName(printOption)));
            result.Add(new ActionSummaryUnitRecord("TargetAmountUnit", data.TargetExposureUnit.SubstanceAmountUnit.GetShortDisplayName()));
            result.Add(new ActionSummaryUnitRecord("TargetExposureUnit", data.TargetExposureUnit.GetShortDisplayName()));
            result.Add(new ActionSummaryUnitRecord("BodyWeightUnit", data.BodyWeightUnit.GetShortDisplayName()));
            result.Add(new ActionSummaryUnitRecord("ExternalExposureUnit", data.ExternalExposureUnit.GetShortDisplayName()));
            if (project.AssessmentSettings.ExposureType == ExposureType.Chronic) {
                result.Add(new ActionSummaryUnitRecord("IndividualDayUnit", "individuals"));
            } else {
                result.Add(new ActionSummaryUnitRecord("IndividualDayUnit", "individual days"));
            }
            result.Add(new ActionSummaryUnitRecord("LowerPercentage", $"p{project.OutputDetailSettings.LowerPercentage}"));
            result.Add(new ActionSummaryUnitRecord("UpperPercentage", $"p{project.OutputDetailSettings.UpperPercentage}"));
            result.Add(new ActionSummaryUnitRecord("ConcentrationUnit", data.ConcentrationUnit.GetShortDisplayName()));
            result.Add(new ActionSummaryUnitRecord("ConsumptionUnit", data.ConsumptionUnit.GetShortDisplayName()));
            result.Add(new ActionSummaryUnitRecord("LowerBound", $"p{project.UncertaintyAnalysisSettings.UncertaintyLowerBound}"));
            result.Add(new ActionSummaryUnitRecord("UpperBound", $"p{project.UncertaintyAnalysisSettings.UncertaintyUpperBound}"));
            return result;
        }
    }
}

