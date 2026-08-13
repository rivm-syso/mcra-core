using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.ComponentCalculation.DriverSubstanceCalculation;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.HumanMonitoringAnalysis {
    public enum HumanMonitoringAnalysisSections {
        CumulativeConcentrationsSections,
        HbmConcentrationsByTargetSubstanceSection,
        HbmConcentrationsByTargetSubstanceTimePointSection,
        HbmConcentrationsByTargetSubstanceDetailsSection,
        IndividualMonitoringConcentrationsSection,
        ConcentrationModelSection,
        McrCoExposureSection,
        CumulativeRiskDriversSections,
        IndividualContributionsSections,
    }

    public sealed class HumanMonitoringAnalysisSummarizer : ActionModuleResultsSummarizer<HumanMonitoringAnalysisModuleConfig, HumanMonitoringAnalysisActionResult> {

        public HumanMonitoringAnalysisSummarizer(HumanMonitoringAnalysisModuleConfig config) : base(config) {
        }

        public override void Summarize(
            ActionModuleConfig sectionConfig,
            HumanMonitoringAnalysisActionResult actionResult,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var outputSettings = new ModuleOutputSectionsManager<HumanMonitoringAnalysisSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }

            var stratifier = getPopulationStratifier(data);

            var subHeader = header.AddEmptySubSectionHeader(ActionType.GetDisplayName(), order, ActionType.ToString());
            subHeader.Units = CollectUnits(data);
            var subOrder = 0;

            // HBM cumulative concentrations
            if ((data.HbmCumulativeIndividualCollection != null || data.HbmCumulativeIndividualDayCollection != null)
                && outputSettings.ShouldSummarize(HumanMonitoringAnalysisSections.CumulativeConcentrationsSections)
            ) {
                SummarizeCumulativeConcentrations(
                    stratifier,
                    data.HbmCumulativeIndividualDayCollection,
                    data.HbmCumulativeIndividualCollection,
                    _configuration.ExposureType,
                    _configuration.VariabilityLowerPercentage,
                    _configuration.VariabilityUpperPercentage,
                    _configuration.SkipPrivacySensitiveOutputs,
                    subHeader,
                    subOrder++
                 );
            }

            // HBM calculated (derived) concentrations by substance
            if (outputSettings.ShouldSummarize(HumanMonitoringAnalysisSections.HbmConcentrationsByTargetSubstanceSection)
                && data.HbmIndividualDayCollections.Any()
            ) {
                summarizeHbmConcentrationsByTargetSubstance(
                    stratifier,
                    data.HbmIndividualDayCollections,
                    data.HbmIndividualCollections,
                    data.ActiveSubstances,
                    _configuration.ExposureType,
                    _configuration.VariabilityLowerPercentage,
                    _configuration.VariabilityUpperPercentage,
                    _configuration.SkipPrivacySensitiveOutputs,
                    subHeader,
                    subOrder++
                 );
            }

            // HBM calculated (derived) concentrations by substance for different timepoints
            if (outputSettings.ShouldSummarize(HumanMonitoringAnalysisSections.HbmConcentrationsByTargetSubstanceTimePointSection)
                && _configuration.OccupationalExposureAssessment
                && _configuration.ShiftAnalysis
            ) {
                summarizeHbmConcentrationsBySubstanceTimePoint(
                    stratifier,
                    data.HbmIndividualDayCollections,
                    data.HbmIndividualCollections,
                    data.ActiveSubstances,
                    _configuration.VariabilityLowerPercentage,
                    _configuration.VariabilityUpperPercentage,
                    _configuration.SkipPrivacySensitiveOutputs,
                    subHeader,
                    subOrder++
                 );
            }

            // HBM measured (original) concentrations by substance
            if (outputSettings.ShouldSummarize(HumanMonitoringAnalysisSections.HbmConcentrationsByTargetSubstanceDetailsSection)
                && (actionResult.HbmMeasuredMatrixIndividualDayCollections?.Count > 0)
            ) {
                summarizeHbmConcentrationsByTargetSubstanceDetails(
                    actionResult,
                    data,
                    _configuration,
                    stratifier,
                    subHeader,
                    subOrder++
                 );
            }

            // HBM contributions by substance
            if ((data.HbmCumulativeIndividualCollection != null || data.HbmCumulativeIndividualDayCollection != null)
                && outputSettings.ShouldSummarize(HumanMonitoringAnalysisSections.CumulativeRiskDriversSections)
                && data.ActiveSubstances.Count > 1
            ) {
                summarizeContributionsBySubstance(
                    stratifier,
                    data.HbmIndividualDayCollections,
                    data.HbmIndividualCollections,
                    data.HbmCumulativeIndividualDayCollection,
                    data.HbmCumulativeIndividualCollection,
                    data.ActiveSubstances,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
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
            if (_configuration.McrAnalysis
                && data.ActiveSubstances.Count > 1
                && actionResult.ExposureMatrix != null
                && outputSettings.ShouldSummarize(HumanMonitoringAnalysisSections.McrCoExposureSection)
            ) {
                SummarizeMaximumCumulativeRatio(
                    actionResult.DriverSubstances,
                    actionResult.ExposureMatrix,
                    data.HbmIndividualDayCollections.FirstOrDefault().TargetUnit,
                    _configuration.McrExposureApproachType,
                    _configuration.McrPlotRatioCutOff,
                    _configuration.McrPlotPercentiles.ToArray(),
                    _configuration.McrCalculationTotalExposureCutOff,
                    _configuration.McrPlotMinimumPercentage,
                    _configuration.SkipPrivacySensitiveOutputs,
                    subHeader,
                    subOrder++
                );
            }

            // HBM individual monitoring concentrations
            if (_configuration.StoreIndividualDayIntakes
                && !_configuration.SkipPrivacySensitiveOutputs
                && outputSettings.ShouldSummarize(HumanMonitoringAnalysisSections.IndividualMonitoringConcentrationsSection)
                && data.HbmIndividualDayCollections.Any()
            ) {
                SummarizeIndividualMonitoringConcentrations(
                    data.ActiveSubstances,
                    data.HbmSamplingMethods.First(),
                    data.HbmIndividualDayCollections,
                    data.HbmIndividualCollections,
                    _configuration.ExposureType,
                    subHeader,
                    subOrder++
                );
            }
        }

        public void SummarizeUncertain(
           HumanMonitoringAnalysisActionResult result,
           ActionData data,
           SectionHeader header
        ) {
            var stratifier = getPopulationStratifier(data);

            if (data.HbmCumulativeIndividualCollection != null || data.HbmCumulativeIndividualDayCollection != null) {
                summarizeCumulativeConcentrationseUncertainty(
                    stratifier,
                    data.HbmCumulativeIndividualDayCollection,
                    data.HbmCumulativeIndividualCollection,
                    _configuration.ExposureType,
                    _configuration.UncertaintyLowerBound,
                    _configuration.UncertaintyUpperBound,
                    header
                );

                summarizeRiskDriversUncertainty(
                    stratifier,
                    data.HbmIndividualDayCollections,
                    data.HbmIndividualCollections,
                    data.HbmCumulativeIndividualDayCollection,
                    data.HbmCumulativeIndividualCollection,
                    data.ActiveSubstances,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    _configuration.ExposureType,
                    _configuration.VariabilityUpperTailPercentage,
                    _configuration.UncertaintyLowerBound,
                    _configuration.UncertaintyUpperBound,
                    _configuration.UncertaintyAnalysisCycles,
                    header
                );

            }

            summarizeHbmConcentrationsByTargetSubstanceUncertainty(
                stratifier,
                data.HbmIndividualDayCollections,
                data.HbmIndividualCollections,
                data.ActiveSubstances,
                _configuration.ExposureType,
                _configuration.OccupationalExposureAssessment,
                _configuration.ShiftAnalysis,
                _configuration.UncertaintyLowerBound,
                _configuration.UncertaintyUpperBound,
                header
            );

            summarizeHbmConcentrationsBySubstanceTimePointUncertainty(
                stratifier,
                data.HbmIndividualDayCollections,
                data.ActiveSubstances,
                _configuration.UncertaintyLowerBound,
                _configuration.UncertaintyUpperBound,
                header
            );

            if (result.HbmMeasuredMatrixIndividualDayCollections?.Count > 0) {
                summarizeHbmConcentrationsByTargetSubstanceDetailsUncertainty(
                    result,
                    data,
                    _configuration,
                    stratifier,
                    header
                );
            }

            // IndividualContributions
            summarizeHbmIndividualContributionsUncertainty(
                    data.HbmIndividualDayCollections,
                    data.HbmIndividualCollections,
                    data.ActiveSubstances,
                    data.CorrectedRelativePotencyFactors,
                    _configuration.ExposureType,
                    _configuration.UncertaintyLowerBound,
                    _configuration.UncertaintyUpperBound,
                    header
            );
        }

        private List<ActionSummaryUnitRecord> CollectUnits(ActionData data) {
            var actionSummaryUnitRecords = new List<ActionSummaryUnitRecord> {
                new("LowerPercentage", $"p{_configuration.VariabilityLowerPercentage}"),
                new("UpperPercentage", $"p{_configuration.VariabilityUpperPercentage}"),
                new("LowerBound", $"p{_configuration.UncertaintyLowerBound}"),
                new("UpperBound", $"p{_configuration.UncertaintyUpperBound}"),
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

            if (_configuration.ExposureType == ExposureType.Chronic) {
                actionSummaryUnitRecords.Add(new ActionSummaryUnitRecord("IndividualDayUnit", "individuals"));
            } else {
                actionSummaryUnitRecords.Add(new ActionSummaryUnitRecord("IndividualDayUnit", "individual days"));
            }
            return actionSummaryUnitRecords;
        }

        private PopulationStratifier getPopulationStratifier(ActionData data) {
            var outputStratifiers = new List<ISimplePopulationStratifier>();
            if (_configuration.OccupationalExposureAssessment) {
                // The CaseControl stratifier returns only 'Case' or 'Control',
                if (_configuration.CaseControlAnalysis) {
                    var caseControlScenarioStratifier = new OccupationalCaseControlStratifier(
                        data.OccupationalScenarios?.Values,
                        _configuration.CaseControlSelection
                    );
                    outputStratifiers.Add(caseControlScenarioStratifier);
                }
                var scenarioStratifier = new OccupationalScenarioStratifier(data.OccupationalScenarios?.Values);
                outputStratifiers.Add(scenarioStratifier);
            }
            if (_configuration.StratifyOutputs) {
                if (_configuration.OutputStratificationVariable == OutputStratificationVariable.Sex) {
                    var genderStratifier = new GenderStratifier();
                    outputStratifiers.Add(genderStratifier);
                }
                if (_configuration.OutputStratificationVariable == OutputStratificationVariable.Age) {
                    var ageStratifier = new AgeStratifier([.. _configuration.AgeStratificationBoundaries]);
                    outputStratifiers.Add(ageStratifier);
                }
            }
            return new PopulationStratifier(outputStratifiers);
        }

        private void summarizeHbmConcentrationsByTargetSubstance(
            PopulationStratifier stratifier,
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
                    stratifier,
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
                    stratifier,
                    lowerPercentage,
                    upperPercentage,
                    skipPrivacySensitiveOutputs
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private void summarizeHbmConcentrationsBySubstanceTimePoint(
           PopulationStratifier stratifier,
           ICollection<HbmIndividualDayCollection> hbmIndividualDayCollections,
           ICollection<HbmIndividualCollection> hbmIndividualCollections,
           ICollection<Compound> activeSubstances,
           double lowerPercentage,
           double upperPercentage,
           bool skipPrivacySensitiveOutputs,
           SectionHeader header,
           int order
       ) {
            var section = new HbmIndividualDayDistributionBySubstanceTimePointSection() {
                SectionLabel = getSectionLabel(HumanMonitoringAnalysisSections.HbmConcentrationsByTargetSubstanceTimePointSection),
                Units = header.Units
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Concentrations by substance and time point",
                order
            );
            section.Summarize(
                hbmIndividualDayCollections,
                activeSubstances,
                stratifier,
                lowerPercentage,
                upperPercentage,
                skipPrivacySensitiveOutputs
             );
            subHeader.SaveSummarySection(section);
        }

        private void summarizeHbmConcentrationsByTargetSubstanceDetails(
            HumanMonitoringAnalysisActionResult actionResult,
            ActionData data,
            HumanMonitoringAnalysisModuleConfig config,
            PopulationStratifier stratifier,
            SectionHeader header,
            int order
       ) {
            if (_configuration.ExposureType == ExposureType.Acute) {
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
                    actionResult.HbmMeasuredMatrixIndividualDayCollections,
                    data.AllCompounds,
                    stratifier,
                    config.VariabilityLowerPercentage,
                    config.VariabilityUpperPercentage,
                    config.SkipPrivacySensitiveOutputs,
                    true
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
                    actionResult.HbmMeasuredMatrixIndividualCollections,
                    data.AllCompounds,
                    stratifier,
                    config.VariabilityLowerPercentage,
                    config.VariabilityUpperPercentage,
                    config.SkipPrivacySensitiveOutputs,
                    true
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private void summarizeContributionsBySubstance(
            PopulationStratifier stratifier,
            ICollection<HbmIndividualDayCollection> hbmIndividualDayCollections,
            ICollection<HbmIndividualCollection> hbmIndividualCollections,
            HbmCumulativeIndividualDayCollection hbmCumulativeIndividualDayConcentrations,
            HbmCumulativeIndividualCollection hbmCumulativeIndividualConcentrations,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> assessmentGroupMemberships,
            SectionHeader header,
            int order
        ) {
            var subHeader = header.AddEmptySubSectionHeader(
                "Contributions by substance",
                order,
                getSectionLabel(HumanMonitoringAnalysisSections.CumulativeRiskDriversSections)
            );

            {
                // Contributions total
                if (hbmIndividualCollections != null || hbmIndividualDayCollections != null) {
                    var subSection = new HbmContributionsBySubstanceTotalSection();
                    var subSubHeader = subHeader.AddSubSectionHeaderFor(subSection, "Contributions to cumulative concentrations for total distribution", order++);
                    subSection.Summarize(
                        stratifier,
                        hbmIndividualDayCollections,
                        hbmIndividualCollections,
                        hbmCumulativeIndividualDayConcentrations,
                        hbmCumulativeIndividualConcentrations,
                        activeSubstances,
                        relativePotencyFactors,
                        assessmentGroupMemberships,
                        _configuration.ExposureType
                    );
                    subSubHeader.SaveSummarySection(subSection);
                }
            }

            {
                // Contributions upper
                if (hbmIndividualCollections != null || hbmIndividualDayCollections != null) {
                    var subSection = new HbmContributionsBySubstanceUpperSection();
                    var subSubHeader = subHeader.AddSubSectionHeaderFor(subSection, "Contributions to cumulative concentrations for upper distribution", order++);
                    subSection.Summarize(
                        stratifier,
                        hbmIndividualDayCollections,
                        hbmIndividualCollections,
                        hbmCumulativeIndividualDayConcentrations,
                        hbmCumulativeIndividualConcentrations,
                        activeSubstances,
                        relativePotencyFactors,
                        assessmentGroupMemberships,
                        _configuration.ExposureType,
                        _configuration.VariabilityUpperTailPercentage
                    );
                    subSubHeader.SaveSummarySection(subSection);
                }
            }

            // Individuals contributions
            if (hbmIndividualDayCollections.Count == 1) {
                summarizeIndividualContributions(
                    hbmIndividualDayCollections,
                    hbmIndividualCollections,
                    activeSubstances,
                    relativePotencyFactors,
                    header.Units,
                    subHeader,
                    order
                );
            }
        }

        private void summarizeIndividualContributions(
            ICollection<HbmIndividualDayCollection> hbmIndividualDayCollections,
            ICollection<HbmIndividualCollection> hbmIndividualCollections,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            List<ActionSummaryUnitRecord> units,
            SectionHeader header,
            int order
        ) {
            if (relativePotencyFactors.Any()) {
                if (_configuration.ExposureType == ExposureType.Chronic
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
                        !_configuration.SkipPrivacySensitiveOutputs
                    );
                    subHeader.SaveSummarySection(section);
                } else if (_configuration.ExposureType == ExposureType.Acute
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
                        !_configuration.SkipPrivacySensitiveOutputs
                    );
                    subHeader.SaveSummarySection(section);
                }
            }
        }

        private void summarizeHbmConcentrationsByTargetSubstanceUncertainty(
            PopulationStratifier stratifier,
            ICollection<HbmIndividualDayCollection> hbmIndividualDayCollections,
            ICollection<HbmIndividualCollection> hbmIndividualCollections,
            ICollection<Compound> activeSubstances,
            ExposureType exposureType,
            bool occupationalExposureAnalysis,
            bool occupationalShiftAnalysis,
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
                        stratifier,
                        lowerBound,
                        upperBound
                    );
                    subHeader.SaveSummarySection(section);
                }
            } else if (exposureType == ExposureType.Chronic && hbmIndividualCollections.Any()) {
                var subHeader = header.GetSubSectionHeader<HbmIndividualDistributionBySubstanceSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as HbmIndividualDistributionBySubstanceSection;
                    section.SummarizeUncertainty(
                        hbmIndividualCollections,
                        activeSubstances,
                        stratifier,
                        lowerBound,
                        upperBound
                    );
                    subHeader.SaveSummarySection(section);
                }
            }
        }
        private void summarizeHbmConcentrationsBySubstanceTimePointUncertainty(
            PopulationStratifier stratifier,
            ICollection<HbmIndividualDayCollection> hbmIndividualDayCollections,
            ICollection<Compound> activeSubstances,
            double lowerBound,
            double upperBound,
            SectionHeader header
        ) {
            var subHeader = header.GetSubSectionHeader<HbmIndividualDayDistributionBySubstanceTimePointSection>();
            if (subHeader != null) {
                var section = subHeader.GetSummarySection() as HbmIndividualDayDistributionBySubstanceTimePointSection;
                section.SummarizeUncertainty(
                    hbmIndividualDayCollections,
                    activeSubstances,
                    stratifier,
                    lowerBound,
                    upperBound
                );
                subHeader.SaveSummarySection(section);
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
            HumanMonitoringAnalysisActionResult actionResult,
            ActionData data,
            HumanMonitoringAnalysisModuleConfig config,
            PopulationStratifier stratifier,
            SectionHeader header
        ) {
            if (config.ExposureType == ExposureType.Acute && actionResult.HbmMeasuredMatrixIndividualDayCollections.Count != 0) {
                var subHeader = header.GetSubSectionHeader<HbmIndividualDayDistributionBySubstanceDetailsSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as HbmIndividualDayDistributionBySubstanceDetailsSection;
                    section.SummarizeUncertainty(
                        actionResult.HbmMeasuredMatrixIndividualDayCollections,
                        data.AllCompounds,
                        stratifier,
                        config.VariabilityLowerPercentage,
                        config.VariabilityUpperPercentage
                    );
                    subHeader.SaveSummarySection(section);
                }
            } else if (config.ExposureType == ExposureType.Chronic && actionResult.HbmMeasuredMatrixIndividualCollections.Count != 0) {
                var subHeader = header.GetSubSectionHeader<HbmIndividualDistributionBySubstanceDetailsSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as HbmIndividualDistributionBySubstanceDetailsSection;
                    section.SummarizeUncertainty(
                        actionResult.HbmMeasuredMatrixIndividualCollections,
                        data.AllCompounds,
                        stratifier,
                        config.VariabilityLowerPercentage,
                        config.VariabilityUpperPercentage
                    );
                    subHeader.SaveSummarySection(section);
                }
            }
        }

        private void summarizeCumulativeConcentrationseUncertainty(
            PopulationStratifier stratifier,
            HbmCumulativeIndividualDayCollection hbmCumulativeIndividualDayCollection,
            HbmCumulativeIndividualCollection hbmCumulativeIndividualCollection,
            ExposureType exposureType,
            double lowerBound,
            double upperBound,
            SectionHeader header
        ) {
            if (exposureType == ExposureType.Acute && hbmCumulativeIndividualDayCollection.IndividualDayConcentrations.Any()) {
                var subHeader = header.GetSubSectionHeader<HbmCumulativeIndividualDayDistributionsSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as HbmCumulativeIndividualDayDistributionsSection;
                    section.SummarizeUncertainty(
                        hbmCumulativeIndividualDayCollection,
                        stratifier,
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
                        stratifier,
                        lowerBound,
                        upperBound
                    );
                    subHeader.SaveSummarySection(section);
                }
            }
        }

        private void summarizeRiskDriversUncertainty(
            PopulationStratifier stratifier,
            ICollection<HbmIndividualDayCollection> hbmIndividualDayCollections,
            ICollection<HbmIndividualCollection> hbmIndividualCollections,
            HbmCumulativeIndividualDayCollection hbmCumulativeIndividualDayCollection,
            HbmCumulativeIndividualCollection hbmCumulativeIndividualCollection,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> assessmentGroupMemberships,
            ExposureType exposureType,
            double upperPercentage,
            double lowerBound,
            double upperBound,
            int uncertaintyAnalysisCycles,
            SectionHeader header
        ) {
            var subHeader = header.GetSubSectionHeader<HbmContributionsBySubstanceTotalSection>();
            if (subHeader != null) {
                var section = subHeader.GetSummarySection() as HbmContributionsBySubstanceTotalSection;
                section.SummarizeUncertainty(
                    stratifier,
                    hbmIndividualDayCollections,
                    hbmIndividualCollections,
                    hbmCumulativeIndividualDayCollection,
                    hbmCumulativeIndividualCollection,
                    activeSubstances,
                    relativePotencyFactors,
                    assessmentGroupMemberships,
                    exposureType,
                    lowerBound,
                    upperBound,
                    uncertaintyAnalysisCycles
                );
                subHeader.SaveSummarySection(section);
            }

            subHeader = header.GetSubSectionHeader<HbmContributionsBySubstanceUpperSection>();
            if (subHeader != null) {
                var section = subHeader.GetSummarySection() as HbmContributionsBySubstanceUpperSection;
                section.SummarizeUncertainty(
                    stratifier,
                    hbmIndividualDayCollections,
                    hbmIndividualCollections,
                    hbmCumulativeIndividualDayCollection,
                    hbmCumulativeIndividualCollection,
                    activeSubstances,
                    relativePotencyFactors,
                    assessmentGroupMemberships,
                    exposureType,
                    upperPercentage,
                    lowerBound,
                    upperBound,
                    uncertaintyAnalysisCycles
                );
                subHeader.SaveSummarySection(section);
            }
        }


        private void SummarizeCumulativeConcentrations(
            PopulationStratifier stratifier,
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
                    stratifier,
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
                    stratifier,
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
