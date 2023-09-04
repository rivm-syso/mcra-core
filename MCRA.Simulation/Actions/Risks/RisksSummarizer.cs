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

            var exposureModel = project.EffectSettings.TargetDoseLevelType == TargetLevelType.Internal
                ? ActionType.TargetExposures
                : ActionType.DietaryExposures;

            var isHazardCharacterisationDistribution = data.HazardCharacterisationModels?
                .Any(r => !double.IsNaN(r.Value.GeometricStandardDeviation)) ?? false;

            outputSummary.Summarize(
                project.AssessmentSettings.ExposureType,
                project.EffectSettings.TargetDoseLevelType,
                project.EffectModelSettings.RiskMetricType,
                project.EffectModelSettings.RiskMetricCalculationType,
                exposureModel,
                isHazardCharacterisationDistribution
            );

            var subHeader = header.AddSubSectionHeaderFor(outputSummary, ActionType.GetDisplayName(), order);
            subHeader.Units = collectUnits(project, data);
            subHeader.SaveSummarySection(outputSummary);
            var subOrder = 0;

            // Total distribution section
            var onlyCumulativeOutput = !data.ActiveSubstances.All(r => data.HazardCharacterisationModels.ContainsKey(r)) && data.HazardCharacterisationModels.ContainsKey(data.ReferenceSubstance);
            var isCumulative = project.AssessmentSettings.MultipleSubstances && project.EffectModelSettings.CumulativeRisk;
            var referenceSubstance = data.ActiveSubstances.Count == 1 ? data.ActiveSubstances.First() : data.ReferenceSubstance;
            if (outputSettings.ShouldSummarize(RisksSections.RisksDistributionSection)) {
                summarizeRiskDistribution(
                    project.AssessmentSettings.MultipleSubstances
                        ? result.IndividualEffects
                        : result.IndividualEffectsBySubstance[referenceSubstance],
                    result.ReferenceDose,
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
                && data.ActiveSubstances.Count > 1 && !onlyCumulativeOutput) {
                summarizeRiskBySubstanceOverview(
                    result.IndividualEffectsBySubstance,
                    result.IndividualEffects,
                    data.ActiveSubstances,
                    data.SelectedEffect,
                    project.EffectModelSettings.RiskMetricType,
                    project.EffectModelSettings.RiskMetricCalculationType,
                    project.EffectModelSettings.ConfidenceInterval,
                    project.EffectModelSettings.HealthEffectType,
                    project.EffectModelSettings.LeftMargin,
                    project.EffectModelSettings.RightMargin,
                    project.EffectModelSettings.ThresholdMarginOfExposure,
                    project.EffectModelSettings.IsInverseDistribution,
                    project.EffectSettings.UseIntraSpeciesConversionFactors,
                    isCumulative,
                    onlyCumulativeOutput,
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
                    result.IndividualEffectsBySubstance,
                    result.IndividualEffects,
                    project.EffectModelSettings.HealthEffectType,
                    data.ActiveSubstances,
                    referenceSubstance,
                    data.HazardCharacterisationModels,
                    project.EffectModelSettings.RiskMetricType,
                    project.EffectModelSettings.RiskMetricCalculationType,
                    project.EffectModelSettings.ConfidenceInterval,
                    project.EffectModelSettings.ThresholdMarginOfExposure,
                    project.EffectModelSettings.NumberOfLabels,
                    project.EffectModelSettings.NumberOfSubstances,
                    project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                    project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                    onlyCumulativeOutput
                );
                subSubHeader.SaveSummarySection(section);
            }

            // Distributions by substance
            if (result.IndividualEffectsBySubstance != null
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
                && (!project.AssessmentSettings.MultipleSubstances 
                    || (isCumulative && project.EffectModelSettings.RiskMetricCalculationType == RiskMetricCalculationType.RPFWeighted))
                && (data.HazardCharacterisationModels?.TryGetValue(referenceSubstance ?? data.ActiveSubstances.First(), out var referenceHazardCharacterisation) ?? false)
                && !double.IsNaN(referenceHazardCharacterisation.GeometricStandardDeviation)
                && outputSettings.ShouldSummarize(RisksSections.HazardDistributionSection)
            ) {
                summarizeHazardDistribution(
                    result.IndividualEffects,
                    project.EffectModelSettings.HealthEffectType,
                    referenceSubstance,
                    data.HazardCharacterisationModels,
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
            if (project.EffectModelSettings.IsEAD && result.IndividualEffects != null) {
                var section = new EquivalentAnimalDoseSection();
                var subSubHeader = header.AddSubSectionHeaderFor(section, "Equivalent animal dose (EAD)", subOrder++);
                section.Summarize(
                    result.IndividualEffects,
                    project.EffectModelSettings.HealthEffectType,
                    result.ReferenceDose,
                    referenceSubstance,
                    data.HazardCharacterisationModels,
                    project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                    project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                    project.OutputDetailSettings.SelectedPercentiles);
                subSubHeader.SaveSummarySection(section);
            }

            // Forward effect calculation (predicted responses / health effects)
            if (project.EffectModelSettings.IsEAD && result.IndividualEffects != null) {
                var section = new PredictedHealthEffectSection();
                var subSubHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Predicted health effects",
                    subOrder++
                );
                // Note: when adding a reference dose, then make sure that it is based on the HC from the index substance
                // (don't use the obsolete reference dose from dietary exposures)
                section.Summarize(
                    result.IndividualEffects,
                    project.EffectModelSettings.HealthEffectType,
                    referenceSubstance,
                    data.HazardCharacterisationModels,
                    project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                    project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                    project.OutputDetailSettings.SelectedPercentiles);
                subSubHeader.SaveSummarySection(section);
            }

            // Risks by food/substance
            if (result.IndividualEffects != null
                && project.EffectSettings.TargetDoseLevelType == TargetLevelType.External
                && project.EffectModelSettings.CalculateRisksByFood
                && project.EffectModelSettings.RiskMetricCalculationType == RiskMetricCalculationType.RPFWeighted
            ) {
                summarizeRisks(
                    result.IndividualEffects,
                    result.IndividualEffectsByModelledFood,
                    result.IndividualEffectsBySubstance,
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
        /// Summarizes full tables with risks and percentages at risk for modelled foods, substances and combinations
        /// </summary>
        /// <param name="individualEffects"></param>
        /// <param name="individualEffectsByModelledFood"></param>
        /// <param name="individualEffectsBySubstance"></param>
        /// <param name="outputSettings"></param>
        /// <param name="project"></param>
        /// <param name="header"></param>
        /// <param name="subOrder"></param>
        private void summarizeRisks(
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

            if (project.EffectSettings.TargetDoseLevelType == TargetLevelType.External
                && project.EffectModelSettings.CalculateRisksByFood
                && outputSettings.ShouldSummarize(RisksSections.RisksByModelledFoodSection)
            ) {
                summarizeRiskByModelledFood(
                    individualEffectsByModelledFood,
                    project,
                    subHeader,
                    subOrder
                );
            }

            var hasThresholdExceedances = false;
            if (project.EffectModelSettings.RiskMetricType == RiskMetricType.MarginOfExposure) {
                hasThresholdExceedances = individualEffects
                    .Any(c => c.ExposureConcentration > 0 && c.HazardExposureRatio <= project.EffectModelSettings.ThresholdMarginOfExposure);
            } else {
                hasThresholdExceedances = individualEffects
                    .Any(c => c.ExposureHazardRatio >= project.EffectModelSettings.ThresholdMarginOfExposure);
            }

            if (hasThresholdExceedances) {
                if (project.EffectSettings.TargetDoseLevelType == TargetLevelType.External
                    && project.EffectModelSettings.CalculateRisksByFood
                    && outputSettings.ShouldSummarize(RisksSections.ModelledFoodAtRiskSection)
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
                && project.EffectModelSettings.CalculateRisksByFood
                && outputSettings.ShouldSummarize(RisksSections.RisksBySubstanceSection)
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
                    && project.EffectModelSettings.CalculateRisksByFood
                    && outputSettings.ShouldSummarize(RisksSections.SubstanceAtRiskSection)
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

            if (project.EffectSettings.TargetDoseLevelType == TargetLevelType.External
                && project.EffectModelSettings.CalculateRisksByFood
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
                    && project.EffectModelSettings.CalculateRisksByFood
                    && outputSettings.ShouldSummarize(RisksSections.ModelledFoodSubstanceAtRiskSection)
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
        /// Safety charts for multiple substances
        /// </summary>
        /// <param name="individualEffectsBySubstance"></param>
        /// <param name="individualEffects"></param>
        /// <param name="substances"></param>
        /// <param name="focalEffect"></param>
        /// <param name="riskMetric"></param>
        /// <param name="confidenceInterval"></param>
        /// <param name="healthEffectType"></param>
        /// <param name="leftMargin"></param>
        /// <param name="rightMargin"></param>
        /// <param name="threshold"></param>
        /// <param name="isInverseDistribution"></param>
        /// <param name="isCumulative"></param>
        /// <param name="onlyCumulativeOutput"></param>
        /// <param name="subOrder"></param>
        /// <param name="header"></param>
        private void summarizeRiskBySubstanceOverview(
            Dictionary<Compound, List<IndividualEffect>> individualEffectsBySubstance,
            List<IndividualEffect> individualEffects,
            ICollection<Compound> substances,
            Effect focalEffect,
            RiskMetricType riskMetric,
            RiskMetricCalculationType riskMetricCalculationType,
            double confidenceInterval,
            HealthEffectType healthEffectType,
            double leftMargin,
            double rightMargin,
            double threshold,
            bool isInverseDistribution,
            bool useIntraSpeciesFactors,
            bool isCumulative,
            bool onlyCumulativeOutput,
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
                section.SummarizeMultipleSubstances(
                    individualEffectsBySubstance,
                    individualEffects,
                    substances,
                    focalEffect,
                    threshold,
                    confidenceInterval,
                    healthEffectType,
                    riskMetric,
                    riskMetricCalculationType,
                    leftMargin,
                    rightMargin,
                    isInverseDistribution,
                    isCumulative,
                    onlyCumulativeOutput
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
                section.SummarizeMultipleSubstances(
                    individualEffectsBySubstance,
                    individualEffects,
                    substances,
                    focalEffect,
                    riskMetricCalculationType,
                    riskMetric,
                    confidenceInterval,
                    threshold,
                    healthEffectType,
                    leftMargin,
                    rightMargin,
                    isInverseDistribution,
                    useIntraSpeciesFactors,
                    isCumulative,
                    onlyCumulativeOutput
                );
                subHeader.SaveSummarySection(section);
            }

            if (riskMetric == RiskMetricType.HazardIndex && isCumulative) {
                var section = new CumulativeExposureHazardRatioSection() {
                    SectionLabel = getSectionLabel(RisksSections.RisksBySubstanceSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Percentile risk ratio sums",
                    subOrder++
                );
                section.SummarizeMultipleSubstances(
                    individualEffectsBySubstance,
                    individualEffects,
                    substances,
                    focalEffect,
                    riskMetricCalculationType,
                    riskMetric,
                    confidenceInterval,
                    threshold,
                    healthEffectType,
                    leftMargin,
                    rightMargin,
                    isInverseDistribution,
                    useIntraSpeciesFactors,
                    isCumulative,
                    onlyCumulativeOutput
                );
                subHeader.SaveSummarySection(section);
            }
        }

        /// <summary>
        /// Safety chart for single substance
        /// </summary>
        /// <param name="individualEffects"></param>
        /// <param name="substance"></param>
        /// <param name="focalEffect"></param>
        /// <param name="riskMetric"></param>
        /// <param name="confidenceInterval"></param>
        /// <param name="healthEffectType"></param>
        /// <param name="leftMargin"></param>
        /// <param name="rightMargin"></param>
        /// <param name="threshold"></param>
        /// <param name="isInverseDistribution"></param>
        /// <param name="isCumulative"></param>
        /// <param name="subOrder"></param>
        /// <param name="header"></param>
        private void summarizeRiskBySingleSubstance(
            List<IndividualEffect> individualEffects,
            Compound substance,
            Effect focalEffect,
            RiskMetricType riskMetric,
            RiskMetricCalculationType riskMetricCalculationType,
            double confidenceInterval,
            HealthEffectType healthEffectType,
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
                section.SummarizeSingleSubstance(
                    individualEffects,
                    substance,
                    focalEffect,
                    threshold,
                    confidenceInterval,
                    healthEffectType,
                    riskMetric,
                    riskMetricCalculationType,
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
                section.SummarizeSingleSubstance(
                    individualEffects,
                    substance,
                    focalEffect,
                    confidenceInterval,
                    threshold,
                    healthEffectType,
                    riskMetricCalculationType,
                    riskMetric,
                    leftMargin,
                    rightMargin,
                    isInverseDistribution,
                    isCumulative
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private static List<ActionSummaryUnitRecord> collectUnits(ProjectDto project, ActionData data) {
            var result = new List<ActionSummaryUnitRecord>();
            //specific for risks bar
            var lowerPercentage = (100 - project.EffectModelSettings.ConfidenceInterval) / 2;
            var upperPercentage = 100 - (100 - project.EffectModelSettings.ConfidenceInterval) / 2;
            var printOption = project.EffectSettings.TargetDoseLevelType == TargetLevelType.External 
                ? TargetUnit.DisplayOption.AppendBiologicalMatrix 
                : TargetUnit.DisplayOption.UnitOnly;
            result.Add(new ActionSummaryUnitRecord("TargetDoseUnit", data.HazardCharacterisationsUnit.GetShortDisplayName(printOption)));
            result.Add(new ActionSummaryUnitRecord("IntakeUnit", data.HazardCharacterisationsUnit.GetShortDisplayName(printOption)));
            //uncertainty
            result.Add(new ActionSummaryUnitRecord("LowerBound", $"p{project.UncertaintyAnalysisSettings.UncertaintyLowerBound:#0.##}"));
            result.Add(new ActionSummaryUnitRecord("UpperBound", $"p{project.UncertaintyAnalysisSettings.UncertaintyUpperBound:#0.##}"));
            //quartiles lower and upper, variabiliteit
            result.Add(new ActionSummaryUnitRecord("LowerPercentage", $"p{project.OutputDetailSettings.LowerPercentage:#0.##}"));
            result.Add(new ActionSummaryUnitRecord("UpperPercentage", $"p{project.OutputDetailSettings.UpperPercentage:#0.##}"));
            result.Add(new ActionSummaryUnitRecord("LowerConfidenceBound", $"p{lowerPercentage:#0.##}"));
            result.Add(new ActionSummaryUnitRecord("UpperConfidenceBound", $"p{upperPercentage:#0.##}"));
            result.Add(new ActionSummaryUnitRecord("RiskMetric", project.EffectModelSettings.RiskMetricType.GetDisplayName()));
            result.Add(new ActionSummaryUnitRecord("RiskMetricShort", project.EffectModelSettings.RiskMetricType.GetShortDisplayName()));
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
            var isCumulative = project.AssessmentSettings.MultipleSubstances && project.EffectModelSettings.CumulativeRisk;
            var referenceSubstance = data.ActiveSubstances.Count == 1 ? data.ActiveSubstances.First() : data.ReferenceSubstance;
            if (outputSettings.ShouldSummarize(RisksSections.RisksDistributionSection)) {
                if (project.AssessmentSettings.MultipleSubstances) {
                    summarizeRiskDistributionUncertainty(
                        referenceSubstance,
                        result.IndividualEffects,
                        project.EffectModelSettings.RiskMetricType,
                        project.EffectModelSettings.RiskMetricCalculationType,
                        project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                        project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                        project.EffectModelSettings.IsInverseDistribution,
                        isCumulative,
                        header
                    );
                } else {
                    summarizeRiskDistributionUncertainty(
                        referenceSubstance,
                        result.IndividualEffectsBySubstance[referenceSubstance],
                        project.EffectModelSettings.RiskMetricType,
                        project.EffectModelSettings.RiskMetricCalculationType,
                        project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                        project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                        project.EffectModelSettings.IsInverseDistribution,
                        isCumulative,
                        header
                    );
                }
            }

            // Risks by substance (overview)
            if (project.EffectModelSettings.RiskMetricType == RiskMetricType.HazardIndex) {
                subHeader = header.GetSubSectionHeader<MultipleExposureHazardRatioSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as MultipleExposureHazardRatioSection;
                    section.SummarizeMultipleSubstancesUncertainty(
                        data.ActiveSubstances,
                        result.IndividualEffectsBySubstance,
                        result.IndividualEffects,
                        project.EffectModelSettings.IsInverseDistribution,
                        project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                        project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                        isCumulative);
                    subHeader.SaveSummarySection(section);
                }
            } else if (project.EffectModelSettings.RiskMetricType == RiskMetricType.MarginOfExposure) {
                subHeader = header.GetSubSectionHeader<MultipleHazardExposureRatioSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as MultipleHazardExposureRatioSection;
                    section.SummarizeMultipleSubstancesUncertainty(
                        data.ActiveSubstances,
                        result.IndividualEffectsBySubstance,
                        result.IndividualEffects,
                        project.EffectModelSettings.RiskMetricCalculationType,
                        project.EffectModelSettings.IsInverseDistribution,
                        project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                        project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                        isCumulative);
                    subHeader.SaveSummarySection(section);
                }
            }

            // Sum of substance risk ratios (at percentile)
            if (project.EffectModelSettings.RiskMetricType == RiskMetricType.HazardIndex && isCumulative) {
                subHeader = header.GetSubSectionHeader<CumulativeExposureHazardRatioSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as CumulativeExposureHazardRatioSection;
                    section.SummarizeUncertain(
                        data.ActiveSubstances,
                        result.IndividualEffectsBySubstance,
                        result.IndividualEffects,
                        project.EffectModelSettings.IsInverseDistribution,
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
                var onlyCumulativeOutput = !data.ActiveSubstances
                    .All(r => data.HazardCharacterisationModels.ContainsKey(r)) && data.HazardCharacterisationModels.ContainsKey(data.ReferenceSubstance);

                var section = subHeader.GetSummarySection() as HazardExposureSection;
                section.SummarizeUncertainty(
                    result.IndividualEffectsBySubstance,
                    result.IndividualEffects,
                    data.HazardCharacterisationModels,
                    data.ActiveSubstances,
                    referenceSubstance,
                    project.EffectModelSettings.RiskMetricType,
                    project.EffectModelSettings.RiskMetricCalculationType,
                    project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                    project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                    onlyCumulativeOutput
                );
                subHeader.SaveSummarySection(section);
            }

            // Distributions by substance
            summarizeRiskDistributionBySubstancesUncertainty(
                result.IndividualEffectsBySubstance,
                data.ActiveSubstances,
                project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                project.EffectModelSettings.RiskMetricType,
                project.EffectModelSettings.IsInverseDistribution,
                header
            );

            // Hazard distribution
            if (result.IndividualEffects != null
                && (!project.AssessmentSettings.MultipleSubstances
                    || (isCumulative && project.EffectModelSettings.RiskMetricCalculationType == RiskMetricCalculationType.RPFWeighted))
                && (data.HazardCharacterisationModels?.TryGetValue(referenceSubstance ?? data.ActiveSubstances.First(), out var referenceHazardCharacterisation) ?? false)
                && !double.IsNaN(referenceHazardCharacterisation.GeometricStandardDeviation)
                && outputSettings.ShouldSummarize(RisksSections.HazardDistributionSection)
            ) {
                summarizeHazardDistributionUncertainty(
                    project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                    project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                    result.IndividualEffects,
                    header
                );
            }

            if (project.EffectModelSettings.IsEAD && result.IndividualEffects != null) {
                subHeader = header.GetSubSectionHeader<EquivalentAnimalDoseSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as EquivalentAnimalDoseSection;
                    section.SummarizeUncertainty(result.IndividualEffects);
                    subHeader.SaveSummarySection(section);
                }
            }

            if (project.EffectModelSettings.IsEAD && result.IndividualEffects != null) {
                subHeader = header.GetSubSectionHeader<PredictedHealthEffectSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as PredictedHealthEffectSection;
                    section.SummarizeUncertainty(result.IndividualEffects);
                    subHeader.SaveSummarySection(section);
                }
            }
            if (project.EffectSettings.TargetDoseLevelType == TargetLevelType.External && project.EffectModelSettings.CalculateRisksByFood) {
                subHeader = header.GetSubSectionHeader<HazardExposureRatioModelledFoodSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as HazardExposureRatioModelledFoodSection;
                    section.SummarizeFoodsUncertainty(
                        result.IndividualEffectsByModelledFood);
                    subHeader.SaveSummarySection(section);
                }
                subHeader = header.GetSubSectionHeader<ExposureHazardRatioModelledFoodSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as ExposureHazardRatioModelledFoodSection;
                    section.SummarizeFoodsUncertainty(
                        result.IndividualEffectsByModelledFood);
                    subHeader.SaveSummarySection(section);
                }
            }

            if (project.EffectSettings.TargetDoseLevelType == TargetLevelType.External 
                && project.EffectModelSettings.CalculateRisksByFood
                && project.EffectModelSettings.RiskMetricCalculationType == RiskMetricCalculationType.RPFWeighted
            ) {
                subHeader = header.GetSubSectionHeader<HazardExposureRatioSubstanceSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as HazardExposureRatioSubstanceSection;
                    section.SummarizeSubstancesUncertainty(
                        result.IndividualEffectsBySubstance,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities
                    );
                    subHeader.SaveSummarySection(section);
                }
                subHeader = header.GetSubSectionHeader<ExposureHazardRatioSubstanceSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as ExposureHazardRatioSubstanceSection;
                    section.SummarizeSubstancesUncertainty(
                        result.IndividualEffectsBySubstance,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities
                    );
                    subHeader.SaveSummarySection(section);
                }
            }

            if (project.EffectSettings.TargetDoseLevelType == TargetLevelType.External 
                && project.EffectModelSettings.CalculateRisksByFood 
                && project.EffectModelSettings.RiskMetricCalculationType == RiskMetricCalculationType.RPFWeighted
            ) {
                subHeader = header.GetSubSectionHeader<HazardExposureRatioModelledFoodSubstanceSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as HazardExposureRatioModelledFoodSubstanceSection;
                    section.SummarizeModelledFoodSubstancesUncertainty(
                        result.IndividualEffectsByModelledFoodSubstance);
                    subHeader.SaveSummarySection(section);
                }
                subHeader = header.GetSubSectionHeader<ExposureHazardRatioModelledFoodSubstanceSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as ExposureHazardRatioModelledFoodSubstanceSection;
                    section.SummarizeModelledFoodSubstancesUncertainty(
                        result.IndividualEffectsByModelledFoodSubstance);
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
                result.IndividualEffectsBySubstance,
                activeSubstances,
                project.EffectModelSettings.ConfidenceInterval,
                project.EffectModelSettings.ThresholdMarginOfExposure,
                project.EffectModelSettings.HealthEffectType,
                project.EffectModelSettings.RiskMetricType,
                project.EffectModelSettings.IsInverseDistribution,
                project.EffectModelSettings.RiskMetricCalculationType,
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
            SectionHeader subHeader = null;
            if (project.EffectModelSettings.RiskMetricType == RiskMetricType.MarginOfExposure) {
                var section = new HazardExposureRatioModelledFoodSection() {
                    SectionLabel = getSectionLabel(RisksSections.RisksByModelledFoodSection)
                };
                subHeader = header.AddSubSectionHeaderFor(section, "Risks by modelled food", subOrder++);
                section.SummarizeRiskByFoods(
                   individualEffectsPerModelledFood,
                   project.OutputDetailSettings.LowerPercentage,
                   project.OutputDetailSettings.UpperPercentage,
                   project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                   project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                   project.EffectModelSettings.IsInverseDistribution
                );
                subHeader.SaveSummarySection(section);

            }
            if (project.EffectModelSettings.RiskMetricType == RiskMetricType.HazardIndex) {
                var section = new ExposureHazardRatioModelledFoodSection() {
                    SectionLabel = getSectionLabel(RisksSections.RisksByModelledFoodSection)
                };
                subHeader = header.AddSubSectionHeaderFor(section, "Risks by modelled food", subOrder++);
                section.SummarizeRiskByFoods(
                   individualEffectsPerModelledFood,
                   project.OutputDetailSettings.LowerPercentage,
                   project.OutputDetailSettings.UpperPercentage,
                   project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                   project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                   project.EffectModelSettings.IsInverseDistribution
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
            SectionHeader subHeader = header.AddSubSectionHeaderFor(section, "Percentage at risk by modelled food", subOrder++);
            section.SummarizeModelledFoodsAtRisk(
               individualEffectsPerModelledFood,
               numberOfCumulativeIndividualEffects,
               project.EffectModelSettings.HealthEffectType,
               project.EffectModelSettings.RiskMetricType,
               project.EffectModelSettings.ThresholdMarginOfExposure
            );
            subHeader.SaveSummarySection(section);
        }

        private void summarizeRiskBySubstance(
            Dictionary<Compound, List<IndividualEffect>> individualEffectsPerSubstance,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipsProbabilities,
            ProjectDto project,
            SectionHeader header,
            int subOrder
        ) {
            SectionHeader subHeader = null;
            if (project.EffectModelSettings.RiskMetricType == RiskMetricType.MarginOfExposure) {
                var section = new HazardExposureRatioSubstanceSection() {
                    SectionLabel = getSectionLabel(RisksSections.RisksBySubstanceSection)
                };
                subHeader = header.AddSubSectionHeaderFor(section, "Risks by substance", subOrder++);
                section.SummarizeRiskBySubstances(
                   individualEffectsPerSubstance,
                   relativePotencyFactors,
                   membershipsProbabilities,
                   project.OutputDetailSettings.LowerPercentage,
                   project.OutputDetailSettings.UpperPercentage,
                   project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                   project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                   project.EffectModelSettings.IsInverseDistribution
                );
                subHeader.SaveSummarySection(section);

            }
            if (project.EffectModelSettings.RiskMetricType == RiskMetricType.HazardIndex) {
                var section = new ExposureHazardRatioSubstanceSection() {
                    SectionLabel = getSectionLabel(RisksSections.RisksBySubstanceSection)
                };
                subHeader = header.AddSubSectionHeaderFor(section, "Risks by substance", subOrder++);
                section.SummarizeRiskBySubstances(
                   individualEffectsPerSubstance,
                   relativePotencyFactors,
                   membershipsProbabilities,
                   project.OutputDetailSettings.LowerPercentage,
                   project.OutputDetailSettings.UpperPercentage,
                   project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                   project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                   project.EffectModelSettings.IsInverseDistribution
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private void summarizeSubstancesAtRisk(
           Dictionary<Compound, List<IndividualEffect>> individualEffectsPerSubstance,
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
               individualEffectsPerSubstance,
               numberOfCumulativeIndividualEffects,
               project.EffectModelSettings.HealthEffectType,
               project.EffectModelSettings.RiskMetricType,
               project.EffectModelSettings.ThresholdMarginOfExposure
            );
            subHeader.SaveSummarySection(section);
        }



        private void summarizeRiskByModelledFoodSubstance(
          IDictionary<(Food, Compound), List<IndividualEffect>> individualEffectsPerModelledFoodSubstance,
          ProjectDto project,
          SectionHeader header,
          int subOrder
      ) {
            SectionHeader subHeader = null;
            if (project.EffectModelSettings.RiskMetricType == RiskMetricType.MarginOfExposure) {
                var section = new HazardExposureRatioModelledFoodSubstanceSection() {
                    SectionLabel = getSectionLabel(RisksSections.RisksByModelledFoodSubstanceSection)
                };
                subHeader = header.AddSubSectionHeaderFor(section, "Risks by modelled food x substance", subOrder++);
                section.SummarizeRiskByModelledFoodSubstances(
                   individualEffectsPerModelledFoodSubstance,
                   project.OutputDetailSettings.LowerPercentage,
                   project.OutputDetailSettings.UpperPercentage,
                   project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                   project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                   project.EffectModelSettings.IsInverseDistribution
                );
                subHeader.SaveSummarySection(section);

            }
            if (project.EffectModelSettings.RiskMetricType == RiskMetricType.HazardIndex) {
                var section = new ExposureHazardRatioModelledFoodSubstanceSection() {
                    SectionLabel = getSectionLabel(RisksSections.RisksByModelledFoodSubstanceSection)
                };
                subHeader = header.AddSubSectionHeaderFor(section, "Risks by modelled food x substance", subOrder++);
                section.SummarizeRiskByModelledFoodSubstances(
                   individualEffectsPerModelledFoodSubstance,
                   project.OutputDetailSettings.LowerPercentage,
                   project.OutputDetailSettings.UpperPercentage,
                   project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                   project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                   project.EffectModelSettings.IsInverseDistribution
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
               project.EffectModelSettings.HealthEffectType,
               project.EffectModelSettings.RiskMetricType,
               project.EffectModelSettings.ThresholdMarginOfExposure
            );
            subHeader.SaveSummarySection(section);
        }

        private void summarizeRiskDistribution(
            List<IndividualEffect> individualEffects,
            IHazardCharacterisationModel referenceDose,
            Compound substance,
            Effect selectedEffect,
            bool isCumulative,
            ProjectDto project,
            SectionHeader header,
            int subOrder
        ) {
            SectionHeader subHeader = null;
            if (individualEffects != null) {
                if (project.EffectModelSettings.RiskMetricType == RiskMetricType.MarginOfExposure) {
                    subHeader = header.AddEmptySubSectionHeader(
                        "Risks distribution",
                        subOrder++,
                        getSectionLabel(RisksSections.RisksDistributionSection)
                    );
                    var graphSection = new HazardExposureRatioDistributionSection();
                    var sub2Header = subHeader.AddSubSectionHeaderFor(graphSection, "Graphs", subOrder++);
                    graphSection.Summarize(
                        project.EffectModelSettings.ConfidenceInterval,
                        project.EffectModelSettings.ThresholdMarginOfExposure,
                        project.EffectModelSettings.HealthEffectType,
                        project.EffectModelSettings.IsInverseDistribution,
                        project.OutputDetailSettings.SelectedPercentiles,
                        individualEffects,
                        referenceDose,
                        project.EffectModelSettings.RiskMetricType,
                        project.EffectModelSettings.RiskMetricCalculationType
                    );
                    sub2Header.SaveSummarySection(graphSection);

                    var percentileSection = new HazardExposureRatioPercentileSection();
                    sub2Header = subHeader.AddSubSectionHeaderFor(percentileSection, "Percentiles", subOrder++);
                    percentileSection.Summarize(
                        individualEffects,
                        project.OutputDetailSettings.SelectedPercentiles.Select(c => 100 - c).Reverse().ToList(),
                        referenceDose,
                        project.EffectModelSettings.HealthEffectType,
                        project.EffectModelSettings.RiskMetricCalculationType,
                        project.EffectModelSettings.IsInverseDistribution
                    );
                    sub2Header.SaveSummarySection(percentileSection);
                }

                if (project.EffectModelSettings.RiskMetricType == RiskMetricType.HazardIndex) {
                    subHeader = header.AddEmptySubSectionHeader(
                        "Risks distribution",
                        subOrder++,
                        getSectionLabel(RisksSections.RisksDistributionSection)
                     );

                    var graphSection = new ExposureHazardRatioDistributionSection();
                    var sub2Header = subHeader.AddSubSectionHeaderFor(graphSection, "Graphs", subOrder++);
                    graphSection.Summarize(
                        project.EffectModelSettings.ConfidenceInterval,
                        project.EffectModelSettings.ThresholdMarginOfExposure,
                        project.EffectModelSettings.HealthEffectType,
                        project.EffectModelSettings.IsInverseDistribution,
                        project.OutputDetailSettings.SelectedPercentiles,
                        individualEffects,
                        referenceDose,
                        project.EffectModelSettings.RiskMetricType,
                        project.EffectModelSettings.RiskMetricCalculationType
                    );
                    sub2Header.SaveSummarySection(graphSection);

                    var percentileSection = new ExposureHazardRatioPercentileSection();
                    sub2Header = subHeader.AddSubSectionHeaderFor(percentileSection, "Percentiles", subOrder++);
                    percentileSection.Summarize(
                        individualEffects,
                        project.OutputDetailSettings.SelectedPercentiles,
                        referenceDose,
                        project.EffectModelSettings.HealthEffectType,
                        project.EffectModelSettings.RiskMetricCalculationType,
                        project.EffectModelSettings.IsInverseDistribution
                    );
                    sub2Header.SaveSummarySection(percentileSection);
                }

                summarizeRiskBySingleSubstance(
                    individualEffects,
                    substance,
                    selectedEffect,
                    project.EffectModelSettings.RiskMetricType,
                    project.EffectModelSettings.RiskMetricCalculationType,
                    project.EffectModelSettings.ConfidenceInterval,
                    project.EffectModelSettings.HealthEffectType,
                    project.EffectModelSettings.LeftMargin,
                    project.EffectModelSettings.RightMargin,
                    project.EffectModelSettings.ThresholdMarginOfExposure,
                    project.EffectModelSettings.IsInverseDistribution,
                    isCumulative,
                    subOrder,
                    subHeader
                );
            }
        }


        private void summarizeRiskDistributionBySubstancesUncertainty(
               Dictionary<Compound, List<IndividualEffect>> individualEffectsBySubstance,
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
                    activeSubstances,
                    individualEffectsBySubstance,
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
            if (individualEffects != null && riskMetricType == RiskMetricType.MarginOfExposure) {
                var subHeader = header.GetSubSectionHeader<SingleHazardExposureRatioSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as SingleHazardExposureRatioSection;
                    section.SummarizeSingleSubstanceUncertainty(
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

            if (individualEffects != null && riskMetricType == RiskMetricType.HazardIndex) {
                var subHeader = header.GetSubSectionHeader<SingleExposureHazardRatioSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as SingleExposureHazardRatioSection;
                    section.SummarizeSingleSubstanceUncertainty(
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
            HealthEffectType healthEffectType,
            Compound referenceCompound,
            IDictionary<Compound, IHazardCharacterisationModel> hazardCharacterisations,
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
                    healthEffectType,
                    referenceCompound,
                    individualEffects,
                    referenceDose,
                    hazardCharacterisations,
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
