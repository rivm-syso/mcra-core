﻿using MCRA.Data.Compiled.Objects;
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
        CumulativeConcentrationsSections,
        HbmConcentrationsByTargetSubstanceSection,
        HbmConcentrationsByTargetSubstanceDetailsSection,
        IndividualMonitoringConcentrationsSection,
        ConcentrationModelSection,
        McrCoExposureSection,
        CumulativeRiskDriversSections,
        IndividualContributionsSections,
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
            subHeader.Units = CollectUnits(project, data);
            subHeader.SaveSummarySection(outputSummary);

            var subOrder = 0;

            // HBM cumulative concentrations
            if ((data.HbmCumulativeIndividualCollection != null || data.HbmCumulativeIndividualDayCollection != null)
                && outputSettings.ShouldSummarize(HumanMonitoringAnalysisSections.CumulativeConcentrationsSections)
            ) {
                SummarizeCumulativeConcentrations(
                    data.HbmCumulativeIndividualDayCollection,
                    data.HbmCumulativeIndividualCollection,
                    project.AssessmentSettings.ExposureType,
                    project.OutputDetailSettings.LowerPercentage,
                    project.OutputDetailSettings.UpperPercentage,
                    project.OutputDetailSettings.SkipPrivacySensitiveOutputs,
                    subHeader,
                    subOrder++
                 );
            }

            // HBM calculated (derived) concentrations by substance
            if (outputSettings.ShouldSummarize(HumanMonitoringAnalysisSections.HbmConcentrationsByTargetSubstanceSection)
                && data.HbmIndividualDayCollections.Any()
            ) {
                summarizeHbmConcentrationsByTargetSubstance(
                    data.HbmIndividualDayCollections,
                    data.HbmIndividualCollections,
                    data.ActiveSubstances,
                    project.AssessmentSettings.ExposureType,
                    project.OutputDetailSettings.LowerPercentage,
                    project.OutputDetailSettings.UpperPercentage,
                    project.OutputDetailSettings.SkipPrivacySensitiveOutputs,
                    subHeader,
                    subOrder++
                 );
            }

            // HBM measured (original) concentrations by substance
            if (outputSettings.ShouldSummarize(HumanMonitoringAnalysisSections.HbmConcentrationsByTargetSubstanceDetailsSection)
                && (actionResult.HbmMeasuredMatrixIndividualDayCollections?.Any() ?? false)
            ) {
                summarizeHbmConcentrationsByTargetSubstanceDetails(
                    actionResult.HbmMeasuredMatrixIndividualDayCollections,
                    actionResult.HbmMeasuredMatrixIndividualCollections,
                    data.AllCompounds,
                    project.AssessmentSettings.ExposureType,
                    project.OutputDetailSettings.LowerPercentage,
                    project.OutputDetailSettings.UpperPercentage,
                    project.OutputDetailSettings.SkipPrivacySensitiveOutputs,
                    subHeader,
                    subOrder++
                 );
            }

            // HBM contributions by substance
            if ((data.HbmCumulativeIndividualCollection != null || data.HbmCumulativeIndividualDayCollection != null)
                && outputSettings.ShouldSummarize(HumanMonitoringAnalysisSections.CumulativeRiskDriversSections) 
                && data.ActiveSubstances.Count() > 1
            ) {
                summarizeContributionsBySubstance(
                    data.HbmIndividualDayCollections,
                    data.HbmIndividualCollections,
                    data.HbmCumulativeIndividualDayCollection,
                    data.HbmCumulativeIndividualCollection,
                    data.ActiveSubstances,
                    data.CorrectedRelativePotencyFactors,
                    project,
                    subHeader,
                    subOrder++
                );
            }

            // HBM concentration models
            if (actionResult.HbmConcentrationModels != null
                && outputSettings.ShouldSummarize(HumanMonitoringAnalysisSections.IndividualMonitoringConcentrationsSection)
            ) {
                summarizeConcentrationModels(
                    actionResult.HbmConcentrationModels,
                    subHeader,
                    subOrder++
                );
            }

            // MCR co-exposures
            if (project.HumanMonitoringSettings.AnalyseMcr
                && data.ActiveSubstances.Count > 1
                && actionResult.ExposureMatrix != null
                && outputSettings.ShouldSummarize(HumanMonitoringAnalysisSections.McrCoExposureSection)
            ) {
                SummarizeMaximumCumulativeRatio(
                    actionResult.DriverSubstances,
                    actionResult.ExposureMatrix,
                    data.HbmIndividualDayCollections.FirstOrDefault().TargetUnit,
                    project.HumanMonitoringSettings.ExposureApproachType,
                    project.OutputDetailSettings.MaximumCumulativeRatioCutOff,
                    project.OutputDetailSettings.MaximumCumulativeRatioPercentiles,
                    project.MixtureSelectionSettings.TotalExposureCutOff,
                    project.OutputDetailSettings.MaximumCumulativeRatioMinimumPercentage,
                    project.OutputDetailSettings.SkipPrivacySensitiveOutputs,
                    subHeader,
                    subOrder++
                );
            }

            // HBM individual monitoring concentrations
            if (project.OutputDetailSettings.StoreIndividualDayIntakes
                && !project.OutputDetailSettings.SkipPrivacySensitiveOutputs
                && outputSettings.ShouldSummarize(HumanMonitoringAnalysisSections.IndividualMonitoringConcentrationsSection)
                && data.HbmIndividualDayCollections.Any()
            ) {
                SummarizeIndividualMonitoringConcentrations(
                    data.ActiveSubstances,
                    data.HbmSamplingMethods.First(),
                    data.HbmIndividualDayCollections,
                    data.HbmIndividualCollections,
                    project.AssessmentSettings.ExposureType,
                    subHeader,
                    subOrder++
                );
            }
        }

        public void SummarizeUncertain(
           ProjectDto project,
           HumanMonitoringAnalysisActionResult result,
           ActionData data,
           SectionHeader header
        ) {
            var subHeader = header.GetSubSectionHeader<HumanMonitoringAnalysisSummarySection>();
            if (subHeader == null) {
                return;
            }
            if (data.HbmCumulativeIndividualCollection != null || data.HbmCumulativeIndividualDayCollection != null) {
                summarizeCumulativeConcentrationseUncertainty(
                    data.HbmCumulativeIndividualDayCollection,
                    data.HbmCumulativeIndividualCollection,
                    data.ActiveSubstances,
                    project.AssessmentSettings.ExposureType,
                    project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                    project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                    header
                );

                summarizeRiskDriversUncertainty(
                    data.HbmIndividualDayCollections,
                    data.HbmIndividualCollections,
                    data.HbmCumulativeIndividualDayCollection,
                    data.HbmCumulativeIndividualCollection,
                    data.ActiveSubstances,
                    data.CorrectedRelativePotencyFactors,
                    project.AssessmentSettings.ExposureType,
                    project.OutputDetailSettings.PercentageForUpperTail,
                    project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                    project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                    header
                );

            }

            summarizeHbmConcentrationsByTargetSubstanceUncertainty(
                data.HbmIndividualDayCollections,
                data.HbmIndividualCollections,
                data.ActiveSubstances,
                project.AssessmentSettings.ExposureType,
                project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                header
            );

            if (result.HbmMeasuredMatrixIndividualDayCollections?.Any() ?? false) {
                summarizeHbmConcentrationsByTargetSubstanceDetailsUncertainty(
                    result.HbmMeasuredMatrixIndividualDayCollections,
                    result.HbmMeasuredMatrixIndividualCollections,
                    data.ActiveSubstances,
                    project.AssessmentSettings.ExposureType,
                    project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                    project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                    header
                );
            }

            // IndividualContributions
            summarizeHbmIndividualContributionsUncertainty(
                    data.HbmIndividualDayCollections,
                    data.HbmIndividualCollections,
                    data.ActiveSubstances,
                    data.CorrectedRelativePotencyFactors,
                    project.AssessmentSettings.ExposureType,
                    project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                    project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                    header
            );

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
                        u.Target.ExpressionType == ExpressionType.SpecificGravity
                            ? u.GetShortDisplayName()
                            : u.GetShortDisplayName(TargetUnit.DisplayOption.AppendExpressionType)
                    ))
                );

            if (project.AssessmentSettings.ExposureType == ExposureType.Chronic) {
                actionSummaryUnitRecords.Add(new ActionSummaryUnitRecord("IndividualDayUnit", "individuals"));
            } else {
                actionSummaryUnitRecords.Add(new ActionSummaryUnitRecord("IndividualDayUnit", "individual days"));
            }
            return actionSummaryUnitRecords;
        }

        private void summarizeHbmConcentrationsByTargetSubstance(
            ICollection<HbmIndividualDayCollection> hbmIndividualDayCollections,
            ICollection<HbmIndividualCollection> hbmIndividualCollections,
            ICollection<Compound> activeSubstances,
            ExposureType exposureType,
            double lowerPercentage,
            double upperPercentage,
            bool skipPrivacySensitiveOutputs,
            SectionHeader header,
            int order
        ) {
            if (exposureType == ExposureType.Acute) {
                var section = new HbmIndividualDayDistributionBySubstanceSection() {
                    SectionLabel = getSectionLabel(HumanMonitoringAnalysisSections.HbmConcentrationsByTargetSubstanceSection),
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
                    upperPercentage,
                    skipPrivacySensitiveOutputs
                 );
                subHeader.SaveSummarySection(section);
            } else {
                var section = new HbmIndividualDistributionBySubstanceSection() {
                    SectionLabel = getSectionLabel(HumanMonitoringAnalysisSections.HbmConcentrationsByTargetSubstanceSection),
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
                    upperPercentage,
                    skipPrivacySensitiveOutputs
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private void summarizeHbmConcentrationsByTargetSubstanceDetails(
           ICollection<HbmIndividualDayCollection> otherHbmIndividualDayCollections,
           ICollection<HbmIndividualCollection> otherHbmIndividualCollections,
           ICollection<Compound> substances,
           ExposureType exposureType,
           double lowerPercentage,
           double upperPercentage,
           bool skipPrivacySensitiveOutputs,
           SectionHeader header,
           int order
       ) {
            if (exposureType == ExposureType.Acute) {
                var section = new HbmIndividualDayDistributionBySubstanceDetailsSection() {
                    SectionLabel = getSectionLabel(HumanMonitoringAnalysisSections.HbmConcentrationsByTargetSubstanceDetailsSection),
                    Units = header.Units
                };
                var subHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Concentrations by substance before conversion",
                    order
                );
                section.Summarize(
                    otherHbmIndividualDayCollections,
                    substances,
                    lowerPercentage,
                    upperPercentage,
                    skipPrivacySensitiveOutputs
                 );
                subHeader.SaveSummarySection(section);
            } else {
                var section = new HbmIndividualDistributionBySubstanceDetailsSection() {
                    SectionLabel = getSectionLabel(HumanMonitoringAnalysisSections.HbmConcentrationsByTargetSubstanceDetailsSection),
                    Units = header.Units
                };
                var subHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Concentrations by substance before conversion",
                    order
                );
                section.Summarize(
                    otherHbmIndividualCollections,
                    substances,
                    lowerPercentage,
                    upperPercentage,
                    skipPrivacySensitiveOutputs
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private void summarizeContributionsBySubstance(
            ICollection<HbmIndividualDayCollection> hbmIndividualDayCollections,
            ICollection<HbmIndividualCollection> hbmIndividualCollections,
            HbmCumulativeIndividualDayCollection hbmCumulativeIndividualDayConcentrations,
            HbmCumulativeIndividualCollection hbmCumulativeIndividualConcentrations,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            ProjectDto project,
            SectionHeader header,
            int order
        ) {
            var section = new HbmRiskDriverSection() {
                SectionLabel = getSectionLabel(HumanMonitoringAnalysisSections.CumulativeRiskDriversSections)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Contributions by substance",
                order
            );

            // Contributions total
            if (hbmIndividualCollections != null || hbmIndividualDayCollections != null) {
                var section1 = new HbmTotalDistributionRiskDriversSection();
                var subHeader2 = subHeader.AddSubSectionHeaderFor(section1, "Contributions to cumulative concentrations for total distribution", order++);
                section1.Summarize(
                    hbmIndividualDayCollections,
                    hbmIndividualCollections,
                    hbmCumulativeIndividualDayConcentrations,
                    hbmCumulativeIndividualConcentrations,
                    activeSubstances,
                    relativePotencyFactors,
                    project.AssessmentSettings.ExposureType
                );

                subHeader2.SaveSummarySection(section1);
            }

            // Contributions upper
            if (hbmIndividualCollections != null || hbmIndividualDayCollections != null) {
                var section1 = new HbmUpperDistributionRiskDriversSection();
                var subHeader2 = subHeader.AddSubSectionHeaderFor(section1, "Contributions to cumulative concentrations for upper distribution", order++);
                section1.Summarize(
                    hbmIndividualDayCollections,
                    hbmIndividualCollections,
                    hbmCumulativeIndividualDayConcentrations,
                    hbmCumulativeIndividualConcentrations,
                    activeSubstances,
                    relativePotencyFactors,
                    project.AssessmentSettings.ExposureType,
                    project.OutputDetailSettings.PercentageForUpperTail
                );
                subHeader2.SaveSummarySection(section1);
            }

            // Individuals contributions
            if (hbmIndividualDayCollections.Count == 1) {
                summarizeIndividualContributions(
                    hbmIndividualDayCollections,
                    hbmIndividualCollections,
                    activeSubstances,
                    relativePotencyFactors,
                    project,
                    header.Units,
                    subHeader,
                    order
                );
            }
            subHeader.SaveSummarySection(section);
        }

        private void summarizeIndividualContributions(
            ICollection<HbmIndividualDayCollection> hbmIndividualDayCollections,
            ICollection<HbmIndividualCollection> hbmIndividualCollections,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            ProjectDto project,
            List<ActionSummaryUnitRecord> units,
            SectionHeader header,
            int order
        ) {
            if (relativePotencyFactors.Any()) {
                if (project.AssessmentSettings.ExposureType == ExposureType.Chronic 
                    && hbmIndividualCollections != null
                ) {
                    var section = new HbmIndividualContributionsSection() {
                        SectionLabel = getSectionLabel(HumanMonitoringAnalysisSections.IndividualContributionsSections),
                        Units = units
                    };
                    var subHeader = header.AddSubSectionHeaderFor(
                        section,
                        "Contributions to cumulative concentrations for individuals",
                        order
                    );
                    section.SummarizeBoxPlots(
                        hbmIndividualCollections,
                        activeSubstances,
                        relativePotencyFactors,
                        !project.OutputDetailSettings.SkipPrivacySensitiveOutputs
                    );
                    subHeader.SaveSummarySection(section);
                } else if (project.AssessmentSettings.ExposureType == ExposureType.Acute 
                    && hbmIndividualDayCollections.Any()
                ) {
                    var section = new HbmIndividualDayContributionsSection() {
                        SectionLabel = getSectionLabel(HumanMonitoringAnalysisSections.IndividualContributionsSections),
                        Units = units
                    };
                    var subHeader = header.AddSubSectionHeaderFor(
                        section,
                        "Contributions to cumulative concentrations for individuals",
                        order
                    );
                    section.SummarizeBoxPlots(
                        hbmIndividualDayCollections,
                        activeSubstances,
                        relativePotencyFactors,
                        !project.OutputDetailSettings.SkipPrivacySensitiveOutputs
                    );
                    subHeader.SaveSummarySection(section);
                }
            }
        }

        private void summarizeHbmConcentrationsByTargetSubstanceUncertainty(
            ICollection<HbmIndividualDayCollection> hbmIndividualDayCollections,
            ICollection<HbmIndividualCollection> hbmIndividualCollections,
            ICollection<Compound> activeSubstances,
            ExposureType exposureType,
            double lowerBound,
            double upperBound,
            SectionHeader header
        ) {
            if (exposureType == ExposureType.Acute
                && hbmIndividualDayCollections.Any()
            ) {
                var subHeader = header.GetSubSectionHeader<HbmIndividualDayDistributionBySubstanceSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as HbmIndividualDayDistributionBySubstanceSection;
                    section.SummarizeUncertainty(
                        hbmIndividualDayCollections,
                        activeSubstances,
                        lowerBound,
                        upperBound
                    );
                    subHeader.SaveSummarySection(section);
                }
            } else if (exposureType == ExposureType.Chronic
                    && hbmIndividualCollections.Any()
                     ) {
                var subHeader = header.GetSubSectionHeader<HbmIndividualDistributionBySubstanceSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as HbmIndividualDistributionBySubstanceSection;
                    section.SummarizeUncertainty(
                        hbmIndividualCollections,
                        activeSubstances,
                        lowerBound,
                        upperBound
                    );
                    subHeader.SaveSummarySection(section);
                }
            }
        }


        private void summarizeHbmIndividualContributionsUncertainty(
            ICollection<HbmIndividualDayCollection> hbmIndividualDayCollections,
            ICollection<HbmIndividualCollection> hbmIndividualCollections,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            ExposureType exposureType,
            double lowerBound,
            double upperBound,
            SectionHeader header
        ) {
            if (exposureType == ExposureType.Chronic && hbmIndividualCollections.Any()) {
                var subHeader = header.GetSubSectionHeader<HbmIndividualContributionsSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as HbmIndividualContributionsSection;
                    section.SummarizeUncertain(
                        hbmIndividualCollections,
                        substances,
                        relativePotencyFactors,
                        lowerBound,
                        upperBound
                    );
                }

            } else if (exposureType == ExposureType.Acute && hbmIndividualDayCollections.Any()) {
                var subHeader = header.GetSubSectionHeader<HbmIndividualDayContributionsSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as HbmIndividualDayContributionsSection;
                    section.SummarizeUncertain(
                        hbmIndividualDayCollections,
                        substances,
                        relativePotencyFactors,
                        lowerBound,
                        upperBound
                    );
                }
            }
        }


        private void summarizeHbmConcentrationsByTargetSubstanceDetailsUncertainty(
            ICollection<HbmIndividualDayCollection> hbmIndividualDayCollections,
            ICollection<HbmIndividualCollection> hbmIndividualCollections,
            ICollection<Compound> activeSubstances,
            ExposureType exposureType,
            double lowerBound,
            double upperBound,
            SectionHeader header
        ) {
            if (exposureType == ExposureType.Acute && hbmIndividualDayCollections.Any()) {
                var subHeader = header.GetSubSectionHeader<HbmIndividualDayDistributionBySubstanceDetailsSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as HbmIndividualDayDistributionBySubstanceDetailsSection;
                    section.SummarizeUncertainty(
                        hbmIndividualDayCollections,
                        activeSubstances,
                        lowerBound,
                        upperBound
                    );
                    subHeader.SaveSummarySection(section);
                }
            } else if (exposureType == ExposureType.Chronic && hbmIndividualCollections.Any()) {
                var subHeader = header.GetSubSectionHeader<HbmIndividualDistributionBySubstanceDetailsSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as HbmIndividualDistributionBySubstanceDetailsSection;
                    section.SummarizeUncertainty(
                        hbmIndividualCollections,
                        activeSubstances,
                        lowerBound,
                        upperBound
                    );
                    subHeader.SaveSummarySection(section);
                }
            }
        }

        private void summarizeCumulativeConcentrationseUncertainty(
            HbmCumulativeIndividualDayCollection hbmCumulativeIndividualDayCollection,
            HbmCumulativeIndividualCollection hbmCumulativeIndividualCollection,
            ICollection<Compound> activeSubstances,
            ExposureType exposureType,
            double lowerBound,
            double upperBound,
            SectionHeader header
        ) {
            if (exposureType == ExposureType.Acute && hbmCumulativeIndividualDayCollection.HbmCumulativeIndividualDayConcentrations.Any()) {
                var subHeader = header.GetSubSectionHeader<HbmCumulativeIndividualDayDistributionsSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as HbmCumulativeIndividualDayDistributionsSection;
                    section.SummarizeUncertainty(
                        hbmCumulativeIndividualDayCollection,
                        lowerBound,
                        upperBound
                    );
                    subHeader.SaveSummarySection(section);
                }
            } else if (exposureType == ExposureType.Chronic && hbmCumulativeIndividualCollection.HbmCumulativeIndividualConcentrations.Any()) {
                var subHeader = header.GetSubSectionHeader<HbmCumulativeIndividualDistributionsSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as HbmCumulativeIndividualDistributionsSection;
                    section.SummarizeUncertainty(
                        hbmCumulativeIndividualCollection,
                        lowerBound,
                        upperBound
                    );
                    subHeader.SaveSummarySection(section);
                }
            }
        }

        private void summarizeRiskDriversUncertainty(
            ICollection<HbmIndividualDayCollection> hbmIndividualDayCollections,
            ICollection<HbmIndividualCollection> hbmIndividualCollections,
            HbmCumulativeIndividualDayCollection hbmCumulativeIndividualDayCollection,
            HbmCumulativeIndividualCollection hbmCumulativeIndividualCollection,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            ExposureType exposureType,
            double upperPercentage,
            double lowerBound,
            double upperBound,
            SectionHeader header
        ) {
            var subHeader = header.GetSubSectionHeader<HbmTotalDistributionRiskDriversSection>();
            if (subHeader != null) {
                var section = subHeader.GetSummarySection() as HbmTotalDistributionRiskDriversSection;
                section.SummarizeUncertainty(
                    hbmIndividualDayCollections,
                    hbmIndividualCollections,
                    hbmCumulativeIndividualDayCollection,
                    hbmCumulativeIndividualCollection,
                    activeSubstances,
                    relativePotencyFactors,
                    exposureType,
                    lowerBound,
                    upperBound
                );
                subHeader.SaveSummarySection(section);
            }

            subHeader = header.GetSubSectionHeader<HbmUpperDistributionRiskDriversSection>();
            if (subHeader != null) {
                var section = subHeader.GetSummarySection() as HbmUpperDistributionRiskDriversSection;
                section.SummarizeUncertainty(
                    hbmIndividualDayCollections,
                    hbmIndividualCollections,
                    hbmCumulativeIndividualDayCollection,
                    hbmCumulativeIndividualCollection,
                    activeSubstances,
                    relativePotencyFactors,
                    exposureType,
                    upperPercentage,
                    lowerBound,
                    upperBound
                );
                subHeader.SaveSummarySection(section);
            }
        }


        private void SummarizeCumulativeConcentrations(
            HbmCumulativeIndividualDayCollection hbmCumulativeIndividualDayCollection,
            HbmCumulativeIndividualCollection hbmCumulativeIndividualCollection,
            ExposureType exposureType,
            double lowerPercentage,
            double upperPercentage,
            bool skipPrivacySensitiveOutputs,
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
                    hbmCumulativeIndividualDayCollection,
                    lowerPercentage,
                    upperPercentage,
                    skipPrivacySensitiveOutputs
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
                    hbmCumulativeIndividualCollection,
                    lowerPercentage,
                    upperPercentage,
                    skipPrivacySensitiveOutputs
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

        private void summarizeConcentrationModels(
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
            bool skipPrivacySensitiveOutputs,
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
                    maximumCumulativeRatioMinimumPercentage,
                    skipPrivacySensitiveOutputs
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
