using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.Calculators.IntraSpeciesConversion;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.OutputGeneration.ActionSummaries.Risk;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.Risks {

    public enum RisksSections {
        RisksDistributionSection,
        RisksDistributionsBySubstanceSection,
        RisksBySubstanceOverviewSection,
        HazardExposureSection,
        HazardDistributionSection,
        RisksByModelledFoodSection,
        ModelledFoodAtRiskSection,
        RisksBySubstanceSection,
        SubstanceAtRiskSection,
        RisksByModelledFoodSubstanceSection,
        ModelledFoodSubstanceAtRiskSection
    }
    public class RisksSummarizer : ActionResultsSummarizerBase<RisksActionResult> {

        public override ActionType ActionType => ActionType.Risks;

        public override void Summarize(
            ProjectDto project,
            RisksActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var outputSettings = new ModuleOutputSectionsManager<RisksSections>(project, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var outputSummary = new RiskSummarySection() {
                SectionLabel = ActionType.ToString()
            };

            var isHazardDistribution = data.HazardCharacterisationModelsCollections
                .SelectMany(r => r.HazardCharacterisationModels)
                .Any(r => !double.IsNaN(r.Value.GeometricStandardDeviation));

            outputSummary.Summarize(
                project.AssessmentSettings.ExposureType,
                project.EffectSettings.TargetDoseLevelType,
                project.RisksSettings.RiskMetricType,
                project.RisksSettings.RiskMetricCalculationType,
                project.EffectSettings.TargetDoseLevelType == TargetLevelType.Internal
                    ? ActionType.TargetExposures
                    : ActionType.DietaryExposures,
                isHazardDistribution
            );

            var subHeader = header.AddSubSectionHeaderFor(outputSummary, ActionType.GetDisplayName(), order);
            subHeader.Units = collectUnits(project);
            subHeader.SaveSummarySection(outputSummary);
            var subOrder = 0;

            // Total distribution section
            var isCumulative = project.AssessmentSettings.MultipleSubstances && project.RisksSettings.CumulativeRisk;
            var referenceSubstance = data.ActiveSubstances.Count == 1
                ? data.ActiveSubstances.First()
                : data.ReferenceSubstance;
            if (outputSettings.ShouldSummarize(RisksSections.RisksDistributionSection)) {
                summarizeRiskDistribution(
                    result.IndividualEffects,
                    result.ReferenceDose,
                    result.TargetUnits.Count == 1
                        ? result.TargetUnits.First()
                        : null,
                    referenceSubstance,
                    data.SelectedEffect,
                    isCumulative,
                    project,
                    subHeader,
                    subOrder
                );
            }

            // Risk by substance (overview)
            if (outputSettings.ShouldSummarize(RisksSections.RisksBySubstanceSection)
                && (result.IndividualEffectsBySubstanceCollections?.Any() ?? false)
            ) {
                summarizeRiskBySubstanceOverview(
                    result.TargetUnits,
                    result.IndividualEffectsBySubstanceCollections,
                    result.IndividualEffects,
                    data.ActiveSubstances,
                    data.SelectedEffect,
                    project.RisksSettings.RiskMetricType,
                    project.RisksSettings.RiskMetricCalculationType,
                    project.RisksSettings.ConfidenceInterval,
                    project.RisksSettings.LeftMargin,
                    project.RisksSettings.RightMargin,
                    project.RisksSettings.ThresholdMarginOfExposure,
                    project.RisksSettings.IsInverseDistribution,
                    project.EffectSettings.UseIntraSpeciesConversionFactors,
                    isCumulative,
                    subOrder,
                    subHeader
                 );
            }

            // Hazard versus exposure
            if (outputSettings.ShouldSummarize(RisksSections.HazardExposureSection)) {
                var section = new HazardExposureSection() {
                    SectionLabel = getSectionLabel(RisksSections.HazardExposureSection)
                };
                var subSubHeader = subHeader.AddSubSectionHeaderFor(section, "Hazard versus exposure", subOrder++);
                section.Summarize(
                    result.ExposureTargets,
                    result.IndividualEffectsBySubstanceCollections,
                    result.IndividualEffects,
                    project.RisksSettings.HealthEffectType,
                    data.ActiveSubstances,
                    data.HazardCharacterisationModelsCollections,
                    result.ReferenceDose,
                    project.RisksSettings.RiskMetricType,
                    project.RisksSettings.RiskMetricCalculationType,
                    project.RisksSettings.ConfidenceInterval,
                    project.RisksSettings.ThresholdMarginOfExposure,
                    project.RisksSettings.NumberOfLabels,
                    project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                    project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                    isCumulative
                );
                subSubHeader.SaveSummarySection(section);
            }

            // Distributions by substance
            if (result.IndividualEffectsBySubstanceCollections?.Any() ?? false
                && data.ActiveSubstances.Count > 1
                && outputSettings.ShouldSummarize(RisksSections.RisksDistributionsBySubstanceSection)
            ) {
                summarizeRiskDistributionBySubstances(
                    project,
                    result,
                    data.ActiveSubstances,
                    subHeader,
                    subOrder
                );
            }

            // Hazard distribution
            if (result.IndividualEffects != null
                && result.ReferenceDose != null
                && !double.IsNaN(result.ReferenceDose.GeometricStandardDeviation)
                && outputSettings.ShouldSummarize(RisksSections.HazardDistributionSection)
            ) {
                summarizeHazardDistribution(
                    result.IndividualEffects,
                    result.ReferenceDose,
                    project.EffectSettings.UseIntraSpeciesConversionFactors ? data.IntraSpeciesFactorModels : null,
                    project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                    project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                    project.OutputDetailSettings.SelectedPercentiles,
                    subHeader,
                    subOrder
                );
            }

            // Equivalent animal dose (EAD)
            if (project.RisksSettings.IsEAD && result.IndividualEffects != null) {
                var section = new EquivalentAnimalDoseSection();
                var subSubHeader = header.AddSubSectionHeaderFor(section, "Equivalent animal dose (EAD)", subOrder++);
                section.Summarize(
                    result.IndividualEffects,
                    result.ReferenceDose,
                    project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                    project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                    project.OutputDetailSettings.SelectedPercentiles
                );
                subSubHeader.SaveSummarySection(section);
            }

            // Forward effect calculation (predicted responses / health effects)
            if (project.RisksSettings.IsEAD && result.IndividualEffects != null) {
                var section = new PredictedHealthEffectSection();
                var subSubHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Predicted health effects",
                    subOrder++
                );
                section.Summarize(
                    result.IndividualEffects,
                    project.RisksSettings.HealthEffectType,
                    result.ReferenceDose,
                    project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                    project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                    project.OutputDetailSettings.SelectedPercentiles
                );
                subSubHeader.SaveSummarySection(section);
            }

            // Risks by food/substance
            if (isCumulative
                && result.IndividualEffects != null
                && project.EffectSettings.TargetDoseLevelType == TargetLevelType.External
                && project.RisksSettings.CalculateRisksByFood
                && project.RisksSettings.RiskMetricCalculationType == RiskMetricCalculationType.RPFWeighted
            ) {
                summarizeDietaryRisks(
                    result.IndividualEffects,
                    result.IndividualEffectsByModelledFood,
                    result.IndividualEffectsBySubstanceCollections?.First().IndividualEffects,
                    result.IndividualEffectsByModelledFoodSubstance,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    outputSettings,
                    project,
                    subHeader,
                    subOrder
                );
            }
        }

        /// <summary>
        /// Summarizes full tables with risks and percentages at risk for modelled foods, 
        /// substances and combinations
        /// </summary>
        /// <param name="individualEffects"></param>
        /// <param name="individualEffectsByModelledFood"></param>
        /// <param name="individualEffectsBySubstance"></param>
        /// <param name="outputSettings"></param>
        /// <param name="project"></param>
        /// <param name="header"></param>
        /// <param name="subOrder"></param>
        private void summarizeDietaryRisks(
            ICollection<IndividualEffect> individualEffects,
            Dictionary<Food, List<IndividualEffect>> individualEffectsByModelledFood,
            Dictionary<Compound, List<IndividualEffect>> individualEffectsBySubstance,
            IDictionary<(Food, Compound), List<IndividualEffect>> individualEffectsByModelledFoodSubstance,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> memberships,
            ModuleOutputSectionsManager<RisksSections> outputSettings,
            ProjectDto project,
            SectionHeader header,
            int subOrder
        ) {
            var subHeader = header.AddEmptySubSectionHeader("Details", subOrder++);

            // (Dietary) risks by modelled food
            if ((individualEffectsByModelledFood?.Any() ?? false)
                && outputSettings.ShouldSummarize(RisksSections.RisksByModelledFoodSection)
            ) {
                summarizeRiskByModelledFood(
                    individualEffectsByModelledFood,
                    project,
                    subHeader,
                    subOrder
                );
            }

            var hasThresholdExceedances = (project.RisksSettings.RiskMetricType == RiskMetricType.MarginOfExposure)
                ? individualEffects.Any(c => c.ExposureConcentration > 0 && c.HazardExposureRatio <= project.RisksSettings.ThresholdMarginOfExposure)
                : individualEffects.Any(c => c.ExposureHazardRatio >= project.RisksSettings.ThresholdMarginOfExposure);

            if (hasThresholdExceedances) {
                // Risks modelled foods at risks
                if (project.EffectSettings.TargetDoseLevelType == TargetLevelType.External
                    && project.RisksSettings.CalculateRisksByFood
                    && outputSettings.ShouldSummarize(RisksSections.ModelledFoodAtRiskSection)
                    && (individualEffectsByModelledFood?.Any() ?? false)
                ) {
                    summarizeModelledFoodsAtRisk(
                        individualEffectsByModelledFood,
                        individualEffects.Count,
                        project,
                        subHeader,
                        subOrder
                    );
                }
            }

            if (project.EffectSettings.TargetDoseLevelType == TargetLevelType.External
                && project.RisksSettings.CalculateRisksByFood
                && outputSettings.ShouldSummarize(RisksSections.RisksBySubstanceSection)
                && (individualEffectsBySubstance?.Any() ?? false)
            ) {
                summarizeRiskBySubstance(
                    individualEffectsBySubstance,
                    relativePotencyFactors,
                    memberships,
                    project,
                    subHeader,
                    subOrder
                );
            }

            if (hasThresholdExceedances) {
                if (project.EffectSettings.TargetDoseLevelType == TargetLevelType.External
                    && project.RisksSettings.CalculateRisksByFood
                    && outputSettings.ShouldSummarize(RisksSections.SubstanceAtRiskSection)
                    && (individualEffectsBySubstance?.Any() ?? false)
                ) {
                    summarizeSubstancesAtRisk(
                        individualEffectsBySubstance,
                        individualEffects.Count,
                        project,
                        subHeader,
                        subOrder
                    );
                }
            }

            // (Dietary) risks by modelled food and substance
            if ((individualEffectsByModelledFoodSubstance?.Any() ?? false)
                && outputSettings.ShouldSummarize(RisksSections.RisksByModelledFoodSubstanceSection)
            ) {
                summarizeRiskByModelledFoodSubstance(
                    individualEffectsByModelledFoodSubstance,
                    project,
                    subHeader,
                    subOrder
                );
            }

            if (hasThresholdExceedances) {
                if (project.EffectSettings.TargetDoseLevelType == TargetLevelType.External
                    && project.RisksSettings.CalculateRisksByFood
                    && outputSettings.ShouldSummarize(RisksSections.ModelledFoodSubstanceAtRiskSection)
                    && (individualEffectsByModelledFoodSubstance?.Any() ?? false)
                ) {
                    summarizeModelledFoodSubstancesAtRisk(
                        individualEffectsByModelledFoodSubstance,
                        individualEffects.Count,
                        project,
                        subHeader,
                        subOrder
                    );
                }
            }
        }

        /// <summary>
        /// Summarize risks by substance for multiple substances.
        /// </summary>
        /// <param name="targetUnits"></param>
        /// <param name="individualEffectsBySubstanceCollections"></param>
        /// <param name="individualEffects"></param>
        /// <param name="substances"></param>
        /// <param name="focalEffect"></param>
        /// <param name="riskMetric"></param>
        /// <param name="riskMetricCalculationType"></param>
        /// <param name="confidenceInterval"></param>
        /// <param name="leftMargin"></param>
        /// <param name="rightMargin"></param>
        /// <param name="threshold"></param>
        /// <param name="isInverseDistribution"></param>
        /// <param name="useIntraSpeciesFactors"></param>
        /// <param name="isCumulative"></param>
        /// <param name="subOrder"></param>
        /// <param name="header"></param>
        private void summarizeRiskBySubstanceOverview(
            List<TargetUnit> targetUnits,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)> individualEffectsBySubstanceCollections,
            List<IndividualEffect> individualEffects,
            ICollection<Compound> substances,
            Effect focalEffect,
            RiskMetricType riskMetric,
            RiskMetricCalculationType riskMetricCalculationType,
            double confidenceInterval,
            double leftMargin,
            double rightMargin,
            double threshold,
            bool isInverseDistribution,
            bool useIntraSpeciesFactors,
            bool isCumulative,
            int subOrder,
            SectionHeader header
        ) {
            if (riskMetric == RiskMetricType.MarginOfExposure) {
                var section = new MultipleHazardExposureRatioSection() {
                    SectionLabel = getSectionLabel(RisksSections.RisksBySubstanceSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(
                   section,
                   "Risks by substance (overview)",
                   subOrder++
               );
                section.Summarize(
                    targetUnits,
                    individualEffectsBySubstanceCollections,
                    individualEffects,
                    substances,
                    focalEffect,
                    threshold,
                    confidenceInterval,
                    riskMetric,
                    riskMetricCalculationType,
                    leftMargin,
                    rightMargin,
                    isInverseDistribution,
                    isCumulative
                );
                subHeader.SaveSummarySection(section);
            } else if (riskMetric == RiskMetricType.HazardIndex) {
                var section = new MultipleExposureHazardRatioSection() {
                    SectionLabel = getSectionLabel(RisksSections.RisksBySubstanceSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Risks by substance (overview)",
                    subOrder++
                );
                section.Summarize(
                    targetUnits,
                    individualEffectsBySubstanceCollections,
                    individualEffects,
                    substances,
                    focalEffect,
                    riskMetricCalculationType,
                    riskMetric,
                    confidenceInterval,
                    threshold,
                    leftMargin,
                    rightMargin,
                    isInverseDistribution,
                    useIntraSpeciesFactors,
                    isCumulative
                );
                subHeader.SaveSummarySection(section);
            }

            // Sum of substance risk ratios (at percentile)
            if (riskMetric == RiskMetricType.HazardIndex && isCumulative) {
                var section = new CumulativeExposureHazardRatioSection() {
                    SectionLabel = getSectionLabel(RisksSections.RisksBySubstanceSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Percentile risk ratio sums",
                    subOrder++
                );
                section.Summarize(
                    targetUnits,
                    individualEffectsBySubstanceCollections,
                    individualEffects,
                    substances,
                    focalEffect,
                    riskMetricCalculationType,
                    riskMetric,
                    confidenceInterval,
                    threshold,
                    leftMargin,
                    rightMargin,
                    isInverseDistribution,
                    useIntraSpeciesFactors,
                    isCumulative
                );
                subHeader.SaveSummarySection(section);
            }
        }

        /// <summary>
        /// Safety chart for single substance
        /// </summary>
        /// <param name="individualEffects"></param>
        /// <param name="target"></param>
        /// <param name="substance"></param>
        /// <param name="focalEffect"></param>
        /// <param name="riskMetric"></param>
        /// <param name="confidenceInterval"></param>
        /// <param name="leftMargin"></param>
        /// <param name="rightMargin"></param>
        /// <param name="threshold"></param>
        /// <param name="isInverseDistribution"></param>
        /// <param name="isCumulative"></param>
        /// <param name="subOrder"></param>
        /// <param name="header"></param>
        private void summarizeRiskBySingleSubstance(
            List<IndividualEffect> individualEffects,
            TargetUnit targetUnit,
            Compound substance,
            Effect focalEffect,
            RiskMetricType riskMetric,
            RiskMetricCalculationType riskMetricCalculationType,
            IHazardCharacterisationModel referenceDose,
            double confidenceInterval,
            double leftMargin,
            double rightMargin,
            double threshold,
            bool isInverseDistribution,
            bool isCumulative,
            int subOrder,
            SectionHeader header
        ) {
            if (riskMetric == RiskMetricType.MarginOfExposure) {
                var section = new SingleHazardExposureRatioSection();
                SectionHeader subHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Safety chart",
                    subOrder++
                );
                section.Summarize(
                    individualEffects,
                    targetUnit,
                    substance,
                    focalEffect,
                    threshold,
                    confidenceInterval,
                    riskMetric,
                    riskMetricCalculationType,
                    referenceDose,
                    leftMargin,
                    rightMargin,
                    isInverseDistribution,
                    isCumulative
                );
                subHeader.SaveSummarySection(section);
            } else if (riskMetric == RiskMetricType.HazardIndex) {
                var section = new SingleExposureHazardRatioSection();
                var subHeader = header.AddSubSectionHeaderFor(
                     section,
                    "Safety chart",
                    subOrder++
                );
                section.Summarize(
                    individualEffects,
                    targetUnit,
                    substance,
                    focalEffect,
                    confidenceInterval,
                    threshold,
                    riskMetricCalculationType,
                    referenceDose,
                    riskMetric,
                    leftMargin,
                    rightMargin,
                    isInverseDistribution,
                    isCumulative
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private static List<ActionSummaryUnitRecord> collectUnits(ProjectDto project) {
            var result = new List<ActionSummaryUnitRecord>();

            // Risk metric
            result.Add(new ActionSummaryUnitRecord("RiskMetric", project.RisksSettings.RiskMetricType.GetDisplayName()));
            result.Add(new ActionSummaryUnitRecord("RiskMetricShort", project.RisksSettings.RiskMetricType.GetShortDisplayName()));

            // Quartiles lower and upper, variabiliteit
            var lowerPercentage = (100 - project.RisksSettings.ConfidenceInterval) / 2;
            var upperPercentage = 100 - (100 - project.RisksSettings.ConfidenceInterval) / 2;
            result.Add(new ActionSummaryUnitRecord("LowerPercentage", $"p{project.OutputDetailSettings.LowerPercentage:#0.##}"));
            result.Add(new ActionSummaryUnitRecord("UpperPercentage", $"p{project.OutputDetailSettings.UpperPercentage:#0.##}"));
            result.Add(new ActionSummaryUnitRecord("LowerConfidenceBound", $"p{lowerPercentage:#0.##}"));
            result.Add(new ActionSummaryUnitRecord("UpperConfidenceBound", $"p{upperPercentage:#0.##}"));

            // Uncertainty
            result.Add(new ActionSummaryUnitRecord("LowerBound", $"p{project.UncertaintyAnalysisSettings.UncertaintyLowerBound:#0.##}"));
            result.Add(new ActionSummaryUnitRecord("UpperBound", $"p{project.UncertaintyAnalysisSettings.UncertaintyUpperBound:#0.##}"));

            if (project.AssessmentSettings.ExposureType == ExposureType.Chronic) {
                result.Add(new ActionSummaryUnitRecord("IndividualDayUnit", "individuals"));
            } else {
                result.Add(new ActionSummaryUnitRecord("IndividualDayUnit", "individual days"));
            }
            return result;
        }

        public void SummarizeUncertain(ProjectDto project, RisksActionResult result, ActionData data, SectionHeader header) {
            var outputSettings = new ModuleOutputSectionsManager<RisksSections>(project, ActionType);
            var subHeader = header.GetSubSectionHeader<RiskSummarySection>();
            if (subHeader == null) {
                return;
            }
            var outputSummary = (RiskSummarySection)subHeader.GetSummarySection();
            if (outputSummary == null) {
                return;
            }

            // Total distribution section
            var isCumulative = project.AssessmentSettings.MultipleSubstances && project.RisksSettings.CumulativeRisk;
            var referenceSubstance = data.ActiveSubstances.Count == 1
                ? data.ActiveSubstances.First()
                : data.ReferenceSubstance;
            if (outputSettings.ShouldSummarize(RisksSections.RisksDistributionSection)) {
                summarizeRiskDistributionUncertainty(
                    referenceSubstance,
                    result.IndividualEffects,
                    project.RisksSettings.RiskMetricType,
                    project.RisksSettings.RiskMetricCalculationType,
                    project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                    project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                    project.RisksSettings.IsInverseDistribution,
                    isCumulative,
                    header
                );
            }

            // Risks by substance (overview)
            if (project.RisksSettings.RiskMetricType == RiskMetricType.HazardIndex) {
                subHeader = header.GetSubSectionHeader<MultipleExposureHazardRatioSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as MultipleExposureHazardRatioSection;
                    section.SummarizeUncertain(
                        result.TargetUnits,
                        data.ActiveSubstances,
                        result.IndividualEffectsBySubstanceCollections,
                        result.IndividualEffects,
                        project.RisksSettings.IsInverseDistribution,
                        project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                        project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                        isCumulative);
                    subHeader.SaveSummarySection(section);
                }
            } else if (project.RisksSettings.RiskMetricType == RiskMetricType.MarginOfExposure) {
                subHeader = header.GetSubSectionHeader<MultipleHazardExposureRatioSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as MultipleHazardExposureRatioSection;
                    section.SummarizeUncertain(
                        result.TargetUnits,
                        data.ActiveSubstances,
                        result.IndividualEffectsBySubstanceCollections,
                        result.IndividualEffects,
                        project.RisksSettings.RiskMetricCalculationType,
                        project.RisksSettings.IsInverseDistribution,
                        project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                        project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                        isCumulative);
                    subHeader.SaveSummarySection(section);
                }
            }

            // Sum of substance risk ratios (at percentile)
            if (project.RisksSettings.RiskMetricType == RiskMetricType.HazardIndex && isCumulative) {
                subHeader = header.GetSubSectionHeader<CumulativeExposureHazardRatioSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as CumulativeExposureHazardRatioSection;
                    section.SummarizeUncertain(
                        result.ExposureTargets,
                        data.ActiveSubstances,
                        result.IndividualEffectsBySubstanceCollections,
                        result.IndividualEffects,
                        project.RisksSettings.IsInverseDistribution,
                        project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                        project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                        isCumulative
                    );
                    subHeader.SaveSummarySection(section);
                }
            }

            // Hazard versus exposure
            subHeader = header.GetSubSectionHeader<HazardExposureSection>();
            if (subHeader != null) {
                var hazardCharacterisationSubstances = data.HazardCharacterisationModelsCollections?
                    .SelectMany(c => c.HazardCharacterisationModels.Select(r => r.Key))
                    .ToHashSet();

                var section = subHeader.GetSummarySection() as HazardExposureSection;
                section.SummarizeUncertainty(
                    result.ExposureTargets,
                    result.IndividualEffectsBySubstanceCollections,
                    result.IndividualEffects,
                    data.HazardCharacterisationModelsCollections,
                    data.ActiveSubstances,
                    result.ReferenceDose,
                    project.RisksSettings.RiskMetricCalculationType,
                    project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                    project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                    isCumulative
                );
                subHeader.SaveSummarySection(section);
            }

            // Distributions by substance
            summarizeRiskDistributionBySubstancesUncertainty(
                result.ExposureTargets,
                result.IndividualEffectsBySubstanceCollections,
                data.ActiveSubstances,
                project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                project.RisksSettings.RiskMetricType,
                project.RisksSettings.IsInverseDistribution,
                header
            );

            // Hazard distribution
            if (result.IndividualEffects != null
                && result.ReferenceDose != null
                && !double.IsNaN(result.ReferenceDose.GeometricStandardDeviation)
                && outputSettings.ShouldSummarize(RisksSections.HazardDistributionSection)
            ) {
                summarizeHazardDistributionUncertainty(
                    project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                    project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                    result.IndividualEffects,
                    header
                );
            }

            if (project.RisksSettings.IsEAD && result.IndividualEffects != null) {
                subHeader = header.GetSubSectionHeader<EquivalentAnimalDoseSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as EquivalentAnimalDoseSection;
                    section.SummarizeUncertainty(result.IndividualEffects);
                    subHeader.SaveSummarySection(section);
                }
            }

            if (project.RisksSettings.IsEAD && result.IndividualEffects != null) {
                subHeader = header.GetSubSectionHeader<PredictedHealthEffectSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as PredictedHealthEffectSection;
                    section.SummarizeUncertainty(result.IndividualEffects);
                    subHeader.SaveSummarySection(section);
                }
            }

            // (Dietary) risks by food
            if (result.IndividualEffectsByModelledFood?.Any() ?? false) {
                subHeader = header.GetSubSectionHeader<HazardExposureRatioModelledFoodSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as HazardExposureRatioModelledFoodSection;
                    section.SummarizeFoodsUncertainty(result.IndividualEffectsByModelledFood);
                    subHeader.SaveSummarySection(section);
                }
                subHeader = header.GetSubSectionHeader<ExposureHazardRatioModelledFoodSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as ExposureHazardRatioModelledFoodSection;
                    section.SummarizeFoodsUncertainty(result.IndividualEffectsByModelledFood);
                    subHeader.SaveSummarySection(section);
                }
            }

            // (Dietary) risks by substance
            if (isCumulative
                && project.EffectSettings.TargetDoseLevelType == TargetLevelType.External
                && project.RisksSettings.CalculateRisksByFood
                && project.RisksSettings.RiskMetricCalculationType == RiskMetricCalculationType.RPFWeighted
            ) {
                subHeader = header.GetSubSectionHeader<HazardExposureRatioSubstanceSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as HazardExposureRatioSubstanceSection;
                    section.SummarizeSubstancesUncertainty(
                        result.IndividualEffectsBySubstanceCollections.First().IndividualEffects,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities
                    );
                    subHeader.SaveSummarySection(section);
                }
                subHeader = header.GetSubSectionHeader<ExposureHazardRatioSubstanceSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as ExposureHazardRatioSubstanceSection;
                    section.SummarizeSubstancesUncertainty(
                        result.IndividualEffectsBySubstanceCollections.First().IndividualEffects,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities
                    );
                    subHeader.SaveSummarySection(section);
                }
            }

            // (Dietary) risks by modelled food and substance
            if (result.IndividualEffectsByModelledFoodSubstance?.Any() ?? false) {
                subHeader = header.GetSubSectionHeader<HazardExposureRatioModelledFoodSubstanceSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as HazardExposureRatioModelledFoodSubstanceSection;
                    section.SummarizeModelledFoodSubstancesUncertainty(result.IndividualEffectsByModelledFoodSubstance);
                    subHeader.SaveSummarySection(section);
                }
                subHeader = header.GetSubSectionHeader<ExposureHazardRatioModelledFoodSubstanceSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as ExposureHazardRatioModelledFoodSubstanceSection;
                    section.SummarizeModelledFoodSubstancesUncertainty(result.IndividualEffectsByModelledFoodSubstance);
                    subHeader.SaveSummarySection(section);
                }
            }
        }

        private void summarizeRiskDistributionBySubstances(
            ProjectDto project,
            RisksActionResult result,
            ICollection<Compound> activeSubstances,
            SectionHeader header,
            int subOrder
        ) {
            var substancesOverViewSection = new SubstancesOverviewSection() {
                SectionLabel = getSectionLabel(RisksSections.RisksDistributionsBySubstanceSection)
            };
            var sectionTitle = "Risks distributions by substances";
            var subHeader = header.AddSubSectionHeaderFor(substancesOverViewSection, sectionTitle, subOrder++);
            substancesOverViewSection.Summarize(
                subHeader,
                result.ExposureTargets,
                result.IndividualEffectsBySubstanceCollections,
                activeSubstances,
                project.RisksSettings.ConfidenceInterval,
                project.RisksSettings.ThresholdMarginOfExposure,
                project.RisksSettings.RiskMetricType,
                project.RisksSettings.IsInverseDistribution,
                project.OutputDetailSettings.SelectedPercentiles
             );
            subHeader.SaveSummarySection(substancesOverViewSection);
        }

        private void summarizeRiskByModelledFood(
            Dictionary<Food, List<IndividualEffect>> individualEffectsPerModelledFood,
            ProjectDto project,
            SectionHeader header,
            int subOrder
        ) {
            if (project.RisksSettings.RiskMetricType == RiskMetricType.MarginOfExposure) {
                var section = new HazardExposureRatioModelledFoodSection() {
                    SectionLabel = getSectionLabel(RisksSections.RisksByModelledFoodSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(section, "Risks by modelled food", subOrder++);
                section.SummarizeRiskByFoods(
                   individualEffectsPerModelledFood,
                   project.OutputDetailSettings.LowerPercentage,
                   project.OutputDetailSettings.UpperPercentage,
                   project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                   project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                   project.RisksSettings.IsInverseDistribution
                );
                subHeader.SaveSummarySection(section);

            }
            if (project.RisksSettings.RiskMetricType == RiskMetricType.HazardIndex) {
                var section = new ExposureHazardRatioModelledFoodSection() {
                    SectionLabel = getSectionLabel(RisksSections.RisksByModelledFoodSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(section, "Risks by modelled food", subOrder++);
                section.SummarizeRiskByFoods(
                   individualEffectsPerModelledFood,
                   project.OutputDetailSettings.LowerPercentage,
                   project.OutputDetailSettings.UpperPercentage,
                   project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                   project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                   project.RisksSettings.IsInverseDistribution
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private void summarizeModelledFoodsAtRisk(
            Dictionary<Food, List<IndividualEffect>> individualEffectsPerModelledFood,
            int numberOfCumulativeIndividualEffects,
            ProjectDto project,
            SectionHeader header,
            int subOrder
        ) {
            var section = new ModelledFoodsAtRiskSection() {
                SectionLabel = getSectionLabel(RisksSections.ModelledFoodAtRiskSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(section, "Percentage at risk by modelled food", subOrder++);
            section.SummarizeModelledFoodsAtRisk(
               individualEffectsPerModelledFood,
               numberOfCumulativeIndividualEffects,
               project.RisksSettings.HealthEffectType,
               project.RisksSettings.RiskMetricType,
               project.RisksSettings.ThresholdMarginOfExposure
            );
            subHeader.SaveSummarySection(section);
        }

        private void summarizeRiskBySubstance(
            Dictionary<Compound, List<IndividualEffect>> individualEffectsBySubstance,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipsProbabilities,
            ProjectDto project,
            SectionHeader header,
            int subOrder
        ) {
            if (project.RisksSettings.RiskMetricType == RiskMetricType.MarginOfExposure) {
                var section = new HazardExposureRatioSubstanceSection() {
                    SectionLabel = getSectionLabel(RisksSections.RisksBySubstanceSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(section, "Risks by substance", subOrder++);
                section.SummarizeRiskBySubstances(
                   individualEffectsBySubstance,
                   relativePotencyFactors,
                   membershipsProbabilities,
                   project.OutputDetailSettings.LowerPercentage,
                   project.OutputDetailSettings.UpperPercentage,
                   project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                   project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                   project.RisksSettings.IsInverseDistribution
                );
                subHeader.SaveSummarySection(section);
            } else if (project.RisksSettings.RiskMetricType == RiskMetricType.HazardIndex) {
                var section = new ExposureHazardRatioSubstanceSection() {
                    SectionLabel = getSectionLabel(RisksSections.RisksBySubstanceSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(section, "Risks by substance", subOrder++);
                section.SummarizeRiskBySubstances(
                   individualEffectsBySubstance,
                   relativePotencyFactors,
                   membershipsProbabilities,
                   project.OutputDetailSettings.LowerPercentage,
                   project.OutputDetailSettings.UpperPercentage,
                   project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                   project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                   project.RisksSettings.IsInverseDistribution
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private void summarizeSubstancesAtRisk(
           Dictionary<Compound, List<IndividualEffect>> individualEffectsBySubstance,
           int numberOfCumulativeIndividualEffects,
           ProjectDto project,
           SectionHeader header,
           int subOrder
       ) {
            var section = new SubstancesAtRiskSection() {
                SectionLabel = getSectionLabel(RisksSections.SubstanceAtRiskSection)
            };
            SectionHeader subHeader = header.AddSubSectionHeaderFor(section, "Percentage at risk by substance", subOrder++);
            section.SummarizeSubstancesAtRisk(
               individualEffectsBySubstance,
               numberOfCumulativeIndividualEffects,
               project.RisksSettings.HealthEffectType,
               project.RisksSettings.RiskMetricType,
               project.RisksSettings.ThresholdMarginOfExposure
            );
            subHeader.SaveSummarySection(section);
        }

        private void summarizeRiskByModelledFoodSubstance(
          IDictionary<(Food, Compound), List<IndividualEffect>> individualEffectsPerModelledFoodSubstance,
          ProjectDto project,
          SectionHeader header,
          int subOrder
      ) {
            if (project.RisksSettings.RiskMetricType == RiskMetricType.MarginOfExposure) {
                var section = new HazardExposureRatioModelledFoodSubstanceSection() {
                    SectionLabel = getSectionLabel(RisksSections.RisksByModelledFoodSubstanceSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(section, "Risks by modelled food x substance", subOrder++);
                section.SummarizeRiskByModelledFoodSubstances(
                   individualEffectsPerModelledFoodSubstance,
                   project.OutputDetailSettings.LowerPercentage,
                   project.OutputDetailSettings.UpperPercentage,
                   project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                   project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                   project.RisksSettings.IsInverseDistribution
                );
                subHeader.SaveSummarySection(section);

            }
            if (project.RisksSettings.RiskMetricType == RiskMetricType.HazardIndex) {
                var section = new ExposureHazardRatioModelledFoodSubstanceSection() {
                    SectionLabel = getSectionLabel(RisksSections.RisksByModelledFoodSubstanceSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(section, "Risks by modelled food x substance", subOrder++);
                section.SummarizeRiskByModelledFoodSubstances(
                   individualEffectsPerModelledFoodSubstance,
                   project.OutputDetailSettings.LowerPercentage,
                   project.OutputDetailSettings.UpperPercentage,
                   project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                   project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                   project.RisksSettings.IsInverseDistribution
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private void summarizeModelledFoodSubstancesAtRisk(
           IDictionary<(Food, Compound), List<IndividualEffect>> individualEffectsPerModelledFoodSubstance,
           int numberOfCumulativeIndividualEffects,
           ProjectDto project,
           SectionHeader header,
           int subOrder
       ) {
            var section = new ModelledFoodSubstancesAtRiskSection() {
                SectionLabel = getSectionLabel(RisksSections.ModelledFoodSubstanceAtRiskSection)
            };
            SectionHeader subHeader = header.AddSubSectionHeaderFor(section, "Percentage at risk by modelled food x substance", subOrder++);
            section.SummarizeModelledFoodSubstancesAtRisk(
               individualEffectsPerModelledFoodSubstance,
               numberOfCumulativeIndividualEffects,
               project.RisksSettings.HealthEffectType,
               project.RisksSettings.RiskMetricType,
               project.RisksSettings.ThresholdMarginOfExposure
            );
            subHeader.SaveSummarySection(section);
        }

        private void summarizeRiskDistribution(
            List<IndividualEffect> individualEffects,
            IHazardCharacterisationModel referenceDose,
            TargetUnit targetUnit,
            Compound substance,
            Effect selectedEffect,
            bool isCumulative,
            ProjectDto project,
            SectionHeader header,
            int subOrder
        ) {
            SectionHeader subHeader = null;
            if (individualEffects != null) {
                subHeader = header.AddEmptySubSectionHeader(
                    "Risks distribution",
                    subOrder++,
                    getSectionLabel(RisksSections.RisksDistributionSection)
                );
                if (project.RisksSettings.RiskMetricType == RiskMetricType.MarginOfExposure) {
                    var graphSection = new HazardExposureRatioDistributionSection();
                    var sub2Header = subHeader.AddSubSectionHeaderFor(graphSection, "Graphs", subOrder++);
                    graphSection.Summarize(
                        project.RisksSettings.ConfidenceInterval,
                        project.RisksSettings.ThresholdMarginOfExposure,
                        project.RisksSettings.IsInverseDistribution,
                        individualEffects
                    );
                    sub2Header.SaveSummarySection(graphSection);

                    var percentileSection = new HazardExposureRatioPercentileSection();
                    sub2Header = subHeader.AddSubSectionHeaderFor(percentileSection, "Percentiles", subOrder++);
                    percentileSection.Summarize(
                        individualEffects,
                        project.OutputDetailSettings.SelectedPercentiles
                            .Select(c => 100 - c).Reverse().ToList(),
                        referenceDose,
                        targetUnit,
                        project.RisksSettings.RiskMetricCalculationType,
                        project.RisksSettings.IsInverseDistribution
                    );
                    sub2Header.SaveSummarySection(percentileSection);
                }

                if (project.RisksSettings.RiskMetricType == RiskMetricType.HazardIndex) {
                    var graphSection = new ExposureHazardRatioDistributionSection();
                    var sub2Header = subHeader.AddSubSectionHeaderFor(graphSection, "Graphs", subOrder++);
                    graphSection.Summarize(
                        project.RisksSettings.ConfidenceInterval,
                        project.RisksSettings.ThresholdMarginOfExposure,
                        project.RisksSettings.IsInverseDistribution,
                        individualEffects
                    );
                    sub2Header.SaveSummarySection(graphSection);

                    var percentileSection = new ExposureHazardRatioPercentileSection();
                    sub2Header = subHeader.AddSubSectionHeaderFor(percentileSection, "Percentiles", subOrder++);
                    percentileSection.Summarize(
                        individualEffects,
                        project.OutputDetailSettings.SelectedPercentiles,
                        referenceDose,
                        targetUnit,
                        project.RisksSettings.RiskMetricCalculationType,
                        project.RisksSettings.IsInverseDistribution
                    );
                    sub2Header.SaveSummarySection(percentileSection);
                }

                summarizeRiskBySingleSubstance(
                    individualEffects,
                    targetUnit,
                    substance,
                    selectedEffect,
                    project.RisksSettings.RiskMetricType,
                    project.RisksSettings.RiskMetricCalculationType,
                    referenceDose,
                    project.RisksSettings.ConfidenceInterval,
                    project.RisksSettings.LeftMargin,
                    project.RisksSettings.RightMargin,
                    project.RisksSettings.ThresholdMarginOfExposure,
                    project.RisksSettings.IsInverseDistribution,
                    isCumulative,
                    subOrder,
                    subHeader
                );
            }
        }

        private void summarizeRiskDistributionBySubstancesUncertainty(
               ICollection<ExposureTarget> targets,
               List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)> individualEffectsBySubstanceCollections,
               ICollection<Compound> activeSubstances,
               double uncertaintyLowerLimit,
               double uncertaintyUpperLimit,
               RiskMetricType riskMetricType,
               bool isInverseDistribution,
               SectionHeader header
           ) {
            var subHeader = header.GetSubSectionHeader<SubstancesOverviewSection>();
            if (subHeader != null) {
                var section = subHeader.GetSummarySection() as SubstancesOverviewSection;
                section.SummarizeUncertain(
                    subHeader,
                    targets,
                    activeSubstances,
                    individualEffectsBySubstanceCollections,
                    riskMetricType,
                    isInverseDistribution,
                    uncertaintyLowerLimit,
                    uncertaintyUpperLimit
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private void summarizeRiskDistributionUncertainty(
            Compound substance,
            List<IndividualEffect> individualEffects,
            RiskMetricType riskMetricType,
            RiskMetricCalculationType riskMetricCalculationType,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isInverseDistribution,
            bool isCumulative,
            SectionHeader header
        ) {
            if (individualEffects != null && riskMetricType == RiskMetricType.MarginOfExposure) {
                var subHeader = header.GetSubSectionHeader<HazardExposureRatioDistributionSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as HazardExposureRatioDistributionSection;
                    section.SummarizeUncertainty(
                        individualEffects,
                        isInverseDistribution,
                        uncertaintyLowerBound,
                        uncertaintyUpperBound
                    );
                    subHeader.SaveSummarySection(section);
                }
            }

            if (individualEffects != null && riskMetricType == RiskMetricType.HazardIndex) {
                var subHeader = header.GetSubSectionHeader<ExposureHazardRatioDistributionSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as ExposureHazardRatioDistributionSection;
                    section.SummarizeUncertainty(
                        individualEffects,
                        isInverseDistribution,
                        uncertaintyLowerBound,
                        uncertaintyUpperBound
                    );
                    subHeader.SaveSummarySection(section);
                }
            }

            if (individualEffects != null && riskMetricType == RiskMetricType.MarginOfExposure) {
                var subHeader = header.GetSubSectionHeader<HazardExposureRatioPercentileSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as HazardExposureRatioPercentileSection;
                    section.SummarizeUncertainty(
                        individualEffects,
                        isInverseDistribution,
                        uncertaintyLowerBound,
                        uncertaintyUpperBound
                    );
                    subHeader.SaveSummarySection(section);
                }
            }

            if (individualEffects != null && riskMetricType == RiskMetricType.HazardIndex) {
                var subHeader = header.GetSubSectionHeader<ExposureHazardRatioPercentileSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as ExposureHazardRatioPercentileSection;
                    section.SummarizeUncertainty(
                        individualEffects,
                        isInverseDistribution,
                        uncertaintyLowerBound,
                        uncertaintyUpperBound
                    );
                    subHeader.SaveSummarySection(section);
                }
            }

            if (individualEffects != null && riskMetricType == RiskMetricType.MarginOfExposure) {
                var subHeader = header.GetSubSectionHeader<SingleHazardExposureRatioSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as SingleHazardExposureRatioSection;
                    section.SummarizeUncertain(
                        substance,
                        individualEffects,
                        riskMetricCalculationType,
                        isInverseDistribution,
                        uncertaintyLowerBound,
                        uncertaintyUpperBound,
                        isCumulative
                    );
                    subHeader.SaveSummarySection(section);
                }
            }

            if (individualEffects != null && riskMetricType == RiskMetricType.HazardIndex) {
                var subHeader = header.GetSubSectionHeader<SingleExposureHazardRatioSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as SingleExposureHazardRatioSection;
                    section.SummarizeUncertain(
                        substance,
                        individualEffects,
                        riskMetricCalculationType,
                        isInverseDistribution,
                        uncertaintyLowerBound,
                        uncertaintyUpperBound,
                        isCumulative
                    );
                    subHeader.SaveSummarySection(section);
                }
            }
        }

        private void summarizeHazardDistribution(
            List<IndividualEffect> individualEffects,
            IHazardCharacterisationModel referenceDose,
            IDictionary<(Effect, Compound), IntraSpeciesFactorModel> intraSpeciesConversionModels,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            double[] selectedPercentiles,
            SectionHeader header,
            int subOrder
        ) {
            if (individualEffects?.Any() ?? false) {
                var subHeader = header.AddEmptySubSectionHeader("Hazard distribution", subOrder++, getSectionLabel(RisksSections.HazardDistributionSection));
                var graphSection = new HazardDistributionSection();
                var subSubHeader = subHeader.AddSubSectionHeaderFor(graphSection, "Graphs", 1);
                graphSection.Summarize(
                    uncertaintyLowerBound,
                    uncertaintyUpperBound,
                    individualEffects,
                    referenceDose,
                    intraSpeciesConversionModels
                );
                subSubHeader.SaveSummarySection(graphSection);

                var percentileSection = new HazardPercentileSection();
                subSubHeader = subHeader.AddSubSectionHeaderFor(percentileSection, "Percentiles", 2);
                percentileSection.Summarize(individualEffects, selectedPercentiles, referenceDose);
                subSubHeader.SaveSummarySection(percentileSection);
            }
        }

        private void summarizeHazardDistributionUncertainty(
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            List<IndividualEffect> individualEffects,
            SectionHeader header
        ) {
            var subHeader = header.GetSubSectionHeader<HazardDistributionSection>();
            if (subHeader != null) {
                var distributionSection = subHeader.GetSummarySection() as HazardDistributionSection;
                distributionSection.SummarizeUncertainty(individualEffects);
                subHeader.SaveSummarySection(distributionSection);
            }
            subHeader = header.GetSubSectionHeader<HazardPercentileSection>();
            if (subHeader != null) {
                var percentilesSection = subHeader.GetSummarySection() as HazardPercentileSection;
                percentilesSection.SummarizeUncertainty(
                    individualEffects,
                    uncertaintyLowerBound,
                    uncertaintyUpperBound
                );
            }
        }
    }
}
