using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.BiologicalMatrixConcentrationComparisons {
    public enum BiologicalMatrixConcentrationComparisonsSections {
        MonitoringModelledCumulativeConcentrationSection,
        MonitoringModelledBySubstanceConcentrationSection,
        MonotoringVersusModelCorrelationsBySubstanceSection,
        MonotoringVersusModelCorrelationsCumulativeSection
    }

    public sealed class BiologicalMatrixConcentrationComparisonsSummarizer : ActionResultsSummarizerBase<BiologicalMatrixConcentrationComparisonsActionResult> {

        public override ActionType ActionType => ActionType.BiologicalMatrixConcentrationComparisons;

        public BiologicalMatrixConcentrationComparisonsSummarizer() {
        }

        public override void Summarize(
            ProjectDto project, 
            BiologicalMatrixConcentrationComparisonsActionResult result, 
            ActionData data, 
            SectionHeader header, 
            int order
        ) {
            var outputSettings = new ModuleOutputSectionsManager<BiologicalMatrixConcentrationComparisonsSections>(project, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var outputSummary = new BiologicalMatrixConcentrationComparisonsSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(outputSummary, ActionType.GetDisplayName(), order);
            var subOrder = 0;
            subHeader.Units = collectUnits(project, data);

            if (outputSettings.ShouldSummarize(BiologicalMatrixConcentrationComparisonsSections.MonitoringModelledCumulativeConcentrationSection)) {
                summarizeCumulativeMonitoringVsModelledConcentrations(
                    project,
                    data.AggregateIndividualDayExposures,
                    data.AggregateIndividualExposures,
                    data.HbmCumulativeIndividualDayCollection,
                    data.HbmCumulativeIndividualCollection,
                    data.ReferenceSubstance,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    project.AssessmentSettings.ExposureType,
                    data.TargetExposureUnit,
                    subHeader,
                    subOrder++
                );
            }
            if (outputSettings.ShouldSummarize(BiologicalMatrixConcentrationComparisonsSections.MonitoringModelledBySubstanceConcentrationSection)) {
                SummarizeMonitoringVersusModelResults(
                    project,
                    data.ActiveSubstances,
                    data.AggregateIndividualDayExposures,
                    data.AggregateIndividualExposures,
                    data.HbmIndividualDayCollections,
                    data.HbmIndividualCollections,
                    data.TargetExposureUnit,
                    project.AssessmentSettings.ExposureType,
                    subHeader,
                    subOrder++
                );
            }
            if (outputSettings.ShouldSummarize(BiologicalMatrixConcentrationComparisonsSections.MonotoringVersusModelCorrelationsCumulativeSection)
                && project.HumanMonitoringSettings.CorrelateTargetConcentrations) {
                summarizeMonotoringVersusModelCorrelationsCumulative(
                    project,
                    data.ActiveSubstances,
                    data.AggregateIndividualDayExposures,
                    data.AggregateIndividualExposures,
                    data.HbmIndividualDayCollections,
                    data.HbmIndividualCollections,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    data.TargetExposureUnit,
                    project.AssessmentSettings.ExposureType,
                    subHeader,
                    subOrder++
                );
            }
            if (outputSettings.ShouldSummarize(BiologicalMatrixConcentrationComparisonsSections.MonotoringVersusModelCorrelationsBySubstanceSection)
                && project.HumanMonitoringSettings.CorrelateTargetConcentrations) {
                summarizeMonotoringVersusModelCorrelationsBySubstance(
                    project,
                    data.ActiveSubstances,
                    data.AggregateIndividualDayExposures,
                    data.AggregateIndividualExposures,
                    data.HbmIndividualDayCollections, 
                    data.HbmIndividualCollections,
                    data.TargetExposureUnit,
                    project.AssessmentSettings.ExposureType,
                    subHeader,
                    subOrder++
                );
            }
            subHeader.SaveSummarySection(outputSummary);
        }

        private static List<ActionSummaryUnitRecord> collectUnits(ProjectDto project, ActionData data) {
            var result = new List<ActionSummaryUnitRecord> {
                new ActionSummaryUnitRecord("ModelledExposureUnit", data.TargetExposureUnit.GetShortDisplayName()),
                new ActionSummaryUnitRecord("LowerPercentage", $"p{project.OutputDetailSettings.LowerPercentage}"),
                new ActionSummaryUnitRecord("UpperPercentage", $"p{project.OutputDetailSettings.UpperPercentage}")
            };
            if (project.AssessmentSettings.ExposureType == ExposureType.Chronic) {
                result.Add(new ActionSummaryUnitRecord("IndividualDayUnit", "individuals"));
            } else {
                result.Add(new ActionSummaryUnitRecord("IndividualDayUnit", "individual days"));
            }
            return result;
        }

        private void summarizeCumulativeMonitoringVsModelledConcentrations(
            ProjectDto project,
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
            if (exposureType == ExposureType.Acute && aggregateIndividualDayExposures != null) {
                var section = new CumulativeDayConcentrationsSection() {
                    SectionLabel = getSectionLabel(BiologicalMatrixConcentrationComparisonsSections.MonitoringModelledCumulativeConcentrationSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Comparison cumulative",
                    order
                );
                section.Summarize(
                    aggregateIndividualDayExposures.Cast<ITargetIndividualDayExposure>().ToList(),
                    hbmCumulativeIndividualDayCollection,
                    referenceSubstance,
                    relativePotencyFactors,
                    membershipProbabilities,
                    targetExposureUnit,
                    project.OutputDetailSettings.LowerPercentage,
                    project.OutputDetailSettings.UpperPercentage
                );
                subHeader.SaveSummarySection(section);
            } else if (exposureType == ExposureType.Chronic && aggregateIndividualExposures != null) {
                var section = new CumulativeIndividualConcentrationsSection() {
                    SectionLabel = getSectionLabel(BiologicalMatrixConcentrationComparisonsSections.MonitoringModelledCumulativeConcentrationSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Comparison cumulative",
                    order
                );
                section.Summarize(
                    aggregateIndividualExposures.Cast<ITargetIndividualExposure>().ToList(),
                    hbmCumulativeIndividualCollection,
                    referenceSubstance,
                    relativePotencyFactors,
                    membershipProbabilities,
                    targetExposureUnit,
                    project.OutputDetailSettings.LowerPercentage,
                    project.OutputDetailSettings.UpperPercentage
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private void SummarizeMonitoringVersusModelResults(
            ProjectDto project,
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
                    aggregateIndividualDayExposures.Cast<ITargetIndividualDayExposure>().ToList(),
                    hbmIndividualDayConcentrationsCollections,
                    activeSubstances,
                    targetExposureUnit,
                    project.OutputDetailSettings.LowerPercentage,
                    project.OutputDetailSettings.UpperPercentage
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
                    aggregateIndividualExposures.Cast<ITargetIndividualExposure>().ToList(),
                    hbmIndividualConcentrationsCollections,
                    activeSubstances,
                    targetExposureUnit,
                    project.OutputDetailSettings.LowerPercentage,
                    project.OutputDetailSettings.UpperPercentage
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private void summarizeMonotoringVersusModelCorrelationsBySubstance(
            ProjectDto project,
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
                    project.OutputDetailSettings.LowerPercentage,
                    project.OutputDetailSettings.UpperPercentage
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
                    project.OutputDetailSettings.LowerPercentage,
                    project.OutputDetailSettings.UpperPercentage
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private void summarizeMonotoringVersusModelCorrelationsCumulative(
           ProjectDto project,
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
                    project.OutputDetailSettings.LowerPercentage,
                    project.OutputDetailSettings.UpperPercentage
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
                    project.OutputDetailSettings.LowerPercentage,
                    project.OutputDetailSettings.UpperPercentage
                );
                subHeader.SaveSummarySection(section);
            }
        }
    }
}
