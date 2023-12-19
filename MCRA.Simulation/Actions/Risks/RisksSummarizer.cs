using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Actions.HumanMonitoringAnalysis;
using MCRA.Simulation.Calculators.ComponentCalculation.DriverSubstanceCalculation;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
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
        RiskContributionsBySubstanceTotalSection,
        RiskContributionsBySubstanceUpperSection,
        RisksRatioSumsSection,
        ContributionsForIndividualsSection,
        SubstanceAtRiskSection,
        RisksByModelledFoodSubstanceSection,
        ModelledFoodSubstanceAtRiskSection,
        McrCoExposureSection
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

            var isCumulative = project.AssessmentSettings.MultipleSubstances && project.RisksSettings.CumulativeRisk;
            var referenceSubstance = data.ActiveSubstances.Count == 1
                ? data.ActiveSubstances.First()
                : data.ReferenceSubstance;

            // Cumulative risk
            summarizeRiskDistribution(
                result.TargetUnits,
                result.IndividualEffects,
                result.IndividualEffectsBySubstanceCollections,
                result.IndividualEffectsByModelledFood,
                result.IndividualEffectsByModelledFoodSubstance,
                result.DriverSubstances,
                result.RiskMatrix,
                result.ReferenceDose,
                result.TargetUnits.Count == 1 ? result.TargetUnits.First() : null,
                referenceSubstance,
                data.SelectedEffect,
                isCumulative,
                project,
                outputSettings,
                subHeader,
                subOrder
            );

            // Risk by substance (overview)
            if (result.IndividualEffectsBySubstanceCollections?.Any() ?? false) {
                summarizeRiskBySubstanceOverview(
                    result.ExposureTargets,
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
                    project.OutputDetailSettings.SelectedPercentiles,
                    isCumulative,
                    outputSettings,
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
                var subSubHeader = subHeader.AddSubSectionHeaderFor(section, "Equivalent animal dose (EAD)", subOrder++);
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
                var subSubHeader = subHeader.AddSubSectionHeaderFor(
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
        }

        private void summarizeRiskDistribution(
            List<TargetUnit> targetUnits,
            List<IndividualEffect> individualEffects,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstanceCollections,
            IDictionary<Food, List<IndividualEffect>> individualEffectsByModelledFood,
            IDictionary<(Food, Compound), List<IndividualEffect>> individualEffectsByModelledFoodSubstance,
            List<DriverSubstance> driverSubstances,
            ExposureMatrix riskMatrix,
            IHazardCharacterisationModel referenceDose,
            TargetUnit targetUnit,
            Compound substance,
            Effect selectedEffect,
            bool isCumulative,
            ProjectDto project,
            ModuleOutputSectionsManager<RisksSections> outputSettings,
            SectionHeader header,
            int subOrder
        ) {

            if (individualEffects != null) {
                var subHeader = isCumulative
                    ? header.AddEmptySubSectionHeader("Cumulative risks", subOrder++)
                    : header;

                //Safety charts
                summarizeSafetyCharts(
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
                    subHeader,
                    subOrder
                );

                //Distribution
                summarizeGraphsPercentiles(
                    individualEffects,
                    referenceDose,
                    targetUnit,
                    project,
                    outputSettings,
                    subOrder,
                    subHeader
                );

                //Maximum cumulative ratio
                summarizeMcr(
                    driverSubstances,
                    riskMatrix,
                    project,
                    subOrder,
                    subHeader
                );


                // Contributions by substance
                if ((individualEffects?.Any() ?? false)
                    && (individualEffectsBySubstanceCollections?.Any() ?? false)
                ) {
                    var sub1Header = subHeader.AddEmptySubSectionHeader(
                        "Contributions by substance",
                        subOrder++
                    );
                    var hasThresholdExceedances = (project.RisksSettings.RiskMetricType == RiskMetricType.HazardExposureRatio)
                        ? individualEffects.Any(c => c.Exposure > 0 && c.HazardExposureRatio <= project.RisksSettings.ThresholdMarginOfExposure)
                        : individualEffects.Any(c => c.ExposureHazardRatio >= project.RisksSettings.ThresholdMarginOfExposure);

                    summarizeContributionsTotalBySubstance(
                        individualEffects,
                        individualEffectsBySubstanceCollections,
                        outputSettings,
                        project,
                        sub1Header,
                        subOrder
                    );

                    summarizeContributionsUpperBySubstance(
                        individualEffects,
                        individualEffectsBySubstanceCollections,
                        outputSettings,
                        project,
                        sub1Header,
                        subOrder
                    );

                    //Contributions to risks for individuals
                    summarizeContributionsForIndivididuals(
                        individualEffects,
                        individualEffectsBySubstanceCollections,
                        outputSettings,
                        sub1Header,
                        subOrder
                    );

                    if (hasThresholdExceedances && project.RisksSettings.CalculateRisksByFood) {
                        summarizeSubstancesAtRisk(
                            individualEffectsBySubstanceCollections,
                            individualEffects.Count,
                            outputSettings,
                            project,
                            sub1Header,
                            subOrder
                        );
                    }

                    summarizeRiskRatios(
                        targetUnits,
                        individualEffects,
                        individualEffectsBySubstanceCollections,
                        selectedEffect,
                        isCumulative,
                        outputSettings,
                        project,
                        sub1Header,
                        subOrder
                    );

                }

                // Risks by food
                if (individualEffectsByModelledFood?.Any() ?? false) {
                    var sub1Header = subHeader.AddEmptySubSectionHeader(
                        "Contributions by food",
                        subOrder++
                    );
                    summarizeRisksByFood(
                        individualEffects,
                        individualEffectsByModelledFood,
                        outputSettings,
                        project,
                        sub1Header,
                        subOrder
                    );
                }
                // Risks by food x substance
                if (individualEffectsByModelledFoodSubstance?.Any() ?? false) {
                    var sub1Header = subHeader.AddEmptySubSectionHeader(
                        "Contributions by food and substance",
                        subOrder++
                    );
                    summarizeRisksByFoodSubstance(
                        individualEffects,
                        individualEffectsByModelledFoodSubstance,
                        outputSettings,
                        project,
                        sub1Header,
                        subOrder
                    );
                }
            }
        }

        /// <summary>
        /// Summarizes full tables with risks and percentages at risk for modelled foods, 
        /// substances and combinations
        /// </summary>
        /// <param name="individualEffects"></param>
        /// <param name="individualEffectsByModelledFood"></param>
        /// <param name="outputSettings"></param>
        /// <param name="project"></param>
        /// <param name="header"></param>
        /// <param name="subOrder"></param>
        private void summarizeRisksByFood(
            ICollection<IndividualEffect> individualEffects,
            IDictionary<Food, List<IndividualEffect>> individualEffectsByModelledFood,
            ModuleOutputSectionsManager<RisksSections> outputSettings,
            ProjectDto project,
            SectionHeader header,
            int subOrder
        ) {
            var hasThresholdExceedances = (project.RisksSettings.RiskMetricType == RiskMetricType.HazardExposureRatio)
                ? individualEffects.Any(c => c.Exposure > 0 && c.HazardExposureRatio <= project.RisksSettings.ThresholdMarginOfExposure)
                : individualEffects.Any(c => c.ExposureHazardRatio >= project.RisksSettings.ThresholdMarginOfExposure);

            // (Dietary) risks by modelled food
            if ((individualEffectsByModelledFood?.Any() ?? false)
                && project.RisksSettings.CalculateRisksByFood
                && project.EffectSettings.TargetDoseLevelType == TargetLevelType.External
                && outputSettings.ShouldSummarize(RisksSections.RisksByModelledFoodSection)
            ) {
                summarizeRiskByModelledFood(
                    individualEffectsByModelledFood,
                    project,
                    header,
                    subOrder
                );

                // Risks modelled foods at risks
                if (hasThresholdExceedances && outputSettings.ShouldSummarize(RisksSections.ModelledFoodAtRiskSection)) {
                    summarizeModelledFoodsAtRisk(
                        individualEffectsByModelledFood,
                        individualEffects.Count,
                        project,
                        header,
                        subOrder
                    );
                }
            }
        }


        /// <summary>
        /// Summarizes full tables with risks and percentages at risk for modelled foods and substances, 
        /// substances and combinations
        /// </summary>
        /// <param name="individualEffects"></param>
        /// <param name="individualEffectsByModelledFood"></param>
        /// <param name="outputSettings"></param>
        /// <param name="project"></param>
        /// <param name="header"></param>
        /// <param name="subOrder"></param>
        private void summarizeRisksByFoodSubstance(
            ICollection<IndividualEffect> individualEffects,
            IDictionary<(Food, Compound), List<IndividualEffect>> individualEffectsByModelledFoodSubstance,
            ModuleOutputSectionsManager<RisksSections> outputSettings,
            ProjectDto project,
            SectionHeader header,
            int subOrder
        ) {
            var hasThresholdExceedances = (project.RisksSettings.RiskMetricType == RiskMetricType.HazardExposureRatio)
                ? individualEffects.Any(c => c.Exposure > 0 && c.HazardExposureRatio <= project.RisksSettings.ThresholdMarginOfExposure)
                : individualEffects.Any(c => c.ExposureHazardRatio >= project.RisksSettings.ThresholdMarginOfExposure);

            // (Dietary) risks by modelled food and substance
            if ((individualEffectsByModelledFoodSubstance?.Any() ?? false)
                && project.RisksSettings.CalculateRisksByFood
                && project.EffectSettings.TargetDoseLevelType == TargetLevelType.External
                && outputSettings.ShouldSummarize(RisksSections.RisksByModelledFoodSubstanceSection)
            ) {
                summarizeRiskByModelledFoodSubstance(
                    individualEffectsByModelledFoodSubstance,
                    project,
                    header,
                    subOrder
                );

                if (hasThresholdExceedances && outputSettings.ShouldSummarize(RisksSections.ModelledFoodSubstanceAtRiskSection)) {
                    summarizeModelledFoodSubstancesAtRisk(
                        individualEffectsByModelledFoodSubstance,
                        individualEffects.Count,
                        project,
                        header,
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
            ICollection<ExposureTarget> exposureTargets,
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
            double[] percentiles,
            bool isCumulative,
            ModuleOutputSectionsManager<RisksSections> outputSettings,
            SectionHeader header,
            int subOrder
        ) {
            var subHeader = header.AddEmptySubSectionHeader(
                "Risks by substance",
                subOrder++
            );

            summarizeSafetyChartsBySubstance(
                targetUnits,
                individualEffectsBySubstanceCollections,
                individualEffects,
                substances,
                focalEffect,
                riskMetric,
                riskMetricCalculationType,
                confidenceInterval,
                leftMargin,
                rightMargin,
                threshold,
                isInverseDistribution,
                isCumulative,
                subHeader,
                subOrder
            );

            // Distributions by substance
            if (individualEffectsBySubstanceCollections?.Any() ?? false
                && substances.Count > 1
            ) {
                summarizeDistributionBySubstances(
                    exposureTargets,
                    individualEffectsBySubstanceCollections,
                    riskMetric,
                    substances,
                    confidenceInterval,
                    threshold,
                    isInverseDistribution,
                    percentiles,
                    outputSettings,
                    subHeader,
                    subOrder
                 );
            }
        }

        private void summarizeSafetyChartsBySubstance(
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
            bool isCumulative,
            SectionHeader header,
            int subOrder
        ) {
            if (riskMetric == RiskMetricType.HazardExposureRatio) {
                var section = new MultipleHazardExposureRatioSection() {
                    SectionLabel = getSectionLabel(RisksSections.RisksBySubstanceOverviewSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(
                   section,
                   "Safety charts",
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
            } else if (riskMetric == RiskMetricType.ExposureHazardRatio) {
                var section = new MultipleExposureHazardRatioSection() {
                    SectionLabel = getSectionLabel(RisksSections.RisksBySubstanceOverviewSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Safety charts",
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
        private void summarizeSafetyCharts(
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
            SectionHeader header,
            int subOrder
        ) {
            if (riskMetric == RiskMetricType.HazardExposureRatio) {
                var section = new SingleHazardExposureRatioSection();
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
            } else if (riskMetric == RiskMetricType.ExposureHazardRatio) {
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
            if (project.RisksSettings.RiskMetricType == RiskMetricType.ExposureHazardRatio) {
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
            } else if (project.RisksSettings.RiskMetricType == RiskMetricType.HazardExposureRatio) {
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

            // Sum of substance risk characterisation ratios (at percentile)
            if (project.RisksSettings.RiskMetricType == RiskMetricType.ExposureHazardRatio && isCumulative) {
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

            // Risks by substance
            if (isCumulative
                && project.RisksSettings.RiskMetricCalculationType == RiskMetricCalculationType.RPFWeighted
            ) {
                subHeader = header.GetSubSectionHeader<HazardExposureRatioSubstanceSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as HazardExposureRatioSubstanceSection;
                    section.SummarizeUncertain(
                        result.IndividualEffects,
                        result.IndividualEffectsBySubstanceCollections
                    );
                    subHeader.SaveSummarySection(section);
                }
                subHeader = header.GetSubSectionHeader<ExposureHazardRatioSubstanceSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as ExposureHazardRatioSubstanceSection;
                    section.SummarizeUncertain(
                        result.IndividualEffects,
                        result.IndividualEffectsBySubstanceCollections
                    );
                    subHeader.SaveSummarySection(section);
                }

                subHeader = header.GetSubSectionHeader<HazardExposureRatioSubstanceUpperSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as HazardExposureRatioSubstanceUpperSection;
                    section.SummarizeUpperUncertain(
                        result.IndividualEffects,
                        result.IndividualEffectsBySubstanceCollections
                    );
                    subHeader.SaveSummarySection(section);
                }
                subHeader = header.GetSubSectionHeader<ExposureHazardRatioSubstanceUpperSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as ExposureHazardRatioSubstanceUpperSection;
                    section.SummarizeUpperUncertain(
                        result.IndividualEffects,
                        result.IndividualEffectsBySubstanceCollections
                    );
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
            // IndividualContributions
            subHeader = header.GetSubSectionHeader<ContributionsForIndividualsSection>();
            if (subHeader != null) {
                var section = subHeader.GetSummarySection() as ContributionsForIndividualsSection;
                section.SummarizeUncertain(
                    result.IndividualEffects,
                    result.IndividualEffectsBySubstanceCollections,
                    project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                    project.UncertaintyAnalysisSettings.UncertaintyUpperBound
                );
            }

        }

        private void summarizeDistributionBySubstances(
            ICollection<ExposureTarget> exposureTargets,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)> individualEffectsBySubstanceCollections,
            RiskMetricType riskMetric,
            ICollection<Compound> activeSubstances,
            double confidenceInterval,
            double threshold,
            bool isInverseDistribution,
            double[] percentiles,
            ModuleOutputSectionsManager<RisksSections> outputSettings,
            SectionHeader header,
            int subOrder
        ) {
            if (outputSettings.ShouldSummarize(RisksSections.RisksDistributionsBySubstanceSection)) {
                var section = new SubstancesOverviewSection() {
                    SectionLabel = getSectionLabel(RisksSections.RisksDistributionsBySubstanceSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Distributions",
                    subOrder++
                );
                section.Summarize(
                    subHeader,
                    exposureTargets,
                    individualEffectsBySubstanceCollections,
                    activeSubstances,
                    confidenceInterval,
                    threshold,
                    riskMetric,
                    isInverseDistribution,
                    percentiles
                 );
                subHeader.SaveSummarySection(section);
            }
        }

        private void summarizeRiskByModelledFood(
            IDictionary<Food, List<IndividualEffect>> individualEffectsPerModelledFood,
            ProjectDto project,
            SectionHeader header,
            int subOrder
        ) {
            if (project.RisksSettings.RiskMetricType == RiskMetricType.HazardExposureRatio) {
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
            if (project.RisksSettings.RiskMetricType == RiskMetricType.ExposureHazardRatio) {
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
            IDictionary<Food, List<IndividualEffect>> individualEffectsPerModelledFood,
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

        private void summarizeContributionsTotalBySubstance(
            List<IndividualEffect> individualEffects,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstance,
            ModuleOutputSectionsManager<RisksSections> outputSettings,
            ProjectDto project,
            SectionHeader header,
            int subOrder
        ) {
            if (project.RisksSettings.RiskMetricType == RiskMetricType.HazardExposureRatio
                && outputSettings.ShouldSummarize(RisksSections.RiskContributionsBySubstanceTotalSection)
            ) {
                var section = new HazardExposureRatioSubstanceSection() {
                    SectionLabel = getSectionLabel(RisksSections.RiskContributionsBySubstanceTotalSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(section, "Contributions to risks for total distribution", subOrder++);
                section.SummarizeTotalRiskBySubstances(
                    individualEffects,
                    individualEffectsBySubstance,
                    project.OutputDetailSettings.LowerPercentage,
                    project.OutputDetailSettings.UpperPercentage,
                    project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                    project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                    project.RisksSettings.IsInverseDistribution
                );
                subHeader.SaveSummarySection(section);
            } else if (project.RisksSettings.RiskMetricType == RiskMetricType.ExposureHazardRatio
                && outputSettings.ShouldSummarize(RisksSections.RiskContributionsBySubstanceTotalSection)
            ) {
                var section = new ExposureHazardRatioSubstanceSection() {
                    SectionLabel = getSectionLabel(RisksSections.RiskContributionsBySubstanceTotalSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(section, "Contributions to risks for total distribution", subOrder++);
                section.SummarizeTotalRiskBySubstances(
                    individualEffects,
                    individualEffectsBySubstance,
                    project.OutputDetailSettings.LowerPercentage,
                    project.OutputDetailSettings.UpperPercentage,
                    project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                    project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                    project.RisksSettings.IsInverseDistribution
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private void summarizeContributionsUpperBySubstance(
            List<IndividualEffect> individualEffects,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstance,
            ModuleOutputSectionsManager<RisksSections> outputSettings,
            ProjectDto project,
            SectionHeader header,
            int subOrder
        ) {
            if (project.RisksSettings.RiskMetricType == RiskMetricType.HazardExposureRatio
                && outputSettings.ShouldSummarize(RisksSections.RiskContributionsBySubstanceUpperSection)
            ) {
                var section = new HazardExposureRatioSubstanceUpperSection() {
                    SectionLabel = getSectionLabel(RisksSections.RiskContributionsBySubstanceUpperSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(section, "Contributions to risks for upper distribution", subOrder++);
                section.SummarizeUpperRiskBySubstances(
                    individualEffects,
                    individualEffectsBySubstance,
                    project.OutputDetailSettings.LowerPercentage,
                    project.OutputDetailSettings.UpperPercentage,
                    project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                    project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                    project.RisksSettings.IsInverseDistribution,
                    project.RisksSettings.ConfidenceInterval
                );
                subHeader.SaveSummarySection(section);
            } else if (project.RisksSettings.RiskMetricType == RiskMetricType.ExposureHazardRatio
                && outputSettings.ShouldSummarize(RisksSections.RiskContributionsBySubstanceUpperSection)
            ) {
                var section = new ExposureHazardRatioSubstanceUpperSection() {
                    SectionLabel = getSectionLabel(RisksSections.RiskContributionsBySubstanceUpperSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(section, "Contributions to risks for upper distribution", subOrder++);
                section.SummarizeUpperRiskBySubstances(
                    individualEffects,
                    individualEffectsBySubstance,
                    project.OutputDetailSettings.LowerPercentage,
                    project.OutputDetailSettings.UpperPercentage,
                    project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                    project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                    project.RisksSettings.IsInverseDistribution,
                    project.RisksSettings.ConfidenceInterval
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private void summarizeSubstancesAtRisk(
           List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)> individualEffectsBySubstanceCollections,
           int numberOfCumulativeIndividualEffects,
           ModuleOutputSectionsManager<RisksSections> outputSettings,
           ProjectDto project,
           SectionHeader header,
           int subOrder
       ) {
            if (outputSettings.ShouldSummarize(RisksSections.SubstanceAtRiskSection)) {
                var individualEffectsBySubstance = individualEffectsBySubstanceCollections
                    .First().IndividualEffects;
                var section = new SubstancesAtRiskSection() {
                    SectionLabel = getSectionLabel(RisksSections.SubstanceAtRiskSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(section, "Percentage at risk by substance", subOrder++);
                section.SummarizeSubstancesAtRisk(
                   individualEffectsBySubstance,
                   numberOfCumulativeIndividualEffects,
                   project.RisksSettings.HealthEffectType,
                   project.RisksSettings.RiskMetricType,
                   project.RisksSettings.ThresholdMarginOfExposure
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private void summarizeRiskByModelledFoodSubstance(
          IDictionary<(Food, Compound), List<IndividualEffect>> individualEffectsPerModelledFoodSubstance,
          ProjectDto project,
          SectionHeader header,
          int subOrder
      ) {
            if (project.RisksSettings.RiskMetricType == RiskMetricType.HazardExposureRatio) {
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
            if (project.RisksSettings.RiskMetricType == RiskMetricType.ExposureHazardRatio) {
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
            var subHeader = header.AddSubSectionHeaderFor(section, "Percentage at risk by modelled food x substance", subOrder++);
            section.SummarizeModelledFoodSubstancesAtRisk(
               individualEffectsPerModelledFoodSubstance,
               numberOfCumulativeIndividualEffects,
               project.RisksSettings.HealthEffectType,
               project.RisksSettings.RiskMetricType,
               project.RisksSettings.ThresholdMarginOfExposure
            );
            subHeader.SaveSummarySection(section);
        }


        private void summarizeMcr(
            List<DriverSubstance> driverSubstances,
            ExposureMatrix riskMatrix,
            ProjectDto project,
            int subOrder,
            SectionHeader header
        ) {
            if ((driverSubstances?.Any() ?? false)) {
                //Maximum Cumulative Ratio
                var section = new MaximumCumulativeRatioSection() {
                    SectionLabel = getSectionLabel(HumanMonitoringAnalysisSections.McrCoExposureSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Maximum Cumulative Ratio",
                    subOrder++
                );

                section.Summarize(
                    driverSubstances,
                    null,
                    project.MixtureSelectionSettings.McrExposureApproachType,
                    project.OutputDetailSettings.MaximumCumulativeRatioCutOff,
                    project.OutputDetailSettings.MaximumCumulativeRatioPercentiles,
                    project.MixtureSelectionSettings.TotalExposureCutOff,
                    project.OutputDetailSettings.MaximumCumulativeRatioMinimumPercentage,
                    project.RisksSettings.ThresholdMarginOfExposure,
                    project.RisksSettings.RiskMetricCalculationType, 
                    isRiskMcrPlot: true
                );
                subHeader.SaveSummarySection(section);
            }
        }

        /// <summary>
        /// Summarized grapghs and percentiles of Distribution section
        /// </summary>
        /// <param name="individualEffects"></param>
        /// <param name="referenceDose"></param>
        /// <param name="targetUnit"></param>
        /// <param name="project"></param>
        /// <param name="outputSettings"></param>
        /// <param name="subOrder"></param>
        /// <param name="subHeader"></param>
        private void summarizeGraphsPercentiles(
            List<IndividualEffect> individualEffects,
            IHazardCharacterisationModel referenceDose,
            TargetUnit targetUnit,
            ProjectDto project,
            ModuleOutputSectionsManager<RisksSections> outputSettings,
            int subOrder,
            SectionHeader subHeader
        ) {
            if (outputSettings.ShouldSummarize(RisksSections.RisksDistributionSection)) {
                var sub1Header = subHeader.AddEmptySubSectionHeader(
                    "Distribution",
                    subOrder++,
                    getSectionLabel(RisksSections.RisksDistributionSection)
                );

                if (project.RisksSettings.RiskMetricType == RiskMetricType.HazardExposureRatio) {
                    var graphSection = new HazardExposureRatioDistributionSection();
                    var sub3Header = sub1Header.AddSubSectionHeaderFor(graphSection, "Graphs", subOrder++);
                    graphSection.Summarize(
                        project.RisksSettings.ConfidenceInterval,
                        project.RisksSettings.ThresholdMarginOfExposure,
                        project.RisksSettings.IsInverseDistribution,
                        individualEffects
                    );
                    sub3Header.SaveSummarySection(graphSection);

                    var percentileSection = new HazardExposureRatioPercentileSection();
                    sub3Header = sub1Header.AddSubSectionHeaderFor(percentileSection, "Percentiles", subOrder++);
                    percentileSection.Summarize(
                        individualEffects,
                        project.OutputDetailSettings.SelectedPercentiles
                            .Select(c => 100 - c).Reverse().ToList(),
                        referenceDose,
                        targetUnit,
                        project.RisksSettings.RiskMetricCalculationType,
                        project.RisksSettings.IsInverseDistribution,
                        project.EffectSettings.HCSubgroupDependent
                    );
                    sub3Header.SaveSummarySection(percentileSection);
                }

                if (project.RisksSettings.RiskMetricType == RiskMetricType.ExposureHazardRatio) {
                    var graphSection = new ExposureHazardRatioDistributionSection();
                    var sub3Header = sub1Header.AddSubSectionHeaderFor(graphSection, "Graphs", subOrder++);
                    graphSection.Summarize(
                        project.RisksSettings.ConfidenceInterval,
                        project.RisksSettings.ThresholdMarginOfExposure,
                        project.RisksSettings.IsInverseDistribution,
                        individualEffects
                    );
                    sub3Header.SaveSummarySection(graphSection);

                    var percentileSection = new ExposureHazardRatioPercentileSection();
                    sub3Header = sub1Header.AddSubSectionHeaderFor(percentileSection, "Percentiles", subOrder++);
                    percentileSection.Summarize(
                        individualEffects,
                        project.OutputDetailSettings.SelectedPercentiles,
                        referenceDose,
                        targetUnit,
                        project.RisksSettings.RiskMetricCalculationType,
                        project.RisksSettings.IsInverseDistribution,
                        project.EffectSettings.HCSubgroupDependent
                    );
                    sub3Header.SaveSummarySection(percentileSection);
                }
            }
        }

        private void summarizeRiskRatios(
            List<TargetUnit> targetUnits,
            List<IndividualEffect> individualEffects,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstanceCollections,
            Effect selectedEffect,
            bool isCumulative,
            ModuleOutputSectionsManager<RisksSections> outputSettings,
            ProjectDto project,
            SectionHeader header,
            int subOrder
        ) {
            if (project.RisksSettings.RiskMetricType == RiskMetricType.ExposureHazardRatio && isCumulative
                && outputSettings.ShouldSummarize(RisksSections.RisksRatioSumsSection)
            ) {
                var section = new CumulativeExposureHazardRatioSection() {
                    SectionLabel = getSectionLabel(RisksSections.RisksRatioSumsSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Percentile risk characterisation ratio sums",
                    subOrder++
                );
                section.Summarize(
                    targetUnits,
                    individualEffectsBySubstanceCollections,
                    individualEffects,
                    individualEffectsBySubstanceCollections.SelectMany(c => c.SubstanceIndividualEffects.Keys).ToList(),
                    selectedEffect,
                    project.RisksSettings.RiskMetricCalculationType,
                    project.RisksSettings.RiskMetricType,
                    project.RisksSettings.ConfidenceInterval,
                    project.RisksSettings.ThresholdMarginOfExposure,
                    project.RisksSettings.LeftMargin,
                    project.RisksSettings.RightMargin,
                    project.RisksSettings.IsInverseDistribution,
                    isCumulative
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private void summarizeContributionsForIndivididuals(
            List<IndividualEffect> individualEffects,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstanceCollections,
            ModuleOutputSectionsManager<RisksSections> outputSettings,
            SectionHeader header,
            int subOrder
        ) {
            if ((individualEffects?.Any() ?? false)
                && (individualEffectsBySubstanceCollections?.Any() ?? false)
                && outputSettings.ShouldSummarize(RisksSections.ContributionsForIndividualsSection)
            ) {
                var section = new ContributionsForIndividualsSection() {
                    SectionLabel = getSectionLabel(RisksSections.ContributionsForIndividualsSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(section, "Contributions to risks for individuals", subOrder++);
                section.SummarizeBoxPlots(
                    individualEffects,
                    individualEffectsBySubstanceCollections
                );
                subHeader.SaveSummarySection(section);
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
            if (individualEffects != null && riskMetricType == RiskMetricType.HazardExposureRatio) {
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

            if (individualEffects != null && riskMetricType == RiskMetricType.ExposureHazardRatio) {
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

            if (individualEffects != null && riskMetricType == RiskMetricType.HazardExposureRatio) {
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

            if (individualEffects != null && riskMetricType == RiskMetricType.ExposureHazardRatio) {
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

            if (individualEffects != null && riskMetricType == RiskMetricType.HazardExposureRatio) {
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

            if (individualEffects != null && riskMetricType == RiskMetricType.ExposureHazardRatio) {
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
