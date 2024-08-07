using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.HumanMonitoringData {
    public enum HumanMonitoringDataSections {
        SurveysSection,
        IndividualStatisticsSection,
        SamplingMethodsSection,
        SamplesPerSamplingMethodSubstanceSection
    }
    public sealed class HumanMonitoringDataSummarizer : ActionModuleResultsSummarizer<HumanMonitoringDataModuleConfig, IHumanMonitoringDataActionResult> {

        public HumanMonitoringDataSummarizer(HumanMonitoringDataModuleConfig config): base(config) {
        }

        public override void Summarize(ActionModuleConfig sectionConfig, IHumanMonitoringDataActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<HumanMonitoringDataSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new HbmDataSummarySection() {
                SectionLabel = ActionType.ToString()
            };

            var nonAnalysedSamples = calculateNonAnalysedSamples(
                data.HbmAllSamples,
                data.HbmSampleSubstanceCollections,
                _configuration.ExcludeSubstancesFromSamplingMethod,
                _configuration.ExcludedSubstancesFromSamplingMethodSubset
            );

            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            var subOrder = 0;
            subHeader.Units = collectUnits();
            if (outputSettings.ShouldSummarize(HumanMonitoringDataSections.SurveysSection)) {
                summarizeSurveys(
                    outputSettings,
                    data.HbmSurveys.First(),
                    data.HbmIndividuals,
                    data.HbmAllSamples,
                    nonAnalysedSamples,
                    data.SelectedPopulation,
                    subHeader,
                    subOrder++
                );
            }
            if (outputSettings.ShouldSummarize(HumanMonitoringDataSections.IndividualStatisticsSection)) {
                summarizeHbmIndividualStatistics(
                    data.HbmIndividuals,
                    data.SelectedPopulation,
                    subHeader,
                    subOrder++
                );
            }
            if (outputSettings.ShouldSummarize(HumanMonitoringDataSections.SamplesPerSamplingMethodSubstanceSection)) {
                summarizeHumanMonitoringSamplesPerSamplingMethodSubstance(
                    data.HbmAllSamples,
                    data.HbmSampleSubstanceCollections,
                    data.AllCompounds,
                    _configuration.UseCompleteAnalysedSamples ? new() : nonAnalysedSamples,
                    subHeader,
                    subOrder++
                );
            }
            subHeader.SaveSummarySection(section);
        }

        private List<ActionSummaryUnitRecord> collectUnits() {
            return new() {
                new ("LowerPercentage", $"p{_configuration.VariabilityLowerPercentage}"),
                new ("UpperPercentage", $"p{_configuration.VariabilityUpperPercentage}"),
            };
        }

        private void summarizeSurveys(
            ModuleOutputSectionsManager<HumanMonitoringDataSections> outputSettings,
            HumanMonitoringSurvey humanMonitoringSurvey,
            ICollection<Individual> hbmIndividuals,
            ICollection<HumanMonitoringSample> hbmAllSamples,
            Dictionary<(HumanMonitoringSamplingMethod method, Compound a), List<string>> nonAnalysedSamples,
            Population population,
            SectionHeader header,
            int order
        ) {
            var section = new HbmSurveySummarySection() {
                SectionLabel = getSectionLabel(HumanMonitoringDataSections.SurveysSection)
            };
            section.Summarize(
                humanMonitoringSurvey,
                population
            );
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "HBM study",
                order
            );
            subHeader.SaveSummarySection(section);

            if (outputSettings.ShouldSummarize(HumanMonitoringDataSections.SamplingMethodsSection)) {
                summarizeSamples(
                    hbmAllSamples,
                    nonAnalysedSamples,
                    subHeader,
                    order
                );
            }
        }

        private void summarizeHbmIndividualStatistics(
            ICollection<Individual> hbmIndividuals,
            Population population,
            SectionHeader header,
            int order
        ) {
            var section = new HbmIndividualStatisticsSummarySection() {
                SectionLabel = getSectionLabel(HumanMonitoringDataSections.IndividualStatisticsSection)
            };
            section.Summarize(
                hbmIndividuals,
                population,
                _configuration.MatchHbmIndividualSubsetWithPopulation,
                _configuration.SelectedHbmSurveySubsetProperties,
                _configuration.SkipPrivacySensitiveOutputs
            );
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Individuals",
                order
            );
            subHeader.SaveSummarySection(section);
        }

        private void summarizeSamples(
            ICollection<HumanMonitoringSample> humanMonitoringsamplingSamples,
            Dictionary<(HumanMonitoringSamplingMethod method, Compound a), List<string>> nonAnalysedSamples,
            SectionHeader header,
            int order
        ) {
            var section = new HbmSamplesSummarySection() {
                SectionLabel = getSectionLabel(HumanMonitoringDataSections.SamplingMethodsSection)
            };
            section.Summarize(
                humanMonitoringsamplingSamples,
                nonAnalysedSamples
            );
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Samples overview",
                order
            );
            subHeader.SaveSummarySection(section);
        }

        private void summarizeHumanMonitoringSamplesPerSamplingMethodSubstance(
            ICollection<HumanMonitoringSample> allHbmSamples,
            ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections,
            ICollection<Compound> substances,
            Dictionary<(HumanMonitoringSamplingMethod method, Compound a), List<string>> nonAnalysedSamples,
            SectionHeader header,
            int order
        ) {
            var section = new HbmSamplesBySamplingMethodSubstanceSection() {
                SectionLabel = getSectionLabel(HumanMonitoringDataSections.SamplesPerSamplingMethodSubstanceSection)
            };
            section.Summarize(
                allHbmSamples,
                hbmSampleSubstanceCollections,
                substances,
                _configuration.VariabilityLowerPercentage,
                _configuration.VariabilityUpperPercentage,
                nonAnalysedSamples,
                _configuration.SkipPrivacySensitiveOutputs
            );
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Samples per substance",
                order
            );
            subHeader.SaveSummarySection(section);
        }

        private Dictionary<(HumanMonitoringSamplingMethod Method, Compound Substance), List<string>> calculateNonAnalysedSamples(
           ICollection<HumanMonitoringSample> allHbmSamples,
           ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections,
           bool excludeSubstancesFromSamplingMethod,
           List<HbmSamplingMethodSubstance> excludedSubstancesFromSamplingMethodSubset
        ) {
            var excludedSubstanceMethods = excludeSubstancesFromSamplingMethod ? excludedSubstancesFromSamplingMethodSubset
            .GroupBy(c => c.SamplingMethodCode)
                   .ToDictionary(c => c.Key, c => c.Select(n => n.SubstanceCode).ToList())
                   : new();

            var notAnalysedSamples = new List<((HumanMonitoringSamplingMethod method, Compound a) methodSubstanceKey, List<string> NonAnalysedSamples)>();
            var samplingMethods = hbmSampleSubstanceCollections.Select(c => c.SamplingMethod).ToList();
            foreach (var method in samplingMethods) {
                excludedSubstanceMethods.TryGetValue(method.Code, out List<string> excludedSubstances);
                var allSubstances = allHbmSamples
                    .Where(c => c.SamplingMethod == method)
                    .SelectMany(c => c.SampleAnalyses.SelectMany(am => am.AnalyticalMethod.AnalyticalMethodCompounds.Keys))
                    .Distinct()
                    .Where(s => !excludedSubstances?.Contains(s.Code) ?? true)
                    .ToList();

                var notAnalysed = allHbmSamples
                    .Where(s => s.SamplingMethod == method)
                    .SelectMany(s => {
                        var substances = s.SampleAnalyses.SelectMany(am => am.AnalyticalMethod.AnalyticalMethodCompounds.Keys).ToList();
                        var missingSubstances = allSubstances.Except(substances);
                        return missingSubstances.Select(c => (Method: method, Substance: c, SampleId: s.Code));
                    })
                    .ToList();

                var countedNoAnalysedSamples = notAnalysed
                    .GroupBy(x => (x.Method, x.Substance))
                    .Select(x => {
                        return (Substance: x.Key, NonAnalysedSamples: x.Select(a => a.SampleId).ToList());
                    })
                    .ToList();
                notAnalysedSamples.AddRange(countedNoAnalysedSamples);
            }
            return notAnalysedSamples.ToDictionary(c => c.methodSubstanceKey, c => c.NonAnalysedSamples);
        }
    }
}
