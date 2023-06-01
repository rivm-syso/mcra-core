using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.BiologicalMatrixConcentrationComparisons {
    public enum BiologicalMatrixConcentrationComparisonsSections {
        MonitoringModelledCumulativeConcentrationSection,
        MonitoringModelledBySubstanceConcentrationSection,
        MonotoringVersusModelCorrelationsBySubstanceSection,
        MonotoringVersusModelCorrelationsCumulativeSection
    }

    public sealed class BiologicalMatrixConcentrationComparisonsSummarizer : ActionResultsSummarizerBase<BiologicalMatrixConcentrationComparisonsActionResult> {

        public override ActionType ActionType => ActionType.BiologicalMatrixConcentrationComparisons;

        private CompositeProgressState _progressState;
        public BiologicalMatrixConcentrationComparisonsSummarizer(CompositeProgressState progressState = null) {
            _progressState = progressState;
        }

        public override void Summarize(ProjectDto project, BiologicalMatrixConcentrationComparisonsActionResult result, ActionData data, SectionHeader header, int order) {
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
                    data.ActiveSubstances,
                    data.AggregateIndividualDayExposures,
                    data.AggregateIndividualExposures,
                    data.HbmCumulativeIndividualDayConcentrations,
                    data.HbmCumulativeIndividualConcentrations,
                    data.ReferenceSubstance,
                    project.KineticModelSettings.BiologicalMatrix,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    project.AssessmentSettings.ExposureType,
                    data.TargetExposureUnit,
                    data.HbmTargetConcentrationUnits,
                    subHeader,
                    subOrder++
                );
            }
            if (outputSettings.ShouldSummarize(BiologicalMatrixConcentrationComparisonsSections.MonitoringModelledBySubstanceConcentrationSection)) {
                summarizeMonotoringVersusModelResults(
                    project,
                    data.ActiveSubstances,
                    data.AggregateIndividualDayExposures,
                    data.AggregateIndividualExposures,
                    data.HbmIndividualDayConcentrations,
                    data.HbmIndividualConcentrations,
                    project.KineticModelSettings.BiologicalMatrix,
                    data.TargetExposureUnit,
                    data.HbmTargetConcentrationUnits,
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
                    data.HbmIndividualDayConcentrations,
                    data.HbmIndividualConcentrations,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    project.KineticModelSettings.BiologicalMatrix,
                    data.TargetExposureUnit,
                    data.HbmTargetConcentrationUnits,
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
                    data.HbmIndividualDayConcentrations,
                    data.HbmIndividualConcentrations,
                    project.KineticModelSettings.BiologicalMatrix,
                    data.TargetExposureUnit,
                    data.HbmTargetConcentrationUnits,
                    project.AssessmentSettings.ExposureType,
                    subHeader,
                    subOrder++
                );
            }
            subHeader.SaveSummarySection(outputSummary);
        }

        private static List<ActionSummaryUnitRecord> collectUnits(ProjectDto project, ActionData data) {
            var result = new List<ActionSummaryUnitRecord> {
                new ActionSummaryUnitRecord("MonitoringConcentrationUnit", string.Join(" or ", data.HbmTargetConcentrationUnits.Select(t => t.GetShortDisplayName(TargetUnit.DisplayOption.AppendBiologicalMatrix)))),
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
            ICollection<Compound> activeSubstances,
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<HbmCumulativeIndividualDayConcentration> hbmCumulativeIndividualDayConcentrations,
            ICollection<HbmCumulativeIndividualConcentration> hbmCumulativeIndividualConcentrations,
            Compound referenceSubstance,
            BiologicalMatrix biologicalMatrix,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ExposureType exposureType,
            TargetUnit targetExposureUnit,
            List<TargetUnit> hbmConcentrationUnits,
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
                    hbmCumulativeIndividualDayConcentrations,
                    activeSubstances,
                    referenceSubstance,
                    biologicalMatrix,
                    relativePotencyFactors,
                    membershipProbabilities,
                    targetExposureUnit,
                    hbmConcentrationUnits,
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
                    hbmCumulativeIndividualConcentrations,
                    activeSubstances,
                    referenceSubstance,
                    biologicalMatrix,
                    relativePotencyFactors,
                    membershipProbabilities,
                    targetExposureUnit,
                    hbmConcentrationUnits,
                    project.OutputDetailSettings.LowerPercentage,
                    project.OutputDetailSettings.UpperPercentage
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private void summarizeMonotoringVersusModelResults(
            ProjectDto project,
            ICollection<Compound> activeSubstances,
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<HbmIndividualDayConcentration> hbmIndividualDayConcentrations,
            ICollection<HbmIndividualConcentration> hbmIndividualConcentrations,
            BiologicalMatrix hbmBiologicalMatrix,
            TargetUnit targetExposureUnit,
            List<TargetUnit> hbmConcentrationUnits,
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
                    hbmIndividualDayConcentrations,
                    activeSubstances,
                    targetExposureUnit,
                    hbmConcentrationUnits,
                    project.OutputDetailSettings.LowerPercentage,
                    project.OutputDetailSettings.UpperPercentage,
                    hbmBiologicalMatrix
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
                    hbmIndividualConcentrations,
                    activeSubstances,
                    targetExposureUnit,
                    hbmConcentrationUnits,
                    project.OutputDetailSettings.LowerPercentage,
                    project.OutputDetailSettings.UpperPercentage,
                    hbmBiologicalMatrix
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private void summarizeMonotoringVersusModelCorrelationsBySubstance(
            ProjectDto project,
            ICollection<Compound> activeSubstances,
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<HbmIndividualDayConcentration> hbmIndividualDayConcentrations,
            ICollection<HbmIndividualConcentration> hbmIndividualConcentrations,
            BiologicalMatrix hbmBiologicalMatrix,
            TargetUnit targetExposureUnit,
            List<TargetUnit> hbmConcentrationUnits,
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
                    hbmIndividualDayConcentrations,
                    activeSubstances,
                    targetExposureUnit,
                    hbmConcentrationUnits,
                    project.OutputDetailSettings.LowerPercentage,
                    project.OutputDetailSettings.UpperPercentage);
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
                    hbmIndividualConcentrations,
                    activeSubstances,
                    targetExposureUnit,
                    hbmConcentrationUnits,
                    project.OutputDetailSettings.LowerPercentage,
                    project.OutputDetailSettings.UpperPercentage);
                subHeader.SaveSummarySection(section);
            }
        }

        private void summarizeMonotoringVersusModelCorrelationsCumulative(
           ProjectDto project,
           ICollection<Compound> activeSubstances,
           ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
           ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
           ICollection<HbmIndividualDayConcentration> hbmIndividualDayConcentrations,
           ICollection<HbmIndividualConcentration> hbmIndividualConcentrations,
           IDictionary<Compound, double> relativePotencyFactors,
           IDictionary<Compound, double> membershipProbabilities,
           BiologicalMatrix hbmBiologicalMatrix,
           TargetUnit targetExposureUnit,
           List<TargetUnit> hbmConcentrationUnits,
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
                    hbmIndividualDayConcentrations,
                    activeSubstances,
                    relativePotencyFactors,
                    membershipProbabilities,
                    targetExposureUnit,
                    hbmConcentrationUnits,
                    project.OutputDetailSettings.LowerPercentage,
                    project.OutputDetailSettings.UpperPercentage);
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
                    hbmIndividualConcentrations,
                    activeSubstances,
                    relativePotencyFactors,
                    membershipProbabilities,
                    targetExposureUnit,
                    hbmConcentrationUnits,
                    project.OutputDetailSettings.LowerPercentage,
                    project.OutputDetailSettings.UpperPercentage);
                subHeader.SaveSummarySection(section);
            }
        }
    }
}
