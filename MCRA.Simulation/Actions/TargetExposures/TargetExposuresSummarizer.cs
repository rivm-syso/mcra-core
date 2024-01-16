using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.ExposureLevelsCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation;
using MCRA.Simulation.Calculators.NonDietaryIntakeCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.TargetExposures {

    public enum TargetExposuresSections {
        DetailsSection,
        ExposuresByRouteSection,
        ExposuresBySubstanceSection,
        ExposuresByRouteSubstanceSection,
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
            var substances = data.ActiveSubstances;
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
                && (substances.Count == 1 || data.CorrectedRelativePotencyFactors != null)
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
                && (substances.Count == 1 || data.CorrectedRelativePotencyFactors != null)
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

            if (project.AssessmentSettings.Aggregate && data.CorrectedRelativePotencyFactors != null
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

            if (substances.Count > 1
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

            if (project.AssessmentSettings.Aggregate
                && substances.Count > 1
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

            if (result.KineticModelCalculators?.Values.Any(r => r is PbpkModelCalculator) ?? false) {
                subHeaderDetails = subHeaderDetails ?? subHeader.AddEmptySubSectionHeader("Details", subOrder);
                summarizeKineticModellingResults(
                    project,
                    result,
                    data,
                    subHeaderDetails,
                    subOrder++
                );
            }

            if (project.MixtureSelectionSettings.IsMcrAnalysis
                && result.ExposureMatrix != null
                && outputSettings.ShouldSummarize(TargetExposuresSections.McrCoExposureSection)
            ) {
                subHeaderDetails = subHeaderDetails ?? subHeader.AddEmptySubSectionHeader("Details", subOrder);
                summarizeMCRSection(
                    project,
                    result,
                    data,
                    subOrder++,
                    subHeaderDetails
                );
            }
            subHeader.SaveSummarySection(outputSummary);
        }

        private void summarizeMCRSection(
            ProjectDto project,
            TargetExposuresActionResult result,
            ActionData data,
            int subOrder,
            SectionHeader subHeaderDetails
        ) {
            var section = new MaximumCumulativeRatioSection() {
                SectionLabel = getSectionLabel(TargetExposuresSections.McrCoExposureSection)
            };
            var subSubHeader = subHeaderDetails.AddSubSectionHeaderFor(section, "MCR co-exposure", subOrder++);
            section.Summarize(
                result.DriverSubstances,
                result.TargetExposureUnit,
                project.MixtureSelectionSettings.McrExposureApproachType,
                project.OutputDetailSettings.MaximumCumulativeRatioCutOff,
                project.OutputDetailSettings.MaximumCumulativeRatioPercentiles,
                project.MixtureSelectionSettings.TotalExposureCutOff,
                project.OutputDetailSettings.MaximumCumulativeRatioMinimumPercentage
            );

            section.Summarize(
                result.ExposureMatrix,
                project.OutputDetailSettings.MaximumCumulativeRatioPercentiles,
                project.OutputDetailSettings.MaximumCumulativeRatioMinimumPercentage
            );
            subSubHeader.SaveSummarySection(section);
        }

        private void summarizeExposureDistribution(
            ProjectDto project,
            TargetExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int subOrder
        ) {
            var section = new ChronicAggregateSection();
            var subHeader = header.AddSubSectionHeaderFor(section, $"Exposure distribution", subOrder++);
            var substances = data.ActiveSubstances;
            section.Summarize(
                subHeader,
                result.AggregateIndividualExposures,
                substances,
                data.CorrectedRelativePotencyFactors,
                data.MembershipProbabilities,
                result.KineticConversionFactors,
                data.ExposureRoutes,
                data.ReferenceSubstance,
                project.OutputDetailSettings.ExposureMethod,
                project.OutputDetailSettings.ExposureLevels,
                project.OutputDetailSettings.SelectedPercentiles,
                project.OutputDetailSettings.PercentageForUpperTail,
                project.AssessmentSettings.Aggregate,
                project.SubsetSettings.IsPerPerson
            );
            subHeader.SaveSummarySection(section);

            if (project.OutputDetailSettings.StoreIndividualDayIntakes) {
                var individualDaysection = new IndividualCompoundIntakeSection();
                var sub2Header = subHeader.AddSubSectionHeaderFor(individualDaysection, "Simulated individual exposures", 10);
                individualDaysection.Summarize(
                    result.AggregateIndividualExposures,
                    substances,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    data.ReferenceSubstance ?? substances.First(),
                    true
                );
                sub2Header.SaveSummarySection(individualDaysection);
            }
        }

        private void summarizeDailyExposures(
            ProjectDto project,
            TargetExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int subOrder
        ) {
            var substances = data.ActiveSubstances;
            var section = new AggregateIntakeDistributionSection();
            var subHeader = header.AddSubSectionHeaderFor(section, "Exposures (daily intakes)", subOrder);
            section.Summarize(
                subHeader,
                result.AggregateIndividualDayExposures,
                substances,
                data.CorrectedRelativePotencyFactors,
                data.MembershipProbabilities,
                result.KineticConversionFactors,
                data.ExposureRoutes,
                data.ReferenceSubstance,
                project.OutputDetailSettings.ExposureMethod,
                project.OutputDetailSettings.ExposureLevels,
                project.OutputDetailSettings.SelectedPercentiles,
                project.OutputDetailSettings.PercentageForUpperTail,
                project.SubsetSettings.IsPerPerson,
                project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                project.UncertaintyAnalysisSettings.UncertaintyUpperBound
            );
            subHeader.SaveSummarySection(section);

            if (project.OutputDetailSettings.StoreIndividualDayIntakes) {
                var individualDaySection = new IndividualDayCompoundIntakeSection();
                var sub2Header = subHeader.AddSubSectionHeaderFor(individualDaySection, "Simulated individual day exposures", 10);
                individualDaySection.Summarize(
                    result.AggregateIndividualDayExposures,
                    substances,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    data.ReferenceSubstance ?? substances.First(),
                    true
                );
                sub2Header.SaveSummarySection(individualDaySection);
            }
        }

        /// <summary>
        /// Uncertainty
        /// </summary>
        /// <param name="header"></param>
        /// <param name="actionResult"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="activeSubstances"></param>
        /// <param name="kineticModelInstances"></param>
        /// <param name="exposureRoutes"></param>
        /// <param name="nonDietaryExposureRoutes"></param>
        /// <param name="exposureType"></param>
        /// <param name="uncertaintyLowerBound"></param>
        /// <param name="uncertaintyUpperBound"></param>
        /// <param name="isPerPerson"></param>
        /// <param name="isAggregate"></param>
        public void SummarizeUncertain(
            SectionHeader header,
            TargetExposuresActionResult actionResult,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<Compound> activeSubstances,
            ICollection<KineticModelInstance> kineticModelInstances,
            ICollection<ExposurePathType> exposureRoutes,
            ICollection<ExposurePathType> nonDietaryExposureRoutes,
            ExposureType exposureType,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson,
            bool isAggregate
        ) {
            SectionHeader subHeader, subHeader1, subSubHeader;
            subHeader = header.GetSubSectionHeader<TargetExposuresSummarySection>();

            var outputSummary = subHeader?.GetSummarySection() as TargetExposuresSummarySection;
            if (outputSummary == null) {
                return;
            }

            if (exposureType == ExposureType.Acute) {
                if (actionResult.AggregateIndividualDayExposures != null
                    && (activeSubstances.Count == 1 || relativePotencyFactors != null)
                ) {
                    subHeader = header.GetSubSectionHeader<AggregateIntakeDistributionSection>();
                    if (subHeader != null) {
                        var section = subHeader.GetSummarySection() as AggregateIntakeDistributionSection;
                        section.SummarizeUncertainty(
                            subHeader,
                            actionResult.AggregateIndividualDayExposures,
                            activeSubstances,
                            relativePotencyFactors,
                            membershipProbabilities,
                            uncertaintyLowerBound,
                            uncertaintyUpperBound,
                            isPerPerson
                        );
                        subHeader.SaveSummarySection(section);
                    }
                }
            } else {
                if (actionResult.AggregateIndividualExposures != null
                    && (activeSubstances.Count == 1 || relativePotencyFactors != null)
                ) {
                    subHeader = header.GetSubSectionHeader<ChronicAggregateSection>();
                    if (subHeader != null) {
                        var section = subHeader.GetSummarySection() as ChronicAggregateSection;
                        section.SummarizeUncertainty(
                            subHeader,
                            actionResult.AggregateIndividualExposures,
                            activeSubstances,
                            relativePotencyFactors,
                            membershipProbabilities,
                            uncertaintyLowerBound,
                            uncertaintyUpperBound,
                            isPerPerson
                        );
                        subHeader.SaveSummarySection(section);
                    }
                }
            }

            if (actionResult.KineticModelCalculators?.Values.Any(r => r is PbpkModelCalculator) ?? false) {
                foreach (var substance in activeSubstances) {
                    if (actionResult.KineticModelCalculators[substance] is PbpkModelCalculator) {
                        subHeader = header.GetSubSectionHeaderFromTitleString<KineticModelSection>(substance.Name);
                        if (subHeader != null) {
                            var section = subHeader.GetSummarySection() as KineticModelSection;
                            var kineticModelInstance = kineticModelInstances.Single(c => c.IsHumanModel && c.Substances.Contains(substance));
                            section.SummarizeAbsorptionFactorsUncertainty(actionResult.KineticConversionFactors, substance, exposureRoutes);
                            subHeader.SaveSummarySection(section);
                        }
                    } else {
                        // TODO: summarize linear aggregation results
                    }
                }
            }

            if (isAggregate) {
                subHeader = header.GetSubSectionHeader<TargetExposuresSummarySection>();
                if (subHeader != null) {
                    subSubHeader = subHeader.GetSubSectionHeader<TotalDistributionAggregateRouteSection>();
                    if (subSubHeader != null) {
                        var section1 = subSubHeader.GetSummarySection() as TotalDistributionAggregateRouteSection;
                        section1.SummarizeUncertainty(
                            actionResult.AggregateIndividualExposures,
                            actionResult.AggregateIndividualDayExposures,
                            relativePotencyFactors,
                            membershipProbabilities,
                            actionResult.KineticConversionFactors,
                            exposureRoutes,
                            isPerPerson
                        );
                        subSubHeader.SaveSummarySection(section1);
                    }
                    subSubHeader = subHeader.GetSubSectionHeader<UpperDistributionAggregateRouteSection>();
                    if (subSubHeader != null) {
                        var section2 = subSubHeader.GetSummarySection() as UpperDistributionAggregateRouteSection;
                        section2.SummarizeUncertainty(
                            actionResult.AggregateIndividualExposures,
                            actionResult.AggregateIndividualDayExposures,
                            relativePotencyFactors,
                            membershipProbabilities,
                            actionResult.KineticConversionFactors,
                            exposureRoutes,
                            isPerPerson
                        );
                        subSubHeader.SaveSummarySection(section2);
                    }
                }
            }

            if (isAggregate) {
                subHeader = header.GetSubSectionHeader<TargetExposuresSummarySection>();
                if (subHeader != null) {
                    subSubHeader = subHeader.GetSubSectionHeader<TotalDistributionRouteCompoundSection>();
                    if (subSubHeader != null) {
                        var section1 = subSubHeader.GetSummarySection() as TotalDistributionRouteCompoundSection;
                        section1.SummarizeUncertainty(
                            actionResult.AggregateIndividualExposures,
                            actionResult.AggregateIndividualDayExposures,
                            relativePotencyFactors,
                            membershipProbabilities,
                            actionResult.KineticConversionFactors,
                            activeSubstances,
                            isPerPerson
                        );
                        subSubHeader.SaveSummarySection(section1);
                    }
                    subSubHeader = subHeader.GetSubSectionHeader<UpperDistributionRouteCompoundSection>();
                    if (subSubHeader != null) {
                        var section2 = subSubHeader.GetSummarySection() as UpperDistributionRouteCompoundSection;
                        section2.SummarizeUncertainty(
                            actionResult.AggregateIndividualExposures,
                            actionResult.AggregateIndividualDayExposures,
                            relativePotencyFactors,
                            membershipProbabilities,
                            actionResult.KineticConversionFactors,
                            activeSubstances,
                            isPerPerson
                        );
                        subSubHeader.SaveSummarySection(section2);
                    }
                }
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
                    var section1 = subHeader.GetSummarySection() as NonDietaryTotalDistributionRouteSection;
                    section1.SummarizeUncertainty(
                        actionResult.NonDietaryIndividualDayIntakes,
                        relativePotencyFactors,
                        membershipProbabilities,
                        nonDietaryExposureRoutes,
                        exposureType,
                        isPerPerson
                    );
                    subHeader.SaveSummarySection(section1);
                }

                subHeader = header.GetSubSectionHeader<NonDietaryUpperDistributionRouteSection>();
                if (subHeader != null) {
                    var section2 = subHeader.GetSummarySection() as NonDietaryUpperDistributionRouteSection;
                    section2.SummarizeUncertainty(
                        actionResult.NonDietaryIndividualDayIntakes,
                        relativePotencyFactors,
                        membershipProbabilities,
                        nonDietaryExposureRoutes,
                        exposureType,
                        isPerPerson
                    );
                    subHeader.SaveSummarySection(section2);
                }
            }

            // Non-dietary route x substance
            if (isAggregate) {
                subHeader = header.GetSubSectionHeader<NonDietaryTotalDistributionRouteCompoundSection>();
                if (subHeader != null) {
                    var section1 = subHeader.GetSummarySection() as NonDietaryTotalDistributionRouteCompoundSection;
                    section1.SummarizeUncertainty(
                        activeSubstances,
                        actionResult.NonDietaryIndividualDayIntakes,
                        relativePotencyFactors,
                        membershipProbabilities,
                        nonDietaryExposureRoutes,
                        exposureType,
                        isPerPerson
                    );
                    subHeader.SaveSummarySection(section1);
                }
                subHeader = header.GetSubSectionHeader<NonDietaryUpperDistributionRouteCompoundSection>();
                if (subHeader != null) {
                    var section2 = subHeader.GetSummarySection() as NonDietaryUpperDistributionRouteCompoundSection;
                    section2.SummarizeUncertainty(
                        activeSubstances,
                        actionResult.NonDietaryIndividualDayIntakes,
                        relativePotencyFactors,
                        membershipProbabilities,
                        nonDietaryExposureRoutes,
                        exposureType,
                        isPerPerson
                    );
                    subHeader.SaveSummarySection(section2);
                }
            }
            if (activeSubstances.Count > 1 && relativePotencyFactors != null) {
                subHeader = header.GetSubSectionHeader<TargetExposuresSummarySection>();
                if (subHeader != null) {
                    subHeader1 = subHeader.GetSubSectionHeader<TotalDistributionCompoundSection>();
                    if (subHeader1 != null) {
                        var section1 = subHeader1.GetSummarySection() as TotalDistributionCompoundSection;
                        section1.SummarizeUncertainty(
                            actionResult.AggregateIndividualExposures,
                            actionResult.AggregateIndividualDayExposures,
                            relativePotencyFactors,
                            membershipProbabilities,
                            activeSubstances,
                            isPerPerson
                        );
                        subHeader1.SaveSummarySection(section1);
                    }
                    subHeader1 = subHeader.GetSubSectionHeader<UpperDistributionCompoundSection>();
                    if (subHeader1 != null) {
                        var section2 = subHeader1.GetSummarySection() as UpperDistributionCompoundSection;
                        section2.SummarizeUncertainty(
                            actionResult.AggregateIndividualExposures,
                            actionResult.AggregateIndividualDayExposures,
                            relativePotencyFactors,
                            membershipProbabilities,
                            activeSubstances,
                            isPerPerson
                        );
                        subHeader1.SaveSummarySection(section2);
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
            var substances = data.ActiveSubstances;
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
                substances,
                project.OutputDetailSettings.LowerPercentage,
                project.OutputDetailSettings.UpperPercentage,
                project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                project.SubsetSettings.IsPerPerson
            );
            sub2Header.SaveSummarySection(totalSection);

            var boxPlotSection = new TargetExposuresBySubstanceSection();
            sub2Header = subHeader.AddSubSectionHeaderFor(boxPlotSection, "Total distribution (boxplots)", subOrder++);
            boxPlotSection.Summarize(
                result.AggregateIndividualExposures,
                result.AggregateIndividualDayExposures,
                substances,
                totalSection.Records.Select(c => c.CompoundCode).ToList(),
                project.SubsetSettings.IsPerPerson
            );
            sub2Header.SaveSummarySection(boxPlotSection);

            if (data.CorrectedRelativePotencyFactors != null) {
                var upperSection = new UpperDistributionCompoundSection();
                sub2Header = subHeader.AddSubSectionHeaderFor(upperSection, "Upper tail distribution", subOrder++);
                upperSection.Summarize(
                    result.AggregateIndividualExposures,
                    result.AggregateIndividualDayExposures,
                    substances,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    project.AssessmentSettings.ExposureType,
                    project.OutputDetailSettings.PercentageForUpperTail,
                    project.OutputDetailSettings.LowerPercentage,
                    project.OutputDetailSettings.UpperPercentage,
                    project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                    project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                    project.SubsetSettings.IsPerPerson
                );
                sub2Header.SaveSummarySection(upperSection);
            }
            if (project.AssessmentSettings.Aggregate) {
                var section = new CoExposureTotalDistributionSection();
                sub2Header = subHeader.AddSubSectionHeaderFor(section, "Co-exposure total distribution", subOrder++);
                section.Summarize(
                    result.AggregateIndividualExposures?.Cast<ITargetIndividualExposure>().ToList(),
                    result.AggregateIndividualDayExposures?.Cast<ITargetIndividualDayExposure>().ToList(),
                     substances
                );
                sub2Header.SaveSummarySection(section);
            }
            if (project.AssessmentSettings.Aggregate && data.CorrectedRelativePotencyFactors != null) {
                var section = new CoExposureUpperDistributionSection();
                sub2Header = subHeader.AddSubSectionHeaderFor(section, "Co-exposure upper tail", subOrder++);
                section.Summarize(
                    result.AggregateIndividualExposures?.Cast<ITargetIndividualExposure>().ToList(),
                    result.AggregateIndividualDayExposures?.Cast<ITargetIndividualDayExposure>().ToList(),
                    substances,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    project.AssessmentSettings.ExposureType,
                    project.OutputDetailSettings.PercentageForUpperTail,
                    project.SubsetSettings.IsPerPerson
                );
                sub2Header.SaveSummarySection(section);
            }

            if (project.OutputDetailSettings.StoreIndividualDayIntakes) {
                if (result.AggregateIndividualExposures != null) {
                    var section = new IndividualCompoundIntakeSection();
                    sub2Header = subHeader.AddSubSectionHeaderFor(section, "Individual exposures by substance", subOrder++);
                    section.Summarize(
                        result.AggregateIndividualExposures,
                        substances,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities
                    );
                    sub2Header.SaveSummarySection(section);
                } else {
                    var section = new IndividualDayCompoundIntakeSection();
                    sub2Header = subHeader.AddSubSectionHeaderFor(section, "Individual day exposures by substance", subOrder++);
                    section.Summarize(
                        result.AggregateIndividualDayExposures,
                        substances,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities
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
        private void summarizeExposureByRouteSubstance(
            ProjectDto project,
            TargetExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var substances = data.ActiveSubstances;
            var headSection = new ExposureByRouteCompoundSection() {
                SectionLabel = getSectionLabel(TargetExposuresSections.ExposuresByRouteSubstanceSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(headSection, "Exposures by route and substance", order++);
            if (data.MembershipProbabilities != null) {
                var section = new TotalDistributionRouteCompoundSection();
                var sub2Header = subHeader.AddSubSectionHeaderFor(section, "Total distribution", order++);
                section.Summarize(
                    result.AggregateIndividualExposures,
                    result.AggregateIndividualDayExposures,
                    substances,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    result.KineticConversionFactors,
                    project.OutputDetailSettings.LowerPercentage,
                    project.OutputDetailSettings.UpperPercentage,
                    project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                    project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                    project.SubsetSettings.IsPerPerson
                );
                sub2Header.SaveSummarySection(section);
            }

            if (data.MembershipProbabilities != null && data.CorrectedRelativePotencyFactors != null) {
                var section = new UpperDistributionRouteCompoundSection();
                var sub2Header = subHeader.AddSubSectionHeaderFor(section, "Upper tail distribution", order++);
                section.Summarize(
                    result.AggregateIndividualExposures,
                    result.AggregateIndividualDayExposures,
                    substances,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    result.KineticConversionFactors,
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
            subHeader.SaveSummarySection(headSection);
        }
        /// <summary>
        /// Summarize KineticModellingResults
        /// </summary>
        /// <param name="project"></param>
        /// <param name="result"></param>
        /// <param name="data"></param>
        /// <param name="header"></param>
        /// <param name="order"></param>
        private void summarizeKineticModellingResults(
            ProjectDto project,
            TargetExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var substances = data.ActiveSubstances;
            ICollection<ITargetExposure> drillDownRecords = null;
            var rpfs = data.CorrectedRelativePotencyFactors ?? data.MembershipProbabilities.ToDictionary(c => c.Key, c => 1d);
            if (result.AggregateIndividualExposures != null) {
                drillDownRecords = KineticModelTimeCourseSection
                     .GetDrilldownIndividualTargetExposures(
                        result.AggregateIndividualExposures,
                        rpfs,
                        data.MembershipProbabilities,
                        project.OutputDetailSettings.PercentageForDrilldown,
                        project.SubsetSettings.IsPerPerson
                    )
                    .Cast<ITargetExposure>().ToList();
            } else {
                drillDownRecords = KineticModelTimeCourseSection
                    .GetDrilldownIndividualDayTargetExposures(
                        result.AggregateIndividualDayExposures,
                        rpfs,
                        data.MembershipProbabilities,
                        project.OutputDetailSettings.PercentageForDrilldown,
                        project.SubsetSettings.IsPerPerson
                     )
                    .Cast<ITargetExposure>().ToList();
            }

            if (substances.Count == 1) {
                // Single compound
                var substance = substances.First();
                if (result.KineticModelCalculators[substance] is PbpkModelCalculator) {
                    summarizeCompoundKineticModel(
                        result,
                        data.ExposureRoutes,
                        data.KineticModelInstances.Single(c => c.IsHumanModel && c.Substances.Contains(substance)),
                        data.ExternalExposureUnit,
                        substance,
                        drillDownRecords,
                        header,
                        project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                        project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                        project.AssessmentSettings.ExposureType == ExposureType.Acute,
                        order,
                        "Kinetic model"
                    );
                }
            } else {
                // Multiple compounds
                var subHeader = header.AddEmptySubSectionHeader("Kinetic models", order++);
                var subOrder = 0;

                var pbkModelSubstances= new List<Compound>();
                foreach (var calculator in result.KineticModelCalculators.Values) {
                    if (calculator is PbpkModelCalculator) {
                        foreach (var outputSubstance in calculator.OutputSubstances) {
                            summarizeCompoundKineticModel(
                                result,
                                data.ExposureRoutes,
                                (calculator as PbpkModelCalculator).KineticModelInstance,
                                data.ExternalExposureUnit,
                                outputSubstance,
                                drillDownRecords,
                                subHeader,
                                project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                                project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                                project.AssessmentSettings.ExposureType == ExposureType.Acute,
                                subOrder++,
                                $"PBPK model {outputSubstance.Name}"
                            );
                            pbkModelSubstances.Add(outputSubstance);
                        }
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

        private void summarizeCompoundKineticModel(
            TargetExposuresActionResult actionResult,
            ICollection<ExposurePathType> exposureRoutes,
            KineticModelInstance kineticModelInstance,
            ExposureUnitTriple externalExposureUnit,
            Compound substance,
            ICollection<ITargetExposure> drillDownRecords,
            SectionHeader header,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isAcute,
            int subOrder,
            string subTitle
        ) {
            var section = new KineticModelSection();
            var subHeader = header.AddSubSectionHeaderFor(section, subTitle, subOrder);
            section.Summarize(
                substance,
                kineticModelInstance,
                exposureRoutes,
                uncertaintyLowerBound,
                uncertaintyUpperBound
            );
            if (actionResult.AggregateIndividualExposures != null) {
                section.SummarizeAbsorptionChart(actionResult.AggregateIndividualExposures, substance, exposureRoutes);
            } else {
                section.SummarizeAbsorptionChart(actionResult.AggregateIndividualDayExposures, substance, exposureRoutes);
            }
            section.SummarizeAbsorptionFactors(actionResult.KineticConversionFactors, substance, exposureRoutes);
            subHeader.SaveSummarySection(section);

            if (drillDownRecords != null) {
                summarizeKineticModelTimeCourse(
                    subHeader,
                    exposureRoutes,
                    kineticModelInstance,
                    substance,
                    drillDownRecords,
                    isAcute,
                    0
                );
            }
        }

        /// <summary>
        /// Summarize KineticModelTimeCourse
        /// </summary>
        /// <param name="header"></param>
        /// <param name="exposureRoutes"></param>
        /// <param name="kineticModelInstances"></param>
        /// <param name="compound"></param>
        /// <param name="drillDownRecords"></param>
        /// <param name="isAcute"></param>
        /// <param name="subOrder"></param>
        private void summarizeKineticModelTimeCourse(
            SectionHeader header,
            ICollection<ExposurePathType> exposureRoutes,
            KineticModelInstance kineticModelInstance,
            Compound compound,
            ICollection<ITargetExposure> drillDownRecords,
            bool isAcute,
            int subOrder
        ) {
            var section = new KineticModelTimeCourseSection();
            var subHeader = header.AddSubSectionHeaderFor(section, $"Individual drilldown PBPK model {compound.Name}", subOrder++);
            if (isAcute) {
                section.SummarizeIndividualDayDrillDown(
                    drillDownRecords.Cast<ITargetIndividualDayExposure>().ToList(),
                    exposureRoutes,
                    compound,
                    kineticModelInstance);
            } else {
                section.SummarizeIndividualDrillDown(
                    drillDownRecords.Cast<ITargetIndividualExposure>().ToList(),
                    exposureRoutes,
                    compound,
                    kineticModelInstance,
                    isAcute
                );
            }
            subHeader.SaveSummarySection(section);
        }

        /// <summary>
        /// Summarize NonDietaryExposures
        /// </summary>
        /// <param name="project"></param>
        /// <param name="result"></param>
        /// <param name="data"></param>
        /// <param name="header"></param>
        /// <param name="order"></param>
        private void summarizeNonDietaryExposures(
                ProjectDto project,
                TargetExposuresActionResult result,
                ActionData data,
                SectionHeader header,
                int order
            ) {
            var substances = data.ActiveSubstances;
            if (result.NonDietaryIndividualDayIntakes != null) {
                var section = new NonDietaryIntakeDistributionSection();
                var sub2Header = header.AddSubSectionHeaderFor(section, "Non-dietary distribution (daily intakes)", order++);
                var intakes = result.NonDietaryIndividualDayIntakes.Select(c => c.ExternalTotalNonDietaryIntakePerMassUnit(data.CorrectedRelativePotencyFactors, data.MembershipProbabilities, project.SubsetSettings.IsPerPerson)).ToList();
                var exposureLevels = ExposureLevelsCalculator.GetExposureLevels(
                    intakes,
                    project.OutputDetailSettings.ExposureMethod,
                    project.OutputDetailSettings.ExposureLevels);
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
            if (result.NonDietaryIndividualDayIntakes != null && substances.Count > 1) {
                var subHeader = header.AddEmptySubSectionHeader("Exposures by route and substance", order++);
                if (result.NonDietaryIndividualDayIntakes != null) {
                    var section = new NonDietaryTotalDistributionRouteCompoundSection();
                    var sub2Header = subHeader.AddSubSectionHeaderFor(section, "Total distribution", 1);
                    section.Summarize(
                        substances,
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
                        substances,
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

            if (project.OutputDetailSettings.IsDetailedOutput) {
                summarizeNonDietaryDrillDown(
                    header,
                    data.NonDietaryExposureRoutes,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    data.ReferenceSubstance,
                    result.NonDietaryIndividualDayIntakes,
                    result.KineticConversionFactors,
                    project.OutputDetailSettings.PercentageForDrilldown,
                    project.OutputDetailSettings.IsDetailedOutput,
                    project.SubsetSettings.IsPerPerson,
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
            if (true) {
                var section = new TotalDistributionAggregateRouteSection();
                var sub2Header = subHeader.AddSubSectionHeaderFor(section, "Total distribution", 1);
                section.Summarize(
                    result.AggregateIndividualExposures,
                    result.AggregateIndividualDayExposures,
                    data.ExposureRoutes,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    result.KineticConversionFactors,
                    project.OutputDetailSettings.LowerPercentage,
                    project.OutputDetailSettings.UpperPercentage,
                    project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                    project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                    project.SubsetSettings.IsPerPerson
                );
                sub2Header.SaveSummarySection(section);
            }

            if (true) {
                var section = new UpperDistributionAggregateRouteSection();
                var sub2Header = subHeader.AddSubSectionHeaderFor(section, "Upper tail distribution", 2);
                section.Summarize(
                    result.AggregateIndividualExposures,
                    result.AggregateIndividualDayExposures,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    result.KineticConversionFactors,
                    data.ExposureRoutes,
                    project.OutputDetailSettings.LowerPercentage,
                    project.OutputDetailSettings.UpperPercentage,
                    project.OutputDetailSettings.PercentageForUpperTail,
                    project.AssessmentSettings.ExposureType,
                    project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                    project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                    project.SubsetSettings.IsPerPerson
                );
                sub2Header.SaveSummarySection(section);
            }
            subHeader.SaveSummarySection(headSection);
        }

        private void summarizeNonDietaryDrillDown(
                SectionHeader header,
                ICollection<ExposurePathType> nonDietaryExposureRoutes,
                IDictionary<Compound, double> relativePotencyFactors,
                IDictionary<Compound, double> membershipProbabilities,
                Compound reference,
                ICollection<NonDietaryIndividualDayIntake> nonDietaryIndividualDayIntakes,
                IDictionary<(ExposurePathType, Compound), double> absorptionFactors,
                double percentageForDrilldown,
                bool isDetailedOutput,
                bool isPerPerson,
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
                    absorptionFactors,
                    isPerPerson
                 );
                section.Summarize(
                    nonDietaryIndividualDayIntakes,
                    ids,
                    nonDietaryExposureRoutes,
                    relativePotencyFactors,
                    membershipProbabilities,
                    absorptionFactors,
                    reference,
                    isPerPerson
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private static List<ActionSummaryUnitRecord> collectUnits(ProjectDto project, ActionData data) {
            var result = new List<ActionSummaryUnitRecord>();
            var targetConcentrationUnit = data.TargetExposureUnit;
            var printOption = project.EffectSettings.TargetDoseLevelType == TargetLevelType.External ? TargetUnit.DisplayOption.AppendBiologicalMatrix : TargetUnit.DisplayOption.UnitOnly;
            result.Add(new ActionSummaryUnitRecord("IntakeUnit", data.TargetExposureUnit.GetShortDisplayName(printOption)));
            result.Add(new ActionSummaryUnitRecord("TargetAmountUnit", data.TargetExposureUnit.SubstanceAmountUnit.GetShortDisplayName()));
            result.Add(new ActionSummaryUnitRecord("TargetConcentrationUnit", targetConcentrationUnit.GetShortDisplayName()));
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

