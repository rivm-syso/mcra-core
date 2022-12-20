using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;
using MCRA.Simulation.OutputGeneration;
using System.Collections.Generic;

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
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            var subOrder = 0;
            subHeader.Units = collectUnits(project, data);
            if (outputSettings.ShouldSummarize(HumanMonitoringDataSections.SurveysSection)) {
                summarizeSurveys(
                    data.HbmSurveys,
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
                    data.HbmSamples,
                    subHeader,
                    subOrder++
                );
            }
            if (outputSettings.ShouldSummarize(HumanMonitoringDataSections.SamplesPerSamplingMethodSubstanceSection)) {
                summarizeHumanMonitoringSamplesPerSamplingMethodSubstance(
                    data.HbmSampleSubstanceCollections,
                    data.AllCompounds,
                    project.OutputDetailSettings.LowerPercentage,
                    project.OutputDetailSettings.UpperPercentage,
                    subHeader,
                    subOrder++
                );
            }
            subHeader.SaveSummarySection(section);
        }

        private static List<ActionSummaryUnitRecord> collectUnits(ProjectDto project, ActionData data) {
            var result = new List<ActionSummaryUnitRecord>();
            result.Add(new ActionSummaryUnitRecord("LowerPercentage", $"p{project.OutputDetailSettings.LowerPercentage}"));
            result.Add(new ActionSummaryUnitRecord("UpperPercentage", $"p{project.OutputDetailSettings.UpperPercentage}"));
            result.Add(new ActionSummaryUnitRecord("HbmConcentrationUnit", data.HbmConcentrationUnit.GetShortDisplayName()));
            return result;
        }

        private void summarizeSurveys(
            ICollection<HumanMonitoringSurvey> humanMonitoringSurveys,
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
                humanMonitoringSurveys,
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
            SectionHeader header,
            int order
        ) {
            var section = new HbmSamplesSummarySection() {
                SectionLabel = getSectionLabel(HumanMonitoringDataSections.SamplingMethodsSection)
            };
            section.Summarize(humanMonitoringsamplingSamples);
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Samples overview",
                order
            );
            subHeader.SaveSummarySection(section);
        }

        private void summarizeHumanMonitoringSamplesPerSamplingMethodSubstance(
            ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollection,
            ICollection<Compound> substances,
            double lowerPercentage,
            double upperPercentage,
            SectionHeader header,
            int order
        ) {
            var section = new HbmSamplesBySamplingMethodSubstanceSection() {
                SectionLabel = getSectionLabel(HumanMonitoringDataSections.SamplesPerSamplingMethodSubstanceSection)
            };
            section.Summarize(hbmSampleSubstanceCollection, substances, lowerPercentage, upperPercentage);
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Samples per substance",
                order
            );
            subHeader.SaveSummarySection(section);
        }
    }
}
