using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.ComponentCalculation.DriverSubstanceCalculation;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
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

            if ((data.HbmCumulativeIndividualCollections != null || data.HbmCumulativeIndividualDayCollections != null)
                && outputSettings.ShouldSummarize(HumanMonitoringAnalysisSections.CumulativeConcentrationsSections)
            ) {
                SummarizeCumulativeConcentrations(
                    data.HbmCumulativeIndividualDayCollections,
                    data.HbmCumulativeIndividualCollections,
                    project.AssessmentSettings.ExposureType,
                    project.OutputDetailSettings.LowerPercentage,
                    project.OutputDetailSettings.UpperPercentage,
                    subHeader,
                    subOrder++
                 );
            }
            if (outputSettings.ShouldSummarize(HumanMonitoringAnalysisSections.MonitoringConcentrationsBySamplingMethodSubstanceSection)
                && data.HbmIndividualDayCollections.Any()) {
                SummarizeMonitoringConcentrationsBySamplingMethodSubstance(
                    data.HbmIndividualDayCollections,
                    data.HbmIndividualCollections,
                    data.ActiveSubstances,
                    project.AssessmentSettings.ExposureType,
                    project.OutputDetailSettings.LowerPercentage,
                    project.OutputDetailSettings.UpperPercentage,
                    subHeader,
                    subOrder++
                 );
            }

            if (project.OutputDetailSettings.StoreIndividualDayIntakes
                && outputSettings.ShouldSummarize(HumanMonitoringAnalysisSections.IndividualMonitoringConcentrationsSection)
                && data.HbmIndividualDayCollections.Any()) {
                SummarizeIndividualMonitoringConcentrations(
                    data.ActiveSubstances,
                    data.HbmSamplingMethods.First(),
                    data.HbmIndividualDayCollections,
                    data.HbmIndividualCollections,
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
            //TODO
            if (project.MixtureSelectionSettings.IsMcrAnalysis
                && data.ActiveSubstances.Count > 1
                && actionResult.ExposureMatrix != null
                && outputSettings.ShouldSummarize(HumanMonitoringAnalysisSections.McrCoExposureSection)
            ) {
                SummarizeMaximumCumulativeRatio(
                    actionResult.DriverSubstances,
                    actionResult.ExposureMatrix,
                    data.HbmIndividualDayCollections.FirstOrDefault().TargetUnit,
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

        public void SummarizeUncertain(
           ProjectDto project,
           HumanMonitoringAnalysisActionResult result,
           ActionData data,
           SectionHeader header
        ) {
            var outputSettings = new ModuleOutputSectionsManager<HumanMonitoringAnalysisSections>(project, ActionType);
            var subHeader = header.GetSubSectionHeader<HumanMonitoringAnalysisSummarySection>();
            if (subHeader == null) {
                return;
            }
            var outputSummary = (ActionSummaryBase)subHeader.GetSummarySection();
            if (project.AssessmentSettings.ExposureType == ExposureType.Acute
                && result.HbmIndividualDayConcentrations.Any()
                && outputSettings.ShouldSummarize(HumanMonitoringAnalysisSections.MonitoringConcentrationsBySamplingMethodSubstanceSection)
            ) {
                subHeader = header.GetSubSectionHeader<HbmIndividualDayDistributionBySubstanceSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as HbmIndividualDayDistributionBySubstanceSection;
                    section.SummarizeUncertainty(
                        data.HbmIndividualDayCollections,
                        data.ActiveSubstances,
                        project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                        project.UncertaintyAnalysisSettings.UncertaintyUpperBound
                    );
                    subHeader.SaveSummarySection(section);
                }
            } else if (project.AssessmentSettings.ExposureType == ExposureType.Chronic
                && result.HbmIndividualConcentrations.Any()
                && outputSettings.ShouldSummarize(HumanMonitoringAnalysisSections.MonitoringConcentrationsBySamplingMethodSubstanceSection)
            ) {
                subHeader = header.GetSubSectionHeader<HbmIndividualDistributionBySubstanceSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as HbmIndividualDistributionBySubstanceSection;
                    section.SummarizeUncertainty(
                        data.HbmIndividualCollections,
                        data.ActiveSubstances,
                        project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                        project.UncertaintyAnalysisSettings.UncertaintyUpperBound
                    );
                    subHeader.SaveSummarySection(section);
                }
            }
        }


        private static List<ActionSummaryUnitRecord> CollectUnits(ProjectDto project, ActionData data) {
            var actionSummaryUnitRecords = new List<ActionSummaryUnitRecord> {
                new ActionSummaryUnitRecord("LowerPercentage", $"p{project.OutputDetailSettings.LowerPercentage}"),
                new ActionSummaryUnitRecord("UpperPercentage", $"p{project.OutputDetailSettings.UpperPercentage}"),
                new ActionSummaryUnitRecord("LowerBound", $"p{project.UncertaintyAnalysisSettings.UncertaintyLowerBound}"),
                new ActionSummaryUnitRecord("UpperBound", $"p{project.UncertaintyAnalysisSettings.UncertaintyUpperBound}")
            };

            // TODO: check whether this is still used/needed. Remove if possible.
            var uniqueTargetUnits = data.HbmIndividualDayCollections.Select(c => c.TargetUnit);
            actionSummaryUnitRecords.AddRange(
                uniqueTargetUnits
                    .Select(u => new ActionSummaryUnitRecord(
                        u.Target.Code,
                        u.GetShortDisplayName(TargetUnit.DisplayOption.AppendExpressionType)
                    ))
                );

            if (project.AssessmentSettings.ExposureType == ExposureType.Chronic) {
                actionSummaryUnitRecords.Add(new ActionSummaryUnitRecord("IndividualDayUnit", "individuals"));
            } else {
                actionSummaryUnitRecords.Add(new ActionSummaryUnitRecord("IndividualDayUnit", "individual days"));
            }
            return actionSummaryUnitRecords;
        }

        private void SummarizeMonitoringConcentrationsBySamplingMethodSubstance(
            ICollection<HbmIndividualDayCollection> hbmIndividualDayCollections,
            ICollection<HbmIndividualCollection> hbmIndividualCollections,
            ICollection<Compound> activeSubstances,
            ExposureType exposureType,
            double lowerPercentage,
            double upperPercentage,
            SectionHeader header,
            int order
        ) {
            if (exposureType == ExposureType.Acute) {
                var section = new HbmIndividualDayDistributionBySubstanceSection() {
                    SectionLabel = getSectionLabel(HumanMonitoringAnalysisSections.MonitoringConcentrationsBySamplingMethodSubstanceSection),
                    Units = header.Units
                };
                var subHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Concentrations by substance",
                    order
                );
                section.Summarize(
                    hbmIndividualDayCollections,
                    activeSubstances,
                    lowerPercentage,
                    upperPercentage
                 );
                subHeader.SaveSummarySection(section);
            } else {
                var section = new HbmIndividualDistributionBySubstanceSection() {
                    SectionLabel = getSectionLabel(HumanMonitoringAnalysisSections.MonitoringConcentrationsBySamplingMethodSubstanceSection),
                    Units = header.Units
                };
                var subHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Concentrations by substance",
                    order
                );
                section.Summarize(
                    hbmIndividualCollections,
                    activeSubstances,
                    lowerPercentage,
                    upperPercentage
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private void SummarizeCumulativeConcentrations(
            ICollection<HbmCumulativeIndividualDayCollection> hbmCumulativeIndividualDayCollections,
            ICollection<HbmCumulativeIndividualCollection> hbmCumulativeIndividualCollections,
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
                    hbmCumulativeIndividualDayCollections,
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
                    hbmCumulativeIndividualCollections,
                    lowerPercentage,
                    upperPercentage
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private void SummarizeIndividualMonitoringConcentrations(
            ICollection<Compound> activeSubstances,
            HumanMonitoringSamplingMethod samplingMethod,
            ICollection<HbmIndividualDayCollection> hbmIndividualDayCollections,
            ICollection<HbmIndividualCollection> hbmIndividualCollections,
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
                    hbmIndividualDayCollections,
                    activeSubstances,
                    samplingMethod
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
                    hbmIndividualCollections,
                    activeSubstances,
                    samplingMethod
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
            TargetUnit targetUnit,
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
                    targetUnit,
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
