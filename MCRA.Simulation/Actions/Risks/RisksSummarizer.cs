using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Actions.HumanMonitoringAnalysis;
using MCRA.Simulation.Calculators.ComponentCalculation.DriverSubstanceCalculation;
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
        HazardExposureByAgeSection,
        HazardDistributionSection,
        RisksByModelledFoodSection,
        ModelledFoodAtRiskSection,
        RiskContributionsBySubstanceTotalSection,
        RiskContributionsBySubstanceUpperSection,
        RiskContributionsBySubstanceAtRisksSection,
        RisksRatioSumsSection,
        ContributionsForIndividualsSection,
        ContributionsForIndividualsUpperSection,
        ContributionsForIndividualsAtRiskSection,
        SubstanceAtRiskSection,
        RisksByModelledFoodSubstanceSection,
        ModelledFoodSubstanceAtRiskSection,
        McrCoExposureSection
    }

    public class RisksSummarizer : ActionModuleResultsSummarizer<RisksModuleConfig, RisksActionResult> {

        public RisksSummarizer(RisksModuleConfig config) : base(config) {
        }

        public override void Summarize(
            ActionModuleConfig outputConfig,
            RisksActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var outputSettings = new ModuleOutputSectionsManager<RisksSections>(outputConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var isHazardDistribution = data.HazardCharacterisationModelsCollections
                .SelectMany(r => r.HazardCharacterisationModels)
                .Any(r => !double.IsNaN(r.Value.GeometricStandardDeviation));

            var positiveSubstanceCount = result.IndividualEffectsBySubstanceCollections?
               .SelectMany(c => c.IndividualEffects)
               .Where(c => c.Value.Any(r => r.IsPositive))
               .Select(c => c.Key)
               .Distinct()
               .Count() ?? (data.ActiveSubstances.Count == 1 ? 1 : 0);

            var outputSummary = new RiskSummarySection() {
                SectionLabel = ActionType.ToString(),
                TargetDoseLevel = _configuration.TargetDoseLevelType,
                IsHazardCharacterisationDistribution = isHazardDistribution,
                ExposureModel = _configuration.TargetDoseLevelType == TargetLevelType.Internal
                    ? ActionType.TargetExposures
                    : ActionType.DietaryExposures,
                ExposureType = _configuration.ExposureType,
                RiskMetricType = _configuration.RiskMetricType,
                RiskMetricCalculationType = _configuration.RiskMetricCalculationType,
                NumberOfSubstances = data.ActiveSubstances.Count,
                NumberOfMissingSubstances = data.ActiveSubstances.Count - positiveSubstanceCount,
            };

            var subHeader = header.AddSubSectionHeaderFor(outputSummary, ActionType.GetDisplayName(), order);
            subHeader.Units = collectUnits();
            subHeader.SaveSummarySection(outputSummary);
            var subOrder = 0;

            var isCumulative = _configuration.MultipleSubstances && _configuration.CumulativeRisk;
            var referenceSubstance = data.ActiveSubstances.Count == 1
                ? data.ActiveSubstances.First()
                : data.ReferenceSubstance;

            // Cumulative risk
            summarizeRiskDistribution(
                result.TargetUnits,
                result.IndividualRisks,
                result.IndividualEffectsBySubstanceCollections,
                result.IndividualEffectsByModelledFood,
                result.IndividualEffectsByModelledFoodSubstance,
                result.DriverSubstances,
                result.ReferenceDose,
                result.TargetUnits.Count == 1 ? result.TargetUnits.First() : null,
                referenceSubstance,
                data.SelectedEffect,
                data.HazardCharacterisationModelsCollections,
                isCumulative,
                outputSettings,
                subHeader,
                subOrder
            );

            // Risk by substance (overview)
            if (result.IndividualEffectsBySubstanceCollections?.Count > 0) {
                summarizeRiskBySubstanceOverview(
                    result.ExposureTargets,
                    result.TargetUnits,
                    result.IndividualEffectsBySubstanceCollections,
                    result.IndividualRisks,
                    data.ActiveSubstances,
                    data.SelectedEffect,
                    isCumulative,
                    outputSettings,
                    subHeader,
                    subOrder
                );
            }

            // Hazard distribution
            if (result.IndividualRisks != null
                && result.ReferenceDose != null
                && !double.IsNaN(result.ReferenceDose.GeometricStandardDeviation)
                && outputSettings.ShouldSummarize(RisksSections.HazardDistributionSection)
            ) {
                summarizeHazardDistribution(
                    result.IndividualRisks,
                    result.ReferenceDose,
                    _configuration.UseIntraSpeciesConversionFactors ? data.IntraSpeciesFactorModels : null,
                    _configuration.UncertaintyLowerBound,
                    _configuration.UncertaintyUpperBound,
                    _configuration.SelectedPercentiles.ToArray(),
                    subHeader,
                    subOrder
                );
            }

            // Equivalent animal dose (EAD)
            if (_configuration.IsEAD && result.IndividualRisks != null) {
                var section = new EquivalentAnimalDoseSection();
                var subSubHeader = subHeader.AddSubSectionHeaderFor(section, "Equivalent animal dose (EAD)", subOrder++);
                section.Summarize(
                    result.IndividualRisks,
                    result.ReferenceDose,
                    _configuration.UncertaintyLowerBound,
                    _configuration.UncertaintyUpperBound,
                    _configuration.SelectedPercentiles.ToArray()
                );
                subSubHeader.SaveSummarySection(section);
            }

            // Forward effect calculation (predicted responses / health effects)
            if (_configuration.IsEAD && result.IndividualRisks != null) {
                var section = new PredictedHealthEffectSection();
                var subSubHeader = subHeader.AddSubSectionHeaderFor(
                    section,
                    "Predicted health effects",
                    subOrder++
                );
                section.Summarize(
                    result.IndividualRisks,
                    _configuration.HealthEffectType,
                    result.ReferenceDose,
                    _configuration.UncertaintyLowerBound,
                    _configuration.UncertaintyUpperBound,
                    _configuration.SelectedPercentiles.ToArray()
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
                    result.IndividualRisks,
                    _configuration.HealthEffectType,
                    data.ActiveSubstances,
                    data.HazardCharacterisationModelsCollections,
                    result.ReferenceDose,
                    _configuration.RiskMetricType,
                    _configuration.RiskMetricCalculationType,
                    _configuration.ConfidenceInterval,
                    _configuration.ThresholdMarginOfExposure,
                    _configuration.NumberOfLabels,
                    _configuration.UncertaintyLowerBound,
                    _configuration.UncertaintyUpperBound,
                    _configuration.SkipPrivacySensitiveOutputs,
                    isCumulative
                );
                subSubHeader.SaveSummarySection(section);

                // Exposures and hazards by age
                if (outputSettings.ShouldSummarize(RisksSections.HazardExposureByAgeSection)
                    && !_configuration.SkipPrivacySensitiveOutputs
                    && (data.ActiveSubstances.Count == 1
                        || (isCumulative && _configuration.RiskMetricCalculationType == RiskMetricCalculationType.RPFWeighted))
                    && (result.ReferenceDose?.HCSubgroups?.Count > 0)
                ) {
                    summarizeExposuresAndHazardsByAge(
                        result,
                        data.ActiveSubstances.Count > 1,
                        subSubHeader,
                        subOrder++
                    );
                }
            }
        }

        private void summarizeRiskDistribution(
            List<TargetUnit> targetUnits,
            List<IndividualEffect> cumulativeIndividualRisks,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstanceCollections,
            IDictionary<Food, List<IndividualEffect>> individualEffectsByModelledFood,
            IDictionary<(Food, Compound), List<IndividualEffect>> individualEffectsByModelledFoodSubstance,
            List<DriverSubstance> driverSubstances,
            IHazardCharacterisationModel referenceDose,
            TargetUnit targetUnit,
            Compound substance,
            Effect selectedEffect,
            ICollection<HazardCharacterisationModelCompoundsCollection> hazardCharacterisationModelCompoundsCollections,
            bool isCumulative,
            ModuleOutputSectionsManager<RisksSections> outputSettings,
            SectionHeader header,
            int subOrder
        ) {

            if (cumulativeIndividualRisks != null) {
                var subHeader = isCumulative
                    ? header.AddEmptySubSectionHeader("Cumulative risks", subOrder++)
                    : header;

                //Safety charts
                summarizeSafetyCharts(
                    cumulativeIndividualRisks,
                    targetUnit,
                    substance,
                    selectedEffect,
                    referenceDose,
                    isCumulative,
                    subHeader,
                    subOrder
                );

                //Distribution
                summarizeGraphsPercentiles(
                    cumulativeIndividualRisks,
                    hazardCharacterisationModelCompoundsCollections,
                    referenceDose,
                    targetUnit,
                    outputSettings,
                    subOrder,
                    subHeader
                );

                //Maximum cumulative ratio
                summarizeMcr(
                    driverSubstances,
                    referenceDose,
                    targetUnit,
                    subOrder,
                    subHeader
                );


                // Contributions by substance
                if ((cumulativeIndividualRisks?.Count > 0)
                    && (individualEffectsBySubstanceCollections?.Count > 0)
                ) {
                    var sub1Header = subHeader.AddEmptySubSectionHeader(
                        "Contributions by substance",
                        subOrder++
                    );
                    var hasThresholdExceedances = (_configuration.RiskMetricType == RiskMetricType.HazardExposureRatio)
                        ? cumulativeIndividualRisks.Any(c => c.Exposure > 0 && c.HazardExposureRatio <= _configuration.ThresholdMarginOfExposure)
                        : cumulativeIndividualRisks.Any(c => c.ExposureHazardRatio >= _configuration.ThresholdMarginOfExposure);

                    summarizeContributionsTotalBySubstance(
                        cumulativeIndividualRisks,
                        individualEffectsBySubstanceCollections,
                        outputSettings,
                        sub1Header,
                        subOrder
                    );

                    summarizeContributionsUpperBySubstance(
                        cumulativeIndividualRisks,
                        individualEffectsBySubstanceCollections,
                        outputSettings,
                        sub1Header,
                        subOrder
                    );
                    summarizeContributionsPercentageAtRiskBySubstance(
                        cumulativeIndividualRisks,
                        individualEffectsBySubstanceCollections,
                        outputSettings,
                        sub1Header,
                        subOrder
                    );
                    //Contributions to risks for individuals
                    summarizeContributionsForIndivididuals(
                        cumulativeIndividualRisks,
                        individualEffectsBySubstanceCollections,
                        outputSettings,
                        sub1Header,
                        subOrder
                    );

                    summarizeContributionsUpperForIndivididuals(
                        cumulativeIndividualRisks,
                        individualEffectsBySubstanceCollections,
                        outputSettings,
                        sub1Header,
                        subOrder
                    );

                    summarizeContributionsPercentageAtRiskForIndivididuals(
                        cumulativeIndividualRisks,
                        individualEffectsBySubstanceCollections,
                        outputSettings,
                        sub1Header,
                        subOrder
                    );

                    if (hasThresholdExceedances && _configuration.CalculateRisksByFood) {
                        summarizeSubstancesAtRisk(
                            individualEffectsBySubstanceCollections,
                            cumulativeIndividualRisks.Count,
                            outputSettings,
                            sub1Header,
                            subOrder
                        );
                    }

                    summarizeRiskRatios(
                        targetUnits,
                        cumulativeIndividualRisks,
                        individualEffectsBySubstanceCollections,
                        selectedEffect,
                        isCumulative,
                        outputSettings,
                        sub1Header,
                        subOrder
                    );

                }

                // Risks by food
                if (individualEffectsByModelledFood?.Count > 0) {
                    var sub1Header = subHeader.AddEmptySubSectionHeader(
                        "Contributions by food",
                        subOrder++
                    );
                    summarizeRisksByFood(
                        cumulativeIndividualRisks,
                        individualEffectsByModelledFood,
                        outputSettings,
                        sub1Header,
                        subOrder
                    );
                }
                // Risks by food x substance
                if (individualEffectsByModelledFoodSubstance?.Count > 0) {
                    var sub1Header = subHeader.AddEmptySubSectionHeader(
                        "Contributions by food and substance",
                        subOrder++
                    );
                    summarizeRisksByFoodSubstance(
                        cumulativeIndividualRisks,
                        individualEffectsByModelledFoodSubstance,
                        outputSettings,
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
        /// <param name="header"></param>
        /// <param name="subOrder"></param>
        private void summarizeRisksByFood(
            ICollection<IndividualEffect> individualEffects,
            IDictionary<Food, List<IndividualEffect>> individualEffectsByModelledFood,
            ModuleOutputSectionsManager<RisksSections> outputSettings,
            SectionHeader header,
            int subOrder
        ) {
            var hasThresholdExceedances = (_configuration.RiskMetricType == RiskMetricType.HazardExposureRatio)
                ? individualEffects.Any(c => c.Exposure > 0 && c.HazardExposureRatio <= _configuration.ThresholdMarginOfExposure)
                : individualEffects.Any(c => c.ExposureHazardRatio >= _configuration.ThresholdMarginOfExposure);

            // (Dietary) risks by modelled food
            if ((individualEffectsByModelledFood?.Count > 0)
                && _configuration.CalculateRisksByFood
                && _configuration.TargetDoseLevelType == TargetLevelType.External
                && outputSettings.ShouldSummarize(RisksSections.RisksByModelledFoodSection)
            ) {
                summarizeRiskByModelledFood(
                    individualEffectsByModelledFood,
                    header,
                    subOrder
                );

                // Risks modelled foods at risks
                if (hasThresholdExceedances && outputSettings.ShouldSummarize(RisksSections.ModelledFoodAtRiskSection)) {
                    summarizeModelledFoodsAtRisk(
                        individualEffectsByModelledFood,
                        individualEffects.Count,
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
        /// <param name="individualEffectsByModelledFoodSubstance"></param>
        /// <param name="outputSettings"></param>
        /// <param name="header"></param>
        /// <param name="subOrder"></param>
        private void summarizeRisksByFoodSubstance(
            ICollection<IndividualEffect> individualEffects,
            IDictionary<(Food, Compound), List<IndividualEffect>> individualEffectsByModelledFoodSubstance,
            ModuleOutputSectionsManager<RisksSections> outputSettings,
            SectionHeader header,
            int subOrder
        ) {
            var hasThresholdExceedances = (_configuration.RiskMetricType == RiskMetricType.HazardExposureRatio)
                ? individualEffects.Any(c => c.Exposure > 0 && c.HazardExposureRatio <= _configuration.ThresholdMarginOfExposure)
                : individualEffects.Any(c => c.ExposureHazardRatio >= _configuration.ThresholdMarginOfExposure);

            // (Dietary) risks by modelled food and substance
            if ((individualEffectsByModelledFoodSubstance?.Count > 0)
                && _configuration.CalculateRisksByFood
                && _configuration.TargetDoseLevelType == TargetLevelType.External
                && outputSettings.ShouldSummarize(RisksSections.RisksByModelledFoodSubstanceSection)
            ) {
                summarizeRiskByModelledFoodSubstance(
                    individualEffectsByModelledFoodSubstance,
                    header,
                    subOrder
                );

                if (hasThresholdExceedances && outputSettings.ShouldSummarize(RisksSections.ModelledFoodSubstanceAtRiskSection)) {
                    summarizeModelledFoodSubstancesAtRisk(
                        individualEffectsByModelledFoodSubstance,
                        individualEffects.Count,
                        header,
                        subOrder
                    );
                }
            }
        }

        /// <summary>
        /// Summarize risks by substance for multiple substances.
        /// </summary>
        /// <param name="exposureTargets"></param>
        /// <param name="targetUnits"></param>
        /// <param name="individualEffectsBySubstanceCollections"></param>
        /// <param name="individualEffects"></param>
        /// <param name="substances"></param>
        /// <param name="focalEffect"></param>
        /// <param name="isCumulative"></param>
        /// <param name="outputSettings"></param>
        /// <param name="header"></param>
        /// <param name="subOrder"></param>
        private void summarizeRiskBySubstanceOverview(
            ICollection<ExposureTarget> exposureTargets,
            List<TargetUnit> targetUnits,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)> individualEffectsBySubstanceCollections,
            List<IndividualEffect> individualEffects,
            ICollection<Compound> substances,
            Effect focalEffect,
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
                isCumulative,
                subHeader,
                subOrder
            );

            // Distributions by substance
            if (individualEffectsBySubstanceCollections?.Count > 0
                && substances.Count > 1
            ) {
                summarizeDistributionBySubstances(
                    exposureTargets,
                    individualEffectsBySubstanceCollections,
                    _configuration.RiskMetricType,
                    substances,
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
            bool isCumulative,
            SectionHeader header,
            int subOrder
        ) {
            if (_configuration.RiskMetricType == RiskMetricType.HazardExposureRatio) {
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
                    _configuration.ThresholdMarginOfExposure,
                    _configuration.ConfidenceInterval,
                    _configuration.RiskMetricType,
                    _configuration.RiskMetricCalculationType,
                    _configuration.LeftMargin,
                    _configuration.RightMargin,
                    _configuration.IsInverseDistribution,
                    isCumulative,
                    _configuration.SkipPrivacySensitiveOutputs
                );
                subHeader.SaveSummarySection(section);
            } else if (_configuration.RiskMetricType == RiskMetricType.ExposureHazardRatio) {
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
                    _configuration.RiskMetricCalculationType,
                    _configuration.RiskMetricType,
                    _configuration.ConfidenceInterval,
                    _configuration.ThresholdMarginOfExposure,
                    _configuration.LeftMargin,
                    _configuration.RightMargin,
                    _configuration.IsInverseDistribution,
                    isCumulative,
                    _configuration.SkipPrivacySensitiveOutputs
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private void summarizeSafetyCharts(
            List<IndividualEffect> individualEffects,
            TargetUnit targetUnit,
            Compound substance,
            Effect focalEffect,
            IHazardCharacterisationModel referenceDose,
            bool isCumulative,
            SectionHeader header,
            int subOrder
        ) {
            var section = new SingleRiskRatioSection(_configuration.RiskMetricType);
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
                _configuration.ConfidenceInterval,
                _configuration.ThresholdMarginOfExposure,
                _configuration.RiskMetricCalculationType,
                referenceDose,
                _configuration.LeftMargin,
                _configuration.RightMargin,
                _configuration.IsInverseDistribution,
                isCumulative,
                _configuration.SkipPrivacySensitiveOutputs
            );
            subHeader.SaveSummarySection(section);
        }

        private List<ActionSummaryUnitRecord> collectUnits() {
            // Quartiles lower and upper, variabiliteit
            var lowerPercentage = (100 - _configuration.ConfidenceInterval) / 2;
            var upperPercentage = 100 - (100 - _configuration.ConfidenceInterval) / 2;
            var individualDayUnit = _configuration.ExposureType == ExposureType.Chronic
                ? "individuals" : "individual days";

            var result = new List<ActionSummaryUnitRecord> {
                new("RiskMetric", _configuration.RiskMetricType.GetDisplayName()),
                new("RiskMetricShort", _configuration.RiskMetricType.GetShortDisplayName()),
                new("LowerPercentage", $"p{_configuration.VariabilityLowerPercentage:#0.##}"),
                new("UpperPercentage", $"p{_configuration.VariabilityUpperPercentage:#0.##}"),
                new("LowerConfidenceBound", $"p{lowerPercentage:#0.##}"),
                new("UpperConfidenceBound", $"p{upperPercentage:#0.##}"),
                new("LowerBound", $"p{_configuration.UncertaintyLowerBound:#0.##}"),
                new("UpperBound", $"p{_configuration.UncertaintyUpperBound:#0.##}"),
                new("IndividualDayUnit", individualDayUnit)
            };
            return result;
        }

        public void SummarizeUncertain(ActionModuleConfig outputConfig, RisksActionResult result, ActionData data, SectionHeader header) {
            var outputSettings = new ModuleOutputSectionsManager<RisksSections>(outputConfig, ActionType);
            var subHeader = header.GetSubSectionHeader<RiskSummarySection>();
            if (subHeader == null) {
                return;
            }
            var outputSummary = (RiskSummarySection)subHeader.GetSummarySection();
            if (outputSummary == null) {
                return;
            }

            // Total distribution section
            var isCumulative = _configuration.MultipleSubstances && _configuration.CumulativeRisk;
            var referenceSubstance = data.ActiveSubstances.Count == 1
                ? data.ActiveSubstances.First()
                : data.ReferenceSubstance;
            if (outputSettings.ShouldSummarize(RisksSections.RisksDistributionSection)) {
                summarizeRiskDistributionUncertainty(
                    referenceSubstance,
                    result.IndividualRisks,
                    _configuration.RiskMetricType,
                    _configuration.RiskMetricCalculationType,
                    _configuration.UncertaintyLowerBound,
                    _configuration.UncertaintyUpperBound,
                    _configuration.IsInverseDistribution,
                    isCumulative,
                    header
                );
            }

            // Risks by substance (overview)
            if (_configuration.RiskMetricType == RiskMetricType.ExposureHazardRatio) {
                subHeader = header.GetSubSectionHeader<MultipleExposureHazardRatioSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as MultipleExposureHazardRatioSection;
                    section.SummarizeUncertain(
                        result.TargetUnits,
                        data.ActiveSubstances,
                        result.IndividualEffectsBySubstanceCollections,
                        result.IndividualRisks,
                        _configuration.IsInverseDistribution,
                        _configuration.UncertaintyLowerBound,
                        _configuration.UncertaintyUpperBound,
                        isCumulative);
                    subHeader.SaveSummarySection(section);
                }
            } else if (_configuration.RiskMetricType == RiskMetricType.HazardExposureRatio) {
                subHeader = header.GetSubSectionHeader<MultipleHazardExposureRatioSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as MultipleHazardExposureRatioSection;
                    section.SummarizeUncertain(
                        result.TargetUnits,
                        data.ActiveSubstances,
                        result.IndividualEffectsBySubstanceCollections,
                        result.IndividualRisks,
                        _configuration.RiskMetricCalculationType,
                        _configuration.IsInverseDistribution,
                        _configuration.UncertaintyLowerBound,
                        _configuration.UncertaintyUpperBound,
                        isCumulative);
                    subHeader.SaveSummarySection(section);
                }
            }

            // Sum of substance risk characterisation ratios (at percentile)
            if (_configuration.RiskMetricType == RiskMetricType.ExposureHazardRatio && isCumulative) {
                subHeader = header.GetSubSectionHeader<CumulativeExposureHazardRatioSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as CumulativeExposureHazardRatioSection;
                    section.SummarizeUncertain(
                        result.ExposureTargets,
                        data.ActiveSubstances,
                        result.IndividualEffectsBySubstanceCollections,
                        result.IndividualRisks,
                        _configuration.IsInverseDistribution,
                        _configuration.UncertaintyLowerBound,
                        _configuration.UncertaintyUpperBound,
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
                    result.IndividualRisks,
                    data.HazardCharacterisationModelsCollections,
                    data.ActiveSubstances,
                    result.ReferenceDose,
                    _configuration.RiskMetricCalculationType,
                    _configuration.RiskMetricType,
                    _configuration.UncertaintyLowerBound,
                    _configuration.UncertaintyUpperBound,
                    isCumulative
                );
                subHeader.SaveSummarySection(section);
            }

            // Distributions by substance
            summarizeRiskDistributionBySubstancesUncertainty(
                result.ExposureTargets,
                result.IndividualEffectsBySubstanceCollections,
                data.ActiveSubstances,
                _configuration.UncertaintyLowerBound,
                _configuration.UncertaintyUpperBound,
                _configuration.RiskMetricType,
                _configuration.IsInverseDistribution,
                header
            );

            // Hazard distribution
            if (result.IndividualRisks != null
                && result.ReferenceDose != null
                && !double.IsNaN(result.ReferenceDose.GeometricStandardDeviation)
                && outputSettings.ShouldSummarize(RisksSections.HazardDistributionSection)
            ) {
                summarizeHazardDistributionUncertainty(
                    _configuration.UncertaintyLowerBound,
                    _configuration.UncertaintyUpperBound,
                    result.IndividualRisks,
                    header
                );
            }

            if (_configuration.IsEAD && result.IndividualRisks != null) {
                subHeader = header.GetSubSectionHeader<EquivalentAnimalDoseSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as EquivalentAnimalDoseSection;
                    section.SummarizeUncertainty(result.IndividualRisks);
                    subHeader.SaveSummarySection(section);
                }
            }

            if (_configuration.IsEAD && result.IndividualRisks != null) {
                subHeader = header.GetSubSectionHeader<PredictedHealthEffectSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as PredictedHealthEffectSection;
                    section.SummarizeUncertainty(result.IndividualRisks);
                    subHeader.SaveSummarySection(section);
                }
            }

            // Risks by substance
            if (isCumulative) {
                subHeader = header.GetSubSectionHeader<RiskContributionsBySubstanceSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as RiskContributionsBySubstanceSection;
                    section.SummarizeUncertain(
                        result.IndividualRisks,
                        result.IndividualEffectsBySubstanceCollections
                    );
                    subHeader.SaveSummarySection(section);
                }

                subHeader = header.GetSubSectionHeader<RiskContributionsBySubstanceUpperSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as RiskContributionsBySubstanceUpperSection;
                    section.SummarizeUpperUncertain(
                        result.IndividualRisks,
                        result.IndividualEffectsBySubstanceCollections,
                        _configuration.RiskMetricType,
                        _configuration.VariabilityUpperTailPercentage
                    );
                    subHeader.SaveSummarySection(section);
                }

                subHeader = header.GetSubSectionHeader<RiskContributionsBySubstanceAtRiskSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as RiskContributionsBySubstanceAtRiskSection;
                    section.SummarizeUpperAtRiskUncertain(
                        result.IndividualRisks,
                        result.IndividualEffectsBySubstanceCollections,
                        _configuration.RiskMetricType,
                        _configuration.ThresholdMarginOfExposure
                    );
                    subHeader.SaveSummarySection(section);
                }
            }

            // (Dietary) risks by food
            if (result.IndividualEffectsByModelledFood?.Count > 0) {
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
            if (result.IndividualEffectsByModelledFoodSubstance?.Count > 0) {
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
                section.SummarizeUncertainTotalDistribution(
                    result.IndividualRisks,
                    result.IndividualEffectsBySubstanceCollections,
                    _configuration.UncertaintyLowerBound,
                    _configuration.UncertaintyUpperBound
                );
            }

            subHeader = header.GetSubSectionHeader<ContributionsForIndividualsUpperSection>();
            if (subHeader != null) {
                var section = subHeader.GetSummarySection() as ContributionsForIndividualsUpperSection;
                section.SummarizeUncertainty(
                    result.IndividualRisks,
                    result.IndividualEffectsBySubstanceCollections,
                    _configuration.RiskMetricType,
                    _configuration.VariabilityUpperTailPercentage,
                    _configuration.UncertaintyLowerBound,
                    _configuration.UncertaintyUpperBound
                );
            }

            subHeader = header.GetSubSectionHeader<ContributionsForIndividualsAtRiskSection>();
            if (subHeader != null) {
                var section = subHeader.GetSummarySection() as ContributionsForIndividualsAtRiskSection;
                section.SummarizeUncertainty(
                    result.IndividualRisks,
                    result.IndividualEffectsBySubstanceCollections,
                    _configuration.RiskMetricType,
                    _configuration.ThresholdMarginOfExposure,
                    _configuration.UncertaintyLowerBound,
                    _configuration.UncertaintyUpperBound
                );
            }
        }

        private void summarizeDistributionBySubstances(
            ICollection<ExposureTarget> exposureTargets,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)> individualEffectsBySubstanceCollections,
            RiskMetricType riskMetric,
            ICollection<Compound> activeSubstances,
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
                    _configuration.ConfidenceInterval,
                    _configuration.ThresholdMarginOfExposure,
                    riskMetric,
                    _configuration.IsInverseDistribution,
                    _configuration.SelectedPercentiles.ToArray(),
                    _configuration.SkipPrivacySensitiveOutputs
                 );
                subHeader.SaveSummarySection(section);
            }
        }

        private void summarizeRiskByModelledFood(
            IDictionary<Food, List<IndividualEffect>> individualEffectsPerModelledFood,
            SectionHeader header,
            int subOrder
        ) {
            if (_configuration.RiskMetricType == RiskMetricType.HazardExposureRatio) {
                var section = new HazardExposureRatioModelledFoodSection() {
                    SectionLabel = getSectionLabel(RisksSections.RisksByModelledFoodSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(section, "Risks by modelled food", subOrder++);
                section.SummarizeRiskByFoods(
                   individualEffectsPerModelledFood,
                   _configuration.VariabilityLowerPercentage,
                   _configuration.VariabilityUpperPercentage,
                   _configuration.UncertaintyLowerBound,
                   _configuration.UncertaintyUpperBound,
                   _configuration.IsInverseDistribution
                );
                subHeader.SaveSummarySection(section);

            }
            if (_configuration.RiskMetricType == RiskMetricType.ExposureHazardRatio) {
                var section = new ExposureHazardRatioModelledFoodSection() {
                    SectionLabel = getSectionLabel(RisksSections.RisksByModelledFoodSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(section, "Risks by modelled food", subOrder++);
                section.SummarizeRiskByFoods(
                   individualEffectsPerModelledFood,
                   _configuration.VariabilityLowerPercentage,
                   _configuration.VariabilityUpperPercentage,
                   _configuration.UncertaintyLowerBound,
                   _configuration.UncertaintyUpperBound,
                   _configuration.IsInverseDistribution
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private void summarizeModelledFoodsAtRisk(
            IDictionary<Food, List<IndividualEffect>> individualEffectsPerModelledFood,
            int numberOfCumulativeIndividualEffects,
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
               _configuration.HealthEffectType,
               _configuration.RiskMetricType,
               _configuration.ThresholdMarginOfExposure
            );
            subHeader.SaveSummarySection(section);
        }

        private void summarizeContributionsTotalBySubstance(
            List<IndividualEffect> cumulativeIndividualRisks,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstance,
            ModuleOutputSectionsManager<RisksSections> outputSettings,
            SectionHeader header,
            int subOrder
        ) {
            if (outputSettings.ShouldSummarize(RisksSections.RiskContributionsBySubstanceTotalSection)) {
                var section = new RiskContributionsBySubstanceSection() {
                    SectionLabel = getSectionLabel(RisksSections.RiskContributionsBySubstanceTotalSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(section, "Contributions to risks for total distribution", subOrder++);
                section.Summarize(
                    cumulativeIndividualRisks,
                    individualEffectsBySubstance,
                    _configuration.VariabilityLowerPercentage,
                    _configuration.VariabilityUpperPercentage,
                    _configuration.UncertaintyLowerBound,
                    _configuration.UncertaintyUpperBound,
                    _configuration.IsInverseDistribution,
                    _configuration.RiskMetricType
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private void summarizeContributionsUpperBySubstance(
            List<IndividualEffect> individualEffects,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstance,
            ModuleOutputSectionsManager<RisksSections> outputSettings,
            SectionHeader header,
            int subOrder
        ) {
            if (outputSettings.ShouldSummarize(RisksSections.RiskContributionsBySubstanceUpperSection)) {
                var section = new RiskContributionsBySubstanceUpperSection() {
                    SectionLabel = getSectionLabel(RisksSections.RiskContributionsBySubstanceUpperSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(section, "Contributions to risks for upper distribution", subOrder++);
                section.SummarizeUpper(
                    individualEffects,
                    individualEffectsBySubstance,
                    _configuration.VariabilityLowerPercentage,
                    _configuration.VariabilityUpperPercentage,
                    _configuration.UncertaintyLowerBound,
                    _configuration.UncertaintyUpperBound,
                    _configuration.IsInverseDistribution,
                    _configuration.RiskMetricType,
                    _configuration.VariabilityUpperTailPercentage
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private void summarizeContributionsPercentageAtRiskBySubstance(
            List<IndividualEffect> individualEffects,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstance,
            ModuleOutputSectionsManager<RisksSections> outputSettings,
            SectionHeader header,
            int subOrder
        ) {
            if (outputSettings.ShouldSummarize(RisksSections.RiskContributionsBySubstanceAtRisksSection)) {
                var section = new RiskContributionsBySubstanceAtRiskSection() {
                    SectionLabel = getSectionLabel(RisksSections.RiskContributionsBySubstanceAtRisksSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(section, "Contributions to percentage at risk", subOrder++);
                section.SummarizeUpperAtRisk(
                    individualEffects,
                    individualEffectsBySubstance,
                    _configuration.VariabilityLowerPercentage,
                    _configuration.VariabilityUpperPercentage,
                    _configuration.UncertaintyLowerBound,
                    _configuration.UncertaintyUpperBound,
                    _configuration.IsInverseDistribution,
                    _configuration.RiskMetricType,
                    _configuration.ThresholdMarginOfExposure
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private void summarizeSubstancesAtRisk(
           List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)> individualEffectsBySubstanceCollections,
           int numberOfCumulativeIndividualEffects,
           ModuleOutputSectionsManager<RisksSections> outputSettings,
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
                   _configuration.HealthEffectType,
                   _configuration.RiskMetricType,
                   _configuration.ThresholdMarginOfExposure
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private void summarizeRiskByModelledFoodSubstance(
          IDictionary<(Food, Compound), List<IndividualEffect>> individualEffectsPerModelledFoodSubstance,
          SectionHeader header,
          int subOrder
      ) {
            if (_configuration.RiskMetricType == RiskMetricType.HazardExposureRatio) {
                var section = new HazardExposureRatioModelledFoodSubstanceSection() {
                    SectionLabel = getSectionLabel(RisksSections.RisksByModelledFoodSubstanceSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(section, "Risks by modelled food x substance", subOrder++);
                section.SummarizeRiskByModelledFoodSubstances(
                   individualEffectsPerModelledFoodSubstance,
                   _configuration.VariabilityLowerPercentage,
                   _configuration.VariabilityUpperPercentage,
                   _configuration.UncertaintyLowerBound,
                   _configuration.UncertaintyUpperBound,
                   _configuration.IsInverseDistribution
                );
                subHeader.SaveSummarySection(section);

            }
            if (_configuration.RiskMetricType == RiskMetricType.ExposureHazardRatio) {
                var section = new ExposureHazardRatioModelledFoodSubstanceSection() {
                    SectionLabel = getSectionLabel(RisksSections.RisksByModelledFoodSubstanceSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(section, "Risks by modelled food x substance", subOrder++);
                section.SummarizeRiskByModelledFoodSubstances(
                   individualEffectsPerModelledFoodSubstance,
                   _configuration.VariabilityLowerPercentage,
                   _configuration.VariabilityUpperPercentage,
                   _configuration.UncertaintyLowerBound,
                   _configuration.UncertaintyUpperBound,
                   _configuration.IsInverseDistribution
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private void summarizeModelledFoodSubstancesAtRisk(
           IDictionary<(Food, Compound), List<IndividualEffect>> individualEffectsPerModelledFoodSubstance,
           int numberOfCumulativeIndividualEffects,
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
               _configuration.HealthEffectType,
               _configuration.RiskMetricType,
               _configuration.ThresholdMarginOfExposure
            );
            subHeader.SaveSummarySection(section);
        }

        private void summarizeMcr(
            List<DriverSubstance> driverSubstances,
            IHazardCharacterisationModel referenceDose,
            TargetUnit targetUnit,
            int subOrder,
            SectionHeader header
        ) {
            if (driverSubstances?.Count > 0) {
                //Maximum Cumulative Ratio
                var section = new MaximumCumulativeRatioSection() {
                    SectionLabel = getSectionLabel(HumanMonitoringAnalysisSections.McrCoExposureSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Maximum Cumulative Ratio",
                    subOrder++
                );
                var threshold = _configuration.RiskMetricCalculationType == RiskMetricCalculationType.RPFWeighted
                    ? _configuration.ThresholdMarginOfExposure * referenceDose.Value
                    : _configuration.ThresholdMarginOfExposure;
                section.Summarize(
                    driverSubstances,
                    targetUnit,
                    _configuration.McrExposureApproachType,
                    _configuration.McrPlotRatioCutOff,
                    _configuration.McrPlotPercentiles.ToArray(),
                    _configuration.McrCalculationTotalExposureCutOff,
                    _configuration.McrPlotMinimumPercentage,
                    _configuration.SkipPrivacySensitiveOutputs,
                    threshold,
                    _configuration.RiskMetricCalculationType,
                    _configuration.RiskMetricType,
                    isRiskMcrPlot: true
                );
                subHeader.SaveSummarySection(section);
            }
        }

        /// <summary>
        /// Summarized grapghs and percentiles of Distribution section.
        /// </summary>
        private void summarizeGraphsPercentiles(
            List<IndividualEffect> individualEffects,
            ICollection<HazardCharacterisationModelCompoundsCollection> hazardCharacterisationModelCompoundsCollections,
            IHazardCharacterisationModel referenceDose,
            TargetUnit targetUnit,
            ModuleOutputSectionsManager<RisksSections> outputSettings,
            int subOrder,
            SectionHeader subHeader
        ) {
            var hasHCSubgroups = hazardCharacterisationModelCompoundsCollections
                .SelectMany(c => c.HazardCharacterisationModels.Values)
                .Where(c => c.HCSubgroups != null)
                .SelectMany(c => c.HCSubgroups)?.Any() ?? false;

            if (outputSettings.ShouldSummarize(RisksSections.RisksDistributionSection)) {
                var sub1Header = subHeader.AddEmptySubSectionHeader(
                    "Distribution",
                    subOrder++,
                    getSectionLabel(RisksSections.RisksDistributionSection)
                );

                var graphSection = new RiskRatioDistributionSection();
                var sub3Header = sub1Header.AddSubSectionHeaderFor(graphSection, "Graphs", subOrder++);
                graphSection.Summarize(
                    _configuration.ConfidenceInterval,
                    _configuration.ThresholdMarginOfExposure,
                    _configuration.IsInverseDistribution,
                    individualEffects,
                    _configuration.RiskMetricType
                );
                sub3Header.SaveSummarySection(graphSection);

                var percentileSection = new RiskRatioPercentileSection();
                sub3Header = sub1Header.AddSubSectionHeaderFor(percentileSection, "Percentiles", subOrder++);
                percentileSection.Summarize(
                    individualEffects,
                    _configuration.SelectedPercentiles.ToArray(),
                    referenceDose,
                    targetUnit,
                    _configuration.RiskMetricCalculationType,
                    _configuration.RiskMetricType,
                    _configuration.IsInverseDistribution,
                    _configuration.HCSubgroupDependent,
                    hasHCSubgroups,
                    _configuration.SkipPrivacySensitiveOutputs
                );
                sub3Header.SaveSummarySection(percentileSection);
            }
        }

        private void summarizeRiskRatios(
            List<TargetUnit> targetUnits,
            List<IndividualEffect> individualEffects,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstanceCollections,
            Effect selectedEffect,
            bool isCumulative,
            ModuleOutputSectionsManager<RisksSections> outputSettings,
            SectionHeader header,
            int subOrder
        ) {
            if (_configuration.RiskMetricType == RiskMetricType.ExposureHazardRatio && isCumulative
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
                    _configuration.RiskMetricCalculationType,
                    _configuration.RiskMetricType,
                    _configuration.ConfidenceInterval,
                    _configuration.ThresholdMarginOfExposure,
                    _configuration.LeftMargin,
                    _configuration.RightMargin,
                    _configuration.IsInverseDistribution,
                    _configuration.SkipPrivacySensitiveOutputs,
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
            if ((individualEffects?.Count > 0)
                && (individualEffectsBySubstanceCollections?.Count > 0)
                && outputSettings.ShouldSummarize(RisksSections.ContributionsForIndividualsSection)
            ) {
                var section = new ContributionsForIndividualsSection() {
                    SectionLabel = getSectionLabel(RisksSections.ContributionsForIndividualsSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(section, "Contributions to risks for individuals", subOrder++);
                section.SummarizeBoxPlotsTotalDistribution(
                    individualEffects,
                    individualEffectsBySubstanceCollections,
                    !_configuration.SkipPrivacySensitiveOutputs
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private void summarizeContributionsUpperForIndivididuals(
           List<IndividualEffect> individualEffects,
           List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstanceCollections,
           ModuleOutputSectionsManager<RisksSections> outputSettings,
           SectionHeader header,
           int subOrder
        ) {
            if ((individualEffects?.Count > 0)
                && (individualEffectsBySubstanceCollections?.Count > 0)
                && outputSettings.ShouldSummarize(RisksSections.ContributionsForIndividualsUpperSection)
            ) {
                var section = new ContributionsForIndividualsUpperSection() {
                    SectionLabel = getSectionLabel(RisksSections.ContributionsForIndividualsUpperSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(section, "Contributions to risks for individuals for upper distribution", subOrder++);
                section.Summarize(
                    individualEffects,
                    individualEffectsBySubstanceCollections,
                    _configuration.RiskMetricType,
                    _configuration.VariabilityUpperTailPercentage,
                    !_configuration.SkipPrivacySensitiveOutputs
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private void summarizeContributionsPercentageAtRiskForIndivididuals(
           List<IndividualEffect> individualEffects,
           List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstanceCollections,
           ModuleOutputSectionsManager<RisksSections> outputSettings,
           SectionHeader header,
           int subOrder
        ) {
            if ((individualEffects?.Count > 0)
                && (individualEffectsBySubstanceCollections?.Count > 0)
                && outputSettings.ShouldSummarize(RisksSections.ContributionsForIndividualsAtRiskSection)
            ) {
                var section = new ContributionsForIndividualsAtRiskSection() {
                    SectionLabel = getSectionLabel(RisksSections.ContributionsForIndividualsAtRiskSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(section, "Contributions for individuals at risk", subOrder++);
                section.Summarize(
                    individualEffects,
                    individualEffectsBySubstanceCollections,
                    _configuration.RiskMetricType,
                    _configuration.ThresholdMarginOfExposure,
                    !_configuration.SkipPrivacySensitiveOutputs

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
            var subHeaderGraph = header.GetSubSectionHeader<RiskRatioDistributionSection>();
            if (subHeaderGraph != null && individualEffects != null) {
                var sectionGraph = subHeaderGraph.GetSummarySection() as RiskRatioDistributionSection;
                sectionGraph.SummarizeUncertainty(
                    individualEffects,
                    isInverseDistribution,
                    uncertaintyLowerBound,
                    uncertaintyUpperBound,
                    riskMetricType
                );
                subHeaderGraph.SaveSummarySection(sectionGraph);
            }

            var subHeaderPercentile = header.GetSubSectionHeader<RiskRatioPercentileSection>();
            if (subHeaderPercentile != null && individualEffects != null) {
                var sectionPercentile = subHeaderPercentile.GetSummarySection() as RiskRatioPercentileSection;
                sectionPercentile.SummarizeUncertainty(
                    individualEffects,
                    isInverseDistribution,
                    uncertaintyLowerBound,
                    uncertaintyUpperBound
                );
                subHeaderPercentile.SaveSummarySection(sectionPercentile);
            }

            if (individualEffects != null) {
                var subHeader = header.GetSubSectionHeader<SingleRiskRatioSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as SingleRiskRatioSection;
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
            if (individualEffects?.Count > 0) {
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

        private void summarizeExposuresAndHazardsByAge(
            RisksActionResult result,
            bool isCumulative,
            SectionHeader header,
            int subOrder
        ) {
            var section = new ExposuresAndHazardsByAgeSection() {
                SectionLabel = getSectionLabel(RisksSections.HazardExposureByAgeSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(section, "Exposures and hazards by age", subOrder++);
            section.Summarize(
                result.IndividualRisks,
                _configuration.RiskMetricType,
                result.TargetUnits.First(),
                result.ReferenceDose,
                isCumulative
            );
            subHeader.SaveSummarySection(section);
        }
    }
}
