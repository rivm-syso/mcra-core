using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.HumanMonitoringData {
    public enum HumanMonitoringDataSections {
        SurveysSection,
        SamplingMethodsSection,
        SamplesPerSamplingMethodSubstanceSection
    }
    public sealed class HumanMonitoringDataSummarizer : ActionResultsSummarizerBase<IHumanMonitoringDataActionResult> {

        public override ActionType ActionType => ActionType.HumanMonitoringData;

        public override void Summarize(ProjectDto project, IHumanMonitoringDataActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<HumanMonitoringDataSections>(project, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new HbmDataSummarySection() {
                SectionLabel = ActionType.ToString()
            };

            var nonAnalysedSamples = calculateNonAnalysedSamples(
                data.HbmAllSamples,
                data.HbmSampleSubstanceCollections,
                project.HumanMonitoringSettings.ExcludeSubstancesFromSamplingMethod,
                project.HumanMonitoringSettings.ExcludedSubstancesFromSamplingMethodSubset
            );

            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            var subOrder = 0;
            subHeader.Units = collectUnits(project, data);
            if (outputSettings.ShouldSummarize(HumanMonitoringDataSections.SurveysSection)) {
                summarizeSurveys(
                    data.HbmSurveys.First(),
                    data.HbmIndividuals,
                    data.SelectedPopulation,
                    project.SubsetSettings.MatchHbmIndividualSubsetWithPopulation,
                    project.SelectedHbmSurveySubsetProperties,
                    subHeader,
                    subOrder++
                );
            }
            if (outputSettings.ShouldSummarize(HumanMonitoringDataSections.SamplingMethodsSection)) {
                summarizeSamples(
                    data.HbmAllSamples,
                    nonAnalysedSamples,
                    subHeader,
                    subOrder++
                );
            }
            if (outputSettings.ShouldSummarize(HumanMonitoringDataSections.SamplesPerSamplingMethodSubstanceSection)) {
                summarizeHumanMonitoringSamplesPerSamplingMethodSubstance(
                    data.HbmAllSamples,
                    data.HbmSampleSubstanceCollections,
                    data.AllCompounds,
                    project.OutputDetailSettings.LowerPercentage,
                    project.OutputDetailSettings.UpperPercentage,
                    project.HumanMonitoringSettings.UseCompleteAnalysedSamples ? new() : nonAnalysedSamples,
                    subHeader,
                    subOrder++
                );
            }
            subHeader.SaveSummarySection(section);
        }

        private static List<ActionSummaryUnitRecord> collectUnits(ProjectDto project, ActionData data) {
            return new() {
                new ("LowerPercentage", $"p{project.OutputDetailSettings.LowerPercentage}"),
                new ("UpperPercentage", $"p{project.OutputDetailSettings.UpperPercentage}"),
            };
        }

        private void summarizeSurveys(
            HumanMonitoringSurvey humanMonitoringSurvey,
            ICollection<Individual> hbmIndividuals,
            Population population,
            IndividualSubsetType individualSubsetType,
            List<string> selectedHbmSubsetProperties,
            SectionHeader header,
            int order
        ) {
            var section = new HbmSurveySummarySection() {
                SectionLabel = getSectionLabel(HumanMonitoringDataSections.SurveysSection)
            };
            section.Summarize(
                humanMonitoringSurvey,
                hbmIndividuals,
                population,
                individualSubsetType,
                selectedHbmSubsetProperties
            );
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Study and individuals",
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
                nonAnalysedSamples);
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
            double lowerPercentage,
            double upperPercentage,
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
                lowerPercentage,
                upperPercentage,
                nonAnalysedSamples
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
