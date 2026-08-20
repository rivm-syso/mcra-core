using CommandLine;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
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
                    result,
                    data,
                    subHeader,
                    subOrder++
                );
            }

            // Comparisons by substance
            if (outputSettings.ShouldSummarize(BiologicalMatrixConcentrationComparisonsSections.MonitoringModelledBySubstanceConcentrationSection)) {
                SummarizeMonitoringVersusModelResults(
                    result,
                    data,
                    subHeader,
                    subOrder++
                );
            }

            if (outputSettings.ShouldSummarize(BiologicalMatrixConcentrationComparisonsSections.MonotoringVersusModelCorrelationsCumulativeSection)
                && _configuration.CorrelateTargetConcentrations) {
                summarizeMonotoringVersusModelCorrelationsCumulative(
                    result,
                    data,
                    subHeader,
                    subOrder++
                );
            }
            if (outputSettings.ShouldSummarize(BiologicalMatrixConcentrationComparisonsSections.MonotoringVersusModelCorrelationsBySubstanceSection)
                && _configuration.CorrelateTargetConcentrations) {
                summarizeMonotoringVersusModelCorrelationsBySubstance(
                    result,
                    data,
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
            BiologicalMatrixConcentrationComparisonsActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            if (_configuration.ExposureType == ExposureType.Acute) {
                var section = new CumulativeDayConcentrationsSection() {
                    SectionLabel = getSectionLabel(BiologicalMatrixConcentrationComparisonsSections.MonitoringModelledCumulativeConcentrationSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Comparison cumulative",
                    order
                );
                section.Summarize(
                    data.AggregateIndividualDayExposures,
                    data.HbmCumulativeIndividualDayCollection,
                    data.ReferenceSubstance,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    data.TargetExposureUnit,
                    _configuration.VariabilityLowerPercentage,
                    _configuration.VariabilityUpperPercentage
                );
                subHeader.SaveSummarySection(section);
            } else {
                var section = new CumulativeIndividualConcentrationsSection() {
                    SectionLabel = getSectionLabel(BiologicalMatrixConcentrationComparisonsSections.MonitoringModelledCumulativeConcentrationSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Comparison cumulative",
                    order
                );
                section.Summarize(
                    data.AggregateIndividualExposures,
                    data.HbmCumulativeIndividualCollection,
                    data.ReferenceSubstance,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    data.TargetExposureUnit,
                    _configuration.VariabilityLowerPercentage,
                    _configuration.VariabilityUpperPercentage
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private void SummarizeMonitoringVersusModelResults(
            BiologicalMatrixConcentrationComparisonsActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            if (_configuration.ExposureType == ExposureType.Acute) {
                var section = new HbmVsModelledIndividualDayConcentrationsBySubstanceSection() {
                    SectionLabel = getSectionLabel(BiologicalMatrixConcentrationComparisonsSections.MonitoringModelledBySubstanceConcentrationSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Comparisons by substance",
                    order
                );
                section.Summarize(
                    data.AggregateIndividualDayExposures,
                    data.HbmIndividualDayCollections,
                    data.ActiveSubstances,
                    data.TargetExposureUnit,
                    _configuration.VariabilityLowerPercentage,
                    _configuration.VariabilityUpperPercentage
                );
                subHeader.SaveSummarySection(section);
            } else {
                var section = new HbmVsModelledIndividualConcentrationsBySubstanceSection() {
                    SectionLabel = getSectionLabel(BiologicalMatrixConcentrationComparisonsSections.MonitoringModelledBySubstanceConcentrationSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Comparisons by substance",
                    order
                );
                section.Summarize(
                    data.AggregateIndividualExposures,
                    data.HbmIndividualCollections,
                    data.ActiveSubstances,
                    data.TargetExposureUnit,
                    _configuration.VariabilityLowerPercentage,
                    _configuration.VariabilityUpperPercentage
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private void summarizeMonotoringVersusModelCorrelationsBySubstance(
            BiologicalMatrixConcentrationComparisonsActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            if (_configuration.ExposureType == ExposureType.Acute) {
                var section = new DayConcentrationCorrelationsBySubstanceSection() {
                    SectionLabel = getSectionLabel(BiologicalMatrixConcentrationComparisonsSections.MonotoringVersusModelCorrelationsBySubstanceSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Correlations by substance",
                    order
                );
                section.Summarize(
                    data.AggregateIndividualDayExposures,
                    data.HbmIndividualDayCollections,
                    data.ActiveSubstances,
                    data.TargetExposureUnit,
                    _configuration.VariabilityLowerPercentage,
                    _configuration.VariabilityUpperPercentage
                );
                subHeader.SaveSummarySection(section);
            } else {
                var section = new IndividualConcentrationCorrelationsBySubstanceSection() {
                    SectionLabel = getSectionLabel(BiologicalMatrixConcentrationComparisonsSections.MonotoringVersusModelCorrelationsBySubstanceSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Correlations by substance",
                    order
                );
                section.Summarize(
                    data.AggregateIndividualExposures,
                    data.HbmIndividualCollections,
                    data.ActiveSubstances,
                    data.TargetExposureUnit,
                    _configuration.VariabilityLowerPercentage,
                    _configuration.VariabilityUpperPercentage
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private void summarizeMonotoringVersusModelCorrelationsCumulative(
            BiologicalMatrixConcentrationComparisonsActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            if (_configuration.ExposureType == ExposureType.Acute) {
                var section = new DayConcentrationCorrelationsCumulativeSection() {
                    SectionLabel = getSectionLabel(BiologicalMatrixConcentrationComparisonsSections.MonotoringVersusModelCorrelationsCumulativeSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Correlations cumulative",
                    order
                );
                section.Summarize(
                    data.AggregateIndividualDayExposures,
                    data.HbmIndividualDayCollections,
                    data.ActiveSubstances,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    data.TargetExposureUnit,
                    _configuration.VariabilityLowerPercentage,
                    _configuration.VariabilityUpperPercentage
                );
                subHeader.SaveSummarySection(section);
            } else {
                var section = new IndividualConcentrationCorrelationsCumulativeSection() {
                    SectionLabel = getSectionLabel(BiologicalMatrixConcentrationComparisonsSections.MonotoringVersusModelCorrelationsCumulativeSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Correlations cumulative",
                    order
                );
                section.Summarize(
                    data.AggregateIndividualExposures,
                    data.HbmIndividualCollections,
                    data.ActiveSubstances,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    data.TargetExposureUnit,
                    _configuration.VariabilityLowerPercentage,
                    _configuration.VariabilityUpperPercentage
                );
                subHeader.SaveSummarySection(section);
            }
        }
    }
}
