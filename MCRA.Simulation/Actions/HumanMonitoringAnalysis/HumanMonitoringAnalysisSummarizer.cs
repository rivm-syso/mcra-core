using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.ComponentCalculation.DriverSubstanceCalculation;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.HumanMonitoringAnalysis {
    public enum HumanMonitoringAnalysisSections {
        DetailsSection,
        CumulativeConcentrationsSections,
        MonitoringConcentrationsBySamplingMethodSubstanceSection,
        IndividualMonitoringConcentrationsSection,
        ConcentrationModelSection,
        McrCoExposureSection
    }

    public sealed class HumanMonitoringAnalysisSummarizer : ActionResultsSummarizerBase<HumanMonitoringAnalysisActionResult> {

        public override ActionType ActionType => ActionType.HumanMonitoringAnalysis;

        public override void Summarize(ProjectDto project, HumanMonitoringAnalysisActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<HumanMonitoringAnalysisSections>(project, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var outputSummary = new HumanMonitoringAnalysisSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(outputSummary, ActionType.GetDisplayName(), order);
            var subOrder = 0;
            subHeader.Units = CollectUnits(project, data);

            var subHeaderDetails = subHeader.AddEmptySubSectionHeader("Details", order, getSectionLabel(HumanMonitoringAnalysisSections.DetailsSection));
            subHeaderDetails.SaveSummarySection(outputSummary);

            if ((data.HbmCumulativeIndividualConcentrations != null || data.HbmCumulativeIndividualDayConcentrations != null)
                && outputSettings.ShouldSummarize(HumanMonitoringAnalysisSections.CumulativeConcentrationsSections)
            ) {
                SummarizeCumulativeConcentrations(
                    data.HbmCumulativeIndividualDayConcentrations,
                    data.HbmCumulativeIndividualConcentrations,
                    project.HumanMonitoringSettings.TargetMatrix,
                    project.AssessmentSettings.ExposureType,
                    project.OutputDetailSettings.LowerPercentage,
                    project.OutputDetailSettings.UpperPercentage,
                    subHeader,
                    subOrder++
                 );
            }
            if (outputSettings.ShouldSummarize(HumanMonitoringAnalysisSections.MonitoringConcentrationsBySamplingMethodSubstanceSection)
                && data.HbmIndividualDayConcentrations.Any()) {
                SummarizeMonitoringConcentrationsBySamplingMethodSubstance(
                    data.HbmIndividualDayConcentrations,
                    data.HbmIndividualConcentrations,
                    data.ActiveSubstances,
                    project.HumanMonitoringSettings.TargetMatrix,
                    project.AssessmentSettings.ExposureType,
                    project.OutputDetailSettings.LowerPercentage,
                    project.OutputDetailSettings.UpperPercentage,
                    subHeader,
                    subOrder++
                 );
            }

            if (project.OutputDetailSettings.StoreIndividualDayIntakes
                && outputSettings.ShouldSummarize(HumanMonitoringAnalysisSections.IndividualMonitoringConcentrationsSection)
                && data.HbmIndividualDayConcentrations.Any()) {
                SummarizeIndividualMonitoringConcentrations(
                    data.ActiveSubstances,
                    data.HbmSamplingMethods.First(),
                    data.HbmIndividualDayConcentrations,
                    data.HbmIndividualConcentrations,
                    project.AssessmentSettings.ExposureType,
                    subHeaderDetails,
                    subOrder++
                );
            }

            if (actionResult.HbmConcentrationModels != null && outputSettings.ShouldSummarize(HumanMonitoringAnalysisSections.IndividualMonitoringConcentrationsSection)) {
                SummarizeConcentrationModels(
                    actionResult.HbmConcentrationModels,
                    subHeaderDetails,
                    subOrder++
                );
            }

            // MCR co-exposures
            if (project.MixtureSelectionSettings.IsMcrAnalysis
                && data.ActiveSubstances.Count > 1
                && actionResult.ExposureMatrix != null
                && outputSettings.ShouldSummarize(HumanMonitoringAnalysisSections.McrCoExposureSection)
            ) {
                // TODO. TIJ 10-03-2023
                System.Diagnostics.Debug.Assert(data.HbmTargetConcentrationUnits.Count > 0);
                var intakeUnit = data.HbmTargetConcentrationUnits.FirstOrDefault();

                SummarizeMaximumCumulativeRatio(
                    actionResult.DriverSubstances,
                    actionResult.ExposureMatrix,
                    intakeUnit,
                    project.MixtureSelectionSettings.McrExposureApproachType,
                    project.OutputDetailSettings.MaximumCumulativeRatioCutOff,
                    project.OutputDetailSettings.MaximumCumulativeRatioPercentiles,
                    project.MixtureSelectionSettings.TotalExposureCutOff,
                    project.OutputDetailSettings.MaximumCumulativeRatioMinimumPercentage,
                    subHeaderDetails,
                    subOrder++
                );
            }

            subHeader.SaveSummarySection(outputSummary);
        }

        private static List<ActionSummaryUnitRecord> CollectUnits(ProjectDto project, ActionData data) {
            var result = new List<ActionSummaryUnitRecord> {
                new ActionSummaryUnitRecord("MonitoringConcentrationUnit", string.Join(" or ", data.HbmTargetConcentrationUnits.Select(t => t.GetShortDisplayName(TargetUnit.DisplayOption.AppendExpressionType)))),
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

        private void SummarizeMonitoringConcentrationsBySamplingMethodSubstance(
            ICollection<HbmIndividualDayConcentration> hbmIndividualDayConcentrations,
            ICollection<HbmIndividualConcentration> hbmIndividualConcentrations,
            ICollection<Compound> activeSubstances,
            BiologicalMatrix biologicalMatrix,
            ExposureType exposureType,
            double lowerPercentage,
            double upperPercentage,
            SectionHeader header,
            int order
        ) {
            if (exposureType == ExposureType.Acute) {
                var section = new HbmIndividualDayDistributionBySubstanceSection() {
                    SectionLabel = getSectionLabel(HumanMonitoringAnalysisSections.MonitoringConcentrationsBySamplingMethodSubstanceSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Concentrations by substance",
                    order
                );
                section.Summarize(
                    hbmIndividualDayConcentrations,
                    activeSubstances,
                    biologicalMatrix,
                    lowerPercentage,
                    upperPercentage
                 );
                subHeader.SaveSummarySection(section);
            } else {
                var section = new HbmIndividualDistributionBySubstanceSection() {
                    SectionLabel = getSectionLabel(HumanMonitoringAnalysisSections.MonitoringConcentrationsBySamplingMethodSubstanceSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Concentrations by substance",
                    order
                );
                section.Summarize(
                    hbmIndividualConcentrations,
                    activeSubstances,
                    biologicalMatrix,
                    lowerPercentage,
                    upperPercentage
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private void SummarizeCumulativeConcentrations(
            ICollection<HbmCumulativeIndividualDayConcentration> hbmCumulativeIndividualDayConcentrations,
            ICollection<HbmCumulativeIndividualConcentration> hbmCumulativeIndividualConcentrations,
            BiologicalMatrix biologicalMatrix,
            ExposureType exposureType,
            double lowerPercentage,
            double upperPercentage,
            SectionHeader header,
            int order
        ) {
            if (exposureType == ExposureType.Acute) {
                var section = new HbmCumulativeIndividualDayDistributionsSection() {
                    SectionLabel = getSectionLabel(HumanMonitoringAnalysisSections.CumulativeConcentrationsSections)
                };
                var subHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Cumulative concentrations",
                    order
                );
                section.Summarize(
                    hbmCumulativeIndividualDayConcentrations,
                    biologicalMatrix,
                    lowerPercentage,
                    upperPercentage
                 );
                subHeader.SaveSummarySection(section);
            } else {
                var section = new HbmCumulativeIndividualDistributionsSection() {
                    SectionLabel = getSectionLabel(HumanMonitoringAnalysisSections.CumulativeConcentrationsSections)
                };
                var subHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Cumulative concentrations",
                    order
                );
                section.Summarize(
                    hbmCumulativeIndividualConcentrations,
                    biologicalMatrix,
                    lowerPercentage,
                    upperPercentage
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private void SummarizeIndividualMonitoringConcentrations(
            ICollection<Compound> activeSubstances,
            HumanMonitoringSamplingMethod hbmBiologicalMatrix,
            ICollection<HbmIndividualDayConcentration> hbmIndividualDayConcentrations,
            ICollection<HbmIndividualConcentration> hbmIndividualConcentrations,
            ExposureType exposureType,
            SectionHeader header,
            int order
        ) {
            if (exposureType == ExposureType.Acute) {
                var section = new HbmIndividualDaySubstanceConcentrationsSection() {
                    SectionLabel = getSectionLabel(HumanMonitoringAnalysisSections.IndividualMonitoringConcentrationsSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Computed individual day concentrations by sampling method and substance",
                    order
                );
                section.Summarize(
                    hbmIndividualDayConcentrations,
                    activeSubstances,
                    hbmBiologicalMatrix
                );
                subHeader.SaveSummarySection(section);
            } else if (exposureType == ExposureType.Chronic) {
                var section = new HbmIndividualSubstanceConcentrationsSection() {
                    SectionLabel = getSectionLabel(HumanMonitoringAnalysisSections.IndividualMonitoringConcentrationsSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Computed individual concentrations by sampling method and substance",
                    order
                );
                section.Summarize(
                    hbmIndividualConcentrations,
                    activeSubstances,
                    hbmBiologicalMatrix
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private void SummarizeConcentrationModels(
            IDictionary<(HumanMonitoringSamplingMethod, Compound), ConcentrationModel> concentrationModels,
            SectionHeader header,
            int order
        ) {
            var section = new HbmConcentrationModelsSection() {
                SectionLabel = getSectionLabel(HumanMonitoringAnalysisSections.ConcentrationModelSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Concentration models for non detects imputation",
                order
            );
            section.Summarize(concentrationModels);
            subHeader.SaveSummarySection(section);
        }
        private void SummarizeMaximumCumulativeRatio(
            List<DriverSubstance> driverSubstances,
            ExposureMatrix exposureMatrix,
            TargetUnit intakeUnit,
            ExposureApproachType exposureApproachType,
            double maximumCumulativeRatioCutOff,
            double[] maximumCumulativeRatioPercentiles,
            double totalExposureCutOffPercentage,
            double maximumCumulativeRatioMinimumPercentage,
            SectionHeader header,
            int order
        ) {
            if (driverSubstances != null) {
                var mcrSection = new MaximumCumulativeRatioSection() {
                    SectionLabel = getSectionLabel(HumanMonitoringAnalysisSections.McrCoExposureSection)
                };
                var mcrHeader = header.AddSubSectionHeaderFor(
                    mcrSection,
                    "MCR co-exposure",
                    order++
                );
                mcrSection.Summarize(
                    driverSubstances,
                    intakeUnit,
                    exposureApproachType,
                    maximumCumulativeRatioCutOff,
                    maximumCumulativeRatioPercentiles,
                    totalExposureCutOffPercentage,
                    maximumCumulativeRatioMinimumPercentage
                );

                mcrSection.Summarize(
                    exposureMatrix,
                    maximumCumulativeRatioPercentiles,
                    maximumCumulativeRatioMinimumPercentage
                );
                mcrHeader.SaveSummarySection(mcrSection);
            }
        }
    }
}
