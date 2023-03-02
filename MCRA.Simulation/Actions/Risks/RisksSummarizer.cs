using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
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

            var isHazardCharacterisationDistribution = data.HazardCharacterisations?
                .Any(r => !double.IsNaN(r.Value.GeometricStandardDeviation)) ?? false;

            var exposureModel = project.EffectSettings.TargetDoseLevelType == TargetLevelType.Internal
                ? ActionType.TargetExposures
                : ActionType.DietaryExposures;
            outputSummary.Summarize(
                project.AssessmentSettings.ExposureType,
                project.EffectSettings.TargetDoseLevelType,
                project.EffectModelSettings.RiskMetricType,
                exposureModel,
                isHazardCharacterisationDistribution
            );

            var subHeader = header.AddSubSectionHeaderFor(outputSummary, ActionType.GetDisplayName(), order);
            subHeader.Units = collectUnits(project, data);
            subHeader.SaveSummarySection(outputSummary);
            var subOrder = 0;

            // Total distribution section
            var onlyCumulativeOutput = !data.ActiveSubstances.All(r => data.HazardCharacterisations.ContainsKey(r)) && data.HazardCharacterisations.ContainsKey(data.ReferenceCompound);
            var isCumulative = project.AssessmentSettings.MultipleSubstances && project.AssessmentSettings.Cumulative;
            if (outputSettings.ShouldSummarize(RisksSections.RisksDistributionSection)) {
                if (project.AssessmentSettings.MultipleSubstances) {
                    summarizeRiskDistribution(
                        result.CumulativeIndividualEffects,
                        result.ReferenceDose,
                        data.ReferenceCompound,
                        data.SelectedEffect,
                        isCumulative,
                        project,
                        subHeader,
                        subOrder
                    );
                } else {
                    summarizeRiskDistribution(
                        result.IndividualEffectsBySubstance[data.ReferenceCompound],
                        result.ReferenceDose,
                        data.ReferenceCompound,
                        data.SelectedEffect,
                        isCumulative,
                        project,
                        subHeader,
                        subOrder
                    );
                }
            }

            // Hazard versus exposure
            if (outputSettings.ShouldSummarize(RisksSections.HazardExposureSection)) {
                var section = new HazardExposureSection() {
                    SectionLabel = getSectionLabel(RisksSections.HazardExposureSection)
                };
                var subSubHeader = subHeader.AddSubSectionHeaderFor(section, "Hazard versus exposure", subOrder++);
                section.Summarize(
                    result.IndividualEffectsBySubstance,
                    result.CumulativeIndividualEffects,
                    project.EffectModelSettings.HealthEffectType,
                    data.ActiveSubstances,
                    data.ReferenceCompound,
                    data.HazardCharacterisations,
                    project.EffectModelSettings.RiskMetricType,
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

            // HI/MOE by substance (overview)
            if (outputSettings.ShouldSummarize(RisksSections.RisksBySubstanceSection)
                && data.ActiveSubstances.Count > 1 && !onlyCumulativeOutput) {
                summarizeRiskBySubstanceOverview(
                    result.IndividualEffectsBySubstance,
                    result.CumulativeIndividualEffects,
                    data.ActiveSubstances,
                    data.SelectedEffect,
                    project.EffectModelSettings.RiskMetricType,
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
            IHazardCharacterisationModel referenceHazardCharacterisation = null;
            if (result.CumulativeIndividualEffects != null
                && (data.HazardCharacterisations?.TryGetValue(data.ReferenceCompound, out referenceHazardCharacterisation) ?? false)
                && !double.IsNaN(referenceHazardCharacterisation.GeometricStandardDeviation)
                && outputSettings.ShouldSummarize(RisksSections.HazardDistributionSection)
            ) {
                summarizeHazardDistribution(
                    result.CumulativeIndividualEffects,
                    project.EffectModelSettings.HealthEffectType,
                    data.ReferenceCompound,
                    data.HazardCharacterisations,
                    result.ReferenceDose,
                    project.EffectSettings.UseIntraSpeciesConversionFactors ? data.IntraSpeciesFactorModels : null,
                    project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                    project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                    project.OutputDetailSettings.SelectedPercentiles,
                    subHeader,
                    subOrder
                );
            }

            if (project.EffectModelSettings.IsEAD && result.CumulativeIndividualEffects != null) {
                var section = new EquivalentAnimalDoseSection();
                var subSubHeader = header.AddSubSectionHeaderFor(section, "Equivalent animal dose (EAD)", subOrder++);
                section.Summarize(
                    result.CumulativeIndividualEffects,
                    project.EffectModelSettings.HealthEffectType,
                    result.ReferenceDose,
                    data.ReferenceCompound,
                    data.HazardCharacterisations,
                    project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                    project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                    project.OutputDetailSettings.SelectedPercentiles);
                subSubHeader.SaveSummarySection(section);
            }

            if (project.EffectModelSettings.IsEAD && result.CumulativeIndividualEffects != null) {
                var section = new PredictedHealthEffectSection();
                var subSubHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Predicted health effects",
                    subOrder++
                );
                // Note: when adding a reference dose, then make sure that it is based on the HC from the index substance
                // (don't use the obsolete reference dose from dietary exposures)
                section.Summarize(
                    result.CumulativeIndividualEffects,
                    project.EffectModelSettings.HealthEffectType,
                    data.ReferenceCompound,
                    data.HazardCharacterisations,
                    project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                    project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                    project.OutputDetailSettings.SelectedPercentiles);
                subSubHeader.SaveSummarySection(section);
            }
            if (result.CumulativeIndividualEffects != null
                && project.EffectSettings.TargetDoseLevelType == TargetLevelType.External
                && project.EffectModelSettings.CalculateRisksByFood
            ) {
                summarizeRisks(
                    result.CumulativeIndividualEffects,
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
        /// Summarizes full tables with HI's or MOE's and percentages at risk for modelled foods, substances and combinations
        /// </summary>
        /// <param name="cumulativeIndividualEffects"></param>
        /// <param name="individualEffectsByModelledFood"></param>
        /// <param name="individualEffectsBySubstance"></param>
        /// <param name="outputSettings"></param>
        /// <param name="project"></param>
        /// <param name="header"></param>
        /// <param name="subOrder"></param>
        private void summarizeRisks(
            ICollection<IndividualEffect> cumulativeIndividualEffects,
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
                hasThresholdExceedances = cumulativeIndividualEffects
                    .Any(c => c.ExposureConcentration > 0 && c.MarginOfExposure(project.EffectModelSettings.HealthEffectType) <= project.EffectModelSettings.ThresholdMarginOfExposure);
            } else {
                hasThresholdExceedances = cumulativeIndividualEffects
                    .Any(c => c.HazardIndex(project.EffectModelSettings.HealthEffectType) >= project.EffectModelSettings.ThresholdMarginOfExposure);
            }

            if (hasThresholdExceedances) {
                if (project.EffectSettings.TargetDoseLevelType == TargetLevelType.External 
                    && project.EffectModelSettings.CalculateRisksByFood
                    && outputSettings.ShouldSummarize(RisksSections.ModelledFoodAtRiskSection)
                ) {
                    summarizeModelledFoodsAtRisk(
                        individualEffectsByModelledFood,
                        cumulativeIndividualEffects.Count,
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
                        cumulativeIndividualEffects.Count,
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
                        cumulativeIndividualEffects.Count,
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
        /// <param name="substanceIndividualEffects"></param>
        /// <param name="cumulativeIndividualEffects"></param>
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
                Dictionary<Compound, List<IndividualEffect>> substanceIndividualEffects,
                List<IndividualEffect> cumulativeIndividualEffects,
                ICollection<Compound> substances,
                Effect focalEffect,
                RiskMetricType riskMetric,
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
            SectionHeader subHeader = null;
            if (riskMetric == RiskMetricType.MarginOfExposure) {
                var section = new MultipleMarginOfExposureSection() {
                    SectionLabel = getSectionLabel(RisksSections.RisksBySubstanceSection)
                };
                subHeader = header.AddSubSectionHeaderFor(
                   section,
                   "Margin of exposure by substance (overview)",
                   subOrder++
               );
                section.SummarizeMultipleSubstances(
                    substanceIndividualEffects,
                    cumulativeIndividualEffects,
                    substances,
                    focalEffect,
                    threshold,
                    confidenceInterval,
                    healthEffectType,
                    leftMargin,
                    rightMargin,
                    isInverseDistribution,
                    isCumulative,
                    onlyCumulativeOutput
                );
                subHeader.SaveSummarySection(section);
            } else if (riskMetric == RiskMetricType.HazardIndex) {
                var section = new MultipleHazardIndexSection() {
                    SectionLabel = getSectionLabel(RisksSections.RisksBySubstanceSection)
                };
                subHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Hazard index by substance (overview)",
                    subOrder++
                );
                section.SummarizeMultipleSubstances(
                    substanceIndividualEffects,
                    cumulativeIndividualEffects,
                    substances,
                    focalEffect,
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
                var section = new SingleMarginOfExposureSection();
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
                    leftMargin,
                    rightMargin,
                    isInverseDistribution,
                    isCumulative
                );
                subHeader.SaveSummarySection(section);
            } else if (riskMetric == RiskMetricType.HazardIndex) {
                var section = new SingleHazardIndexSection();
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
            //specific for riks, margin of exposure bar
            var lowerPercentage = (100 - project.EffectModelSettings.ConfidenceInterval) / 2;
            var upperPercentage = 100 - (100 - project.EffectModelSettings.ConfidenceInterval) / 2;
            result.Add(new ActionSummaryUnitRecord("TargetDoseUnit", data.HazardCharacterisationsUnit.GetShortDisplayName(project.EffectSettings.TargetDoseLevelType == TargetLevelType.External)));
            result.Add(new ActionSummaryUnitRecord("IntakeUnit", data.HazardCharacterisationsUnit.GetShortDisplayName(project.EffectSettings.TargetDoseLevelType == TargetLevelType.External)));
            //uncertainty
            result.Add(new ActionSummaryUnitRecord("LowerBound", $"p{project.UncertaintyAnalysisSettings.UncertaintyLowerBound:#0.##}"));
            result.Add(new ActionSummaryUnitRecord("UpperBound", $"p{project.UncertaintyAnalysisSettings.UncertaintyUpperBound:#0.##}"));
            //quartiles lower and upper, variabiliteit
            result.Add(new ActionSummaryUnitRecord("LowerPercentage", $"p{project.OutputDetailSettings.LowerPercentage:#0.##}"));
            result.Add(new ActionSummaryUnitRecord("UpperPercentage", $"p{project.OutputDetailSettings.UpperPercentage:#0.##}"));
            result.Add(new ActionSummaryUnitRecord("LowerConfidenceBound", $"p{lowerPercentage:#0.##}"));
            result.Add(new ActionSummaryUnitRecord("UpperConfidenceBound", $"p{upperPercentage:#0.##}"));
            if (project.EffectModelSettings.RiskMetricType == RiskMetricType.HazardIndex) {
                result.Add(new ActionSummaryUnitRecord("RiskMetricShort", "HI"));
                result.Add(new ActionSummaryUnitRecord("RiskMetric", "hazard index"));
            } else {
                result.Add(new ActionSummaryUnitRecord("RiskMetricShort", "MOE"));
                result.Add(new ActionSummaryUnitRecord("RiskMetric", "margin of exposure"));
            }
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
            var isCumulative = project.AssessmentSettings.MultipleSubstances && project.AssessmentSettings.Cumulative;
            if (outputSettings.ShouldSummarize(RisksSections.RisksDistributionSection)) {
                if (project.AssessmentSettings.MultipleSubstances) {
                    summarizeRiskDistributionUncertainty(
                        data.ReferenceCompound,
                        result.CumulativeIndividualEffects,
                        project.EffectModelSettings.RiskMetricType,
                        project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                        project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                        project.EffectModelSettings.IsInverseDistribution,
                        isCumulative,
                        header
                    );
                } else {
                    summarizeRiskDistributionUncertainty(
                        data.ReferenceCompound,
                        result.IndividualEffectsBySubstance[data.ReferenceCompound],
                        project.EffectModelSettings.RiskMetricType,
                        project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                        project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                        project.EffectModelSettings.IsInverseDistribution,
                        isCumulative,
                        header
                    );
                }
            }

            var onlyCumulativeOutput = !data.ActiveSubstances.All(r => data.HazardCharacterisations.ContainsKey(r)) && data.HazardCharacterisations.ContainsKey(data.ReferenceCompound);
            // HI/MOE by substance (overview)
            if (project.EffectModelSettings.RiskMetricType == RiskMetricType.HazardIndex) {
                subHeader = header.GetSubSectionHeader<MultipleHazardIndexSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as MultipleHazardIndexSection;
                    section.SummarizeMultipleSubstancesUncertainty(
                        data.ActiveSubstances,
                        result.IndividualEffectsBySubstance,
                        result.CumulativeIndividualEffects,
                        project.EffectModelSettings.IsInverseDistribution,
                        project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                        project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                        isCumulative);
                    subHeader.SaveSummarySection(section);
                }
            }
            if (project.EffectModelSettings.RiskMetricType == RiskMetricType.MarginOfExposure) {
                subHeader = header.GetSubSectionHeader<MultipleMarginOfExposureSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as MultipleMarginOfExposureSection;
                    section.SummarizeMultipleSubstancesUncertainty(
                        data.ActiveSubstances,
                        result.IndividualEffectsBySubstance,
                        result.CumulativeIndividualEffects,
                        project.EffectModelSettings.IsInverseDistribution,
                        project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                        project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                        isCumulative);
                    subHeader.SaveSummarySection(section);
                }
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

            // Hazard versus exposure
            subHeader = header.GetSubSectionHeader<HazardExposureSection>();
            if (subHeader != null) {
                var section = subHeader.GetSummarySection() as HazardExposureSection;
                section.SummarizeUncertainty(
                    result.IndividualEffectsBySubstance,
                    result.CumulativeIndividualEffects,
                    data.HazardCharacterisations,
                    data.ActiveSubstances,
                    data.ReferenceCompound,
                    project.EffectModelSettings.RiskMetricType,
                    project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                    project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                    onlyCumulativeOutput
                );
                subHeader.SaveSummarySection(section);
            }

            // Hazard distribution
            if (result.CumulativeIndividualEffects != null
                && data.HazardCharacterisations != null
                && data.HazardCharacterisations.ContainsKey(data.ReferenceCompound)
                && !double.IsNaN(data.HazardCharacterisations[data.ReferenceCompound].GeometricStandardDeviation)
            ) {
                summarizeHazardDistributionUncertainty(
                    project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                    project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                    result.CumulativeIndividualEffects,
                    header
                );
            }

            if (project.EffectModelSettings.IsEAD && result.CumulativeIndividualEffects != null) {
                subHeader = header.GetSubSectionHeader<EquivalentAnimalDoseSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as EquivalentAnimalDoseSection;
                    section.SummarizeUncertainty(result.CumulativeIndividualEffects);
                    subHeader.SaveSummarySection(section);
                }
            }

            if (project.EffectModelSettings.IsEAD && result.CumulativeIndividualEffects != null) {
                subHeader = header.GetSubSectionHeader<PredictedHealthEffectSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as PredictedHealthEffectSection;
                    section.SummarizeUncertainty(result.CumulativeIndividualEffects);
                    subHeader.SaveSummarySection(section);
                }
            }
            if (project.EffectSettings.TargetDoseLevelType == TargetLevelType.External && project.EffectModelSettings.CalculateRisksByFood) {
                subHeader = header.GetSubSectionHeader<MarginOfExposureModelledFoodSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as MarginOfExposureModelledFoodSection;
                    section.SummarizeFoodsUncertainty(
                        result.IndividualEffectsByModelledFood);
                    subHeader.SaveSummarySection(section);
                }
                subHeader = header.GetSubSectionHeader<HazardIndexModelledFoodSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as HazardIndexModelledFoodSection;
                    section.SummarizeFoodsUncertainty(
                        result.IndividualEffectsByModelledFood);
                    subHeader.SaveSummarySection(section);
                }
            }

            if (project.EffectSettings.TargetDoseLevelType == TargetLevelType.External && project.EffectModelSettings.CalculateRisksByFood) {
                subHeader = header.GetSubSectionHeader<MarginOfExposureSubstanceSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as MarginOfExposureSubstanceSection;
                    section.SummarizeSubstancesUncertainty(
                        result.IndividualEffectsBySubstance,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities
                    );
                    subHeader.SaveSummarySection(section);
                }
                subHeader = header.GetSubSectionHeader<HazardIndexSubstanceSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as HazardIndexSubstanceSection;
                    section.SummarizeSubstancesUncertainty(
                        result.IndividualEffectsBySubstance,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities
                    );
                    subHeader.SaveSummarySection(section);
                }
            }

            if (project.EffectSettings.TargetDoseLevelType == TargetLevelType.External && project.EffectModelSettings.CalculateRisksByFood) {
                subHeader = header.GetSubSectionHeader<MarginOfExposureModelledFoodSubstanceSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as MarginOfExposureModelledFoodSubstanceSection;
                    section.SummarizeModelledFoodSubstancesUncertainty(
                        result.IndividualEffectsByModelledFoodSubstance);
                    subHeader.SaveSummarySection(section);
                }
                subHeader = header.GetSubSectionHeader<HazardIndexModelledFoodSubstanceSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as HazardIndexModelledFoodSubstanceSection;
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
            var sectionTitle = project.EffectModelSettings.RiskMetricType == RiskMetricType.HazardIndex
                ? "Hazard index distributions by substances"
                : "Margin of exposure distributions by substances";
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
                var section = new MarginOfExposureModelledFoodSection() {
                    SectionLabel = getSectionLabel(RisksSections.RisksByModelledFoodSection)
                };
                subHeader = header.AddSubSectionHeaderFor(section, "Margin of exposure by modelled food", subOrder++);
                section.SummarizeRiskByFoods(
                   individualEffectsPerModelledFood,
                   project.OutputDetailSettings.LowerPercentage,
                   project.OutputDetailSettings.UpperPercentage,
                   project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                   project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                   project.EffectModelSettings.HealthEffectType,
                   project.EffectModelSettings.IsInverseDistribution
                );
                subHeader.SaveSummarySection(section);

            }
            if (project.EffectModelSettings.RiskMetricType == RiskMetricType.HazardIndex) {
                var section = new HazardIndexModelledFoodSection() {
                    SectionLabel = getSectionLabel(RisksSections.RisksByModelledFoodSection)
                };
                subHeader = header.AddSubSectionHeaderFor(section, "Hazard index by modelled food", subOrder++);
                section.SummarizeRiskByFoods(
                   individualEffectsPerModelledFood,
                   project.OutputDetailSettings.LowerPercentage,
                   project.OutputDetailSettings.UpperPercentage,
                   project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                   project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                   project.EffectModelSettings.HealthEffectType,
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
                var section = new MarginOfExposureSubstanceSection() {
                    SectionLabel = getSectionLabel(RisksSections.RisksBySubstanceSection)
                };
                subHeader = header.AddSubSectionHeaderFor(section, "Margin of exposure by substance", subOrder++);
                section.SummarizeRiskBySubstances(
                   individualEffectsPerSubstance,
                   relativePotencyFactors,
                   membershipsProbabilities,
                   project.OutputDetailSettings.LowerPercentage,
                   project.OutputDetailSettings.UpperPercentage,
                   project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                   project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                   project.EffectModelSettings.HealthEffectType,
                   project.EffectModelSettings.IsInverseDistribution
                );
                subHeader.SaveSummarySection(section);

            }
            if (project.EffectModelSettings.RiskMetricType == RiskMetricType.HazardIndex) {
                var section = new HazardIndexSubstanceSection() {
                    SectionLabel = getSectionLabel(RisksSections.RisksBySubstanceSection)
                };
                subHeader = header.AddSubSectionHeaderFor(section, "Hazard index by substance", subOrder++);
                section.SummarizeRiskBySubstances(
                   individualEffectsPerSubstance,
                   relativePotencyFactors,
                   membershipsProbabilities,
                   project.OutputDetailSettings.LowerPercentage,
                   project.OutputDetailSettings.UpperPercentage,
                   project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                   project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                   project.EffectModelSettings.HealthEffectType,
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
                var section = new MarginOfExposureModelledFoodSubstanceSection() {
                    SectionLabel = getSectionLabel(RisksSections.RisksByModelledFoodSubstanceSection)
                };
                subHeader = header.AddSubSectionHeaderFor(section, "Margin of exposure by modelled food x substance", subOrder++);
                section.SummarizeRiskByModelledFoodSubstances(
                   individualEffectsPerModelledFoodSubstance,
                   project.OutputDetailSettings.LowerPercentage,
                   project.OutputDetailSettings.UpperPercentage,
                   project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                   project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                   project.EffectModelSettings.HealthEffectType,
                   project.EffectModelSettings.IsInverseDistribution
                );
                subHeader.SaveSummarySection(section);

            }
            if (project.EffectModelSettings.RiskMetricType == RiskMetricType.HazardIndex) {
                var section = new HazardIndexModelledFoodSubstanceSection() {
                    SectionLabel = getSectionLabel(RisksSections.RisksByModelledFoodSubstanceSection)
                };
                subHeader = header.AddSubSectionHeaderFor(section, "Hazard index by modelled food x substance", subOrder++);
                section.SummarizeRiskByModelledFoodSubstances(
                   individualEffectsPerModelledFoodSubstance,
                   project.OutputDetailSettings.LowerPercentage,
                   project.OutputDetailSettings.UpperPercentage,
                   project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                   project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                   project.EffectModelSettings.HealthEffectType,
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
            List<IndividualEffect> cumulativeIndividualEffects,
            IHazardCharacterisationModel referenceDose,
            Compound substance,
            Effect selectedEffect,
            bool isCumulative,
            ProjectDto project,
            SectionHeader header,
            int subOrder
        ) {
            SectionHeader subHeader = null;
            if (cumulativeIndividualEffects != null) {
                if (project.EffectModelSettings.RiskMetricType == RiskMetricType.MarginOfExposure) {
                    subHeader = header.AddEmptySubSectionHeader(
                        "Margin of exposure distribution",
                        subOrder++,
                        getSectionLabel(RisksSections.RisksDistributionSection)
                    );
                    var graphSection = new MarginOfExposureDistributionSection();
                    var sub2Header = subHeader.AddSubSectionHeaderFor(graphSection, "Graphs", subOrder++);
                    graphSection.Summarize(
                        project.EffectModelSettings.ConfidenceInterval,
                        project.EffectModelSettings.ThresholdMarginOfExposure,
                        project.EffectModelSettings.HealthEffectType,
                        project.EffectModelSettings.IsInverseDistribution,
                        project.OutputDetailSettings.SelectedPercentiles,
                        cumulativeIndividualEffects,
                        referenceDose
                    );
                    sub2Header.SaveSummarySection(graphSection);

                    var percentileSection = new MarginOfExposurePercentileSection();
                    sub2Header = subHeader.AddSubSectionHeaderFor(percentileSection, "Percentiles", subOrder++);
                    percentileSection.Summarize(
                        cumulativeIndividualEffects,
                        project.OutputDetailSettings.SelectedPercentiles.Select(c => 100 - c).Reverse().ToList(),
                        referenceDose,
                        project.EffectModelSettings.HealthEffectType,
                        project.EffectModelSettings.IsInverseDistribution
                    );
                    sub2Header.SaveSummarySection(percentileSection);
                }

                if (project.EffectModelSettings.RiskMetricType == RiskMetricType.HazardIndex) {
                    subHeader = header.AddEmptySubSectionHeader(
                        "Hazard index distribution",
                        subOrder++,
                        getSectionLabel(RisksSections.RisksDistributionSection)
                     );

                    var graphSection = new HazardIndexDistributionSection();
                    var sub2Header = subHeader.AddSubSectionHeaderFor(graphSection, "Graphs", subOrder++);
                    graphSection.Summarize(
                        project.EffectModelSettings.ConfidenceInterval,
                        project.EffectModelSettings.ThresholdMarginOfExposure,
                        project.EffectModelSettings.HealthEffectType,
                        project.EffectModelSettings.IsInverseDistribution,
                        project.OutputDetailSettings.SelectedPercentiles,
                        cumulativeIndividualEffects,
                        referenceDose
                    );
                    sub2Header.SaveSummarySection(graphSection);

                    var percentileSection = new HazardIndexPercentileSection();
                    sub2Header = subHeader.AddSubSectionHeaderFor(percentileSection, "Percentiles", subOrder++);
                    percentileSection.Summarize(
                        cumulativeIndividualEffects,
                        project.OutputDetailSettings.SelectedPercentiles,
                        referenceDose,
                        project.EffectModelSettings.HealthEffectType,
                        project.EffectModelSettings.IsInverseDistribution
                    );
                    sub2Header.SaveSummarySection(percentileSection);
                }
                summarizeRiskBySingleSubstance(
                    cumulativeIndividualEffects,
                    substance,
                    selectedEffect,
                    project.EffectModelSettings.RiskMetricType,
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
                List<IndividualEffect> cumulativeIndividualEffects,
                RiskMetricType riskMetricType,
                double uncertaintyLowerBound,
                double uncertaintyUpperBound,
                bool isInverseDistribution,
                bool isCumulative,
                SectionHeader header
            ) {
            if (cumulativeIndividualEffects != null && riskMetricType == RiskMetricType.MarginOfExposure) {
                var subHeader = header.GetSubSectionHeader<MarginOfExposureDistributionSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as MarginOfExposureDistributionSection;
                    section.SummarizeUncertainty(
                        cumulativeIndividualEffects,
                        isInverseDistribution,
                        uncertaintyLowerBound,
                        uncertaintyUpperBound
                    );
                    subHeader.SaveSummarySection(section);
                }
            }
            if (cumulativeIndividualEffects != null && riskMetricType == RiskMetricType.MarginOfExposure) {
                var subHeader = header.GetSubSectionHeader<MarginOfExposurePercentileSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as MarginOfExposurePercentileSection;
                    section.SummarizeUncertainty(
                        cumulativeIndividualEffects,
                        isInverseDistribution,
                        uncertaintyLowerBound,
                        uncertaintyUpperBound
                    );
                    subHeader.SaveSummarySection(section);
                }
            }
            if (cumulativeIndividualEffects != null && riskMetricType == RiskMetricType.MarginOfExposure) {
                var subHeader = header.GetSubSectionHeader<SingleMarginOfExposureSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as SingleMarginOfExposureSection;
                    section.SummarizeSingleSubstanceUncertainty(
                        substance,
                        cumulativeIndividualEffects,
                        isInverseDistribution,
                        uncertaintyLowerBound,
                        uncertaintyUpperBound,
                        isCumulative
                    );
                    subHeader.SaveSummarySection(section);
                }
            }
            if (cumulativeIndividualEffects != null && riskMetricType == RiskMetricType.HazardIndex) {
                var subHeader = header.GetSubSectionHeader<HazardIndexDistributionSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as HazardIndexDistributionSection;
                    section.SummarizeUncertainty(
                        cumulativeIndividualEffects,
                        isInverseDistribution,
                        uncertaintyLowerBound,
                        uncertaintyUpperBound
                    );
                    subHeader.SaveSummarySection(section);
                }
            }
            if (cumulativeIndividualEffects != null && riskMetricType == RiskMetricType.HazardIndex) {
                var subHeader = header.GetSubSectionHeader<HazardIndexPercentileSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as HazardIndexPercentileSection;
                    section.SummarizeUncertainty(
                        cumulativeIndividualEffects,
                        isInverseDistribution,
                        uncertaintyLowerBound,
                        uncertaintyUpperBound
                    );
                    subHeader.SaveSummarySection(section);
                }
            }

            if (cumulativeIndividualEffects != null && riskMetricType == RiskMetricType.HazardIndex) {
                var subHeader = header.GetSubSectionHeader<SingleHazardIndexSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as SingleHazardIndexSection;
                    section.SummarizeSingleSubstanceUncertainty(
                        substance,
                        cumulativeIndividualEffects,
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
            List<IndividualEffect> cumulativeIndividualEffects,
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
            if (cumulativeIndividualEffects?.Any() ?? false) {
                var subHeader = header.AddEmptySubSectionHeader("Hazard distribution", subOrder++, getSectionLabel(RisksSections.HazardDistributionSection));
                var graphSection = new HazardDistributionSection();
                var subSubHeader = subHeader.AddSubSectionHeaderFor(graphSection, "Graphs", 1);
                graphSection.Summarize(
                    uncertaintyLowerBound,
                    uncertaintyUpperBound,
                    healthEffectType,
                    referenceCompound,
                    cumulativeIndividualEffects,
                    referenceDose,
                    hazardCharacterisations,
                    intraSpeciesConversionModels
                );
                subSubHeader.SaveSummarySection(graphSection);

                var percentileSection = new HazardPercentileSection();
                subSubHeader = subHeader.AddSubSectionHeaderFor(percentileSection, "Percentiles", 2);
                percentileSection.Summarize(cumulativeIndividualEffects, selectedPercentiles, referenceDose);
                subSubHeader.SaveSummarySection(percentileSection);
            }
        }

        private void summarizeHazardDistributionUncertainty(
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            List<IndividualEffect> cumulativeIndividualEffects,
            SectionHeader header
        ) {
            var subHeader = header.GetSubSectionHeader<HazardDistributionSection>();
            if (subHeader != null) {
                var distributionSection = subHeader.GetSummarySection() as HazardDistributionSection;
                distributionSection.SummarizeUncertainty(cumulativeIndividualEffects);
                subHeader.SaveSummarySection(distributionSection);
            }
            subHeader = header.GetSubSectionHeader<HazardPercentileSection>();
            if (subHeader != null) {
                var percentilesSection = subHeader.GetSummarySection() as HazardPercentileSection;
                percentilesSection.SummarizeUncertainty(
                    cumulativeIndividualEffects,
                    uncertaintyLowerBound,
                    uncertaintyUpperBound
                );
            }
        }
    }
}
