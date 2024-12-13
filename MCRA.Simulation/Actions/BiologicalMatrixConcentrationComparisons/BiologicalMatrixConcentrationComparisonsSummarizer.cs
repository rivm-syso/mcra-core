using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.BiologicalMatrixConcentrationComparisons {
    public enum BiologicalMatrixConcentrationComparisonsSections {
        MonitoringModelledCumulativeConcentrationSection,
        MonitoringModelledBySubstanceConcentrationSection,
        MonotoringVersusModelCorrelationsBySubstanceSection,
        MonotoringVersusModelCorrelationsCumulativeSection
    }

    public sealed class BiologicalMatrixConcentrationComparisonsSummarizer :
        ActionModuleResultsSummarizer<BiologicalMatrixConcentrationComparisonsModuleConfig, BiologicalMatrixConcentrationComparisonsActionResult> {

        public BiologicalMatrixConcentrationComparisonsSummarizer(BiologicalMatrixConcentrationComparisonsModuleConfig config) : base(config) {
        }

        public override void Summarize(
            ActionModuleConfig settingsConfig,
            BiologicalMatrixConcentrationComparisonsActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var outputSettings = new ModuleOutputSectionsManager<BiologicalMatrixConcentrationComparisonsSections>(settingsConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var outputSummary = new BiologicalMatrixConcentrationComparisonsSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(outputSummary, ActionType.GetDisplayName(), order);
            var subOrder = 0;
            subHeader.Units = collectUnits(data);

            // Comparisons cumulative exposures/concentrations
            if (outputSettings.ShouldSummarize(BiologicalMatrixConcentrationComparisonsSections.MonitoringModelledCumulativeConcentrationSection)) {
                summarizeCumulativeMonitoringVsModelledConcentrations(
                    data.AggregateIndividualDayExposures,
                    data.AggregateIndividualExposures,
                    data.HbmCumulativeIndividualDayCollection,
                    data.HbmCumulativeIndividualCollection,
                    data.ReferenceSubstance,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    _configuration.ExposureType,
                    data.TargetExposureUnit,
                    subHeader,
                    subOrder++
                );
            }

            // Comparisons by substance
            if (outputSettings.ShouldSummarize(BiologicalMatrixConcentrationComparisonsSections.MonitoringModelledBySubstanceConcentrationSection)) {
                SummarizeMonitoringVersusModelResults(
                    data.ActiveSubstances,
                    data.AggregateIndividualDayExposures,
                    data.AggregateIndividualExposures,
                    data.HbmIndividualDayCollections,
                    data.HbmIndividualCollections,
                    data.TargetExposureUnit,
                    _configuration.ExposureType,
                    subHeader,
                    subOrder++
                );
            }

            if (outputSettings.ShouldSummarize(BiologicalMatrixConcentrationComparisonsSections.MonotoringVersusModelCorrelationsCumulativeSection)
                && _configuration.CorrelateTargetConcentrations) {
                summarizeMonotoringVersusModelCorrelationsCumulative(
                    data.ActiveSubstances,
                    data.AggregateIndividualDayExposures,
                    data.AggregateIndividualExposures,
                    data.HbmIndividualDayCollections,
                    data.HbmIndividualCollections,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    data.TargetExposureUnit,
                    _configuration.ExposureType,
                    subHeader,
                    subOrder++
                );
            }
            if (outputSettings.ShouldSummarize(BiologicalMatrixConcentrationComparisonsSections.MonotoringVersusModelCorrelationsBySubstanceSection)
                && _configuration.CorrelateTargetConcentrations) {
                summarizeMonotoringVersusModelCorrelationsBySubstance(
                    data.ActiveSubstances,
                    data.AggregateIndividualDayExposures,
                    data.AggregateIndividualExposures,
                    data.HbmIndividualDayCollections,
                    data.HbmIndividualCollections,
                    data.TargetExposureUnit,
                    _configuration.ExposureType,
                    subHeader,
                    subOrder++
                );
            }
            subHeader.SaveSummarySection(outputSummary);
        }

        private List<ActionSummaryUnitRecord> collectUnits(ActionData data) {
            var result = new List<ActionSummaryUnitRecord> {
                new("ModelledExposureUnit", data.TargetExposureUnit.GetShortDisplayName()),
                new("LowerPercentage", $"p{_configuration.VariabilityLowerPercentage}"),
                new("UpperPercentage", $"p{_configuration.VariabilityUpperPercentage}")
            };
            if (_configuration.ExposureType == ExposureType.Chronic) {
                result.Add(new ActionSummaryUnitRecord("IndividualDayUnit", "individuals"));
            } else {
                result.Add(new ActionSummaryUnitRecord("IndividualDayUnit", "individual days"));
            }
            return result;
        }

        private void summarizeCumulativeMonitoringVsModelledConcentrations(
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            HbmCumulativeIndividualDayCollection hbmCumulativeIndividualDayCollection,
            HbmCumulativeIndividualCollection hbmCumulativeIndividualCollection,
            Compound referenceSubstance,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ExposureType exposureType,
            TargetUnit targetExposureUnit,
            SectionHeader header,
            int order
        ) {
            if (exposureType == ExposureType.Acute
                && aggregateIndividualDayExposures != null
                && hbmCumulativeIndividualDayCollection != null
            ) {
                var section = new CumulativeDayConcentrationsSection() {
                    SectionLabel = getSectionLabel(BiologicalMatrixConcentrationComparisonsSections.MonitoringModelledCumulativeConcentrationSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Comparison cumulative",
                    order
                );
                section.Summarize(
                    aggregateIndividualDayExposures,
                    hbmCumulativeIndividualDayCollection,
                    referenceSubstance,
                    relativePotencyFactors,
                    membershipProbabilities,
                    targetExposureUnit,
                    _configuration.VariabilityLowerPercentage,
                    _configuration.VariabilityUpperPercentage
                );
                subHeader.SaveSummarySection(section);
            } else if (exposureType == ExposureType.Chronic
                && aggregateIndividualExposures != null
                && hbmCumulativeIndividualCollection != null
            ) {
                var section = new CumulativeIndividualConcentrationsSection() {
                    SectionLabel = getSectionLabel(BiologicalMatrixConcentrationComparisonsSections.MonitoringModelledCumulativeConcentrationSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Comparison cumulative",
                    order
                );
                section.Summarize(
                    aggregateIndividualExposures,
                    hbmCumulativeIndividualCollection,
                    referenceSubstance,
                    relativePotencyFactors,
                    membershipProbabilities,
                    targetExposureUnit,
                    _configuration.VariabilityLowerPercentage,
                    _configuration.VariabilityUpperPercentage
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private void SummarizeMonitoringVersusModelResults(
            ICollection<Compound> activeSubstances,
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<HbmIndividualDayCollection> hbmIndividualDayConcentrationsCollections,
            ICollection<HbmIndividualCollection> hbmIndividualConcentrationsCollections,
            TargetUnit targetExposureUnit,
            ExposureType exposureType,
            SectionHeader header,
            int order
        ) {
            if (exposureType == ExposureType.Acute && aggregateIndividualDayExposures != null) {
                var section = new HbmVsModelledIndividualDayConcentrationsBySubstanceSection() {
                    SectionLabel = getSectionLabel(BiologicalMatrixConcentrationComparisonsSections.MonitoringModelledBySubstanceConcentrationSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Comparisons by substance",
                    order
                );
                section.Summarize(
                    aggregateIndividualDayExposures,
                    hbmIndividualDayConcentrationsCollections,
                    activeSubstances,
                    targetExposureUnit,
                    _configuration.VariabilityLowerPercentage,
                    _configuration.VariabilityUpperPercentage
                );
                subHeader.SaveSummarySection(section);
            } else if (exposureType == ExposureType.Chronic && aggregateIndividualExposures != null) {
                var section = new HbmVsModelledIndividualConcentrationsBySubstanceSection() {
                    SectionLabel = getSectionLabel(BiologicalMatrixConcentrationComparisonsSections.MonitoringModelledBySubstanceConcentrationSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Comparisons by substance",
                    order
                );
                section.Summarize(
                    aggregateIndividualExposures,
                    hbmIndividualConcentrationsCollections,
                    activeSubstances,
                    targetExposureUnit,
                    _configuration.VariabilityLowerPercentage,
                    _configuration.VariabilityUpperPercentage
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private void summarizeMonotoringVersusModelCorrelationsBySubstance(
            ICollection<Compound> activeSubstances,
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<HbmIndividualDayCollection> hbmIndividualDayConcentrationsCollections,
            ICollection<HbmIndividualCollection> hbmIndividualConcentrationsCollections,
            TargetUnit targetExposureUnit,
            ExposureType exposureType,
            SectionHeader header,
            int order
        ) {
            if (exposureType == ExposureType.Acute && aggregateIndividualDayExposures != null) {
                var section = new DayConcentrationCorrelationsBySubstanceSection() {
                    SectionLabel = getSectionLabel(BiologicalMatrixConcentrationComparisonsSections.MonotoringVersusModelCorrelationsBySubstanceSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Correlations by substance",
                    order
                );
                section.Summarize(
                    aggregateIndividualDayExposures.Cast<ITargetIndividualDayExposure>().ToList(),
                    hbmIndividualDayConcentrationsCollections,
                    activeSubstances,
                    targetExposureUnit,
                    _configuration.VariabilityLowerPercentage,
                    _configuration.VariabilityUpperPercentage
                );
                subHeader.SaveSummarySection(section);
            } else if (exposureType == ExposureType.Chronic && aggregateIndividualExposures != null) {
                var section = new IndividualConcentrationCorrelationsBySubstanceSection() {
                    SectionLabel = getSectionLabel(BiologicalMatrixConcentrationComparisonsSections.MonotoringVersusModelCorrelationsBySubstanceSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Correlations by substance",
                    order
                );
                section.Summarize(
                    aggregateIndividualExposures.Cast<ITargetIndividualExposure>().ToList(),
                    hbmIndividualConcentrationsCollections,
                    activeSubstances,
                    targetExposureUnit,
                    _configuration.VariabilityLowerPercentage,
                    _configuration.VariabilityUpperPercentage
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private void summarizeMonotoringVersusModelCorrelationsCumulative(
           ICollection<Compound> activeSubstances,
           ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
           ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
           ICollection<HbmIndividualDayCollection> hbmIndividualDayCollections,
           ICollection<HbmIndividualCollection> hbmIndividualCollections,
           IDictionary<Compound, double> relativePotencyFactors,
           IDictionary<Compound, double> membershipProbabilities,
           TargetUnit targetExposureUnit,
           ExposureType exposureType,
           SectionHeader header,
           int order
       ) {
            if (exposureType == ExposureType.Acute && aggregateIndividualDayExposures != null) {
                var section = new DayConcentrationCorrelationsCumulativeSection() {
                    SectionLabel = getSectionLabel(BiologicalMatrixConcentrationComparisonsSections.MonotoringVersusModelCorrelationsCumulativeSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Correlations cumulative",
                    order
                );
                section.Summarize(
                    aggregateIndividualDayExposures.Cast<ITargetIndividualDayExposure>().ToList(),
                    hbmIndividualDayCollections,
                    activeSubstances,
                    relativePotencyFactors,
                    membershipProbabilities,
                    targetExposureUnit,
                    _configuration.VariabilityLowerPercentage,
                    _configuration.VariabilityUpperPercentage
                );
                subHeader.SaveSummarySection(section);
            } else if (exposureType == ExposureType.Chronic && aggregateIndividualExposures != null) {
                var section = new IndividualConcentrationCorrelationsCumulativeSection() {
                    SectionLabel = getSectionLabel(BiologicalMatrixConcentrationComparisonsSections.MonotoringVersusModelCorrelationsCumulativeSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Correlations cumulative",
                    order
                );
                section.Summarize(
                    aggregateIndividualExposures.Cast<ITargetIndividualExposure>().ToList(),
                    hbmIndividualCollections,
                    activeSubstances,
                    relativePotencyFactors,
                    membershipProbabilities,
                    targetExposureUnit,
                    _configuration.VariabilityLowerPercentage,
                    _configuration.VariabilityUpperPercentage
                );
                subHeader.SaveSummarySection(section);
            }
        }
    }
}
